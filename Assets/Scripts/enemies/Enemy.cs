using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject target;
    public Rigidbody2D rb2d;
    private ParticleSystem particles;
    public Attributes attributes;
    public float speed_animation_multiplier = 1;
    private Dictionary<Attribute, float> current_attributes;
    
    

    /** Start is called before the first frame update */
    public void Start()
    {  
        current_attributes = new Dictionary<Attribute, float>(attributes.default_attributes);
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
        if(current_attributes.TryGetValue(Attribute.moveSpeed, out float speed)){
            current_attributes[Attribute.moveSpeed] = speed;
        }
        if (target == null)
        {
            return;
        }

        float distance = speed * speed_animation_multiplier * Time.deltaTime;
        Vector3 target_position = target.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, target_position, distance);
        rb2d.velocity = Vector2.zero;
    }

    public bool Damage(int damage)
    {
        if(current_attributes.TryGetValue(Attribute.health, out float health)){
            current_attributes[Attribute.health] = health - damage;
            particles.Emit(damage);
        }

        if (health <= 0)
        {
            particles.Emit((int)health);
            StartCoroutine(Kill());
        }

        return true;
    }

    public bool Knockback(int knockback)
    {
        //this is getting called but not working
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

   /*  bool TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Kill();
        }

        return true;
    } */
}
