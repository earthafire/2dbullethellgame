using System.Collections.Generic;
using UnityEngine;

public class FrostPulseInstance : AbilityObject
{
    ParticleSystem _particles;

    public override float knockback { get; set; } = .5f;
    public override float duration { get; set; } = 1f;
    public override float damage { get; set; } = 10f;

    private void Awake()
    {
        _particles = GetComponent<ParticleSystem>();
        radius = 2.5f;
    }

    public override void OnEnable()
    {
        base.OnEnable();

        if (_particles == null)
            _particles = GetComponent<ParticleSystem>();

        var main = _particles.main;
        main.startLifetime = duration;
    }

    protected override void QueryOverlappingEnemies(List<Collider2D> results)
    {
        QueryCircleResults(transform.position, radius, results);
    }

    public override void OnStay(Enemy enemy)
    {
        if (enemy == null)
            return;

        Transform source = player != null ? player.transform : transform;
        enemy.TakeDamage((int)damage);
        enemy.GetKnockbacked(source, knockback);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = new Color(0.2f, 0.5f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
