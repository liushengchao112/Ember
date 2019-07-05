using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class MoFaBaoLieDanSkill : Skill
    {
        private int spawnProjectileTimer = 0;

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

            spawnProjectileTimer = 0;
            DebugUtils.Log( DebugUtils.Type.AI_Skill , "MoFaBaoLieDanSkill + fire" );
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            if ( spawnProjectileTimer > skillActionHitTime )
            {
                SpawnProjectile();
                spawnProjectileTimer = 0;
            }

            spawnProjectileTimer += deltaTime;
        }

        public override bool DependOnSkillState()
        {
            return true;
        }

        private void SpawnProjectile()
        {
            MoFaBaoLieDanArrow projectile = (MoFaBaoLieDanArrow)GenerateProjectile( owner , carrierId , owner.position , owner.target );
            projectile.RegisterRenderMessageHandler( PostRenderMessage );
            projectile.RegisterDestroyHandler( PostDestroy );
            projectile.RegisterRandomMethod( GetRandomNumber );
            projectile.RegisterFindOpponentSoldiers( FindOpponentSoldiers );
            projectile.RegisterFindOpponentCrystalCars( FindOpponentCrystalCars );
            projectile.RegisterFindOpponentDemolishers( FindOpponentDemolishers );
            projectile.RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate );
            projectile.RegisterWithinFrontRectangleAreaPredicate( WithinFrontRectAreaPredicate );
            projectile.RegisterGenerateAttributeEffect( GenerateAttributeEffect );
            projectile.RegisterFindNeutralUnits( FindNeutralUnits );
            projectile.SetRadius( radius );


            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnProjectile;
            rm.ownerId = projectile.id;
            rm.position = projectile.position.vector3;
            rm.direction = projectile.speed.vector3;
            rm.arguments.Add( "mark" , projectile.mark );
            rm.arguments.Add( "metaId" , projectile.metaId );
            rm.arguments.Add( "holderType" , (int)projectile.owner.type );
            rm.arguments.Add( "holderId" , projectile.owner.id );
            PostRenderMessage( rm );

            ReleaseEnd();
            DebugUtils.Log( DebugUtils.Type.AI_Projectile , "SpawnProjectile+MoFaBaoLieDanArrow" );
        }
    }
}

;