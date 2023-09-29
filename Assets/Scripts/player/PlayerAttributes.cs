using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAttributes : MonoBehaviour
{
    public HealthBar healthbar;
    public Attributes _playerAttributes;
    public Dictionary<Attribute, float> _localAttributes;
    public int _experienceUntilLevelUp = 100 ;
    private float damageModifier = 1;
    private ParticleSystem particles;
    [SerializeField] private GameObject uI;
    LevelUp levelUp;
    CircleCollider2D _pickUpRange;

   
    void Start()
    {
        _pickUpRange = transform.GetChild(1).GetComponent<CircleCollider2D>();
        
        _playerAttributes.upgradeApplied += UpgradeApplied;

        levelUp = uI.GetComponent<LevelUp>();
        _localAttributes = new Dictionary<Attribute, float>(_playerAttributes.current_attributes);

        particles = gameObject.GetComponent<ParticleSystem>();
        healthbar.SetMaxHealth(_playerAttributes.GetAttribute(Attribute.health));
       
    }

    public void takeDamage(int damage)
    {
        if(Time.timeScale == 0){
            return;
        }

        if(_localAttributes.TryGetValue(Attribute.health, out float health))
        {
            _localAttributes[Attribute.health] = health - damage;
            healthbar.SetHealth(health);
        }

        if (health < 1)
        {
            Destroy(gameObject);
        }
    }

    public void addExperience(int experienceToAdd)
    {
        if(_localAttributes.TryGetValue(Attribute.experience, out float _exp))
        {
          _localAttributes[Attribute.experience] = _exp + (float)experienceToAdd;
        }

        if(_localAttributes[Attribute.experience] > _experienceUntilLevelUp)
        {
            DoLevelUp();
            _localAttributes[Attribute.experience] = 0;
        }
        
    }
    public void DoLevelUp()
    {
        particles.Emit(_experienceUntilLevelUp);

        _experienceUntilLevelUp = (int)((float)_experienceUntilLevelUp * 1.1f);

        if(_localAttributes.TryGetValue(Attribute.level, out float level)){

            _localAttributes[Attribute.level] = level + 1.0f;
            levelUp.HandleLevelUp();
        }
    }

    private void UpgradeApplied(Attributes attribute, UpgradeAttribute upgradeAttribute)
    {
        
        if(upgradeAttribute.upgradeToApply.TryGetValue(Attribute.health, out float value))
        {
            if(upgradeAttribute.isPercent)
            {
                _localAttributes[Attribute.health] *= (value / 100f) + 1f;
            }else{
                _localAttributes[Attribute.health] += value;
            }  
            healthbar.SetMaxHealth(_playerAttributes.GetAttribute(Attribute.health));
            healthbar.SetHealth(_localAttributes[Attribute.health]);
        }
       _pickUpRange.radius = _playerAttributes.GetAttribute(Attribute.pickUpRange);
    }
}
