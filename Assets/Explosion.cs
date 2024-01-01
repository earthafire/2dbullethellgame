using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    int damage = 10;
    private void OnTriggerEnter2D(Collider2D other)
    {
        try
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();

            if (enemy.gameObject.layer == 7) // Enemy layer
            {
                enemy.TakeDamage(damage);
                Destroy(this.gameObject);   
            }
        }
        catch (System.NullReferenceException)
        {
            // Object is not an enemy
        }
    }
}
