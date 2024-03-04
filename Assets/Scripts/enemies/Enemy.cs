using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public Attributes attributes;
    public EnemyXpObjectData _xpData;
    public GameObject player;

    private float health, speed, damage;
    private Rigidbody2D _rb2d;
    private ParticleSystem particles;
    [SerializeField] private GameObject _lootBag;
    //[SerializeField] private int dropChance = 10;
    public float speed_animation_multiplier = 1;

    private Coroutine _tickRate;

    // Define a UnityEvent that accepts a GameObject parameter
    //[System.Serializable]
    public class GameObjectUnityEvent : UnityEvent<GameObject> { }
    public GameObjectUnityEvent OnEnemyDeath = new GameObjectUnityEvent();

    public bool suspendActions = false; //suspends all actions

    public void Awake()
    {
        particles = GetComponent<ParticleSystem>();
        _rb2d = GetComponent<Rigidbody2D>();
    }
    public void Start()
    {        
        player = GlobalReferences.player;
    }
    public void OnEnable()
    {
        health = attributes.GetAttribute(Attribute.maxHealth);
        speed = attributes.GetAttribute(Attribute.moveSpeed);
        damage = attributes.GetAttribute(Attribute.damage);
    }

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
    private IEnumerator Tick(float duration)
    {
        yield return new WaitForSeconds(duration);
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

        ObjectPoolManager.ReturnObjectToPool(this.gameObject);
    }
}
