using System.Collections.Generic;
using UnityEngine;

public class EnemyXpObjectManager : MonoBehaviour
{
    private GameObject xpPrefab;

    // Active (spawned, not yet returned to the pool) XP orbs, checked against the
    // player's position once per frame instead of via per-orb Collider2D triggers -
    // the trigger-based approach caused a severe Physics2D broad-phase cost once
    // enough orbs clustered together after a big kill (see
    // Docs/NAVIGATION_MIGRATION.md). A plain distance check over even hundreds of
    // orbs is trivial by comparison. Uses swap-remove (via each orb's ActiveIndex)
    // so unregistering during a mass-pickup burst stays O(1) per orb instead of the
    // O(n) a plain List.Remove would cost.
    static readonly List<EnemyXpObjectBehaviour> _activeOrbs = new();

    // Cached from the player's current pickUpRange attribute and only refreshed when
    // an upgrade actually changes it (via Attributes.upgradeApplied) - avoids every
    // active orb doing its own PlayerAttributes.stats lookup every single tick.
    static float _magnetRange = 0.5f;
    static bool _subscribedToUpgrades;

    public void Awake()
    {
        GlobalReferences.enemyXpObjectManager = this;
        _subscribedToUpgrades = false;
    }
    public void Start()
    {
        xpPrefab = (GameObject)Resources.Load("Prefabs/Loot/Experience", typeof(GameObject));
    }

    public void SpawnXP(GameObject caller)
    {
        Enemy enemy = caller.GetComponent<Enemy>();

        GameObject newXPObject = ObjectPoolManager.SpawnObject(xpPrefab, caller.transform.position, Quaternion.identity);

        newXPObject.GetComponent<SpriteRenderer>().sprite = enemy._xpData.GetRandomSprite();
        newXPObject.GetComponent<EnemyXpObjectBehaviour>().experienceAmount = enemy._xpData.xpValue;
    }

    public static void Register(EnemyXpObjectBehaviour orb)
    {
        orb.ActiveIndex = _activeOrbs.Count;
        _activeOrbs.Add(orb);
    }

    public static void Unregister(EnemyXpObjectBehaviour orb)
    {
        int index = orb.ActiveIndex;
        if (index < 0)
            return;

        int lastIndex = _activeOrbs.Count - 1;
        EnemyXpObjectBehaviour last = _activeOrbs[lastIndex];
        _activeOrbs[index] = last;
        last.ActiveIndex = index;
        _activeOrbs.RemoveAt(lastIndex);

        orb.ActiveIndex = -1;
    }

    void Update()
    {
        GameObject player = GlobalReferences.player;
        if (player == null)
            return;

        if (!_subscribedToUpgrades)
            TrySubscribeToUpgrades(player);

        // Iterate backwards: TickProximity can deactivate the orb, which triggers
        // Unregister (swap-remove) on this same list mid-loop.
        for (int i = _activeOrbs.Count - 1; i >= 0; i--)
        {
            _activeOrbs[i].TickProximity(player, _magnetRange);
        }
    }

    // Deferred rather than done in Awake()/Start() - PlayerAttributes.attributes is a
    // scene reference not guaranteed ready yet relative to this manager, so this
    // retries every frame (guarded by _subscribedToUpgrades) until it succeeds once.
    static void TrySubscribeToUpgrades(GameObject player)
    {
        PlayerAttributes playerAttributes = player.GetComponent<PlayerAttributes>();
        if (playerAttributes == null || playerAttributes.attributes == null)
            return;

        _magnetRange = playerAttributes.attributes.GetAttribute(Attribute.pickUpRange);
        playerAttributes.attributes.upgradeApplied += OnUpgradeApplied;
        _subscribedToUpgrades = true;
    }

    static void OnUpgradeApplied(Attributes attributes, UpgradeAttribute upgrade)
    {
        if (upgrade.upgradeToApply.ContainsKey(Attribute.pickUpRange))
        {
            _magnetRange = attributes.GetAttribute(Attribute.pickUpRange);
        }
    }
}
