using FIMSpace.FProceduralAnimation;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.AI;

public class PlayerWorker : MonoBehaviour
{
    public static Transform player;
    [SerializeField] private Transform crosshead;
    private Animator anim;
    public static Transform psword;
    [SerializeField] private float sensitivity = 5;
    private Vector3 camDis;
    [SerializeField] private float canSwing = 10f;
    [SerializeField] private float swordOffsetY = 1.5f;
    public static float tx, ty;
    public static int swing, guard;
    public static bool isTwoHand = true;
    private GameObject hitVFX, shieldVFX;
    [SerializeField] private Transform bag, defaultTransform, shieldTransform; 
    public static GameObject ent;
    private Animator entAnim;
    private bool onTarget;
    private Rigidbody rigid;
    private int health = 0;
    private RigBuilder rb;
    private AudioSource audioSource;
    private NavMeshAgent nav;
    [SerializeField] private MultiAimConstraint ma;
    [SerializeField] private TwoBoneIKConstraint tb;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float power = 5f;
    [SerializeField] private Vector3 camPos;
    [SerializeField] private LayerMask layermask;
    [SerializeField] private RectTransform endUI;
    [SerializeField] private Text header;
    [SerializeField] private bool isGameMode;
    [SerializeField] private Transform body;
    [SerializeField] private Transform des;
    private float preScore;
    private bool isEnd;

    private void Awake()
    {
        player = transform;
        Time.timeScale = 1.0f;
        endUI.gameObject.SetActive(false);
        isEnd = false;
        Cursor.visible = false;
        anim = GetComponent<Animator>();
        psword = GetComponentInChildren<TargetMatching>().transform;
        hitVFX = Resources.Load<GameObject>($"Prefab/HitVFX 1");
        shieldVFX = Resources.Load<GameObject>($"Prefab/ShieldVFX");
        rigid = GetComponent<Rigidbody>();
        rb = GetComponent<RigBuilder>();
        audioSource = GetComponent<AudioSource>();
        nav = body.GetComponent<NavMeshAgent>();
        preScore = (new Vector3(0, 60) - new Vector3(tx, ty)).magnitude;
        if (isTwoHand)
        {
            anim.SetLayerWeight(1, 1f);
            anim.SetLayerWeight(2, 0f);
            bag.gameObject.SetActive(false);
            tb.weight = 1;
            anim.CrossFade("TH Guard", 0, 3);
        }
        else
        {
            anim.SetLayerWeight(1, 0f);
            anim.SetLayerWeight(2, 1f);
            bag.gameObject.SetActive(true);
            tb.weight = 0;
            anim.CrossFade("Guard", 0, 3);
        }
    }

