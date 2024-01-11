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

    EnemyXpObjectManager enemyXpObjectManager;

    [SerializeField] private int EnemiesPerCooldown = 40;
    [SerializeField] private float SpawnCooldownSeconds = 5;

    private int secondsUntilIncreaseEnemyCount = 60,
                enemyTier = 0;

    private float ringSize = 2f,
                  spawnTimer = 0;

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

        if (player == null)
        {
            return;
        }
        if (suspendSpawning)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer > SpawnCooldownSeconds)
        {
            IncreaseEnemyTier();
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
    }

/*     GameObject getRandomRegularEnemy()
    {
        int randInt = random.Next(regularEnemies.Length);
        return regularEnemies[randInt];
    }

    GameObject getRandomBossEnemy()
    {
        int randInt = random.Next(bossEnemies.Length);
        return bossEnemies[randInt];
    } 
*/

    int GetEnemyCountModifier(){
        return (int)Mathf.Floor(gameTimer / secondsUntilIncreaseEnemyCount);
    }

    int IncreaseEnemyTier(){
        if(GetEnemyCountModifier() > 0){
            if(enemyTier + 1 < regularEnemies.Length){
                enemyTier++;
                return enemyTier;
            }
            return regularEnemies.Length;
        }
        return 0;
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
            enemy.OnEnemyDeath.AddListener((GameObject caller) => enemyXpObjectManager.SpawnXP(caller));
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
            enemy.OnEnemyDeath.RemoveListener((GameObject caller) => enemyXpObjectManager.SpawnXP(caller));
        }
    }

    private Vector3 generateRandRingPosition(Vector3 center, float distance)
    {
        Vector3 randomDirection = Random.insideUnitCircle.normalized * distance;

        Vector3 targetPosition = center + (randomDirection * distance);
        return targetPosition;
    }
}
