using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    // Holds all abilities
    // Ability 0 or 3: mouse left click
    // Ability 1 or 4: mouse right click
    // Ability 2 or 5: space bar
    private ActivatableAbility[] AbilitySlots = new ActivatableAbility[6];

    // Modifier key (swaps ability slots) shift and tab by default
    // If true, use abilites 3,4,5 (indexes in the AbilitySlots array)
    // If false, use abilites 0,1,2
    public bool useAltAbilities = false;

    bool swapIsOnCooldown = false; // blocks swap so you don't swap a bunch really fast
    float swapCooldownTime = 1f; // seconds

    void Start()
    {
        GameObject player = GameObject.Find("Player");
        AbilitySlots[0] = player.AddComponent<Wand>() as Wand;
    }

    /// <summary>
    /// checks for ability keys pressed every update
    /// </summary>
    void FixedUpdate()
    {
        DetectAbilitiesPressed(useAltAbilities);
        DetectModifierPressed();
    }

    /// <summary>
    /// Checks if ability keys are pressed and tries to activate them 
    /// </summary>
    /// <param name="useAltAbilities">
    /// If true, uses abilites 3,4,5 (in the AbilitySlots array)
    /// If false, uses abilites 0,1,2
    /// </param>
    public void DetectAbilitiesPressed(bool useAltAbilities)
    {
        // this value is added to slot checked 
        // basically uses slots 0,1,2 by default and 3,4,5 when modified
        int slotModifierValue = 0;
        if (useAltAbilities)
        {
            slotModifierValue = 3;
        }

        for (int i = 0; i <= 2; i++)
        {
            int abilityNumber = i + slotModifierValue;

            // if ability button is pressed, trigger ability in that slot (if it exists)
            if (Input.GetButton("Ability" + i))
            {
                ActivatableAbility abilityToActivate = AbilitySlots[abilityNumber];
                if (abilityToActivate != null)
                {
                    AbilitySlots[abilityNumber].Activate();
                }

            }
        }
    }

    /// <summary>
    /// Checks if the modifier key is pressed (default shift or tab) and swaps the
    /// ability set (123 -> 456) (if this hasn't been done in the last second)
    /// </summary>
    public void DetectModifierPressed()
    {
        if (Input.GetButton("AbilityModifier"))
        {
            if (!swapIsOnCooldown)
            {
                swapIsOnCooldown = true;
                StartCoroutine(ToggleAbilities(swapCooldownTime));
            }
        }
    }

    /// <summary>
    /// toggles useAltAbilities boolean and waits {seconds} time before allowing
    /// another toggle
    /// </summary>
    /// <param name="seconds"> time to wait (in seconds) </param>
    /// <returns></returns>
    private IEnumerator ToggleAbilities(float seconds)
    {
        useAltAbilities = !useAltAbilities; // inverse boolean
        yield return new WaitForSeconds(seconds);
        swapIsOnCooldown = false; // allow toggling again
    }
}
