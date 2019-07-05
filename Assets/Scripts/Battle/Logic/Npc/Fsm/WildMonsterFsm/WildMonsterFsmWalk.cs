using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class WildMonsterFsmWalk : Fsm
    {
        public Npc owner;

        public WildMonsterFsmWalk() { }

        public WildMonsterFsmWalk( Npc n )
        {
            owner = n;
        }
        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            owner.WaypointHandler();

            owner.pathAgent.Move( owner.speed * deltaTime );
        }
    }
}
