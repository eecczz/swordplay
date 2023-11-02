using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class TargetMatching : RigidbodyMatchingBase
{
    [Header("Target")]
    public Transform Target;

    protected override void FixedUpdate()
    {
        SetTargetPlacement(Target);

        base.FixedUpdate();
    }
}
