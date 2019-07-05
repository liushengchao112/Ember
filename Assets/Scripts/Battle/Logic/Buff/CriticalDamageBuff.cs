using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class CriticalDamageBuff : Buff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.criticalDamageEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( " {0} {1} didn't support Percent for calculate. " , type , attributeAffectType ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                if ( ownerBuff.criticalDamageValue < m )
                {
                    ownerBuff.criticalDamageValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit critical chance  value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( " {0} {1} didn't support {0} for calculate. " , type , attributeAffectType, calculateType ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.criticalDamageEffects.Remove( this );
                ownerBuff.criticalDamageValue = GetMaxValueFromEffects( ownerBuff.criticalDamageEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerBuff.criticalDamageValue ) );
            }
        }
    }

}
