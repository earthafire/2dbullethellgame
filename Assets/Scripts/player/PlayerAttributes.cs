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

    public static Dictionary<Attribute, float> stats = new Dictionary<Attribute, float>() { };
    public UnityEvent OnPlayerDeath =  new UnityEvent();

    void Start()
    {
        updateTotalStats();

        _pickUpRange = transform.GetChild(1).GetComponent<CircleCollider2D>();

        _playerAttributes.upgradeApplied += UpgradeApplied;

        levelUp = uI.GetComponent<LevelUp>();

        //XPparticles = gameObject.GetComponent<ParticleSystem>();

        currentHealth = stats[Attribute.maxHealth];
        healthbar.SetMaxHealth(stats[Attribute.maxHealth]);
        healthbar.SetHealth(currentHealth);
    }
    private void OnApplicationQuit() => _playerAttributes.ResetAppliedUpgrades();

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
        damageParticles.Emit(damage);

        if (currentHealth < 1)
        {
            //trigger the OnPlayerDeath event for any scripts that might listen for it
            OnPlayerDeath.Invoke();
            //Make the player no longer controllable
            SuspendPlayerControl(this);
            //suspend enemies' actions
            SuspendSpawnedEnemyActions();
            //stop enemies spawning
            SetEnemiesSpawningSuspended(true);
            //maybe add death FX
            //Trigger the UI to show some options
        }
    }

   
    private void SuspendPlayerControl(PlayerAttributes playerAttributes)
    {
        //disable movement
        SetPlayerMovementAllowed(false, playerAttributes);
        //disable activatable ability components on player
        SetPlayerAbilitiesAllowed(false, playerAttributes);
    }
    private void SetPlayerMovementAllowed(bool shouldAllow, PlayerAttributes playerAttributes)
    {
        if (playerAttributes.TryGetComponent(out PlayerMovement playerMovement))
        {
            if (shouldAllow)
            {
                playerMovement.isPlayerInControl = true;
            }
            else
            {
                playerMovement.isPlayerInControl = false;
            }
        }
    }
    private void SetPlayerAbilitiesAllowed(bool shouldAllow, PlayerAttributes playerAttributes)
    {
        ActivatableAbility[] abilities = playerAttributes.GetComponents<ActivatableAbility>();
        foreach (var ability in abilities)
        {
            if (shouldAllow)
            {
                ability.enabled = true;
            }
            else
            {
                ability.enabled = false;
            }
        }
        if(TryGetComponent(out PlayerAbilityManager playerAbilityManager))
        {
            if (shouldAllow)
            {
                playerAbilityManager.suspendAbilities = false;
            }
            else
            {
                playerAbilityManager.suspendAbilities = true;
            }
            
        }
    }
    private static void SuspendSpawnedEnemyActions()
    {
        if (gameSupervisorController.instance != null)
        {
            if (gameSupervisorController.instance.spawnedEnemiesInScene != null)
            {
                foreach (var spawnedEnemy in gameSupervisorController.instance.spawnedEnemiesInScene)
                {
                    if (spawnedEnemy.TryGetComponent(out Enemy enemy))
                    {
                        enemy.suspendActions = true;
                    }
                }
            }
        }
    }

    private void SetEnemiesSpawningSuspended(bool shouldSuspend)
    {
        if (gameSupervisorController.instance != null)
        {
            if (shouldSuspend)
            {
                gameSupervisorController.instance.suspendSpawning = true;
            }
            else
            {
                gameSupervisorController.instance.suspendSpawning = false;
            }
        }
    }


    public void addExperience(int experienceToAdd)
    {

        stats[Attribute.experience] += (float)experienceToAdd;


        if (stats[Attribute.experience] > _experienceUntilLevelUp)
        {
            DoLevelUp();
            stats[Attribute.experience] = 0;
        }

    }
    public void DoLevelUp()
    {
        experienceParticles.Emit(_experienceUntilLevelUp);
        _experienceUntilLevelUp = (int)((float)_experienceUntilLevelUp * 1.1f);

        stats[Attribute.level] += 1.0f;
        levelUp.HandleLevelUp();
    }

    private void UpgradeApplied(Attributes attribute, UpgradeAttribute upgradeAttribute)
    {

        if (upgradeAttribute.upgradeToApply.TryGetValue(Attribute.maxHealth, out float value))
        {
            // Save old health stats
            float oldMaxHealth = healthbar.slider.maxValue;
            float newMaxHealth = _playerAttributes.GetAttribute(Attribute.maxHealth);

            // Calculate how much the health increased, increase max health by the same amount
            float healthDelta = newMaxHealth - oldMaxHealth;
            healthbar.SetMaxHealth(newMaxHealth);
            currentHealth += healthDelta;
            healthbar.SetHealth(currentHealth);
        }
        // GetAttribute finds the Attribute inside of either _playerAttributes dictionary (Current or Default)
        _pickUpRange.radius = stats[Attribute.pickUpRange];

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
                stats[attributeFloatPair.Key] += attributeFloatPair.Value;
            }
        }

        // add XP choices values to totalStats
        foreach (KeyValuePair<Attribute, float> attributeFloatPair in _playerAttributes.current_attributes)
        {
            if (stats.ContainsKey(attributeFloatPair.Key))
            {
                stats[attributeFloatPair.Key] += _playerAttributes.GetAttribute(attributeFloatPair.Key);
            }
            else
            {
                stats[attributeFloatPair.Key] = _playerAttributes.GetAttribute(attributeFloatPair.Key);
            }

        }
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

