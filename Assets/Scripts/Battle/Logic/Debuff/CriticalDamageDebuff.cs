using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class CriticalDamageDebuff:Debuff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.criticalDamageEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent)
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( " {0} {1} didn't support Percent for calculate. " , type , attributeAffectType ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                if ( ownerDebuff.criticalDamageValue < m )
                {
                    ownerDebuff.criticalDamageValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0}  value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( " {0} {1} didn't support {2} for calculate. " , type , attributeAffectType, calculateType ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.criticalDamageEffects.Remove( this );
                ownerDebuff.criticalDamageValue = GetMaxValueFromEffects( ownerDebuff.criticalDamageEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerDebuff.criticalDamageValue ) );
            }
        }
    }
}
