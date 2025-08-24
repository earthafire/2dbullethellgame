using UnityEngine;

public class FrostPulseInstance : AbilityObject
{
    ParticleSystem _particles;

    public override float knockback { get; set; } = .5f;
    public override float duration { get; set; } = 1f;
    public override float damage { get; set; } = 10f;

    new public void OnEnable()
    {
        base.OnEnable();
        var main = _particles.main;
        main.startLifetime = duration;
    }

    private void Awake()
    {
        _particles = GetComponent<ParticleSystem>();
    }
    public override void OnStay(Enemy enemy)
    {
        enemy.TakeDamage((int)damage);
        enemy.GetKnockbacked(player.transform, knockback);
    }
}
