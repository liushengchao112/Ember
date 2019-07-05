using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class SummonedUnitFsmBorn : Fsm
    {
        private SummonedUnit owner;

        //Timer..
        private int brithTimer;

        private int brithDuration;

        public SummonedUnitFsmBorn( SummonedUnit summon )
        {
            owner = summon;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            brithTimer = 0;
            brithDuration = owner.brithDuration;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            if ( brithTimer >= brithDuration )
            {
                owner.Idle();
            }

            brithTimer += deltaTime;
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
