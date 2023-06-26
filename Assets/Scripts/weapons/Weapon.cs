using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    // Duration in seconds

    public float cooldown;
    private float cooldown_timer = 0f;
    public int damage;


    // Start is called before the first frame update
    public void Start()
    {
        cooldown = 200f;
        damage = 0;
    }

    // Update is called once per frame
    public void Update()
    {
        LowerCooldown(Time.deltaTime);
    }

    // Reduce remaining time on cooldown by 'amount'
    void LowerCooldown(float amount)
    {
        if (cooldown_timer < cooldown)
        {
            cooldown_timer += amount;
        }
    }

    // Activates weapon's ability (Activated) if cooldown is met
    public void Activate()
    {
        if (cooldown_timer >= cooldown)
        {
            cooldown_timer = 0;
            Activated();    
        }
    }

    // Weapon's ability (override this)
    public abstract void Activated();

    public void DealDamage(Enemy target)
    {
        target.Damage(damage);
    }
}
