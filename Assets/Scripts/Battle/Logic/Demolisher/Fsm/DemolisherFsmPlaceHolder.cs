using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class DemolisherFsmPlaceHolder : Fsm
    {
        private Demolisher owner;

        public DemolisherFsmPlaceHolder( Demolisher car )
        {
            owner = car;
        }

        // Update is called once per frame
        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );
        }
    }

}
