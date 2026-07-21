using System.Collections.Generic;
using UnityEngine;

public class EnemyHitParticleManager : MonoBehaviour
{
    [Tooltip("Prefab containing a ParticleSystem for enemy hit VFX. One instance will be reused per pool slot.")]
    [SerializeField] private GameObject hitEmitterPrefab;

    [Tooltip("Number of particle emitter instances to keep available for reuse.")]
    [SerializeField] private int poolSize = 8;

    [Tooltip("If true, each emitter will re-randomize its color on every spawn.")]
    [SerializeField] private bool randomizeColorOnSpawn = true;

    private readonly List<ParticleSystem> _emitters = new List<ParticleSystem>();
    private int _nextEmitterIndex;

    private void Awake()
    {
        GlobalReferences.enemyHitParticleManager = this;
        InitializePool();
    }

    private void InitializePool()
    {
        if (hitEmitterPrefab == null)
        {
            Debug.LogWarning("EnemyHitParticleManager has no hitEmitterPrefab assigned.");
            return;
        }

        for (int i = 0; i < Mathf.Max(1, poolSize); i++)
        {
            GameObject instance = Instantiate(hitEmitterPrefab, transform);
            instance.SetActive(false);

            ParticleSystem ps = instance.GetComponentInChildren<ParticleSystem>();
            if (ps == null)
            {
                Debug.LogWarning("EnemyHitParticleManager hitEmitterPrefab does not contain a ParticleSystem.");
                Destroy(instance);
                continue;
            }

            _emitters.Add(ps);
        }
    }

    public void SpawnHitEffect(Vector3 position, Vector2 direction, int emitCount, ParticleColorRandomizer.ColorChance[] colorChances = null)
    {
        if (_emitters.Count == 0)
            return;

        ParticleSystem emitter = GetNextEmitter();
        if (emitter == null)
            return;

        GameObject emitterObject = emitter.gameObject;
        emitterObject.transform.position = position;
        emitterObject.transform.rotation = Quaternion.identity;

        if (direction.sqrMagnitude > 0.0001f)
        {
            emitterObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        Color chosenColor = Color.white;
        if (colorChances != null && colorChances.Length > 0)
        {
            chosenColor = ParticleColorRandomizer.PickRandomColor(colorChances);
        }
        else if (randomizeColorOnSpawn)
        {
            ParticleColorRandomizer randomizer = emitter.GetComponentInChildren<ParticleColorRandomizer>();
            if (randomizer != null)
            {
                chosenColor = ParticleColorRandomizer.PickRandomColor(randomizer.colorChances);
            }
        }

        SetEmitterColor(emitter, chosenColor);

        emitter.Clear();
        emitterObject.SetActive(true);
        emitter.Emit(Mathf.Max(1, emitCount));
    }

    private ParticleSystem GetNextEmitter()
    {
        if (_emitters.Count == 0)
            return null;

        ParticleSystem emitter = _emitters[_nextEmitterIndex];
        _nextEmitterIndex = (_nextEmitterIndex + 1) % _emitters.Count;
        return emitter;
    }

    private void SetEmitterColor(ParticleSystem emitter, Color color)
    {
        if (emitter == null)
            return;

        var main = emitter.main;
        main.startColor = new ParticleSystem.MinMaxGradient(color);
    }
}
