using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : AbilityObject
{

    public override float damage { get; set; } = 20f;
    public override float duration { get; set; } = 2.0f;
    public override float speed { get; set; } = 1f;

    public SoundComponent sound;

    public void Start()
    {
        if (sound)
        {
            sound.sfxToPlay.PlaySFX();
        }
        
    }
    public void Update()
    {
        Move();
    }
    public void Move()
    {
        transform.Translate(speed * Time.deltaTime * Vector3.right);
    }
    public override void OnHit(Enemy enemy)
    {
        enemy.TakeDamage((int)damage);
        sound.sfxToPlay.PlaySFX();
        PiercingCount();
    }
    public void PiercingCount()
    {
        pierce--;

        if (pierce <= 0)
        {
            Destroy(gameObject);
        }
    }
}