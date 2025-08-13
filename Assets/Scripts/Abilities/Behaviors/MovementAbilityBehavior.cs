using UnityEngine;
using System.Collections;

/// <summary>
/// Behavior for movement-based abilities (Dash, etc.)
/// </summary>
public class MovementAbilityBehavior : ActivatableAbility
{
    [Header("Movement Settings")]
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private bool usePlayerMovement = true;

    private PlayerMovement playerMovement;
    private Rigidbody2D playerRigidbody;
    private bool isDashing = false;

    public override void Initialize(AbilityData data)
    {
        base.Initialize(data);

        // Get player components
        playerMovement = GetComponent<PlayerMovement>();
        playerRigidbody = GetComponent<Rigidbody2D>();

        if (playerMovement == null)
        {
            Debug.LogWarning($"[{name}] No PlayerMovement component found!");
        }

        if (playerRigidbody == null)
        {
            Debug.LogWarning($"[{name}] No Rigidbody2D component found!");
        }
    }

    public override void Activated()
    {
        if (isDashing) return;

        // Get movement direction
        Vector2 dashDirection = GetMovementDirection();

        if (dashDirection.magnitude < 0.1f)
        {
            Debug.LogWarning($"[{name}] Cannot dash: No movement direction!");
            return;
        }

        StartCoroutine(PerformDash(dashDirection));
    }

    private Vector2 GetMovementDirection()
    {
        if (playerMovement != null)
        {
            return playerMovement.direction;
        }

        // Fallback to input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        return new Vector2(horizontal, vertical).normalized;
    }

    private IEnumerator PerformDash(Vector2 direction)
    {
        isDashing = true;

        // Disable player control
        if (playerMovement != null)
        {
            playerMovement.isPlayerInControl = false;
        }

        // Apply dash force
        if (playerRigidbody != null)
        {
            playerRigidbody.AddForce(direction * dashForce, ForceMode2D.Impulse);
            playerRigidbody.drag = 15f; // High drag to slow down quickly
        }

        // Wait for dash to complete
        yield return new WaitForSeconds(dashDuration);

        // Re-enable player control
        if (playerMovement != null)
        {
            playerMovement.isPlayerInControl = true;
        }

        // Reset drag
        if (playerRigidbody != null)
        {
            playerRigidbody.drag = 0f;
        }

        isDashing = false;

        Debug.Log($"[{name}] Dash completed in direction {direction}");
    }

    public override void Cleanup()
    {
        // Stop any ongoing dash
        if (isDashing)
        {
            StopAllCoroutines();
            isDashing = false;

            if (playerMovement != null)
            {
                playerMovement.isPlayerInControl = true;
            }

            if (playerRigidbody != null)
            {
                playerRigidbody.drag = 0f;
            }
        }
    }
}
