using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 100;
    public int attack = 1;
    public GameObject target;
    public Rigidbody2D rb2d;

    // Start is called before the first frame update
    public void Start()
    {
        target = GameObject.FindWithTag("Player");
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.freezeRotation = true;
    }

    public void Update()
    {
        if (Vector3.Distance(transform.position, target.transform.position) > 4)
        {
            Destroy(gameObject);
        }
    }

    public bool Damage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Kill();
        }
        return true;
    }

    void Kill()
    {
        Destroy(gameObject);
    }

    bool TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Kill();
        }

        return true;
    }
}
