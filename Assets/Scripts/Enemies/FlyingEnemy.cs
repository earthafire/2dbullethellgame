using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy: Enemy
{
    GameObject shadow;
    SpriteRenderer shadowSpriteRenderer;
    new void Start()
    {
        shadow = transform.GetChild(0).gameObject;
        shadowSpriteRenderer = shadow.GetComponent<SpriteRenderer>();
        base.Start();
    }

    new void FixedUpdate()
    {
        if ( suspendActions ){ return; }

        base.FixedUpdate();
    }

    override public IEnumerator GetDeath()
    {
        HideShadow();
        yield return base.GetDeath();
    }
    
    private void HideShadow()
    {
        shadowSpriteRenderer.color = new Color(1, 1, 1, 0);
    }
}
