using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalReferences : MonoBehaviour
{
    public static PlayerXPManager xpManager;
    public static GameObject player;
    public static gameSupervisorController gameSupervisorController;
    public static EnemyXpObjectManager enemyXpObjectManager;
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

}
