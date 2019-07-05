using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public class SimSoldierFsmSkill : SoldierFsmSkill
    {
        public SimSoldierFsmSkill() 
        {

        }

        public SimSoldierFsmSkill( Soldier soldier ) : base( soldier )
        {
            owner = soldier;
        }
    }
}
