using UnityEngine;
using System.Collections.Generic;
using System;

using Utils;
using Constants;
using Resource;
using Data;

using ProjectileProto = Data.ProjectileProto.Projectile;

namespace Render
{
    public class ProjectileRender : UnitRender
    {
        public Vector3 speed;
        public Vector3 direction;

        public GameObject hitEffect;

        private bool beginTimingRecycle;
        private float recycleTimer;
        private float recycleTime = 1;

        // Use this for initialization
        void Start()
        {
            MessageDispatcher.AddObserver( BattleEnd, MessageType.BattleEnd );
        }

        void OnDestroy()
        {
            MessageDispatcher.RemoveObserver( BattleEnd, MessageType.BattleEnd );
        }

        public void Initialize( long id, Vector3 position, Vector3 direction, ProjectileProto proto )
        {
            this.id = id;
            this.gameObject.name = id.ToString();
            this.transform.position = position;
            this.transform.rotation = Quaternion.LookRotation( direction );
            this.gameObject.SetActive( true );

            if ( proto.HitEffect_ResourceId != 0 && hitEffect == null )
            {
                GameObject go = GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( proto.HitEffect_ResourceId );
                hitEffect = GameObject.Instantiate( go );
                hitEffect.SetActive( false );
            }

            SetChildActive( true );
        }

        public void Hit()
        {
            if ( hitEffect != null )
            {
                hitEffect.transform.position = transform.position;

                hitEffect.gameObject.SetActive( false );
                hitEffect.gameObject.SetActive( true );
            }

            SetChildActive( false );
            beginTimingRecycle = true;
        }

        public void TimeOut()
        {
            gameObject.SetActive( false );
        }

        public override void Reset()
        {
            id = -1;
            transform.position = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
            speed = Vector3.zero;
        }

        public void BattleEnd( object type )
        {
            TimeOut();
        }

        private void SetChildActive( bool active )
        {
            for ( int i = 0; i < transform.childCount; i++ )
            {
                transform.GetChild( i ).gameObject.SetActive( active );
            }
        }

        protected override void Update()
        {
            base.Update();

            if ( beginTimingRecycle )
            {
                recycleTimer += Time.deltaTime;
                if ( recycleTimer >= recycleTime )
                {
                    Destroy();
                    beginTimingRecycle = false;
                    recycleTimer = 0;
                }
            }
        }
    }
}
