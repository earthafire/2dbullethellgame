using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Explosion : AbilityObject
{
    public override float damage { get; set; } = 10f;
    public override float duration { get; set; } = .3f;
}
