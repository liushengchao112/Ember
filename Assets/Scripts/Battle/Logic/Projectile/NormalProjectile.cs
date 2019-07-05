using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    /// <summary>
    /// Normal Projectile in ember
    /// Will follow target until hit it or time out, can't be avoid
    /// </summary>
    public class NormalProjectile : Projectile
    {
        public override void LogicUpdate( int deltaTime )
        {
            FixVector3 direction = ( targetPosition - position ).normalized;
            speed = direction * speedFactor;

            base.LogicUpdate( deltaTime );
        }

        public override void Hit()
        {
            base.Hit();

            if ( target != null && target.Alive() && target.id == targetId )
            {
                target.Hurt( damage , hurtType, isCritAttack, owner );                
            }
        }
    }
}

