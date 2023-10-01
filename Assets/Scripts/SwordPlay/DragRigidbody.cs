using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragRigidbody : MonoBehaviour
{
    public float forceAmount = 500;
    Rigidbody rigid;
    public Transform sword;

    void Update()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rigid)
        {
            rigid.velocity = (sword.position - rigid.transform.position) * forceAmount * Time.deltaTime;
            rigid.transform.rotation = sword.rotation;
        }
    }
}
