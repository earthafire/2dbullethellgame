using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostPulseInstance : AbilityObject
{
    public Action<Enemy> onEnemyHit;
    SoundComponent sound;
    [SerializeField] float knockback = .15f;

    public override float duration { get; set; } = 1f;
    public override float damage { get; set; } = 1f;

    // Start is called before the first frame update
    void Start()
    {
        sound = GetComponent<SoundComponent>();
        sound.sfxToPlay.PlaySFX();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, 1);
    }

    public override void OnStay(Enemy enemy)
    {
        enemy.TakeDamage((int)damage);
        enemy.GetKnockbacked(player.transform, knockback);
    }
}
