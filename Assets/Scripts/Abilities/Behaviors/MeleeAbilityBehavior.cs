using UnityEngine;

/// <summary>
/// Behavior for melee attack abilities
/// </summary>
public class MeleeAbilityBehavior : ActivatableAbility
{
    [Header("Melee Settings")]
    [SerializeField] private GameObject meleeEffectPrefab;
    [SerializeField] private float meleeRange = 1.5f;
    [SerializeField] private float meleeDamage = 25f;
    [SerializeField] private float meleeKnockback = 1.5f;

    public override void Initialize(AbilityData data)
    {
        base.Initialize(data);

        // Try to find the melee effect prefab from the ability data
        if (data.effectPrefab != null)
        {
            meleeEffectPrefab = data.effectPrefab;
        }

        if (meleeEffectPrefab == null)
        {
            Debug.LogWarning($"[{name}] No melee effect prefab assigned!");
        }
    }

    public override void Activated()
    {
        // Get attack direction
        Vector2 attackDirection = GetFireDirection();

        // Spawn melee effect
        if (meleeEffectPrefab != null)
        {
            Vector3 spawnPosition = firePoint.position + (Vector3)(attackDirection * 0.5f);
            var meleeEffect = Instantiate(meleeEffectPrefab, spawnPosition, Quaternion.identity);

            // Configure the effect if it has an AbilityObject component
            var abilityObject = meleeEffect.GetComponent<AbilityObject>();

        }

        // Deal damage to enemies in range
        DealMeleeDamage(attackDirection);

        Debug.Log($"[{name}] Melee attack in direction {attackDirection}");
    }

    private void DealMeleeDamage(Vector2 direction)
    {
        // Find enemies in melee range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, meleeRange);

        foreach (var collider in colliders)
        {
            if (collider.gameObject.layer == 7 || collider.gameObject.layer == 9) // Enemy layers
            {
                var enemy = collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // Check if enemy is in front of player
                    Vector2 toEnemy = (collider.transform.position - transform.position).normalized;
                    float dotProduct = Vector2.Dot(direction, toEnemy);

                    if (dotProduct > 0.5f) // Enemy is in front (within ~60 degrees)
                    {
                        // Deal damage
                        enemy.TakeDamage((int)meleeDamage, transform, GetInstanceID());

                        // Apply knockback
                        enemy.GetKnockbacked(transform, meleeKnockback);

                        Debug.Log($"[{name}] Hit enemy {enemy.name} for {meleeDamage} damage");
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            // Draw melee range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, meleeRange);

            // Draw attack direction
            Vector2 direction = GetFireDirection();
            Gizmos.DrawLine(firePoint.position, firePoint.position + (Vector3)(direction * meleeRange));
        }
    }
}
