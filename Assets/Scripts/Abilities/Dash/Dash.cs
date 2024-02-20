using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A short uninterruptible dash ability, in the direction the player was previously moving
/// </summary>
public class Dash : ActivatableAbility
{
    private Animator player_animator;

    void Start()
    {
        player_animator = GetComponent<Animator>();
        base.cooldownTimeMax = 2f;
    }

    public override void Activated()
    {
        player_animator.SetTrigger("Dash");
        StartCoroutine(Move(player.GetComponent<PlayerMovement>(), player.GetComponent<Rigidbody2D>()));
    }

    /// <summary>
    /// applies a large force in the direction the player is currently traveling
    /// gives player back control after dash is complete.
    /// </summary>
    /// <param name="movement">player movement script, used to toggle player control</param>
    /// <param name="rb2d">player physics component</param>
    /// <returns></returns>
    private IEnumerator Move(PlayerMovement movement, Rigidbody2D rb2d)
    {
        movement.isPlayerInControl = false;

        Vector2 direction = movement.direction;
        rb2d.AddForce(direction * 15, ForceMode2D.Impulse);
        rb2d.drag = 15;

        // Basically, loop forever wait until player has slowed down from dash, then give player control back
        while (rb2d.velocity.magnitude > 2)
        {
            yield return null;
        }

        movement.isPlayerInControl = true;
    }
}
