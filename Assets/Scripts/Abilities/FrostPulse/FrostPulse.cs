using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostPulse : ActivatableAbility
{
    public GameObject frostpulseobj;


    void Start()
    {
        frostpulseobj = (GameObject)Resources.Load("Prefabs/Abilities/FrostPulse/FrostPulse", typeof(GameObject));

        cooldownTimeMax = 5f;
    }

    public override void Activated()
    {
        FrostPulseInstance pulse = Instantiate(frostpulseobj, player.transform.position, Quaternion.Euler(0f, 0f, 0f)).GetComponent<FrostPulseInstance>();
    }
}
