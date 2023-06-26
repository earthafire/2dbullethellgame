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
        Activated();
        cooldownRemainingTime = cooldownTimeMax;
        while (cooldownRemainingTime > 0)
        {
            cooldownRemainingTime -= Time.deltaTime;

            // This will wait until the next frame before continuing execution
            yield return null;
        }
        cooldownRemainingTime = 0f;
    }

    // Weapon's ability (override this)
    public abstract void Activated();

}
