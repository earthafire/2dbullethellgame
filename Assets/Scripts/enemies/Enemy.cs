using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectDawn.Navigation.Hybrid;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb2d;
    private CircleCollider2D _circleCollider;
    private ParticleSystem particles;
    private ParticleColorRandomizer _particleColorRandomizer;
    private Animator _animator;
    private Vector3 _localScale;

    // Null if this prefab hasn't been migrated to ProjectDawn.Navigation yet
    // (Tools/Navigation/Add Agent Navigation To Selected Prefabs, see
    // Docs/NAVIGATION_MIGRATION.md) - Move() falls back to the old
    // Vector3.MoveTowards path in that case, so prefabs can be migrated
    // one at a time.
    protected AgentAuthoring _agent;
    private bool _wasSuspended;

    public Attributes attributes;
    public EnemyXpObjectData _xpData;

    public GameObject player;
    public float health, speed, damage;
    [SerializeField] private GameObject _lootBag;
    public float speed_animation_multiplier = 1;

    GameObject shadow;

    private Coroutine _tickRate;

    // Define a UnityEvent that accepts a GameObject parameter
    //[System.Serializable]
    public class GameObjectUnityEvent : UnityEvent<GameObject> { }
    public GameObjectUnityEvent OnEnemyDeath = new GameObjectUnityEvent();

    public bool suspendActions = false; //suspends all actions

    public void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb2d = GetComponent<Rigidbody2D>();
        _circleCollider = GetComponent<CircleCollider2D>();
        particles = GetComponentInChildren<ParticleSystem>();
        _particleColorRandomizer = GetComponentInChildren<ParticleColorRandomizer>();
        _animator = GetComponent<Animator>();
        _agent = GetComponent<AgentAuthoring>();

        _localScale = transform.localScale;
    }
    public void Start()
    {
        shadow = transform.GetChild(0).gameObject;
        player = GlobalReferences.player;

        if (_particleColorRandomizer != null)
        {
            _particleColorRandomizer.ApplyRandomColor();
        }

        // On a fresh Instantiate(), OnEnable() fires per-component in component list
        // order - Awake()/OnEnable() are NOT batched as "all Awakes then all
        // OnEnables" the way "all Awakes then all Starts" is guaranteed scene-wide.
        // Since Enemy predates the Agent components on these prefabs (appended later
        // by the setup tool), Enemy.OnEnable() runs before AgentAuthoring.Awake() has
        // even created LocalTransform on the entity yet, so RefreshAgentState() there
        // has to skip out (guarded on HasComponent<LocalTransform>) rather than crash.
        // Start() is scene-wide "after everything's Awake+OnEnable", so it's always
        // safe here - this is what actually completes the wiring for a fresh spawn.
        RefreshAgentState();
    }
    public void OnEnable()
    {
        _animator.speed = 1 + (float)GlobalReferences.GetRandomDouble()/2;

        _spriteRenderer.enabled = true;
        _circleCollider.enabled = true;
        if(shadow != null)
        {
            shadow.SetActive(true);
        }

        //public for animator access mostly for blue cube cringe lol
        health = attributes.GetAttribute(Attribute.maxHealth);
        speed = attributes.GetAttribute(Attribute.moveSpeed);
        damage = attributes.GetAttribute(Attribute.damage);

        _wasSuspended = false;

        // Safe here for pool reactivation (AgentAuthoring.Awake() already ran during
        // this object's first-ever activation and never runs again) but not for a
        // fresh spawn - see the comment in Start().
        RefreshAgentState();
    }

    // Syncs entity position/speed/destination-state to this (re)activation and wires
    // AgentCrowdPathingAuthoring.Group. Called from both OnEnable() and Start() (see
    // comments there for why both are needed); guarded so it's a harmless no-op if
    // the entity isn't fully set up yet.
    void RefreshAgentState()
    {
        if (_agent == null)
            return;

        var entity = _agent.GetOrCreateEntity();
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null)
            return;

        var manager = world.EntityManager;
        if (!manager.HasComponent<LocalTransform>(entity))
            return;

        // Pooled reactivation (ObjectPoolManager) sets transform.position to the
        // new spawn point before SetActive(true), but AgentAuthoring only ever
        // captures transform.position once, in its own Awake() - without this the
        // entity would keep its position from wherever this enemy last died and
        // visibly slide across the map to the new spawn point instead of
        // appearing there.
        var localTransform = manager.GetComponentData<LocalTransform>(entity);
        localTransform.Position = transform.position;
        manager.SetComponentData(entity, localTransform);

        // Clears stale velocity/destination left over from this enemy's previous
        // life in the pool.
        _agent.Stop();

        var locomotion = _agent.EntityLocomotion;
        locomotion.Speed = speed * speed_animation_multiplier;
        _agent.EntityLocomotion = locomotion;

        // Prefab assets can't hold a direct reference to the scene's crowd group,
        // so it's wired up here at runtime instead - see CrowdGroupRegistrar and
        // Docs/NAVIGATION_MIGRATION.md §4b.
        //
        // HasEntityPath guards against the same per-component OnEnable() ordering
        // gotcha as the LocalTransform guard above, but for a different reason:
        // unlike LocalTransform/AgentBody (added once in Awake(), never removed),
        // AgentCrowdPathingAuthoring's own OnDisable()/OnEnable() remove and re-add
        // the AgentCrowdPath component on every pool activation cycle. Since Enemy
        // precedes the Agent components in component order, this OnEnable()-triggered
        // call can run before AgentCrowdPathingAuthoring.OnEnable() has re-added it
        // this cycle - SetSharedComponent (unlike AddSharedComponent) throws if the
        // component isn't already present. Setting Group once (via Start(), which
        // always runs after everything on a fresh spawn) is enough regardless -
        // m_Group is a plain field on the component instance, not ECS data, so it
        // persists across disable/enable cycles and AgentCrowdPathingAuthoring.OnEnable()
        // correctly restores AgentCrowdPath from it on every later pool reactivation
        // with no help needed here.
        var pathing = GetComponent<AgentCrowdPathingAuthoring>();
        if (pathing != null && GlobalReferences.crowdGroup != null && pathing.HasEntityPath)
            pathing.Group = GlobalReferences.crowdGroup;
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
        // if player is to the right of the enemy
