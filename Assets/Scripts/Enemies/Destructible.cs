using System.Collections;
using UnityEngine;

// A stationary, non-threatening "enemy" (barrel, crate, pot, etc.) - reuses
// Enemy's TakeDamage/hit-detection pipeline (AbilityObject.OnTriggerEnter2D looks
// up a literal Enemy component) so every existing weapon just works against it for
// free, but hides via `new` the parts of Enemy that assume a living, wandering
// combatant: navigation/chase (Move), the far-from-player despawn check (Update),
// and the Attributes/animator/shadow-driven OnEnable/Start. See Docs or Enemy.cs
// for why those particular members are non-virtual and have to be hidden this way
// rather than overridden.
public class Destructible : Enemy
{
    [SerializeField] private float maxHealth = 30f;

    [Tooltip("Power-up prefabs this can drop when destroyed - one is picked at random.")]
    [SerializeField] private GameObject[] powerUpDrops;

    [Range(0f, 1f)]
    [SerializeField] private float dropChance = 1f;

    [SerializeField] private float destroyDelay = 0.5f;

    new void Start()
    {
        player = GlobalReferences.player;
    }

    new void OnEnable()
    {
        health = maxHealth;
        speed = 0;
        damage = 0; // never deals contact damage, even if the player stands on it
    }

    // Stationary - no chasing, no "wandered too far from player" despawn.
    new void Update() { }
    new void FixedUpdate() { }

    public override IEnumerator GetDeath()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        TrySpawnPowerUp();

        OnEnemyDeath.Invoke(gameObject);

        yield return new WaitForSeconds(destroyDelay);

        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    private void TrySpawnPowerUp()
    {
        if (powerUpDrops == null || powerUpDrops.Length == 0)
        {
            return;
        }
        if (GlobalReferences.GetRandomDouble() > dropChance)
        {
            return;
        }

        GameObject drop = powerUpDrops[GlobalReferences.GetRandomNumber(0, powerUpDrops.Length)];
        ObjectPoolManager.SpawnObject(drop, transform.position, Quaternion.identity);
    }
}
