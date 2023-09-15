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
        PlayerInventory inventory = playerObject.GetComponent<PlayerInventory>();
        Item item = GetComponent<Item>();
        inventory.inventory.AddItem(item.item, 1);
        Destroy(gameObject);
    }
}
