using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public string Name;
    public int Id = -1;
    public Dictionary<Attribute, float> Buffs = new Dictionary<Attribute, float>()
    {
        { Attribute.damage, 1f },
        { Attribute.health, 1f },
        { Attribute.strength, 1f }
    };

    public Item()
    {
        Name = "";
        Id = -1;
    }
    public Item(ItemObject item)
    {
        Name = item.name;
        Id = item.data.Id;
    }
}
