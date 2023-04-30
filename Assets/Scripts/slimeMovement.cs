using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slimeMovement : MonoBehaviour
{
    public int health = 100;
    private GameObject player;
    public float speed = 0.003f;

    Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, player.transform.position) > 4)
        {
            Destroy(gameObject);
        }
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed);
    }

    void takeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "weapon")
        {
            takeDamage(10);
        }
    }
}
