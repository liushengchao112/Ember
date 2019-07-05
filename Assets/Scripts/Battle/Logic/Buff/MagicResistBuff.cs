using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class MagicResistBuff : Buff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.magicResistRateEffects.Add( this );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.magicResistEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerBuff.magicResistRateValue < factor )
                {
                    ownerBuff.magicResistRateValue = factor;

                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} rate value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {                
                if ( ownerBuff.magicResistValue < m )
                {
                    ownerBuff.magicResistValue = m;

                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} rate value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.magicResistRateEffects.Remove( this );
                ownerBuff.magicResistRateValue = GetMaxFactorFromEffects( ownerBuff.magicResistRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} rate value has been revert to {2}" , owner.id , type , ownerBuff.magicResistRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.magicResistEffects.Remove( this );
                ownerBuff.magicResistValue = GetMaxValueFromEffects( ownerBuff.magicResistEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} value {0} has been revert to {2}" , owner.id , type , ownerBuff.magicResistValue ) );
            }
        }
    }
}

