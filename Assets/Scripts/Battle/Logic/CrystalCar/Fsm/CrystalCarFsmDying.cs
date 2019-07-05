using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class CrystalCarFsmDying : Fsm
    {
        private CrystalCar owner;

        public CrystalCarFsmDying( CrystalCar crystalCar )
        {
            owner = crystalCar;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            owner.Dying();
        }
    }

}
