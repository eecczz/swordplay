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

    protected override void UpdateRotation()
    {
        base.UpdateRotation();
        float angle = Quaternion.Angle(GetComponent<Rigidbody>().rotation, Target.rotation);
        if (angle < 10f && GetComponent<Rigidbody>().angularVelocity.magnitude < 0.1f && (GetComponent<EnemyMotion>()!=null || GetComponent<PlayerMotion>() != null))
        {
            GetComponent<Animator>().applyRootMotion = true;
            GetComponent<NavMeshAgent>().enabled = true;
            GetComponent<RigBuilder>().enabled = true;
        }
    }
}
