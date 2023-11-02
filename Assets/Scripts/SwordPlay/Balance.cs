using FIMSpace.FProceduralAnimation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balance : MonoBehaviour
{
    public Transform body;
    private void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, Quaternion.LookRotation(body.position - transform.position).eulerAngles.y, 0));
    }
}
