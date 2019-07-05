using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Resource;

namespace Render
{
    public class TrapRender : UnitRender
    {
        private int hitResId;
        private int explodeResID;
        private GameObject effect;

        public override void Reset()
        {
            
        }

        protected override void Awake()
        {
            base.Awake();
            animator = gameObject.GetComponent<Animator>();
        }

        public void Initialized( int metaId )
        {
            TrapProto.Trap proto = DataManager.GetInstance().trapProtoData.Find( p => p.ID == metaId );
            hitResId = proto.HitResID;
            explodeResID = proto.ExplodeResID;
            unitRenderType = UnitRenderType.Trap;
            gameObject.SetActive( true );
        }

        public override void Destroy()
        {
            if ( explodeResID != 0 )
            {
                PlayerEffect( explodeResID );
            }
            gameObject.SetActive( false );
            if ( effect != null )
            {
                effect.SetActive( false );
            }
            base.Destroy();
        }       

        private void PlayerEffect( int resId )
        {
            if ( effect == null )
            {
                effect = GameObject.Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( resId ) );
            }
                        
            effect.transform.position = transform.position;
            effect.transform.localPosition = Vector3.zero;
            effect.transform.localRotation = Quaternion.identity;
            effect.gameObject.SetActive( false );
            effect.gameObject.SetActive( true );
        }
    }
}