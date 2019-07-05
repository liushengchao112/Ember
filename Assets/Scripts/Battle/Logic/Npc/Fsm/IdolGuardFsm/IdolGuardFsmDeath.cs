using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class IdolGuardFsmDeath : Fsm
    {
        private IdolGuard owner;

        public IdolGuardFsmDeath( IdolGuard i )
        {
            owner = i;
        }

        public override void Update( int deltaTime )
        {
            if ( owner.rebornTimer >= owner.rebornInterval )
            {
                owner.Reborn();
            }
            else
            {
                owner.rebornTimer += deltaTime;
            }
        }
    }
}
