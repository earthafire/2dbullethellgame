public class Wave : Bullet
{
    public override float damage { get; set; } = 5f;
    public override float knockback { get; set; } = .1f;
    public override float pierce { get; set; } = 100f;

    public override void OnStay(Enemy enemy)
    {
        base.OnStay(enemy);
        enemy.GetKnockbacked(player.transform, knockback);
    }
}
