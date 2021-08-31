using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    //Getting Outside 
    public Transform target;
    NavMeshAgent agent;
    public Animator animator;

    //Stats;
    public float health = 100;
    [Range(0, 100)]
    public float stamina;
    public float gainingStamina = 1f;

    //Movement
    Vector3 initialSpot; //Spawning Spot
    public bool idle = true;
    public bool running;

    //Following
    public float targetDistance;

    //Taunt
    public bool taunted = false;
    public float timer2 = 0;


    //Attack mode bools
    public bool attacking, almostClose, closeEnough, canAttack = true;
    public bool twoHanded;
    public float timer = -1;
    private const float timeToAttack = 1.0f; //wait between attacks
    Quaternion lastRotation;

    //Attacking Animations 
    public int combo = 0;
    public string[] oneHanded_meleeCombos;
    public string oneHanded_meleeDash;

    //Debugging
    public string animName;
    public bool playAnim;
    public float sp;

    void Start()
    {
        //State Machine
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        attacking = animator.GetBool("IsAttacking");

        //Initial Values
        initialSpot = transform.position;
        idle = true;
        stamina = 0;
        agent.speed = 0;
        twoHanded = false;

        //Animaations
        oneHanded_meleeCombos = new string[3];
        oneHanded_meleeCombos[0] = "Enemy Melee 3";
        oneHanded_meleeCombos[1] = "Enemy Melee 2";
        oneHanded_meleeCombos[2] = "Enemy Melee 1";
        oneHanded_meleeDash = "Enemy Dash Melee";
    }

    void Update()
    {
        animator.SetBool("TwoHanded", twoHanded);
        #region DEBUGGING
        sp = agent.speed;
        targetDistance = Vector3.Distance(target.position, transform.position);
        Debug.DrawLine(target.position, transform.position, Color.red);
        #endregion

        #region IDLE TRUE
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
            }
        }
        #endregion

        #region IDLE FALSE
        else if (idle == false)
        {
            animator.SetBool("Taunting", !taunted);
            #region ROTATE ENEMY
            if (attacking == false)
            {
                Vector3 dir = target.position - transform.position;
                dir.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.1f);
            }
            #endregion

            if (taunted == false)
            {
                TauntTimer();
                //taunted = animator.GetBool("Taunting");
            }
            else if (taunted == true)
            #region ALREADY TAUNTED
            {
                //Põe o inimigo a uma pequena distância de segurança do Jogador. Geralmente, o inimigo vai para esta distânica para regenerar stamina e posicionar-se melhor. A esta distância nem todos os ataques acertam.
                #region To SAFE DISTANCE
                if (targetDistance < 4 && targetDistance > 2)
                {
                    almostClose = true;
                }
                else
                {
                    almostClose = false;
                }
                #endregion

                #region SAFE DISTANCE
                if (almostClose == false)
                {
                    if (!attacking)
                    {
                        //Movimento basico do inimigo até chegar ao jogador
                        agent.isStopped = false;
                        Accelarate(3.5f, 0.1f);
                        running = true;
                        agent.destination = target.position;

                    }
                }
                else
                {
                    if (!attacking)
                    {
                        //Caso esteja na distância de segurança mas ainda não pode atacar.
                        if (HasStaminaToAttack() == false)
                        {
                            agent.isStopped = false;
                            //Accelaration(0.8f);
                            int op = Random.Range(0, 1);
                            if (op == 0)
                            {
                                MovingManually("Left", 0.01f);
                            }
                            else
                            if (op == 1)
                            {
                                MovingManually("Right", 0.01f);
                            }

                            running = true;
                            Decelerate(0.1f);
                            //agent.destination = target.position;
                        }
                        else //Caso esteja na distância de segurança mas pode atacar. O que ele vai fazer é correr até à ATTACK DISTANCE
                        {
                            agent.isStopped = false;
                            Accelarate(1.5f, 0.1f);
                            animator.SetFloat("X", 0);
                            animator.SetFloat("Y", 1);
                            running = true;
                            agent.destination = target.position;
                        }

                    }
                }
                #endregion

                //Põe o inimigo quase colado a Jogador. Nesta distância todos os ataques acertam.
                #region To ATTACK DISTANCE
                if (targetDistance < 2f)
                {
                    closeEnough = true;
                }
                else
                {
                    closeEnough = false;
                }
                #endregion

                #region ATTACK DISTANCE && ATTACKING
                if (closeEnough == true)
                {
                    agent.isStopped = true;
                    Decelerate(0.1f);
                    if (attacking == false && canAttack == true && HasStaminaToAttack())
                    {
                        canAttack = false;
                        timer = 0;
                        transform.rotation = lastRotation;
                        //Decelerate(0.1f);
                        animator.SetBool("IsAttacking", true);
                        Attack(30, oneHanded_meleeCombos[combo]);
                        stamina -= 30;
                        if (combo < 2)
                            combo++;
                        else
                            combo = 0;

                    }

                }
                #endregion


                #region TO CLOSE
                if (targetDistance < 1.8f)
                {
                    if (!attacking)
                    {
                        MovingManually("Back", 0.005f);
                    }
                }
                else
                {
                    //animator.SetFloat("X", 0);
                    //animator.SetFloat("Y", 0);
                }
                #endregion

                //Cria uma pausa entre os ataques durante o combo
                #region PAUSE BETWEEN ATTACKS
                if (attacking == false && canAttack == false)
                {
                    //int op = Random.Range(0, 2);
                    //if (op == 0)
                    //{
                    //    MovingManually("Back", 0.005f);
                    //}
                    //else
                    //if (op == 1)
                    //{
                    //    MovingManually("Left", 0.007f);
                    //}
                    //else
                    //if (op == 2)
                    //{
                    //    MovingManually("Right", 0.007f);
                    //}

                    if (timer < timeToAttack)
                        timer += 1f * Time.deltaTime;
                    if (timer > timeToAttack)
                    {
                        timer = 0;
                        canAttack = true;
                    }
                }
                #endregion

                #region GO BACK TO IDLE
                if (targetDistance > 20 || Vector3.Distance(transform.position, initialSpot) > 50)
                {
                    DeAggro();
                }
                #endregion
            }
            #endregion
        }
        #endregion

        lastRotation = transform.rotation;

        //OTHER FUNCTIONS
        if (!attacking)
            StaminaController();

        Animations();

    }

    private void TauntTimer()
    {
        float max = 2;
        if (timer2 < max)
            timer2 += 1f * Time.deltaTime;
        if (timer2 >= max)
        {
            timer2 = 0;
            taunted = true;
        }
    }

    private bool HasStaminaToAttack()
    {
        if ((stamina > 90 && combo == 0) || (stamina > 60 && combo == 1) || (stamina > 30 && combo == 2))
            return true;
        return false;
    }

    private void GetPlayerDirection(float turningVel)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turningVel);
    }

    private void MovingManually(string direction, float value)
    {
        if (direction == "Right")
        {
            Vector3 position = this.transform.position;
            position += transform.right * value;
            this.transform.position = position;
            animator.SetFloat("X", 1);
            animator.SetFloat("Y", 0);
        }

        if (direction == "Left")
        {
            Vector3 position = this.transform.position;
            position += -transform.right * value;
            this.transform.position = position;
            animator.SetFloat("X", -1);
            animator.SetFloat("Y", 0);
        }

        if (direction == "Forward")
        {
            Vector3 position = this.transform.position;
            position += transform.forward * value;
            this.transform.position = position;
            animator.SetFloat("X", 0);
            animator.SetFloat("Y", 1);
        }

        if (direction == "Back")
        {
            Vector3 position = this.transform.position;
            position += -transform.forward * value;
            this.transform.position = position;
            animator.SetFloat("X", 0);
            animator.SetFloat("Y", -1);
        }
    }

    private void DeAggro()
    {
        idle = true;
        Vector3 dir = initialSpot - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.9f);
        agent.SetDestination(initialSpot);
        agent.destination = initialSpot;
    }

    private bool IsPlayerFar()
    {
        if (targetDistance < 15)
            return false;
        return true;
    }

    private void Accelarate(float maximum, float speed)
    {
        if (agent.speed < maximum)
            agent.speed += speed;
        if (agent.speed > maximum)
            agent.speed = maximum;
    }

    private void Decelerate(float speed)
    {
        if (agent.speed > 0)
            agent.speed -= speed;
        if (agent.speed < 0)
            agent.speed = 0;
    }

    private void StaminaController()
    {
        stamina += gainingStamina;

        if (stamina > 100)
            stamina = 100;

        if (stamina < 0)
            stamina = 0;
    }

    private void Attack(int staminaLost, string attackName)
    {
        animator.CrossFade(attackName, 0f);
    }

    private void Animations()
    {
        animator.SetFloat("Speed", agent.speed);
        attacking = animator.GetBool("IsAttacking");
        //animator.SetBool("IsAttacking", attacking);

        if (playAnim)
        {
            int i = Random.Range(0, oneHanded_meleeCombos.Length);
            Debug.Log(i);
            animator.CrossFade(oneHanded_meleeCombos[i], 0.2f);
            playAnim = false;
        }

        if (closeEnough == false && almostClose == false)
        {
            animator.SetBool("IsFighting", false);
        }
        else if (closeEnough == true || almostClose == true)
        {
            animator.SetBool("IsFighting", true);
        }

    }

    private void PlayAnimation(string animation, float transitionTime)
    {
        //if (playAnim)
        {
            animator.SetBool("IsAttacking", true);
            animator.CrossFade(animation, transitionTime);
        }
    }

    private bool CheckIfAnimationActive(string name, int layer)
    {
        if (this.animator.GetCurrentAnimatorStateInfo(layer).IsName(name) &&
               animator.GetCurrentAnimatorStateInfo(layer).normalizedTime >= 1.0f)
        {
            Debug.Log("Animation over");
            return false;
        }
        return true;
    }
}



