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
    public Animator animator;
    #endregion

    #region :::Moving_Sprinting
    //<MOVE>
    private float move_Speed;
    private const float move_SpeedMax = 0.1f;
    private const float move_SpeedAcc = 0.02f;
    private const float move_SpeedRot = 10f;
    //<SPRINT>
    private float sprint_Speed;
    private const float sprint_SpeedMax = 0.1f;
    private const float sprint_SpeedAcc = 0.005f;
    private const float sprint_SpeedRot = 20f;
    //<TOTAL>
    private float move_SpeedTotal;
    #endregion

    #region :::j_Jumping
    //<NORMAL>
    private float j_NormalHeight;
    private const float j_NormalHeightDec = 0.006f;
    private const float j_NormalHeightMax = 0.14f;
    private const float j_NormalSpeed = 0.1f;
    //<SPRINT>
    private float j_SprintHeight;
    private const float j_SprintHeightDec = 0.003f;
    private const float j_SprintHeightMax = 0.07f;
    private const float j_SprintSpeed = 0.2f;
    //<BOOLS>
    private bool j_IsJump;
    private bool j_IsJumpNormal, j_IsJumpSprinting;
    #endregion

    #region :::g_Gravity
    private float g_Force;
    private float g_ForwardSpeed;
    private const float g_ForceIncrement = 0.015f;
    private Vector3 g_Forward;
    #endregion

    #region :::col_Colliders
    public bool col_ClimbingBox1;
    public bool col_ClimbingBox2;
    public bool col_LeftWall;
    public bool col_RightWall;
    public bool col_FrontWall;
    #endregion

    #region :::wr_WallRunning
    //<SIDE>
    private float wr_SpeedSide;
    private const float wr_SpeedSideMax = 0.14f;
    private const float wr_SpeedSideDec = 0.0009f;

    private float wr_HeightSide;
    private const float wr_HeightSideMax = 0.05f;
    private const float wr_HeightSideDec = 0.001f;
    private const float wr_SideTime = 7f;
    //<FRONT>
    private float wr_SpeedFront;
    private const float wr_SpeedFrontMax = 0.16f;
    private const float wr_SpeedFrontDec = 0.003f;
    private const float wr_FrontTime = 4f;
    //<JUMP>
    private const float wr_JumpSpeed = 0.2f;
    private float wr_JumpChainTime;

    private float wr_JumpHeight;
    private const float wr_JumpHeightDec = 0.006f;
    private const float wr_JumpHeightMax = 0.14f;
    //<BOOLS>
    private bool wr_IsJump;
    private bool wr_IsJumpLeft, wr_IsJumpRight;
    [SerializeField]
    private bool wr_JumpChain;
    #endregion

    #region :::Vectors
    //<AXIS>
    private Vector3 v_playerNormal;
    private Vector3 v_playerRight;
    private Vector3 v_playerDir;
    //Movement
    private Vector3 lastDirection;
    private Vector3 moveDirection;
    private Vector3 movement;
    #endregion

    #region :::Timer
    private float timer;
    private float timerTarget;
    private static float timerIncrement = 0.1f; // mudam isto, fodem tudo 
    #endregion
    #endregion
    ////
    //////// START()
    ////
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
       // animator.SetLayerWeight(1, 1);

    }

    ////
    //////// FIXED UPDATE()
    ////
    void FixedUpdate()
    {
        //////// SUDDEN TURN BLOCK
        #region ...
        if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(moveDirection)) > 90)
        {
            if (animator.GetBool("Sprinting") == true) move_Speed = -move_SpeedMax - sprint_SpeedMax / 2f;
            else move_Speed = 0;
            animator.SetBool("Turn", true);
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
        if (animator.GetBool("Moving"))
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
        if (animator.GetBool("Sprinting"))
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
        if (!animator.GetBool("Climbing")
            && !animator.GetBool("Wall Running")
            && !animator.GetBool("Wall Running Jump")
            && !animator.GetBool("Jump"))
        {
            if (!animator.GetBool("MidAir"))
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
        if (animator.GetBool("Climbing"))
        {
            M_Apply_Climb();
        }
        #endregion
        ////
        //////// JUMPING BLOCK
        ////
        #region ...
        if (!animator.GetBool("Jump"))
        {
            j_IsJump = false;
            j_IsJumpNormal = false;
            j_IsJumpSprinting = false;
            animator.SetBool("Sprinting", false);
        }
        if (animator.GetBool("Jump") && !j_IsJump)
        {
            M_Apply_Stop();
        }
        if (j_IsJump)
        {
            if (j_IsJumpNormal)
            {
                M_Apply_JumpMoving();
            }
            else if (j_IsJumpSprinting)
            {
                M_Apply_JumpSprint();
            }
            //Verifica se o player ja subiu o suficiente
            if (j_NormalHeight < 0 || j_SprintHeight < 0)
            {
                animator.SetBool("Jump", false);
            }
        }
        #endregion
        ////
        //////// WALL RUNNING BLOCK
        ////
        #region ...
        //<WALLRUNNING>
        if (animator.GetBool("Wall Running"))
        {
            if (M_Dec_Time())
            {
                animator.SetBool("Wall Running", false);
            }
            M_Apply_WallRunning();
        }
        //<WALLRUNNING JUMP>
        if (animator.GetBool("Wall Running Jump") && !wr_IsJump)
        {
            M_Apply_Stop();
        }
        if (wr_IsJump)
        {
            M_Apply_WallRunningJump();
            //M_Enable_WallRunningChain(); // NOT WORKING AINDA, ARRANJAR MANEIRA DE ATIVAR MEIO SALTO; 

            if (wr_JumpHeight < 0)
            {
                animator.SetBool("Wall Running Jump", false);
                wr_JumpChain = false;
                wr_IsJump = false;
                v_playerDir = lastDirection;
            }
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
        v_playerNormal = Vector3.Normalize(Vector3.up);
        v_playerRight = Vector3.Normalize(Vector3.Cross(moveDirection, v_playerNormal));
        v_playerDir = Vector3.Normalize(Vector3.Cross(v_playerNormal, v_playerRight));
        #endregion
        ////
        //////// MOVEMENT BLOCK
        ////
        #region ...
        if (Input.GetAxis("Horizontal") != 0 
            || Input.GetAxis("Vertical") != 0
            && !animator.GetBool("MidAir"))
        {
            #region GetInput Moving
            if (Input.GetAxis("Horizontal") < 0) moveDirection -= cameraTransform.right; //A
            if (Input.GetAxis("Horizontal") > 0) moveDirection += cameraTransform.right; //D
            if (Input.GetAxis("Vertical") > 0) moveDirection += cameraTransform.forward; //W
            if (Input.GetAxis("Vertical") < 0) moveDirection -= cameraTransform.forward; //S
            animator.SetBool("Moving", true);
            #endregion
            #region GetInput Sprinting
            if (Input.GetAxis("Left Ctrl") > 0)
            {
                animator.SetBool("Sprinting", true);
            }
            else
            {
                animator.SetBool("Sprinting", false);
            }
        }
        else animator.SetBool("Moving", false);
        #endregion
            #region GetInput FreeRunning
            if (Input.GetAxis("Shift") != 0)
               animator.SetBool("FreeRunning", true);
            else animator.SetBool("FreeRunning", false);
        #endregion
        #endregion
        ////
        //////// JUMPING BLOCK
        ////
        #region ...
        if (Input.GetKeyDown("space")
            && !animator.GetBool("MidAir")
            && !animator.GetBool("Jump"))
        {
            animator.SetBool("Jump", true);
            if (animator.GetBool("Sprinting"))
            {
                animator.Play("Jump Sprinting");
                j_IsJumpSprinting = true;
                j_IsJumpNormal = false;
            }
            else
            {
                animator.Play("Jump Moving");
                j_IsJumpSprinting = false;
                j_IsJumpNormal = true;
            }
            M_Reset_JumpVariables();
        }
        #endregion
        ////
        //////// SLIDE DASH BLOCK
        ////
        #region ...
        if (Input.GetKeyDown("left alt")
            && !animator.GetBool("Slide")
            && animator.GetBool("Sprinting")
            && !animator.GetBool("MidAir"))
        {
            animator.SetBool("Slide", true);
        }
        if (animator.GetBool("Slide"))
        {
            if (M_Check_AnimationEnd("Slide", "Slide"))
            {
                M_Set_PlayerHitBox(0.2f, new Vector3(0, 0.2f, 0), 0.1f);
            }
            else
            {
                M_Reset_PlayerHitBox();
            }
        }
        #endregion
        ////
        //////// CLIMBING BLOCK
        ////
        #region ...
        if (col_ClimbingBox1
            && !col_ClimbingBox2
            && animator.GetBool("MidAir")
            && !animator.GetBool("Slide"))
        {
            animator.SetBool("Climbing", true);
            animator.SetBool("Jump", false);
            animator.SetBool("Wall Running", false);
            animator.SetBool("Wall Running Jump", false);
            animator.Play("Climbing");
        }
        //SE AINDA TIVER A DECORRER A ANIMAÇÃO, CLIMBING = TRUE
        M_Check_AnimationEnd("Climbing", "Climbing");
    
        #endregion
        ////
        //////// WALL RUNNING BLOCK
        ////
        #region ...
        if ((col_LeftWall || col_RightWall || col_FrontWall)
            && animator.GetBool("Moving")
            && animator.GetBool("FreeRunning")
            && !animator.GetBool("MidAir")
            && !animator.GetBool("Jump")
            && !animator.GetBool("Wall Running Jump")
            && !animator.GetBool("Wall Running"))
        {
            animator.SetBool("Wall Running", true);
            if (col_FrontWall)
            {
                animator.Play("Wall Running Forward");
                wr_IsJumpLeft = false;
                wr_IsJumpRight = false;
                M_Set_Timer(wr_FrontTime);
            }
            else if (col_LeftWall)
            {
                animator.Play("Wall Running Left");
                wr_IsJumpLeft = true;
                wr_IsJumpRight = false;
                M_Set_Timer(wr_SideTime);
            }
            else if (col_RightWall)
            {
                animator.Play("Wall Running Right");
                wr_IsJumpLeft = false;
                wr_IsJumpRight = true;
                M_Set_Timer(wr_SideTime);
            }
            M_Set_GravityDirection(v_playerDir, wr_SpeedSideMax);
            M_Align_PlayerWall();
            M_Reset_WallRunningVariables();
        }
        //VERIFICA SE O PLAYER AINDA ESTA A FAZER WALLRUNNING
        if (animator.GetBool("Wall Running"))
        {
            //VERIFICA SE PAROU DE PRIMIR O SHIFT
            if (!animator.GetBool("FreeRunning"))
            {
                //Play Animation
                animator.Play("Jump Mid Air");
                //Bools
                animator.SetBool("Wall Running", false);
                col_LeftWall = false;
                col_RightWall = false;
                col_FrontWall = false;
            }
            //// WALL JUMP
            if (Input.GetKeyDown("space"))
            {
                //Play Animation
                animator.Play("Wall Running Jump");
                //Bools
                animator.SetBool("Wall Running", false);
                animator.SetBool("Wall Running Jump", true);
                //Rotate Torwads Wall
                if (col_LeftWall)
                    M_Set_PlayerRotation(0,-90,0);
                if (col_RightWall)
                    M_Set_PlayerRotation(0,90,0);
            }
        }
        else if (!col_LeftWall && !col_RightWall && !col_FrontWall) animator.SetBool("Wall Running", false);
        ////JUMPING FROM WALL TO WALL
        //if (animator.GetBool("Wall Running Jump") && col_FrontWall && animator.GetBool("FreeRunning") && wr_JumpChain)
        //{
        //    //Play Animation
        //    animator.Play("Wall Running Forward");
        //    //Bools
        //    animator.SetBool("Wall Running", true);
        //    animator.SetBool("Wall Running Jump", false);
        //    wr_IsJumpLeft = false;
        //    wr_IsJumpRight = false;
        //    //Methods
        //    M_Set_Timer(wr_FrontTime / 4);
        //    M_Reset_WallRunningVariables();
        //    M_Align_PlayerWall();
        //}
        #endregion
    }

    ////
    //////// LATE UPDATE()
    ////
    void LateUpdate()
    {
        animator.SetBool("Turn", false); //DONT ASK WHY, IT WORKS GUYS
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
    #region Animation
    private void M_Enable_Jump()
    {
        j_IsJump = true;
    }
    private void M_Enable_WallJump()
    {
        wr_IsJump = true;
    }
    private void M_Apply_Stop()
    {
        M_Apply_Movement(moveDirection, 0.0001f);
    }
    private bool M_Check_AnimationEnd(string animationName, string boolName)
    {
        if (animator.GetCurrentAnimatorStateInfo(1).IsName(animationName))
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
    #endregion
    //------------------------------------//
    #region Climbing
    private void M_Apply_Climb()
    {
        M_Apply_Movement(v_playerNormal, 0.06f);
    }
    #endregion
    //------------------------------------//
    #region Gravity
    private void M_Check_Ground()
    {
        float hitDistance;
        if (!animator.GetBool("MidAir"))
        {
            hitDistance = 0.35f;

            M_Set_GravityDirection(moveDirection, move_SpeedMax);
            M_Reset_Gravity();
        }
        else
        {
            hitDistance = 0.20f;
        }

        if (Physics.Raycast(transform.position + new Vector3(0, 0.2f, 0), -transform.up, hitDistance))
        {
            animator.SetBool("MidAir", false);
        }
        else
        {
            animator.SetBool("MidAir", true);
        }
    }
    private void M_Reset_Gravity()
    {
        g_Force = 0;
    }
    private void M_Apply_Gravity()
    {
        //Frontal Force
        M_Apply_Movement(g_Forward, g_ForwardSpeed);
        //DownForce
        M_Apply_Movement(Vector3.down, g_Force);

        g_Force += g_ForceIncrement;
        moveDirection.y = 0; //Prevent mid air bug
    }
    private void M_Apply_PlayerMovement()
    {
        M_Apply_Movement(moveDirection, move_SpeedTotal);
        //MELHORAR ESTE CODIGO AQUI ABAIXO; ESTA MAL FEITO QUE DOI
        if (animator.GetBool("Sprinting"))
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveDirection), sprint_SpeedRot); //RODAR CONFORME A CAMERA // RUNNING
        }
        else if(animator.GetBool("Moving"))
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveDirection), move_SpeedRot); //RODAR CONFORME A CAMERA // MOVING
        }
        //NORMALIZAR DIRECTION
        moveDirection = moveDirection.normalized;
        //MOVE SPEED
        move_SpeedTotal = move_Speed + sprint_Speed;
        animator.SetFloat("Speed", move_SpeedTotal);
    }
    private void M_Set_GravityDirection(Vector3 direction, float speed)
    {
        g_Forward = direction;
        g_ForwardSpeed = speed;
    }
    #endregion
    //------------------------------------//
    #region Jump
    private void M_Reset_JumpVariables()
    {
        j_NormalHeight = j_NormalHeightMax;
        j_SprintHeight = j_SprintHeightMax;
        lastDirection = v_playerDir;
    }
    private void M_Apply_JumpMoving()
    {
        //<UP FORCE>
        M_Apply_Movement(v_playerNormal, j_NormalHeight);

        //<FORWARD FORCE>
        M_Apply_Movement(lastDirection, j_NormalSpeed);

        M_Set_GravityDirection(v_playerDir, j_NormalSpeed);
        M_Dec_Value(ref j_NormalHeight, j_NormalHeightDec);
    }
    private void M_Apply_JumpSprint()
    {
        //UP FORCE
        M_Apply_Movement(v_playerNormal, j_SprintHeight);

        //FORWARD FORCE
        M_Apply_Movement(lastDirection, j_SprintSpeed);

        M_Set_GravityDirection(v_playerDir, j_SprintSpeed);
        M_Dec_Value(ref j_SprintHeight, j_SprintHeightDec);
    }
    #endregion
    //------------------------------------//
    #region Wall_Running
        
    private void M_Reset_WallRunningVariables()
    {
        #region INFO
        /*      Esta função tem como trabalho, dar reset ás varias velocidades que são aplicadas
         * nos diferentes tipos de movimentos WallRunning;
         */
        #endregion
        //<SIDE WR>
        wr_SpeedSide = wr_SpeedSideMax;
        wr_HeightSide = wr_HeightSideMax;
        //<FRONT WR>
        wr_SpeedFront = wr_SpeedFrontMax;
        //<JUMP WR>
        wr_JumpHeight = wr_JumpHeightMax;
        wr_JumpChainTime = 4f;
    }
    private void M_Apply_WallRunning()
    {
        #region INFO
        /*      Nesta função, a direçao do player (frente ou cima), é aplicada conforme o tipo de salto que é possivel aplicar
         * neste caso, se o player poder saltar para a direita ou para a esquerda, quer dizer que está a fazer um side
         * wallRunning. Se não, é aplicado uma direção de wallRunning Frontal.
         *      Ao movimento é aplicada uma velocidade (wr_SpeedSide, wr_SpeedFront) que é decrementada com a função M_Dec_SlowDown
         *      A Variavel g_ForwardForce igual a moveDirection, para acabando o WallRun, o player cair gravitalmente nessa direção
         */
        #endregion
        if (wr_IsJumpLeft || wr_IsJumpRight) //Lado
        {
            if (wr_HeightSide > 0)
            {
                //Up Direction
                M_Apply_Movement(v_playerNormal, wr_HeightSide);
                M_Dec_Value(ref wr_HeightSide, wr_HeightSideDec);
            }
            //FrontalDirection
            M_Apply_Movement(lastDirection, wr_SpeedSide);
            M_Dec_SlowDown(ref wr_SpeedSide, wr_SpeedSideDec, wr_SpeedSideMax);
            //Velocidade Animação
            animator.SetFloat("A_Speed", wr_SpeedSide * 15);
        }
        else
        {
            M_Apply_Movement(v_playerNormal, wr_SpeedFront);
            M_Dec_SlowDown(ref wr_SpeedFront, wr_SpeedFrontDec, wr_SpeedFrontMax);
            //Velocidade Animação
            animator.SetFloat("A_Speed", wr_SpeedFront * 15);
        }
    }
    private void M_Apply_WallRunningJump()
    {
        #region INFO
        /*      Ao começar o WallRunningJump, é calculada a direçao que este vai tomar, como tambem a rotação necessaria
         * para o player ficar alinhado com o vetor.
         *      É adicionado ao vetor movimento, uma altura, que vai "Inclinar" o vetor movimento, para um salto
         * mais alto;
         *      Ao movimento é aplicado a velocidade do salto, que no fim da função é decrementado, usando a funçao M_Dec_SlowDown
         *      A variavel g_ForwardForce igual a moveDirection, para acabando o salto, o player cair gravitalmente nessa direção
         */
        #endregion
        //<UP FORCE>
        M_Apply_Movement(v_playerNormal, wr_JumpHeight);
        M_Dec_Value(ref wr_JumpHeight, wr_JumpHeightDec);
        //<FORWARD FORCE>
        if (wr_IsJumpLeft)
        {
            M_Apply_Movement(Vector3.Cross(-lastDirection, Vector3.up), wr_JumpSpeed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveDirection.normalized), move_SpeedRot);
        }
        else if (wr_IsJumpRight)
        {
            M_Apply_Movement(Vector3.Cross(lastDirection, Vector3.up), wr_JumpSpeed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveDirection.normalized), move_SpeedRot);
        }
        else
        {
            M_Apply_Movement(-lastDirection, wr_JumpSpeed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveDirection.normalized), sprint_SpeedRot);
        }
        M_Set_GravityDirection(v_playerDir, wr_JumpSpeed); // PRECISA DE ESTAR AQUI POR CAUSA DO PLAYER RODAR NA ANIMAÇAO
    }
    private void M_Align_PlayerWall()
    {
        #region INFO
        /*
         Lança um PhysicsRaycast para o lado player, returnando a normal da parede,
         com isto conseguimos alinhar o vetor de movimento e a rotação do player;
         */
        #endregion
        RaycastHit hit;
        Vector3 line;
        if (col_FrontWall)
        {
            line = v_playerDir;
        }
        else if (col_RightWall)
        {
            line = v_playerRight;
        }
        else
        {
            line = -v_playerRight;
        }

        if (Physics.Raycast(transform.position, line, out hit))
        {
            if (col_FrontWall)
            {
                lastDirection = Quaternion.FromToRotation(Vector3.back, hit.normal) * Vector3.forward;
                transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
            }
            else if (col_RightWall)
            {
                lastDirection = Quaternion.FromToRotation(Vector3.right, hit.normal) * Vector3.forward;
                transform.rotation = Quaternion.FromToRotation(Vector3.right, hit.normal);
            }
            else
            {
                lastDirection = Quaternion.FromToRotation(Vector3.left, hit.normal) * Vector3.forward;
                transform.rotation = Quaternion.FromToRotation(Vector3.left, hit.normal);
            }
        }
    }
    private void M_Enable_WallRunningChain()
    {
        #region INFO
        /*O player pode voltar a fazer wallrunning frontal, 
         * se entrar em contacto com uma parede no intermedio do salto
         * |||||   ||||| -> JumpTIme
         *       ^
         *       |
         * wr_JumpChain = true neste espaço de tempo
         */
        #endregion
        wr_JumpChainTime -= timerIncrement;
        if (wr_JumpChainTime > 5 / 2 && wr_JumpChainTime < (5 / 3) * 2)
        {
            wr_JumpChain = true;
        }
        else wr_JumpChain = false;
    }
    #endregion
    //------------------------------------//
    #region Decrements
    private void M_Dec_SlowDown(ref float speed, float decrement, float max)
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
    private float M_Dec_UpDown(float speed, float decrement, float max)
    {
        float speed2 = 0;
        if (speed < max)
        {
            if (speed >= 0)
            {
                speed += decrement;
            }
            speed2 = max;
            return speed;
        }
        else
        {
            speed2 -= decrement;
            return speed2;
        }
    }
    private void M_Dec_Value(ref float speed, float decrement)
    {
        speed -= decrement;
    }
    private bool M_Dec_Time()
    {
        if (timer < timerTarget)
        {
            timer += timerIncrement;
            return false;
        }
        else return true;
    }
    #endregion
    //ROTATE PLAYER
    private void M_Set_PlayerRotation(float angleX,float angleY, float angleZ)
    {
        transform.rotation *= Quaternion.Euler(angleX, angleY, angleZ);
    }
    //CHANGE PLAYER HITBOX
    private void M_Set_PlayerHitBox(float height, Vector3 center, float radius)
    {
        controller.height = height;
        controller.center = center;
        controller.radius = radius;
    }
    private void M_Reset_PlayerHitBox()
    {
        controller.height = 1.80f;
        controller.center = new Vector3(0, 0.9f, 0);
    }
    //TIMERS
    private void M_Set_Timer(float timerTarget)
    {
        this.timer = 0;
        this.timerTarget = timerTarget;
    }

    private void M_Apply_Movement(Vector3 direction, float speed)
    {
        moveDirection = direction;
        movement = moveDirection.normalized * speed;
        controller.Move(movement);
    }
    //asdasd
    #endregion
}

