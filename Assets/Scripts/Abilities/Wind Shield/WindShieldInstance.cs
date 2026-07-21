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
        if (player != null)
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, 1);
    }

    protected override void QueryOverlappingEnemies(List<Collider2D> results)
    {
        QueryCircleResults(transform.position, radius, results);
    }

    private void Awake()
    {
        radius = 1.8f;
    }

    public override void OnStay(Enemy enemy)
    {
        if (enemy == null)
            return;

        enemy.TakeDamage((int)damage);
        enemy.GetKnockbacked(player.transform, knockback);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.7f, 0.2f, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = new Color(0.2f, 0.7f, 0.2f, 1f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
