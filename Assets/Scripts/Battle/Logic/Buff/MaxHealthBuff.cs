using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class MaxHealthBuff : Buff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.maxHealthRateEffects.Add( this );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.maxHealthEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            base.SetMainValue( m );

            if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerBuff.maxHealthRateValue < factor )
                {
                    ownerBuff.maxHealthRateValue = factor;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {                
                if ( ownerBuff.maxHealthValue < m )
                {
                    ownerBuff.maxHealthValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.maxHealthRateEffects.Remove( this );
                ownerBuff.maxHealthRateValue = GetMaxFactorFromEffects( ownerBuff.maxHealthRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {1}" , owner.id , type , attributeAffectType , ownerBuff.maxHealthRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.maxHealthEffects.Remove( this );
                ownerBuff.maxHealthValue = GetMaxValueFromEffects( ownerBuff.maxHealthEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {1}" , owner.id , type , attributeAffectType , ownerBuff.maxHealthValue ) );
            }
        }
    }
}
