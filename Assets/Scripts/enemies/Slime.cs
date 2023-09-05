using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{    
    private Vector3 target_position;
    public experience xp;
    public int xp_tier;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        SlimeMove();
    }

    void SlimeMove()
    {
        Move();

        // only change directions when not moving
        if (speed_animation_multiplier <= 0)
        {
            target_position = player.transform.position;
        }
    }

    override public IEnumerator GetDeath()
    {
        experience new_loot = Instantiate(xp, transform.position, Quaternion.identity).GetComponent<experience>();
        new_loot.SetTier(xp_tier);
        yield return base.GetDeath();
    }
}
