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
    public void HandleLevelUp()
    {
        UpgradeAttributeStack[] upgradesToShow = GetRandomUpgrades();

        if (upgradesToShow == null)
        {
            Debug.Log("No more upgrades :(");
            return;
        }

        OpenUpgradePanel();

        for (int i = 0; i < 3; i++)
        {
            Button b = buttons[i].GetComponent<Button>();
            LevelUpButtons script = b.GetComponent<LevelUpButtons>();

            if (upgradesToShow.Length < i + 1)
            {
                buttons[i].gameObject.SetActive(false);
                continue;
            }
            int indexOfStack = upgradeStacks.IndexOf(upgradesToShow[i]);
            Stack<UpgradeAttribute> nextStack = upgradesToShow[i].upgradeAttributeStack;
            UpgradeAttribute nextAttribute = nextStack.Peek();


            b.onClick.RemoveAllListeners();

            b.onClick.AddListener(() =>
            {
                nextAttribute.DoUpgrade();

                Debug.Log("size before pop: " + nextStack.Count);

                UpgradeAttribute temp = nextStack.Pop();
                Debug.Log("name of stack: " + temp.name);
                Debug.Log("size after pop: " + nextStack.Count);
                if (nextStack.Count == 0)
                {
                    // if stack is empty, remove it from the upgrades entirely
                    Debug.Log("Stack is empty");
                    Debug.Log("does the stack contain this object?:");
                    upgradeStacks.RemoveAt(indexOfStack);
                }
            });

            b.onClick.AddListener(CloseUpgradePanel);

            script.image.sprite = nextAttribute.icon;
            script.name.SetText(nextAttribute.title);
            script.description.SetText(nextAttribute.description);
        }
    }

    // 
    public UpgradeAttributeStack[] GetRandomUpgrades()
    {
        // foreach (UpgradeAttributeStack temp in upgradeStacks)
        // {
        //     Debug.Log(temp.upgradeAttributeStack.Count);
        // }

        if (upgradeStacks.Count <= 3)
        {
            return upgradeStacks.ToArray();
        }
        else if (upgradeStacks.Count == 0)
        {
            return null;
        }

        UpgradeAttributeStack[] randomUpgrades = new UpgradeAttributeStack[3];

        for (int i = 0; i < 3; i++)
        {
            randomUpgrades[i] = upgradeStacks[random.Next(upgradeStacks.Count)];
        }

        return randomUpgrades;
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
