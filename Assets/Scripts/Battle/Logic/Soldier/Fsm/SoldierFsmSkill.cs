using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class SoldierFsmSkill : Fsm
    {
        protected Soldier owner;
        protected UnitBehaviorListener stateListener;

        public SoldierFsmSkill()
        {

        }

        public SoldierFsmSkill( Soldier soldier )
        {
            owner = soldier;
            stateListener = owner.stateListener;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            stateListener.PostSkillStateEnter();
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );
        }

        public override void OnExit()
        {
            owner.skillHandler.ExitSKillState();
            base.OnExit();
        }
    }
}

