using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnipHelicopter: Enemy
{
    private Vector3 _startPosition;
    GameObject shadow;

    new void Start()
    {
        base.Start();
        _startPosition = transform.position;
        shadow = transform.GetChild(0).gameObject;

    }

    new void Update()
    {
        base.Update();
        if (suspendActions)
        {
            return;
        }
        TurnipHelicopterMove();
    }

    void TurnipHelicopterMove()
    {
        Move();
        //transform.position = _startPosition + new Vector3(0.0f, Mathf.Sin(Time.time), 0.0f);
    }

    override public IEnumerator GetDeath()
    {
       
        shadow.SetActive(false);

        yield return base.GetDeath();
    }
}
