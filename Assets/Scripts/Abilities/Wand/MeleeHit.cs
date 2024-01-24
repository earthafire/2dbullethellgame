using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeHit : MonoBehaviour
{
    public float duration = .25f;
    public float offsetDistance = .25f;
    public Action<Enemy> onEnemyHit;
    public GameObject player;
    //private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        //animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.transform.lossyScale.x > 0){
            transform.position = new Vector3(player.transform.position.x + offsetDistance, player.transform.position.y, 1);
        }
        else
        {
            transform.position = new Vector3(player.transform.position.x - offsetDistance, player.transform.position.y, 1);
            GetComponent<SpriteRenderer>().flipX = false;
        }

        duration -= Time.deltaTime;

        if (duration < 0)
        {
            Destroy(gameObject);
           // gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //animator.SetTrigger("Attack");
        try
        {
            if (other.gameObject.layer == 7 || other.gameObject.layer == 9) // Enemy Layer OR Flying Enemy Layer
            {

                Enemy enemy = other.gameObject.GetComponent<Enemy>();
                onEnemyHit?.Invoke(enemy);
            }
        }
        catch (System.NullReferenceException)
        {
            // Object is not enemy
        }
    }
}
