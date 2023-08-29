using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using UnityEditor.Rendering.Universal;

public class LevelUp : MonoBehaviour
 {
    public static int numberOfButtons = 3;
    private System.Random random = new System.Random();
    //[OdinSerialize] public List<UpgradeAttributeStack> permenantUpgradeStacks = new List<UpgradeAttributeStack>();
    private List<UpgradeAttributeStack> upgradeStacks = new List<UpgradeAttributeStack>();
    private Stack<UpgradeAttribute> upgradeAttributes;
    public int nextTier;
    public  Button[] buttons;
    private LevelUpButtons levelUpButton;

    [SerializeField]
    GameObject upgradeButtonsPanel;

     void Start()
    {
     UpgradeAttributeStack[] temp = Resources.LoadAll<UpgradeAttributeStack>("Data/Upgrades/UpgradeStacks");
     //permenantUpgradeStacks.AddRange(Resources.LoadAll<UpgradeAttributeStack>("Data/Upgrades/UpgradeStacks"));
     foreach(var upgradeStack in temp)
        {
            upgradeStacks.Add(Instantiate(upgradeStack));
        }
    }
    public void HandleLevelUp(){

        OpenUpgradePanel();

        // Get a random upgrade and assign it to a button in the list
         foreach (Button button in buttons)
        {
            LevelUpButtons script = button.GetComponent<LevelUpButtons>();
            /*if (upgradeStacks.Count <= 3)
            {

            }*/
            UpgradeAttribute upgradeAttribute = GetRandomUpgrade();

            Button b = button.GetComponent<Button>();
            b.onClick.AddListener(()=> upgradeAttribute.DoUpgrade());
            b.onClick.AddListener(CloseUpgradePanel);

            script.image.sprite = upgradeAttribute.icon;
            script.name.SetText(upgradeAttribute.title);
            script.description.SetText(upgradeAttribute.description);
        }
    }

    // 
    public UpgradeAttribute GetRandomUpgrade(){

        if(upgradeStacks.Count == 0)
        {
            Debug.Log("Stack is empty");
            return null;
        }
        UpgradeAttribute upgradeAttribute = upgradeStacks[random.Next(upgradeStacks.Count)].upgradeAttributeStack.Peek();
        
        return upgradeAttribute;
    }

    public void CloseUpgradePanel(){
        upgradeButtonsPanel.SetActive(false);
        Time.timeScale = 1.0f;

    }


    public void OpenUpgradePanel(){
        upgradeButtonsPanel.SetActive(true);
        Time.timeScale = 0;
    }
    public void PopUpgradeStack()
    {
        
    }
}
