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

    public EnemyXpObjectManager enemyXpObjectManager;

    [SerializeField] private int EnemiesPerCooldown = 40;
    [SerializeField] private float SpawnCooldownSeconds = 5;
    [SerializeField] private int secToIncreaseCount = 10;
    [SerializeField] private int secToIncreaseTier = 30;
    private int enemyTier = 0;

    private float ringSize = 4f,
                  spawnTimer = 0,
                  tierTimer = 0;

    public float gameTimer = 0;

    private System.Random random = new System.Random();

    public bool suspendSpawning;


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
        enemyXpObjectManager = GlobalReferences.enemyXpObjectManager;
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
                int randInt = random.Next(10);
                GameObject nextEntity;

                if (randInt >= 9)                
                {
                    nextEntity = bossEnemies[enemyTier];

                    //nextEntity = getRandomBossEnemy();
                }
                else
                {
                    nextEntity = regularEnemies[enemyTier];

                    //nextEntity = getRandomRegularEnemy();
                } 

                spawnEntity(nextEntity);
            }
        }

        if (tierTimer > secToIncreaseTier)
        {
            IncreaseEnemyTier();

            tierTimer = 0;
        }

    }

    int GetEnemyCountModifier(){
        return (int)Mathf.Floor(gameTimer / secToIncreaseCount);
    }

    void IncreaseEnemyTier(){

        if(enemyTier + 1 < regularEnemies.Length){

            enemyTier++;
        }
    }
    void spawnEntity(GameObject entity)
    {
        Vector3 new_position = generateRandRingPosition(player.transform.position, ringSize);

        GameObject spawnedEnemy = Instantiate(entity, new_position, Quaternion.identity);

        spawnedEnemiesInScene.Add(spawnedEnemy);

        //subscribe to future death events to remove us from the spawn list
        if(spawnedEnemy.TryGetComponent(out Enemy enemy))
        {
            enemy.OnEnemyDeath.AddListener((GameObject caller) => RemoveSelfFromSpawnedEnemiesInScene(caller));
        }
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
                return targetPosition;
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
            position.x < -6.169815 ||
            position.x > 5.689815 ||
            position.y < -6.104609 ||
            position.y > 4.956743
            )
        {
            //Debug.Log(position.x + " " + position.y);
            return false;
        }

        return true;
    }
}
