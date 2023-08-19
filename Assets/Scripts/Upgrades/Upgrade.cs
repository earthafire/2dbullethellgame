using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Upgrade", order = 0)]
public abstract class Upgrade : SerializedScriptableObject {
    
    public Sprite icon{get; private set; }

    public string upgrade_name;
    public string description{get; private set; }

    [Button]
    public abstract void DoUpgrade();
 
}
