using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class gameSupervisorController : MonoBehaviour
{
    public GameObject[] regularEnemies;
    public GameObject[] bossEnemies;
    private GameObject player;
    
    [SerializeField] private int EnemiesPerCooldown = 20;
    [SerializeField] private float SpawnCooldownSeconds = 5;

    private int secondsUntilIncreaseEnemyCount = 60,
                enemyTier = 0;

    private float ringSize = 2f,
                  spawnTimer = 0;

    public float gameTimer = 0;

    private System.Random random = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        gameTimer += Time.deltaTime;

        if (player == null)
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

                if (randInt >= 8)                
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
    } */
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
        GameObject new_slime = Instantiate(entity, new_position, Quaternion.identity);
    }

    private Vector3 generateRandRingPosition(Vector3 center, float distance)
    {
        Vector3 randomDirection = Random.insideUnitCircle.normalized * distance;

        Vector3 targetPosition = center + (randomDirection * distance);
        return targetPosition;
    }
}
