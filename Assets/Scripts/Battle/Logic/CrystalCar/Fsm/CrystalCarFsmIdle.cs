using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class CrystalCarFsmIdle : Fsm
    {
        private CrystalCar owner;

        public CrystalCarFsmIdle( CrystalCar car )
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
