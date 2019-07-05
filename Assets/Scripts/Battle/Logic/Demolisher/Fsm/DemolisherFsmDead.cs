using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class DemolisherFsmDead : Fsm
    {
        private Demolisher owner;

        public DemolisherFsmDead( Demolisher Demolisher )
        {
            owner = Demolisher;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            owner.Destroy();
        }
    }

}
