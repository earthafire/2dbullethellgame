using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricSpinInstance : AbilityObject
{
    public float distance = 3.5f;
    Transform orbiter;

    public Action<Enemy> onEnemyHit;
    private void Awake()
    {
        orbiter = this.transform;
        player = GlobalReferences.player;
    }

    private void OnEnable()
    {
        duration = 2.0f;
        speed = 100f;
    }

    // Update is called once per frame
    void Update()
    {
        Orbit();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 7 || collision.gameObject.layer == 9) // Enemy Layer
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            onEnemyHit?.Invoke(enemy);
        }
    }

    private void Orbit()
    {
        // Rotate the 'orbiter' around the player
        orbiter.Rotate(Vector3.forward, speed * Time.deltaTime);

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
}
