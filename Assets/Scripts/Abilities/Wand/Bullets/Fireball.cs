using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : Bullet
{
    GameObject explosion;
    public override float pierce { get => base.pierce; set => base.pierce = 1f; }
    public override float speed { get => base.speed; set => base.speed = .6f; }
    public override float duration { get => base.duration; set => base.duration = 3f; }

    public override void Start()
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
