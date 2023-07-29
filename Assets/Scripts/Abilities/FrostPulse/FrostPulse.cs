using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostPulse : ActivatableAbility
{
    // Start is called before the first frame update
    void Start()
    {
        Animator m_Animator = gameObject.GetComponent<Animator>();
        m_Animator.speed = .25f;

        base.cooldownTimeMax = .2f;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Activated()
    {

    }
}
