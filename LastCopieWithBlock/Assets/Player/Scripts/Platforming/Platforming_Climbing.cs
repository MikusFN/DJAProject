//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Platforming_Climbing : MonoBehaviour {

//    private PlayerControls player;
//    private Animator animator;

//    private static float c_ClimbingSpeedUp = 0.09f;
//    private static float c_ClimbingSpeedForward = 0.025f;
//    private bool c_IsClimbing;

//    public bool col_ClimbingBox1;
//    public bool col_ClimbingBox2;

//    void Start () {
//        QualitySettings.vSyncCount = 0;
//        Application.targetFrameRate = 60;
//        player = GetComponent<PlayerControls>();
//        animator = GetComponent<Animator>();
//        animator.SetLayerWeight(1, 1);
//    }
	
//    void FixedUpddate()
//    {
//        if (animator.GetBool("Plat_Climbing"))
//        {
//            M_Apply_Climb();
//        }
//    }

//    // Update is called once per frame
//    void Update () {
//        //VERIFY CLIMBING COLISIONS
//        if (animator.GetBool("Move_MidAir")
//            || animator.GetBool("Plat_WallRunning")
//            || animator.GetBool("Plat_WallRunningJump"))
//        {
//            M_Check_Climbing(1.3f, ref col_ClimbingBox1, 0.5f);
//            M_Check_Climbing(2.0f, ref col_ClimbingBox2, 0.5f);
//        }

//        ////
//        //////// CLIMBING BLOCK
//        ////
//        #region ...
//        if (col_ClimbingBox1
//            && !col_ClimbingBox2
//            && animator.GetBool("Move_MidAir")
//            && !animator.GetBool("Plat_Slide"))
//        {
//            animator.SetBool("Plat_Climbing", true);
//            animator.SetBool("Plat_Jump", false);
//            animator.SetBool("Plat_WallRunning", false);
//            animator.SetBool("Plat_WallRunningJump", false);
//            animator.Play("Climbing");

//            player.M_Set_GravitySpeed(c_ClimbingSpeedForward);
//            player.M_Set_GravityDirection(player.v_Direction);
//            c_IsClimbing = true;
//        }
//        //SE AINDA TIVER A DECORRER A ANIMAÇÃO, CLIMBING = TRUE
//        player.M_Check_AnimationEnd("Climbing", "Plat_Climbing", 1);
//        #endregion
//    }

//    #region Climbing
//    private void M_Apply_Climb()
//    {
//        player.M_Apply_Movement(player.g_Forward, c_ClimbingSpeedForward);
//        if (c_IsClimbing)
//        {
//            player.M_Apply_Movement(player.v_Normal, c_ClimbingSpeedUp);
//        }
//        else player.M_Apply_Gravity();
//    }
//    private void M_Check_Climbing(float height, ref bool col, float hitDistance)
//    {
//        RaycastHit hit;
//        Vector3 line = player.v_Direction;

//        if (Physics.Raycast(transform.position + player.v_Normal * height, line, out hit, hitDistance))
//        {
//            col = true;
//        }
//        else col = false;
//    }
//    private void M_Disable_Climbing()
//    {
//        c_IsClimbing = false;
//    }

//    #endregion
//}
