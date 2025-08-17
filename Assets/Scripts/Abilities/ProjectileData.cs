using Sirenix.OdinInspector;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[CreateAssetMenu(fileName = "New Projectile Data", menuName = "Abilities/Projectile Data", order = 1)]
public class ProjectileData : SerializedScriptableObject
{
    [TitleGroup("Basic Info")]
    [LabelWidth(120)]
    public string projectileName;
    
    [LabelWidth(120)]
    [TextArea(2, 4)]
    public string description;
    
    [TitleGroup("Visual & Animation")]
    [LabelWidth(120)]
    [PreviewField(75, ObjectFieldAlignment.Center)]
    public Sprite projectileSprite;
    
    [LabelWidth(120)]
    public AnimatorOverrideController abilityAnimation;
    
    [LabelWidth(120)]
    [InfoBox("Optional: Custom material for special effects")]
    public Material projectileMaterial;

    public Attributes attributes;


    /*    
        [TitleGroup("Combat Stats")]
        [LabelWidth(120)]
        [Range(0f, 1000f)]
        [SuffixLabel("damage")]
        public float damage = 20f;

        [LabelWidth(120)]
        [Range(0f, 10f)]
        [SuffixLabel("seconds")]
        public float duration = 2.0f;

        [LabelWidth(120)]
        [Range(0.1f, 50f)]
        [SuffixLabel("units/second")]
        public float speed = 10f;

        [LabelWidth(120)]
        [Range(0f, 10f)]
        [SuffixLabel("pierces")]
        public float pierce = 1f;

        [LabelWidth(120)]
        [Range(0f, 10f)]
        [SuffixLabel("knockback")]
        public float knockback = 1f;*/

    [TitleGroup("Behavior")]
    [LabelWidth(120)]
    [InfoBox("If true, projectile will home in on nearest enemy")]
    public bool homing = false;
    
    [LabelWidth(120)]
    [ShowIf("@this.homing")]
    [Range(0.1f, 10f)]
    [SuffixLabel("detection range")]
    public float homingRange = 2f;
    
    [LabelWidth(120)]
    [InfoBox("If true, projectile will bounce off walls")]
    public bool bouncy = false;
    
    [LabelWidth(120)]
    [ShowIf("@this.bouncy")]
    [Range(1, 10)]
    [SuffixLabel("bounces")]
    public int maxBounces = 3;
    
    [TitleGroup("Audio")]
    [LabelWidth(120)]
    public AudioClip spawnSound;
    
    [LabelWidth(120)]
    public AudioClip hitSound;
    
    [TitleGroup("Effects")]
    [LabelWidth(120)]
    public GameObject spawnEffect;
    
    [LabelWidth(120)]
    public GameObject hitEffect;
    
    [TitleGroup("Advanced")]
    [LabelWidth(120)]
    [InfoBox("Custom behavior script to attach at runtime")]
    public System.Type customBehaviorType;
    
    private void OnValidate()
    {
        // Auto-fill projectile name if empty
        if (string.IsNullOrEmpty(projectileName))
        {
            projectileName = name;
        }
    }
    
    [Button("Validate Projectile Data")]
    public void ValidateProjectileData()
    {
        if (string.IsNullOrEmpty(projectileName))
        {
            Debug.LogError($"[{name}] Projectile name is empty!");
            return;
        }
        
        if (attributes.GetAttribute(Attribute.damage) < 0)
        {
            Debug.LogError($"[{name}] Damage cannot be negative!");
            return;
        }
        
        if (attributes.GetAttribute(Attribute.duration) <= 0)
        {
            Debug.LogError($"[{name}] Duration must be greater than 0!");
            return;
        }
        
        if (attributes.GetAttribute(Attribute.bulletSpeed) <= 0)
        {
            Debug.LogError($"[{name}] Speed must be greater than 0!");
            return;
        }
        
        Debug.Log($"[{name}] Projectile data validation passed! ✓");
    }

}
