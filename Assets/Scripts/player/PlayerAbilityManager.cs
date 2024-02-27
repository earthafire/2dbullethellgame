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
    public bool autofire = true;
    // !! -- REBINDING BUTTONS -- !! 
    // buttons can be bound in Edit>Project Settings>Input Manager
    // !! --                   -- !!


    /// <summary>
    /// Abilities currently equipped on player
    /// </summary>
    /// 
    private Dictionary<ActivatableAbilityType, ActivatableAbility> abilities;

    /// <summary>
    /// Inventory equipment (10 slots, top -> bottom, left -> right, 0 - 9)
    /// </summary>
    private InventoryObject equipment;

    /// <summary>
    /// Map of AbilitySlots (keybinds) to ints (Equipment slots)
    /// </summary>
    private Dictionary<AbilitySlot, int> EquippedAbilities = new Dictionary<AbilitySlot, int>();

    public bool suspendAbilities;
    void Start()
    {
        suspendAbilities = false;
        equipment = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>().equipment;

        // instantiate all abilities, for ease of swapping them out later
        abilities = new Dictionary<ActivatableAbilityType, ActivatableAbility>()
        {
            {ActivatableAbilityType.MELEE, gameObject.AddComponent<Melee>()},
            {ActivatableAbilityType.WAND, gameObject.AddComponent<Wand>()},
            {ActivatableAbilityType.FROSTPULSE, gameObject.AddComponent<FrostPulse>()},
            {ActivatableAbilityType.DASH, gameObject.AddComponent<Dash>()},
            {ActivatableAbilityType.ELECTRICSPIN, gameObject.AddComponent<ElectricSpinManager>()},
            {ActivatableAbilityType.WINDSHIELD, gameObject.AddComponent<WindShieldManager>()}
        };

        // initialize all abilities available
        EquippedAbilities[AbilitySlot.Q] = 5;
        EquippedAbilities[AbilitySlot.E] = 7;
        EquippedAbilities[AbilitySlot.R] = 2;
        // EquippedAbilities[AbilitySlot.Shift] = null;
        EquippedAbilities[AbilitySlot.LeftClick] = 4;
        EquippedAbilities[AbilitySlot.RightClick] = 6;
        EquippedAbilities[AbilitySlot.Space] = 9;
    }

    /// <summary>
    /// given an int (equipmentID) returns the type of ability in that slot
    /// </summary>
    /// <param name="equipmentID">equipment slot to check</param>
    void getAbilityFromEquipmentID(int equipmentID)
    {
        abilities[equipment.GetSlots[equipmentID].item.Ability].Activate();
    }

    /// <summary>
    /// checks for ability keys pressed every update
    /// </summary>
    void FixedUpdate()
    {
        if (!suspendAbilities)
        {
            ActivateAbilities();
        }
    }

    /// <summary>
    /// Iterates through equipped abilities, and triggers them all
    /// </summary>
    public bool DetectAbilitiesPressed(KeyValuePair<AbilitySlot,int> abilitySlotKV)
    {
        if (Input.GetButton(abilitySlotKV.Key.ToString()))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ActivateAbilities()
    {

            foreach (var abilitySlotKV in EquippedAbilities)
            {
                ActivatableAbilityType typeOfAbility = equipment.GetSlots[abilitySlotKV.Value].item.Ability;

                if (autofire)
                {
                    if (typeOfAbility != ActivatableAbilityType.NULL)
                    {
                        abilities[typeOfAbility].Activate();
                    }
                    else if (abilitySlotKV.Key == AbilitySlot.LeftClick)
                    {
                        abilities[ActivatableAbilityType.MELEE].Activate();
                    }
                }
                else
                {
                    if(DetectAbilitiesPressed(abilitySlotKV))
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

// list of all available abilities, ALSO ADD TO `abilities` dictionary ABOVE!
public enum ActivatableAbilityType
{
    NULL,
    WAND,
    MELEE,
    DASH,
    FROSTPULSE,
    ELECTRICSPIN,
    WINDSHIELD
}
