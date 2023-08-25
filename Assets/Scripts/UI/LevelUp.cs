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
    GameObject upgradeButtonsPanel;

     void Start()
    {
        upgrades.AddRange(Resources.LoadAll<UpgradeAttribute>("Data/Upgrades"));
    }
    public void HandleLevelUp(){

        OpenUpgradePanel();

        // Get a random upgrade and assign it to a button in the list
         foreach (Button button in buttons)
        {
            LevelUpButtons script = button.GetComponent<LevelUpButtons>();
            UpgradeAttribute upgrade = GetRandomUpgrade();

            Button b = button.GetComponent<Button>();
            b.onClick.AddListener(upgrade.DoUpgrade);
            b.onClick.AddListener(CloseUpgradePanel);

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

    public void CloseUpgradePanel(){
        upgradeButtonsPanel.SetActive(false);
            Time.timeScale = 1.0f;

    }
    public void OpenUpgradePanel(){
        upgradeButtonsPanel.SetActive(true);
        Time.timeScale = 0;
    }

}
