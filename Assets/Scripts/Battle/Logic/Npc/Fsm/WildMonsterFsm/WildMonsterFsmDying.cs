using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class WildMonsterFsmDying : Fsm
    {
        public Npc owner;

        public WildMonsterFsmDying() { }

        public WildMonsterFsmDying( Npc w )
        {
            owner = w;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            owner.Death();
        }
    }
}
