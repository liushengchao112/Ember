using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class MagicResistDebuff : Debuff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.magicResistRateEffects.Add( this );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.magicResistEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerDebuff.magicResistRateValue < factor )
                {
                    ownerDebuff.magicResistRateValue = factor;

                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} resist rate value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {                
                if ( ownerDebuff.magicResistValue < m )
                {
                    ownerDebuff.magicResistValue = m;

                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} rate value to {1}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.magicResistRateEffects.Remove( this );
                ownerDebuff.magicResistRateValue = GetMaxFactorFromEffects( ownerDebuff.magicResistRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} rate value has been revert to {2}" , owner.id , type , ownerDebuff.magicResistRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.magicResistEffects.Remove( this );
                ownerDebuff.magicResistValue = GetMaxValueFromEffects( ownerDebuff.magicResistEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} value has been revert to {2}" , owner.id , type , ownerDebuff.magicResistValue ) );
            }
        }
    }
}