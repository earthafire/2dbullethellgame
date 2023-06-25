using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wand : ActivatableAbility
{
    // object that will be used as bullet
    private GameObject projectile;
    private int damage = 30;
    // Start is called before the first frame update
    void Start()
    {
        base.cooldownTimeMax = .2f;
        projectile = (GameObject)Resources.Load("Prefabs/Weapons/Bullet", typeof(GameObject));
    }

    public override void Activated()
    {
        Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 difference = mouse_pos - transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        Bullet bullet = Instantiate(projectile, transform.position, Quaternion.Euler(0f, 0f, rotationZ)).GetComponent<Bullet>();
        bullet.onEnemyHit += bulletHit;
    }

    public void bulletHit(Enemy enemy)
    {
        enemy.Damage(damage);
        FindObjectOfType<AudioManager>().Play("ShootFireball");
    }
}