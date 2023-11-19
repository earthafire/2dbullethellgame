using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

public class PlayerAttributes : MonoBehaviour
{
    public HealthBar healthbar;
    public Attributes _playerAttributes;
    public int _experienceUntilLevelUp = 100;
    public float currentHealth;
    private float damageModifier = 1;
    public ParticleSystem experienceParticles;
    public ParticleSystem damageParticles;
    [SerializeField]
    private GameObject uI;
    LevelUp levelUp;
    CircleCollider2D _pickUpRange;

    public static Dictionary<Attribute, float> totalStats = new Dictionary<Attribute, float>() { };


    void Start()
    {
        updateTotalStats();

        _pickUpRange = transform.GetChild(1).GetComponent<CircleCollider2D>();

        _playerAttributes.upgradeApplied += UpgradeApplied;

        levelUp = uI.GetComponent<LevelUp>();

        //XPparticles = gameObject.GetComponent<ParticleSystem>();

        currentHealth = totalStats[Attribute.health];
        healthbar.SetMaxHealth(totalStats[Attribute.health]);
        healthbar.SetHealth(currentHealth);
    }
    private void OnApplicationQuit() => _playerAttributes.ResetAppliedUpgrades();

    public void wipeTotalStats()
    {
        foreach (Attribute key in totalStats.Keys.ToArray())
        {
            totalStats[key] = 0;
        }
    }

    public void takeDamage(int damage)
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        currentHealth -= damage;
        healthbar.SetHealth(currentHealth);
        damageParticles.Emit(damage);

        if (currentHealth < 1)
        {
            Destroy(gameObject);
        }
    }

    public void addExperience(int experienceToAdd)
    {

        totalStats[Attribute.experience] += (float)experienceToAdd;


        if (totalStats[Attribute.experience] > _experienceUntilLevelUp)
        {
            DoLevelUp();
            totalStats[Attribute.experience] = 0;
        }

    }
    public void DoLevelUp()
    {
        experienceParticles.Emit(_experienceUntilLevelUp);
        _experienceUntilLevelUp = (int)((float)_experienceUntilLevelUp * 1.1f);

        totalStats[Attribute.level] += 1.0f;
        levelUp.HandleLevelUp();
    }

    private void UpgradeApplied(Attributes attribute, UpgradeAttribute upgradeAttribute)
    {

        if (upgradeAttribute.upgradeToApply.TryGetValue(Attribute.health, out float value))
        {
            // Save old health stats
            float oldMaxHealth = healthbar.slider.maxValue;
            float newMaxHealth = _playerAttributes.GetAttribute(Attribute.health);

            // Calculate how much the health increased, increase max health by the same amount
            float healthDelta = newMaxHealth - oldMaxHealth;
            healthbar.SetMaxHealth(newMaxHealth);
            currentHealth += healthDelta;
            healthbar.SetHealth(currentHealth);
        }
        // GetAttribute finds the Attribute inside of either _playerAttributes dictionary (Current or Default)
        _pickUpRange.radius = totalStats[Attribute.pickUpRange];

        updateTotalStats();
    }


    public void updateTotalStats()
    {
        wipeTotalStats();

        PlayerInventory playerInventory = GetComponentInParent<PlayerInventory>();

        // iterate through all equipped items
        foreach (InventorySlot slot in playerInventory.equipment.GetSlots)
        {
            // add each key's (attribute's) associated value (float) to the totalStats table
            foreach (KeyValuePair<Attribute, float> attributeFloatPair in slot.item.Buffs)
            {
                totalStats[attributeFloatPair.Key] += attributeFloatPair.Value;
            }
        }

        // add XP choices values to totalStats
        foreach (KeyValuePair<Attribute, float> attributeFloatPair in _playerAttributes.current_attributes)
        {
            if (totalStats.ContainsKey(attributeFloatPair.Key))
            {
                totalStats[attributeFloatPair.Key] += _playerAttributes.GetAttribute(attributeFloatPair.Key);
            }
            else
            {
                totalStats[attributeFloatPair.Key] = _playerAttributes.GetAttribute(attributeFloatPair.Key);
            }

        }
    }

    [Button("Print Stats")]
    public void printStats()
    {
        String temp = "STAT\t\tVALUE\n";
        foreach (KeyValuePair<Attribute, float> attributeFloatPair in totalStats)
        {
            temp += String.Format("{0}\t\t{1}\n", attributeFloatPair.Key, attributeFloatPair.Value);
        }
        Debug.Log(temp);
    }
}

