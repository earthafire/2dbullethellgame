using UnityEngine;

/// <summary>
/// Behavior for area-of-effect abilities (FrostPulse, WindShield, etc.)
/// </summary>
public class AreaEffectAbilityBehavior : ActivatableAbility
{
    [Header("Area Effect Settings")]
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private Vector3 effectOffset = Vector3.zero;
    [SerializeField] private bool attachToPlayer = true;

    private GameObject currentEffect;

    public override void Initialize(AbilityData data)
    {
        base.Initialize(data);

        // Try to find the effect prefab from the ability data
        if (data.effectPrefab != null)
        {
            effectPrefab = data.effectPrefab;
        }

        if (effectPrefab == null)
        {
            Debug.LogWarning($"[{name}] No effect prefab assigned for area effect ability!");
        }
    }

    public override void Activated()
    {
        if (effectPrefab == null)
        {
            Debug.LogError($"[{name}] Cannot activate area effect ability: No effect prefab assigned!");
            return;
        }

        // Clean up any existing effect
        Cleanup();

        // Determine spawn position
        Vector3 spawnPosition;
        if (attachToPlayer)
        {
            spawnPosition = transform.position + effectOffset;
        }
        else
        {
            spawnPosition = firePoint.position;
        }

        // Spawn the effect
        if (attachToPlayer)
        {
            currentEffect = Instantiate(effectPrefab, spawnPosition, Quaternion.identity, transform);
        }
        else
        {
            currentEffect = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);
        }

        // Configure the effect if it has an AbilityObject component
        var abilityObject = currentEffect.GetComponent<AbilityObject>();

        Debug.Log($"[{name}] Activated area effect at {spawnPosition}");
    }

    public override void Cleanup()
    {
        if (currentEffect != null)
        {
            DestroyImmediate(currentEffect);
            currentEffect = null;
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}
