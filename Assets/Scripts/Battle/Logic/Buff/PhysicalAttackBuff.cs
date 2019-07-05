using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class PhysicalAttackBuff : Buff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.physicalAtkRateEffects.Add( this );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.physicalAtkEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerBuff.physicalAtkRateValue < factor )
                {
                    ownerBuff.physicalAtkRateValue = factor;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {0} has been increased the unit {0} {1} rate value to {1}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {                
                if ( ownerBuff.physicalAtkValue < m )
                {
                    ownerBuff.physicalAtkValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} {1} value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.physicalAtkRateEffects.Remove( this );
                ownerBuff.physicalAtkRateValue = GetMaxFactorFromEffects( ownerBuff.physicalAtkRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} rate value has been revert to {3}" , owner.id , type , attributeAffectType , ownerBuff.physicalAtkRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.physicalAtkEffects.Remove( this );
                ownerBuff.physicalAtkValue = GetMaxValueFromEffects( ownerBuff.physicalAtkEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerBuff.physicalAtkValue ) );
            }
        }
    }
}
