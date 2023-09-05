using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagController : MonoBehaviour
{
    public GameObject bag;
    public GameObject item;
    private ParticleSystem particles;

public void Start()
    {
        particles = gameObject.GetComponent<ParticleSystem>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.layer == 12) // Player layer
        {
            Instantiate(item, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }



 
}
