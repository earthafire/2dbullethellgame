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
        Instantiate(frostPulseObj, player.transform);
    }
}
