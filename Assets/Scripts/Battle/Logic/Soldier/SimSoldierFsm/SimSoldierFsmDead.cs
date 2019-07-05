using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class SimSoldierFsmDead : SoldierFsmDead
    {
        public SimSoldierFsmDead( Soldier soldier ) : base( soldier )
        {
            owner = soldier;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

        }
    }
}
