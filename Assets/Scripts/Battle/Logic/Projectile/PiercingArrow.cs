using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Utils;

namespace Logic
{
    /// <summary>
    /// Piecring arrow will attack every unit once in flying state
    /// until the flying distance is over the maximum distance
    /// </summary>
    public class PiercingArrow : Projectile
    {
        private long attackRadius = 0;
        private List<long> suffererIdList = new List<long>();

        public void SetAttackRadius( long r )
        {
            attackRadius = r;
        }

        protected override void FlyingOperation( int deltaTime )
        {
            List<Soldier> soldiers = FindOpponentSoldiers( mark, position, (s) =>
            {
                return WithinCircleAreaPredicate( position, attackRadius, s.position );
            } );

            for ( int i = 0; i < soldiers.Count; i++ )
            {
                if ( !suffererIdList.Exists( p => p == soldiers[i].id ) )
                {
                    soldiers[i].Hurt( damage, hurtType, false, owner );
                    suffererIdList.Add( soldiers[i].id );
                }
            }

            if ( IsDistanceOut() )
            {
                flyingDistance = 0;
                DistanceOut();
            }
        }

        public override void Reset()
        {
            suffererIdList.Clear();
            base.Reset();
        }
    }
}
