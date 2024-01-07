using Sirenix.OdinInspector;
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
    private float direction = 0;

    private GameObject explosion;
    private ParticleSystem particles;
    public bool piercingEnabled = true;
    private int pierceAmount = 10;

    float detectionRange = .5f;
    Collider2D[] detections;


    public void Start()
    {
        detections = new Collider2D[32];

        sound.sfxToPlay.PlaySFX();
        particles = gameObject.GetComponent<ParticleSystem>();
        explosion = (GameObject)Resources.Load("Prefabs/Weapons/Fireball/Explosion", typeof(GameObject));

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


    private void OnTriggerEnter2D(Collider2D other)
    {
        try
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            
            if (enemy.gameObject.layer == 7) // Enemy layer
            {
                DetectClosestEnemy();
                sound.sfxToPlay.PlaySFX();
                
                onEnemyHit?.Invoke(enemy);// Calling an event that the ability can subscribe to

                if (piercingEnabled)
                {
                    piercingCount();
                    RotateTowards(DetectClosestEnemy().transform.position);
                }
                else
                {
                    Explode();
                    Destroy(gameObject);
                }
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
    private Enemy DetectClosestEnemy()
    {
        float closestDistance = Mathf.Infinity;
        Enemy closestEnemy = null;

        detections = Physics2D.OverlapCircleAll(transform.position, detectionRange);

        foreach (var col in detections)
        {
            if (col.gameObject.layer == 7)
            {
                float distance = Vector2.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestEnemy = col.gameObject.GetComponent<Enemy>();
                    closestDistance = distance;
                }
            }
        }
        return closestEnemy;
    }
    private void RotateTowards(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        Vector3 rotatedVectorToTarget = Quaternion.Euler(0, 0, 90) * direction;
        Quaternion targetRotation = Quaternion.LookRotation(forward: Vector3.forward, upwards: rotatedVectorToTarget);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 45);
    }
    private void Explode()
    {
        Instantiate(explosion, transform.position, Quaternion.Euler(0f, 0f, 0f));
    }
}
