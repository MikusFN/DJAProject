using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Secalhar vou fazer uma interface
public abstract class WeaponCollisionAbs : MonoBehaviour
{

    public GameObject holder;
    protected GameObject opponent;
    protected Animator holderAnimator;
    protected AnimatorStateInfo holderAnimatorState;
    protected float damage = 0, gunMultiplier;

    public virtual void start()
    {
        holderAnimator = holderAnimator.GetComponentInParent<Animator>();
    }

    protected virtual void OnCollisionEnter(Collision col)
    {


        if (col.gameObject != holder && col.gameObject.tag == "inimigo")
        {
            holderAnimatorState = holderAnimator.GetCurrentAnimatorStateInfo(2);
            damage = DamageAtribution(holderAnimatorState);
            col.gameObject.GetComponent<Rigidbody>().isKinematic = true;
           
        }
        else
        if (col.gameObject != holder && col.gameObject.tag == "player")
        {
            holderAnimatorState = holderAnimator.GetCurrentAnimatorStateInfo(2);
            damage = DamageAtribution(holderAnimatorState);

        }
    }

    protected virtual float DamageAtribution(AnimatorStateInfo currentState)
    {
        float insideDamage = 0;
        //Atribuiçao do damage de acordo com o seu estado actual
        //Quando for block retira-se o is kinematic
        //col.gameObject.GetComponent<Rigidbody>().isKinematic = true;   

        return insideDamage;
    }

    //protected virtual void DamageTaken()
    //{
    //             //holder.GetComponent<Collider>().
    //}

    //// Use this for initialization
    //void Start () {
    //}
    //// Update is called once per frame
    //void Update () {
    //}
}
