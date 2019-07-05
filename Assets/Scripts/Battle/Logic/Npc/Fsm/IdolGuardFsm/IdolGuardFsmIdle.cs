using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class IdolGuardFsmIdle : Fsm
    {
        private IdolGuard owner;

        public IdolGuardFsmIdle( IdolGuard i )
        {
            owner = i;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            owner.FindOppenont();
        }
    }
}
