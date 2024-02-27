using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : ActivatableAbility
{
    public float knockback = 1.5f;

    public GameObject meleeObj;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        meleeObj = (GameObject)Resources.Load("Prefabs/Abilities/Wands/Melee", typeof(GameObject));
        cooldownTimeMax = 1f;
    }

    public override void Activated()
    {
        animator.SetTrigger("Attack");
        Instantiate(meleeObj, GlobalReferences.firePoint.position, Quaternion.identity).GetComponent<MeleeHit>();

    }
}
