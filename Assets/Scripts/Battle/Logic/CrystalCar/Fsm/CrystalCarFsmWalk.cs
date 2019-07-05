using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class CrystalCarFsmWalk : Fsm
    {
        private CrystalCar owner;

        public CrystalCarFsmWalk( CrystalCar crystalCar )
        {
            owner = crystalCar;
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
