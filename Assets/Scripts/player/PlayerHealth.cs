using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public HealthBar healthbar;
    private float hp = 500;


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

    private void takeDamage(int damage)
    {
        hp -= damage;
        healthbar.SetHealth(hp);
        if (hp < 1)
        {
            Destroy(gameObject);
        }
    }
}
