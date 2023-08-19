using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    public HealthBar healthbar;
    private float hp = 500;
    public int current_experience = 0,
                experience_until_level_up = 100,
                player_level = 0;
    private float damageModifier = 1;
    private ParticleSystem particles;


    // Start is called before the first frame update
    void Start()
    {
        particles = gameObject.GetComponent<ParticleSystem>();
        healthbar.SetMaxHealth(hp);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        
        try
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            takeDamage(enemy.attack);
        }
        catch (System.Exception)
        {
           Debug.Log(other.gameObject + "has no attack");
        }
    }

    public void takeDamage(int damage)
    {
        hp -= damage;
        healthbar.SetHealth(hp);
        if (hp < 1)
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
