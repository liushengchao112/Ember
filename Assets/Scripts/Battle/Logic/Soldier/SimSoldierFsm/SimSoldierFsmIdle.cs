using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Constants;
using Utils;

namespace Logic
{
    public class SimSoldierFsmIdle : SoldierFsmIdle
    {
        int stayIdleTime = 0;
        int stayIdleTimer = 0;
        bool alreadyNoticeEnterIdle;

        public SimSoldierFsmIdle( Soldier soldier ) : base( soldier )
        {
            owner = soldier;
            stayIdleTime = GameConstants.PVE_SIMUNIT_STAYIDLESTATE_TIME;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            stayIdleTimer = 0;
            alreadyNoticeEnterIdle = false;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            if ( !alreadyNoticeEnterIdle && stayIdleTimer >= stayIdleTime )
            {
                stayIdleTimer = 0;
                alreadyNoticeEnterIdle = true;

                RenderMessage rm = new RenderMessage();
                rm.type = RenderMessage.Type.SimUnitEnterIdleState;
                rm.ownerId = owner.id;
                owner.PostRenderMessage( rm );
            }
            else
            {
                stayIdleTimer += deltaTime;
            }
        }
    }
}
