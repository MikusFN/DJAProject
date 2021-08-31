using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : WeaponCollisionAbs {

    public Transform cameraGO;
    public float currentdamage=0;

        public override void start()
    {
        base.start();
        currentdamage = damage;
    }

    protected override void OnCollisionEnter(Collision col)
    {
        base.OnCollisionEnter(col);
        opponent = cameraGO.gameObject.GetComponentInParent<CameraScript>().locked_lookAt.gameObject;
        holderAnimator = GetComponentInParent<Animator>();

        if (col.gameObject != holder && col.gameObject.tag == "inimigo")
        {
            holderAnimatorState = holderAnimator.GetCurrentAnimatorStateInfo(2);
            currentdamage = DamageAtribution(holderAnimatorState);

        }
       
        //Teste
        Debug.Log("Hitted "+opponent.name+" with: "+ currentdamage );
        

    }
    protected override float DamageAtribution(AnimatorStateInfo currentState)
    {

        currentdamage = 0;
        if (currentState.IsName("Stab"))
            currentdamage = 5;
        if (currentState.IsName("Slash"))
            currentdamage = 3;
        if (currentState.IsName("rotationAttack"))
            currentdamage = 10;
        if (currentState.IsName("SwordHighAttack"))
            currentdamage = 7;

        //return base.DamageAtribution(currentState);
        return currentdamage;
    }
}
