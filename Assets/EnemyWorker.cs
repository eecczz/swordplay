using FIMSpace.FProceduralAnimation;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using XftWeapon;

public class EnemyWorker : MonoBehaviour
{
    private Animator anim;
    private NavMeshAgent nav;
    private Animator playerAnim;
    [SerializeField] private Transform crosshead;
    public static Transform psword;
    [SerializeField] private float swordOffsetY = 1.5f;
    private float sensitivity = 0.01f;
    private float tx, ty, rtx, rty;
    private int cool = 100;
    private int cool1 = 100;
    private float guardTime = -1;
    private int swing, guard;
    private GameObject hitVFX, shieldVFX;
    private AudioSource audioSource;
    [SerializeField] private Transform bag, defaultTransform, shieldTransform;
    private Rigidbody rigid;
    [SerializeField] private MultiAimConstraint ma;
    [SerializeField] private TwoBoneIKConstraint tb;
    [SerializeField] private int guardRate = 20;
    [SerializeField] private int health = 2;
    [SerializeField] private float movementSpeed = 5f;
    private RigBuilder rb;
    [SerializeField] private float power = 5f;
    [SerializeField] private LayerMask layermask;

    // Start is called before the first frame update
    private void Start()
    {
        anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        psword = GetComponentInChildren<TargetMatching>().transform;
        hitVFX = Resources.Load<GameObject>($"Prefab/HitVFX");
        shieldVFX = Resources.Load<GameObject>($"Prefab/ShieldVFX");
        audioSource = GetComponent<AudioSource>();
        playerAnim = PlayerWorker.player.GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        rb = GetComponent<RigBuilder>();
        
        if (PlayerWorker.isTwoHand)
        {
            anim.SetLayerWeight(1, 0f);
            anim.SetLayerWeight(2, 1f);
            anim.CrossFade("TH Guard", 0, 3);
            bag.gameObject.SetActive(false);
            tb.weight = 1;
        }
        else
        {
            anim.SetLayerWeight(1, 1f);
            anim.SetLayerWeight(2, 0f);
            anim.CrossFade("Guard", 0, 3);
            bag.gameObject.SetActive(true);
            tb.weight = 0;
        }
    }

