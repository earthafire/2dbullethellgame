using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostPulse : ActivatableAbility
{
    // Start is called before the first frame update
    public GameObject frostpulseobj;
    private GameObject player;
    [SerializeField] private int damage = 5;


    void Start()
    {
        frostpulseobj = (GameObject)Resources.Load("Prefabs/Weapons/FrostPulse", typeof(GameObject));
        player = GameObject.Find("Player");
        base.cooldownTimeMax = 1f;
    }

    public override void Activated()
    {

        Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 difference = mouse_pos - player.transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        Pulse pulse = Instantiate(frostpulseobj, player.transform.position, Quaternion.Euler(0f, 0f, rotationZ)).GetComponent<Pulse>();

        pulse.onEnemyHit += frostHit;
    }

    public void frostHit(Enemy enemy)
    {
        enemy.Damage(damage);
    }
}
