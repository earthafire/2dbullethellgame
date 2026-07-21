using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Animator player_animator;
    SoundComponent sound;
    public Vector2 direction { get; private set; }
    public bool isPlayerInControl = true;
    Rigidbody2D rb2d;
    PlayerAttributes playerAttributes;


    // Start is called before the first frame update
    void Start()
    {
        sound = GetComponent<SoundComponent>();
        player_animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        playerAttributes = GetComponent<PlayerAttributes>();
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
        float acceleration = PlayerAttributes.stats[Attribute.acceleration];
        float speed = PlayerAttributes.stats[Attribute.moveSpeed] * playerAttributes.speedBoostMultiplier;

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
            rb2d.linearDamping = 0;

            // apply acceleration in direction
            direction = Vector2.ClampMagnitude(new Vector2(xIn, yIn), 1);
            rb2d.AddForce(direction * acceleration, ForceMode2D.Impulse);

            // cap out player movement by "top_speed"
            rb2d.linearVelocity = Vector2.ClampMagnitude(rb2d.linearVelocity, speed);

        }
        else // stop the player if there are no inputs
        {
            player_animator.SetBool("Run", false);

            // use drag to stop player
            rb2d.linearDamping = 15;

            // when player is close to not moving, stop completely.
            if (rb2d.linearVelocity.magnitude < .1)
            {
                rb2d.linearVelocity = new Vector2(0, 0);
            }
            // else, clamp to stopping speed and drag takes care of the rest
            else
            {
                rb2d.linearVelocity = Vector2.ClampMagnitude(rb2d.linearVelocity, .8f);
            }
        }
    }
}
