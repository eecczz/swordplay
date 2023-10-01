using FIMSpace.FProceduralAnimation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using XftWeapon;

public class EnemyMotion : MonoBehaviour
{
    Animator anim;
    NavMeshAgent nav;
    public static Transform player;
    public Transform jointParent, joint;
    public static Transform ejoint;
    Collider sword;
    float sensitivity = 0.01f;
    float tx, ty, rtx, rty;
    int cool = 100;
    int cool1 = 100;
    float guardTime = -1;
    int swing, guard;
    bool lr;
    public AudioClip[] clips;
    public GameObject hitVFX, shieldVFX;
    int sign;
    Vector3 vec, knockBack;
    Rigidbody rigid;
    public static Rigidbody rigid1;
    int health = 2;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<PlayerMotion>().transform;
        sword = jointParent.GetComponentInChildren<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Guarded"))
        {
            GetComponentInChildren<Rig>().weight = 1;
        }
        else
        {
            GetComponentInChildren<Rig>().weight = 0;
        }
        if (anim.enabled && anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && health > -1)
            GetComponent<LegsAnimator>().UseGluing = true;
        else
            GetComponent<LegsAnimator>().UseGluing = false;
        anim.SetBool("leftStep", PlayerMotion.lr);
        if (player != null)
            anim.SetFloat("dis", (player.position - transform.position).magnitude);
        if (health > -1)
        {
            if (cool1 > 0)
            {
                cool1--;
                tx = Mathf.Lerp(tx, rtx, sensitivity);
                ty = Mathf.Lerp(ty, rty, sensitivity);
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                    sensitivity = 0.01f;
                else
                    sensitivity = 0.1f;
            }
            if (cool1 == 0)
            {
                Physics.IgnoreLayerCollision(6, 9, true);
                cool1 = 100;
                if (guard == 0)
                {
                    rtx = Random.Range(-180, 180);
                    rty = Random.Range(-60, 150);
                }
                else
                {
                    rtx = Random.Range(-90, 90);
                    rty = Random.Range(-60, 90);
                }
            }
        }
        if (nav.enabled && swing == 0 && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Guarded") && player != null)
            nav.SetDestination(player.position);
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && swing == 0 && guardTime == -1 && health > -1)
        {
            //sword movement
            if (guard == 1)
            {
                sword.transform.localPosition = new Vector3(0, -1.25f, 1.5f);
                guard = 0;
            }
            joint.localRotation = Quaternion.Euler(new Vector3(-ty, 0, 0));
            sword.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, tx * Mathf.Clamp((new Vector2(tx, ty + 60).magnitude - 75) / 75, 0, 1)));
            joint.RotateAround(joint.position, sword.transform.up, tx);
            joint.RotateAround(joint.position, transform.forward, -tx * Mathf.Clamp((new Vector2(tx, ty + 60).magnitude - 75) / 75, 0, 1) * Mathf.Clamp((75 - ty) / 75, 0, 1));
        }
        if (PlayerMotion.ent != null)
        {
            if (cool > 0)
            {
                cool--;
            }
            if (cool == 0 && health > -1)
            {
                cool = -1;
                int r = Random.Range(0, 2);
                if (health > -1 && r == 0 && lr && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                {
                    if (PlayerMotion.ent.transform == transform)
                    {
                        anim.CrossFade("Attack", 0, 0);
                        swing = 1;
                        Physics.IgnoreLayerCollision(6, 9, false);
                        //sword movement
                        cool1 = 100;
                        rtx = -tx;
                        rty = -ty;
                        rtx = Mathf.Clamp(rtx, -90, 90);
                        rty = Mathf.Clamp(rty, -60, 150);
                    }
                }
                else if (health > -1 && r == 0 && !lr && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                {
                    if (PlayerMotion.ent.transform == transform)
                    {
                        anim.CrossFade("Attack", 0, 0);
                        swing = 1;
                        Physics.IgnoreLayerCollision(6, 9, false);
                        //sword movement
                        cool1 = 100;
                        rtx = -tx;
                        rty = -ty;
                        rtx = Mathf.Clamp(rtx, -90, 90);
                        rty = Mathf.Clamp(rty, -60, 150);
                    }
                }
                else if(r==0)
                {
                    cool = Random.Range(100, 500);
                }
                if (r == 1)
                {
                    guardTime = Random.Range(500, 1000);
                }
            }
            if (PlayerMotion.ent.transform == transform && player != null)
            {
                ejoint = joint;
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && health > -1)
                {
                    if (guard == 1)
                    {
                        sword.transform.localPosition = new Vector3(0, -1.25f, 1.5f);
                        guard = 0;
                    }
                    joint.localRotation = Quaternion.Euler(new Vector3(-ty, 0, 0));
                    sword.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, tx * Mathf.Clamp((new Vector2(tx, ty + 60).magnitude - 75) / 75, 0, 1)));
                    joint.RotateAround(joint.position, sword.transform.up, tx);
                    joint.RotateAround(joint.position, transform.forward, -tx * Mathf.Clamp((new Vector2(tx, ty + 60).magnitude - 75) / 75, 0, 1) * Mathf.Clamp((75 - ty) / 75, 0, 1));
                }
                else if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && health > -1)
                {
                    if (swing == 1)
                    {
                        GetComponent<LegsAnimator>().UseGluing = true;
                        //sword movement
                        cool = Random.Range(100, 500);
                        swing = 0;
                    }
                }
            }
            if (guardTime > 0 && health > -1)
            {
                guardTime--;
                if (guard == 0)
                {
                    tx = Mathf.Clamp(tx, -90, 90);
                    ty = Mathf.Clamp(ty, -60, 90);
                    rtx = Random.Range(-90, 90);
                    rty = Random.Range(-60, 90);
                    guard = 1;
                }
                joint.localRotation = Quaternion.Euler(new Vector3(-ty, 0, 0));
                sword.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, tx * Mathf.Clamp((new Vector2(tx, ty + 60).magnitude - 75) / 75, 0, 1)));
                joint.RotateAround(joint.position, sword.transform.up, tx);
                joint.RotateAround(joint.position, transform.forward, -tx * Mathf.Clamp((new Vector2(tx, ty + 60).magnitude - 75) / 75, 0, 1) * Mathf.Clamp((75 - ty) / 75, 0, 1));
                sword.transform.localPosition = new Vector3(0, -1.25f, 1.5f);
                sword.transform.position = transform.position + transform.up * 2 + transform.forward * 1.5f + transform.right * -tx / 90 + transform.up * -ty / 100;
            }
            if (guardTime == 0)
            {
                cool = Random.Range(100, 500);
                guardTime = -1;
            }
        }
    }

    private void FixedUpdate()
    { //everything is explained in PlayerMotion.cs
        if (player != null && health > -1)
            if ((transform.position - player.transform.position).magnitude <= 5 && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Guarded"))
                transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, Quaternion.LookRotation(player.position - transform.position).eulerAngles.y, transform.rotation.eulerAngles.z));
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && player != null && player.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Hurted") && health > -1)
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
            if (player != null && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                anim.GetBoneTransform(HumanBodyBones.Head).rotation = Quaternion.Euler(new Vector3(anim.GetBoneTransform(HumanBodyBones.Head).rotation.eulerAngles.x, anim.GetBoneTransform(HumanBodyBones.Head).rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
            else
                anim.GetBoneTransform(HumanBodyBones.Head).rotation = transform.rotation;
            anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).rotation = rotlleg;
            anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).rotation = rotrleg;
            anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position = poslleg;
            anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).position = posrleg;
        }
        else if(health >-1)
        {
            Vector3 poslleg = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position;
            Vector3 posrleg = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).position;
            Quaternion rotlleg = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).rotation;
            Quaternion rotrleg = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).rotation;
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.5f)
                transform.position += knockBack * anim.GetCurrentAnimatorStateInfo(0).normalizedTime * 0.1f;
            anim.GetBoneTransform(HumanBodyBones.Hips).rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0));
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime <=0.5f)
                anim.GetBoneTransform(HumanBodyBones.Hips).RotateAround(anim.GetBoneTransform(HumanBodyBones.Hips).position, Quaternion.Euler(transform.rotation.eulerAngles) * vec, 30);
            else
                anim.GetBoneTransform(HumanBodyBones.Hips).RotateAround(anim.GetBoneTransform(HumanBodyBones.Hips).position, Quaternion.Euler(transform.rotation.eulerAngles) * vec, 60 * (1 - anim.GetCurrentAnimatorStateInfo(0).normalizedTime));
            anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).rotation = rotlleg;
            anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).rotation = rotrleg;
            anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position = poslleg;
            anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).position = posrleg;
        }
        //manual parenting
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Guarded"))
        {
            jointParent.position = transform.position;
            jointParent.rotation = transform.rotation;
        }
        else
        {
            jointParent.position = anim.GetBoneTransform(HumanBodyBones.Head).position - transform.up * 2.866453f;
            jointParent.rotation = anim.GetBoneTransform(HumanBodyBones.Head).rotation;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8 && player!=null&&!player.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Guarded"))
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
                    SoundManager.Instance.SFXPlay("Hit", clips[0]);
                    Physics.IgnoreLayerCollision(6, 9, true);
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
                                Physics.IgnoreLayerCollision(3, 9, false);
                            }
                        }
                    }
                    SoundManager.Instance.SFXPlay("Hit", clips[0]);
                    foreach (XWeaponTrail xw in jointParent.GetComponentsInChildren<XWeaponTrail>())
                        xw.MaxFrame = 0;
                    GetComponent<LegsAnimator>().enabled = false;
                    gameObject.tag = "Untagged";
                    anim.GetBoneTransform(HumanBodyBones.Hips).parent = null;
                    transform.parent = anim.GetBoneTransform(HumanBodyBones.Hips);
                    Invoke("Dissolve", 4);
                    Destroy(jointParent.gameObject, 5f);
                    Destroy(gameObject, 5f);
                }
            }
            else if (guard == 1)
            {
                Vector2 v = joint.localRotation * Vector3.up;
                float r = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + 180;
                Vector2 v1 = new Vector2(-Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).normalized;
                float r1 = Mathf.Atan2(v1.y, v1.x) * Mathf.Rad2Deg + 180;
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
                        SoundManager.Instance.SFXPlay("Hit", clips[0]);
                        Physics.IgnoreLayerCollision(6, 9, true);
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
                                    Physics.IgnoreLayerCollision(3, 9, false);
                                }
                            }
                        }
                        SoundManager.Instance.SFXPlay("Hit", clips[0]);
                        foreach (XWeaponTrail xw in jointParent.GetComponentsInChildren<XWeaponTrail>())
                            xw.MaxFrame = 0;
                        GetComponent<LegsAnimator>().enabled = false;
                        gameObject.tag = "Untagged";
                        anim.GetBoneTransform(HumanBodyBones.Hips).parent = null;
                        transform.parent = anim.GetBoneTransform(HumanBodyBones.Hips);
                        Invoke("Dissolve", 4);
                        Destroy(jointParent.gameObject, 5f);
                        Destroy(gameObject, 5f);
                    }
                }
                else
                {
                    GameObject shield = Instantiate(shieldVFX, collision.contacts[0].point, Quaternion.LookRotation(transform.forward));
                    Destroy(shield, 0.3f);
                    SoundManager.Instance.SFXPlay("Shield", clips[1]);
                    player.position -= player.forward;
                    if (!player.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Guarded"))
                        player.GetComponent<Animator>().CrossFade("Guarded", 0, 0);
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