using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricSpinInstance : ActivatableAbility
{
    public float duration = 5.0f;
    public float distance = 1.0f;
    public int damage = 10;
    private float time = 0.0f;
    public Action<Enemy> onEnemyHit;
    public float speedMultiplier = 100.0f;
    public float rotationSpeed = 105f; //in degrees per second
    GameObject player;
    Transform orbiter;

    private void Awake()
    {
        orbiter = this.transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            return;
        }

        duration -= Time.deltaTime;
        if (duration < 0)
        {
            // gameObject.SetActive(false);
        }

        // Rotate the 'orbiter' around the player
        orbiter.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        // Calculate the rotation in radians
        float angle = Mathf.Deg2Rad * orbiter.rotation.eulerAngles.z;

        // Calculate the new position for 'orbiter' based on the angle and distance
        Vector3 newPosition = new Vector3(
            player.transform.position.x + distance * Mathf.Cos(angle),
            player.transform.position.y + distance * Mathf.Sin(angle),
            orbiter.position.z
        );

        // Set the new position for 'orbiter'
        orbiter.position = newPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 7)
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            enemy.TakeDamage(damage);
            // this is just because it's not calling activated right now
            onEnemyHit?.Invoke(enemy);
        }
    }
    public override void Activated()
    {
        onEnemyHit += Hit;
    }
    private void Hit(Enemy enemy)
    {
        enemy.TakeDamage(damage);
    }
}
