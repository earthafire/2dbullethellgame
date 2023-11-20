using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricSpinManager : ActivatableAbility
{
    public GameObject electricSpinObj;
    private GameObject player;
    private int damage = 10;
    private float knockback = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        electricSpinObj = (GameObject)Resources.Load("Prefabs/Weapons/ElectricSpin/ElectricSpin", typeof(GameObject));
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    private void Hit(Enemy enemy)
    {
        enemy.TakeDamage(damage);
    }
    public override void Activated()
    {
        ElectricSpinInstance instance = Instantiate(electricSpinObj, player.transform.position, Quaternion.identity).GetComponent<ElectricSpinInstance>();
        instance.onEnemyHit += Hit;
    }
}
