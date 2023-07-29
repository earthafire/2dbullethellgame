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

        /** Creates and Plays the Death Particles for all entities who have been hit recently */
        // GameObject instDeathParticles = Instantiate(hit_particles, this.transform);
        //instDeathParticles.transform.parent = null;
        particles.Emit(damage);

        if (health <= 0)
        {
            particles.Emit(10);
            Kill();
        }
        return true;
    }

    public virtual void Kill()
    {
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
