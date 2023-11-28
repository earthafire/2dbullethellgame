using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricSpinInstance : MonoBehaviour
{
    public float duration = 5.0f;
    public float distance = 1.0f;
    public Action<Enemy> onEnemyHit;
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
            Destroy(gameObject);
        }

        Orbit();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 7) // Enemy Layer
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            onEnemyHit?.Invoke(enemy);
        }
    }

    private void Orbit()
    {

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
}
