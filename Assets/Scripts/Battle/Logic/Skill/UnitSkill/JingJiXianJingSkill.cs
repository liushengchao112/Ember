using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class JingJiXianJingSkill : Skill
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

            timer = 0;
            DebugUtils.Log( DebugUtils.Type.AI_Skill , "MoShouXianJingSkill + fire" );
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
            if ( owner.target != null && owner.target.Alive() && owner.target.id == owner.targetId )
            {
                trapMetaId = Constants.GameConstants.JINGJIXIANJINGID;
                TrapArrow projectile = (TrapArrow)GenerateProjectile( owner , carrierId , owner.position , owner.target );
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
                projectile.ReisterTrapGenerator( GenerateTrap );
                projectile.SetTrapId( trapMetaId );

                for ( int i = 0; i < attributeEffects.Count; i++ )
                {
                    projectile.FillingAttributEffect( attributeEffects[i] );
                }

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
                DebugUtils.Log( DebugUtils.Type.AI_Projectile , "SpawnProjectile + TrapArrow" );
            }
            ReleaseEnd();            
        }
    }
}