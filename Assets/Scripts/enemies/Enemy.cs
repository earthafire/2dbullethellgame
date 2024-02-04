using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public GameObject player;
    public Attributes attributes;
    private float health, speed, damage;
    private Rigidbody2D _rb2d;
    private ParticleSystem particles;
    public EnemyXpObjectData _xpData;
    [SerializeField] private GameObject _lootBag;
    //[SerializeField] private int dropChance = 10;
    public float speed_animation_multiplier = 1;

    // Define a UnityEvent that accepts a GameObject parameter
    //[System.Serializable]
    public class GameObjectUnityEvent : UnityEvent<GameObject> { }
    public GameObjectUnityEvent OnEnemyDeath = new GameObjectUnityEvent();

    public bool suspendActions = false; //suspends all actions


    public void Start()
    {
        health = attributes.GetAttribute(Attribute.maxHealth);
        speed = attributes.GetAttribute(Attribute.moveSpeed);
        damage = attributes.GetAttribute(Attribute.damage);

        player = GlobalReferences.player;

        particles = GetComponent<ParticleSystem>();
        _rb2d = GetComponent<Rigidbody2D>();    }

    public void Update()
    {
        //enemy has strayed too far from the player, kill it
        if (player != null && Vector3.Distance(transform.position, player.transform.position) > 8)
        {
            GetDeath();
        }
    }
    public void FixedUpdate()
    {
        Move();
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

    // abilities call this method to deal damage to enemies

    /// <param name="_ablityDamage"> base damage of ability </param>
    public bool TakeDamage(int _ablityDamage)  // returns true if damage was taken
    {
        if (Time.timeScale == 0){ return false; }

        float damageModifier = 1 + PlayerAttributes.stats[Attribute.damage] / 100; // adjust damage dealt by applying modifiers
        int modifiedPlayerDamage = (int)Math.Ceiling(_ablityDamage * damageModifier); // rounding up to nearest int
        health -= modifiedPlayerDamage;

        //Debug.Log("base damage: " + _ablityDamage + ", actual damage: " + modifiedPlayerDamage);

        particles.Emit(modifiedPlayerDamage);

        if (health <= 0)
        {
            StartCoroutine(GetDeath());
        }

        return true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 12) // Player layer
        { 
            PlayerAttributes player = collision.gameObject.GetComponent<PlayerAttributes>();
            player.takeDamage((int)damage);
        }
    }

    public bool GetKnockbacked(Transform knockbackFromPosition, float knockbackForce)
    {
        Vector2 knockbackDirection = transform.position - knockbackFromPosition.position;
        knockbackDirection = knockbackForce * _rb2d.mass * knockbackDirection.normalized;
        _rb2d.AddForce(knockbackDirection, ForceMode2D.Impulse);
        return true;
    }

    public virtual IEnumerator GetDeath()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<CircleCollider2D>().enabled = false;

        // Spawns XP at current position
        GlobalReferences.enemyXpObjectManager.SpawnXP(this.gameObject);

        // Spawns Loot Bag at current position
        if (_lootBag != null)
        {
            Instantiate(_lootBag, transform.position, Quaternion.identity);
        }

        // from Game Supervisor Controller
        OnEnemyDeath.Invoke(this.gameObject);

        yield return new WaitForSeconds(1.5f);

        Destroy(gameObject);

    }
}
