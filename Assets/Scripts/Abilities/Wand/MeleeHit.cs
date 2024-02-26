using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class MeleeHit : AbilityObject
{
    public override float duration { get; set; } = .25f;
    public override float damage { get; set; } = 20f;

    public override void OnHit(Enemy enemy)
    {
        enemy.TakeDamage((int)damage);
    }
}
