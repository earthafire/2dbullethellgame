using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActivatableAbility : MonoBehaviour
{
    Coroutine cooldownCoroutine;
    Coroutine durationCoroutine;

    public float cooldownTimeMax = 200f; // in seconds, overwritten in inheriting class
    public float durationTimeMax = 200f; // in seconds, overwritten in inheriting class
    public float cooldownRemainingTime { get; private set; } = 0;
    public float durationRemainingTime { get; private set; } = 0;

    // Activates weapon's ability (Activated) if cooldown is met
    public void Activate()
    {
        if (cooldownRemainingTime <= 0)
        {
            Activated();
            cooldownCoroutine = StartCoroutine(CountCooldown());
            //durationCoroutine = StartCoroutine(CountDuration());
        }
    }
    public abstract void Activated(); // Weapon's ability override this
    
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

            yield return null; // This will wait until the next frame before continuing execution
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
        // ability haste based system

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
        // Debug.Log("CD% " + percentageReduction + " CD: " + modifiedCooldown);

        return modifiedCooldown;
    }

    public static float CalculateModifiedDuration(float baseValue)
    {
        float durationFactor = PlayerAttributes.stats[Attribute.duration];
        float modifiedDuration = baseValue * durationFactor;
        print(modifiedDuration);
        return modifiedDuration;
    }
    public static Vector3 CalculateModifiedSize(Vector3 baseSize)
    {
        float scaleFactor = PlayerAttributes.stats[Attribute.size];
        Vector3 modifiedSize = new Vector3(baseSize.x * scaleFactor, baseSize.y * scaleFactor, baseSize.z);
        return modifiedSize;
    }
    public static float CalculateModifiedBulletSpeed(float baseValue)
    {
        float scaleFactor = PlayerAttributes.stats[Attribute.bulletSpeed];
        float modifiedSpeed = baseValue * scaleFactor;
        print(modifiedSpeed);
        return modifiedSpeed;
    }
}
