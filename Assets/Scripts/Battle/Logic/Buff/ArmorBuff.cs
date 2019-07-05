using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class ArmorBuff : Buff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.armorRateEffects.Add( this );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.armorEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerBuff.armorRateValue < factor )
                {
                    ownerBuff.armorRateValue = factor;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit defence rate value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {                
                if ( ownerBuff.armorValue < m )
                {
                    ownerBuff.armorValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit defence rate value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerBuff.armorRateEffects.Remove( this );
                ownerBuff.armorRateValue = GetMaxFactorFromEffects( ownerBuff.armorRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} rate value has been revert to {3}" , owner.id , type , attributeAffectType , ownerBuff.armorRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.armorEffects.Remove( this );
                ownerBuff.armorValue = GetMaxValueFromEffects( ownerBuff.armorEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerBuff.armorValue ) );
            }
        }
    }
}
