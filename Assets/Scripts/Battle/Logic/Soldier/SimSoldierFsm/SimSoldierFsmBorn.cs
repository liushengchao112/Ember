using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class SimSoldierFsmBorn : SoldierFsmBorn
    {
        public SimSoldierFsmBorn( Soldier soldier ) : base( soldier )
        {
            owner = soldier;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );
        }
    }
}
