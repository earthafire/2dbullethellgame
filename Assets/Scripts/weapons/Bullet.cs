using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Weapon
{
    public float speed = 1f;

    // Start is called before the first frame update
    public void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public void Update()
    {
        base.Update();
        Move();
    }

    public void Move()
    {
        transform.Translate(Vector3.right * Time.deltaTime * speed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("TRIGGER");
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        try
        {
            enemy.Damage(base.damage);
        }
        catch (System.NullReferenceException)
        {
            // Do nothing
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("TRIGGER2");
    }
}
