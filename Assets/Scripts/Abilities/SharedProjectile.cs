using UnityEngine;
using System.Collections;

public class SharedProjectile : MonoBehaviour
{
    [Header("Runtime Configuration")]
    [SerializeField] private ProjectileData projectileData;
    
    // Components
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D projectileCollider;
    
    // Runtime state
    private float currentDamage;
    private float currentSpeed;
    private float currentDuration;
    private float currentPierce;
    private float currentKnockback;
    private bool isHoming;
    private float homingRange;
    private bool isBouncy;
    private int maxBounces;
    private int currentBounces;
    
    // References
    private Transform player;
    private Vector2 direction;
    private float spawnTime;
    Coroutine duration;
    
    // State tracking
    private bool isConfigured = false;
    private bool isDestroying = false; // Prevent multiple destruction calls
    
    // Unique identifier for debugging
    private int projectileId;
    private static int nextProjectileId = 0;
    
    // Events
    public System.Action<SharedProjectile> OnProjectileDestroyed;
    
    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        if (projectileCollider == null)
        {
            projectileCollider = gameObject.AddComponent<CircleCollider2D>();
        }
    }
    
    private void OnEnable()
    {
        // Prevent multiple OnEnable calls on the same instance
        if (isConfigured && duration != null)
        {
            Debug.LogWarning($"SharedProjectile [{projectileId}]: OnEnable called on already configured projectile - forcing cleanup");
            // Force cleanup of any existing state
            if (duration != null)
            {
                StopCoroutine(duration);
                duration = null;
            }
            isConfigured = false;
            isDestroying = false;
        }
        
        // Additional safety check - ensure this projectile is not already active
        if (gameObject.activeInHierarchy && isConfigured)
        {
            Debug.LogWarning($"SharedProjectile [{projectileId}]: OnEnable called on already active projectile - forcing cleanup");
            // Force cleanup of any existing state
            if (duration != null)
            {
                StopCoroutine(duration);
                duration = null;
            }
            isConfigured = false;
            isDestroying = false;
        }
        
        // Additional safety check - if this projectile was already active and configured, 
        // it might be a reuse case from the pool, so we need to be extra careful
        if (projectileId > 0 && isConfigured)
        {
            Debug.LogWarning($"SharedProjectile [{projectileId}]: OnEnable called on reused projectile - ensuring clean state");
            // Force cleanup of any existing state
            if (duration != null)
            {
                StopCoroutine(duration);
                duration = null;
            }
            isConfigured = false;
            isDestroying = false;
        }
        
        // Assign unique ID for debugging (only if not already assigned)
        if (projectileId == 0)
        {
            projectileId = ++nextProjectileId;
        }
        
        Debug.Log($"SharedProjectile [{projectileId}]: OnEnable called - initializing new projectile");
        
        spawnTime = Time.time;
        player = GlobalReferences.player?.transform;
        
        // Reset configuration state
        isConfigured = false;
        isDestroying = false;
        
        // Reset visual appearance
        ResetVisualAppearance();
        
        // CRITICAL FIX: Re-enable the collider when recycling projectiles
        if (projectileCollider != null)
        {
            projectileCollider.enabled = true;
            Debug.Log($"SharedProjectile [{projectileId}]: Re-enabled collider for recycled projectile");
        }
        
        // Ensure any leftover coroutines are stopped when reactivating from pool
        if (duration != null)
        {
            StopCoroutine(duration);
            duration = null;
        }
        
        // Don't start lifetime coroutine here - wait for ConfigureProjectile to set the duration first
    }
    
    private void OnDisable()
    {
        Debug.Log($"SharedProjectile [{projectileId}]: OnDisable called - cleaning up coroutines");
        
        // Clean up coroutines when projectile is disabled (returned to pool)
        if (duration != null)
        {
            StopCoroutine(duration);
            duration = null;
        }
        
        // Only reset state if this is not a destruction process
        // If isDestroying is true, the DestroyProjectile method will handle the state
        if (!isDestroying)
        {
            // Ensure the projectile is properly reset for the next use
            isConfigured = false;
            isDestroying = false;
        }
        else
        {
            Debug.Log($"SharedProjectile [{projectileId}]: OnDisable called during destruction - preserving state");
        }
    }
    
    private void Update()
    {
        if (isHoming)
        {
            UpdateHoming();
        }
    }
    
    /// <summary>
    /// Configure this projectile with data from an ability
    /// </summary>
    public void ConfigureProjectile(ProjectileData data, Vector2 fireDirection)
    {
        if (data == null)
        {
            Debug.LogError($"SharedProjectile [{projectileId}]: Cannot configure with null ProjectileData!");
            return;
        }
        
        // Prevent multiple configuration calls on the same projectile
        if (isConfigured)
        {
            Debug.LogWarning($"SharedProjectile [{projectileId}]: ConfigureProjectile called on already configured projectile - forcing cleanup and reconfiguring");
            // Force cleanup of any existing state
            if (duration != null)
            {
                StopCoroutine(duration);
                duration = null;
            }
            isConfigured = false;
            isDestroying = false;
        }
        
        projectileData = data;
        direction = fireDirection.normalized;
        
        // Apply visual configuration
        if (data.projectileSprite != null)
        {
           //spriteRenderer.sprite = data.projectileSprite;
        }
        
        if (data.projectileMaterial != null)
        {
            spriteRenderer.material = data.projectileMaterial;
        }
        
        if (data.abilityAnimation != null && animator != null)
        {
            animator.runtimeAnimatorController = data.abilityAnimation;
        }
        
        // Apply combat stats
        currentDamage = data.attributes.GetAttribute(Attribute.damage);
        currentSpeed = data.attributes.GetAttribute(Attribute.bulletSpeed);
        currentDuration = data.attributes.GetAttribute(Attribute.duration);
        currentPierce = data.attributes.GetAttribute(Attribute.pierceCount);
        currentKnockback = data.attributes.GetAttribute(Attribute.knockback);
        
        // Debug logging for duration
        Debug.Log($"SharedProjectile [{projectileId}]: Configured with duration {currentDuration} from data, isConfigured: {isConfigured}");
        
        // Debug logging for configuration process
        Debug.Log($"SharedProjectile [{projectileId}]: About to start lifetime coroutine with duration {currentDuration}");
        
        // Apply behavior settings
        isHoming = data.homing;
        homingRange = data.homingRange;
        isBouncy = data.bouncy;
        maxBounces = data.maxBounces;
        currentBounces = 0;
        
        // Apply audio
        if (data.spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(data.spawnSound, transform.position);
        }
        
        // Apply spawn effects
        if (data.spawnEffect != null)
        {
            Instantiate(data.spawnEffect, transform.position, transform.rotation);
        }
        
        // Set initial velocity
        if (rb != null)
        {
            // CRITICAL FIX: Ensure Rigidbody2D is properly configured for rapid spawning
            rb.velocity = Vector2.zero; // Reset velocity first
            rb.angularVelocity = 0f; // Reset angular velocity
            rb.velocity = direction * currentSpeed; // Set new velocity
            
            // Ensure the Rigidbody2D is awake and active
            if (!rb.IsAwake() )
            {
                Debug.LogWarning($"SharedProjectile [{projectileId}]: Rigidbody2D was inactive, re-enabling it");
                rb.WakeUp();
            }
        }
        
        // Apply custom behavior if specified
        if (data.customBehaviorType != null)
        {
            var customBehavior = gameObject.AddComponent(data.customBehaviorType);
            if (customBehavior != null)
            {
                Debug.Log($"Added custom behavior: {data.customBehaviorType.Name}");
            }
        }
        
        // Start the lifetime coroutine after duration is configured
        // Ensure any existing coroutine is stopped first
        if (duration != null)
        {
            StopCoroutine(duration);
            duration = null;
        }
        
        // Only start the coroutine if duration is valid and GameObject is active
        if (currentDuration > 0)
        {
            // Ensure GameObject is active before starting coroutine
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogError($"SharedProjectile [{projectileId}]: Cannot start coroutine on inactive GameObject! Activating first.");
                gameObject.SetActive(true);
            }
            
                    // Set configured flag BEFORE starting coroutine to prevent race condition
        isConfigured = true;
        
        // Final safety check: ensure collider is enabled after configuration
        if (projectileCollider != null && !projectileCollider.enabled)
        {
            projectileCollider.enabled = true;
            Debug.LogWarning($"SharedProjectile [{projectileId}]: Collider was disabled during configuration - re-enabled it");
        }
        
        duration = StartCoroutine(LifetimeCoroutine());
        Debug.Log($"SharedProjectile [{projectileId}]: Successfully started lifetime coroutine. isConfigured: {isConfigured}, duration: {currentDuration}, collider enabled: {projectileCollider?.enabled}");
        }
        else
        {
            Debug.LogWarning($"SharedProjectile: Invalid duration ({currentDuration}), not starting lifetime coroutine");
        }
    }
    
    private void UpdateHoming()
    {
        if (player == null) return;
        
        // Find nearest enemy
        var nearestEnemy = FindNearestEnemy();
        if (nearestEnemy != null)
        {
            Vector2 directionToEnemy = (nearestEnemy.position - transform.position).normalized;
            Vector2 currentVelocity = rb.velocity.normalized;
            
            // Gradually turn towards enemy
            Vector2 newDirection = Vector2.Lerp(currentVelocity, directionToEnemy, Time.deltaTime * 2f);
            rb.velocity = newDirection * currentSpeed;
        }
    }
    
    private Transform FindNearestEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, homingRange);
        Transform nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var collider in colliders)
        {
            if (collider.gameObject.layer == 7 || collider.gameObject.layer == 9) // Enemy layers
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = collider.transform;
                }
            }
        }
        
        return nearest;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore collisions if projectile is being destroyed
        if (isDestroying || !isConfigured)
        {
            if (isDestroying)
            {
                Debug.Log($"SharedProjectile [{projectileId}]: Ignoring collision - projectile is being destroyed");
            }
            else if (!isConfigured)
            {
                Debug.LogWarning($"SharedProjectile [{projectileId}]: Ignoring collision - projectile is not configured yet (isConfigured: {isConfigured})");
            }
            return;
        }
        
