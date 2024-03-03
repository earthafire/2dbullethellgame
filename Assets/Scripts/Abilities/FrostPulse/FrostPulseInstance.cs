using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostPulseInstance : AbilityObject
{
    public override float knockback { get; set; } = .25f;
    public override float duration { get; set; } = 1f;
    public override float damage { get; set; } = 5f;

    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, 1);
    }

    public override void OnStay(Enemy enemy)
    {
        enemy.TakeDamage((int)damage);
        enemy.GetKnockbacked(player.transform, knockback);
    }
}
