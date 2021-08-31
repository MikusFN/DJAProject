using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platforming_Slide : MonoBehaviour {

    private PlayerControls player;
    private Animator animator;

    private const float s_Speed = 0.1f;


    // Use this for initialization
    void Start () {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        player = GetComponent<PlayerControls>();
        animator = GetComponent<Animator>();
        animator.SetLayerWeight(1, 1);
    }
    void FixedUpdate()
    {
        if (animator.GetBool("Plat_Slide"))
        {
            player.M_Apply_Movement(player.lastDirection, s_Speed);
        }
    }
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetAxis("Slide") != 0
            && !animator.GetBool("Plat_Slide")
            && animator.GetBool("Move_Sprinting")
            && !animator.GetBool("Move_MidAir"))
        {
            animator.Play("Slide");
            animator.SetBool("Plat_Slide", true);
            player.lastDirection = player.moveDirection;
        }
        player.M_Check_AnimationEnd("Slide", "Plat_Slide", 1);
        //BUGGED STILL DONT KNOW WHY
        //if (animator.GetBool("Plat_Slide"))
        //{
        //    if (player.M_Check_AnimationEnd("Slide", "Plat_Slide", 1))
        //    {
        //        player.M_Set_PlayerHitBox(0.2f, new Vector3(0, 0.2f, 0), 0.1f);
        //        player.M_Apply_Gravity();
        //    }
        //    else
        //    {
        //        player.M_Reset_PlayerHitBox();
        //    }
        //}
    }


}