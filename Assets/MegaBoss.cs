using System;
using System.Collections;
using UnityEngine;

public class MegaBoss : FlyingEnemy
{
    GameObject projectile;
    [SerializeField] float fireDistance = 2;
    Transform firePoint;

    new void Start()
    {
        base.Start();
        projectile = (GameObject)Resources.Load("Prefabs/Enemies/Projectile/Projectile", typeof(GameObject));
        firePoint = transform.GetChild(0);
    }

    private bool InRange()
    {
        if (Vector2.Distance(player.transform.position, transform.position) < fireDistance)
        {
            return true;
        }
        return false;
    }

    public void Fire()
    {
        if (InRange())
        {
            Instantiate(projectile, firePoint.position, AimTowardsPlayer(), firePoint.transform);
        }
    }

    private Quaternion AimTowardsPlayer()
    {
        Vector3 difference = player.transform.position - this.transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0f, 0f, rotationZ);
    }
}
