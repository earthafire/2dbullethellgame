using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wand : ActivatableAbility
{
    // object that will be used as bullet
    public Animator player_animator;
    public GameObject projectile;
    private Transform firePoint;
    private GameObject player;
    private int damage = 20;
    private int playerBaseDamage;
    Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        firePoint = transform.GetChild(0).GetChild(0).GetChild(0);

        mainCamera = Camera.main;

        player_animator = GetComponent<Animator>();
        player = GameObject.Find("Player");
        playerBaseDamage = (int)PlayerAttributes.stats[Attribute.damage];

        base.cooldownTimeMax = .2f;
        //projectile = (GameObject)Resources.Load("Prefabs/Weapons/Lightning/Lightning", typeof(GameObject));
        projectile = (GameObject)Resources.Load("Prefabs/Weapons/Fireball/Fireball", typeof(GameObject));
    }

    public override void Activated()
    {
        player_animator.SetTrigger("Attack");

        Bullet bullet = Instantiate(projectile, firePoint.position, AimTowardsCursor()).GetComponent<Bullet>();

        bullet.onEnemyHit += DealDamage;
    }

    private Quaternion AimTowardsCursor()
    {
        Vector3 mouse_pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 difference = mouse_pos - player.transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        return Quaternion.Euler(0f,0f, rotationZ);
    }

    public void DealDamage(Enemy enemy)
    {
        enemy.TakeDamage(playerBaseDamage + damage);
    }
}
