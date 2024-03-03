using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUp : MonoBehaviour
 {
    private System.Random random = new System.Random();
    private List<UpgradeAttributeStack> upgradeStacks = new List<UpgradeAttributeStack>();
    public  Button[] buttons;
    [SerializeField] GameObject upgradeButtonsPanel;

     void Start()
    {
     UpgradeAttributeStack[] temp = Resources.LoadAll<UpgradeAttributeStack>("Data/Upgrades/UpgradeStacks");

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

                //Debug.Log("size before pop: " + nextStack.Count);

                UpgradeAttribute temp = nextStack.Pop();
/*                 Debug.Log("name of stack: " + temp.name);
                Debug.Log("size after pop: " + nextStack.Count); */
                if (nextStack.Count == 0)
                {
/*                     // if stack is empty, remove it from the upgrades entirely
                    Debug.Log("Stack is empty");
                    Debug.Log("does the stack contain this object?:"); */
                    upgradeStacks.RemoveAt(indexOfStack);
                }
            });

            b.onClick.AddListener(CloseUpgradePanel);

            script.image.sprite = nextAttribute.icon;
            script.name.SetText(nextAttribute.title);
            script.description.SetText(nextAttribute.description);
        }
    }

    /// <summary>
    /// Gets a a list of random upgrades, this list is used to populate the buttons.
    /// </summary>
    /// <returns>
    /// null: if there are no upgrades
    /// not random list: if there are 3 or less upgrade stacks left
    /// random list of 3: if there are more than 3 upgrade stacks left
    /// </returns>
    public UpgradeAttributeStack[] GetRandomUpgrades()
    {
        if (upgradeStacks.Count == 0) // no upgrades left
        {
            return null;
        }
        else if (upgradeStacks.Count <= 3) // some upgrades left, not enough to use random
        {
            return upgradeStacks.ToArray();
        }


        // this variable holds all the possible indexes, we will pick randomly from this list.
        List<int> unusedUpgrades = new List<int>();
        for (int i = 0; i < upgradeStacks.Count; i++)
        {
            unusedUpgrades.Add(i);
        }

        // holds our final upgrade results
        UpgradeAttributeStack[] randomUpgrades = new UpgradeAttributeStack[3];

        // pick a random upgrade from the list 'upgradeStacks' 3 times, no repeats
        for (int i = 0; i < 3; i++)
        {
            int randomNum = random.Next(unusedUpgrades.Count);
            randomUpgrades[i] = upgradeStacks[unusedUpgrades[randomNum]];
            unusedUpgrades.RemoveAt(randomNum);
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
}
