using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    // TODO: stun debuff's design is not clear about the stun debuff need to be added or replaced, when soldier in stun state, and now it be attached a stun debuff.
    // Now use the replace logic. 
    public class StunDebuff : Debuff
    {
        private Soldier soldier;

        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( t.type == LogicUnitType.Soldier )
            {
                soldier = (Soldier)t;

                if ( !soldier.InStunState() )
                {
                    soldier.Stun();
                }

                soldier.debuffHandler.stunEffect = this;

                if ( soldier.debuffHandler.stunTime <= duration )
                {
                    soldier.debuffHandler.stunTime = duration;
                }
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} can't handle {2} {3} now!" , t.type , t.id , type , attributeAffectType ) );
            }
        }

        public override void Detach()
        {
            base.Detach();

            soldier.debuffHandler.stunEffect = new Debuff()            ;
            soldier.debuffHandler.stunTime = 0;

            soldier.StunEnd();
        }
    }
}