    private List<GameObject> enemies = new List<GameObject>();
    private int _idx = 0;
    public GameObject FindAdvancedEnemy(int idx)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Enemy");
        Vector3 position = transform.position;
        enemies.Clear();
        foreach (GameObject go in gos)
        {
            if(IsEnemyInView(go.gameObject))
                enemies.Add(go);
        }
        if (idx > enemies.Count - 1 || idx == -1)
            return ent = null;
        return ent = enemies[idx].gameObject;
    }

    private bool IsEnemyInView(GameObject enemy)
    {
        if (enemy == null)
            return false;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, enemy.transform.position - transform.position, out hit))
        {
            if (hit.collider.gameObject.layer != 7)
                return false;
        }
        return (enemy.transform.position - transform.position).magnitude < 10f;
    } 

    private void IsGoal()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 5, transform.forward, out hit))
        {
            if (isGameMode && hit.collider.gameObject.layer == 10)
            {
                isEnd = true;
                Cursor.visible = true;
                Invoke("ClearUI", 4);
            }
        }
    }

    private bool IsStuffInView()
    {
        RaycastHit hit;
        // 벽이 있을 경우 Raycast로 충돌 확인
        if (Physics.Raycast(transform.position + Vector3.up * 5, -transform.forward, out hit, 5f))
        {
            if (hit.collider.gameObject.layer == 0)
            {
                camDis = new Vector3(hit.point.x - transform.position.x, 0, hit.point.z - transform.position.z);
                return true;
            }
        }
        return false;
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

    private void Update()
    {
        body.position = transform.position;
        nav.SetDestination(des.transform.position);
        anim.SetFloat("tx", tx);
        anim.SetFloat("ty", ty);
        tx += Input.GetAxis("Mouse X") * sensitivity;
        ty += Input.GetAxis("Mouse Y") * sensitivity;
        tx = Mathf.Clamp(tx, -180, 180);
        ty = Mathf.Clamp(ty, -30, 150);
        IsGoal();
        if (isEnd)
            Time.timeScale = Mathf.Lerp(Time.timeScale, 0.5f, 0.01f);
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") || (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && anim.IsInTransition(0)))
            Physics.IgnoreLayerCollision(7, 8, true);
        if (!onTarget && ma.data.sourceObjects[0].transform != crosshead)
        {
            var data = ma.data.sourceObjects;
            data.SetTransform(0, crosshead);
            ma.data.sourceObjects = data;
            rb.Build();
        }
        if (health > -1)
        {
            //player movement
            if (guard == 0 && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Guarded") && swing == 0)
            {
                float x = Input.GetAxis("Horizontal"); // 수평 입력
                float z = Input.GetAxis("Vertical"); // 수직 입력
                anim.SetFloat("mx", x);
                anim.SetFloat("mz", z);
                Vector3 targetPos = transform.position + Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0)) * new Vector3(x, 0, z) * movementSpeed * Time.deltaTime;
                if (CanMoveToPosition(targetPos))
                {
                    if(!audioSource.isPlaying && new Vector2(x,z).magnitude > 0 && !isEnd)
                        audioSource.PlayOneShot(Resources.Load<AudioClip>($"Audio/SFX/Step"));
                    // 벽이 없으면 위치 갱신
                    transform.position = new Vector3(targetPos.x, Mathf.Lerp(targetPos.y, GetGroundHeight(targetPos + Vector3.up), 0.1f), targetPos.z);
                }
            }
            if (onTarget && !isEnd)
            {
                if (swing == 0 && health > -1)
                    body.rotation = Quaternion.Euler(new Vector3(0, Quaternion.LookRotation(ent.transform.position - body.position).eulerAngles.y, 0));
            }
        }
        if (!Input.GetMouseButton(1))
        {
            if (guard == 1)
            {
                if (isTwoHand)
                    anim.SetLayerWeight(1, 1f);
                else
                    anim.SetLayerWeight(2, 1f);
                anim.SetLayerWeight(3, 0);
                bag.parent = defaultTransform;
                bag.localPosition = Vector3.zero;
                bag.localRotation = Quaternion.identity;
                guard = 0;
            }
        }
        else if (health > -1)
        {
            if (guard == 0)
            {
                anim.SetLayerWeight(1, 0);
                anim.SetLayerWeight(2, 0);
                anim.SetLayerWeight(3, 1);
                bag.parent = shieldTransform;
                bag.localPosition = Vector3.zero;
                bag.localRotation = Quaternion.identity;
                guard = 1;
                SoundManager.Instance.SFXPlay("Shield");
            }
        }
        float curScore = (new Vector3(0, 60) - new Vector3(tx, ty)).magnitude;
        float goalScore = preScore - curScore;
        float testScore = (new Vector3(0, 0) - new Vector3(tx, ty)).magnitude;

        anim.SetFloat("time", 1 - testScore / 180);
        preScore = curScore;
        if (health > -1 && goalScore > canSwing && guard == 0 && swing == 0 && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")) //������
        {
            anim.CrossFade("Attack", 0, 0);
            SoundManager.Instance.SFXPlay("Swing1");
            ma.weight = 0;
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && ent != null && !entAnim.GetCurrentAnimatorStateInfo(0).IsName("Hurted") && health > -1)
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
                ma.weight = 1;
            }
        }
        if (!onTarget)
        {
            if (FindAdvancedEnemy(0)!= null)
            {
                entAnim = ent.GetComponent<Animator>();
                onTarget = true;
            }
        }
        if (onTarget)
        {
            if (ent.gameObject.tag != "Enemy" || (ent.transform.position - transform.position).magnitude > 10f)
            {
                _idx = 0;
                onTarget = false;
            }
        }
        /*if (!IsStuffInView())
        {
            Camera.main.transform.localPosition = camPos + Vector3.up * anim.GetBoneTransform(HumanBodyBones.Hips).localPosition.y;
            Camera.main.transform.localRotation = Quaternion.Euler(new Vector3(20, 0, 0));
        }
        else
        {
            Camera.main.transform.localPosition = new Vector3(camPos.x, camPos.y + anim.GetBoneTransform(HumanBodyBones.Hips).localPosition.y, 0);
            Camera.main.transform.position -=  Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0)) * Vector3.forward * camDis.magnitude * 0.75f;
            Camera.main.transform.localRotation = Quaternion.Euler(new Vector3(20, 0, 0));
        }*/
    }

    private void LateUpdate()
    {
        if (health > -1)
        {
            if (anim.GetLayerWeight(4) == 1 && !isTwoHand)
            {
                anim.GetBoneTransform(HumanBodyBones.Spine).RotateAround(anim.GetBoneTransform(HumanBodyBones.Spine).position, Vector3.up, -60);
                anim.GetBoneTransform(HumanBodyBones.Head).rotation = transform.rotation;
            }
            //Head IK fix
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted"))
                if (ent == null && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                    anim.GetBoneTransform(HumanBodyBones.Head).rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0));
                else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !isTwoHand)
                {
                    anim.GetBoneTransform(HumanBodyBones.Head).rotation = transform.rotation;
                    anim.GetBoneTransform(HumanBodyBones.RightShoulder).localRotation = Quaternion.Euler(new Vector3(90, 0, -120));
                    anim.GetBoneTransform(HumanBodyBones.RightLowerArm).localRotation = Quaternion.Euler(new Vector3(45, 0, 0));
                    anim.GetBoneTransform(HumanBodyBones.RightUpperArm).localRotation = Quaternion.identity;
                }
        }
        float weight = Mathf.Clamp01((180 + tx) / 360f);
        Vector3 tmp = anim.GetBoneTransform(HumanBodyBones.RightHand).position;
        Vector3 lp = anim.GetBoneTransform(HumanBodyBones.LeftHand).position;
        anim.GetBoneTransform(HumanBodyBones.RightHand).position += (lp - anim.GetBoneTransform(HumanBodyBones.RightHand).position) * weight;
        anim.GetBoneTransform(HumanBodyBones.LeftHand).position += (tmp - anim.GetBoneTransform(HumanBodyBones.LeftHand).position) * weight;
        GameObject rightTemp = new GameObject("RightTemp");
        GameObject leftTemp = new GameObject("LeftTemp");
        rightTemp.transform.position = anim.GetBoneTransform(HumanBodyBones.RightHand).position;
        leftTemp.transform.position = anim.GetBoneTransform(HumanBodyBones.LeftHand).position;
        Quaternion tmp1 = anim.GetBoneTransform(HumanBodyBones.RightHand).rotation;
        Quaternion lp1 = anim.GetBoneTransform(HumanBodyBones.LeftHand).rotation;
        rightTemp.transform.rotation = lp1;
        rightTemp.transform.RotateAround(rightTemp.transform.position, rightTemp.transform.up, 180);
        leftTemp.transform.rotation = tmp1;
        leftTemp.transform.RotateAround(leftTemp.transform.position, leftTemp.transform.up, 180);


        Quaternion rightRot = anim.GetBoneTransform(HumanBodyBones.RightHand).rotation;
        Quaternion leftRot = anim.GetBoneTransform(HumanBodyBones.LeftHand).rotation;
        Quaternion rightTargetRot = rightTemp.transform.rotation;
        Quaternion leftTargetRot = leftTemp.transform.rotation;

        Quaternion newRightRot = new Quaternion(
            rightRot.x + (rightTargetRot.x - rightRot.x) * weight,
            rightRot.y + (rightTargetRot.y - rightRot.y) * weight,
            rightRot.z + (rightTargetRot.z - rightRot.z) * weight,
            rightRot.w + (rightTargetRot.w - rightRot.w) * weight
        ).normalized;

        Quaternion newLeftRot = new Quaternion(
            leftRot.x + (leftTargetRot.x - leftRot.x) * weight,
            leftRot.y + (leftTargetRot.y - leftRot.y) * weight,
            leftRot.z + (leftTargetRot.z - leftRot.z) * weight,
            leftRot.w + (leftTargetRot.w - leftRot.w) * weight
        ).normalized;

        anim.GetBoneTransform(HumanBodyBones.RightHand).rotation = newRightRot;
        anim.GetBoneTransform(HumanBodyBones.LeftHand).rotation = newLeftRot;

        Destroy(rightTemp);
        Destroy(leftTemp);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 9 && ent && !anim.GetCurrentAnimatorStateInfo(0).IsName("Hurted"))
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
                    SoundManager.Instance.SFXPlay("Shield Impact 1");
                    Physics.IgnoreLayerCollision(6, 9, true);
                }
                else
                {
                    if (guard == 1)
                    {
                        guard = 0;
                    }
                    health = -1;
                    anim.enabled = false;
                    rigid.AddForceAtPosition(collision.gameObject.GetComponent<Rigidbody>().velocity, collision.contacts[0].point, ForceMode.Impulse);
                    GetComponentInChildren<Rig>().weight = 0;
                    GameObject hit = Instantiate(hitVFX, collision.contacts[0].point, Quaternion.LookRotation(anim.GetBoneTransform(HumanBodyBones.Neck).position - collision.contacts[0].point));
                    Destroy(hit, 0.3f);
                    Destroy(GetComponent<LegsAnimator>());
                    Destroy(GetComponent<ConfigurableJoint>());
                    Destroy(GetComponent<Collider>());
                    Destroy(GetComponent<Rigidbody>());
                    SoundManager.Instance.SFXPlay("Shield Impact 1");
                    EnemyMotion.player = null;
                    Invoke("Dissolve", 4);
                    Invoke("OverUI", 4);
                    isEnd = true;
                    Cursor.visible = true;
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
                ent.transform.position -= ent.transform.forward;
                if (!entAnim.GetCurrentAnimatorStateInfo(0).IsName("Guarded"))
                    entAnim.CrossFade("Guarded", 0, 0);
            }
        }
    }

    private void ClearUI()
    {
        header.text = "클리어!!";
        endUI.gameObject.SetActive(true);
        isTwoHand = !isTwoHand;
    }

    private void OverUI()
    {
        header.text = "게임오버!!";
        endUI.gameObject.SetActive(true);
        isTwoHand = !isTwoHand;
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