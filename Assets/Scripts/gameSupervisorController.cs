using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// There can be only one of these in the scene, as there is a static reference to the instance (singleton)
/// </summary>
public class gameSupervisorController : MonoBehaviour
{
    //singleton
    public static gameSupervisorController instance;

    public GameObject[] regularEnemies;
    public GameObject[] bossEnemies;
    private GameObject player;
    public List<GameObject> spawnedEnemiesInScene = new List<GameObject>();

    private LevelGenerator levelGenerator;

    [SerializeField] private int EnemiesPerCooldown = 40;
    [SerializeField] private float SpawnCooldownSeconds = 5;
    [SerializeField] private int secToIncreaseCount = 10;
    [SerializeField] private int secToIncreaseTier = 30;
    private int enemyTier = 0;

    private float ringSize = 4.0f;
    public float gameTimer = 0;

    public bool suspendSpawning;
    private bool _willBossSpawnThisTier = false;
    private bool _isBossDefeatedThisTier = false;
    private GameObject bossObj;
    private GameObject currentBossInstance;
    Collider2D[] detections;

    // Cleaner timer system using a dictionary for easy extensibility
    private Dictionary<string, float> timers = new Dictionary<string, float>();
    private Dictionary<string, float> timerDurations = new Dictionary<string, float>();

    private void Awake()
    {
        instance = this;
        GlobalReferences.gameSupervisorController = this;
        suspendSpawning = false;
        InitializeTimers();
    }

    private void InitializeTimers()
    {
        // Easy to add new timers - just add them here
        timers["spawn"] = 0f;
        timers["tier"] = 0f;
        
        timerDurations["spawn"] = SpawnCooldownSeconds;
        timerDurations["tier"] = secToIncreaseTier;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GlobalReferences.player;
        levelGenerator = GlobalReferences.levelGenerator;
        detections = new Collider2D[32];

        levelGenerator.GenerateMap();
    }

    // Update is called every frame
    void Update()
    {
        gameTimer += Time.deltaTime;
        UpdateTimers();

        bossObj = bossEnemies[enemyTier];

        if (player == null)
        {
            return;
        }
        if (suspendSpawning)
        {
            return;
        }

        // Handle tier progression BEFORE spawning so new enemies use the new tier
        HandleTierProgression();
        HandleSpawning();
    }

    private void UpdateTimers()
    {
        // Update all timers at once - easy to extend
        // Use ToList() to create a copy of the keys to avoid modification during enumeration
        foreach (var timerKey in timers.Keys.ToList())
        {
            timers[timerKey] += Time.deltaTime;
        }
    }

    private void HandleSpawning()
    {
        if (timers["spawn"] > timerDurations["spawn"])
        {
            timers["spawn"] = 0;

           // Debug.Log($"Spawning {EnemiesPerCooldown + GetEnemyCountModifier()} enemies from tier {enemyTier}: {regularEnemies[enemyTier].name}");
            
            for (int i = 0; i < EnemiesPerCooldown + GetEnemyCountModifier(); i++)
            {
                spawnEntity(regularEnemies[enemyTier]);
            }

            if (_willBossSpawnThisTier == false)
            {
                bossObj = bossEnemies[enemyTier];
                StartCoroutine(SpawnWithDelay(bossObj, 15f));
                _willBossSpawnThisTier = true;
                _isBossDefeatedThisTier = false;
            }
        }
    }

    private void HandleTierProgression()
    {
        // Check if boss is defeated - no timer requirement for immediate progression
        if (_isBossDefeatedThisTier)
        {
            Debug.Log($"Tier progression triggered! Current tier: {enemyTier}");
            timers["tier"] = 0;
            IncreaseEnemyTier();
        }
    }

    // Call this when boss dies to mark tier as complete
    public void OnBossDefeated()
    {
        _isBossDefeatedThisTier = true;
        _willBossSpawnThisTier = false;
        // Resume spawning after boss is defeated
        suspendSpawning = false;
        Debug.Log($"Boss defeated! Tier {enemyTier} complete. Ready to progress to tier {enemyTier + 1}");
    }

    // Call this when boss spawns to suspend spawning
    public void OnBossSpawned(GameObject bossInstance)
    {
        currentBossInstance = bossInstance;
        suspendSpawning = true;
        
        // Subscribe to boss death event
        if (bossInstance.TryGetComponent(out Enemy bossEnemy))
        {
            bossEnemy.OnEnemyDeath.AddListener((GameObject caller) => OnBossDefeated());
        }
    }

