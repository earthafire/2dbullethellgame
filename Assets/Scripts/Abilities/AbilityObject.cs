using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityObject : MonoBehaviour
{
    public virtual float duration { get; set; } = 1f;
    public virtual float speed { get; set; } = 1f;
    public virtual float damage { get; set; } = 1f;
    public virtual float pierce { get; set; } = 1f;
    public virtual float knockback { get; set; } = .1f;

    // How often (in seconds) OnStay can re-trigger for the same enemy - override
    // per-ability so e.g. Electric Spin can tick slower/faster than Frost Pulse.
    public virtual float tickRate { get; set; } = .1f;
    public float radius = 1f;
    public virtual float detectionRadius => Mathf.Max(0.1f, radius);

    public GameObject player;

    private Collider2D _collider;
    private ContactFilter2D _enemyFilter;
    private readonly Collider2D[] _queryBuffer = new Collider2D[32];
    private readonly List<Collider2D> _overlapResults = new List<Collider2D>(16);
    private readonly HashSet<Enemy> _currentEnemies = new HashSet<Enemy>();
    private readonly Dictionary<Enemy, float> _nextTickTime = new Dictionary<Enemy, float>();

    private const int EnemyLayerMask = (1 << 7) | (1 << 9);

    private void Awake()
    {
        player = GlobalReferences.player;
        _collider = GetComponent<Collider2D>();
        _enemyFilter = new ContactFilter2D();
        _enemyFilter.useLayerMask = true;
        _enemyFilter.layerMask = EnemyLayerMask;
        _enemyFilter.useTriggers = true;
    }

    public virtual void OnEnable()
    {
        if (player == null)
            player = GlobalReferences.player;

        if (_collider == null)
            _collider = GetComponent<Collider2D>();

        AbilityTickManager.CreateInstanceIfNeeded();
        AbilityTickManager.Instance.RegisterAbility(this);
        Initialize(this);
    }

    public virtual void OnDisable()
    {
        AbilityTickManager.Instance?.UnregisterAbility(this);
        _currentEnemies.Clear();
        _nextTickTime.Clear();
    }

    public void Tick()
    {
        _overlapResults.Clear();
        QueryOverlappingEnemies(_overlapResults);

        var stillOverlapping = new HashSet<Enemy>();
        for (int i = 0; i < _overlapResults.Count; i++)
        {
            if (_overlapResults[i].TryGetComponent(out Enemy enemy))
            {
                stillOverlapping.Add(enemy);

                if (!_currentEnemies.Contains(enemy))
                {
                    _currentEnemies.Add(enemy);
                    OnHit(enemy);
                }

                if (IsTickReady(enemy))
                {
                    _nextTickTime[enemy] = Time.time + tickRate;
                    OnStay(enemy);
                }
            }
        }

        if (_currentEnemies.Count > stillOverlapping.Count)
        {
            var enemiesToRemove = new List<Enemy>();
            foreach (var enemy in _currentEnemies)
            {
                if (!stillOverlapping.Contains(enemy))
                    enemiesToRemove.Add(enemy);
            }

            for (int i = 0; i < enemiesToRemove.Count; i++)
            {
                _currentEnemies.Remove(enemiesToRemove[i]);
                _nextTickTime.Remove(enemiesToRemove[i]);
            }
        }
    }

    protected virtual void QueryOverlappingEnemies(List<Collider2D> results)
    {
        if (_collider != null && _collider.enabled)
        {
            _collider.Overlap(_enemyFilter, results);
            return;
        }

        // If no collider is present, fall back to a simple circle query around
        // the object using the default detection radius.
        QueryCircleResults(transform.position, detectionRadius, results);
    }

    protected int QueryCircleResults(Vector2 position, float radius, List<Collider2D> results, int layerMask = EnemyLayerMask)
    {
        _enemyFilter.layerMask = layerMask;
        int count = Physics2D.OverlapCircle(position, radius, _enemyFilter, _queryBuffer);
        for (int i = 0; i < count; i++)
        {
            if (_queryBuffer[i] != null)
                results.Add(_queryBuffer[i]);
        }
        return count;
    }

    private bool IsTickReady(Enemy enemy)
    {
        return !_nextTickTime.TryGetValue(enemy, out float nextTick) || Time.time >= nextTick;
    }

    public virtual void OnHit(Enemy enemy)
    {
        // override this to customize on hit behaviour
    }

    public virtual void OnStay(Enemy enemy)
    {
        // override this to customize on hit behaviour
    }

    public IEnumerator CountDuration(float _duration)
    {
        yield return new WaitForSeconds(_duration);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.25f, 0.2f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = new Color(1f, 0.3f, 0.25f, 1f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void Initialize(AbilityObject _obj)
    {
        _obj.duration = CalculateModifiedDuration(_obj.duration);
        _obj.speed = CalculateModifiedBulletSpeed(_obj.speed);
        _obj.transform.localScale = CalculateModifiedSize(_obj.transform.localScale);
        StartCoroutine(CountDuration(_obj.duration));
    }

    public static float CalculateModifiedDuration(float baseValue)
    {
        float durationFactor = PlayerAttributes.stats[Attribute.duration];
        float modifiedDuration = baseValue * durationFactor;
        return modifiedDuration;
    }

    public static Vector3 CalculateModifiedSize(Vector3 baseSize)
    {
        float scaleFactor = PlayerAttributes.stats[Attribute.size];
        Vector3 modifiedSize = new Vector3(baseSize.x * scaleFactor, baseSize.y * scaleFactor, baseSize.z);
        return modifiedSize;
    }

    public static float CalculateModifiedBulletSpeed(float baseValue)
    {
        float scaleFactor = PlayerAttributes.stats[Attribute.bulletSpeed];
        float modifiedSpeed = baseValue * scaleFactor;
        return modifiedSpeed;
    }
}
