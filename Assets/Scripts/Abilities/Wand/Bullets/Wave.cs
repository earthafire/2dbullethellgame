using System.Collections.Generic;
using UnityEngine;

public class Wave : Bullet
{
    public override float damage { get; set; } = 5f;
    public override float knockback { get; set; } = .45f;
    public override float pierce { get; set; } = 100f;
    public override float speed { get => base.speed; set => base.speed = .75f; }
    public override float duration { get => base.duration; set => base.duration = 5f; }

    private void Awake()
    {
        radius = 0.35f;
    }

    protected override void QueryOverlappingEnemies(List<Collider2D> results)
    {
        QueryCircleResults(transform.position, radius, results);
    }

    public override void OnStay(Enemy enemy)
    {
        base.OnStay(enemy);
        enemy.GetKnockbacked(player.transform, knockback);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.7f, 0.2f, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = new Color(1f, 0.7f, 0.2f, 1f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
