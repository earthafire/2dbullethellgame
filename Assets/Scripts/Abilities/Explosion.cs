using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Explosion : AbilityObject
{
    private void OnEnable()
    {
        duration = .3f;
        damage = 10f;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 7 || other.gameObject.layer == 9) // Enemy layers
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            enemy.TakeDamage((int)damage); 
        }
    }
}
