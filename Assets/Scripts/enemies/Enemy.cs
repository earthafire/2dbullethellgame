using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 100;
    public int attack = 1;
    public GameObject target;
    public Rigidbody2D rb2d;
    private ParticleSystem particles;
    
    

    /** Start is called before the first frame update */
    public void Start()
    {   
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

    public bool Damage(int damage)
    {
        health -= damage;
        particles.Emit(damage);

        if (health <= 0)
        {
            particles.Emit(health);
            StartCoroutine(Kill());
        }
        return true;
    }

    public bool Knockback(int knockback)
    {
        Debug.Log("knockback");
        Vector2 direction = (transform.position - target.transform.position).normalized;
        rb2d.AddForce(direction * knockback, ForceMode2D.Impulse);
        return true;
    }

    public virtual IEnumerator Kill()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }

    bool TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Kill();
        }

        return true;
    }
}
