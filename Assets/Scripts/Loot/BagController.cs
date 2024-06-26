using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class BagController : InteractableLoot
{
    [SerializeField] private List<GameObject> lootPool;

    private ParticleSystem particles;
    private Random random = new Random();
    private int lootCount = 1;
    public SoundComponent sound;


    public void Start()
    {
        particles = gameObject.GetComponent<ParticleSystem>();
    }

    public override void OnPickUp(GameObject playerObject)
    {
        gameObject.GetComponent<CircleCollider2D>().enabled = false;
        StartCoroutine(SpewItems(playerObject));
    }

    // spawns in all the bag's contents, delay between each
    IEnumerator SpewItems(GameObject playerObject)
    {

        particles.Play();
        sound.sfxToPlay.PlaySFX();

        // maybe play an animation here?
        yield return new WaitForSeconds(15);
        

        // spew out items
        for (int i = 0; i < lootCount; i++)
        {
            GameObject newItem = Instantiate(PickRandomLoot(), gameObject.transform.position, Quaternion.identity);
            yield return new WaitForSeconds(.25f);
        }

        Destroy(gameObject);
    }

    private GameObject PickRandomLoot()
    {
        return lootPool[random.Next(lootPool.Count)]; ;
    }
}
