using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class SummonedUnitFsmIdle : Fsm
    {
        private SummonedUnit owner;

        public SummonedUnitFsmIdle( SummonedUnit summon )
        {
            owner = summon;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            RenderMessage rm = new RenderMessage();
            rm.ownerId = owner.id;
            rm.direction = owner.direction.vector3;

            rm.type = RenderMessage.Type.SummonedUnitIdle;
            owner.PostRenderMessage( rm );
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            LogicUnit unit = owner.LookingForOpponent();

            if ( unit != null )
            {
                owner.Attack( unit );
            }
        }
    }
}
