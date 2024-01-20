using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnipHelicopter: Enemy
{
    private Vector3 _startPosition;

    new void Start()
    {
        base.Start();
        _startPosition = transform.position;

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
        base.Move();
        //transform.position = _startPosition + new Vector3(0.0f, Mathf.Sin(Time.time), 0.0f);
    }

    override public IEnumerator GetDeath()
    {
        GameObject shadow = transform.GetChild(0).gameObject;
        shadow.SetActive(false);

        yield return base.GetDeath();
    }
}
