using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyXpObjectBehaviour : InteractableLoot
{
    private SoundComponent sound;
    private Rigidbody2D _rb2d;
    private CircleCollider2D _circleCollider;
    private ParticleSystem _particles;
    private SpriteRenderer _spriteRenderer;
    private GameObject player;
    private Coroutine _coroutine;
    private bool _isCollected = false;
    [SerializeField] public int experienceAmount = 1;
    [SerializeField] float speed = 5;
    [SerializeField] float knockbackForce = 3;
    [SerializeField] int particleOnDeathCount = 30;

    public void Start()
    {

        player = GlobalReferences.player;
    }
    private void FixedUpdate()
    {
        if(_isCollected)
        {
            MoveTowardsPlayer(player);
        }
        if(_isCollected && !_particles.IsAlive())
        {
            gameObject.SetActive(false);
        }
    }
    private void OnEnable()
    {
        sound = GetComponent<SoundComponent>();
        _rb2d = GetComponent<Rigidbody2D>();
        _particles = GetComponent<ParticleSystem>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _circleCollider = GetComponent<CircleCollider2D>();

        _isCollected = false;
        _spriteRenderer.enabled = true;
        _circleCollider.enabled = true;
        base.isReady = true;
    }
    private void OnDisable()
    {
        //Destroy(gameObject);
        ObjectPoolManager.ReturnObjectToPool(this.gameObject);
    }

    private new void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 8) // Experience Layer
        {
            if(!_isCollected)
            {
                GetKnockbacked(player.transform, knockbackForce);
                _particles.Play();
            }
            _isCollected = true;
        }
        base.OnTriggerEnter2D(other);
    }

    private void MoveTowardsPlayer(GameObject player)
    {
        _rb2d.transform.position = Vector3.MoveTowards( transform.position, player.transform.position, speed * Time.deltaTime);
    }
    public override void OnPickUp(GameObject playerObject)
    {
        _particles.Emit(particleOnDeathCount);
        sound.sfxToPlay.PlaySFX();
        GlobalReferences.xpManager.addExperience(experienceAmount);
        _spriteRenderer.enabled = false;
        _particles.Stop();
    }
    public bool GetKnockbacked(Transform Player, float knockbackForce)
    {
        Vector2 knockbackDirection = transform.position - Player.position;
        knockbackDirection = knockbackForce * _rb2d.mass * knockbackDirection.normalized;
        _rb2d.AddForce(knockbackDirection, ForceMode2D.Impulse);
        return true;
    }
}

