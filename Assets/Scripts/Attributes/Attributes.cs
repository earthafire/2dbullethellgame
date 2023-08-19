using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attributes", menuName = "2dbullethellgame/Attributes", order = 0)]
public class Attributes : ScriptableObject {

    public Dictionary<Attribute, float> attributes  = new Dictionary<Attribute, float>();
    public Dictionary<Attribute, float> current_attributes  = new Dictionary<Attribute, float>();
    private List<UpgradeAttribute> applied_upgrades = new List<UpgradeAttribute>();
    
    public float GetAttribute(Attribute attribute)
    {

        if(current_attributes.TryGetValue(attribute, out var current_value))
        {
            return GetUpgradedValue(attribute, current_value);
        }

        else if(attributes.TryGetValue(attribute, out float value))
        {
            return GetUpgradedValue(attribute, value);
        }

        else
        {
            Debug.LogError($"No stat value found for {attribute} on {this.name}");
            return 0;
        }
    }

    public void UnlockUpgrade(UpgradeAttribute upgrade)
    {
        if(!applied_upgrades.Contains(upgrade))
        {
            applied_upgrades.Add(upgrade);
        }
    }

    public float GetUpgradedValue(Attribute attribute, float base_value)
    {
        foreach (var upgrade in applied_upgrades)
        {
            if(!upgrade.upgrade_to_apply.TryGetValue(attribute, out float upgrade_value))
            {
                continue;
            }
            
            if(upgrade.is_percent_upgrade)
            {
                base_value *= (upgrade_value / 100f) + 1f;
            }

            else
            {
                base_value += upgrade_value;
            }
        }
        
        return base_value;
    }

    public void ResetAppliedUpgrades()
    {

    }

}
