using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class MoFaXianJingSkill : Skill
    {
        private int timer;

        public override bool DependOnSkillState()
        {
            return true;
        }

        public override void Fire()
        {
            base.Fire();

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SoldierReleaseSkill;
            rm.ownerId = owner.id;
            rm.direction = owner.direction.vector3;
            rm.arguments.Add( "index" , index );
            rm.arguments.Add( "metaId" , metaId );
            owner.PostRenderMessage( rm );

            DebugUtils.Log( DebugUtils.Type.AI_Skill , "MoFaXianJingSkill + fire" );
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            //if ( timer > skillActionHitTime )
            {
                Work();
                timer = 0;
            }

            timer += deltaTime;
        }

        private void Work()
        {
            if ( carrierType != 2 )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Skill, "MoFaXianJingSkill carrierType != 2" );
                return;
            }
            Trap trap = GenerateTrap( owner , carrierId , owner.position , FixVector3.forward );
            trap.RegisterRenderMessageHandler( PostRenderMessage );
            trap.RegisterDestroyHandler( PostDestroy );
            trap.RegisterBattleAgentMessageHandler( PostBattleMessage );
            trap.RegisterFindOpponentSoldiers( FindOpponentSoldiers );
            trap.RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate );
            trap.RegisterGenerateAttributeEffect( GenerateAttributeEffect );

            RenderMessage rm = new RenderMessage();

            rm.type = RenderMessage.Type.SpawnTrap;
            rm.ownerId = trap.id;
            rm.position = trap.position.vector3;
            rm.direction = trap.rotation.vector3;
            rm.arguments.Add( "mark" , trap.mark );
            rm.arguments.Add( "metaId" , carrierId );
            rm.arguments.Add( "holderType" , (int)trap.owner.type );
            rm.arguments.Add( "holderId" , trap.owner.id );
            rm.arguments.Add( "model" , trap.modelId );

            PostRenderMessage( rm );

            ReleaseEnd();
        }
    }
}