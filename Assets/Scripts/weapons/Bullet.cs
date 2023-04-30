using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    private float lifetime_seconds = 1.5f;
    public float speed = 1f;
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
        if (lifetime_seconds >= 0)
        {
            lifetime_seconds -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }

        transform.Translate(Vector3.right * Time.deltaTime * speed);
    }

    void DealDamage(GameObject target)
    {

    }
}
