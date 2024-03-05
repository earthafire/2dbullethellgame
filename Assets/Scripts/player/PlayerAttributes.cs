using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class PlayerAttributes : MonoBehaviour
{
    public HealthBar healthbar;
    public Attributes attributes;
    public float currentHealth;
    public ParticleSystem damageReceivedParticles;
    [SerializeField]
    private GameObject uI;
    CircleCollider2D _pickUpRange;

    public static Dictionary<Attribute, float> stats = new() { };

    public UnityEvent OnPlayerDeath =  new UnityEvent();

    public void Awake()
    {
        GlobalReferences.player = this.gameObject;
    }
    void Start()
    {
        updateTotalStats();

        _pickUpRange = transform.GetChild(1).GetComponent<CircleCollider2D>();

        attributes.upgradeApplied += UpgradeApplied;

        currentHealth = stats[Attribute.maxHealth];
        healthbar.SetMaxHealth(stats[Attribute.maxHealth]);
        healthbar.SetHealth(currentHealth);
    }
    private void OnApplicationQuit() => attributes.ResetAppliedUpgrades();

    public void wipeTotalStats()
    {
        foreach (Attribute key in stats.Keys.ToArray())
        {
            stats[key] = 0;
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
        damageReceivedParticles.Emit(damage);

        if (currentHealth < 1)
        {
            //trigger the OnPlayerDeath event for any scripts that might listen for it
            OnPlayerDeath.Invoke();
            PlayerDeathHandler playerDeathHandler = new PlayerDeathHandler();
            playerDeathHandler.HandleDeath(this);
            //maybe add death FX
            //Trigger the UI to show some options
        }
    }

    private void UpgradeApplied(Attributes attribute, UpgradeAttribute upgradeAttribute)
    {

        if (upgradeAttribute.upgradeToApply.TryGetValue(Attribute.maxHealth, out float value))
        {
            UpdateHealthbar();
        }
        if (upgradeAttribute.upgradeToApply.ContainsKey(Attribute.pickUpRange))
        {
            _pickUpRange.radius = stats[Attribute.pickUpRange];
        }
        
        updateTotalStats();
    }

    public void UpdateHealthbar()
    {
        // Save old health stats
        float oldMaxHealth = healthbar.slider.maxValue;
        float newMaxHealth = stats[Attribute.maxHealth];

        // Calculate how much the health increased, increase max health by the same amount
        float healthDelta = newMaxHealth - oldMaxHealth;
        healthbar.SetMaxHealth(newMaxHealth);
        currentHealth += healthDelta;
        healthbar.SetHealth(currentHealth);
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
                stats[attributeFloatPair.Key] += attributeFloatPair.Value;
            }
        }

        // add XP choices values to totalStats
        foreach (KeyValuePair<Attribute, float> attributeFloatPair in attributes.current_attributes)
        {
            if (stats.ContainsKey(attributeFloatPair.Key))
            {
                stats[attributeFloatPair.Key] += attributes.GetAttribute(attributeFloatPair.Key);
            }
            else
            {
                stats[attributeFloatPair.Key] = attributes.GetAttribute(attributeFloatPair.Key);
            }
        }
        UpdateHealthbar();
    }

    [Button("Print Stats")]
    public void printStats()
    {
        String temp = "STAT\t\tVALUE\n";
        foreach (KeyValuePair<Attribute, float> attributeFloatPair in stats)
        {
            temp += String.Format("{0}\t\t{1}\n", attributeFloatPair.Key, attributeFloatPair.Value);
        }
        Debug.Log(temp);
    }
}

