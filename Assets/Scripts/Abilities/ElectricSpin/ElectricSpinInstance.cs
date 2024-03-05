using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricSpinInstance : AbilityObject
{
    public float distance = 3.5f;

    public override float knockback { get; set; } = 1f;
    public override float damage { get; set; } = 10f;
    public override float duration { get; set; } = 3.5f;
    public override float speed { get; set; } = 100f;

    Transform orbiter;

    private void Awake()
    {
        orbiter = this.transform;
        player = GlobalReferences.player;
    }
    public override void OnEnable()
    {
        distance *= ((PlayerAttributes.stats[Attribute.size] / 10) + 1);
        base.OnEnable();
    }

    // Update is called once per frame
    void Update()
    {
        Orbit();
    }

    private void Orbit()
    {
        // Rotate the 'orbiter' around the player
        orbiter.Rotate(Vector3.forward, speed * Time.deltaTime);

        // Calculate the rotation in radians
        float angle = Mathf.Deg2Rad * orbiter.rotation.eulerAngles.z;

        // Calculate the new position for 'orbiter' based on the angle and distance
        Vector3 newPosition = new Vector3(
            player.transform.position.x + distance * Mathf.Cos(angle),
            player.transform.position.y + distance * Mathf.Sin(angle),
            orbiter.position.z
        );

        // Set the new position for 'orbiter'
        orbiter.position = newPosition;
    }

    public override void OnStay(Enemy enemy)
    {
        enemy.TakeDamage((int)damage);
        enemy.GetKnockbacked(player.transform, knockback);
    }
}
