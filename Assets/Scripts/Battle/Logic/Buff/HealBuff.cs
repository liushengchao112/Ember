using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class HealBuff : Buff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            // check taker type 
            if ( t.type != LogicUnitType.Soldier )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} can't handle {2} {3} now!" , t.type , t.id , type , attributeAffectType ) );
                return;
            }

            base.Attach( g, t );

            int value = 0;

            if ( calculateType == CalculateType.Percent )
            {
                value = (int)( t.maxHp * MainValue );

                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit health rate value to {3}" , type , attributeAffectType , metaId , MainValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                value = (int)MainValue;

                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit health value to {3}" , type , attributeAffectType , metaId , MainValue ) );
            }

            Soldier s = (Soldier)owner;
            s.Heal( value );

            this.Detach();
        }

        public override void Detach()
        {
            base.Detach();
        }
    }
}
