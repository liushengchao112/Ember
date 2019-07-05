using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class MagicAttackBuff : Buff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.magicAtkEffects.Add( this );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.magicAtkRateEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerBuff.magicAtkRateValue < factor )
                {
                    ownerBuff.magicAtkRateValue = factor;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit rate value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                if ( ownerBuff.magicAtkValue < m )
                {
                    ownerBuff.magicAtkValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit attack buff value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.magicAtkRateEffects.Remove( this );
                ownerBuff.magicAtkRateValue = GetMaxFactorFromEffects( ownerBuff.magicAtkRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} rate value has been revert to {1}" , owner.id , type , attributeAffectType , ownerBuff.magicAtkRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.magicAtkEffects.Remove( this );
                ownerBuff.magicAtkValue = GetMaxValueFromEffects( ownerBuff.magicAtkEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerBuff.magicAtkValue ) );
            }
        }
    }
}

