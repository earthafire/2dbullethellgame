using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : Bullet
{
    GameObject explosion;

    void Start()
    {
        base.Start();
        explosion = (GameObject)Resources.Load("Prefabs/Abilities/Fireball/Explosion", typeof(GameObject));
    }

    public override void OnHit(Enemy enemy)
    {
        Instantiate(explosion, transform.position, Quaternion.Euler(0f, 0f, 0f)).GetComponent<Explosion>();

        base.OnHit(enemy);
    }
}
