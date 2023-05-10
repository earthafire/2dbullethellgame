using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameSupervisorController : MonoBehaviour
{

    public GameObject slime;
    private GameObject player;

    public float ringsize = 1.5f;
    public int EnemiesPerCooldown = 2;
    public float cooldown = .8f;
    private float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            return;
        }
        timer += Time.deltaTime;
        if (timer > cooldown)
        {
            timer = 0;
            for (int i = 0; i < EnemiesPerCooldown; i++)
            {
                spawnEntity(slime);
            }
        }
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
