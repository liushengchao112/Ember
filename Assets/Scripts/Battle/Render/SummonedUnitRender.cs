using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Resource;
using Data;

using SummonProto = Data.SummonProto.Summon;

namespace Render
{
    public class SummonedUnitRender : UnitRender
    {
        private static readonly string TRIGGER_IDLE = "Idle";
        private static readonly string TRIGGER_ATTACK = "Attack";
        private static readonly string TRIGGER_DYING = "Dying";

        private static readonly string FOOT_BINDPOINT = "FootEffectPoint";

        private Dictionary<int, GameObject> effects;

        // components
        private SkinnedMeshRenderer render;

        private int attackEffectResId;
        private int deathEffectResId;
        private string attackEffectBindpoint;
        private string deathEffectBindpoint;

        protected override void Awake()
        {
            unitRenderType = UnitRenderType.Summon;

            base.Awake();

            animator = gameObject.GetComponent<Animator>();

            effects = new Dictionary<int, GameObject>();
        }

        public void Initialized( int metaId )
        {
            SummonProto proto = DataManager.GetInstance().summonProtoData.Find( p => p.ID == metaId );
            DebugUtils.Assert( proto != null, string.Format( "Can't find metaId = {0} in summon table", metaId ) );

            attackEffectResId = proto.Attack_res;
            attackEffectBindpoint = proto.Attack_res_bindpoint;
            deathEffectResId = proto.Death_res_id;
            deathEffectBindpoint = proto.Death_res_bindpoint;

            gameObject.SetActive( true );
        }

        public void Idle()
        {
            PlayAnimation( TRIGGER_IDLE );
        }

        public void Attack( Vector3 direction )
        {
            SetRotation( direction );
            PlayAnimation( TRIGGER_ATTACK );
            PlayEffect( attackEffectResId, attackEffectBindpoint );
        }

        public void Dying()
        {
            PlayAnimation( TRIGGER_DYING );
            PlayEffect( deathEffectResId, deathEffectBindpoint );
        }

        public override void Destroy()
        {
            gameObject.SetActive( false );

            foreach ( KeyValuePair<int, GameObject> effect in effects )
            {
                effect.Value.SetActive( false );
            }

            base.Destroy();
        }

        public override void Reset()
        {
            
        }

        public void PlayEffect( int effectResId, string bindpoint )
        {
            if ( effectResId <= 0 || string.IsNullOrEmpty( bindpoint ) )
            {
                DebugUtils.LogWarning( DebugUtils.Type.AI_Skill, "Summon Render : " + metaId + " play a effect failed！ effectResId:" + effectResId + " bindPoint:" + bindpoint );
                return;
            }

            GameObject effect;
            if ( effects.ContainsKey( effectResId ) )
            {
                effect = effects[effectResId];
            }
            else
            {
                effect = GameObject.Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( effectResId ) );
                effects.Add( effectResId, effect );
            }

            effect.transform.parent = GetBindPoint( bindpoint );
            effect.transform.localPosition = Vector3.zero;
            effect.transform.localRotation = Quaternion.identity;
            effect.gameObject.SetActive( false );
            effect.gameObject.SetActive( true );
        }
    }
}
