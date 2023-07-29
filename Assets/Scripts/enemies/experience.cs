using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class experience : MonoBehaviour
{
    // Start is called before the first frame update

    private System.Random random = new System.Random();
    private int experienceAmount = 1;
    public Sprite[] tier1SkinsArray;
    public Sprite[] tier2SkinsArray;

    public void Start()
    {
        Debug.Log(this.transform.position);
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 1);
        Debug.Log(this.transform.position);
    }

    public void SetTier(int tierNum)
    {

        if (tierNum == 1)
        {
            experienceAmount = 1;
            GetComponent<SpriteRenderer>().sprite = tier1SkinsArray[random.Next(tier1SkinsArray.Length)];
        }
        else if (tierNum == 2)
        {
            experienceAmount = 5;
            GetComponent<SpriteRenderer>().sprite = tier2SkinsArray[random.Next(tier2SkinsArray.Length)];
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        try
        {
            PlayerAttributes player = other.gameObject.GetComponent<PlayerAttributes>();

            // Calling an event that the ability can subscribe to
            if (other.tag == "Player")
            {
                player.addExperience(experienceAmount);
                Destroy(gameObject);
            }
        }
        catch (System.NullReferenceException)
        {
            // Object is not an enemy
        }
    }
}
