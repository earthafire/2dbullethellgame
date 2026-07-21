using System;
using System.Collections;
using System.Collections.Generic;
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

    // Replaces the old Collider2D-based trigger detection (see
    // EnemyXpObjectManager.Update() / TickProximity below). Magnet range is no longer
    // a field here - EnemyXpObjectManager owns a single cached value (refreshed via
    // Attributes.upgradeApplied, matching how the old "Experience Circle Collider"
    // trigger's radius used to get set directly from the pickUpRange stat whenever a
    // pickUpRange upgrade was applied) and passes it into TickProximity, instead of
    // every active orb independently reading the stat every tick. pickupRange (the
    // small "must actually touch" radius) has no attribute behind it in the original
    // game - it mirrored the player's separate damage hitbox collider, not an
    // upgradeable stat - so it stays a plain tunable field here.
    [SerializeField] float pickupRange = 0.15f;

    // Managed by EnemyXpObjectManager for O(1) swap-remove from its active list -
    // not meant to be touched from anywhere else.
    internal int ActiveIndex = -1;

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
        // No longer used for trigger detection (see TickProximity) - disabled so this
        // orb doesn't participate in Physics2D broad-phase at all. Hundreds of these
        // colliders clustering together after a big kill was the actual cause of a
        // severe frame spike (see Docs/NAVIGATION_MIGRATION.md).
        _circleCollider.enabled = false;
        base.isReady = true;

        EnemyXpObjectManager.Register(this);
    }
    private void OnDisable()
    {
        EnemyXpObjectManager.Unregister(this);
        //Destroy(gameObject);
        ObjectPoolManager.ReturnObjectToPool(this.gameObject);
    }

    // Called once per frame by EnemyXpObjectManager instead of per-orb Collider2D
    // triggers - see EnemyXpObjectManager.Update(). magnetRange comes from the
    // manager's single cached, upgrade-event-driven value (see
    // EnemyXpObjectManager.OnUpgradeApplied) rather than each orb reading the stat
    // itself every tick.
    public void TickProximity(GameObject playerObject, float magnetRange)   
    {
        float sqrDistance = (transform.position - playerObject.transform.position).sqrMagnitude;

        if (!_isCollected && sqrDistance <= magnetRange * magnetRange)
        {
            _isCollected = true;
            GetKnockbacked(playerObject.transform, knockbackForce);
            _particles.Play();
        }

        if (isReady && sqrDistance <= pickupRange * pickupRange)
        {
            isReady = false;
            OnPickUp(playerObject);
        }
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
