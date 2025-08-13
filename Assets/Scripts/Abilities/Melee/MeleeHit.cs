public class MeleeHit : AbilityObject
{
    public override float duration { get; set; } = .25f;
    public override float damage { get; set; } = 20f;

    public override void OnHit(Enemy enemy)
    {
        enemy.TakeDamage((int)damage, transform, abilityId);
    }
}
