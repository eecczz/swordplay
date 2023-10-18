using FIMSpace.FProceduralAnimation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balance : MonoBehaviour
{
    public Transform body;

    private void Update()
    {
        transform.position = body.position - Vector3.up * 1;
        GetComponent<LegsAnimator>().UseGluing = false;
    }
}
