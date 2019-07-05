using UnityEngine;
using System.Collections;
using System;

using Data;

namespace Render
{
    public class PowerUpRender : UnitRender
    {
        public PowerUpType PowerUpType;
        //public MatchSide side;
        //public ForceMark mark;

        //public long id;
        public GameObject IdleEffect;
        public GameObject PickupedEffect;
        //private Animator animator;

        protected override void Awake()
        {
            animator = gameObject.GetComponent<Animator>();
        }

        public override void Reset()
        {
            gameObject.SetActive( false );
            gameObject.SetActive( true );
            PickupedEffect.SetActive( false );
            IdleEffect.SetActive( true );
        }

        public override void Destroy()
        {
            base.Destroy();
            animator.SetTrigger("Destroy");
            PickupedEffect.SetActive( true );
            IdleEffect.SetActive( false );
        }

        public void PickedUp()
        {
            Destroy();
        }

    }
}
