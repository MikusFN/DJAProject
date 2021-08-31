﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public GameObject player, swordObject;
    public Transform sword, sword_equip, sword_unquip, swordThrow, cameraTransform, rotationEnemies;
    public Rigidbody swordRigid;
    private CharacterController CharController;
    public bool equiped, throwSword;
    Animator animator;
    AnimatorStateInfo animatorState;
    Quaternion pastQuat, actualQua;
    Quaternion quat;
    Vector3 directionForward = Vector3.zero, directionRight = Vector3.zero, directionUp = Vector3.zero, pastDirection = Vector3.zero;
    float movX = 0, movZ = 0;
    bool hit = false, hitInfo = false;
    int i = 0, l = 0, j = 0;
    float mouseX = 0;
    Vector3 velocityCam = Vector3.one;
    Ray ray;
    RaycastHit rayh;
    private Transform playerTransform, target, closestEnemie = null;
    private List<Transform> enemieList;
    Vector3 fo, po;

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
        mouseX = Input.mousePosition.x;
        fo = new Vector3();
        po = new Vector3();
    }

    private void FixedUpdate()
    {
        //mouseX = Input.mousePosition.x;
        //Debug.Log(" mouseX = " + mouseX);

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            player.GetComponentInChildren<PlayerControls>().enabled = true;
            cameraTransform.gameObject.GetComponentInChildren<CameraScript>().UnlockCamera();
            animator.SetLayerWeight(1, weight: 1);
            animator.SetLayerWeight(2, weight: 0);

        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            player.GetComponentInChildren<PlayerControls>().enabled = false;
            animator.SetLayerWeight(2, weight: 1);
            animator.SetLayerWeight(1, weight: 0);
        }
        if (animator.GetLayerWeight(2) == 1)
        {
            GetEnimies();
            GetInputMovement();
            UpdatePlayerFrame();

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                animator.SetTrigger("HighAttack");
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
        //Debug.Log(" lookAt.X = " + cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lookAt.position.x + " lookAt.Y = " + cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lookAt.position.y + " lookAt.Z = " + cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lookAt.position.z);
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
        if ((Input.GetAxis("Horizontal") != 0
            || Input.GetAxis("Vertical") != 0))
        {
            if (Input.GetAxis("Horizontal") < 0)
            {
                directionForward -= cameraTransform.right.normalized;//A
                if (animator.GetFloat("CombatMovX") <= -1)
                    movX = -1;
                else
                    movX += Input.GetAxis("Horizontal") * 0.2f;

                animator.SetFloat("CombatMovX", movX);
                //directionForward = Vector3.Normalize(directionForward) * 0.08f;
            }
            if (Input.GetAxis("Horizontal") > 0)
            {
                directionForward += cameraTransform.right.normalized;//D
                if (animator.GetFloat("CombatMovX") >= 1)
                    movX = 1;
                else
                    movX += Input.GetAxis("Horizontal") * 0.2f;

                animator.SetFloat("CombatMovX", movX);
                //directionForward = Vector3.Normalize(directionForward) * 0.08f;
            }
            if (Input.GetAxis("Vertical") > 0)
            {
                directionForward += cameraTransform.forward.normalized;//W
                if (animator.GetFloat("CombatMovZ") >= 1)
                    movZ = 1;
                else
                    movZ += Input.GetAxis("Vertical") * 0.2f;

                animator.SetFloat("CombatMovZ", movZ);
                //directionForward = Vector3.Normalize(directionForward) * 0.1f;
            }
            if (Input.GetAxis("Vertical") < 0)
            {
                directionForward -= cameraTransform.forward.normalized;//S
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

            directionForward = Vector3.Normalize(directionForward) * 0.1f;
            animator.SetBool("Move_Moving", true);
            CharController.Move(directionForward);
        }
        else if (!Input.anyKey)
        {
            if (movX != 0)
            {
                if (movX >= 0)
                {
                    movX += -0.05f;
                    animator.SetFloat("CombatMovX", movX);
                }
                if (movX <= 0)
                {
                    movX += 0.05f;
                    animator.SetFloat("CombatMovX", movX);
                }
            }
            else
                movX = 0;
            if (movZ != 0)
            {
                if (movZ >= 0)
                {
                    movZ += -0.05f;
                    animator.SetFloat("CombatMovZ", movZ);
                }
                if (movZ <= 0)
                {
                    movZ += 0.05f;
                    animator.SetFloat("CombatMovZ", movZ);
                }
            }
            else
                movZ = 0;
        }
    }

    Transform CloseTarget(List<Transform> enList, Transform player)
    {
        if (enList.Count == 0)
            return null;

        for (int i = 0; i < enList.Count; i++)
        {

            if (Vector3.Distance(player.transform.position, enList[i].position) < Vector3.Distance(player.transform.position, cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lookAt.position))
            {
                closestEnemie = enList[i];
            }
            else
                closestEnemie = cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lookAt;
        }
        return closestEnemie;
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

    void GetEnimies()
    {
        playerTransform = rotationEnemies;

        for (i = 0; i <= 360; i += 20)
        {
            quat = Quaternion.AngleAxis(i, Vector3.up);

            playerTransform.rotation = quat;


            fo = playerTransform.forward;
            po = playerTransform.position;
            fo.y = playerTransform.forward.y;
            po.y = playerTransform.position.y + 1.0f;


            hit = Physics.Raycast(ray = new Ray(po, fo), out rayh, 10);
            Debug.DrawRay(po, fo * rayh.distance, Color.green);

            if (hit)
            {

                if (rayh.collider.gameObject != null)
                {
                    if (rayh.collider.gameObject.tag == "inimigo")
                    {
                        hitInfo = true;
                        target = rayh.collider.gameObject.transform;
                        if (!enemieList.Contains(target))
                            enemieList.Add(target);
                    }
                    else
                    {
                        hitInfo = false;

                    }
                }
                //else
                //    cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lockedCamera = false;
            }
            //Debug.Log(" hitinfo = " + hitInfo + " hit = " + hit + " distance = " + rayh.distance);
            //Debug..Quaternion.Angle(pastQuat, actualQua)
            //Debug.Log("X = " + fo.x + " Y = " + fo.y + " Z = " + fo.z);
        }



        if (enemieList.Count > 0)
        {
            for (j = 0; j < enemieList.Count; j++)
            {

                if (Vector3.Distance(enemieList[j].position, playerTransform.position) > 10)
                {
                    if (cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lookAt == enemieList[j] && cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lookAt != CloseTarget(enemieList, playerTransform))
                    {
                        cameraTransform.gameObject.GetComponentInChildren<CameraScript>().ChangeTarget(CloseTarget(enemieList, playerTransform));
                    }
                    //enemieList.Find(t => t == CloseTarget(enemieList, player.transform));
                    enemieList.Remove(enemieList[j]);
                    //Debug.Log(" j.value = " + j);
                }
            }
        }
        else
        {
            cameraTransform.gameObject.GetComponentInChildren<CameraScript>().UnlockCamera();
        }

        //Debug.Log(" enemie count1 = " + enemieList.Count + " now enemie1 = " + l + " y.value1 = " + Input.mouseScrollDelta.y);
        //pastDirection = directionForward;

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

            if (cameraTransform.gameObject.GetComponentInChildren<CameraScript>().lookAt != enemieList[l])
                cameraTransform.gameObject.GetComponentInChildren<CameraScript>().ChangeTarget(enemieList[l]);


            Transform trans = rotationEnemies;
            Vector3 foTrans = new Vector3();
            foTrans = Vector3.Normalize(enemieList[l].transform.position - player.transform.position);
            foTrans.y = 0;
            trans.forward = foTrans;

            player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, trans.transform.rotation, 180);


            //Debug.DrawRay(player.transform.position, player.transform.forward * 2.0f, Color.black);
            //Debug.Log(" x.ValueMouse = " + Input.mousePosition.x);
            //Debug.Log(" enemie count = " + enemieList.Count + " now enemie = " + l + " y.value = " + Input.mouseScrollDelta.y);S
        }
    }
}


