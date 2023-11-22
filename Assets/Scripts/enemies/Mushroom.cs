using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : Enemy
{    
    private Vector3 target_position;
    public experience xp;
    public int xp_tier;

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

    void Move()
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
        experience new_loot = Instantiate(xp, this.transform.position, Quaternion.identity).GetComponent<experience>();
        new_loot.SetTier(xp_tier);
        yield return base.GetDeath();
    }
}