/*        if(player.transform.position.x > transform.position.x)
        {
            transform.localScale = _localScale;
        }
        else
        {
            transform.localScale = new Vector3(-_localScale.x, _localScale.y, _localScale.z);
        }*/
    }

    public void Move()
    {
        if (player == null)
        {
            return;
        }

        if (_agent != null)
        {
            if (suspendActions)
            {
                StopAgentOnceIfSuspended();
                return;
            }
            _wasSuspended = false;

            _agent.SetDestinationDeferred(player.transform.position);
            return;
        }

        // Fallback for enemy prefabs not yet migrated to ProjectDawn.Navigation -
        // see Docs/NAVIGATION_MIGRATION.md.
        float distance = speed * speed_animation_multiplier * Time.deltaTime;
        Vector3 target_position = player.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, target_position, distance);
    }

    // Subclasses (e.g. Slime) check suspendActions and skip calling Move() entirely
    // while suspended, rather than routing through it - call this instead in that
    // case so the DOTS agent still gets told to stop (otherwise AgentLocomotionSystem
    // just keeps driving it toward its last-set destination every frame regardless of
    // whether Move() ran). Stop() waits for agent jobs to finish, so this only calls
    // it once on the transition into suspended, not every frame while suspended.
    protected void StopAgentOnceIfSuspended()
    {
        if (_agent == null || _wasSuspended)
            return;
        _agent.Stop();
        _wasSuspended = true;
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

        Vector2 hitDirection = Vector2.zero;
        if (player != null)
        {
            hitDirection = (transform.position - player.transform.position).normalized;
        }

        if (GlobalReferences.enemyHitParticleManager != null)
        {
            GlobalReferences.enemyHitParticleManager.SpawnHitEffect(transform.position, hitDirection, modifiedPlayerDamage, _particleColorRandomizer?.colorChances);
        }
        else
        {
            RotateParticlesAwayFromPlayer();
            if (particles != null)
            {
                particles.Emit(modifiedPlayerDamage);
            }
        }

        if (_animator != null)
        {
            _animator.SetTrigger("GetHit");
        }

        if (health <= 0)
        {
            StartCoroutine(GetDeath());
        }
        return true;
    }

    // Points the hit-particles child away from the player so the burst
    // sprays outward instead of back into the thing that hit it.
    // No-ops for subclasses (e.g. Destructible) that don't have a hit-particle child.
    private void RotateParticlesAwayFromPlayer()
    {
        if (particles == null)
        {
            return;
        }
        Vector3 awayFromPlayer = transform.position - player.transform.position ;
        float rotationZ = Mathf.Atan2(awayFromPlayer.y, awayFromPlayer.x) * Mathf.Rad2Deg;
        particles.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
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
        Vector2 knockbackDirection = (transform.position - knockbackFromPosition.position).normalized;
        Vector2 knockbackVelocity = knockbackDirection * knockbackForce * _rb2d.mass;
        _rb2d.linearVelocity += knockbackVelocity;
        return true;
    }

    public virtual IEnumerator GetDeath()
    {
        _spriteRenderer.enabled = false;
       _circleCollider.enabled = false;
        if (shadow != null)
        {
            shadow.SetActive(false);
        }

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

        //Destroy(gameObject);

        ObjectPoolManager.ReturnObjectToPool(this.gameObject);
    }
}
