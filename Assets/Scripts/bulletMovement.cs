using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletMovement : MonoBehaviour
{

    private float lifetime_seconds = 1.5f;
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

        float degrees = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        rb2d.rotation = degrees;
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
}
