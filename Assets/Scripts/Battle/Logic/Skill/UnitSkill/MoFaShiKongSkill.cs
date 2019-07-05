using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Utils;

using SkillProto = Data.UnitSkillsProto.UnitSkill;

namespace Logic
{
    public class MoFaShiKongSkill : Skill
    {
        private int hitTimer = 0;

        public override void Fire()
        {
            base.Fire();

            hitTimer = 0;

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnSkill;
            rm.ownerId = owner.id;
            rm.position = owner.position.vector3;
            rm.direction = owner.direction.vector3;
            rm.arguments.Add( "index", index );
            rm.arguments.Add( "metaId", metaId );
            rm.arguments.Add( "mark", owner.mark );
            PostRenderMessage( rm );
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            if ( hitTimer >= skillActionHitTime )
            {
                Hit();
                hitTimer = 0;
            }

            hitTimer += deltaTime;
        }

        public void Hit()
        {
            // Find Opponent
            List<Soldier> soldiers = FindOpponentSoldiers( mark, position, ( s ) =>
            {
                return WithinCircleAreaPredicate( position, radius, s.position );
            } );

            List<LogicUnit> npcs = FindNeutralUnits( position, ( s ) =>
            {
                return WithinCircleAreaPredicate( position, radius, s.position );
            } );

            // Affect Opponent
            for ( int j = 0; j < attributeEffects.Count; j++ )
            {
                for ( int i = 0; i < soldiers.Count; i++ )
                {
                    AttributeEffect ae = GenerateAttributeEffect( attributeEffects[j] );
                    ae.Attach( owner, soldiers[i] );
                }

                for ( int i = 0; i < npcs.Count; i++ )
                {
                    AttributeEffect ae = GenerateAttributeEffect( attributeEffects[j] );
                    ae.Attach( owner, npcs[i] );
                }
            }

            ReleaseEnd();

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SkillHit;
            rm.ownerId = id;
            PostRenderMessage( rm );
        }

        public override bool DependOnSkillState()
        {
            return false;
        }
    }
}
