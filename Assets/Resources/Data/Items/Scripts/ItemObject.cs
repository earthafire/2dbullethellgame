using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum ItemType
{
    Food,
    Head,
    Neck,
    Torso,
    Hands,
    MainHand,
    Ring,
    OffHand,
    Legs,
    Feet,
    Default
}

public class ItemObject : SerializedScriptableObject
{

    public Sprite uiDisplay;
    public bool stackable;
    public ItemType type;
    [TextArea(15, 20)]
    public string description;
    public Item data = new Item();
    public Dictionary<Attribute, float> buffs = new();
    public ActivatableAbilityType ability;

    public Item CreateItem()
    {
        Item newItem = new Item(this);
        return newItem;
    }
}