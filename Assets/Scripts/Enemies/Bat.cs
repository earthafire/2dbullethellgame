using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : Enemy
{
    public float speed = .5f;
    public float speed_animation_multiplier = 1;
    private Vector3 target_position;
    public experience xp;
    public int xp_tier;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        target_position = base.target.transform.position;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        Move();
    }

    void Move()
    {
        if (target == null)
        {
            return;
        }

        // only change directions when not moving
        if (speed_animation_multiplier <= 0)
        {
            target_position = base.target.transform.position;
        }

        float distance = speed * speed_animation_multiplier * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target_position, distance);
        base.rb2d.velocity = Vector2.zero;
    }

    override public IEnumerator Kill()
    {
        experience new_loot = Instantiate(xp, this.transform.position, Quaternion.identity).GetComponent<experience>();
        new_loot.SetTier(xp_tier);
        yield return base.Kill();
    }
}
