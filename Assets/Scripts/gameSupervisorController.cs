using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameSupervisorController : MonoBehaviour
{

    public GameObject player;
    public GameObject slime;

    public int EnemiesPerCooldown = 2;
    public int cooldown = 2;
    private float timer = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > cooldown)
        {
            Debug.Log("spawning");
            timer = 0;
            spawnEntity(slime);
        }
    }

    void spawnEntity(GameObject entity)
    {
        Vector3 new_position = generateRandRingPosition(transform.position, 10);
        slime = Instantiate(entity, new_position, Quaternion.identity);
    }

    private Vector3 generateRandRingPosition(Vector3 center, int distance)
    {
        Vector3 randomDirection = Random.onUnitSphere;
        Vector3 targetPosition = center + randomDirection * distance;
        return targetPosition;
    }
}
