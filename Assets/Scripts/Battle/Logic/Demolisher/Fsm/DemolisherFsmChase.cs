using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Constants;
using Utils;

namespace Logic
{
    public class DemolisherFsmChase : Fsm
    {
        private Demolisher owner;

        public DemolisherFsmChase( Demolisher car)
        {
            owner = car;
        }

        // Update is called once per frame
        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            if ( owner.target != null && owner.target.Alive() )
            {
                owner.FindOpponent();
                owner.WaypointHandler();
                owner.pathAgent.Move( owner.speed * deltaTime );

                // After move, Check the crystal's position is in the attack area.
                long distance = FixVector3.SqrDistance( owner.target.position, owner.position );
                long attackDistance = (long)owner.attackArea + (long)owner.target.modelRadius + (long)owner.modelRadius;

                if ( distance < attackDistance )
                {
                    owner.Fight( owner.target );
                }
            }
            else
            {
                owner.ChangeState( DemolisherState.IDLE, owner.fsmIdle );
            }
        }
    }
}
