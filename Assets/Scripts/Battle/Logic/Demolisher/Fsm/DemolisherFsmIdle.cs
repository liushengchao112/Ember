using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class DemolisherFsmIdle : Fsm
    {
        private Demolisher owner;

        public DemolisherFsmIdle( Demolisher car )
        {
            owner = car;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            owner.FindOpponent();
        }
    }

}
