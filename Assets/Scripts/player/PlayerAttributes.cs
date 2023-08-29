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
    public Attributes attributes;
    private Dictionary<Attribute, float> current_attributes;
    public int current_experience = 0, experience_until_level_up = 100, player_level = 0;
    private float damageModifier = 1;
    private ParticleSystem particles;
    [SerializeField] private GameObject ui;
    LevelUp levelUp;



    // Start is called before the first frame update
    void Start()
    {
        levelUp = ui.GetComponent<LevelUp>();
        current_attributes = new Dictionary<Attribute, float>(attributes.default_attributes);
        particles = gameObject.GetComponent<ParticleSystem>();
        healthbar.SetMaxHealth(current_attributes[Attribute.health]);
    }

    public void takeDamage(int damage)
    {
        if(Time.timeScale == 0){
            return;
        }

        if(current_attributes.TryGetValue(Attribute.health, out float health))
        {
            current_attributes[Attribute.health] = health - damage;
            health -= damage;
            healthbar.SetHealth(health);
        }

        if (health < 1)
        {
            Destroy(gameObject);
        }
    }

    public void addExperience(int bonus_experience)
    {
        current_experience += bonus_experience;

        if(current_experience > experience_until_level_up){
            DoLevelUp();
        }
        
    }
    public void DoLevelUp()
    {
        current_experience = 0;
        particles.Emit(experience_until_level_up);
        experience_until_level_up = Convert.ToInt32(Convert.ToSingle(experience_until_level_up) * 1.1f);
        
        player_level += player_level;

        levelUp.HandleLevelUp();
    }
}
