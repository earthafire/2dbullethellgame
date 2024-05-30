using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
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

    private float ringSize = 3.5f,
                  spawnTimer = 0,
                  tierTimer = 0;

    public float gameTimer = 0;

    public bool suspendSpawning;
    private bool _isBossSpawned = false;

    Collider2D[] detections;

    private void Awake()
    {
        instance = this;
        GlobalReferences.gameSupervisorController = this;
        suspendSpawning = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        player = GlobalReferences.player;
        levelGenerator = GlobalReferences.levelGenerator;
        detections = new Collider2D[32];

        levelGenerator.GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        gameTimer += Time.deltaTime;
        spawnTimer += Time.deltaTime;
        tierTimer += Time.deltaTime;

        if (player == null)
        {
            return;
        }
        if (suspendSpawning)
        {
            return;
        }

        if (spawnTimer > SpawnCooldownSeconds)
        {
            spawnTimer = 0;

            for (int i = 0; i < EnemiesPerCooldown + GetEnemyCountModifier(); i++)
            {
                spawnEntity(regularEnemies[enemyTier]);

            }
            if (_isBossSpawned == false)
            {
                StartCoroutine(SpawnWithDelay(bossEnemies[enemyTier], 15f));
                _isBossSpawned = true;
            }
        }

        if (tierTimer > secToIncreaseTier)
        {
            tierTimer = 0;
            IncreaseEnemyTier();
            _isBossSpawned = false;
        }
    }
    void spawnEntity(GameObject entity)
    {
        Vector3 new_position = generateRandRingPosition(player.transform.position, ringSize);

        GameObject spawnedEnemy = ObjectPoolManager.SpawnObject(entity, new_position, Quaternion.identity);
        //GameObject spawnedEnemy = Instantiate(entity, new_position, Quaternion.identity);

        spawnedEnemiesInScene.Add(spawnedEnemy);

        //subscribe to future death events to remove us from the spawn list
        if (spawnedEnemy.TryGetComponent(out Enemy enemy))
        {
            enemy.OnEnemyDeath.AddListener((GameObject caller) => RemoveSelfFromSpawnedEnemiesInScene(caller));
        }
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
        }
    }
    private IEnumerator SpawnWithDelay(GameObject _obj, float _delay)
    {
        yield return new WaitForSeconds(_delay);
        spawnEntity(_obj);
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
        detections = Physics2D.OverlapCircleAll(_position, 1.5f);
        foreach (var col in detections)
        {
            if (col.gameObject.layer == 3) // Wall Layer
            {
                return false;
            }
        }
        return true;
    }
}


