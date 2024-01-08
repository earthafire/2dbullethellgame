using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActivatableAbility : MonoBehaviour
{
    Coroutine cooldownCoroutine;
    public float cooldownTimeMax = 200f; // seconds
    public float cooldownRemainingTime { get; private set; } = 0;
    private float cooldown_timer = 0f;

    // Activates weapon's ability (Activated) if cooldown is met
    public void Activate()
    {
        if (cooldownRemainingTime <= 0)
        {
            Activated();
            cooldownCoroutine = StartCoroutine(CountCooldown());
        }
    }

    public void ResetCooldown()
    {
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownRemainingTime = 0f;
        }
    }

    private IEnumerator CountCooldown()
    {
        cooldownRemainingTime = CalculateModifiedCooldown(cooldownTimeMax);
        while (cooldownRemainingTime > 0)
        {
            cooldownRemainingTime -= Time.deltaTime;

            // This will wait until the next frame before continuing execution
            yield return null;
        }
        cooldownRemainingTime = 0f;
    }

    /// <summary>
    /// gets what the cooldown should be after all effects
    /// </summary>
    /// <param name="baseCooldown">base cooldown to convert</param>
    /// <returns>float cooldown time</returns>
    public float CalculateModifiedCooldown(float baseCooldown)
    {
        // haste based system

        // examples:
        // cdr points   |   percentage reduction
        //      0       |   0
        //      10      |  ~10%
        //      30      |  ~23%
        //      50      |   33%
        //      75      |   43%
        //      100     |   50%
        //      200     |   66%
        //      300     |   75%
        //      400     |   80%


        // this will be .10 or .66 or something, its a percentage
        float percentageReduction = 100 / (100 + PlayerAttributes.stats[Attribute.cooldown]);
        float modifiedCooldown = baseCooldown * percentageReduction;
        Debug.Log("CD% " + percentageReduction + " CD: " + modifiedCooldown);

        return modifiedCooldown;
    }

    // Weapon's ability (override this)
    public abstract void Activated();

}
