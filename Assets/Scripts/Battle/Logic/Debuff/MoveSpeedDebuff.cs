using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class MoveSpeedDebuff: Debuff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.speedRateEffects.Add( this );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.speedEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerDebuff.speedRateValue < factor )
                {
                    ownerDebuff.speedRateValue = factor;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} {1} rate value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {                
                if ( ownerDebuff.speedValue < m )
                {
                    ownerDebuff.speedValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} {1} value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.speedRateEffects.Remove( this );
                ownerDebuff.speedRateValue = GetMaxFactorFromEffects( ownerDebuff.speedRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} rate value has been revert to {3}" , owner.id , type , attributeAffectType , ownerDebuff.speedRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.speedEffects.Remove( this );
                ownerDebuff.speedValue = GetMaxValueFromEffects( ownerDebuff.speedEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value {0} has been revert to {1}" , owner.id , type , attributeAffectType , ownerDebuff.speedValue ) );
            }
        }
    }
}
