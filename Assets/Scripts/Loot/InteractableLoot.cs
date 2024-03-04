using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableLoot : MonoBehaviour
{
    public Boolean isReady { get; set; } = true;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 12 && isReady) // Player layer
        {
            isReady = false;
            OnPickUp(other.gameObject);
        }

    }

    public abstract void OnPickUp(GameObject playerObject);
}
