using UnityEngine;
using System.Collections;

using Utils;
using System;

namespace Logic
{
    public class AttackSpeedDebuff : Debuff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.attackSpeedRateEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerDebuff.attackSpeedRateValue < factor )
                {
                    ownerDebuff.attackSpeedRateValue = factor;
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
                ownerDebuff.attackSpeedRateEffects.Remove( this );
                ownerDebuff.attackSpeedRateValue = GetMaxFactorFromEffects( ownerDebuff.attackSpeedRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} rate value has been revert to {1}" , owner.id , type , attributeAffectType , ownerDebuff.attackSpeedRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} can't support NaturalNumber for calculate." , type , attributeAffectType ) );
            }
        }
    }
}
