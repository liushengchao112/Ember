using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public class IdolFsmDying : Fsm
    {
        private Idol owner;

        public IdolFsmDying( Idol i )
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
