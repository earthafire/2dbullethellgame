using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Upgrade Attribute Stack")]

[ShowOdinSerializedPropertiesInInspector]
public class UpgradeAttributeStack : SerializedScriptableObject
{
    [OdinSerialize] public Stack<UpgradeAttribute> upgradeAttributeStack;
}
