using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wand : ActivatableAbility
{
    // object that will be used as bullet
    public Animator player_animator;
    public GameObject projectile;
    public Transform firePoint;
    private GameObject player;
    private int damage = 20;
    private int playerBaseDamage;

    // Start is called before the first frame update
    void Start()
    {
        firePoint = transform.GetChild(0).GetChild(0).GetChild(0);
        player_animator = GetComponent<Animator>();
        player = GameObject.Find("Player");
        playerBaseDamage = (int)player.GetComponent<PlayerAttributes>()._localAttributes[Attribute.damage];

        base.cooldownTimeMax = .2f;
        projectile = (GameObject)Resources.Load("Prefabs/Weapons/Lightning/Lightning", typeof(GameObject));
    }

    public override void Activated()
    {
        player_animator.SetTrigger("Attack");

        Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 difference = mouse_pos - player.transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        Bullet bullet = Instantiate(projectile, firePoint.transform.position, Quaternion.Euler(0f, 0f, rotationZ)).GetComponent<Bullet>();

        bullet.onEnemyHit += bulletHit;
    }

    public void bulletHit(Enemy enemy)
    {
        enemy.TakeDamage(playerBaseDamage + damage);
        FindObjectOfType<AudioManager>().Play("ShootFireball");
    }
}
