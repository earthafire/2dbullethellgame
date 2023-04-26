using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletMovement : MonoBehaviour
{

    private float lifetime = 5f;
    public float speed = 1f;
    Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();

        Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float y = mouse_pos.y - transform.position.y;
        float x = mouse_pos.x - transform.position.x;

        Vector2 y_vec = new Vector2(0, y);
        Vector2 x_vec = new Vector2(x, 0);

        Debug.Log("dif y: " + y + " x: " + x);
        Debug.Log("mouse y: " + mouse_pos.y + " x: " + mouse_pos.x);
        Debug.Log("object y: " + transform.position.y + " x: " + transform.position.x);


        float degrees = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        Debug.Log(degrees);
        rb2d.rotation = degrees;

        Debug.Log(rb2d.rotation);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb2d.AddForce(transform.right * speed);
    }
}
