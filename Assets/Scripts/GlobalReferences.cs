using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;


public class GlobalReferences : MonoBehaviour
{
    public static PlayerXPManager xpManager;
    public static GameObject player;
    public static Transform firePoint;
    public static gameSupervisorController gameSupervisorController;
    public static EnemyXpObjectManager enemyXpObjectManager;
    public static LevelGenerator levelGenerator;
    private static readonly Random random = new();

    private void Awake()
    {
        #region Singleton
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        #endregion
    }
    private static GlobalReferences _instance;
    public static GlobalReferences Instance { get { return _instance; } }

    public static int GetRandomNumber(int min, int max)
    {
        int temp = 0;
        temp = random.Next(min, max);
        return temp;
    }

    public static double GetRandomDouble()
    {
        double temp = random.NextDouble();
        return temp;
    }

}
