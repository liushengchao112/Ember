using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class MoveSpeedBuff : Buff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.speedRateEffects.Add( this );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.speedEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerBuff.speedRateValue < factor )
                {
                    ownerBuff.speedRateValue = factor;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect, string.Format( "{0} {1} {2} has been increased the unit {0} {1} rate value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {                
                if ( ownerBuff.speedValue < m )
                {
                    ownerBuff.speedValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} {1} value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.speedRateEffects.Remove( this );
                ownerBuff.speedRateValue = GetMaxFactorFromEffects( ownerBuff.speedRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} rate value has been revert to {3}" , owner.id , type , attributeAffectType , ownerBuff.speedRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.speedEffects.Remove( this );
                ownerBuff.speedValue = GetMaxValueFromEffects( ownerBuff.speedEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {1}" , owner.id , type , attributeAffectType , ownerBuff.speedValue ) );
            }
        }
    }
}
