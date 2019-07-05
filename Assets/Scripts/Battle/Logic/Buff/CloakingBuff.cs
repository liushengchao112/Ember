using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class CloakingBuff : Buff
    {
        private Soldier soldier;

        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            soldier = (Soldier)owner;

            Buff b = soldier.buffHandler.cloakingEffect;
            if ( b != null)
            {
                b.Detach();
            }

            soldier.buffHandler.cloakingEffect = this;
            soldier.SetVisible( false );
            soldier.buffHandler.cloakingTime = duration;
        }

        public override void Detach()
        {            
            base.Detach();

            if ( soldier.buffHandler.cloakingEffect == null )
            {
                return;
            }
            soldier.buffHandler.cloakingEffect = null;
            soldier.buffHandler.cloakingTime = 0;
            soldier.SetVisible( true );
        }
    }
}
