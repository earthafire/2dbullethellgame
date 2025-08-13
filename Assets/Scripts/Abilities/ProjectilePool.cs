using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [Header("Pool Configuration")]
    [SerializeField] private GameObject sharedProjectilePrefab;
    [SerializeField] private int initialPoolSize = 50;
    [SerializeField] private int maxPoolSize = 200;
    
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
        }
    }
    
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
            // Reuse the oldest active projectile
            projectile = activeProjectilesList[0];
            activeProjectilesList.RemoveAt(0);
            activeProjectiles--;
            
            // Reset the projectile
            projectile.gameObject.SetActive(false);
            projectile.gameObject.SetActive(true);
        }
        
        if (projectile != null)
        {
            // Configure and activate
            projectile.transform.position = position;
            projectile.gameObject.SetActive(true);
            projectile.ConfigureProjectile(data, direction);
            
            // Add to active list
            activeProjectilesList.Add(projectile);
            activeProjectiles++;
        }
        
        return projectile;
    }
    
    /// <summary>
    /// Return a projectile to the pool
    /// </summary>
    public void ReturnProjectile(SharedProjectile projectile)
    {
        if (projectile == null) return;
        
        // Remove from active list
        if (activeProjectilesList.Contains(projectile))
        {
            activeProjectilesList.Remove(projectile);
            activeProjectiles--;
        }
        
        // Reset and return to inactive queue
        projectile.gameObject.SetActive(false);
        projectile.transform.SetParent(transform);
        inactiveProjectilesQueue.Enqueue(projectile);
        inactiveProjectiles++;
    }
    
    private void OnProjectileDestroyed(SharedProjectile projectile)
    {
        ReturnProjectile(projectile);
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
