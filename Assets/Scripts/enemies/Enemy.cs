using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject target;
    public Rigidbody2D rb2d;
    private ParticleSystem particles;
    private float health, speed, damage;
    public Attributes attributes;
    public float knockbackDuration = .25f;
    public float speed_animation_multiplier = 1;    

    public void Start()
    {  
        health = attributes.GetAttribute(Attribute.health);
        speed = attributes.GetAttribute(Attribute.moveSpeed);
        damage = attributes.GetAttribute(Attribute.damage);
        

        particles = gameObject.GetComponent<ParticleSystem>();
        target = GameObject.FindWithTag("Player");
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.freezeRotation = true;
    }

    public void Update()
    {
        if (target != null && Vector3.Distance(transform.position, target.transform.position) > 8)
        {
            Destroy(gameObject);
        }
    }

    public void Move()
    {
        if (target == null)
        {
            return;
        }

        
        float distance = speed * speed_animation_multiplier * Time.deltaTime;
        Vector3 target_position = target.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, target_position, distance);
        //rb2d.velocity = Vector2.zero;
    }

    public bool TakeDamage(int playerDamage)
    {
        if(Time.timeScale == 0){
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
        if(collision.gameObject.TryGetComponent<PlayerAttributes>(out PlayerAttributes player)){

            player = collision.gameObject.GetComponent<PlayerAttributes>();
            player.takeDamage((int)damage);
        }
        else 
        {
          // Debug.Log(collision.gameObject + "has no attack");
        }
    }

    public bool GetKnockbacked(Transform damageSource, float knockbackForce)
    {
        Vector2 difference = transform.position - damageSource.position;
        difference = difference.normalized * knockbackForce * rb2d.mass;
        rb2d.AddForce(difference, ForceMode2D.Impulse);

        StartCoroutine(KnockbackRoutine());        
        return true;
    }

    private IEnumerator KnockbackRoutine(){
        yield return new WaitForSeconds(knockbackDuration);
    }

    public virtual IEnumerator GetDeath()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
