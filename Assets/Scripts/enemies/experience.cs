using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class experience : MonoBehaviour
{
    // Start is called before the first frame update

    private System.Random random = new System.Random();
    private int experienceAmount = 1, speed = 5;
    public Sprite[] tier1SkinsArray,tier2SkinsArray,
                    tier3SkinsArray,tier4SkinsArray,
                    tier5SkinsArray;

    public void Start()
    {
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 1);
    }

    public void SetTier(int tierNum)
    {
        switch(tierNum){

            case 1:
                experienceAmount = 1;
                GetComponent<SpriteRenderer>().sprite = tier1SkinsArray[random.Next(tier1SkinsArray.Length)];
                break;

            case 2:
                experienceAmount = 3;
                GetComponent<SpriteRenderer>().sprite = tier2SkinsArray[random.Next(tier2SkinsArray.Length)];
                break;

            case 3:
                experienceAmount = 5;
                GetComponent<SpriteRenderer>().sprite = tier3SkinsArray[random.Next(tier3SkinsArray.Length)];
                break;

            case 4:
                experienceAmount = 8;
                GetComponent<SpriteRenderer>().sprite = tier4SkinsArray[random.Next(tier4SkinsArray.Length)];
                break;

            case 5:
                experienceAmount = 12;
                GetComponent<SpriteRenderer>().sprite = tier5SkinsArray[random.Next(tier5SkinsArray.Length)];    
                break;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
            // Calling an event that the ability can subscribe to
            
            if(other.tag == "ExperiencePickUpRange"){
                transform.position = Vector3.MoveTowards(transform.position, other.gameObject.transform.position, speed * Time.deltaTime);
            }
        
    }

    private void OnTriggerEnter2D(Collider2D other){
        Debug.Log(other.tag);
            if (other.tag == "Player")
            {
                PlayerAttributes player = other.gameObject.GetComponent<PlayerAttributes>();
                player.addExperience(experienceAmount);
                Debug.Log(1);
                Destroy(gameObject);
            }
    }
    
}
