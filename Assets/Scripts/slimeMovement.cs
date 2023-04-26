using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slimeMovement : MonoBehaviour
{
    public int health = 100;
    public GameObject player;
    public float speed = 0.003f;

    Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {

        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed);
    }

    void takeDamage(int damage)
    {
        health -= damage;
        Debug.Log(gameObject);
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
