using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "2dbullethellgame/Upgrade", order = 0)]
public abstract class Upgrade : ScriptableObject {
    
    public Sprite icon{get; private set; }

    public bool is_percent_upgrade;

    public string upgrade_name;
    public string description{get; private set; }
    
    public abstract void DoUpgrade();
 
}
