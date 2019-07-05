using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class AdditionalDamageDebuff : Debuff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.additionalDamageRateEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerDebuff.additionalDamageRateValue < factor )
                {
                    ownerDebuff.additionalDamageRateValue = factor;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} {1} rate value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} didn't support NaturalNumber for calculate. " , type , attributeAffectType ) );
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.additionalDamageRateEffects.Remove( this );
                ownerDebuff.additionalDamageRateValue = GetMaxFactorFromEffects( ownerDebuff.additionalDamageRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerDebuff.additionalDamageRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} didn't support NaturalNumber for calculate. " , type , attributeAffectType ) );
            }
        }
    }
}
