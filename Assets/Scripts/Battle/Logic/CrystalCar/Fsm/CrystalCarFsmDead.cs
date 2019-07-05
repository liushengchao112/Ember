using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class CrystalCarFsmDead : Fsm
    {
        private CrystalCar owner;

        public CrystalCarFsmDead( CrystalCar crystalCar )
        {
            owner = crystalCar;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            owner.Destroy();
        }
    }

}
