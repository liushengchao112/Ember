using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class HealthRecoverBuff : Buff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.healthRecoverRateEffects.Add( this );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.healthRecoverEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                // healthRecoverRateValue will use the max hp to calculate the healthRegan.  
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerBuff.healthRecoverRateValue < factor )
                {
                    ownerBuff.healthRecoverRateValue = factor;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit health recover buff value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {                
                if ( ownerBuff.healthRecoverValue < m )
                {
                    ownerBuff.healthRecoverValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit health recover value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {            
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.healthRecoverRateEffects.Remove( this );
                ownerBuff.healthRecoverRateValue = GetMaxFactorFromEffects( ownerBuff.healthRecoverRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerBuff.healthRecoverRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.healthRecoverEffects.Remove( this );
                ownerBuff.healthRecoverValue = GetMaxValueFromEffects( ownerBuff.healthRecoverEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerBuff.healthRecoverValue ) );
            }
        }
    }
}
