using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : ActivatableAbility
{
    public float knockback = 1.5f;
    private float offset;
    public GameObject meleeObj;
    private Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
        meleeObj = (GameObject)Resources.Load("Prefabs/Abilities/Melee/Melee", typeof(GameObject));
        cooldownTimeMax = 2.5f;
    }
    
    public override void Activated()
    {
        
        ChangeDirection();
        Instantiate(meleeObj, GlobalReferences.firePoint.position + new Vector3 (offset * transform.localScale.x, 0, 0), Quaternion.identity);
        animator.SetTrigger("Attack");
    }

    private void ChangeDirection()
    {
        if (player.transform.localScale == Vector3.one)
        {
            offset = .5f * transform.localScale.x;
        }
        else
        {
            offset = -(.5f * transform.localScale.x);
        }
    }
}
