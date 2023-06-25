using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    // Event called when hitting an enemy
    public Action<Enemy> onEnemyHit;


    // Movement parameters
    public float duration = 3f;
    public float speed = 1f;
    public float direction = 0;


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

    private void OnTriggerEnter2D(Collider2D other)
    {
        try
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            // Calling an event that the ability can subscribe to
            onEnemyHit?.Invoke(enemy);
            Destroy(gameObject);
        }
        catch (System.NullReferenceException)
        {
            // Object is not an enemy
        }
    }
}
