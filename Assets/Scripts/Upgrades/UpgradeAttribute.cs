using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade Attribute", menuName = "Upgrade Attribute")]
public class UpgradeAttribute : Upgrade
{
    public List<Attributes> unitsToUpgrade = new List<Attributes>();
    public Dictionary<Attribute, float> upgradeToApply = new Dictionary<Attribute, float>();
    public bool isPercent;
    public int tier;

    public override void DoUpgrade()
   {
        foreach(var unit_to_upgrade in unitsToUpgrade)
        {
            foreach(var upgrade in upgradeToApply)
            {
                unit_to_upgrade.UnlockUpgrade(this);
                
            }            
        }
   }

}
