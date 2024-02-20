using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : ActivatableAbility
{
    public int damage = 20;
    public float knockback = 1.5f;

    public GameObject meleeObj;
    private Animator animator;
    public Action<Enemy> onEnemyHit;

    void Start()
    {
        animator = GetComponent<Animator>();
        meleeObj = (GameObject)Resources.Load("Prefabs/Abilities/Wands/Melee", typeof(GameObject));
        cooldownTimeMax = 1f;
    }

    public override void Activated()
    {
        animator.SetTrigger("Attack");
        MeleeHit meleeHit = Instantiate(meleeObj, GlobalReferences.firePoint.position, Quaternion.identity).GetComponent<MeleeHit>();
        meleeHit.Initialize(meleeHit);
        meleeHit.onEnemyHit += Hit;
    }
    
    public void Hit(Enemy enemy)
    {
        enemy.TakeDamage(damage);
    }
}
