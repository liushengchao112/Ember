using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class SiWangXianJingSkill : Skill
    {
        private int timer;

        public override bool DependOnSkillState()
        {
            return false;
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

            timer = 0;
            DebugUtils.Log( DebugUtils.Type.AI_Skill , "SiWangXianJingSkill + fire" );
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            if ( timer >= skillActionHitTime )
            {
                Work();
                timer = 0;
            }

            timer += deltaTime;
        }

        private void Work()
        {
            List<Soldier> allSoldier = new List<Soldier>();

            allSoldier.AddRange( FindOpponentSoldiers( owner.mark , position , ( s ) =>
            {
                return WithinCircleAreaPredicate( position , radius , s.position );
            } ) );

            for ( int i = 0; i < allSoldier.Count; i++ )
            {
                if ( allSoldier[i] != null && allSoldier[i].Alive() )
                {
                    Projectile projectile = GenerateProjectile( owner , carrierId , owner.position , allSoldier[i] );
                    projectile.RegisterRenderMessageHandler( PostRenderMessage );
                    projectile.RegisterDestroyHandler( PostDestroy );
                    projectile.RegisterRandomMethod( GetRandomNumber );
                    projectile.RegisterFindOpponentSoldiers( FindOpponentSoldiers );
                    projectile.RegisterFindOpponentCrystalCars( FindOpponentCrystalCars );
                    projectile.RegisterFindOpponentDemolishers( FindOpponentDemolishers );
                    projectile.RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate );
                    projectile.RegisterWithinFrontRectangleAreaPredicate( WithinFrontRectAreaPredicate );

                    RenderMessage rm = new RenderMessage();
                    rm.type = RenderMessage.Type.SpawnProjectile;
                    rm.ownerId = projectile.id;
                    rm.position = projectile.position.vector3;
                    rm.direction = projectile.speed.vector3;
                    rm.arguments.Add( "mark" , projectile.mark );
                    rm.arguments.Add( "metaId" , projectile.metaId );
                    rm.arguments.Add( "holderType" , -1 );
                    rm.arguments.Add( "holderId" , projectile.owner.id );
                    PostRenderMessage( rm );
                }
            }
            ReleaseEnd();
        }
    }
}