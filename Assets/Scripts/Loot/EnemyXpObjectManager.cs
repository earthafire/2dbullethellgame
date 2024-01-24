using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyXpObjectManager : MonoBehaviour
{
    private GameObject xpPrefab;

    public void Awake()
    {
        GlobalReferences.enemyXpObjectManager = this;
    }
    public void Start()
    {
        xpPrefab = (GameObject)Resources.Load("Prefabs/Loot/Experience", typeof(GameObject));
    }

    public void SpawnXP(GameObject caller)
    {
        Enemy enemy = caller.GetComponent<Enemy>();

        GameObject newXPObject = Instantiate(xpPrefab, caller.transform.position, Quaternion.identity);

        newXPObject.GetComponent<SpriteRenderer>().sprite = enemy._xpData.GetRandomSprite();
        newXPObject.GetComponent<EnemyXpObjectBehaviour>().experienceAmount = enemy._xpData.xpValue;
    }


}
