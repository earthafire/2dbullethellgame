using System.Collections;
using System.Collections.Generic;
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
    private Dictionary<AbilitySlot, ActivatableAbility> EquipedAbilities = new Dictionary<AbilitySlot, ActivatableAbility>();

    // this is where abilities are added to slots
    void Start()
    {
        EquipedAbilities[AbilitySlot.Q] = null;
        EquipedAbilities[AbilitySlot.E] = gameObject.AddComponent<Wand>();
        EquipedAbilities[AbilitySlot.R] = null;
        EquipedAbilities[AbilitySlot.LeftClick] = gameObject.AddComponent<Melee>();
        EquipedAbilities[AbilitySlot.RightClick] = gameObject.AddComponent<FrostPulse>();
        EquipedAbilities[AbilitySlot.Shift] = null;
        EquipedAbilities[AbilitySlot.Space] = gameObject.AddComponent<Dash>();
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
        foreach (var AbilitySlot in EquipedAbilities)
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
    /// <param name="abilityToAdd">what the ability slot should be set to</param>
    public void setAbility(AbilitySlot slot, ActivatableAbility abilityToAdd)
    {
        // I need to rethink abilities, I think I will convert the ActivatableAbility class to not use monobehaviours, 
        // so they don't need to be attached to a player as a component and can instead be used as normal C# objects
    }
}
