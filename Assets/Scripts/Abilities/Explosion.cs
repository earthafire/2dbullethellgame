using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Explosion : AbilityObject
{
    int damage = 10;

    private void Start()
    {
        duration = 1;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
            if (other.gameObject.layer == 7 || other.gameObject.layer == 9) // Enemy layer
            {
                Enemy enemy = other.gameObject.GetComponent<Enemy>();
                enemy.TakeDamage(damage); 
            }
    }
}
