using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy: Enemy
{
    GameObject shadow;
    new void Start()
    {
        shadow = transform.GetChild(0).gameObject;
        base.Start();
    }

    new void FixedUpdate()
    {
        if ( suspendActions ){ return; }

        base.FixedUpdate();
    }

    override public IEnumerator GetDeath()
    {
        shadow.SetActive(false);

        yield return base.GetDeath();
    }
}
