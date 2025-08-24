using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class AbilityFactory : MonoBehaviour
{
    [TitleGroup("Factory Configuration")]
    [LabelWidth(150)]
    [Required("Please assign an Ability Database")]
    public AbilityDatabase abilityDatabase;
    
    [LabelWidth(150)]
    [InfoBox("If true, abilities will be created on-demand. If false, all abilities will be created at start.")]
    public bool createOnDemand = true;
    
    [TitleGroup("Runtime State")]
    [ShowInInspector]
    [ReadOnly]
    private Dictionary<ActivatableAbilityType, ActivatableAbility> instantiatedAbilities = new Dictionary<ActivatableAbilityType, ActivatableAbility>();
    
    [ShowInInspector]
    [ReadOnly]
    private Dictionary<ActivatableAbilityType, AbilityData> abilityDataCache = new Dictionary<ActivatableAbilityType, AbilityData>();
    
    [TitleGroup("Factory Management")]
    [Button("Initialize Factory")]
    public void InitializeFactory()
    {
        if (abilityDatabase == null)
        {
            Debug.LogError("[AbilityFactory] No ability database assigned!");
            return;
        }
        
        ClearAllAbilities();
        
        if (!createOnDemand)
        {
            CreateAllAbilities();
        }
        
        Debug.Log($"[AbilityFactory] Factory initialized. Create on demand: {createOnDemand}");
    }
    
    [Button("Create All Abilities")]
    public void CreateAllAbilities()
    {
        if (abilityDatabase == null) return;
        
        foreach (var abilityData in abilityDatabase.abilities)
        {
            if (abilityData != null)
            {
                CreateAbility(abilityData.abilityType);
            }
        }
        
        Debug.Log($"[AbilityFactory] Created {instantiatedAbilities.Count} abilities");
    }
    
    [Button("Clear All Abilities")]
    public void ClearAllAbilities()
    {
        foreach (var ability in instantiatedAbilities.Values)
        {
            if (ability != null)
            {
                DestroyImmediate(ability);
            }
        }
        
        instantiatedAbilities.Clear();
        abilityDataCache.Clear();
        
        Debug.Log("[AbilityFactory] Cleared all abilities");
    }
    
    public ActivatableAbility GetAbility(ActivatableAbilityType abilityType)
    {
        // Check if we already have this ability instantiated
        if (instantiatedAbilities.TryGetValue(abilityType, out var existingAbility))
        {
            if (existingAbility != null)
            {
                return existingAbility;
            }
            else
            {
                // Clean up null reference
                instantiatedAbilities.Remove(abilityType);
            }
        }
        
        // Create the ability if we don't have it
        if (createOnDemand)
        {
            return CreateAbility(abilityType);
        }
        
        Debug.LogWarning($"[AbilityFactory] Ability {abilityType} not found and create on demand is disabled");
        return null;
    }
    
    public ActivatableAbility CreateAbility(ActivatableAbilityType abilityType)
    {
        if (abilityDatabase == null)
        {
            Debug.LogError("[AbilityFactory] No ability database assigned!");
            return null;
        }
        
        // Get the ability data
        var abilityData = abilityDatabase.GetAbilityData(abilityType);
        if (abilityData == null)
        {
            Debug.LogError($"[AbilityFactory] No ability data found for type: {abilityType}");
            return null;
        }
        
        // Check if we already have this ability
        if (instantiatedAbilities.ContainsKey(abilityType))
        {
            Debug.LogWarning($"[AbilityFactory] Ability {abilityType} already exists!");
            return instantiatedAbilities[abilityType];
        }

        // Create the appropriate behavior type based on ability data
        var ability = CreateBehaviorForAbility(abilityData);
        if (ability != null)
        {
            instantiatedAbilities[abilityType] = ability;
            abilityDataCache[abilityType] = abilityData;

            Debug.Log($"[AbilityFactory] Created {abilityData.behaviorType} ability: {abilityType}");
            return ability;
        }

        Debug.LogError($"[AbilityFactory] Failed to create ability: {abilityType}");
        return null;
    }

    private ActivatableAbility CreateBehaviorForAbility(AbilityData abilityData)
    {
        switch (abilityData.behaviorType)
        {
            case AbilityBehaviorType.Projectile:
                var projectileAbility = gameObject.AddComponent<ProjectileAbilityBehavior>();
                projectileAbility.Initialize(abilityData);
                return projectileAbility;

            case AbilityBehaviorType.AreaEffect:
                var areaEffectAbility = gameObject.AddComponent<AreaEffectAbilityBehavior>();
                areaEffectAbility.Initialize(abilityData);
                return areaEffectAbility;

            case AbilityBehaviorType.Movement:
                var movementAbility = gameObject.AddComponent<MovementAbilityBehavior>();
                movementAbility.Initialize(abilityData);
                return movementAbility;

            case AbilityBehaviorType.Melee:
                var meleeAbility = gameObject.AddComponent<MeleeAbilityBehavior>();
                meleeAbility.Initialize(abilityData);
                return meleeAbility;

            case AbilityBehaviorType.Custom:
                Debug.LogWarning($"[AbilityFactory] Custom behavior type not implemented for {abilityData.abilityName}");
                return null;

            default:
                Debug.LogError($"[AbilityFactory] Unknown behavior type: {abilityData.behaviorType}");
                return null;
        }
    }

    public bool HasAbility(ActivatableAbilityType abilityType)
    {
        return instantiatedAbilities.ContainsKey(abilityType) && instantiatedAbilities[abilityType] != null;
    }
    
    public AbilityData GetAbilityData(ActivatableAbilityType abilityType)
    {
        if (abilityDataCache.TryGetValue(abilityType, out var abilityData))
        {
            return abilityData;
        }
        
        // Try to get from database and cache it
        if (abilityDatabase != null)
        {
            abilityData = abilityDatabase.GetAbilityData(abilityType);
            if (abilityData != null)
            {
                abilityDataCache[abilityType] = abilityData;
            }
        }
        
        return abilityData;
    }
    
    public List<ActivatableAbilityType> GetAvailableAbilities()
    {
        var available = new List<ActivatableAbilityType>();
        
        foreach (var kvp in instantiatedAbilities)
        {
            if (kvp.Value != null)
            {
                available.Add(kvp.Key);
            }
        }
        
        return available;
    }
    
    private void Start()
    {
        InitializeFactory();
    }
    
    private void OnDestroy()
    {
        ClearAllAbilities();
    }
}
