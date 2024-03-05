using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricSpinManager : ActivatableAbility
{
    public GameObject electricSpinObj;

    void Start()
    {
        electricSpinObj = (GameObject)Resources.Load("Prefabs/Abilities/ElectricSpin/ElectricSpin", typeof(GameObject));
        cooldownTimeMax = 3.25f;
    }
    public override void Activated()
    {
        ElectricSpinInstance instance = Instantiate(electricSpinObj, player.transform.position, Quaternion.identity).GetComponent<ElectricSpinInstance>();
        //cooldownTimeMax = instance.GetComponent<ElectricSpinInstance>().duration;
    }
}
