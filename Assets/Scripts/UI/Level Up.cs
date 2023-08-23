using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;


public class LevelUp : MonoBehaviour 
{
    private System.Random random = new System.Random();
    public Dictionary<UpgradeAttribute, int> upgrades = new Dictionary<UpgradeAttribute,int>();

    
    [SerializeField]
    Button button1, button2, button3;

    [SerializeField]
    GameObject panel;

}
