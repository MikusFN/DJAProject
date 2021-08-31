using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public GameObject player;
    public Transform sword, sword_equip, sword_unquip, swordThrow, controlCamera, rotationEnemies;
    public Rigidbody swordRigid;
    public bool equiped, throwSword;
    public float endAnim = 0.05f;
    Animator animator;
    AnimatorStateInfo animatorState;
    Quaternion quat;
    Vector3 directionForward = Vector3.zero, directionRight = Vector3.zero, directionUp = Vector3.zero, pastDirection = Vector3.zero;
    float movX = 0, movZ = 0, mouseX = 0, enemieCount = 0, lifeCount = 100, targetTimer = 1;
    bool hit = false, hitInfo = false, action = false, perry = false, perryTimer = false;
    int i = 0, l = 0, j = 0, dodgeMult = 1;


    private Ray ray;
    private RaycastHit rayh;
    private CharacterController CharController;
    private Transform playerTransform, target, cameraTransform, closestEnemie = null;
    private List<Transform> enemieList;
    private Vector3 fowardP, positionP;

    private void Awake()
    {
        CharController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        swordRigid = GetComponent<Rigidbody>();
        enemieList = new List<Transform>();
        quat = new Quaternion();
        rayh = new RaycastHit();
        CreatPlayerFrame();
        playerTransform = rotationEnemies;
        cameraTransform = controlCamera;
        mouseX = Input.GetAxis("Mouse X");
        fowardP = new Vector3();
        positionP = new Vector3();
    }

    private void Update()
    {

        mouseX = Input.GetAxis("Mouse X");
        //Debug.Log(mouseX);
        LayerDefinition();

        if (animator.GetLayerWeight(2) == 1)
        {

            perry = PerryTimerInit();

            //Debug.Log(" Perry " + perry);

            //GetEnemies();
            GetInputMovement();
            UpdatePlayerFrame();
            GetInputAction();
            //Debug.DrawRay(controlCamera.position, controlCamera.forward, Color.red);
            //Debug.Log(" lookAt.X = " + cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lookAt.position.x + " lookAt.Y = " + cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lookAt.position.y + " lookAt.Z = " + cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lookAt.position.z);
            enemieCount = enemieList.Count;

        }
    }

    private void LateUpdate()
    {
        if (animator.GetLayerWeight(2) == 1)
        {
            GetEnemies();
        }
    }

    public void Sword_Equip()
    {
        equiped = true;
    }
    public void Sword_Unquip()
    {
        equiped = false;
    }
    public void Throw_SwordAction()
    {
        equiped = false;
        throwSword = true;
    }

    private void CreatPlayerFrame()
    {
        directionUp = Vector3.Normalize(player.transform.up);
        directionRight = Vector3.Normalize(player.transform.right);
        directionForward = Vector3.Normalize(player.transform.forward);
    }
    private void UpdatePlayerFrame()
    {
        directionForward = Vector3.Normalize(Vector3.Cross(directionUp, directionRight));
        directionRight = Vector3.Normalize(Vector3.Cross(directionUp, directionForward));

    }
    private void GetInputMovement()
    {
        cameraTransform = controlCamera;

        if ((Input.GetAxis("Horizontal") != 0
            || Input.GetAxis("Vertical") != 0))
        {
            if (Input.GetAxis("Horizontal") < 0)
            {
                directionForward = -cameraTransform.right;//A
                if (animator.GetFloat("CombatMovX") <= -1)
                    movX = -1;
                else
                    movX += Input.GetAxis("Horizontal") * 0.2f;

                animator.SetFloat("CombatMovX", movX);
                //directionForward = Vector3.Normalize(directionForward) * 0.08f;
            }
            if (Input.GetAxis("Horizontal") > 0)
            {
                directionForward = cameraTransform.right;//D
                if (animator.GetFloat("CombatMovX") >= 1)
                    movX = 1;
                else
                    movX += Input.GetAxis("Horizontal") * 0.2f;

                animator.SetFloat("CombatMovX", movX);
                //directionForward = Vector3.Normalize(directionForward) * 0.08f;
            }
            if (Input.GetAxis("Vertical") > 0)
            {
                directionForward = cameraTransform.forward;//W
                if (animator.GetFloat("CombatMovZ") >= 1)
                    movZ = 1;
                else
                    movZ += Input.GetAxis("Vertical") * 0.2f;

                animator.SetFloat("CombatMovZ", movZ);
                //directionForward = Vector3.Normalize(directionForward) * 0.1f;
            }
            if (Input.GetAxis("Vertical") < 0)
            {
                directionForward = -cameraTransform.forward;//S
                if (animator.GetFloat("CombatMovZ") <= -1)
                    movX = -1f;
                else
                    movZ += Input.GetAxis("Vertical") * 0.2f;

                animator.SetFloat("CombatMovZ", movZ);
                //directionForward = Vector3.Normalize(directionForward) * 0.1f;
            }

            directionForward.y = 0;


            // quat.SetAxisAngle(Vector3.Cross(directionForward, directionRight), 0.0f);
            //actualQua = Quaternion.FromToRotation(pastDirection, directionForward);
            //quat.SetAxisAngle(Vector3.up, /*Quaternion.Angle(pastQuat, actualQua)*/,.1);

            directionForward *= 0.1f;
            animator.SetBool("Move_Moving", true);
            animatorState = animator.GetCurrentAnimatorStateInfo(2);

            if (animatorState.IsName("Strafing"))
                CharController.Move(directionForward);


        }
        else if (!Input.anyKey)
        {
            if (movX != 0)
            {
                if (movX > 0)
                {
                    movX += -endAnim;
                    animator.SetFloat("CombatMovX", movX);
                }
                if (movX < 0)
                {
                    movX += endAnim;
                    animator.SetFloat("CombatMovX", movX);
                }
            }
            else
                movX = 0;

            if (movZ != 0)
            {
                if (movZ > 0)
                {
                    movZ += -endAnim;
                    animator.SetFloat("CombatMovZ", movZ);
                }
                if (movZ < 0)
                {
                    movZ += endAnim;
                    animator.SetFloat("CombatMovZ", movZ);
                }
            }
            else
                movZ = 0;
        }
    }

    private Transform CloseTarget(List<Transform> enList, Transform player)
    {
        if (enList.Count == 0)
            return null;

        for (int i = 0; i < enList.Count; i++)
        {

            if (Vector3.Distance(player.transform.position, enList[i].position) < Vector3.Distance(player.transform.position, cameraTransform.gameObject.GetComponentInParent<CameraScript>().locked_lookAt.position))
            {
                closestEnemie = enList[i];
                break;
            }
            else
                closestEnemie = cameraTransform.gameObject.GetComponentInParent<CameraScript>().locked_lookAt;
        }
        return closestEnemie;
    }

    private void GetEnemies()
    {
        playerTransform = rotationEnemies;


        for (i = 0; i <= 360; i += 30)
        {
            quat = Quaternion.AngleAxis(i, Vector3.up);

            playerTransform.rotation = quat;


            fowardP = playerTransform.forward;
            positionP = playerTransform.position;
            fowardP.y = playerTransform.forward.y;
            positionP.y = playerTransform.position.y + 1.0f;


            hit = Physics.Raycast(ray = new Ray(positionP, fowardP), out rayh, 15);
            // Debug.DrawRay(positionP, fowardP * rayh.distance, Color.green);

            if (hit)
            {

                if (rayh.collider.gameObject != null)
                {
                    if (rayh.collider.gameObject.tag == "inimigo")
                    {
                        hitInfo = true;
                        target = rayh.collider.gameObject.GetComponentInChildren<Transform>();

                        if (!enemieList.Contains(target))
                            enemieList.Add(target);
                    }
                }

            }

        }

        UpdateEnemieList();
        ScrolingEnemies();

        //Debug.DrawRay(player.transform.position, player.transform.forward * 2.0f, Color.black);
        //Debug.Log(" x.ValueMouse = " + Input.mousePosition.x);
        //Debug.Log(" enemie count = " + enemieList.Count + " now enemie = " + l + " y.value = " + Input.mouseScrollDelta.y);S
    }
    private void ScrolingEnemies()
    {
        if (enemieList.Count > 0)
        {
            // cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lockedCamera = true;

            if (Input.mouseScrollDelta.y != 0)
            {
                if (l == enemieList.Count - 1 && Input.mouseScrollDelta.y > 0 && Input.mouseScrollDelta.y <= 1)
                    l = 0;
                else if (l == 0 && Input.mouseScrollDelta.y < 0 && Input.mouseScrollDelta.y >= -1)
                    l = enemieList.Count - 1;
                else if (Input.mouseScrollDelta.y >= -1 && Input.mouseScrollDelta.y <= 1)
                    l += (int)Input.mouseScrollDelta.y;

            }

            if (l == enemieList.Count)
                l--;

            if (cameraTransform.gameObject.GetComponentInParent<CameraScript>().locked_lookAt != enemieList[l])
                cameraTransform.gameObject.GetComponentInParent<CameraScript>().ChangeTarget(enemieList[l]);


            Transform trans = rotationEnemies;
            Vector3 foTrans = new Vector3();
            foTrans = Vector3.Normalize(enemieList[l].transform.position - player.transform.position);
            foTrans.y = 0;
            trans.forward = foTrans;

            player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, trans.transform.rotation, 180);

        }
    }
    private void UpdateEnemieList()
    {

        if (enemieList.Count > 0)
        {
            for (j = 0; j < enemieList.Count; j++)
            {

                if (Vector3.Distance(enemieList[j].position, playerTransform.position) > 10)
                {
                    if (cameraTransform.gameObject.GetComponentInParent<CameraScript>().locked_lookAt == enemieList[j] && cameraTransform.gameObject.GetComponentInParent<CameraScript>().locked_lookAt != CloseTarget(enemieList, playerTransform))
                    {
                        cameraTransform.gameObject.GetComponentInParent<CameraScript>().ChangeTarget(CloseTarget(enemieList, playerTransform));
                    }
                    //enemieList.Find(t => t == CloseTarget(enemieList, player.transform));
                    enemieList.Remove(enemieList[j]);
                    //Debug.Log(" j.value = " + j);
                }
            }
        }
        else
        {
            cameraTransform.gameObject.GetComponentInParent<CameraScript>().UnlockCamera();
            player.GetComponentInChildren<PlayerControls>().enabled = true;
            animator.SetLayerWeight(1, weight: 1);
            animator.SetLayerWeight(2, weight: 0);
        }
    }

    private void LayerDefinition()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (equiped)
                animator.SetTrigger("Sword_Inter");
            equiped = false;
            player.GetComponentInChildren<PlayerControls>().enabled = true;
            cameraTransform.gameObject.GetComponentInParent<CameraScript>().UnlockCamera();
            animator.SetLayerWeight(1, weight: 1);
            animator.SetLayerWeight(2, weight: 0);

        }
        else if (Input.GetKeyDown(KeyCode.Mouse2) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && equiped == false)
                animator.SetTrigger("Sword_Inter");

            player.GetComponentInChildren<PlayerControls>().enabled = false;
            animator.SetLayerWeight(2, weight: 1);
            animator.SetLayerWeight(1, weight: 0);
        }
    }

    private void GetInputAction()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetTrigger("InitialComboAttack");
        }

        animatorState = animator.GetCurrentAnimatorStateInfo(2);

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            animator.SetTrigger("HighAttack");
            animatorState = animator.GetCurrentAnimatorStateInfo(2);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!animator.GetBool("BlockAnimBool"))
            {
                perryTimer = true;
                targetTimer = 1;

                animator.SetBool("BlockAnimBool", true);

            }
            else if (animator.GetBool("BlockAnimBool"))
            {
                perryTimer = false;
                targetTimer = 0;
                perry = PerryTimerInit();
                animator.SetBool("BlockAnimBool", false);
            }


        }
        //Teste
        if (Input.GetKeyDown(KeyCode.H))
        {
            animator.SetTrigger("RollTrig");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            throwSword = false;
            animator.SetTrigger("Sword_Inter");
            animatorState = animator.GetCurrentAnimatorStateInfo(2);
            if (animatorState.IsName("sword_equip") || animatorState.IsName("idlesword"))
                equiped = true;
            else if (animatorState.IsName("sword_unquip"))
                equiped = false;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            //swordThrow = sword_equip;
            equiped = false;

            animator.SetTrigger("SwordThrow");
            animatorState = animator.GetCurrentAnimatorStateInfo(2);

            if (animatorState.IsName("Throw"))
            {
                throwSword = true;
            }
        }
        if (equiped && !throwSword)
        {
            sword.position = sword_equip.position;
            sword.rotation = sword_equip.rotation;
        }
        else if (!equiped && throwSword)
        {
            //sword.rigidbody(ForceMode.Impulse);
            //swordThrow.Translate(directionForward * 100);
            swordThrow = ConstantForce.Instantiate(sword, sword.transform);
            sword.position = swordThrow.position;
            // = swordThrow.position;
            //swordRigid.AddForce(directionForward, ForceMode.Acceleration); 
            //= swordThrow.position + directionForward * 10;
            //sword.rotation = swordThrow.rotation * Quaternion.Euler(5, 10, 5);
        }
        else if (!equiped && !throwSword)
        {
            sword.position = sword_unquip.position;
            sword.rotation = sword_unquip.rotation;
        }
        animator.SetBool("Sword_Equiped", equiped);
    }


    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "enemyWeapon")
        {
            //lifeCount -= col.gameObject.GetComponentInParent<Sword>.currentdamage;
            //holderAnimatorState = holderAnimator.GetCurrentAnimatorStateInfo(2);
            //damage = DamageAtribution(holderAnimatorState);

        }
    }

    bool PerryTimerInit()
    {
        bool timeEval = false;

        if (targetTimer >= 0)
        {
            targetTimer -= Time.deltaTime;
        }

        timeEval = true;

        if (targetTimer <= 0.0f)
            timeEval = false;

        return timeEval;
    }


    //int CloseTargetIndex(List<Transform> enList, Transform player)
    //{
    //    int closestEnemie = -1;

    //    if (enList.Count == 0)
    //        return -1;

    //    for (int i = 0; i < enList.Count; i++)
    //    {

    //        if (Vector3.Distance(player.transform.position, enList[i].position) < Vector3.Distance(player.transform.position, cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lookAt.position))
    //        {
    //            closestEnemie = i;
    //        }
    //        else
    //            closestEnemie =enList.FindIndex(cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lookAt);
    //    }
    //    return closestEnemie;
    //}
}



