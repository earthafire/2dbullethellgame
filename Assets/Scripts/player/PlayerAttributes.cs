using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    public HealthBar healthbar;
    private float hp = 500;
    [SerializeField]
    private int experience = 0;
    private float damageModifier = 1;


    // Start is called before the first frame update
    void Start()
    {
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
            // if enemy has no attack, do nothing
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
        experience += bonus_experience;
    }
}
