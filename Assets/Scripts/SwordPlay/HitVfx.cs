using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitVfx : MonoBehaviour
{
    void Start()
    {
        transform.rotation = Quaternion.LookRotation(new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z));
        Vector2 v1 = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).normalized;
        float r1 = Mathf.Atan2(v1.y, v1.x) * Mathf.Rad2Deg;
        transform.RotateAround(transform.position, transform.forward, r1);
        Debug.Log(r1);
    }
}
