using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{
    private Vector3 target_position;
    
    // Update is called once per frame
    new void FixedUpdate()
    {
        base.Update();
        if (suspendActions)
        {
            return;
        }
        SlimeMove();
    }

    void SlimeMove()
    {
        if (player == null)
        {
            return;
        }

        Move();

        // only change directions when not moving
        if (speed_animation_multiplier <= 0)
        {
           // target_position = player.transform.position;
        }
    }

    new void GetDeath()
    {        
        GetDeath();
    }
}
