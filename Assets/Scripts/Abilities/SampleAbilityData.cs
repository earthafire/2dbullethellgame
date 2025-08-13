using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// This is a sample script showing how to create ability data assets.
/// You can delete this file after creating your actual ability data assets.
/// </summary>
public class SampleAbilityData : MonoBehaviour
{
    [TitleGroup("How to Use")]
    [InfoBox("1. Right-click in Project window\n2. Create > Abilities > Ability Data\n3. Fill in the details\n4. Assign to your Ability Database")]
    [Button("Create Sample Ability Data")]
    public void CreateSampleAbilityData()
    {
        Debug.Log("To create ability data assets:\n" +
                  "1. Right-click in Project window\n" +
                  "2. Create > Abilities > Ability Data\n" +
                  "3. Fill in the details\n" +
                  "4. Assign to your Ability Database");
    }
    
    [TitleGroup("Example Ability Data")]
    [InfoBox("Here's what a typical ability data asset should look like:")]
    [ShowInInspector]
    [ReadOnly]
    private string exampleData = @"
Ability Data Asset Example:
├── Basic Info
│   ├── Icon: [Ability Sprite]
│   ├── Ability Name: 'Electric Spin'
│   └── Description: 'Creates a spinning electric field...'
├── Ability Type
│   ├── Ability Type: Electric_Spin
│   └── Component Type: ElectricSpinManager
├── Cooldown & Stats
│   ├── Base Cooldown: 5.0 seconds
│   └── Base Damage: 25.0
└── Visual & Audio
    ├── Ability Sprite: [Sprite]
    └── Activation Sound: [AudioClip]
";
}
