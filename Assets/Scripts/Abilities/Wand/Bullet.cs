using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    // Event called when hitting an enemy
    public Action<Enemy> onEnemyHit;

    public SoundComponent sound;

    // Movement parameters
    public float duration = 3f;
    public float speed = 1f;
    public float direction = 0;
    private ParticleSystem particles;
    private bool piercingEnabled = false;
    private int pierceAmount = 3;

    public void Start()
    {
        sound.sfxToPlay.PlaySFX();
        particles = gameObject.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    public void Update()
    {
        Move();
        ReduceDuration();
    }

    private void ReduceDuration()
    {
        // check if object should be destroyed
        if (duration >= 0)
        {
            duration -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Move()
    {
        transform.Translate(speed * Time.deltaTime * Vector3.right);
    }

    public void ExplosionParticles()
    {
        particles.Play();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        try
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            // Calling an event that the ability can subscribe to
            if (enemy.gameObject.layer == 7) // Enemy layer
            {
                ExplosionParticles();
                sound.sfxToPlay.PlaySFX();
                onEnemyHit?.Invoke(enemy);
                piercingCount();
                //Destroy(gameObject);
            }
        }
        catch (System.NullReferenceException)
        {
            // Object is not an enemy
        }
    }

    private void piercingCount()
    {
        pierceAmount--;
        if(pierceAmount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
