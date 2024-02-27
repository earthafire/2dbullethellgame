using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostPulse : ActivatableAbility
{
    public GameObject frostPulseObj;

    void Start()
    {
        cooldownTimeMax = 5f;

        frostPulseObj = (GameObject)Resources.Load("Prefabs/Abilities/FrostPulse/FrostPulse", typeof(GameObject));
    }

    public override void Activated()
    {
        Instantiate(frostPulseObj, player.transform.position, Quaternion.Euler(0f, 0f, 0f));
    }
}
