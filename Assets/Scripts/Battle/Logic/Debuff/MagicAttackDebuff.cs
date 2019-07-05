using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class MagicAttackDebuff:Debuff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.magicAtkEffects.Add( this );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.magicAtkRateEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.NaturalNumber )
            {                
                if ( ownerDebuff.magicAtkValue < m )
                {
                    ownerDebuff.magicAtkValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} {1} rate value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
            else if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerDebuff.magicAtkRateValue < factor )
                {
                    ownerDebuff.magicAtkRateValue = factor;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit {0} {1} value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.magicAtkRateEffects.Remove( this );
                ownerDebuff.magicAtkRateValue = GetMaxFactorFromEffects( ownerDebuff.magicAtkRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} rate value has been revert to {3}" , owner.id , type , attributeAffectType , ownerDebuff.magicAtkRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.magicAtkEffects.Remove( this );
                ownerDebuff.magicAtkValue = GetMaxValueFromEffects( ownerDebuff.magicAtkEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerDebuff.magicAtkValue ) );
            }
        }
    }
}
