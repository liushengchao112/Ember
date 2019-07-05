using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class WildMonsterFsmChase : Fsm
    {
        public Npc owner;

        public WildMonsterFsmChase() { }

        public WildMonsterFsmChase( Npc n )
        {
            owner = n;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            if ( owner.target != null && owner.target.Alive() )
            {
                long distance = FixVector3.SqrDistance( owner.brithPosition, owner.position );

                if ( distance < owner.maxChaseDistance  )
                {
                    owner.WaypointHandler();
                    owner.pathAgent.Move( owner.speed * deltaTime );

                    // After move, Check the crystal's position is in the attack area.
                    FixVector3 v = owner.target.position;
                    distance = FixVector3.SqrDistance( v , owner.position );
                    long attackDistance = owner.GetAttackAera();

                    if ( distance < attackDistance )
                    {
                        owner.Attack( owner.target );
                    }
                }
                else
                {
                    owner.Walk( owner.brithPosition );
                }
            }
            else
            {
                owner.Walk( owner.brithPosition );
            }
        }
    }
}

