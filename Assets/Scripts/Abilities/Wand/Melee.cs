using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : ActivatableAbility
{
    public GameObject meleeObj;
    private GameObject player;
    public int damage = 50;
    public float knockback = 1.5f;
    private Animator animator;
    private PlayerAttributes playerAttributes;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        meleeObj = (GameObject)Resources.Load("Prefabs/Weapons/Wands/Melee", typeof(GameObject));
        player = GameObject.Find("Player");
        playerAttributes = player.GetComponent<PlayerAttributes>();
        playerAttributes.attributes.upgradeApplied += UpgradeApplied;

        SetCooldown();
    }
    private void OnEnable()
    {
      //attributes._playerAttributes.upgradeApplied += UpgradeApplied;
    }

    private void OnDisable()
    {
        playerAttributes.attributes.upgradeApplied -= UpgradeApplied;
    }

    public override void Activated()
    {
        animator.SetTrigger("Attack");
        MeleeHit meleeHit = Instantiate(meleeObj, player.transform.position, Quaternion.identity).GetComponent<MeleeHit>();
        meleeHit.onEnemyHit += Hit;
        print (PlayerAttributes.stats[Attribute.cooldown]);
    }

    public void SetCooldown()
    {
        base.cooldownTimeMax = PlayerAttributes.stats[Attribute.cooldown];
    }
    private void UpgradeApplied(Attributes attribute, UpgradeAttribute upgradeAttribute)
    {
        SetCooldown();
    }
    public void Hit(Enemy enemy)
    {
        enemy.TakeDamage(damage);
    }
}
