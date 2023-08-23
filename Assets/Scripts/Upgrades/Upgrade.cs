using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Upgrade", order = 0)]
public abstract class Upgrade : SerializedScriptableObject {

    [LabelWidth(100)]
    [PreviewField(75)]
    public Sprite icon;

    [LabelWidth(100)]
    public string name;

    [LabelWidth(100)]
    [TextArea]
    public string description;

    [Button]
    public abstract void DoUpgrade();
 
}
