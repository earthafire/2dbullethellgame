using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{
    private Vector3 target_position;
    public EnemyXpObjectBehaviour xp;
    public GameObject bag;
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
            target_position = player.transform.position;
        }
    }

    override public IEnumerator GetDeath()
    {
        if (bag != null)
        {
            Instantiate(bag, transform.position, Quaternion.identity);
        }

        yield return base.GetDeath();
    }
}
