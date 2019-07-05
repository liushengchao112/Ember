using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class DemolisherFsmFight : Fsm
    {
        private Demolisher owner;
        private int miningTimer = 0;
        private int miningInterval;

        public DemolisherFsmFight( Demolisher Demolisher )
        {
            owner = Demolisher;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            miningTimer += deltaTime;
            if ( miningTimer > owner.attackInterval )
            {
                miningTimer = 0;
                if ( owner.target != null && owner.target.Alive() )
                {
                    owner.target.Hurt( owner.damage, owner.hurtType, false, owner );
                }
                else
                {
                    owner.ChangeState( DemolisherState.IDLE, owner.fsmIdle );
                }
            }
        }
    }

}
