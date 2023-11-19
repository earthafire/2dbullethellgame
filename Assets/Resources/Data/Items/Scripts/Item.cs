using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public string Name;
    public int Id = -1;
    public Dictionary<Attribute, float> Buffs;

    public Item()
    {
        Name = "";
        Id = -1;
        Buffs = new Dictionary<Attribute, float>() { };
    }

    public Item(ItemObject item)
    {
        Name = item.name;
        Id = item.data.Id;
        Buffs = item.buffs;
    }
}
