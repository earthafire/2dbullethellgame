using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    public HealthBar healthbar;
    public Attributes attributes;
    private Dictionary<Attribute, float> current_attributes;
    public int current_experience = 0,
                experience_until_level_up = 100,
                player_level = 0;
    private float damageModifier = 1;
    private ParticleSystem particles;


    // Start is called before the first frame update
    void Start()
    {
        current_attributes = new Dictionary<Attribute, float>(attributes.default_attributes);
        particles = gameObject.GetComponent<ParticleSystem>();
        healthbar.SetMaxHealth(current_attributes[Attribute.health]);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        try
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if(enemy.attributes.current_attributes.TryGetValue(Attribute.damage, out float damage)){
                int int_damage = (int)damage;
                takeDamage(int_damage);
            }
            
        }
        catch (System.Exception)
        {
           Debug.Log(other.gameObject + "has no attack");
        }
    }

    public void takeDamage(int damage)
    {
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
            levelUp();
        }
        
    }
    public void levelUp(){
        
        particles.Emit(experience_until_level_up);
        experience_until_level_up = Convert.ToInt32(Convert.ToSingle(experience_until_level_up) * 1.1f);
        current_experience = 0;
        player_level = player_level + 1;
        
    }
}
