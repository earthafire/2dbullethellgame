using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagController : InteractableLoot
{
    //public GameObject item;

    public List<GameObject> contents;
    private ParticleSystem particles;

    public void Start()
    {
        //contents = new List<GameObject>();
        //contents.Add(item);
        particles = gameObject.GetComponent<ParticleSystem>();
    }

    public override void OnPickUp(GameObject playerObject)
    {
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        StartCoroutine(SpewItems(playerObject));
    }

    // spawns in all the bag's contents, delay between each
    IEnumerator SpewItems(GameObject playerObject)
    {
        // maybe play an animation here?
        yield return new WaitForSeconds(.2f);


        // spew out items
        for (int i = 0; i < contents.Count; i++)
        {
            GameObject newItem = Instantiate(contents[i], gameObject.transform.position, Quaternion.identity);
            yield return new WaitForSeconds(.25f);
        }
        Destroy(gameObject);
    }
}
