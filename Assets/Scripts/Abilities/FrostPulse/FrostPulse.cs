using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostPulse : ActivatableAbility
{
    public GameObject frostpulseobj;

    void Start()
    {
        cooldownTimeMax = 5f;

        frostpulseobj = (GameObject)Resources.Load("Prefabs/Abilities/FrostPulse/FrostPulse", typeof(GameObject));
    }

    public override void Activated()
    {
        FrostPulseInstance pulse = Instantiate(frostpulseobj, player.transform.position, Quaternion.Euler(0f, 0f, 0f)).GetComponent<FrostPulseInstance>();
    }
}
