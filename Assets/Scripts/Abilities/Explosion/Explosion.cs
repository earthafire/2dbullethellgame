
public class Explosion : AbilityObject
{
    public override float damage { get; set; } = 10f;
    public override float duration { get; set; } = .3f;
    public override float knockback { get; set; } = 1f;
    public override void OnHit(Enemy enemy)
    {
        enemy.TakeDamage((int)damage);
        enemy.GetKnockbacked(transform, knockback);
    }
}
