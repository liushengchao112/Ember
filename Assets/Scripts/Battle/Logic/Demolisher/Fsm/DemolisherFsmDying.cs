using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class DemolisherFsmDying : Fsm
    {
        private Demolisher owner;

        public DemolisherFsmDying( Demolisher Demolisher )
        {
            owner = Demolisher;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            owner.Dying();
        }
    }

}
