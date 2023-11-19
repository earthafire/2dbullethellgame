using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Reflection.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// ability slot choices
/// </summary>
public enum AbilitySlot
{
    E,
    Q,
    R,
    LeftClick,
    RightClick,
    Shift,
    Space,
}

public class PlayerAbilityManager : MonoBehaviour
{
    // !! -- REBINDING BUTTONS -- !! 
    // buttons can be bound in Edit>Project Settings>Input Manager
    // !! --                   -- !!


    /// <summary>
    /// Abilities currently equipped on player
    /// </summary>
    /// 
    private Dictionary<ActivatableAbilityType, ActivatableAbility> abilities;
    // Inventory
    private InventoryObject equipment;
    private Dictionary<AbilitySlot, int> EquippedAbilities = new Dictionary<AbilitySlot, int>();

    void Start()
    {
        equipment = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>().equipment;

        // instantiate all abilities, for ease of swapping them out later
        abilities = new Dictionary<ActivatableAbilityType, ActivatableAbility>()
        {
            {ActivatableAbilityType.MELEE, gameObject.AddComponent<Melee>()},
            {ActivatableAbilityType.WAND, gameObject.AddComponent<Wand>()},
            {ActivatableAbilityType.FROSTPULSE, gameObject.AddComponent<FrostPulse>()},
            {ActivatableAbilityType.DASH, gameObject.AddComponent<Dash>()},
        };

        // initialize all abilities available
        // EquippedAbilities[AbilitySlot.Q] = null;
        // EquippedAbilities[AbilitySlot.E] = null;
        // EquippedAbilities[AbilitySlot.R] = null;
        EquippedAbilities[AbilitySlot.LeftClick] = 4;
        EquippedAbilities[AbilitySlot.RightClick] = 6;
        // EquippedAbilities[AbilitySlot.Shift] = null;
        EquippedAbilities[AbilitySlot.Space] = 9;
    }

    void getAbilityFromEquipment(int equipmentID)
    {
        abilities[equipment.GetSlots[equipmentID].item.Ability].Activate();
    }

    /// <summary>
    /// checks for ability keys pressed every update
    /// </summary>
    void FixedUpdate()
    {
        DetectAbilitiesPressed();
        string temp = "";
        int i = 0;
        foreach (InventorySlot x in equipment.GetSlots)
        {
            temp += "Slot#: " + i + " \tAbility: " + x.item.Ability + "\n";
            i++;
        }
        Debug.Log(temp);
    }

    /// <summary>
    /// Iterates through equipped abilities, and triggers them all
    /// </summary>
    public void DetectAbilitiesPressed()
    {
        foreach (var abilitySlotKV in EquippedAbilities)
        {
            // if ability button is pressed, trigger ability in that slot (if it exists)
            if (Input.GetButton(abilitySlotKV.Key.ToString()))
            {
                ActivatableAbilityType typeOfAbility = equipment.GetSlots[abilitySlotKV.Value].item.Ability;
                if (typeOfAbility != ActivatableAbilityType.NULL)
                {
                    abilities[typeOfAbility].Activate();
                }
                else if (abilitySlotKV.Key == AbilitySlot.LeftClick)
                {
                    abilities[ActivatableAbilityType.MELEE].Activate();
                }
            }
        }
    }
}

// list of all available abilities, make sure to add new abilities to `abilities` dictionary above!
public enum ActivatableAbilityType
{
    NULL,
    WAND,
    MELEE,
    DASH,
    FROSTPULSE,
}
