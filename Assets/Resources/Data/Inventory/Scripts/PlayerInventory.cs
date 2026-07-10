using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventoryObject inventory;
    public InventoryObject equipment;

    // Adds a picked-up item to the backpack, unless it fits an empty,
    // compatible equipment slot - in which case it's equipped immediately.
    public bool PickUpItem(Item item, int amount)
    {
        ItemObject itemObject = inventory.database.ItemObjects[item.Id];
        InventorySlot emptyEquipSlot = FindEmptyCompatibleEquipSlot(itemObject);
        if (emptyEquipSlot != null)
        {
            emptyEquipSlot.UpdateSlot(item, amount);
            GetComponent<PlayerAttributes>()?.updateTotalStats();
            return true;
        }
        return inventory.AddItem(item, amount);
    }

    private InventorySlot FindEmptyCompatibleEquipSlot(ItemObject itemObject)
    {
        foreach (InventorySlot slot in equipment.GetSlots)
        {
            if (slot.item.Id < 0 && slot.AllowedItems.Length > 0 && slot.CanPlaceInSlot(itemObject))
                return slot;
        }
        return null;
    }

    private void OnApplicationQuit()
    {
      inventory.Clear();
      equipment.Clear();
    }
}
