using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBasicBehaviour : MonoBehaviour
{
    //Getting Outside 
    public Transform target;
    public NavMeshAgent agent;
    public Animator animator;

    //Stats;
    public float health = 100;
    public float maxStamina = 100;
    [Range(0, 100)]
    public float stamina;
    public float gainingStamina = 1f;

    //Movement
    public Vector3 initialSpot; //Spawning Spot
    public bool idle = true;

    //Following
    public float targetDistance;
    public float targetMaxFar = 15; //Distância para detetar o player

    public virtual bool HasStaminaToAttack()
    {
        return false;
    } //Override

    public virtual void GetPlayerDirection(float turningVel)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turningVel);
    }

    public virtual void MovingManually(string direction, float value) //Override
    {
        
    }

    public virtual void DeAggro()
    {
        idle = true;
        Vector3 dir = initialSpot - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.9f);
        agent.SetDestination(initialSpot);
        agent.destination = initialSpot;
    }

    public virtual bool IsPlayerFar()
    {
        if (targetDistance < targetMaxFar)
            return false;
        return true;
    }

    public virtual void Accelarate(float maximum, float speed)
    {
        if (agent.speed < maximum)
            agent.speed += speed;
        if (agent.speed > maximum)
            agent.speed = maximum;
    }

    public virtual void Decelerate(float speed)
    {
        if (agent.speed > 0)
            agent.speed -= speed;
        if (agent.speed < 0)
            agent.speed = 0;
    }

    public virtual void StaminaController()
    {
        stamina += gainingStamina;

        if (stamina > maxStamina)
            stamina = maxStamina;

        if (stamina < 0)
            stamina = 0;
    }

    public virtual void Attack(int staminaLost, string attackName) //Override
    {
        animator.CrossFade(attackName, 0f);
    }

    public virtual void Animations() //Override
    {
        
    }

    public virtual void PlayAnimation(string animation, float transitionTime) //Override
    {
        animator.SetBool("IsAttacking", true);
        animator.CrossFade(animation, transitionTime);
    }

    public virtual bool CheckIfAnimationActive(string name, int layer)
    {
        if (this.animator.GetCurrentAnimatorStateInfo(layer).IsName(name) &&
               animator.GetCurrentAnimatorStateInfo(layer).normalizedTime >= 1.0f)
        {
            return false;
        }
        return true;
    }
}



