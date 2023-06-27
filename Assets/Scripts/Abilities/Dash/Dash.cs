using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A short uninterruptible dash ability, in the direction the player was previously moving
/// </summary>
public class Dash : ActivatableAbility
{
    private GameObject player;

    void Start()
    {
        player = GameObject.Find("Player");
        base.cooldownTimeMax = 1f;
    }

    public override void Activated()
    {
        Debug.Log("DASHING");

        Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        Vector2 direction = movement.direction;

        movement.isPlayerInControl = false;



        rb2d.velocity = direction * 20;
    }
}
