using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Utils;

namespace Logic
{
    public class YueXingDaJiSkill : Skill
    {
        private enum SkillState
        {
            Fire,
            FrontSwing,
            HitPoint,
            BackSwing,
        }

        private SkillState state;
        private int hitTimer;

        public override void Fire()
        {
            base.Fire();

            RenderMessage rm = new RenderMessage();
            rm.ownerId = owner.id;
            rm.type = RenderMessage.Type.SoldierReleaseSkill;
            rm.arguments.Add( "index", index );
            rm.arguments.Add( "metaId", metaId );
            PostRenderMessage( rm );

            state = SkillState.Fire;
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            if ( state == SkillState.Fire )
            {
                state = SkillState.FrontSwing;
            }
            else if ( state == SkillState.FrontSwing )
            {
                state = SkillState.FrontSwing;

                if ( hitTimer >= skillActionHitTime )
                {
                    state = SkillState.HitPoint;
                }
            }
            else if ( state == SkillState.HitPoint )
            {
                // Find Opponents
                List<Soldier> soldiers = FindOpponentSoldiers( mark, owner.position, ( s ) =>
                {
                    return WithinSectorAreaPredicate( owner.position, owner.direction, radius, s.position );
                } );

                List<LogicUnit> npcs = FindNeutralUnits( owner.position, ( npc ) =>
                {
                    return WithinSectorAreaPredicate( owner.position, owner.direction, radius, npc.position );
                } );

                // Affect Opponents
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

                state = SkillState.BackSwing;
            }
            else if ( state == SkillState.BackSwing )
            {
                if ( hitTimer >= skillActionDuration )
                {
                    ReleaseEnd();
                }
            }

            hitTimer += deltaTime;
        }

        public override bool DependOnSkillState()
        {
            return true;
        }
    }
}
