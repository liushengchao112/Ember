using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class ShengSiYiZhanSkill : Skill
    {
        private int workCount = 0;
        private const int TimeInterval = 10;
        private List<AttributeEffect> tempEffect = new List<AttributeEffect>();

        public override void Fire()
        {
            base.Fire();

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnSkill;

            rm.ownerId = owner.id;
            rm.position = owner.position.vector3;
            rm.direction = owner.direction.vector3;
            rm.arguments.Add( "index" , index );
            rm.arguments.Add( "metaId" , metaId );
            rm.arguments.Add( "mark" , owner.mark );
            PostRenderMessage( rm );

            AttributEffectAttach();
        }

        private void AttributEffectAttach()
        {
            tempEffect.Clear();
            for ( int i = 0; i < attributeEffects.Count; i++ )
            {
                AttributeEffect ae = GenerateAttributeEffect( attributeEffects[i] );
                tempEffect.Add( ae );
                ae.Attach( (Soldier)owner , (Soldier)owner );
            }
            DebugUtils.Log( DebugUtils.Type.AI_Projectile , "MoFaBaoLieDanArrow + AttributEffectAttach + " + attributeEffects.Count );
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            if ( Time.frameCount % TimeInterval == 0 )
            {
                SkillWork();
            }            
        }

        private void SkillWork()
        {
            if ( tempEffect.Count <= 0 )
            {
                return;
            }

            int enemyCount = FindOpponentSoldiers( mark , owner.position , ( s ) =>
            {
                return WithinCircleAreaPredicate( owner.position , radius , s.position );
            } ).Count;
          
            if ( workCount == enemyCount && enemyCount > 0 )
            {
                return;
            }

            workCount = enemyCount;

            for ( int i = 0; i < tempEffect.Count; i++ )
            {
                tempEffect[i].SetMainValue( tempEffect[i].MainValue * workCount );
            }
        }

        public override bool DependOnSkillState()
        {
            return false;
        }

        public override void ReleaseEnd()
        {
            base.ReleaseEnd();

            tempEffect.Clear();

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SkillHit;
            rm.ownerId = owner.id;
            PostRenderMessage( rm );
        }

        public override void Reset()
        {
            base.Reset();
            tempEffect.Clear();
        }
    }
}