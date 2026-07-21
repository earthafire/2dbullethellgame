using System;
using UnityEngine;
public class ParticleColorRandomizer : MonoBehaviour
{
    [Serializable]
    public struct ColorChance
    {
        public Color color;
        [Tooltip("Relative weight for this color. Higher values make this color more likely.")]
        public float weight;
    }

    [Tooltip("If enabled, the particle system's start color will be randomized when this object enables.")]
    public bool randomizeOnEnable = true;

    [Tooltip("Colors and relative chances used by this particle system.")]
    public ColorChance[] colorChances = new ColorChance[]
    {
        new ColorChance { color = Color.white, weight = 1f }
    };

    [Tooltip("Optional override particle system to apply the color to. If empty, the first ParticleSystem on this object is used.")]
    public ParticleSystem particleSystemOverride;

    private ParticleSystem _particles;

    private void Awake()
    {
        _particles = particleSystemOverride != null
            ? particleSystemOverride
            : GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        if (randomizeOnEnable)
            ApplyRandomColor();
    }

    public void ApplyRandomColor()
    {
        if (_particles == null || colorChances == null || colorChances.Length == 0)
            return;

        Color chosenColor = PickRandomColor(colorChances);
        SetParticleStartColor(chosenColor);
    }

    public void ApplyRandomColor(ColorChance[] overrideChances)
    {
        if (_particles == null || overrideChances == null || overrideChances.Length == 0)
            return;

        Color chosenColor = PickRandomColor(overrideChances);
        SetParticleStartColor(chosenColor);
    }

    public static Color PickRandomColor(ColorChance[] chances)
    {
        if (chances == null || chances.Length == 0)
            return Color.white;

        float totalWeight = 0f;
        for (int i = 0; i < chances.Length; i++)
        {
            totalWeight += Mathf.Max(0f, chances[i].weight);
        }

        if (totalWeight <= 0f)
        {
            return chances.Length > 0 ? chances[0].color : Color.white;
        }

        float sample = UnityEngine.Random.value * totalWeight;
        for (int i = 0; i < chances.Length; i++)
        {
            float weight = Mathf.Max(0f, chances[i].weight);
            if (sample <= weight)
                return chances[i].color;
            sample -= weight;
        }

        return chances[chances.Length - 1].color;
    }

    private void SetParticleStartColor(Color color)
    {
        if (_particles == null)
            return;

        var main = _particles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(color);
    }
}
