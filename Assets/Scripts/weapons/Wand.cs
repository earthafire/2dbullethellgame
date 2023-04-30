using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wand : Weapon
{

    private GameObject projectile;


    // Start is called before the first frame update
    new void Start()
    {
        projectile = (GameObject)Resources.Load("Prefabs/Weapons/Bullet", typeof(GameObject));
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

    }

    public override void Activated()
    {
        Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 difference = mouse_pos - transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        Bullet bullet = Instantiate(projectile, transform.position, Quaternion.Euler(0f, 0f, rotationZ)).GetComponent<Bullet>();
        bullet.weapon = this;
    }
}