/*        // CRITICAL FIX: Add spawn protection to prevent immediate destruction
        float timeSinceSpawn = Time.time - spawnTime;
        float spawnProtectionTime = 0.1f; // Base spawn protection time
        
        // Increase spawn protection for very fast projectiles to account for physics timestep
        if (currentSpeed > 20f)
        {
            spawnProtectionTime = 0.15f; // 150ms for fast projectiles
        }
        
        if (timeSinceSpawn < spawnProtectionTime)
        {
            Debug.LogWarning($"SharedProjectile [{projectileId}]: Ignoring collision during spawn protection (spawned {timeSinceSpawn:F3}s ago, protection: {spawnProtectionTime:F3}s) with {other.name} on layer {other.gameObject.layer}");
            return;
        }
        */
        // CRITICAL FIX: Ignore collisions with other projectiles to prevent chain destruction
        if (other.GetComponent<SharedProjectile>() != null)
        {
            Debug.Log($"SharedProjectile [{projectileId}]: Ignoring collision with other projectile [{other.GetComponent<SharedProjectile>().GetInstanceID()}]");
            return;
        }
        
        if (other.gameObject.layer == 7 || other.gameObject.layer == 9) // Enemy layers
        {
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Only deal damage if we still have pierce remaining
                if (currentPierce > 0)
                {
                    // Deal damage
                    enemy.TakeDamage((int)currentDamage, transform, GetInstanceID());
                    
                    // Apply knockback
                    if (player != null)
                    {
                        enemy.GetKnockbacked(player, currentKnockback);
                    }
                    
                    // Play hit sound
                    if (projectileData?.hitSound != null)
                    {
                        AudioSource.PlayClipAtPoint(projectileData.hitSound, transform.position);
                    }
                    
                    // Spawn hit effect
                    if (projectileData?.hitEffect != null)
                    {
                        Instantiate(projectileData.hitEffect, transform.position, transform.rotation);
                    }
                    
                    // Handle piercing
                    currentPierce--;
                    if (currentPierce <= 0)
                    {
                        // Don't destroy immediately - let the projectile continue until it's naturally destroyed
                        // This allows it to hit any remaining enemies in the same area
                        Debug.Log($"SharedProjectile: Pierce count reached 0, projectile will continue until duration expires");
                        OnPierceExhausted();
                    }
                }
                else
                {
                    // Pierce exhausted - projectile passes through without dealing damage
                    Debug.Log($"SharedProjectile: Pierce exhausted, projectile passing through enemy without damage");
                }
            }
        }
        else if (other.gameObject.layer == 6) // Wall layer
        {
            if (isBouncy && currentBounces < maxBounces)
            {
                HandleBounce(other);
            }
            else
            {
                DestroyProjectile();
            }
        }
    }
    
    private void HandleBounce(Collider2D wall)
    {
        currentBounces++;
        
        // Calculate bounce direction
        Vector2 normal = (transform.position - wall.transform.position).normalized;
        Vector2 reflection = Vector2.Reflect(rb.velocity, normal);
        
        // Apply bounce with some energy loss
        rb.velocity = reflection * (currentSpeed * 0.8f);
        
        // Update direction for homing
        direction = rb.velocity.normalized;
    }
    
    private void OnPierceExhausted()
    {
        // Visual feedback when pierce is exhausted
        if (spriteRenderer != null)
        {
            // Make the projectile semi-transparent to show it's no longer effective
            Color color = spriteRenderer.color;
            color.a = 0.5f;
            spriteRenderer.color = color;
        }
        
        Debug.Log($"SharedProjectile: Pierce exhausted - projectile is now semi-transparent");
    }
    
    private void ResetVisualAppearance()
    {
        // Reset visual appearance when projectile is reactivated from pool
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f; // Full opacity
            spriteRenderer.color = color;
        }
    }
    
    private IEnumerator LifetimeCoroutine()
    {
        // Store the duration locally to prevent race conditions
        float localDuration = currentDuration;
        
        // Safety check - only run if duration is valid
        if (localDuration <= 0)
        {
            Debug.LogWarning($"SharedProjectile [{projectileId}]: Lifetime coroutine called with invalid duration: {localDuration}");
            yield break;
        }
        
        Debug.Log($"SharedProjectile [{projectileId}]: Lifetime coroutine started with duration {localDuration}");
        
        // Wait for the duration, but check if GameObject is still active
        float elapsed = 0f;
        while (elapsed < localDuration)
        {
            // Check if GameObject is still active
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"SharedProjectile [{projectileId}]: GameObject deactivated during lifetime coroutine - stopping");
                yield break;
            }
            
            yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
            elapsed += 0.1f;
        }
        
        Debug.Log($"SharedProjectile [{projectileId}]: Duration {localDuration} ended, returning to pool");
        DestroyProjectile();
    }
    
    private void DestroyProjectile()
    {
        // Prevent multiple calls to DestroyProjectile
        if (!isConfigured || isDestroying)
        {
            if (isDestroying)
            {
                Debug.Log($"SharedProjectile [{projectileId}]: DestroyProjectile called while already destroying - ignoring");
            }
            return;
        }
        
        // CRITICAL FIX: Prevent premature destruction of projectiles
        float timeSinceSpawn = Time.time - spawnTime;
        if (timeSinceSpawn < 0.05f) // 50ms minimum lifetime
        {
            Debug.LogWarning($"SharedProjectile [{projectileId}]: Prevented premature destruction (spawned {timeSinceSpawn:F3}s ago) - extending lifetime");
            // Extend the lifetime by restarting the coroutine
            if (duration != null)
            {
                StopCoroutine(duration);
            }
            duration = StartCoroutine(LifetimeCoroutine());
            return;
        }
        
        Debug.Log($"SharedProjectile [{projectileId}]: DestroyProjectile called - setting destroying flag (lifetime: {timeSinceSpawn:F3}s)");
        
        // Set destroying flag immediately to prevent multiple calls
        isDestroying = true;
        
        // Disable collider immediately to prevent more collisions
        if (projectileCollider != null)
        {
            projectileCollider.enabled = false;
            Debug.Log($"SharedProjectile [{projectileId}]: Disabled collider during destruction");
        }
        
        // Stop the lifetime coroutine before destroying
        if (duration != null)
        {
            StopCoroutine(duration);
            duration = null;
        }
        
        // Mark as not configured to prevent multiple calls
        isConfigured = false;
        
        // Invoke the event before deactivating
        OnProjectileDestroyed?.Invoke(this);
        
        // Return to pool BEFORE deactivating to avoid OnDisable interference
        GlobalProjectileManager.ReturnProjectile(this);
        
        // Ensure the projectile is properly deactivated after returning to pool
        Debug.Log($"SharedProjectile [{projectileId}]: Deactivating projectile after returning to pool");
        gameObject.SetActive(false);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (isHoming)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, homingRange);
        }
    }
}
