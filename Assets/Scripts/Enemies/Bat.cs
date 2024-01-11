using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : Enemy
{
    private Vector3 target_position;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        target_position = base.player.transform.position;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        if (suspendActions)
        {
            return;
        }
        Move();
    }

}
