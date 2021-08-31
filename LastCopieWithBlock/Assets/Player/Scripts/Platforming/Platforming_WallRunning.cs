using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platforming_WallRunning : MonoBehaviour {

    #region -----Variables
    private PlayerControls player;
    private Animator animator;
    //<COLLIDERS>
    private bool col_LeftWall;
    private bool col_RightWall;
    private bool col_FrontWall;
    //<SIDE>
    private float wr_SpeedSide;
    private const float wr_SpeedSideMax = 0.14f;
    private const float wr_SpeedSideDec = 0.0009f;
    private float wr_HeightSide;
    private const float wr_HeightSideMax = 0.05f;
    private const float wr_HeightSideDec = 0.001f;
    private const float wr_SideTime = 9f;
    //<FRONT>
    private float wr_SpeedFront;
    private const float wr_SpeedFrontMax = 0.16f;
    private const float wr_SpeedFrontDec = 0.003f;
    private const float wr_FrontTime = 4f;
    //<JUMP>
    private const float wr_JumpSpeed = 0.2f;
    private float wr_JumpHeight;
    private const float wr_RotationSpeed = 30f;
    private const float wr_JumpHeightDec = 0.006f;
    private const float wr_JumpHeightMax = 0.14f;
    //<BOOLS>
    private bool wr_IsJump;
    private bool wr_IsJumpChain;
    private bool wr_JumpLeft, wr_JumpRight;
    #endregion

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        player = GetComponent<PlayerControls>();
        animator = GetComponent<Animator>();
        animator.SetLayerWeight(1, 1);
    }

    void FixedUpdate()
    {
        //<WALLRUNNING>
        if (animator.GetBool("Plat_WallRunning"))
        {
            if (player.M_Dec_Time())
            {
                animator.SetBool("Plat_WallRunning", false);
            }
            Apply_WallRunning();
        }
        //<WALLRUNNING JUMP>
        if (animator.GetBool("Plat_WallRunningJump") && !wr_IsJump)
        {
            player.M_Apply_Stop();
        }
        if (wr_IsJump)
        {
            Apply_WallRunningJump();
            if (wr_JumpHeight < 0)
            {
                animator.SetBool("Plat_WallRunningJump", false);
                wr_IsJump = false;
                wr_IsJumpChain = false;
            }
        }
    }

    void Update()
    {
        if (!Check_WallCollision(player.v_Direction, ref col_FrontWall, 0.5f))
        {
            Check_WallCollision(player.v_Right, ref col_LeftWall, 1f);
            Check_WallCollision(-player.v_Right, ref col_RightWall, 1f);
        }

        if (Input.GetAxis("Shift") != 0)
            animator.SetBool("Plat_InputKey", true);
        else animator.SetBool("Plat_InputKey", false);


        if ((col_LeftWall || col_RightWall || col_FrontWall)
            && animator.GetBool("Move_Moving")
            && animator.GetBool("Plat_InputKey")
            && !animator.GetBool("Move_MidAir")
            && !animator.GetBool("Plat_Jump")
            && !animator.GetBool("Plat_WallRunningJump")
            && !animator.GetBool("Plat_WallRunning"))
        {
            animator.SetBool("Plat_WallRunning", true);
            if (col_FrontWall)
            {
                animator.Play("Wall Running Forward");
                wr_JumpLeft = false;
                wr_JumpRight = false;
                player.M_Set_Timer(wr_FrontTime);
            }
            else if (col_LeftWall)
            {
                animator.Play("Wall Running Left");
                wr_JumpLeft = true;
                wr_JumpRight = false;
                player.M_Set_Timer(wr_SideTime);
            }
            else if (col_RightWall)
            {
                animator.Play("Wall Running Right");
                wr_JumpLeft = false;
                wr_JumpRight = true;
                player.M_Set_Timer(wr_SideTime);
            }
            player.M_Set_GravityDirection(player.v_Direction);
            player.M_Set_GravitySpeed(wr_SpeedSide / 2);

            Align_PlayerWall();
            Reset_WallRunningVariables();
        }
        //VERIFICA SE O PLAYER AINDA ESTA A FAZER WALLRUNNING
        if (animator.GetBool("Plat_WallRunning"))
        {
            if (wr_JumpLeft && !col_LeftWall)
            {
                animator.SetBool("Plat_WallRunning", false);
            }
            else if (wr_JumpRight && !col_RightWall)
            {
                animator.SetBool("Plat_WallRunning", false);
            }

            //VERIFICA SE PAROU DE PRIMIR O SHIFT
            if (!animator.GetBool("Plat_InputKey"))
            {
                animator.Play("Jump Mid Air");
                animator.SetBool("Plat_WallRunning", false);
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
                animator.SetBool("Plat_WallRunning", false);
                animator.SetBool("Plat_WallRunningJump", true);
                //Rotate Torwads Wall
                if (col_LeftWall)
                    player.M_Set_PlayerRotation(0, -90, 0);
                if (col_RightWall)
                    player.M_Set_PlayerRotation(0, 90, 0);
            }
        }
        ////JUMPING FROM WALL TO WALL
        if (animator.GetBool("Plat_WallRunningJump") && col_FrontWall && animator.GetBool("Plat_InputKey") && wr_IsJumpChain)
        {
            //Play Animation
            animator.Play("Wall Running Forward");
            //Bools
            animator.SetBool("Plat_WallRunning", true);
            animator.SetBool("Plat_WallRunningJump", false);
            wr_JumpLeft = false;
            wr_JumpRight = false;
            wr_IsJump = false;
            wr_IsJumpChain = false;
            //Methods
            player.M_Set_Timer(wr_FrontTime / 2);
            Reset_WallRunningVariables();
            Align_PlayerWall();
        }
    }

    #region -----Methods
    private void Apply_WallRunning()
    {
        #region INFO
        /*      Nesta função, a direçao do player (frente ou cima), é aplicada conforme o tipo de salto que é possivel aplicar
         * neste caso, se o player poder saltar para a direita ou para a esquerda, quer dizer que está a fazer um side
         * wallRunning. Se não, é aplicado uma direção de wallRunning Frontal.
         *      Ao movimento é aplicada uma velocidade (wr_SpeedSide, wr_SpeedFront) que é decrementada com a função M_Dec_SlowDown
         *      A Variavel g_ForwardForce igual a moveDirection, para acabando o WallRun, o player cair gravitalmente nessa direção
         */
        #endregion
        if (col_FrontWall)
        {
            player.M_Apply_Movement(player.v_Normal, wr_SpeedFront);
            player.M_Dec_SlowDown(ref wr_SpeedFront, wr_SpeedFrontDec, wr_SpeedFrontMax);
            //Velocidade Animação
            animator.SetFloat("Anim_Speed", wr_SpeedFront * 15);
        }
        else
        {
            if (wr_HeightSide > 0)
            {
                //Up Direction
                player.M_Apply_Movement(player.v_Normal, wr_HeightSide);
                player.M_Dec_Value(ref wr_HeightSide, wr_HeightSideDec);
            }
            //FrontalDirection
            player.M_Apply_Movement(player.lastDirection, wr_SpeedSide);
            player.M_Dec_SlowDown(ref wr_SpeedSide, wr_SpeedSideDec, wr_SpeedSideMax);
            //Velocidade Animação
            animator.SetFloat("Anim_Speed", wr_SpeedSide * 15);
        }
    }
    private void Apply_WallRunningJump()
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
        player.M_Apply_Movement(player.v_Normal, wr_JumpHeight);
        player.M_Dec_Value(ref wr_JumpHeight, wr_JumpHeightDec);
        //<FORWARD FORCE>
        if (wr_JumpLeft)
        {
            player.M_Apply_Movement(Vector3.Cross(-player.lastDirection, Vector3.up), wr_JumpSpeed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(player.moveDirection.normalized), wr_RotationSpeed);
        }
        else if (wr_JumpRight)
        {
            player.M_Apply_Movement(Vector3.Cross(player.lastDirection, Vector3.up), wr_JumpSpeed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(player.moveDirection.normalized), wr_RotationSpeed);
        }
        else
        {
            player.M_Apply_Movement(-player.lastDirection, wr_JumpSpeed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(player.moveDirection.normalized), wr_RotationSpeed);
        }
        player.M_Set_GravityDirection(player.v_Direction);
        player.M_Set_GravitySpeed(wr_JumpSpeed); // PRECISA DE ESTAR AQUI POR CAUSA DO PLAYER RODAR NA ANIMAÇAO
    }
    private void Align_PlayerWall()
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
            line = player.v_Direction;
        }
        else if (col_RightWall)
        {
            line = player.v_Right;
        }
        else
        {
            line = -player.v_Right;
        }

        if (Physics.Raycast(transform.position, line, out hit))
        {
            if (col_FrontWall)
            {
                player.lastDirection = Quaternion.FromToRotation(Vector3.back, hit.normal) * Vector3.forward;
                transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
            }
            else if (col_RightWall)
            {
                player.lastDirection = Quaternion.FromToRotation(Vector3.right, hit.normal) * Vector3.forward;
                transform.rotation = Quaternion.FromToRotation(Vector3.right, hit.normal);
            }
            else
            {
                player.lastDirection = Quaternion.FromToRotation(Vector3.left, hit.normal) * Vector3.forward;
                transform.rotation = Quaternion.FromToRotation(Vector3.left, hit.normal);
            }
        }
    }
    private void Enable_WallJump()
    {
        wr_IsJump = true;
    }
    private void Enable_WallJumpChain()
    {
        wr_IsJumpChain = true;
    }
    private bool Check_WallCollision(Vector3 direction, ref bool wall, float hitDistance)
    {
        RaycastHit hit;
        Vector3 line = direction;

        if (Physics.Raycast(transform.position + player.v_Normal, line, out hit, hitDistance))
        {
            wall = true;
            return true;
        }
        else
        {
            wall = false;
            return false;
        }
    }
    private void Reset_WallRunningVariables()
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
    }
    #endregion
}