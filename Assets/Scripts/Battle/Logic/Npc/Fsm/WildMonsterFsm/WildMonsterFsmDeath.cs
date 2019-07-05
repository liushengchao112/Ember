using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class WildMonsterFsmDeath : Fsm
    {
        public Npc owner;

        public WildMonsterFsmDeath() {}

        public WildMonsterFsmDeath( Npc w )
        {
            owner = w;
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void Update( int deltaTime )
        {
            if ( owner.rebornTimer >= owner.rebornInterval )
            {
                owner.rebornTimer = 0;
                owner.Reborn();
            }
            else
            {
                owner.rebornTimer += deltaTime;
            }
        }
    }
}
