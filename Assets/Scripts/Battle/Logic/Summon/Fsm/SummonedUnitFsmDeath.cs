using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public class SummonedUnitFsmDeath : Fsm
    {
        private SummonedUnit owner;

        public SummonedUnitFsmDeath( SummonedUnit summon )
        {
            owner = summon;
        }
    }
}
