using System.Collections;
using System.Collections.Generic;

using Utils;

namespace Logic
{
    public class SimSoldierFsmChase : SoldierFsmChase
    {
        public SimSoldierFsmChase( Soldier soldier ) : base( soldier )
        {
            owner = soldier;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );
        }
    }
}
