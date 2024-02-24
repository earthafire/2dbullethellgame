using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : AbilityObject
{ 
    public SoundComponent sound;

    GameObject explosion;

    [SerializeField] bool piercingEnabled;
    [SerializeField] int pierceAmount = 10;

    float detectionRange = .5f;
    Collider2D[] detections;

    public Action<Enemy> onEnemyHit;

    private void OnEnable()
    {
        duration = 1.0f;
        speed = 1.0f;
    }
    public void Start()
    {
        detections = new Collider2D[32];

        sound.sfxToPlay.PlaySFX();

        explosion = (GameObject)Resources.Load("Prefabs/Abilities/Fireball/Explosion", typeof(GameObject));
    }
    public void Update()
    {
        Move();
    }
    public void Move()
    {
        transform.Translate(speed * Time.deltaTime * Vector3.right);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
       
          if (other.gameObject.layer == 7 || other.gameObject.layer == 9) // Enemy layer & Flying Enemy Layer
          {
                Enemy enemy = other.gameObject.GetComponent<Enemy>();

                sound.sfxToPlay.PlaySFX();
                
                onEnemyHit?.Invoke(enemy);// Calling an event that the ability can subscribe to

                if (piercingEnabled)
                {
                    if (DetectClosestEnemy() != null)
                    {
                        RotateTowards(DetectClosestEnemy().transform.position);
                        PiercingCount();
                    }
                }
                else
                {
                    Explode();
                    Destroy(gameObject);
                }
          }
    }

    private void PiercingCount()
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
            if (col.gameObject.layer == 7 || col.gameObject.layer == 9) // Enemy layer & Flying Enemy Layer
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
        Explosion _explosion = Instantiate(explosion, transform.position, Quaternion.Euler(0f, 0f, 0f)).GetComponent<Explosion>();
        _explosion.Initialize(_explosion);
    }
}
