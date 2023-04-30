using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 100;
    public GameObject target;

    Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindWithTag("Player");
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.freezeRotation = true;
    }

    // Update is called once per frame
    void FixedUpdate()
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
}
