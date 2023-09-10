using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulse : MonoBehaviour
{
    public float duration = .55f;
    public Action<Enemy> onEnemyHit;
    public GameObject player;

    private Animator m_Animator;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        m_Animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, 1);

        duration -= Time.deltaTime;
        if (duration < 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        try
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            // Calling an event that the ability can subscribe to
            if (other.tag == "Enemy")
            {
                onEnemyHit?.Invoke(enemy);
            }
        }
        catch (System.NullReferenceException)
        {
            // Object is not an enemy
        }
    }
}
