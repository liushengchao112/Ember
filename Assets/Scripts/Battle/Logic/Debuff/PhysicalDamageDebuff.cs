using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class PhysicalDamageDebuff : Debuff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            int hurtValue = 0;
            if ( calculateType == CalculateType.Percent )
            {
                hurtValue = MainValue * t.maxHp ;

                t.Hurt( hurtValue, AttackPropertyType.PhysicalAttack, false, g );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                hurtValue = MainValue;

                t.Hurt( hurtValue, AttackPropertyType.PhysicalAttack, false, g );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( "Can't handler this hurt type {0} in {1} {2} " , hurtValue , type , attributeAffectType ) );
            }

            DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been hurt the unit value to {3}" , type , attributeAffectType , metaId , hurtValue ) );

            Detach();
        }

        public override void Detach()
        {
            base.Detach();
        }
    }
}

