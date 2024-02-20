using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeHit : AbilityObject
{
    internal Action<Enemy> onEnemyHit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 7 || other.gameObject.layer == 9) // Enemy Layer OR Flying Enemy Layer
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            onEnemyHit?.Invoke(enemy);
        }
    } 

    private void OnEnable()
    {
        duration = .25f;
       // damage = 10f;
    }
}
