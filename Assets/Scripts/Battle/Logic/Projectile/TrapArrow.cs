using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Constants;

namespace Logic
{
    public class TrapArrow : Projectile
    {
        private int trapMetaId;

        public override void Hit()
        {
            GenerateOneTrap();
            base.Hit();                        
        }

        public void SetTrapId( int trapMetaId )
        {
            this.trapMetaId = trapMetaId;
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );
        }

        protected override bool IsHit()
        {
            int distance = FixVector3.SqrDistance( position , destination );
            return distance <= GameConstants.HIT_DISTANCE;
        }

        private void GenerateOneTrap()
        {
            //--need register TrapGenerator
            Trap trap = GenerateTrap( owner , trapMetaId , destination , FixVector3.forward );
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
            rm.arguments.Add( "metaId" , trap.metaId );
            rm.arguments.Add( "holderType" , (int)trap.owner.type );
            rm.arguments.Add( "holderId" , trap.owner.id );
            rm.arguments.Add( "model" , trap.modelId );
            PostRenderMessage( rm );
            //--TODO
        }
    }
}
