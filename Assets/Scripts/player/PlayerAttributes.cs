using System;
using System.Collections;
using System.Collections.Generic;
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
    public Dictionary<Attribute, float> _localAttributes;
    public int _experienceUntilLevelUp = 100;
    private float damageModifier = 1;
    public ParticleSystem experienceParticles;
    public ParticleSystem damageParticles;
    [SerializeField]
    private GameObject uI;
    LevelUp levelUp;
    CircleCollider2D _pickUpRange;

    public Dictionary<Attribute, float> totalStats = new Dictionary<Attribute, float>();


    void Start()
    {
        wipeTotalStats();

        _pickUpRange = transform.GetChild(1).GetComponent<CircleCollider2D>();

        _playerAttributes.upgradeApplied += UpgradeApplied;

        levelUp = uI.GetComponent<LevelUp>();
        _localAttributes = new Dictionary<Attribute, float>(_playerAttributes.current_attributes);

        //XPparticles = gameObject.GetComponent<ParticleSystem>();
        healthbar.SetMaxHealth(_playerAttributes.GetAttribute(Attribute.health));

    }
    private void OnApplicationQuit() => _playerAttributes.ResetAppliedUpgrades();

    public void wipeTotalStats()
    {
        totalStats[Attribute.health] = 0f;
        totalStats[Attribute.moveSpeed] = 0f;
        totalStats[Attribute.damage] = 0f;
        totalStats[Attribute.acceleration] = 0f;
        totalStats[Attribute.experience] = 0f;
        totalStats[Attribute.level] = 0f;
        totalStats[Attribute.pickUpRange] = 0f;
    }

    public void takeDamage(int damage)
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        if (_localAttributes.TryGetValue(Attribute.health, out float health))
        {
            _localAttributes[Attribute.health] = health - damage;
            healthbar.SetHealth(health);
            damageParticles.Emit(damage);
        }

        if (health < 1)
        {
            Destroy(gameObject);
        }
    }

    public void addExperience(int experienceToAdd)
    {
        if (_localAttributes.TryGetValue(Attribute.experience, out float _exp))
        {
            _localAttributes[Attribute.experience] = _exp + (float)experienceToAdd;
        }

        if (_localAttributes[Attribute.experience] > _experienceUntilLevelUp)
        {
            DoLevelUp();
            _localAttributes[Attribute.experience] = 0;
        }

    }
    public void DoLevelUp()
    {
        experienceParticles.Emit(_experienceUntilLevelUp);

        _experienceUntilLevelUp = (int)((float)_experienceUntilLevelUp * 1.1f);

        if (_localAttributes.TryGetValue(Attribute.level, out float level))
        {

            _localAttributes[Attribute.level] = level + 1.0f;
            levelUp.HandleLevelUp();
        }
    }

    private void UpgradeApplied(Attributes attribute, UpgradeAttribute upgradeAttribute)
    {

        if (upgradeAttribute.upgradeToApply.TryGetValue(Attribute.health, out float value))
        {
            if (upgradeAttribute.isPercent)
            {
                _localAttributes[Attribute.health] *= (value / 100f) + 1f;
            }
            else
            {
                _localAttributes[Attribute.health] += value;
            }
            healthbar.SetMaxHealth(_playerAttributes.GetAttribute(Attribute.health));
            healthbar.SetHealth(_localAttributes[Attribute.health]);
        }
        // GetAttribute finds the Attribute inside of either _playerAttributes dictionary (Current or Default)
        _pickUpRange.radius = _playerAttributes.GetAttribute(Attribute.pickUpRange);

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
        foreach (KeyValuePair<Attribute, float> attributeFloatPair in _localAttributes)
        {
            totalStats[attributeFloatPair.Key] += attributeFloatPair.Value;
        }
    }

    [Button("Print Stats")]
    public void printStats()
    {
        updateTotalStats();
        String temp = "STAT\t\tVALUE\n";
        foreach (KeyValuePair<Attribute, float> attributeFloatPair in totalStats)
        {
            temp += String.Format("{0}\t\t{1}\n", attributeFloatPair.Key, attributeFloatPair.Value);
        }
        Debug.Log(temp);
    }
}

