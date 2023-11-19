using System;
using System.Collections;
using System.Collections.Generic;
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
    private Dictionary<AbilitySlot, ActivatableAbility> EquippedAbilities = new Dictionary<AbilitySlot, ActivatableAbility>();

    void Start()
    {
        // instantiate all abilities, for ease of swapping them out later
        abilities = new Dictionary<ActivatableAbilityType, ActivatableAbility>()
        {
            {ActivatableAbilityType.MELEE, gameObject.AddComponent<Melee>()},
            {ActivatableAbilityType.WAND, gameObject.AddComponent<Wand>()},
            {ActivatableAbilityType.FROSTPULSE, gameObject.AddComponent<FrostPulse>()},
            {ActivatableAbilityType.DASH, gameObject.AddComponent<Dash>()},
        };

        // initialize all abilities available
        EquippedAbilities[AbilitySlot.Q] = null;
        EquippedAbilities[AbilitySlot.E] = abilities[ActivatableAbilityType.WAND];
        EquippedAbilities[AbilitySlot.R] = null;
        EquippedAbilities[AbilitySlot.LeftClick] = abilities[ActivatableAbilityType.MELEE];
        EquippedAbilities[AbilitySlot.RightClick] = abilities[ActivatableAbilityType.FROSTPULSE];
        EquippedAbilities[AbilitySlot.Shift] = null;
        EquippedAbilities[AbilitySlot.Space] = abilities[ActivatableAbilityType.DASH];
    }

    /// <summary>
    /// checks for ability keys pressed every update
    /// </summary>
    void FixedUpdate()
    {
        DetectAbilitiesPressed();
    }

    /// <summary>
    /// Iterates through equipped abilities, and triggers them all
    /// </summary>
    public void DetectAbilitiesPressed()
    {
        foreach (var AbilitySlot in EquippedAbilities)
        {
            // if ability button is pressed, trigger ability in that slot (if it exists)
            if (Input.GetButton(AbilitySlot.Key.ToString()) && AbilitySlot.Value != null)
            {
                AbilitySlot.Value.Activate();
            }
        }
    }

    /// <summary>
    /// Sets a specific slot to the class of ability added
    /// </summary>
    /// <param name="slot">ability slot to change</param>
    /// <param name="abilityToAdd">what type the ability slot should be set to</param>
    public void setAbility<T>(AbilitySlot slot, ActivatableAbilityType abilityToAdd)
    {
        // put ability in slot
        if (abilities.ContainsKey(abilityToAdd))
        {
            EquippedAbilities[slot] = abilities[abilityToAdd];
        }

        // Safety if-statement!
        // makes sure you always have a melee at least
        if (EquippedAbilities[AbilitySlot.LeftClick] == null)
        {
            EquippedAbilities[AbilitySlot.LeftClick] = gameObject.AddComponent<Melee>();
        }
    }
}

// list of all available abilities, make sure to add new abilities to `abilities` dictionary above!
public enum ActivatableAbilityType
{
    WAND,
    MELEE,
    DASH,
    FROSTPULSE,

}
