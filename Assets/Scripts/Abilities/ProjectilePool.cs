using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [Header("Pool Configuration")]
    [SerializeField] private GameObject sharedProjectilePrefab;
    [SerializeField] private int initialPoolSize = 50;
    [SerializeField] private int maxPoolSize = 1000;
    
    [Header("Runtime Info")]
    [SerializeField] private int activeProjectiles;
    [SerializeField] private int inactiveProjectiles;
    
    private Queue<SharedProjectile> inactiveProjectilesQueue = new Queue<SharedProjectile>();
    private List<SharedProjectile> activeProjectilesList = new List<SharedProjectile>();
    
    private void Awake()
    {
        if (sharedProjectilePrefab == null)
        {
            Debug.LogError("[ProjectilePool] No shared projectile prefab assigned!");
            return;
        }
        
        // Pre-populate the pool
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewProjectile();
        }
        
        Debug.Log($"[ProjectilePool] Initialized with {initialPoolSize} projectiles");
    }
    
    private void CreateNewProjectile()
    {
        var projectile = Instantiate(sharedProjectilePrefab, transform).GetComponent<SharedProjectile>();
        if (projectile != null)
        {
            projectile.OnProjectileDestroyed += OnProjectileDestroyed;
            projectile.gameObject.SetActive(false);
            inactiveProjectilesQueue.Enqueue(projectile);
            inactiveProjectiles++;
            
            Debug.Log($"[ProjectilePool] Created new projectile. Total inactive: {inactiveProjectiles}");
        }
        else
        {
            Debug.LogError("[ProjectilePool] Failed to create new projectile!");
        }
    }
    
    // Track last spawn time to prevent rapid spawning issues
    private float lastSpawnTime = 0f;
    
    /// <summary>
    /// Get a projectile from the pool and configure it
    /// </summary>
    public SharedProjectile GetProjectile(ProjectileData data, Vector3 position, Vector2 direction)
    {
        SharedProjectile projectile = null;
        
        // Try to get from inactive queue first
        if (inactiveProjectilesQueue.Count > 0)
        {
            projectile = inactiveProjectilesQueue.Dequeue();
            inactiveProjectiles--;
            
            // Safety check - ensure the projectile is actually inactive
            if (projectile.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[ProjectilePool] Retrieved projectile that was still active! Deactivating it.");
                projectile.gameObject.SetActive(false);
            }
            
            // Safety check - ensure the projectile is not already in the active list
            if (activeProjectilesList.Contains(projectile))
            {
                Debug.LogWarning($"[ProjectilePool] Retrieved projectile that was already in active list! Removing it.");
                activeProjectilesList.Remove(projectile);
                activeProjectiles--;
            }
        }
        else if (activeProjectilesList.Count < maxPoolSize)
        {
            // Create new projectile if we haven't hit the max
            CreateNewProjectile();
            projectile = inactiveProjectilesQueue.Dequeue();
            inactiveProjectiles--;
        }
        else
        {
            // Reuse the oldest active projectile if we've hit the max
            if (activeProjectilesList.Count > 0)
            {
                projectile = activeProjectilesList[0];
                activeProjectilesList.RemoveAt(0);
                activeProjectiles--;
                
                // Just remove from active list - don't deactivate/reactivate here
                // The projectile will be properly configured and activated below
            }
        }
        
        if (projectile != null)
        {
/*            // CRITICAL FIX: Add small delay for rapid spawning to prevent physics issues
            float timeSinceLastSpawn = Time.time - lastSpawnTime;
            if (timeSinceLastSpawn < 0.01f) // 10ms minimum between spawns
            {
                Debug.LogWarning($"[ProjectilePool] Rapid spawning detected ({timeSinceLastSpawn:F3}s), adding small delay");
                // Add a small delay by moving the spawn position slightly
                position += Vector3.up * 0.01f; // Tiny offset to prevent exact overlap
            }
            lastSpawnTime = Time.time;
            
            // CRITICAL FIX: Validate spawn position to prevent overlapping projectiles
            Vector3 validatedPosition = ValidateSpawnPosition(position, data);
            if (validatedPosition != position)
            {
                Debug.LogWarning($"[ProjectilePool] Adjusted spawn position from {position} to {validatedPosition} to prevent overlap");
            }*/
            
            // Activate first, then configure (coroutines need active GameObject)
            projectile.transform.position = position;
            projectile.gameObject.SetActive(true);
            projectile.ConfigureProjectile(data, direction);
            
            // Add to active list (ensure it's not already there)
            if (!activeProjectilesList.Contains(projectile))
            {
                activeProjectilesList.Add(projectile);
                activeProjectiles++;
                Debug.Log($"[ProjectilePool] Got projectile from pool. Active: {activeProjectiles}, Inactive: {inactiveProjectiles}");
            }
            else
            {
                Debug.LogWarning($"[ProjectilePool] Projectile was already in active list! This shouldn't happen.");
            }
        }
        else
        {
            Debug.LogWarning($"[ProjectilePool] Failed to get projectile from pool. Active: {activeProjectiles}, Inactive: {inactiveProjectiles}");
        }
        
        return projectile;
    }
    
    /// <summary>
    /// Return a projectile to the pool
    /// </summary>
    public void ReturnProjectile(SharedProjectile projectile)
    {
        if (projectile == null) return;
        
        // Safety check - ensure projectile is not already in the inactive queue
        if (inactiveProjectilesQueue.Contains(projectile))
        {
            Debug.LogWarning($"[ProjectilePool] Projectile was already in inactive queue when returning to pool! Ignoring duplicate return.");
            return;
        }
        
        Debug.Log($"[ProjectilePool] Returning projectile to pool. Active: {activeProjectiles}, Inactive: {inactiveProjectiles}");
        
        // Remove from active list
        if (activeProjectilesList.Contains(projectile))
        {
            activeProjectilesList.Remove(projectile);
            activeProjectiles--;
        }
        else
        {
            Debug.LogWarning($"[ProjectilePool] Projectile was not in active list when returning to pool!");
        }
        
        // Safety check - ensure projectile is not already in the inactive queue
        if (inactiveProjectilesQueue.Contains(projectile))
        {
            Debug.LogWarning($"[ProjectilePool] Projectile was already in inactive queue when returning to pool! Ignoring duplicate return.");
            return;
        }
        
        // Note: The projectile might still be active when returned to pool
        // This is expected behavior when DestroyProjectile calls ReturnProjectile before deactivating
        if (projectile.gameObject.activeInHierarchy)
        {
            Debug.Log($"[ProjectilePool] Projectile is still active when returned to pool - this is expected during destruction");
        }
        
        // Reset and return to inactive queue
        projectile.transform.SetParent(transform);
        inactiveProjectilesQueue.Enqueue(projectile);
        inactiveProjectiles++;
        
        Debug.Log($"[ProjectilePool] Projectile returned to pool. Active: {activeProjectiles}, Inactive: {inactiveProjectiles}");
    }
    
    private void OnProjectileDestroyed(SharedProjectile projectile)
    {
        ReturnProjectile(projectile);
    }
    
    /// <summary>
    /// Validate spawn position to prevent overlapping projectiles
    /// </summary>
    private Vector3 ValidateSpawnPosition(Vector3 requestedPosition, ProjectileData data)
    {
        // Check if there are any active projectiles too close to the requested position
        float minDistance = 0.5f; // Minimum distance between projectiles
        
        foreach (var activeProjectile in activeProjectilesList)
        {
            if (activeProjectile != null && activeProjectile.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(requestedPosition, activeProjectile.transform.position);
                if (distance < minDistance)
                {
                    // Find a safe position by moving away from the conflicting projectile
                    Vector3 direction = (requestedPosition - activeProjectile.transform.position).normalized;
                    Vector3 safePosition = activeProjectile.transform.position + (direction * minDistance);
                    
                    // Ensure the safe position is still in the general direction of the original request
                    Vector3 originalDirection = (requestedPosition - transform.position).normalized;
                    Vector3 safeDirection = (safePosition - transform.position).normalized;
                    
                    // If the safe position is too far from the original direction, use the original
                    if (Vector3.Dot(originalDirection, safeDirection) > 0.5f)
                    {
                        return safePosition;
                    }
                }
            }
        }
        
        return requestedPosition;
    }
    
    /// <summary>
    /// Clear all projectiles from the pool
    /// </summary>
    [ContextMenu("Clear Pool")]
    public void ClearPool()
    {
        // Clear active projectiles
        foreach (var projectile in activeProjectilesList)
        {
            if (projectile != null)
            {
                DestroyImmediate(projectile.gameObject);
            }
        }
        activeProjectilesList.Clear();
        activeProjectiles = 0;
        
        // Clear inactive projectiles
        inactiveProjectilesQueue.Clear();
        inactiveProjectiles = 0;
        
        Debug.Log("[ProjectilePool] Pool cleared");
    }
    
    /// <summary>
    /// Expand the pool by creating more projectiles
    /// </summary>
    [ContextMenu("Expand Pool")]
    public void ExpandPool()
    {
        int currentTotal = activeProjectiles + inactiveProjectiles;
        int toAdd = Mathf.Min(20, maxPoolSize - currentTotal);
        
        for (int i = 0; i < toAdd; i++)
        {
            CreateNewProjectile();
        }
        
        Debug.Log($"[ProjectilePool] Added {toAdd} projectiles to pool");
    }
    
    /// <summary>
    /// Get pool statistics
    /// </summary>
    public (int active, int inactive, int total) GetPoolStats()
    {
        return (activeProjectiles, inactiveProjectiles, activeProjectiles + inactiveProjectiles);
    }
    
    private void OnDestroy()
    {
        // Clean up event subscriptions
        foreach (var projectile in activeProjectilesList)
        {
            if (projectile != null)
            {
                projectile.OnProjectileDestroyed -= OnProjectileDestroyed;
            }
        }
        
        foreach (var projectile in inactiveProjectilesQueue)
        {
            if (projectile != null)
            {
                projectile.OnProjectileDestroyed -= OnProjectileDestroyed;
            }
        }
    }
}
