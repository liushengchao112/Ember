using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public class SummonedUnitFsmDying : Fsm
    {
        private SummonedUnit owner;

        public SummonedUnitFsmDying( SummonedUnit summon )
        {
            owner = summon;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            RenderMessage rm = new RenderMessage();
            rm.ownerId = owner.id;
            rm.direction = owner.direction.vector3;
            rm.type = RenderMessage.Type.SummonedUnitDeath;
            owner.PostRenderMessage( rm );
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            owner.Released();
        }
    }
}
