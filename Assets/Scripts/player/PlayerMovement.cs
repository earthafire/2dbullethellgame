using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Animator player_animator;
    public Attributes attributes;
    SoundComponent sound;
    public Vector2 direction { get; private set; }
    public bool isPlayerInControl = true;
    Rigidbody2D rb2d;


    // Start is called before the first frame update
    void Start()
    {
        sound = GetComponent<SoundComponent>();
        player_animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {

        if (isPlayerInControl == false)
        {
            sound.sfxToPlay.PlaySFX(); 
            return;
        }
        float acceleration = attributes.GetAttribute(Attribute.acceleration);
        float speed = attributes.GetAttribute(Attribute.moveSpeed);

        float xIn = Input.GetAxisRaw("Horizontal");
        float yIn = Input.GetAxisRaw("Vertical");

         // if an input is active, move the player
        if (xIn != 0 || yIn != 0)
        {   
            player_animator.SetBool("Run", true);

            SpriteRenderer sprite = gameObject.GetComponent<SpriteRenderer>();
            
            if (xIn > 0)
            {
                transform.localScale = Vector3.one;
            }
            else
            {
                transform.localScale = new Vector3(-1,1,0);
            }
            // remove drag while moving
            rb2d.drag = 0;

            // apply acceleration in direction
            direction = Vector2.ClampMagnitude(new Vector2(xIn, yIn), 1);
            rb2d.AddForce(direction * acceleration, ForceMode2D.Impulse);

            // cap out player movement by "top_speed"
            rb2d.velocity = Vector2.ClampMagnitude(rb2d.velocity, speed);

        }
        else // stop the player if there are no inputs
        {
            player_animator.SetBool("Run", false);

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
}
