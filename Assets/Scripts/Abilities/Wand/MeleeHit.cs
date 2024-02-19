using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeHit : AbilityObject
{
    //public float offsetDistance = .25f;
    public Action<Enemy> onEnemyHit;
    public GameObject player;
    //private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        duration = 1;
        player = GlobalReferences.player;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 7 || other.gameObject.layer == 9) // Enemy Layer OR Flying Enemy Layer
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            onEnemyHit?.Invoke(enemy);
        }
    }
}
