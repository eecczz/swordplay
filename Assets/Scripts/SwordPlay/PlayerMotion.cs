using FIMSpace.FProceduralAnimation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using XftWeapon;

public class PlayerMotion : MonoBehaviour
{
    public Transform jointParent, joint;
    Collider sword;
    Animator anim;
    NavMeshAgent nav;
    public float sensitivity = 5;
    public float canSwing = 1;
    public static float tx, ty;
    public static int swing, guard;
    public static bool lr;
    public AudioClip[] clips;
    public GameObject hitVFX, shieldVFX;
    public static GameObject ent;
    bool onTarget;
    int sign;
    Vector3 vec, knockBack;
    Rigidbody rigid;
    int health = 2;
    RigBuilder rb;
    public MultiAimConstraint ma;

    private void Start()
    {
        Cursor.visible = false;
        anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        sword = jointParent.GetComponentInChildren<Collider>();
        rb = GetComponent<RigBuilder>();
    }

    public GameObject FindClosestEnemy()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                ent = closest;
                distance = curDistance;
                if (closest != null)
                {
                    var data = ma.data.sourceObjects;
                    data.SetTransform(0, closest.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head));
                    ma.data.sourceObjects = data;
                    rb.Build();
                }
                else
                {
                    Destroy(ma);
                }
            }
        }
        return closest;
    }

    private void Update()
    {
        if (anim.enabled && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && health > -1)
            GetComponent<LegsAnimator>().UseGluing = true;
        else
            GetComponent<LegsAnimator>().UseGluing = false;
        if (health > -1)
        {
            //sword movement
            if (!Input.GetMouseButton(1))
            {
                tx += Input.GetAxis("Mouse X") * sensitivity;
                ty += Input.GetAxis("Mouse Y") * sensitivity;
            }
            else
            {
                tx -= Input.GetAxis("Mouse X") * sensitivity;
                ty -= Input.GetAxis("Mouse Y") * sensitivity;
            }
            if (guard == 0)
            {
                tx = Mathf.Clamp(tx, -180, 180);
                ty = Mathf.Clamp(ty, -60, 150);
            }
            else
            {
                tx = Mathf.Clamp(tx, -90, 90);
                ty = Mathf.Clamp(ty, -60, 90);
            }
        }
        if (!Input.GetMouseButton(1) && health > -1)
        {
            if (guard == 1)
            {
                sword.transform.localPosition = new Vector3(0, -1.25f, 1.5f);
                guard = 0;
            }
            joint.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            joint.localPosition = new Vector3(0, 1.5f, 0);
            joint.RotateAround(joint.position + transform.up * 1.25f, transform.right, -ty);
            sword.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, ty * tx / 120));
            Vector3 v3 = Quaternion.AngleAxis(-ty * tx / 120, transform.forward) * transform.up * 1.25f * Mathf.Clamp(Mathf.Abs(tx) / 120, 0, 1);
            joint.RotateAround(joint.position + v3, sword.transform.up, tx);
            sword.transform.localPosition = new Vector3(0, 0, 1.5f);
        }
        else if (health > -1)
        {
            if (guard == 0)
            {
                guard = 1;
                SoundManager.Instance.SFXPlay("Shield", clips[1]);
            }
            joint.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            joint.localPosition = new Vector3(0, 1.5f, 0);
            joint.RotateAround(joint.position + transform.up * 1.25f, transform.right, -ty);
            sword.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, ty * tx / 120));
            Vector3 v3 = Quaternion.AngleAxis(-ty * tx / 120, transform.forward) * transform.up * 1.25f * Mathf.Clamp(Mathf.Abs(tx) / 120, 0, 1);
            joint.RotateAround(joint.position + v3, sword.transform.up, tx);
            sword.transform.localPosition = new Vector3(0, 0, 1.5f);
            sword.transform.position = transform.position + transform.up * 2 + transform.forward * 1.5f + transform.right * -tx / 90 + transform.up * -ty / 100;
        }
        //animation parameter settings
        anim.SetBool("leftStep", lr);
        if (ent == null)
            anim.SetFloat("dis", 1000);
        else
            anim.SetFloat("dis", (ent.transform.position - transform.position).magnitude);
        if (health > -1 && (new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"))).magnitude > canSwing && guard == 0 && swing == 0 && lr && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Guarded")) //������
        {
            anim.SetBool("leftStep", lr);
            anim.CrossFade("Attack", 0, 0);
            lr = false;
            SoundManager.Instance.SFXPlay("Swing", clips[0]);
        }
        else if (health > -1 && (new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"))).magnitude > canSwing && guard == 0 && swing == 0 && !lr && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Guarded")) //������
        {
            anim.SetBool("leftStep", lr);
            anim.CrossFade("Attack", 0, 0);
            lr = true;
            SoundManager.Instance.SFXPlay("Swing", clips[0]);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && ent != null && !ent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Hurted") && health > -1)
        {
            if (swing == 0)
            {
                swing = 1;
                Physics.IgnoreLayerCollision(7, 8, false);
            }
        }
        else if (health > -1)
        {
            if (swing == 1)
            {
                swing = 0;
                Physics.IgnoreLayerCollision(7, 8, true);
            }
        }
        if (!onTarget)
        {
            nav.speed = 0;
            if (FindClosestEnemy() != null)
                onTarget = true;
        }
        if (onTarget)
        {
            nav.speed = 3.5f;
            if (ent == null)
            {
                onTarget = false;
            }
            if (ent != null)
            {
                if (nav.enabled && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Guarded") && swing == 0)
                    nav.SetDestination(ent.transform.position);
                if (ent.tag == "Untagged")
                    onTarget = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (ent != null && health > -1)
            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, Quaternion.LookRotation(ent.transform.position - transform.position).eulerAngles.y, transform.rotation.eulerAngles.z));
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && ent != null && ent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Hurted") && health > -1)
            GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && health > -1)
        {
            Vector3 poslleg = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position;
            Vector3 posrleg = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).position;
            Quaternion rotlleg = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).rotation;
            Quaternion rotrleg = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).rotation;
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                anim.GetBoneTransform(HumanBodyBones.Hips).localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            //Head IK fix
            if (ent != null && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                anim.GetBoneTransform(HumanBodyBones.Head).rotation = Quaternion.Euler(new Vector3(anim.GetBoneTransform(HumanBodyBones.Head).rotation.eulerAngles.x, anim.GetBoneTransform(HumanBodyBones.Head).rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
            else
                anim.GetBoneTransform(HumanBodyBones.Head).rotation = transform.rotation;
            anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).rotation = rotlleg;
            anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).rotation = rotrleg;
            anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position = poslleg;
            anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).position = posrleg;
        }
        else if (health > -1)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.1f)
                transform.position += knockBack * 0.1f;
        }
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Guarded"))
        {
            GetComponentInChildren<Rig>().weight = 1;
        }
        else
        {
            GetComponentInChildren<Rig>().weight = 0;
        }
        //manual parenting
        if (health>-1)
        {
            jointParent.position = transform.position;
            jointParent.rotation = transform.rotation;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 9 && ent != null && !ent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Guarded"))
        {
            if (guard == 0 && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted"))
            {
                if (health > 0)
                {
                    health--;
                    anim.CrossFade("Hurted", 0, 0);
                    GameObject hit = Instantiate(hitVFX, collision.contacts[0].point, Quaternion.LookRotation(anim.GetBoneTransform(HumanBodyBones.Neck).position - collision.contacts[0].point));
                    Destroy(hit, 0.3f);
                    if ((hit.transform.position - anim.GetBoneTransform(HumanBodyBones.LeftShoulder).position).magnitude > (hit.transform.position - anim.GetBoneTransform(HumanBodyBones.RightShoulder).position).magnitude)
                        anim.SetBool("leftHit", true);
                    else
                        anim.SetBool("leftHit", false);
                    knockBack = hit.transform.forward;
                    vec = new Vector3(hit.transform.right.x, 0, hit.transform.right.z).normalized;
                    SoundManager.Instance.SFXPlay("Hit", clips[2]);
                    Physics.IgnoreLayerCollision(6, 9, true); //prevent natural collision forced until hurt is 0
                    Physics.IgnoreLayerCollision(7, 8, true);
                }
                else
                {
                    health = -1;
                    anim.SetBool("died", true);
                    GetComponentInChildren<Rig>().weight = 0;
                    GetComponentInChildren<Collider>().transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0));
                    nav.enabled = false;
                    GameObject hit = Instantiate(hitVFX, collision.contacts[0].point, Quaternion.LookRotation(anim.GetBoneTransform(HumanBodyBones.Neck).position - collision.contacts[0].point));
                    Destroy(hit, 0.3f);
                    if (hit.transform.position.y > anim.GetBoneTransform(HumanBodyBones.Neck).position.y)
                        sign = 1;
                    else
                        sign = -1;
                    knockBack = hit.transform.forward;
                    vec = new Vector3(hit.transform.right.x, 0, hit.transform.right.z).normalized;
                    Destroy(GetComponent<Rigidbody>());
                    Destroy(GetComponent<Collider>());
                    GetComponent<Animator>().enabled = false;
                    foreach (Collider collider in GetComponentsInChildren<Collider>())
                    {
                        collider.isTrigger = false;
                        collider.enabled = true;
                        if (!collider.gameObject.GetComponent<Rigidbody>())
                        {
                            rigid = collider.gameObject.AddComponent<Rigidbody>();
                            rigid.velocity = Vector3.zero;
                            rigid.angularVelocity = Vector3.zero;
                            collider.GetComponent<Rigidbody>().AddForce((knockBack + Vector3.up) * 5, ForceMode.Impulse);
                            collider.GetComponent<Rigidbody>().AddTorque(vec * sign * 5, ForceMode.Impulse);
                            ConfigurableJoint joint = collider.gameObject.AddComponent<ConfigurableJoint>();
                            if (collider.gameObject.name != "Hips")
                            {
                                joint.xMotion = ConfigurableJointMotion.Locked;
                                joint.yMotion = ConfigurableJointMotion.Locked;
                                joint.zMotion = ConfigurableJointMotion.Locked;
                                joint.angularXMotion = ConfigurableJointMotion.Limited;
                                joint.angularYMotion = ConfigurableJointMotion.Limited;
                                joint.angularZMotion = ConfigurableJointMotion.Limited;
                                JointDrive drive = new JointDrive();
                                drive.positionSpring = 1000;
                                drive.positionDamper = 1000;
                                joint.angularXDrive = drive;
                                joint.angularYZDrive = drive;
                                var limit0 = joint.lowAngularXLimit;
                                limit0.limit = -60;
                                joint.lowAngularXLimit = limit0;
                                var limit = joint.highAngularXLimit;
                                limit.limit = 60;
                                joint.highAngularXLimit = limit;
                                var limit1 = joint.angularYLimit;
                                limit1.limit = 60;
                                joint.angularYLimit = limit1;
                                var limit2 = joint.angularYLimit;
                                limit2.limit = 60;
                                joint.angularZLimit = limit2;
                                Transform rigidParent = joint.transform.parent;
                                for (int i = 0; i < 5; i++)
                                {
                                    if (rigidParent.GetComponent<Rigidbody>())
                                    {
                                        joint.connectedBody = rigidParent.GetComponent<Rigidbody>();
                                        break;
                                    }
                                    else
                                    {
                                        rigidParent = rigidParent.transform.parent;
                                    }
                                }
                            }
                            else
                            {
                                jointParent.parent = collider.transform;
                                Physics.IgnoreLayerCollision(3, 8, false);
                            }
                        }
                    }
                    SoundManager.Instance.SFXPlay("Hit", clips[2]);
                    foreach (XWeaponTrail xw in jointParent.GetComponentsInChildren<XWeaponTrail>())
                        xw.MaxFrame = 0;
                    GetComponent<LegsAnimator>().enabled = false;
                    EnemyMotion.player = null;
                    Camera.main.transform.parent = null;
                    anim.GetBoneTransform(HumanBodyBones.Hips).parent = null;
                    transform.parent = anim.GetBoneTransform(HumanBodyBones.Hips);
                    Physics.IgnoreLayerCollision(6, 9, true); //prevent natural collision forced until hurt is 0
                    Physics.IgnoreLayerCollision(7, 8, true);
                    Invoke("Dissolve", 4);
                    Destroy(jointParent.gameObject, 5f);
                    Destroy(gameObject, 5f);
                }
            }
            else if (guard == 1)
            {
                Vector2 v = joint.localRotation * Vector3.up;
                float r = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + 180;
                Vector2 v1 = EnemyMotion.ejoint.localRotation * Vector3.up;
                float r1 = Mathf.Atan2(v1.y, -v1.x) * Mathf.Rad2Deg + 180;
                float angle = Mathf.Abs(r - r1);
                if (angle < 30 || angle > 345 || (angle > 150 && angle < 210))
                {
                    if (health > 0)
                    {
                        health--;
                        anim.CrossFade("Hurted", 0, 0);
                        GameObject hit = Instantiate(hitVFX, collision.contacts[0].point, Quaternion.LookRotation(anim.GetBoneTransform(HumanBodyBones.Neck).position - collision.contacts[0].point));
                        Destroy(hit, 0.3f);
                        if ((hit.transform.position - anim.GetBoneTransform(HumanBodyBones.LeftShoulder).position).magnitude > (hit.transform.position - anim.GetBoneTransform(HumanBodyBones.RightShoulder).position).magnitude)
                            anim.SetBool("leftHit", true);
                        else
                            anim.SetBool("leftHit", false);
                        knockBack = hit.transform.forward;
                        vec = new Vector3(hit.transform.right.x, 0, hit.transform.right.z).normalized;
                        SoundManager.Instance.SFXPlay("Hit", clips[2]);
                        Physics.IgnoreLayerCollision(6, 9, true); //prevent natural collision forced until hurt is 0
                        Physics.IgnoreLayerCollision(7, 8, true);
                    }
                    else
                    {
                        health = -1;
                        anim.SetBool("died", true);
                        GetComponentInChildren<Rig>().weight = 0;
                        GetComponentInChildren<Collider>().transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0));
                        nav.enabled = false;
                        GameObject hit = Instantiate(hitVFX, collision.contacts[0].point, Quaternion.LookRotation(anim.GetBoneTransform(HumanBodyBones.Neck).position - collision.contacts[0].point));
                        Destroy(hit, 0.3f);
                        if (hit.transform.position.y > anim.GetBoneTransform(HumanBodyBones.Neck).position.y)
                            sign = 1;
                        else
                            sign = -1;
                        knockBack = hit.transform.forward;
                        vec = new Vector3(hit.transform.right.x, 0, hit.transform.right.z).normalized;
                        Destroy(GetComponent<Rigidbody>());
                        Destroy(GetComponent<Collider>());
                        GetComponent<Animator>().enabled = false;
                        foreach (Collider collider in GetComponentsInChildren<Collider>())
                        {
                            collider.isTrigger = false;
                            collider.enabled = true;
                            if (!collider.gameObject.GetComponent<Rigidbody>())
                            {
                                rigid = collider.gameObject.AddComponent<Rigidbody>();
                                rigid.velocity = Vector3.zero;
                                rigid.angularVelocity = Vector3.zero;
                                collider.GetComponent<Rigidbody>().AddForce((knockBack + Vector3.up) * 5, ForceMode.Impulse);
                                collider.GetComponent<Rigidbody>().AddTorque(vec * sign * 5, ForceMode.Impulse);
                                ConfigurableJoint joint = collider.gameObject.AddComponent<ConfigurableJoint>();
                                if (collider.gameObject.name != "Hips")
                                {
                                    joint.xMotion = ConfigurableJointMotion.Locked;
                                    joint.yMotion = ConfigurableJointMotion.Locked;
                                    joint.zMotion = ConfigurableJointMotion.Locked;
                                    joint.angularXMotion = ConfigurableJointMotion.Limited;
                                    joint.angularYMotion = ConfigurableJointMotion.Limited;
                                    joint.angularZMotion = ConfigurableJointMotion.Limited;
                                    JointDrive drive = new JointDrive();
                                    drive.positionSpring = 1000;
                                    drive.positionDamper = 1000;
                                    joint.angularXDrive = drive;
                                    joint.angularYZDrive = drive;
                                    var limit0 = joint.lowAngularXLimit;
                                    limit0.limit = -60;
                                    joint.lowAngularXLimit = limit0;
                                    var limit = joint.highAngularXLimit;
                                    limit.limit = 60;
                                    joint.highAngularXLimit = limit;
                                    var limit1 = joint.angularYLimit;
                                    limit1.limit = 60;
                                    joint.angularYLimit = limit1;
                                    var limit2 = joint.angularYLimit;
                                    limit2.limit = 60;
                                    joint.angularZLimit = limit2;
                                    Transform rigidParent = joint.transform.parent;
                                    for (int i = 0; i < 5; i++)
                                    {
                                        if (rigidParent.GetComponent<Rigidbody>())
                                        {
                                            joint.connectedBody = rigidParent.GetComponent<Rigidbody>();
                                            break;
                                        }
                                        else
                                        {
                                            rigidParent = rigidParent.transform.parent;
                                        }
                                    }
                                }
                                else
                                {
                                    jointParent.parent = collider.transform;
                                    Physics.IgnoreLayerCollision(3, 8, false);
                                }
                            }
                        }
                        SoundManager.Instance.SFXPlay("Hit", clips[2]);
                        foreach (XWeaponTrail xw in jointParent.GetComponentsInChildren<XWeaponTrail>())
                            xw.MaxFrame = 0;
                        GetComponent<LegsAnimator>().enabled = false;
                        EnemyMotion.player = null;
                        Camera.main.transform.parent = null;
                        anim.GetBoneTransform(HumanBodyBones.Hips).parent = null;
                        transform.parent = anim.GetBoneTransform(HumanBodyBones.Hips);
                        Physics.IgnoreLayerCollision(6, 9, true); //prevent natural collision forced until hurt is 0
                        Physics.IgnoreLayerCollision(7, 8, true);
                        Invoke("Dissolve", 4);
                        Destroy(jointParent.gameObject, 5f);
                        Destroy(gameObject, 5f);
                    }
                }
                else
                {
                    GameObject shield = Instantiate(shieldVFX, collision.contacts[0].point, Quaternion.LookRotation(transform.forward));
                    Destroy(shield, 0.3f);
                    SoundManager.Instance.SFXPlay("Shield", clips[3]);
                    ent.transform.position -= ent.transform.forward;
                    if (!ent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Guarded"))
                        ent.GetComponent<Animator>().CrossFade("Guarded", 0, 0);
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public Material dissolve;

    void Dissolve()
    {
        foreach (Renderer renderer in jointParent.GetComponentsInChildren<Renderer>())
        {
            renderer.gameObject.AddComponent<DissolveSphere>();
            Material[] mat = renderer.materials;
            for (int i = 0; i < renderer.materials.Length; i++)
                mat[i] = dissolve;
            renderer.materials = mat;
        }
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.gameObject.AddComponent<DissolveSphere>();
            renderer.material = dissolve;
        }
    }
}