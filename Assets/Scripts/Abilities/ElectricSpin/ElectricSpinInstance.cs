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
    GameObject player;
    

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        if(duration < 0)
        {
           // gameObject.SetActive(false);
        }
        
        transform.RotateAround(player.transform.position, Vector3.forward, Time.deltaTime * speedMultiplier);
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
