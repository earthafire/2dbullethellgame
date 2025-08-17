using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Abilities/Ability Data", order = 0)]
public class AbilityData : SerializedScriptableObject
{
    [TitleGroup("Basic Info")]
    [LabelWidth(120)]
    [PreviewField(75, ObjectFieldAlignment.Center)]
    public Sprite icon;
    
    [LabelWidth(120)]
    public string abilityName;
    
    [LabelWidth(120)]
    [TextArea(3, 5)]
    public string description;
    
    [TitleGroup("Ability Type")]
    [LabelWidth(120)]
    public ActivatableAbilityType abilityType;
    
    [LabelWidth(120)]
    [InfoBox("The type of behavior this ability should use")]
    public AbilityBehaviorType behaviorType = AbilityBehaviorType.Projectile;
    
    [TitleGroup("Projectile Configuration")]
    [LabelWidth(120)]
    [InfoBox("Configuration data for this ability's projectiles")]
    [ShowIf("@this.behaviorType == AbilityBehaviorType.Projectile")]
    [Required("Please assign projectile data for projectile abilities")]
    public ProjectileData projectileData;
    
    [TitleGroup("Effect Configuration")]
    [LabelWidth(120)]
    [InfoBox("Prefab to spawn for area effect abilities")]
    [ShowIf("@this.behaviorType == AbilityBehaviorType.AreaEffect")]
    [Required("Please assign effect prefab for area effect abilities")]
    public GameObject effectPrefab;
    
    [LabelWidth(120)]
    [InfoBox("Shared projectile prefab (assigned globally)")]
    [ReadOnly]
    public GameObject sharedProjectilePrefab;
    
    [TitleGroup("Cooldown & Stats")]
    [LabelWidth(120)]
    [Range(0.1f, 300f)]
    [SuffixLabel("seconds")]
    public float baseCooldown = 1f;
    
    [LabelWidth(120)]
    [Range(0f, 100f)]
    [SuffixLabel("damage")]
    public float baseDamage = 10f;
    
    [TitleGroup("Visual & Audio")]
    [LabelWidth(120)]
    [PreviewField(75, ObjectFieldAlignment.Center)]
    public AnimatorOverrideController abilityAnimation;
    
    [LabelWidth(120)]
    public AudioClip activationSound;
    
    [TitleGroup("Advanced Settings")]
    [LabelWidth(120)]
    [ShowIf("@this.projectileData != null")]
    [Button("Validate Ability Data")]
    public void ValidateAbilityData()
    {
        bool isValid = true;
        
        // Check basic requirements
        if (string.IsNullOrEmpty(abilityName))
        {
            Debug.LogError($"[{name}] Ability name is empty!");
            isValid = false;
        }
        
        // Check behavior-specific requirements
        switch (behaviorType)
        {
            case AbilityBehaviorType.Projectile:
                if (projectileData == null)
                {
                    Debug.LogError($"[{name}] Projectile ability requires ProjectileData!");
                    isValid = false;
                }
                else
                {
                    projectileData.ValidateProjectileData();
                }
                break;
                
            case AbilityBehaviorType.AreaEffect:
                if (effectPrefab == null)
                {
                    Debug.LogError($"[{name}] Area effect ability requires EffectPrefab!");
                    isValid = false;
                }
                break;
                
            case AbilityBehaviorType.Movement:
                // Movement abilities don't need additional data
                break;
                
            case AbilityBehaviorType.Melee:
                // Melee abilities don't need additional data
                break;
                
            case AbilityBehaviorType.Custom:
                // Custom abilities might have their own validation
                break;
        }
        
        if (isValid)
        {
            Debug.Log($"[{abilityName}] Ability data validation passed! ✓");
        }
    }
    
    [Button("Create Projectile from Pool")]
    public SharedProjectile CreateProjectileFromPool(ProjectilePool pool, Vector3 position, Vector2 direction)
    {
        if (pool == null)
        {
            Debug.LogError($"[{abilityName}] Cannot create projectile: No pool assigned!");
            return null;
        }
        
        if (projectileData == null)
        {
            Debug.LogError($"[{abilityName}] Cannot create projectile: No projectile data assigned!");
            return null;
        }
        
        var projectile = pool.GetProjectile(projectileData, position, direction);
        if (projectile != null)
        {
            Debug.Log($"[{abilityName}] Created projectile from pool at {position}");
        }
        
        return projectile;
    }
    
    private void OnValidate()
    {
        // Auto-fill ability name if empty
        if (string.IsNullOrEmpty(abilityName))
        {
            abilityName = name;
        }
        
        // Auto-fill ability type if empty
        if (abilityType == ActivatableAbilityType.NULL)
        {
            abilityType = ActivatableAbilityType.NULL;
        }
        
/*        // Update base damage from projectile data if available
        if (projectileData != null)
        {
            baseDamage = projectileData.damage;
        }*/
    }
}
