using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class HealthRecoverDebuff : Debuff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.healthRecoverRateEffects.Add( this );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.healthRecoverEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                // healthRecoverRateValue will use the max hp to calculate the healthRegan.  
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerDebuff.healthRecoverRateValue < factor )
                {
                    ownerDebuff.healthRecoverRateValue = factor;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect, string.Format( "{0} {1} {2} has been increased the unit {0} {1} value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {                
                if ( ownerDebuff.healthRecoverValue < m )
                {
                    ownerDebuff.healthRecoverValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} value to {1}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.healthRecoverRateEffects.Remove( this );
                ownerDebuff.healthRecoverRateValue = GetMaxFactorFromEffects( ownerDebuff.healthRecoverRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value {0} has been revert to {3}" , owner.id , type , attributeAffectType , ownerDebuff.healthRecoverRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.healthRecoverEffects.Remove( this );
                ownerDebuff.healthRecoverValue = GetMaxValueFromEffects( ownerDebuff.healthRecoverEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerDebuff.healthRecoverValue ) );
            }
        }
    }
}
