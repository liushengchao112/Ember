using UnityEngine;
using System.Collections;

using Utils;
using Constants;
using Data;
using System;

namespace Render
{
    public class DemolisherRender : UnitRender
    {
        public Vector3 speed;
        public Vector3 direction;

        protected override void Awake()
        {
            unitRenderType = UnitRenderType.Demolisher;
            animator = gameObject.GetComponent<Animator>();

            base.Awake();
        }

        // Use this for initialization
        void Start()
        {
            currentAnimator = "Idle";
            MessageDispatcher.AddObserver( BattleEnd, MessageType.BattleEnd );
        }

        void OnDestroy()
        {
            MessageDispatcher.RemoveObserver( BattleEnd, MessageType.BattleEnd );
        }

        public void Idle( Vector3 direction )
        {
            if ( direction != Vector3.zero )
            {
                transform.rotation = Quaternion.LookRotation( direction );
            }

            PlayAnimation( "Idle" );
        }

        public void Walk()
        {
            PlayAnimation( "Walk" );
        }

        public void Attack( Vector3 direction )
        {
            if ( direction != Vector3.zero )
            {
                transform.rotation = Quaternion.LookRotation( direction );
            }

            PlayAnimation( "Attack" );
        }

        public void Dying( Vector3 direction )
        {
            SetRotation( direction );
            PlayAnimation( "Death" );
        }

        public override void Destroy()
        {
            gameObject.SetActive( false );
            base.Destroy();
        }

        public override void Reset()
        {
            id = -1;
            transform.rotation = Quaternion.identity;
            transform.position = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
            speed = Vector3.zero;
        }

        public void BattleEnd( object type )
        {
            Idle( speed );
        }
    }
}