    private bool IsPlayerInView()
    {
        if (PlayerWorker.player == null)
            return false;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, PlayerWorker.player.position - transform.position, out hit))
        {
            if (hit.collider.gameObject != PlayerWorker.player)
                return false;
        }
        return (PlayerWorker.player.position - transform.position).magnitude < 10f;
    }

    private float GetGroundHeight(Vector3 position)
    {
        RaycastHit hit;

        // 플레이어 앞에 레이캐스트를 발사하여 바닥을 감지
        if (Physics.Raycast(position, Vector3.down, out hit))
        {
            // 레이캐스트가 바닥과 충돌하면 그 충돌 지점의 y값을 반환
            return hit.point.y;
        }

        // 바닥이 감지되지 않으면 현재 위치의 y값을 그대로 반환
        return position.y;
    }

    private bool CanMoveToPosition(Vector3 position)
    {
        // Raycast를 사용하여 이동할 위치에 벽이 있는지 확인
        RaycastHit hit;
        Vector3 direction = position - transform.position;

        // 벽이 있는 경우, 이동하지 않도록 false 반환
        if (Physics.Raycast(anim.GetBoneTransform(HumanBodyBones.Hips).position, direction, out hit, direction.magnitude + 2.5f, layermask))
            return false;

        return true; // 벽이 없으면 이동 가능
    }

    // Update is called once per frame
    private void Update()
    {
        anim.SetFloat("tx", tx);
        anim.SetFloat("ty", ty);
        if (PlayerWorker.ent && PlayerWorker.ent.transform == transform)
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") || (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && anim.IsInTransition(0)))
                Physics.IgnoreLayerCollision(6, 9, true);
        if (PlayerWorker.player != null && IsPlayerInView() && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Guarded") && swing == 0 && health > -1)
            transform.rotation = Quaternion.Euler(new Vector3(0, Quaternion.LookRotation(PlayerWorker.player.position - transform.position).eulerAngles.y, 0));
        if (PlayerWorker.player == null && ma.data.sourceObjects[0].transform != crosshead)
        {
            var data = ma.data.sourceObjects;
            data.SetTransform(0, crosshead);
            ma.data.sourceObjects = data;
            rb.Build();
        }
        if (PlayerWorker.player != null && (PlayerWorker.player.position - transform.position).magnitude > 4 && (PlayerWorker.player.position - transform.position).magnitude < 6)
            anim.SetFloat("dis", (PlayerWorker.player.position - transform.position).magnitude);
        else
        {
            anim.SetFloat("dis", 1000);
            if ((PlayerWorker.player.position - transform.position).magnitude >= 4)
            {
                Vector3 targetPos = transform.position + transform.forward * movementSpeed * Time.deltaTime;
                if (CanMoveToPosition(targetPos))
                {
                    if(!audioSource.isPlaying)
                        audioSource.PlayOneShot(Resources.Load<AudioClip>($"Audio/SFX/Step"));
                    anim.SetFloat("mz", 1);
                    // 벽이 없으면 위치 갱신
                    transform.position = new Vector3(targetPos.x, Mathf.Lerp(targetPos.y, GetGroundHeight(targetPos + Vector3.up), 0.1f), targetPos.z);
                }
            }
            else
            {
                Vector3 targetPos = transform.position - transform.forward * movementSpeed * Time.deltaTime;
                if (CanMoveToPosition(targetPos))
                {
                    if(!audioSource.isPlaying)
                        audioSource.PlayOneShot(Resources.Load<AudioClip>($"Audio/SFX/Step"));
                    anim.SetFloat("mz", -1);
                    // 벽이 없으면 위치 갱신
                    transform.position = new Vector3(targetPos.x, Mathf.Lerp(targetPos.y, GetGroundHeight(targetPos + Vector3.up), 0.1f), targetPos.z);
                }
            }
        }
        if (cool1 > 0)
        {
            cool1--;
            if(!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                tx = Mathf.Lerp(tx, rtx, sensitivity);
                ty = Mathf.Lerp(ty, rty, sensitivity);
            }
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                sensitivity = 0.01f;
            else
                sensitivity = 0.1f;
        }
        if (cool1 == 0)
        {
            cool1 = 100;
            if (guard == 0)
            {
                bag.parent = defaultTransform;
                bag.localPosition = Vector3.zero;
                bag.localRotation = Quaternion.identity;
                rtx = Random.Range(-180, 180);
                rty = Random.Range(-30, 150);
            }
            else
            {
                bag.parent = shieldTransform;
                bag.localPosition = Vector3.zero;
                bag.localRotation = Quaternion.identity;
                rtx = Mathf.Clamp(rtx, -180, 180);
                rty = Mathf.Clamp(rty, -30, 150);
            }
        }
        if (nav.enabled && swing == 0 && PlayerWorker.player.position.y - transform.position.y < 1f && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Guarded") && PlayerWorker.player != null)
            nav.SetDestination(PlayerWorker.player.position);
        if (cool > 0)
        {
            cool--;
        }
        if (cool == 0 && health > -1 && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            cool = -1;
            int r = Random.Range(0, 100);
            if (r < 100 - guardRate && (anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") || anim.GetCurrentAnimatorStateInfo(0).IsName("Guarded") || health == -1 || PlayerWorker.player == null))
            {
                r = -1;
                cool = Random.Range(100, 500);
            }
            if (r < 100 - guardRate)
            {
                if (PlayerWorker.ent && PlayerWorker.ent.transform == transform)
                {
                    anim.CrossFade("Attack", 0, 0);
                    anim.CrossFade("Attack", 0, 1);
                    anim.CrossFade("Attack", 0, 2);
                    ma.weight = 0;
                    swing = 1;
                    //sword movement
                    cool1 = 100;
                    rtx = -tx;
                    rty = -ty;
                    rtx = Mathf.Clamp(rtx, -180, 180);
                    rty = Mathf.Clamp(rty, -30, 150);
                    Physics.IgnoreLayerCollision(6, 9, false);
                }
                else
                    cool = Random.Range(100, 500);
            }
            if (r >= 100 - guardRate)
            {
                guardTime = Random.Range(500, 1000);
            }
        }
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            if (swing == 1)
            {
                //sword movement
                cool = Random.Range(100, 500);
                swing = 0;
                ma.weight = 1;
            }
        }
        if (guardTime > 0 && health > -1)
        {
            guardTime--;
            if (guard == 0)
            {
                tx = Mathf.Clamp(tx, -180, 180);
                ty = Mathf.Clamp(ty, -30, 150);
                rtx = Mathf.Clamp(rtx, -180, 180);
                rty = Mathf.Clamp(rty, -30, 150);
                guard = 1;
                anim.SetLayerWeight(1, 0f);
                anim.SetLayerWeight(2, 0f);
                anim.SetLayerWeight(3, 1f);
            }
        }
        if (guardTime == 0)
        {
            cool = Random.Range(100, 500);
            guardTime = -1;
            if (PlayerWorker.isTwoHand)
                anim.SetLayerWeight(2, 1f);
            else
                anim.SetLayerWeight(1, 1f);
            anim.SetLayerWeight(3, 0f);
            guard = 0;
        }
    }

    private void LateUpdate()
    {
        if (health > -1)
        {
            if (anim.GetLayerWeight(3) == 1 && !PlayerWorker.isTwoHand)
            {
                anim.GetBoneTransform(HumanBodyBones.Spine).RotateAround(anim.GetBoneTransform(HumanBodyBones.Spine).position, Vector3.up, -60);
                anim.GetBoneTransform(HumanBodyBones.Head).rotation = transform.rotation;
            }
            //Head IK fix
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted"))
                if (PlayerWorker.player == null && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                    anim.GetBoneTransform(HumanBodyBones.Head).rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0));
                else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                {
                    anim.GetBoneTransform(HumanBodyBones.Head).rotation = transform.rotation;
                    anim.GetBoneTransform(HumanBodyBones.RightShoulder).localRotation = Quaternion.Euler(new Vector3(90, 0, -120));
                    anim.GetBoneTransform(HumanBodyBones.RightLowerArm).localRotation = Quaternion.Euler(new Vector3(45, 0, 0));
                    anim.GetBoneTransform(HumanBodyBones.RightUpperArm).localRotation = Quaternion.identity;
                }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8 && PlayerWorker.player != null && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted"))
        {
            if (guard == 0)
            {
                if (health > 0)
                {
                    health--;
                    Vector3 hitpoint = Quaternion.Euler(new Vector3(0, -transform.rotation.eulerAngles.y, 0)) * (collision.contacts[0].point - (transform.position + Vector3.up));
                    anim.SetFloat("cx", hitpoint.x);
                    anim.SetFloat("cy", hitpoint.y);
                    anim.CrossFade("Hurted", 0, 0);
                    GameObject hit = Instantiate(hitVFX, collision.contacts[0].point, Quaternion.LookRotation(anim.GetBoneTransform(HumanBodyBones.Neck).position - collision.contacts[0].point));
                    Destroy(hit, 0.3f);
                    SoundManager.Instance.SFXPlay("Armor Impact 1");
                    Physics.IgnoreLayerCollision(7, 8, true);
                }
                else
                {
                    if (guard == 1)
                    {
                        guard = 0;
                    }
                    health = -1;
                    anim.enabled = false;
                    nav.enabled = false;
                    GetComponentInChildren<Rig>().weight = 0;
                    GameObject hit = Instantiate(hitVFX, collision.contacts[0].point, Quaternion.LookRotation(anim.GetBoneTransform(HumanBodyBones.Neck).position - collision.contacts[0].point));
                    Destroy(hit, 0.3f);
                    Destroy(GetComponent<ConfigurableJoint>());
                    Destroy(GetComponent<Collider>());
                    Destroy(GetComponent<Rigidbody>());
                    SoundManager.Instance.SFXPlay("Armor Impact 1");
                    gameObject.tag = "Untagged";
                    Destroy(gameObject, 5f);
                    Invoke("Dissolve", 4);
                    foreach (Collider collider in GetComponentsInChildren<Collider>())
                    {
                        collider.isTrigger = false;
                        collider.enabled = true;
                        if (!collider.gameObject.GetComponent<Rigidbody>())
                        {
                            Rigidbody rrigid = collider.gameObject.AddComponent<Rigidbody>();
                            rrigid.velocity = Vector3.zero;
                            rrigid.angularVelocity = Vector3.zero;
                            rrigid.AddForceAtPosition(collision.gameObject.GetComponent<Rigidbody>().velocity, collision.contacts[0].point, ForceMode.Impulse);
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
                        }
                    }

                }
            }
            else if (guard == 1)
            {
                GameObject shield = Instantiate(shieldVFX, collision.contacts[0].point, Quaternion.LookRotation(transform.forward));
                Destroy(shield, 0.3f);
                SoundManager.Instance.SFXPlay("Shield Impact 2");
                PlayerWorker.player.position -= PlayerWorker.player.forward;
                if (!playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Guarded"))
                    playerAnim.CrossFade("Guarded", 0, 0);
            }
        }
    }

    private void Dissolve()
    {
        Material dissolve = Resources.Load<Material>($"Material/Shader Graphs_Dissolve_Dissolve_Metallic");
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.gameObject.AddComponent<DissolveSphere>();
            Material[] mat = renderer.materials;
            for (int i = 0; i < renderer.materials.Length; i++)
                mat[i] = dissolve;
            renderer.materials = mat;
        }
        foreach (Renderer renderer in anim.GetBoneTransform(HumanBodyBones.Hips).GetComponentsInChildren<Renderer>())
        {
            renderer.gameObject.AddComponent<DissolveSphere>();
            Material[] mat = renderer.materials;
            for (int i = 0; i < renderer.materials.Length; i++)
                mat[i] = dissolve;
            renderer.materials = mat;
        }
    }
}