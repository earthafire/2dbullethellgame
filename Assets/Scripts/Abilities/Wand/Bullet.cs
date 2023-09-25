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

    public void Start()
    {
        sound = GetComponent<SoundComponent>();
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
        transform.Translate(Vector3.right * Time.deltaTime * speed);
    }

    public void ExplosionParticles()
    {
        particles.Emit(50);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        try
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            // Calling an event that the ability can subscribe to
            if (other.tag == "Enemy")
            {
                ExplosionParticles();
                sound.sfxToPlay.PlaySFX();
                onEnemyHit?.Invoke(enemy);
                CheckForExplosionCollision();
                Destroy(gameObject);
            }
        }
        catch (System.NullReferenceException)
        {
            // Object is not an enemy
        }
    }
    private void CheckForExplosionCollision()
    {
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, .8f);
        foreach (Collider c in colliders)
        {
            if (c.tag == "Enemy")
            {
                Enemy enemy = c.gameObject.GetComponent<Enemy>();
                onEnemyHit?.Invoke(enemy);
            }
        }
    }

}
