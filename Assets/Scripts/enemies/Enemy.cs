using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{

    public GameObject player;
    public Rigidbody2D rb2d;
    private ParticleSystem particles;
    private float health, speed, damage;
    public Attributes attributes;
    public float knockbackDuration = .25f;
    public float speed_animation_multiplier = 1;
    // Define a UnityEvent that accepts a GameObject parameter
    [System.Serializable]
    public class GameObjectUnityEvent : UnityEvent<GameObject> { }
    public GameObjectUnityEvent OnEnemyDeath = new GameObjectUnityEvent();
    //suspends all actions
    public bool suspendActions = false;

    public void Start()
    {
        health = attributes.GetAttribute(Attribute.maxHealth);
        speed = attributes.GetAttribute(Attribute.moveSpeed);
        damage = attributes.GetAttribute(Attribute.damage);

        particles = gameObject.GetComponent<ParticleSystem>();
        player = GlobalReferences.player;
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.freezeRotation = true;
    }

    public void Update()
    {
        //enemy has strayed too far from the player
        if (player != null && Vector3.Distance(transform.position, player.transform.position) > 8)
        {
            GetDeath();
        }
    }

    public void Move()
    {
        if (player == null)
        {
            return;
        }

        float distance = speed * speed_animation_multiplier * Time.deltaTime;
        Vector3 target_position = player.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, target_position, distance);
        //rb2d.velocity = Vector2.zero;
    }

    public bool TakeDamage(int playerDamage)
    {
        if (Time.timeScale == 0)
        {
            return false;
        }

        health -= playerDamage;
        particles.Emit(playerDamage);

        if (health <= 0)
        {
            particles.Emit((int)health);
            StartCoroutine(GetDeath());
        }
        return true;
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 12)
        { // Player layer

            PlayerAttributes player = collision.gameObject.GetComponent<PlayerAttributes>();
            player.takeDamage((int)damage);
        }
        else
        {
            // Debug.Log(collision.gameObject + "has no attack");
        }
    }

    public bool GetKnockbacked(Transform damageSourcePos, float knockbackForce)
    {
        Vector2 difference = transform.position - damageSourcePos.position;
        difference = difference.normalized * knockbackForce * rb2d.mass;
        rb2d.AddForce(difference, ForceMode2D.Impulse);

        StartCoroutine(KnockbackRoutine());
        return true;
    }

    private IEnumerator KnockbackRoutine()
    {
        yield return new WaitForSeconds(knockbackDuration);
    }

    public virtual IEnumerator GetDeath()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<CircleCollider2D>().enabled = false;
        OnEnemyDeath.Invoke(this.gameObject);
        yield return new WaitForSeconds(.5f);
        Destroy(gameObject);
    }
}
