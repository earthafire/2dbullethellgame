
using Sirenix.OdinInspector;
using System.Collections.Generic;

public enum GunStats
{
    Damage,
    Fire_Rate,
    Multishot,
    Crit_Chance,
    Crit_Damage,
}

public class Stats : SerializedSriptableObject
{
    public Dictionary<GunStats,float> default_attributes = new()

    [ShowInInspector]
    public List<UpgradeAttribute> applied_upgrades = new();

    public event Action<Attributes, GunUpgrade> OnUpgradeApplied;

    public float GetStat(GunStats attribute)
    {
         if (default_attributes.TryGetValue(attribute, out float value))
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
        //if(!applied_upgrades.Contains(upgrade))
        {
            applied_upgrades.Add(upgrade);
            upgradeApplied?.Invoke(this, upgrade);
        }
    }
    public float GetUpgradedValue(Attribute attribute, float base_value)
    {
        foreach (var upgrade in applied_upgrades)
        {
            if (!upgrade.upgradeToApply.TryGetValue(attribute, out float upgrade_value))
            {
                continue;
            }
            if (upgrade.isPercent)
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

[CreateAssetMenu(fileName = "Gun Upgrade", menuName = "Gun Upgrade")]
public class GunUpgrade : Upgrade
{
    public List<Stats> unitsToUpgrade = new List<Stats>();
    public Dictionary<GunStats, float> upgradeToApply = new Dictionary<GunStats, float>();
    public bool isPercent;

    public override void DoUpgrade()
    {
        foreach (var unit_to_upgrade in unitsToUpgrade)
        {
            foreach (var upgrade in upgradeToApply)
            {
                unit_to_upgrade.UnlockUpgrade(this);
            }
        }
    }

}
