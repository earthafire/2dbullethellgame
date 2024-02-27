using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindShieldManager : ActivatableAbility
{
    public GameObject _windShieldObj;

    void Start()
    {
        cooldownTimeMax = 5f;

        _windShieldObj = (GameObject)Resources.Load("Prefabs/Abilities/WindShield/WindShield", typeof(GameObject));
    }

    public override void Activated()
    {
        Instantiate(_windShieldObj, player.transform.position, Quaternion.Euler(0f, 0f, 0f));
    }
}
