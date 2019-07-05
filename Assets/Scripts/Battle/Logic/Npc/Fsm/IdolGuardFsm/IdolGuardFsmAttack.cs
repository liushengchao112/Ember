using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Data;

namespace Logic
{
    public class IdolGuardFsmAttack : Fsm
    {
        private IdolGuard owner;

        public IdolGuardFsmAttack( IdolGuard i )
        {
            owner = i;
        }

        public override void Update( int deltaTime )
        {
            if ( owner.target != null && owner.target.Alive() && owner.target.id == owner.targetId )
            {
                if ( owner.fightTimer >= owner.fightInterval )
                {
                    owner.fightTimer = 0;
                    owner.Fight();
                }
                else
                {
                    owner.fightTimer += deltaTime;
                }

                long distance = FixVector3.SqrDistance( owner.target.position, owner.position );
                if ( distance > owner.attackRange )
                {
                    owner.target = null;
                    owner.targetId = 0;

                    owner.ChangeState( IdolGuardState.IDLE, owner.fsmIdle );
                }
            }
            else
            {
                owner.target = null;
                owner.targetId = 0;

                owner.ChangeState( IdolGuardState.IDLE, owner.fsmIdle );
            }
        }
    }
}
