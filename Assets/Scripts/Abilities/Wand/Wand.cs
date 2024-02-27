using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Wand : ActivatableAbility
{
    public Animator player_animator;
    private int projectilesMax = 3;
    private int projectileIndex = 0;
    private GameObject[] projectiles;
    private Transform firePoint;
    Camera mainCamera;

    public Wand()
    {
        projectiles = new GameObject[projectilesMax];
    }
    void Start()
    {
        firePoint = transform.GetChild(0).GetChild(0).GetChild(0);

        GlobalReferences.firePoint = firePoint;

        mainCamera = Camera.main;

        player_animator = GetComponent<Animator>();

        cooldownTimeMax = 0.75f;

        projectiles[0] = (GameObject)Resources.Load("Prefabs/Abilities/Lightning/Lightning", typeof(GameObject));
        projectiles[1] = (GameObject)Resources.Load("Prefabs/Abilities/Fireball/Fireball", typeof(GameObject));
        projectiles[2] = (GameObject)Resources.Load("Prefabs/Abilities/Wave/Wave", typeof(GameObject));
    }

    public override void Activated()
    {
        player_animator.SetTrigger("Attack");

        Instantiate(projectiles[projectileIndex], firePoint.position, AimTowardsCursor());

        if (projectileIndex < projectilesMax -1)
        {
            projectileIndex++;
        }
        else if(projectileIndex == projectilesMax - 1)
        {
            projectileIndex = 0;
        }
    }

    private Quaternion AimTowardsCursor()
    {
        Vector3 mouse_pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 difference = mouse_pos - player.transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        return Quaternion.Euler(0f,0f, rotationZ);
    }
}
