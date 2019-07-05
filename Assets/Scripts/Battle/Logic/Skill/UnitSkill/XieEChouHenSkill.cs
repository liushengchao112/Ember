using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class XieEChouHenSkill : Skill
    {
        private int spawnProjectileTimer = 0;

        public override void Fire()
        {
            base.Fire();

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SoldierReleaseSkill;
            rm.ownerId = owner.id;
            rm.direction = owner.direction.vector3;
            rm.arguments.Add( "index", index );
            rm.arguments.Add( "metaId", metaId );
            owner.PostRenderMessage( rm );

            spawnProjectileTimer = 0;
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

        private void SpawnProjectile()
        {
            if ( owner.target != null && owner.target.Alive() && owner.targetId == owner.target.id )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier , "the " + owner.type + " soldier " + id + " skill to fire to target " + owner.target.id );
                Projectile projectile = GenerateProjectile( owner , carrierId , owner.position , owner.target );
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
                rm.arguments.Add( "holderType" , (int)projectile.owner.type );
                rm.arguments.Add( "holderId" , projectile.owner.id );
                PostRenderMessage( rm );
            }
            ReleaseEnd();
        }

        public override bool DependOnSkillState()
        {
            return true;
        }
    }
}
