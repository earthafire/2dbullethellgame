using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade")]
public class UpgradeAttribute : Upgrade
{
    public List<Attributes> units_to_upgrade = new List<Attributes>();
    public Dictionary<Attribute, float> upgrade_to_apply = new Dictionary<Attribute, float>();
    public bool is_percent_upgrade = false;

   public override void DoUpgrade()
   {
        foreach(var unit_to_upgrade in units_to_upgrade)
        {
            foreach(var upgrade in upgrade_to_apply)
            {
                unit_to_upgrade.UnlockUpgrade(this);
            }            
        }
    }
}
