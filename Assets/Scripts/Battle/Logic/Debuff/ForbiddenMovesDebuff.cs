using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class ForbiddenMovesDebuff : Debuff
    {
        private Soldier soldier;
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( t.type == LogicUnitType.Soldier )
            {
                soldier = (Soldier)t;

                Debuff targetDebuff = soldier.debuffHandler.forbiddenMoveEffect;
                if ( targetDebuff != null )
                {
                    targetDebuff.Detach();
                    soldier.debuffHandler.forbiddenMoveTime = 0;
                }

                targetDebuff = this;

                soldier.SetMoveableState( true );
                soldier.debuffHandler.forbiddenMoveTime = duration;
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} can't handle {2} {3} now!" , t.type , t.id , type , attributeAffectType ) );
            }
        }

        public override void Detach()
        {
            soldier.SetMoveableState( false );
            soldier.debuffHandler.forbiddenMoveEffect = null;
            soldier.debuffHandler.forbiddenMoveTime = 0;

            base.Detach();
        }
    }
}
