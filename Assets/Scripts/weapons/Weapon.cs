using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    // Duration in seconds
    private float duration = 2f;
    private int damage = 100;
    Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // check if object should be destroyed
        if (duration >= 0)
        {
            duration -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // void Move()
    // {
    //     transform.Translate(Vector3.right * Time.deltaTime * speed);
    // }

    void DealDamage(GameObject target)
    {
        target.GetComponent<Enemy>().Damage(damage);
    }
}
