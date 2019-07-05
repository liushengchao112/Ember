using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Render
{
    public class NpcRender : UnitRender
    {
        public int modelId;
        public Vector3 speed;
        public Vector3 direction;

        protected override void Awake()
        {
            unitRenderType = UnitRenderType.NPC;
            base.Awake();

            animator = GetComponent<Animator>();
        }

        public void Idle( Vector3 rotation )
        {
            SetRotation( rotation );
            PlayAnimation( "Idle", 1 );
        }

        public void Walk( Vector3 rotation )
        {
            SetRotation( rotation );
            PlayAnimation( "Run", 1 );
        }

        public void Attack( Vector3 rotation, float attackInterval )
        {
            SetRotation( rotation );
            PlayAnimation( "Attack", attackInterval );
        }

        public void AttackHit( )
        {
           
        }

        public void Dying()
        {
            PlayAnimation( "Dying", 1 );
        }

        public void Reborn()
        {
            hp = maxHp;
            gameObject.SetActive( true );
            PlayAnimation( "Idle" );
        }

        // Trigger by animation event.
        public override void Destroy()
        {
            gameObject.SetActive( false );
            base.Destroy();
        }

        public override void Reset()
        {

        }
    }
}
