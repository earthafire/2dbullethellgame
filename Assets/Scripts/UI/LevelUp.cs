using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class LevelUp : MonoBehaviour 
{  
    public static int numberOfButtons = 3;
    private System.Random random = new System.Random();
    private List<UpgradeAttribute> upgrades = new List<UpgradeAttribute>();
    public  Button[] buttons;
    private LevelUpButtons levelUpButton;

    [SerializeField]
    GameObject panel;

     void Start()
    {
        upgrades.AddRange(Resources.LoadAll<UpgradeAttribute>("Data/Upgrades"));
        
        foreach (Button button in buttons)
        {
            LevelUpButtons script = button.GetComponent<LevelUpButtons>();
            UpgradeAttribute upgrade = GetRandomUpgrade();
           

            script.image.sprite = upgrade.icon;
            script.name.SetText(upgrade.name);
            script.description.SetText(upgrade.description);
        }
    }

    public UpgradeAttribute GetRandomUpgrade(){
        UpgradeAttribute temp = upgrades[random.Next(upgrades.Count)];
        upgrades.Remove(temp);
        return temp;
        
    }

}
