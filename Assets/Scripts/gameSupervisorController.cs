using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class gameSupervisorController : MonoBehaviour
{
    public GameObject[] regularEnemies;
    public GameObject[] bossEnemies;

    private GameObject player;

    private float ringsize = 2.25f;
    [SerializeField] private int EnemiesPerCooldown = 5;
    [SerializeField] private float spawn_cooldown_seconds = .8f;
    private int seconds_to_difficulty_gain = 10;

    private float timer = 0;
    public float game_timer = 0;

    private System.Random random = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        game_timer += Time.deltaTime;

        if (player == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer > spawn_cooldown_seconds)
        {
            timer = 0;
            for (int i = 0; i < EnemiesPerCooldown + Mathf.Floor(game_timer / seconds_to_difficulty_gain) ; i++)
            {
                int randInt = random.Next(10);
                GameObject nextEntity;

                if (randInt >= 8)
                {
                    nextEntity = getRandomBossEnemy();

                }
                else
                {
                    nextEntity = getRandomRegularEnemy();
                }

                spawnEntity(nextEntity);
            }
        }
    }

    GameObject getRandomRegularEnemy()
    {
        int randInt = random.Next(regularEnemies.Length - 1);
        return regularEnemies[randInt];
    }

    GameObject getRandomBossEnemy()
    {
        int randInt = random.Next(bossEnemies.Length - 1);
        return bossEnemies[randInt];
    }

    void spawnEntity(GameObject entity)
    {
        Vector3 new_position = generateRandRingPosition(player.transform.position, ringsize);
        GameObject new_slime = Instantiate(entity, new_position, Quaternion.identity);
    }

    private Vector3 generateRandRingPosition(Vector3 center, float distance)
    {
        Vector3 randomDirection = Random.insideUnitCircle.normalized * distance;

        Vector3 targetPosition = center + (randomDirection * distance);
        return targetPosition;
    }
}
