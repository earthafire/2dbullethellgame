using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] int damage = 10;
    [SerializeField] float duration = 1;

    private void FixedUpdate()
    {
        ReduceDuration();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
            if (other.gameObject.layer == 7) // Enemy layer
            {
                Enemy enemy = other.gameObject.GetComponent<Enemy>();
                enemy.TakeDamage(damage); 
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
