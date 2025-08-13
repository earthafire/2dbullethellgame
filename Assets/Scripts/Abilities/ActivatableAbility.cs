using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActivatableAbility : MonoBehaviour
{
    public GameObject player;
    protected Transform firePoint;

    Coroutine cooldownCoroutine;
    public AbilityData abilityData { get; private set; }

    public float cooldownTimeMax = 200f; // in seconds, overwritten in inheriting class
    public float cooldownRemainingTime { get; private set; } = 0;


    private void Awake()
    {
        player = GlobalReferences.player;
    }

    public virtual void Initialize(AbilityData abilityData)
    {
        // Find or create fire point
        firePoint = transform.Find("FirePoint");
        if (firePoint == null)
        {
            var firePointObj = new GameObject("FirePoint");
            firePoint = firePointObj.transform;
            firePoint.SetParent(transform);
            firePoint.localPosition = Vector3.right * 0.5f;
        }

        this.abilityData = abilityData;
        this.cooldownTimeMax = abilityData.baseCooldown;
    }

    // Activates weapon's ability (Activated) if cooldown is met
    public void Activate()
    {
        if (cooldownRemainingTime <= 0)
        {
            Activated();
            cooldownCoroutine = StartCoroutine(CountCooldown());
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

    protected Vector2 GetFireDirection()
    {
        if (Camera.main == null) return Vector2.right;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector2 direction = (mouseWorldPos - GlobalReferences.player.transform.position).normalized;
        return direction;
    }

    public virtual void Cleanup()
    {
        // Override in derived classes if cleanup is needed
    }
}
