using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitVfx : MonoBehaviour
{
    void Start()
    {
        if (PlayerWorker.player && EnemyWorker.psword)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z));
            Vector3 v1 = Quaternion.Euler(new Vector3(-PlayerWorker.player.rotation.eulerAngles.x, -PlayerWorker.player.rotation.eulerAngles.y, -PlayerWorker.player.rotation.eulerAngles.z)) * EnemyWorker.psword.GetComponent<Rigidbody>().velocity;
            v1 = new Vector3(v1.x, v1.y, 0);
            float r1 = Mathf.Atan2(v1.y, v1.x) * Mathf.Rad2Deg;
            transform.RotateAround(transform.position, transform.forward, r1);
        }
    }
}
