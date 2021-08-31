using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platforming_Jumping : MonoBehaviour
{
    #region -----Variables
    private PlayerControls player;
    private Animator animator;
    //<NORMAL>
    private float j_NormalHeight;
    private const float j_NormalHeightDec = 0.01f;
    private const float j_NormalHeightMax = 0.17f;
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
        if (!animator.GetBool("Plat_Jump"))
        {
            j_IsJump = false;
            j_IsJumpNormal = false;
            j_IsJumpSprinting = false;
            animator.SetBool("Move_Sprinting", false);
        }
        if (animator.GetBool("Plat_Jump") && !j_IsJump)
        {
            player.M_Apply_Stop();
        }
        if (j_IsJump)
        {
            if (j_IsJumpNormal)
            {
                J_Apply_JumpMoving();
            }
            else if (j_IsJumpSprinting)
            {
                J_Apply_JumpSprint();
            }
            //Verifica se o player ja subiu o suficiente
            if (j_NormalHeight < 0 || j_SprintHeight < 0)
            {
                animator.SetBool("Plat_Jump", false);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("space")
            && !animator.GetBool("Move_MidAir")
            && !animator.GetBool("Plat_Jump")
            && !animator.GetBool("Plat_Slide"))
        {
            animator.SetBool("Plat_Jump", true);
            if (animator.GetBool("Move_Sprinting"))
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
            J_Reset_JumpVariables();
        }
    }

    #region -----Methods
    private void J_Apply_JumpMoving()
    {
        //<UP FORCE>
        player.M_Apply_Movement(player.v_Normal, j_NormalHeight);

        //<FORWARD FORCE>
        player.M_Apply_Movement(player.lastDirection, j_NormalSpeed);

        player.M_Set_GravitySpeed(j_NormalSpeed);
        player.M_Set_GravityDirection(player.v_Direction);

        player.M_Dec_Value(ref j_NormalHeight, j_NormalHeightDec);
    }
    private void J_Apply_JumpSprint()
    {
        //UP FORCE
        player.M_Apply_Movement(player.v_Normal, j_SprintHeight);

        //FORWARD FORCE
        player.M_Apply_Movement(player.lastDirection, j_SprintSpeed);

        player.M_Set_GravitySpeed(j_SprintSpeed);
        player.M_Set_GravityDirection(player.v_Direction);

        player.M_Dec_Value(ref j_SprintHeight, j_SprintHeightDec);
    }
    private void J_Enable_Jump()
    {
        j_IsJump = true;
    }
    private void J_Reset_JumpVariables()
    {
        j_NormalHeight = j_NormalHeightMax;
        j_SprintHeight = j_SprintHeightMax;
        player.lastDirection = player.v_Direction;
    }
    #endregion
}
