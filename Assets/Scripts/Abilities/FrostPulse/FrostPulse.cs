using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostPulse : ActivatableAbility
{
    public GameObject frostpulseobj;
    [SerializeField] int damage = 1;
    [SerializeField] float knockback = .15f;
    GameObject player;

    void Start()
    {
        frostpulseobj = (GameObject)Resources.Load("Prefabs/Abilities/FrostPulse/FrostPulse", typeof(GameObject));

        player = GlobalReferences.player;

        cooldownTimeMax = 5f;
    }

    public override void Activated()
    {
        FrostPulseInstance pulse = Instantiate(frostpulseobj, player.transform.position, Quaternion.Euler(0f, 0f, 0f)).GetComponent<FrostPulseInstance>();
        pulse.onEnemyHit += frostHit;
    }

    public void frostHit(Enemy enemy)
    {
        enemy.TakeDamage(damage);
        enemy.GetKnockbacked(player.transform, knockback);
    }
}
