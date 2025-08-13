using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability Database", menuName = "Abilities/Ability Database", order = 1)]
public class AbilityDatabase : SerializedScriptableObject
{
    [TitleGroup("Database Info")]
    [LabelWidth(120)]
    [ReadOnly]
    public int totalAbilities;
    
    [TitleGroup("All Abilities")]
    [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
    [ListDrawerSettings(ShowFoldout = false, ShowPaging = false, ShowItemCount = true)]
    public List<AbilityData> abilities = new List<AbilityData>();
    
    [TitleGroup("Quick Access")]
    [ShowInInspector]
    [ReadOnly]
    [TableList(ShowIndexLabels = true, AlwaysExpanded = false)]
    private Dictionary<ActivatableAbilityType, AbilityData> abilityLookup;
    
    [TitleGroup("Database Management")]
    [Button("Refresh Database")]
    public void RefreshDatabase()
    {
        totalAbilities = abilities.Count;
        BuildLookupTable();
        Debug.Log($"[AbilityDatabase] Refreshed database with {totalAbilities} abilities");
    }
    
    [Button("Validate All Abilities")]
    public void ValidateAllAbilities()
    {
        int validCount = 0;
        int invalidCount = 0;
        
        foreach (var ability in abilities)
        {
            if (ability == null) continue;
            
            if (ability.projectileData != null)
            {
                validCount++;
            }
            else
            {
                invalidCount++;
                Debug.LogError($"[AbilityDatabase] Invalid ability: {ability.name} - Projectile data is not assigned!");
            }
        }
        
        Debug.Log($"[AbilityDatabase] Validation complete: {validCount} valid, {invalidCount} invalid abilities");
    }
    
    [Button("Auto-Assign Component Types")]
    public void AutoAssignComponentTypes()
    {
        Debug.LogWarning("[AbilityDatabase] Auto-Assign Component Types is deprecated. Please manually assign ability prefabs in the AbilityData assets.");
    }
    
    private void BuildLookupTable()
    {
        abilityLookup = new Dictionary<ActivatableAbilityType, AbilityData>();
        
        foreach (var ability in abilities)
        {
            if (ability != null && !abilityLookup.ContainsKey(ability.abilityType))
            {
                abilityLookup[ability.abilityType] = ability;
            }
        }
    }
    
    public AbilityData GetAbilityData(ActivatableAbilityType abilityType)
    {
        if (abilityLookup == null)
        {
            BuildLookupTable();
        }
        
        if (abilityLookup.TryGetValue(abilityType, out var abilityData))
        {
            return abilityData;
        }
        
        Debug.LogWarning($"[AbilityDatabase] No ability data found for type: {abilityType}");
        return null;
    }
    
    public void OnEnable()
    {
        RefreshDatabase();
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        totalAbilities = abilities.Count;
        BuildLookupTable();
    }
}
