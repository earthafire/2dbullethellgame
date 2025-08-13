using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octoshot : ActivatableAbility
{
    public GameObject _OctoshotObj;

    void Start()
    {
        cooldownTimeMax = 5f;

        _OctoshotObj = (GameObject)Resources.Load("Prefabs/Abilities/Octoshot/Octoshot", typeof(GameObject));
    }

    public override void Activated()
    {
        Instantiate(_OctoshotObj, player.transform.position, Quaternion.Euler(0f, 0f, 0f));
        Instantiate(_OctoshotObj, player.transform.position, Quaternion.Euler(0f, 0f, 45f));
        Instantiate(_OctoshotObj, player.transform.position, Quaternion.Euler(0f, 0f, 90f));
        Instantiate(_OctoshotObj, player.transform.position, Quaternion.Euler(0f, 0f, 135f));
        Instantiate(_OctoshotObj, player.transform.position, Quaternion.Euler(0f, 0f, 180f));
        Instantiate(_OctoshotObj, player.transform.position, Quaternion.Euler(0f, 0f, 225f));
        Instantiate(_OctoshotObj, player.transform.position, Quaternion.Euler(0f, 0f, 270f));
        Instantiate(_OctoshotObj, player.transform.position, Quaternion.Euler(0f, 0f, 315f));
    }
}
