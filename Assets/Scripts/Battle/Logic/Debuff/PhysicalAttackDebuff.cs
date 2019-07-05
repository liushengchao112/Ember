using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class PhysicalAttackDebuff : Debuff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.physicalAtkRateEffects.Add( this );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.physicalAtkEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerDebuff.physicalAtkRateValue < factor )
                {
                    ownerDebuff.physicalAtkRateValue = factor;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} {1} rate value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {                
                if ( ownerDebuff.physicalAtkValue < m )
                {
                    ownerDebuff.physicalAtkValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} {1} value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.physicalAtkRateEffects.Remove( this );
                ownerDebuff.physicalAtkRateValue = GetMaxFactorFromEffects( ownerDebuff.physicalAtkRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} rate value has been revert to {3}" , owner.id , type , attributeAffectType , ownerDebuff.physicalAtkRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.physicalAtkEffects.Remove( this );
                ownerDebuff.physicalAtkValue = GetMaxValueFromEffects( ownerDebuff.physicalAtkEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerDebuff.physicalAtkValue ) );
            }
        }
    }
}
