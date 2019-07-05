using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class SimSoldierFsmWalk : SoldierFsmWalk
    {
        public SimSoldierFsmWalk( Soldier soldier ) : base( soldier )
        {
            owner = soldier;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            owner.FindOpponent();
        }
    }
}
