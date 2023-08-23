using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class LevelUpButton : MonoBehaviour
{
    public UpgradeAttribute upgrade;
    public Sprite image;
    public string name;
    public string description;

    void Start()
    {
    image = upgrade.icon;
    name = upgrade.name;
    description = upgrade.description;
    }
}
