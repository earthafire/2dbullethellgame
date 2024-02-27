using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindShieldInstance : AbilityObject
{ 
    public override float knockback { get; set; } = .5f;
    public override float duration { get; set; } = 3f;
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
