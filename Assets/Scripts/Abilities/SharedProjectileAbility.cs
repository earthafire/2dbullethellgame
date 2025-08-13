using UnityEngine;

public class SharedProjectileAbility : ActivatableAbility
{
    private AbilityData abilityData;
    private Transform firePoint;
    
    public void Initialize(AbilityData data)
    {
        abilityData = data;
        cooldownTimeMax = data.baseCooldown;
        
        // Find fire point (usually a child transform)
        firePoint = transform.Find("FirePoint");
        if (firePoint == null)
        {
            // Create a fire point if none exists
            var firePointObj = new GameObject("FirePoint");
            firePoint = firePointObj.transform;
            firePoint.SetParent(transform);
            firePoint.localPosition = Vector3.right * 0.5f; // Default position to the right
        }
    }
    
    public override void Activated()
    {
        if (abilityData?.projectileData == null)
        {
            Debug.LogError($"[{name}] Cannot activate ability: No projectile data assigned!");
            return;
        }
        
        // Get fire direction (towards mouse cursor)
        Vector2 fireDirection = GetFireDirection();
        
        // Spawn projectile from the global pool
        var projectile = GlobalProjectileManager.GetProjectile(abilityData.projectileData, firePoint.position, fireDirection);
        
        if (projectile == null)
        {
            Debug.LogError($"[{name}] Failed to spawn projectile!");
        }
    }
    
    private Vector2 GetFireDirection()
    {
        if (Camera.main == null) return Vector2.right;
        
        // Get mouse position in world space
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        
        // Calculate direction from player to mouse
        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        
        return direction;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
            Gizmos.DrawLine(firePoint.position, firePoint.position + (Vector3)GetFireDirection() * 2f);
        }
    }
}
