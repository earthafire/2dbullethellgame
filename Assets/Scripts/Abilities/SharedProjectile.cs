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
    
    // Events
    public System.Action<SharedProjectile> OnProjectileDestroyed;
    
    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
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
    
    private void Start()
    {
        spawnTime = Time.time;
        player = GlobalReferences.player?.transform;
        
        // Start the lifetime coroutine
        StartCoroutine(LifetimeCoroutine());
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
            Debug.LogError("SharedProjectile: Cannot configure with null ProjectileData!");
            return;
        }
        
        projectileData = data;
        direction = fireDirection.normalized;
        
        // Apply visual configuration
        if (data.projectileSprite != null)
        {
            spriteRenderer.sprite = data.projectileSprite;
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
        currentDamage = data.damage;
        currentSpeed = data.speed;
        currentDuration = data.duration;
        currentPierce = data.pierce;
        currentKnockback = data.knockback;
        
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
            rb.velocity = direction * currentSpeed;
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
        if (other.gameObject.layer == 7 || other.gameObject.layer == 9) // Enemy layers
        {
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
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
                    DestroyProjectile();
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
    
    private IEnumerator LifetimeCoroutine()
    {
        yield return new WaitForSeconds(currentDuration);
        DestroyProjectile();
    }
    
    private void DestroyProjectile()
    {
        OnProjectileDestroyed?.Invoke(this);
        Destroy(gameObject);
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
