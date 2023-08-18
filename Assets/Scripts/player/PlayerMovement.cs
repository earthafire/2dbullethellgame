using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Animator player_animator;
    public float top_speed = .4f;
    public Vector2 direction { get; private set; }
    public bool isPlayerInControl = true;
    private float acceleration = .2f;
    Rigidbody2D rb2d;


    // Start is called before the first frame update
    void Start()
    {
        player_animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMovement();
    }

    private async void WalkingSoundEffect() {
  
            FindObjectOfType<AudioManager>().Play("walk_Grass"); // play movement sound effect      
        
    }

    void HandleMovement()
    {
        if (isPlayerInControl == false)
        {
            player_animator.Play("Dash");
            return;
        }

            player_animator.Play("Character_Walk");

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
            direction = Vector2.ClampMagnitude(new Vector2(xIn, yIn), 1);
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

    public void setTopSpeed(float new_top_speed)
    {
        this.top_speed = new_top_speed;
        this.acceleration = new_top_speed / 2;
    }

    public float getTopSpeed()
    {
        return top_speed;
    }
}
