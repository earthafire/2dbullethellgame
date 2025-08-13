using UnityEngine;
using Sirenix.OdinInspector;

public class GlobalProjectileManager : MonoBehaviour
{
    [Header("Global Configuration")]
    [LabelWidth(150)]
    [Required("Please assign the shared projectile prefab")]
    public GameObject sharedProjectilePrefab;
    
    [LabelWidth(150)]
    [InfoBox("Reference to the projectile pool component")]
    [Required("Please assign the projectile pool")]
    public ProjectilePool projectilePool;
    
    [Header("Runtime Info")]
    [ShowInInspector]
    [ReadOnly]
    private static GlobalProjectileManager instance;
    
    public static GlobalProjectileManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GlobalProjectileManager>();
                if (instance == null)
                {
                    Debug.LogError("[GlobalProjectileManager] No instance found in scene! Please add GlobalProjectileManager to a GameObject.");
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeManager()
    {
        if (sharedProjectilePrefab == null)
        {
            Debug.LogError("[GlobalProjectileManager] No shared projectile prefab assigned!");
            return;
        }
        
        if (projectilePool == null)
        {
            Debug.LogError("[GlobalProjectileManager] No projectile pool assigned!");
            return;
        }
        
        // Update all AbilityData assets with the shared prefab reference
        UpdateAllAbilityDataAssets();
        
        Debug.Log("[GlobalProjectileManager] Initialized successfully!");
    }
    
    /// <summary>
    /// Get a projectile from the global pool
    /// </summary>
    public static SharedProjectile GetProjectile(ProjectileData data, Vector3 position, Vector2 direction)
    {
        if (Instance?.projectilePool != null)
        {
            return Instance.projectilePool.GetProjectile(data, position, direction);
        }
        
        Debug.LogError("[GlobalProjectileManager] Cannot get projectile: Manager not initialized!");
        return null;
    }
    
    /// <summary>
    /// Return a projectile to the global pool
    /// </summary>
    public static void ReturnProjectile(SharedProjectile projectile)
    {
        if (Instance?.projectilePool != null)
        {
            Instance.projectilePool.ReturnProjectile(projectile);
        }
    }
    
    /// <summary>
    /// Get pool statistics
    /// </summary>
    public static (int active, int inactive, int total) GetPoolStats()
    {
        if (Instance?.projectilePool != null)
        {
            return Instance.projectilePool.GetPoolStats();
        }
        return (0, 0, 0);
    }
    
    /// <summary>
    /// Update all AbilityData assets with the shared prefab reference
    /// </summary>
    [Button("Update All Ability Data Assets")]
    public void UpdateAllAbilityDataAssets()
    {
        if (sharedProjectilePrefab == null)
        {
            Debug.LogError("[GlobalProjectileManager] Cannot update assets: No shared prefab assigned!");
            return;
        }
        
        // Find all AbilityData assets in Resources
        var abilityDataAssets = Resources.LoadAll<AbilityData>("Data/Abilities");
        int updatedCount = 0;
        
        foreach (var abilityData in abilityDataAssets)
        {
            if (abilityData != null)
            {
                abilityData.sharedProjectilePrefab = sharedProjectilePrefab;
                updatedCount++;
            }
        }
        
        Debug.Log($"[GlobalProjectileManager] Updated {updatedCount} AbilityData assets with shared prefab reference");
    }
    
    /// <summary>
    /// Validate the global configuration
    /// </summary>
    [Button("Validate Configuration")]
    public void ValidateConfiguration()
    {
        bool isValid = true;
        
        if (sharedProjectilePrefab == null)
        {
            Debug.LogError("[GlobalProjectileManager] Shared projectile prefab is not assigned!");
            isValid = false;
        }
        else
        {
            var sharedProjectile = sharedProjectilePrefab.GetComponent<SharedProjectile>();
            if (sharedProjectile == null)
            {
                Debug.LogError("[GlobalProjectileManager] Shared projectile prefab does not contain SharedProjectile component!");
                isValid = false;
            }
        }
        
        if (projectilePool == null)
        {
            Debug.LogError("[GlobalProjectileManager] Projectile pool is not assigned!");
            isValid = false;
        }
        
        if (isValid)
        {
            Debug.Log("[GlobalProjectileManager] Configuration validation passed! ✓");
        }
    }
    
    /// <summary>
    /// Clear the projectile pool
    /// </summary>
    [Button("Clear Projectile Pool")]
    public void ClearProjectilePool()
    {
        if (projectilePool != null)
        {
            projectilePool.ClearPool();
        }
    }
    
    /// <summary>
    /// Expand the projectile pool
    /// </summary>
    [Button("Expand Projectile Pool")]
    public void ExpandProjectilePool()
    {
        if (projectilePool != null)
        {
            projectilePool.ExpandPool();
        }
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
