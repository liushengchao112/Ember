using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class DemolisherFsmWalk : Fsm
    {
        private Demolisher owner;

        public DemolisherFsmWalk( Demolisher Demolisher )
        {
            owner = Demolisher;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            owner.FindOpponent();
            owner.WaypointHandler();

            owner.pathAgent.Move( owner.speed * deltaTime );
        }
    }
}
