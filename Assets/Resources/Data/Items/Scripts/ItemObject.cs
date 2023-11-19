using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Timeline.Actions;
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

public class ItemObject : ScriptableObject
{

    public Sprite uiDisplay;
    public bool stackable;
    public ItemType type;
    [TextArea(15, 20)]
    public string description;
    public Item data = new Item();
    public Dictionary<Attribute, int> buffs = new Dictionary<Attribute, int>();

    public Item CreateItem()
    {
        Item newItem = new Item(this);
        return newItem;
    }


}