using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wand : ActivatableAbility
{
    public Animator player_animator;
    public GameObject projectile;
    private Transform firePoint;
    private GameObject player;
    private int damage = 20;
    Camera mainCamera;

    void Start()
    {
        firePoint = transform.GetChild(0).GetChild(0).GetChild(0);
        GlobalReferences.firePoint = firePoint;

        mainCamera = Camera.main;

        player_animator = GetComponent<Animator>();
        player = GlobalReferences.player;
        cooldownTimeMax = 0.75f;

        //projectile = (GameObject)Resources.Load("Prefabs/Abilities/Lightning/Lightning", typeof(GameObject));
        projectile = (GameObject)Resources.Load("Prefabs/Abilities/Fireball/Fireball", typeof(GameObject));
    }

    public override void Activated()
    {
        player_animator.SetTrigger("Attack");

        Bullet bullet = Instantiate(projectile, firePoint.position, AimTowardsCursor()).GetComponent<Bullet>();
        //bullet.transform.localScale = CalculateModifiedSize(bullet.transform.localScale);
        //bullet.duration = CalculateModifiedDuration(durationTimeMax);

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
        enemy.TakeDamage(damage);
    }
}
