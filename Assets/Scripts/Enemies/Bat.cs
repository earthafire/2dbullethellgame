using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : Enemy
{
    private Rigidbody2D rb;
    private Transform sprite_renderer_transform;
    public float rotationSpeed = .2f;
    private Vector3 target_position;
    public experience xp;
    public int xp_tier;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        
        sprite_renderer_transform = this.transform;
        rb = GetComponent<Rigidbody2D>();
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
        base.Move();
    }
}
