using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class MoFaBaoLieDanArrow : Projectile
    {
        private long radius = 5000; // mm 

        public override void Hit()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Projectile , "MoFaBaoLieDanArrow + hit" );
            base.Hit();
            AttributEffectAttach();
            RadiusHurt();
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            FixVector3 direction = ( targetPosition - position ).normalized;
            speed = direction * speedFactor;
        }

        private void AttributEffectAttach()
        {
            for ( int i = 0; i < attributEffects.Count; i++ )
            {
                AttributeEffect ae = GenerateAttributeEffect( attributEffects[i] );
                ae.Attach( (Soldier)owner , (Soldier)target );
            }
            DebugUtils.Log( DebugUtils.Type.AI_Projectile , "MoFaBaoLieDanArrow + AttributEffectAttach + " + attributEffects.Count );
        }

        private void RadiusHurt()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Projectile , "MoFaBaoLieDanArrow + RadiusHurt + start" );

            List<Soldier> allSoldier = new List<Soldier>();

            allSoldier.AddRange( FindOpponentSoldiers( owner.mark , position , ( s ) =>
            {
                return WithinCircleAreaPredicate( position , radius , s.position );
            } ) );

            for ( int i = 0; i < allSoldier.Count; i++ )
            {
                allSoldier[i].Hurt( damage , hurtType , false, owner );
            }

            DebugUtils.Log( DebugUtils.Type.AI_Projectile , "MoFaBaoLieDanArrow + hitallSoldier + " + allSoldier.Count );

            //List<LogicUnit> allUnit = new List<LogicUnit>();

            //allUnit.AddRange( FindNeutralUnits( position , ( s ) =>
            //{
            //    return WithinCircleAreaPredicate( position , radius , s.position );
            //} ) );

            //for ( int i = 0; i < allUnit.Count; i++ )
            //{
            //    allUnit[i].Hurt( damage , hurtType , owner );
            //}

            //DebugUtils.Log( DebugUtils.Type.AI_Projectile , "MoFaBaoLieDanArrow + hitallUnit + " + allUnit.Count );
        }

        public void SetRadius( long radius )
        {
            this.radius = radius;
        }
    }
}
