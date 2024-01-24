using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnipHelicopter: Enemy
{
    GameObject shadow;

    new void Start()
    {
        base.Start();
        shadow = transform.GetChild(0).gameObject;

    }

    new void FixedUpdate()
    {
        base.Update();
        if (suspendActions)
        {
            return;
        }
        TurnipHelicopterMove();
    }

    void TurnipHelicopterMove()
    {
        Move();
    }

    override public IEnumerator GetDeath()
    {
       
        shadow.SetActive(false);

        yield return base.GetDeath();
    }
}
