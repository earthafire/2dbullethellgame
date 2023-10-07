using System.Collections;
using System.Collections.Generic;
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

public enum Modifiers
{
    Agility,
    Intellect,
    Stamina,
    Strength
}
public class ItemObject : ScriptableObject
{

    public Sprite uiDisplay;
    public bool stackable;
    public ItemType type;
    [TextArea(15, 20)]
    public string description;
    public Item data = new Item();

    public Item CreateItem()
    {
        Item newItem = new Item(this);
        return newItem;
    }


}

[System.Serializable]
public class ItemBuff : ItemBuffBase
//: IModifier
{
    public ItemBuff(int _min, int _max)
    {
        min = _min;
        max = _max;
        GenerateValue();
    }
}