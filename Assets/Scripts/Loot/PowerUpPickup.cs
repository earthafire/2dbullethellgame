using System.Collections;
using UnityEngine;

public enum PowerUpType
{
    Speed,
    Heal
}

// Simple walk-over-it pickup, following the same InteractableLoot pattern as
// BagController/GroundItem (trigger-based - no magnet range, unlike the XP orbs,
// since these only ever exist in small numbers dropped by destructibles).
public class PowerUpPickup : InteractableLoot
{
    public PowerUpType type;

    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float speedDuration = 5f;

    [SerializeField] private int healAmount = 20;

    [SerializeField] private SoundComponent sound;
    [SerializeField] private ParticleSystem particles;

    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        isReady = true;
        _spriteRenderer.enabled = true;
        _collider.enabled = true;
    }

    public override void OnPickUp(GameObject playerObject)
    {
        PlayerAttributes playerAttributes = playerObject.GetComponent<PlayerAttributes>();

        switch (type)
        {
            case PowerUpType.Speed:
                playerAttributes.ApplySpeedBoost(speedMultiplier, speedDuration);
                break;
            case PowerUpType.Heal:
                playerAttributes.Heal(healAmount);
                break;
        }

        _collider.enabled = false;
        _spriteRenderer.enabled = false;

        if (sound != null)
        {
            sound.sfxToPlay.PlaySFX();
        }
        if (particles != null)
        {
            particles.Play();
        }

        StartCoroutine(ReturnToPoolAfterDelay(0.5f));
    }

    private IEnumerator ReturnToPoolAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }
}
