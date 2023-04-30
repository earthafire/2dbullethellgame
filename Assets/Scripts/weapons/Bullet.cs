using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float duration = 3f;
    public float speed = 1f;
    public Rigidbody2D rb2d;
    public Weapon weapon;
    public float direction = 0;

    // Start is called before the first frame update
    public void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        // rb2d.rotation = direction;
        // GetComponentInChildren<SpriteRenderer>().enabled = true;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        try
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            weapon.DealDamage(enemy);
            Destroy(gameObject);
        }
        catch (System.NullReferenceException)
        {
            // Object is not an enemy
        }

    }
}
