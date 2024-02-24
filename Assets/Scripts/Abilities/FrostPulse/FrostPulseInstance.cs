using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostPulseInstance : AbilityObject
{
    public Action<Enemy> onEnemyHit;
    SoundComponent sound;

    // Start is called before the first frame update
    void Start()
    {
        sound = GetComponent<SoundComponent>();
        sound.sfxToPlay.PlaySFX();
    }

    private void OnEnable()
    {
        duration = .75f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, 1);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        
        if (other.gameObject.layer == 7 || other.gameObject.layer == 9) // Enemy layer & Flying Enemy Layer
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();

            onEnemyHit?.Invoke(enemy);
        }
    
    }
}
