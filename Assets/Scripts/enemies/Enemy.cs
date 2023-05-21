using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 100;
    public int attack = 1;
    public GameObject target;
    public Rigidbody2D rb2d;
    public GameObject deathParticles;

    /** Start is called before the first frame update */
    public void Start()
    {
        
        target = GameObject.FindWithTag("Player");
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.freezeRotation = true;
    }

    public void Update()
    {
        if (target != null && Vector3.Distance(transform.position, target.transform.position) > 4)
        {
            Destroy(gameObject);
        }
    }

    public bool Damage(int damage)
    {
        health -= damage;

        /** Creates and Plays the Death Particles for all slimes who have been hit recently */
        GameObject instDeathParticles = Instantiate(deathParticles, this.transform);
        instDeathParticles.transform.parent = null;
        ParticleSystem ps = instDeathParticles.GetComponent<ParticleSystem>();
        ps.Play();
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.startSize = 0.20f;

        if (health <= 0)
        {
<<<<<<< HEAD
            ps.Emit(2);
=======
            ps.Emit(emitParams, 3);
>>>>>>> slime-movement-update
            Kill();
        }
        return true;
    }

    void Kill()
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
