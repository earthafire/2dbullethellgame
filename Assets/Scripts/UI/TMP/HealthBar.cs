using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;
    private Animator _heartbeat;

    public void Update()
    {
        _heartbeat.speed = .75f + (slider.maxValue - slider.value) * .05f;
    }

    public void Start(){
        slider.value = slider.maxValue;
        _heartbeat = GetComponentInChildren<Animator>(); 
    }

    public void SetHealth(float health)
    {
        slider.value = health;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void SetMaxHealth(float health)
    {
        slider.maxValue = health;
        fill.color = gradient.Evaluate(1f);
    }
}
