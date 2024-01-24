using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostPulseInstance : MonoBehaviour
{
    [SerializeField] float duration = .55f;
    public Action<Enemy> onEnemyHit;
    GameObject player;
    SoundComponent sound;

    // Start is called before the first frame update
    void Start()
    {
        sound = GetComponent<SoundComponent>();
        player = GlobalReferences.player;
        sound.sfxToPlay.PlaySFX();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, 1);
        ReduceDuration();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        
        if (other.gameObject.layer == 7 || other.gameObject.layer == 9) // Enemy layer & Flying Enemy Layer
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();

            // Calling an event that the ability can subscribe to
            onEnemyHit?.Invoke(enemy);
        }
    }

    private void ReduceDuration()
    {
        if (duration >= 0)
        {
            duration -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
