using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class experience : MonoBehaviour
{
    // Start is called before the first frame update

    private System.Random random = new System.Random();
    public Sprite[] skins;

    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = skins[random.Next(7)];
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        try
        {
            PlayerAbilityManager player = other.gameObject.GetComponent<PlayerAbilityManager>();

            // Calling an event that the ability can subscribe to
            if (other.tag == "Player")
            {
                Destroy(gameObject);
            }
        }
        catch (System.NullReferenceException)
        {
            // Object is not an enemy
        }
    }
}
