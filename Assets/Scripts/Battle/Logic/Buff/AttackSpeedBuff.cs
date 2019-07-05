using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class AttackSpeedBuff : Buff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.attackSpeedRateEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerBuff.attackSpeedRateValue < factor )
                {
                    ownerBuff.attackSpeedRateValue = factor;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit defence rate value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} didn't support NaturalNumber for calculate." , type , attributeAffectType ) );
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.attackSpeedRateEffects.Remove( this );
                ownerBuff.attackSpeedRateValue = GetMaxFactorFromEffects( ownerBuff.attackSpeedRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} rate value has been revert to {3}" , owner.id , type , attributeAffectType , ownerBuff.attackSpeedRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} can't support NaturalNumber for calculate." , type , attributeAffectType ) );
            }
        }
    }
}