    GameObject spawnEntity(GameObject entity)
    {
        // Spawn entity at random ring position around player
        Vector3 new_position = generateRandRingPosition(player.transform.position, ringSize);

        GameObject spawnedEnemy = ObjectPoolManager.SpawnObject(entity, new_position, Quaternion.identity);
        //GameObject spawnedEnemy = Instantiate(entity, new_position, Quaternion.identity);

        spawnedEnemiesInScene.Add(spawnedEnemy);

        //subscribe to future death events to remove us from the spawn list
        if (spawnedEnemy.TryGetComponent(out Enemy enemy))
        {
            enemy.OnEnemyDeath.AddListener((GameObject caller) => RemoveSelfFromSpawnedEnemiesInScene(caller));
        }
        return spawnedEnemy;
    }

    int GetEnemyCountModifier()
    {
        return (int)Mathf.Floor(gameTimer / secToIncreaseCount);
    }

    void IncreaseEnemyTier()
    {
        if (enemyTier + 1 < regularEnemies.Length)
        {
            enemyTier++;
            Debug.Log($"Tier increased to: {enemyTier}. Will spawn enemies from: {regularEnemies[enemyTier].name}");
            // Reset tier-specific flags
            _willBossSpawnThisTier = false;
            _isBossDefeatedThisTier = false;
        }
        else
        {
            Debug.Log($"Cannot increase tier: {enemyTier + 1} >= {regularEnemies.Length} (max tiers)");
        }
    }

    private IEnumerator SpawnWithDelay(GameObject _obj, float _delay)
    {
        yield return new WaitForSeconds(_delay);
        GameObject spawnedBoss = spawnEntity(_obj);
        OnBossSpawned(spawnedBoss);
    }

    private IEnumerator Delay(float _delay)
    {
        yield return new WaitForSeconds(_delay);
    }

    private void RemoveSelfFromSpawnedEnemiesInScene(GameObject caller)
    {
        if (spawnedEnemiesInScene.Contains(caller))
        {
            spawnedEnemiesInScene.Remove(caller);
        }
        //remove the listener also, so it doesn't tie up memory after it is dead
        if (caller.TryGetComponent(out Enemy enemy))
        {
            enemy.OnEnemyDeath.RemoveListener((GameObject caller) => RemoveSelfFromSpawnedEnemiesInScene(caller));
        }
    }

    // Helper method to add new timers easily
    public void AddTimer(string timerName, float duration)
    {
        if (!timers.ContainsKey(timerName))
        {
            timers[timerName] = 0f;
            timerDurations[timerName] = duration;
        }
    }

    // Helper method to check if a timer is ready
    public bool IsTimerReady(string timerName)
    {
        return timers.ContainsKey(timerName) && timers[timerName] >= timerDurations[timerName];
    }

    // Helper method to reset a specific timer
    public void ResetTimer(string timerName)
    {
        if (timers.ContainsKey(timerName))
        {
            timers[timerName] = 0f;
        }
    }

    private Vector3 generateRandRingPosition(Vector3 center, float distance)
    {
        Vector3 currentAngle = Random.insideUnitCircle.normalized * distance;
        Vector3 targetPosition = center + (Vector3)currentAngle;

        int degreesPerAttempt = 75;

        for (int i = 0; i < 1800 / degreesPerAttempt; i++)
        {
            targetPosition = center + Quaternion.Euler(0, 0, degreesPerAttempt * i) * currentAngle;

            if (isPositionInSpawnArea(targetPosition))
            {
                if (isPositionInOpenArea(targetPosition))
                {
                    return targetPosition;
                }
            }
        }

        return targetPosition;
    }

    /// <summary>
    /// Checks if a Vector3 is in the playable area
    /// </summary>
    /// <param name="position">vector3 position to check</param>
    /// <returns>true if within bounding box, false otherwise</returns>
    private bool isPositionInSpawnArea(Vector3 position)
    {
        if (
            position.x < 0 ||
            position.x > 23 ||
            position.y > 23 ||
            position.y < 0
            )
        {
            //Debug.Log(position.x + " " + position.y);
            return false;
        }

        return true;
    }

    private bool isPositionInOpenArea(Vector3 _position)
    {
       // detections = Physics2D.OverlapCircleAll(_position, 1.5f);
        LayerMask walls = LayerMask.GetMask("Walls");
        Collider2D collider = Physics2D.OverlapCircle(_position, 1f, walls);

        // If a collider is found, it means the position is NOT in an open area
        // The collider variable will be null if nothing is found, so we check for that.
        if (collider != null)
        {
            // A wall collider was detected, so the position is blocked
            return false;
        }

        // No wall colliders were detected, so the position is open
        return true;
    }
}


