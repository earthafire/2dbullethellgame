using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : Enemy
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
        MushroomMove();
    }

    void MushroomMove()
    {
        base.Move();

        // only change directions when not moving
        if (base.speed_animation_multiplier <= 0)
        {
            target_position = base.player.transform.position;
        }
    }

    override public IEnumerator GetDeath()
    {

        yield return base.GetDeath();
    }
}
