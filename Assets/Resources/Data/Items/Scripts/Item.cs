using System.Collections.Generic;
using Unity.VisualScripting;

[System.Serializable]
public class Item
{
    public string Name = "";
    public int Id = -1;
    public string Description;
    public Dictionary<Attribute, float> Buffs = new Dictionary<Attribute, float>() { };
    public ActivatableAbilityType Ability = ActivatableAbilityType.NULL;

    public Item()
    {

    }

    public Item(ItemObject item)
    {
        Name = item.name;
        Id = item.data.Id;
        Description = item.description;
        Buffs = item.buffs;
        Ability = item.ability;
    }
}
