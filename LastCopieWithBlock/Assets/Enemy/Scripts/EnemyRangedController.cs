using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyRangedController : EnemyBasicBehaviour
{

    public float speedValue = 0;

    //Attack mode bool
    public bool gettingUp = false;
    public bool ArrowEquiped = false;
    public float shootingRangeMin = 15f, shootingRangeMax = 35f;
    public float timer = -1;
    private const float timeToAttack = 1.0f; //wait between attacks
    public bool attacking;
    Quaternion lastRotation;

    //Keeping Range
    public float minRange = 15;
    public float maxRange = 35;

    //Attacking Animations 


    void Start()
    {
        //State Machine
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        //Initial Values
        initialSpot = transform.position;
        idle = true;
        stamina = 0;
        agent.speed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        targetDistance = Vector3.Distance(target.position, transform.position);
        if (idle == true)
        {
            //Running to initial spot
            if (Vector3.Distance(transform.position, initialSpot) < 4)
                Decelerate(0.1f);
            if (Vector3.Distance(transform.position, initialSpot) < 1)
                agent.speed = 0;

            if (IsPlayerFar() == false)
            {
                idle = false;
                animator.SetBool("GettingUp", true);
            }
        }
        else
        {
            gettingUp = animator.GetBool("GettingUp");

            #region ROTATE ENEMY
            if (attacking == false)
            {
                Vector3 dir = target.position - transform.position;
                dir.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.1f);
            }
            #endregion

            if (gettingUp == false)
            {
                if (targetDistance > maxRange)
                {
                    Accelarate(1, 0.1f);
                    MovingManually("Forward", 0.05f);
                }

                if (targetDistance < minRange)
                {
                    Accelarate(1, 0.1f);
                    MovingManually("Back", 0.05f);
                }
                
                if (targetDistance < maxRange && targetDistance > minRange)
                {
                    Decelerate(0.05f);
                    StopRunning();
                }
            }

            if (stamina > 50)
            {
                ArrowEquiped = true;
            }

            if (stamina > 90)
            {
                ArrowEquiped = false;
                ShootArrow();
                stamina -= 50;
            }

            StaminaController();

            
        }
    }

    public void ShootArrow()
    {

    }

    public void StopRunning()
    {
        Vector3 position = this.transform.position;
        position += Vector3.zero;
        this.transform.position = position;
        animator.SetFloat("X", speedValue);
        animator.SetFloat("Y", speedValue);
    }

    public override void Accelarate(float maximum, float speed)
    {
        if (speedValue < maximum)
            speedValue += speed;
        if (speedValue > maximum)
            speedValue = maximum;
    }

    public override void Decelerate(float speed)
    {
        if (speedValue > 0)
            speedValue -= speed;
        if (speedValue < 0)
            speedValue = 0;
    }

    public override void MovingManually(string direction, float value)
    {
        if (direction == "Right")
        {
            Vector3 position = this.transform.position;
            position += transform.right * value;
            this.transform.position = position;
            animator.SetFloat("X", speedValue);
            animator.SetFloat("Y", 0);
        }

        if (direction == "Left")
        {
            Vector3 position = this.transform.position;
            position += -transform.right * value;
            this.transform.position = position;
            animator.SetFloat("X", -speedValue);
            animator.SetFloat("Y", 0);
        }

        if (direction == "Forward")
        {
            Vector3 position = this.transform.position;
            position += transform.forward * value;
            this.transform.position = position;
            animator.SetFloat("X", 0);
            animator.SetFloat("Y", speedValue);
        }

        if (direction == "Back")
        {
            Vector3 position = this.transform.position;
            position += -transform.forward * value;
            this.transform.position = position;
            animator.SetFloat("X", 0);
            animator.SetFloat("Y", -speedValue);
        }


    }
}
