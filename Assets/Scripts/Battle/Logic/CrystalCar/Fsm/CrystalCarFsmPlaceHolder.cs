using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class CrystalCarFsmPlaceHolder : Fsm
    {
        private CrystalCar owner;

        public CrystalCarFsmPlaceHolder( CrystalCar car )
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
