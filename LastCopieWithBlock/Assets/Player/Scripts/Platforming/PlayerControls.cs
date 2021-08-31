using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    ////
    //////// VARIABLES
    ////
    #region ...
    #region :::PlayerComponents
    public Transform cameraTransform;
    private CharacterController controller;
    private Animator animator;
    #endregion

    #region :::Moving_Sprinting
    //<MOVE>
    private float move_Speed;
    private const float move_SpeedMax = 0.1f;
    private const float move_SpeedAcc = 0.01f;
    private const float move_SpeedRot = 10f;
    //<SPRINT>
    private float sprint_Speed;
    private const float sprint_SpeedMax = 0.1f;
    private const float sprint_SpeedAcc = 0.005f;
    private const float sprint_SpeedRot = 20f;
    //<TOTAL>
    [SerializeField]
    private float move_SpeedTotal;
    #endregion

    #region :::c_Climbing
    private static float c_ClimbingSpeedUp = 0.09f;
    private static float c_ClimbingSpeedForward = 0.007f;
    private bool c_IsClimbing;
    #endregion

    #region :::g_Gravity
    private float g_Force;
    private float g_ForwardSpeed;
    private const float g_ForceIncrement = 0.015f;
    private Vector3 g_vector;
    private Vector3 g_Forward;
    #endregion

    #region :::col_Colliders
    public bool col_ClimbingBox1;
    public bool col_ClimbingBox2;
    #endregion

    #region :::v_Vectors
    //<AXIS>
    [HideInInspector]
    public Vector3 v_Normal;
    [HideInInspector]
    public Vector3 v_Right;
    [HideInInspector]
    public Vector3 v_Direction;
    //Movement
    [HideInInspector]
    public Vector3 lastDirection;
    [HideInInspector]
    public Vector3 moveDirection;
    [HideInInspector]
    private Vector3 movement;
    #endregion

    #region :::Timer
    private float timer;
    private float timerTarget;
    private static float timerIncrement = 0.1f; // mudam isto, fodem tudo 
    #endregion
    #endregion

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        animator.SetLayerWeight(1, 1);
    }

    void FixedUpdate()
    {
        //////// SUDDEN TURN BLOCK
        #region ...
        if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(moveDirection)) > 90 && Quaternion.Angle(transform.rotation, Quaternion.LookRotation(moveDirection)) >= 0)
        {
            if (animator.GetBool("Move_Sprinting") == true) move_Speed = -move_SpeedMax - sprint_SpeedMax / 2f;
            else move_Speed = 0;
            animator.SetBool("Plat_SuddenTurn", true);
            move_Speed += move_SpeedAcc + sprint_SpeedAcc;
        }
        #endregion
        ////
        //////// MOVEMENT BLOCK
        ////
        #region ...
        M_Check_Ground(); //Verifica se o player esta no chão
        moveDirection.y = 0;
        #region Moving
        if (animator.GetBool("Move_Moving"))
        {
            if (move_Speed >= move_SpeedMax) move_Speed = move_SpeedMax;
            else move_Speed += move_SpeedAcc;
        }
        else
        {
            if (move_Speed <= 0) move_Speed = 0;
            else move_Speed -= move_SpeedAcc;
        }
        #endregion
        #region Sprinting
        if (animator.GetBool("Move_Sprinting"))
        {
            if (sprint_Speed >= sprint_SpeedMax) sprint_Speed = sprint_SpeedMax;
            else sprint_Speed += sprint_SpeedAcc;
        }
        else
        {
            if (sprint_Speed <= 0) sprint_Speed = 0;
            else sprint_Speed -= sprint_SpeedAcc;
        }
        #endregion
        #region MovePlayer
        if (!animator.GetBool("Plat_Climbing")
            && !animator.GetBool("Plat_WallRunning")
            && !animator.GetBool("Plat_WallRunningJump")
            && !animator.GetBool("Plat_Jump"))
        {
            if (!animator.GetBool("Move_MidAir"))
            {
                M_Apply_PlayerMovement();
            }
            else
            {
                M_Apply_Gravity();
            }
        }
        #endregion
        #endregion
        ////
        //////// CLIMBING BLOCK
        ////
        #region ...
        if (animator.GetBool("Plat_Climbing"))
        {
            M_Apply_Climb();
        }
        #endregion

    }

    ////
    //////// UPDATE()
    ////
    void Update()
    {
        ////
        //////// PLAYER AXIS
        ////
        #region ...
        v_Normal = Vector3.Normalize(Vector3.up);
        v_Right = Vector3.Normalize(Vector3.Cross(moveDirection, v_Normal));
        v_Direction = Vector3.Normalize(Vector3.Cross(v_Normal, v_Right));
        //VERIFY CLIMBING COLISIONS
        Debug.DrawRay(transform.position, v_Normal, Color.yellow);
        Debug.DrawRay(transform.position, v_Direction, Color.red);
        Debug.DrawRay(transform.position, v_Right, Color.blue);

        if (animator.GetBool("Move_MidAir")
            || animator.GetBool("Plat_Jump")
            || animator.GetBool("Plat_WallRunning")
            || animator.GetBool("Plat_WallRunningJump"))
        {
            M_Check_Climbing(1.5f, ref col_ClimbingBox1, 0.5f);
            M_Check_Climbing(2.4f, ref col_ClimbingBox2, 0.5f);
        }
        #endregion
        ////
        //////// MOVEMENT BLOCK
        ////
        #region ...
        if (Input.GetAxis("Horizontal") != 0
            || Input.GetAxis("Vertical") != 0
            && !animator.GetBool("Move_MidAir"))
        {
            #region GetInput Moving
            if (Input.GetAxis("Horizontal") < 0) moveDirection -= cameraTransform.right; //A
            if (Input.GetAxis("Horizontal") > 0) moveDirection += cameraTransform.right; //D
            if (Input.GetAxis("Vertical") > 0) moveDirection += cameraTransform.forward; //W
            if (Input.GetAxis("Vertical") < 0) moveDirection -= cameraTransform.forward; //S
            animator.SetBool("Move_Moving", true);
            #endregion
            #region GetInput Sprinting
            if (Input.GetAxis("Ctrl") > 0)
            {
                animator.SetBool("Move_Sprinting", true);
            }
            else
            {
                animator.SetBool("Move_Sprinting", false);
            }
        }
        else animator.SetBool("Move_Moving", false);
        #endregion

        #endregion
        ////
        //////// CLIMBING BLOCK
        ////
        #region ...
        if (col_ClimbingBox1
            && !col_ClimbingBox2
            && animator.GetBool("Move_MidAir")
            && !animator.GetBool("Plat_Slide"))
        {
            animator.SetBool("Plat_Climbing", true);
            animator.SetBool("Plat_Jump", false);
            animator.SetBool("Plat_WallRunning", false);
            animator.SetBool("Plat_WallRunningJump", false);
            animator.Play("Climbing");

            M_Set_GravitySpeed(c_ClimbingSpeedForward);
            M_Set_GravityDirection(v_Direction);
            c_IsClimbing = true;
        }
        //SE AINDA TIVER A DECORRER A ANIMAÇÃO, CLIMBING = TRUE
        M_Check_AnimationEnd("Climbing", "Plat_Climbing", 1);
        #endregion
    }

    ////
    //////// LATE UPDATE()
    ////
    void LateUpdate()
    {
        animator.SetBool("Plat_SuddenTurn", false); //DONT ASK WHY, IT WORKS GUYS
    }

    ////
    //////// M_METHODS
    ////
    #region ...
    #region INFO
    /* METHODS ORGANIZED BY
     * M_Enable_"" 
     *      Mais relacionado com os bools, que sao ativados nos eventos das animaçoes;
     * M_Apply_""
     *      Maioritariamente usado para aplicar forças e direçoes a vetores de movimento;
     * M_Check_""
     *      Verifica algo, returnando um bool;
     * M_Reset_""
     *      Muda valor de variaveis, relacionados com variaveis constantes, ou 0;
     * M_Set_""
     *      Muda valor de variaveis, relaticionados com os valores enviados para o metodo;
     * M_Align_""
     *      Alinha algo com algo;
     * M_Dec_""
     *      Decrementa um valor, dependendo do metodo usado;
     */
    #endregion
    //------------------------------------//
    private void M_Disable_Climbing()
    {
        c_IsClimbing = false;
    }
    public void M_Apply_Stop()
    {
        M_Apply_Movement(moveDirection, 0.0001f);
    }
    public bool M_Check_AnimationEnd(string animationName, string boolName, int layer)
    {
        if (animator.GetCurrentAnimatorStateInfo(layer).IsName(animationName))
        {
            animator.SetBool(boolName, true);
            return false;
            //Return false, ainda não acabou;
        }
        else
        {
            animator.SetBool(boolName, false);
            return true;
            //Return true, acabou;
        }
    }
    #region Climbing
    private void M_Apply_Climb()
    {
        M_Apply_Movement(g_Forward, c_ClimbingSpeedForward);
        if (c_IsClimbing)
        {
            M_Apply_Movement(v_Normal, c_ClimbingSpeedUp);
        }
        else M_Apply_Gravity();
    }
    private void M_Check_Climbing(float height, ref bool col, float hitDistance)
    {
        RaycastHit hit;
        Vector3 line = v_Direction;

        if (Physics.Raycast(transform.position + v_Normal * height, line, out hit, hitDistance))
        {
            col = true;
        }
        else col = false;
    }
    #endregion

    #region Gravity
    private void M_Check_Ground()
    {
        float hitDistance;
        if (!animator.GetBool("Move_MidAir"))
        {
            hitDistance = 0.35f;

            M_Set_GravityDirection(moveDirection);
            M_Set_GravitySpeed(move_SpeedMax);

            M_Reset_Gravity();
        }
        else
        {
            hitDistance = 0.20f;
        }

        if (Physics.Raycast(transform.position + new Vector3(0, 0.2f, 0), -transform.up, hitDistance))
        {
            animator.SetBool("Move_MidAir", false);
        }
        else
        {
            animator.SetBool("Move_MidAir", true);
        }
    }
    private void M_Reset_Gravity()
    {
        g_Force = 0;
    }
    public void M_Apply_Gravity()
    {
        //Frontal Force
        M_Apply_GravityForce(g_Forward, g_ForwardSpeed);
        //DownForce
        M_Apply_GravityForce(Vector3.down, g_Force);

        g_Force += g_ForceIncrement;
    }
    public void M_Set_GravityDirection(Vector3 direction)
    {
        g_Forward = direction;
    }
    public void M_Set_GravitySpeed(float speed)
    {
        g_ForwardSpeed = speed;
    }
    public void M_Apply_GravityForce(Vector3 direction, float speed)
    {
        g_vector = direction;
        movement = g_vector.normalized * speed;
        controller.Move(movement);
    }
    #endregion

    public void M_Set_Timer(float timerTarget)
    {
        this.timer = 0;
        this.timerTarget = timerTarget;
    }
    public void M_Dec_SlowDown(ref float speed, float decrement, float max)
    {
        speed -= decrement;
        if (speed < max / 2)
        {
            speed -= decrement;
        }
        if (speed < max / 3)
        {
            speed -= decrement;
        }
        if (speed < max / 4)
        {
            speed -= decrement;
        }
    }
    public void M_Dec_Value(ref float speed, float decrement)
    {
        speed -= decrement;
    }
    public bool M_Dec_Time()
    {
        if (timer < timerTarget)
        {
            timer += timerIncrement;
            return false;
        }
        else return true;
    }

    public void M_Set_PlayerHitBox(float height, Vector3 center, float radius)
    {
        controller.height = height;
        controller.center = center;
        controller.radius = radius;
    }
    public void M_Reset_PlayerHitBox()
    {
        controller.radius = 0.2f;
        controller.height = 1.6f;
        controller.center = new Vector3(0, 1f, 0);
    }

    public void M_Set_PlayerRotation(float angleX, float angleY, float angleZ)
    {
        transform.rotation *= Quaternion.Euler(angleX, angleY, angleZ);
    }

    public void M_Apply_Movement(Vector3 direction, float speed)
    {
        moveDirection = direction;
        movement = moveDirection.normalized * speed;
        controller.Move(movement);
    }
    private void M_Apply_PlayerMovement()
    {
        M_Apply_Movement(moveDirection, move_SpeedTotal);
        //MELHORAR ESTE CODIGO AQUI ABAIXO; ESTA MAL FEITO QUE DOI
        if (animator.GetBool("Move_Sprinting"))
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveDirection), sprint_SpeedRot); //RODAR CONFORME A CAMERA // RUNNING
        }
        else if (animator.GetBool("Move_Moving"))
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveDirection), move_SpeedRot); //RODAR CONFORME A CAMERA // MOVING
        }
        //NORMALIZAR DIRECTION
        moveDirection = moveDirection.normalized;
        //MOVE SPEED
        move_SpeedTotal = move_Speed + sprint_Speed;
        animator.SetFloat("Move_Speed", move_SpeedTotal);
    }
    #endregion
}