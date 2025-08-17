using UnityEngine;

/// <summary>
/// Behavior for projectile-based abilities (Wand, etc.)
/// </summary>
public class ProjectileAbilityBehavior : ActivatableAbility
{
    SharedProjectile projectile;

    public override void Activated()
    {
        ActivatedProjectile();
    }

    public void ActivatedProjectile()
    {
        if (abilityData?.projectileData == null)
        {
            Debug.LogError($"[{name}] Cannot activate projectile ability: No projectile data assigned!");
            return;
        }

        // Use fire point position if no specific position provided
        Vector3 spawnPosition = firePoint.position;

        // Use calculated direction if no specific direction provided
        Vector2 fireDirection = GetFireDirection();

        // Spawn projectile from the global pool
        projectile = GlobalProjectileManager.GetProjectile(abilityData.projectileData, spawnPosition, fireDirection);

        if (projectile == null)
        {
            Debug.LogError($"[{name}] Failed to spawn projectile!");
        }
    }

}
