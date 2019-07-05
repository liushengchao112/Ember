using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Utils;

namespace Logic
{
    public class BaiRenZhanSkill : Skill
    {
        private int hitTimer = 0;
        private int attackStateDurationTimer = 0;

        public override void Fire()
        {
            base.Fire();

            RenderMessage rm = new RenderMessage();
            rm.ownerId = owner.id;
            rm.type = RenderMessage.Type.SoldierReleaseSkill;
            rm.arguments.Add( "index", index );
            rm.arguments.Add( "metaId", metaId );

            PostRenderMessage( rm );
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            attackStateDurationTimer += deltaTime;
            if ( attackStateDurationTimer <= skillActionDuration )
            {
                if ( hitTimer > skillActionHitTime )
                {
                    // Find Opponent
                    List<Soldier> soldiers = FindOpponentSoldiers( mark, owner.position, ( s ) =>
                    {
                        return WithinSectorAreaPredicate( owner.position, owner.direction, radius, s.position );
                    } );

                    List<LogicUnit> npcs = FindNeutralUnits( owner.position, ( npc ) =>
                    {
                        return WithinSectorAreaPredicate( owner.position, owner.direction, radius, npc.position );
                    } );

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

                    hitTimer = 0;
                }
                else
                {
                    // back swing...
                }

                hitTimer += deltaTime;
            }
            else
            {
                // skill completely finished 
                ReleaseEnd();
            }
        }

        public override bool DependOnSkillState()
        {
            return true;
        }
    }
}
