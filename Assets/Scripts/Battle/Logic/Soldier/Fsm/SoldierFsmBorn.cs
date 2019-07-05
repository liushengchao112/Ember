using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Constants;

namespace Logic
{
    public class SoldierFsmBorn : Fsm
    {
        private int bornTimer = 0;
        private int bornTime = ConvertUtils.ToLogicInt( GameConstants.HERO_BORNTIME );


        protected Soldier owner;
        protected UnitBehaviorListener stateListener;

        public SoldierFsmBorn( Soldier s )
        {
            owner = s;
            stateListener = owner.stateListener;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            bornTimer = 0;
            stateListener.PostAliveStateChangedEvent( true );
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            if ( bornTimer >= bornTime )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} enters idle state", owner.id ) );

				owner.BornComplete();
                owner.ChangeState( SoldierState.IDLE, owner.fsmIdle );
            }

            bornTimer += deltaTime;
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }

}

