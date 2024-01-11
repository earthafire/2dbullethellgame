using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyXpObjectBehaviour : InteractableLoot
{
    private SoundComponent sound;
    public int experienceAmount = 1, speed = 5;

    public void Start()
    {
        sound = GetComponent<SoundComponent>();

        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 1);
    }

    private void OnTriggerStay2D(Collider2D other)
    {       
        if(other.gameObject.layer == 8) // Experience Layer
        {
            transform.position = Vector3.MoveTowards(
            transform.position, other.gameObject.transform.position,
            speed * Time.deltaTime);
        }
    }

    public override void OnPickUp(GameObject playerObject)
    {
        sound.sfxToPlay.PlaySFX();
        GlobalReferences.xpManager.addExperience(experienceAmount);
        Destroy(gameObject);
    }
}

