using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitVfx : MonoBehaviour
{
    void Start()
    {
        transform.rotation = Quaternion.LookRotation(new Vector3(-Camera.main.transform.forward.x, 0, -Camera.main.transform.forward.z));
        Vector2 v1 = new Vector2(EnemyMotion.etx - EnemyMotion.ertx, EnemyMotion.ety - EnemyMotion.erty).normalized;
        float r1 = Mathf.Atan2(v1.y, v1.x) * Mathf.Rad2Deg + 180;
        transform.RotateAround(transform.position, transform.forward, r1);
    }
}
