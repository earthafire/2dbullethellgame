using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : ActivatableAbility
{
    public GameObject meleeObj;
    private GameObject player;
    public int damage = 50;
    public float knockback = 1.5f;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        meleeObj = (GameObject)Resources.Load("Prefabs/Weapons/Wands/Melee", typeof(GameObject));
        player = GameObject.Find("Player");
        base.cooldownTimeMax = .5f;
    }

    public override void Activated()
    {
        animator.SetTrigger("Attack");
        MeleeHit meleeHit = Instantiate(meleeObj, player.transform.position, Quaternion.identity).GetComponent<MeleeHit>();
        meleeHit.onEnemyHit += Hit;
    }
    
    public void Hit(Enemy enemy)
    {
        enemy.TakeDamage(damage);
    }
}
