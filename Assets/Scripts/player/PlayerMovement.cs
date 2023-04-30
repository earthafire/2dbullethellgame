using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float cooldown = 0f;
    public float top_speed = .4f;
    private float acceleration = .2f;
    private float hp = 500;
    public GameObject projectile;

    Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMovement();
        DetectFire1();
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        try
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            takeDamage(enemy.attack);
        }
        catch (System.Exception)
        {
            // if enemy has no attack, do nothing
        }
    }

    void HandleMovement()
    {
        float xIn = Input.GetAxisRaw("Horizontal");
        float yIn = Input.GetAxisRaw("Vertical");
        if (xIn != 0 || yIn != 0) // if an input is active, move the player
        {
            SpriteRenderer sprite = gameObject.GetComponent<SpriteRenderer>();
            if (xIn > 0)
            {

                if (sprite.flipX == true)
                {
                    sprite.flipX = false;
                }
            }
            else
            {
                if (sprite.flipX == false)
                {
                    sprite.flipX = true;
                }
            }
            // remove drag while moving
            rb2d.drag = 0;

            // apply acceleration in direction
            Vector2 direction = Vector2.ClampMagnitude(new Vector2(xIn, yIn), 1);
            rb2d.AddForce(direction * acceleration, ForceMode2D.Impulse);

            // cap out player movement by "top_speed"
            rb2d.velocity = Vector2.ClampMagnitude(rb2d.velocity, top_speed);
        }
        else // stop the player if there are no inputs
        {
            // use drag to stop player
            rb2d.drag = 15;

            // when player is close to not moving, stop completely.
            if (rb2d.velocity.magnitude < .1)
            {
                rb2d.velocity = new Vector2(0, 0);
            }
            // else, clamp to stopping speed and drag takes care of the rest
            else
            {
                rb2d.velocity = Vector2.ClampMagnitude(rb2d.velocity, .8f);
            }
        }
    }

    public void DetectFire1()
    {
        if (cooldown >= 0)
        {
            cooldown -= Time.deltaTime;
        }

        if (Input.GetButton("Fire1") && cooldown <= 0)
        {
            cooldown = .1f;
            fireProjectile();
        }
    }

    public void fireProjectile()
    {
        Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float y = mouse_pos.y - transform.position.y;
        float x = mouse_pos.x - transform.position.x;

        Vector2 y_vec = new Vector2(0, y);
        Vector2 x_vec = new Vector2(x, 0);

        float degrees = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        Weapon bullet = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Weapon>();
        bullet.GetComponent<Rigidbody2D>().rotation = degrees;
    }

    public void setTopSpeed(float new_top_speed)
    {
        this.top_speed = new_top_speed;
        this.acceleration = new_top_speed / 2;
    }

    public float getTopSpeed()
    {
        return top_speed;
    }

    private void takeDamage(int damage)
    {
        hp -= damage;
        Debug.Log(hp);
        if (hp < 1)
        {
            Destroy(gameObject);
        }
    }
}
