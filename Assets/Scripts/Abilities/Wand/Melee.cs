using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : ActivatableAbility
{
    public GameObject meleeObj;
    public int damage = 20;
    public float knockback = 1.5f;
    private Animator animator;

    // Start is called before the first frame update
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
        meleeHit.onEnemyHit += Hit;
    }
    
    public void Hit(Enemy enemy)
    {
        enemy.TakeDamage(damage);
    }
}
