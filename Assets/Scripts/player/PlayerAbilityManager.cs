using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    public ActivatableAbility weapon;

    void Start()
    {
        weapon = GetComponent<ActivatableAbility>();
    }

    void FixedUpdate()
    {
        DetectFire1();
    }

    public void DetectFire1()
    {
        if (Input.GetButton("Fire1"))
        {
            weapon.Activate();
        }
    }
}
