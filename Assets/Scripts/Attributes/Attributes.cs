using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Attributes")]
public class Attributes : SerializedScriptableObject {

    public Dictionary<Attribute, float> default_attributes  = new Dictionary<Attribute, float>();
    public Dictionary<Attribute, float> current_attributes  = new Dictionary<Attribute, float>();
    [ShowInInspector]public List<UpgradeAttribute> applied_upgrades = new List<UpgradeAttribute>();

    public event Action<Attributes, UpgradeAttribute> upgradeApplied;
    
    public float GetAttribute(Attribute attribute)
    {

        if(current_attributes.TryGetValue(attribute, out var current_value))
        {
            return GetUpgradedValue(attribute, current_value);
        }

        else if(default_attributes.TryGetValue(attribute, out float value))
        {
            return GetUpgradedValue(attribute, value);
        }

        else
        {
            Debug.LogError($"No stat value found for {attribute} on {this.name}");
            return 0;
        }
    }

        public int GetAttributeAsInt(Attribute attribute)
    {
        return (int)GetAttributeAsInt(attribute);
    }

    public void UnlockUpgrade(UpgradeAttribute upgrade)
    {
        //if(!applied_upgrades.Contains(upgrade))
        {
            applied_upgrades.Add(upgrade);
            upgradeApplied?.Invoke(this, upgrade);
        }
    }

    // Called by GetAttribute()
    public float GetUpgradedValue(Attribute attribute, float base_value)
    {
        foreach (var upgrade in applied_upgrades)
        {

            if(!upgrade.upgradeToApply.TryGetValue(attribute, out float upgrade_value))
            {
                continue;
            }
            
            if(upgrade.isPercent)
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

    [Button]
    public void ResetAppliedUpgrades()
    {
        applied_upgrades.Clear();
    }

}
