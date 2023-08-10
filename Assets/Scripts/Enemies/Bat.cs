using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : Enemy
{
    private Rigidbody2D rb;
    private Transform sprite_renderer_transform;
    public float speed = .5f, rotationSpeed = .2f;
    private Vector3 target_position;
    public experience xp;
    public int xp_tier;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        sprite_renderer_transform = this.transform;
        rb = GetComponent<Rigidbody2D>();
        target_position = base.target.transform.position;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        RotateTowardsTarget();
        Move();
        LockSpriteRotation();
    }

    void Move()
    {
        if (target == null)
        {
            return;
        }

        target_position = base.target.transform.position;
        Vector3 forceDirection = transform.forward;
        float force = speed * Time.deltaTime;
        transform.Translate(Vector3.right * Time.deltaTime * speed);

        //rb.AddForce(Vector2.right, ForceMode2D.Force);
        //transform.position = Vector3.MoveTowards(transform.position, target_position, force);
        //base.rb2d.velocity = Vector2.zero;
    }

    void RotateTowardsTarget()
     {
        // Calculate the direction from the current position to the target position
        Vector3 targetDirection = target_position - transform.position;

        // Calculate the angle in radians
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

        // Smoothly rotate towards the target angle over time using Mathf.LerpAngle
        float newAngle = Mathf.LerpAngle(transform.eulerAngles.z, angle, rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, 0, newAngle);

        float oppositeAngle = transform.eulerAngles.z + 180.0f;
        sprite_renderer_transform.eulerAngles = new Vector3(0, 0, oppositeAngle);
    }

    void LockSpriteRotation()
    {
        sprite_renderer_transform.localRotation = Quaternion.identity;
        
    }

    override public IEnumerator Kill()
    {
        experience new_loot = Instantiate(xp, this.transform.position, Quaternion.identity).GetComponent<experience>();
        new_loot.SetTier(xp_tier);
        yield return base.Kill();
    }
}
