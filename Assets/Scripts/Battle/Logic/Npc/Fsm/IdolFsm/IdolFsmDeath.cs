using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class IdolFsmDeath : Fsm
    {
        private Idol owner;

        public IdolFsmDeath( Idol i )
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

