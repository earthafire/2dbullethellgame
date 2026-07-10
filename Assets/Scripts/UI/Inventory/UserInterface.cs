using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Linq;

[RequireComponent(typeof(EventTrigger))]
public abstract class UserInterface : MonoBehaviour
{
    public InventoryObject inventory;
    private InventoryObject _previousInventory;
    public Dictionary<GameObject, InventorySlot> slotsOnInterface = new();

    private static readonly Color CompatibleSlotOutlineColor = new Color(1f, 0.84f, 0.3f, 1f);
    private static readonly List<UserInterface> AllInterfaces = new();

    private PlayerAttributes stats;
    private PlayerInventory playerInventory;

    public void OnEnable()
    {
        GameObject player = GameObject.FindWithTag("Player");
        stats = player.GetComponent<PlayerAttributes>();
        playerInventory = player.GetComponent<PlayerInventory>();
        CreateSlots();

        for (int i = 0; i < inventory.GetSlots.Length; i++)
        {
            inventory.GetSlots[i].parent = this;
            inventory.GetSlots[i].onAfterUpdated += OnSlotUpdate;
        }
        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });

        AllInterfaces.Add(this);
    }

    public void OnDisable()
    {
        AllInterfaces.Remove(this);
    }

    public abstract void CreateSlots();

    // Wires up the standard set of slot interactions (hover, drag, right-click).
    // Both DynamicInterface and StaticInterface call this for every slot they create.
    protected void RegisterSlotEvents(GameObject obj)
    {
        AddEvent(obj, EventTriggerType.PointerEnter, delegate { OnEnter(obj); });
        AddEvent(obj, EventTriggerType.PointerExit, delegate { OnExit(obj); });
        AddEvent(obj, EventTriggerType.BeginDrag, delegate { OnDragStart(obj); });
        AddEvent(obj, EventTriggerType.EndDrag, delegate { OnDragEnd(obj); });
        AddEvent(obj, EventTriggerType.Drag, delegate { OnDrag(obj); });
        AddEvent(obj, EventTriggerType.PointerClick, delegate (BaseEventData data) { OnSlotClicked(obj, (PointerEventData)data); });
    }

    public void UpdateInventoryLinks()
    {
        int i = 0;
        foreach (var key in slotsOnInterface.Keys.ToList())
        {
            slotsOnInterface[key] = inventory.GetSlots[i];
            i++;
        }
    }

    public void OnSlotUpdate(InventorySlot slot)
    {
        if (slot.item.Id <= -1)
        {
            slot.slotDisplay.transform.GetChild(0).GetComponent<Image>().sprite = null;
            slot.slotDisplay.transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0);
            slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
        }
        else
        {
            slot.slotDisplay.transform.GetChild(0).GetComponent<Image>().sprite = slot.GetItemObject().uiDisplay;
            slot.slotDisplay.transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            slot.slotDisplay.GetComponentInChildren<TextMeshProUGUI>().text = slot.amount == 1 ? string.Empty : slot.amount.ToString("n0");
        }
    }
    public void Update()
    {
        if (_previousInventory != inventory)
        {
            UpdateInventoryLinks();
        }
        _previousInventory = inventory;

    }

    bool IsItemInSlot(GameObject obj)
    {
        return slotsOnInterface[obj].item.Id >= 0;
    }

    protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        //if (!trigger) { Debug.LogWarning("No EventTrigger component found!"); return; }
        var eventTrigger = new EventTrigger.Entry { eventID = type };
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }

    public void OnEnter(GameObject obj)
    {
        MouseData.slotHoveredOver = obj;
        if (IsItemInSlot(obj))
        {
            InventorySlot slot = slotsOnInterface[obj];
            Tooltip.ShowTooltip(slot.item, slot.GetItemObject().type);
            HighlightCompatibleEquipSlots(slot.GetItemObject());
        }
    }

    public void OnEnterInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = obj.GetComponent<UserInterface>();
    }
    public void OnExitInterface(GameObject obj)
    {
        MouseData.interfaceMouseIsOver = null;
    }

    public void OnExit(GameObject obj)
    {
        MouseData.slotHoveredOver = null;
        Tooltip.HideTooltip();
        ClearEquipSlotHighlights();
    }

    // Outlines every equipment slot that the hovered item could be worn in.
    private void HighlightCompatibleEquipSlots(ItemObject itemObject)
    {
        if (itemObject == null) return;
        foreach (UserInterface ui in AllInterfaces)
        {
            if (ui.inventory == null || ui.inventory.type != InterfaceType.Equipment) continue;
            foreach (InventorySlot slot in ui.slotsOnInterface.Values)
            {
                if (slot.AllowedItems.Length > 0 && slot.CanPlaceInSlot(itemObject))
                    SetSlotOutline(slot.slotDisplay, true);
            }
        }
    }

    private void ClearEquipSlotHighlights()
    {
        foreach (UserInterface ui in AllInterfaces)
        {
            if (ui.inventory == null || ui.inventory.type != InterfaceType.Equipment) continue;
            foreach (InventorySlot slot in ui.slotsOnInterface.Values)
                SetSlotOutline(slot.slotDisplay, false);
        }
    }

    private static void SetSlotOutline(GameObject slotDisplay, bool state)
    {
        if (slotDisplay == null) return;
        Outline outline = slotDisplay.GetComponent<Outline>();
        if (!state)
        {
            if (outline != null) outline.enabled = false;
            return;
        }
        if (outline == null)
        {
            outline = slotDisplay.AddComponent<Outline>();
            outline.effectDistance = new Vector2(3, 3);
        }
        outline.effectColor = CompatibleSlotOutlineColor;
        outline.enabled = true;
    }

    // Right-click a backpack item to auto-equip it into the first compatible
    // equipment slot (preferring an empty one), or right-click an equipped
    // item to send it back to the first empty backpack slot.
    public void OnSlotClicked(GameObject obj, PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (playerInventory == null) return;

        InventorySlot clickedSlot = slotsOnInterface[obj];
        if (clickedSlot.item.Id < 0) return;

        if (inventory.type == InterfaceType.Equipment)
        {
            InventorySlot emptyBackpackSlot = playerInventory.inventory.GetEmptySlot();
            if (emptyBackpackSlot == null) return;
            emptyBackpackSlot.UpdateSlot(clickedSlot.item, clickedSlot.amount);
            clickedSlot.RemoveItem();
        }
        else
        {
            InventorySlot targetEquipSlot = FindCompatibleEquipSlot(clickedSlot.GetItemObject());
            if (targetEquipSlot == null) return;
            playerInventory.equipment.SwapItems(clickedSlot, targetEquipSlot);
        }
        stats.updateTotalStats();
    }

    // Prefers an empty compatible slot; falls back to the first compatible
    // occupied slot so equipping swaps the old item back to the backpack.
    private InventorySlot FindCompatibleEquipSlot(ItemObject itemObject)
    {
        InventorySlot firstMatch = null;
        foreach (InventorySlot slot in playerInventory.equipment.GetSlots)
        {
            if (slot.AllowedItems.Length <= 0 || !slot.CanPlaceInSlot(itemObject)) continue;
            if (slot.item.Id < 0) return slot;
            firstMatch ??= slot;
        }
        return firstMatch;
    }
    public void OnDragStart(GameObject obj)
    {
        MouseData.tempItemBeingDragged = CreateTempItem(obj);
    }
    // Visual indicator of item that follows cursor
    private GameObject CreateTempItem(GameObject obj)
    {
        GameObject tempItem = null;
        if (slotsOnInterface[obj].item.Id >= 0)
        {
            tempItem = new GameObject();
            var rt = tempItem.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50, 50);
            tempItem.transform.SetParent(transform.parent); // bring the front of UI, bottom of UI scene heirarchy
            var img = tempItem.AddComponent<Image>();
            img.sprite = slotsOnInterface[obj].GetItemObject().uiDisplay;
            img.raycastTarget = false;
        }
        return tempItem;
        
    }
    public void OnDrag(GameObject obj)
    {
        if (MouseData.tempItemBeingDragged != null)
            MouseData.tempItemBeingDragged.GetComponent<RectTransform>().position = Input.mousePosition;
    }
    public void OnDragEnd(GameObject obj)
    {
        Destroy(MouseData.tempItemBeingDragged);

        if (MouseData.interfaceMouseIsOver == null)
        {
            slotsOnInterface[obj].RemoveItem();
            return;
        }
        if (MouseData.slotHoveredOver)
        {
            InventorySlot mouseHoverSlotData = MouseData.interfaceMouseIsOver.slotsOnInterface[MouseData.slotHoveredOver];
            inventory.SwapItems(slotsOnInterface[obj], mouseHoverSlotData);
        }
        stats.updateTotalStats();
    }
}
    