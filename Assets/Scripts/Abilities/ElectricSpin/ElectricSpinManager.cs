using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricSpinManager : ActivatableAbility
{
    public GameObject electricSpinObj;

    private int damage = 10;
    private float knockback = 1.5f;

    void Start()
    {
        electricSpinObj = (GameObject)Resources.Load("Prefabs/Abilities/ElectricSpin/ElectricSpin", typeof(GameObject));

        cooldownTimeMax = 1f;
    }
    public override void Activated()
    {
        ElectricSpinInstance instance = Instantiate(electricSpinObj, player.transform.position, Quaternion.identity).GetComponent<ElectricSpinInstance>();
        instance.Initialize(instance);
        instance.onEnemyHit += Hit;
    }
    private void Hit(Enemy enemy)
    {
        enemy.TakeDamage(damage);
        enemy.GetKnockbacked(player.transform, knockback);
    }
}
