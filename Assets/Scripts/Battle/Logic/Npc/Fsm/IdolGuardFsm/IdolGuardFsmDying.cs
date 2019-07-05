using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public class IdolGuardFsmDying : Fsm
    {
        private IdolGuard owner;

        public IdolGuardFsmDying( IdolGuard i )
        {
            owner = i;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            owner.Death();
        }
    }
}
