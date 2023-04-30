using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : Enemy
{
    public float speed = .25f;
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        Move();
    }

    void Move()
    {
        float distance = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, base.target.transform.position, distance);
    }
}
