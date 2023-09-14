using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagItem : InteractableLoot
{
    // Start is called before the first frame update
    void Start()
    {
        isReady = false;
        float flingStrength = 3f;
        Rigidbody2D rb2d = gameObject.GetComponent<Rigidbody2D>();
        rb2d.AddForce(Random.insideUnitCircle.normalized * flingStrength, ForceMode2D.Impulse);

        StartCoroutine(setPickupableLater(.75f));
    }

    // Allow pickup after 'seconds'
    IEnumerator setPickupableLater(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        isReady = true;
    }

    public override void OnPickUp(GameObject playerObject)
    {
        Destroy(gameObject);
    }
        private void OnTriggerStay2D(Collider2D other)
    {       
        if(other.gameObject.layer == 8) // Experience Layer
        {
            transform.position = Vector3.MoveTowards(transform.position, other.gameObject.transform.position, 5 * Time.deltaTime);
        }
    }
}
