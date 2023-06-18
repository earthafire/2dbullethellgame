using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float top_speed = .4f;
    private float acceleration = .2f;
    private float hp = 500;
    public Weapon weapon;
    Rigidbody2D rb2d;
    public HealthBar healthbar;


    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        weapon = GetComponent<Weapon>();
        healthbar.SetMaxHealth(hp);
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
    private async void WalkingSoundEffect() {
  
            FindObjectOfType<AudioManager>().Play("walk_Grass"); // play movement sound effect      
        
    }

    void HandleMovement()
    {
        float xIn = Input.GetAxisRaw("Horizontal");
        float yIn = Input.GetAxisRaw("Vertical");
        if (xIn != 0 || yIn != 0) // if an input is active, move the player
        {
            WalkingSoundEffect();

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
        if (Input.GetButton("Fire1"))
        {
            weapon.Activate();
        }
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
        healthbar.SetHealth(hp);
        if (hp < 1)
        {
            Destroy(gameObject);
        }
    }
}
