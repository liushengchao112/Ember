using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class ArmorDebuff : Debuff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.armorRateEffects.Add( this );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.armorEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                FixFactor factor = ChangeMainValueToFactor( m );
                if ( ownerDebuff.armorRateValue < factor )
                {
                    ownerDebuff.armorRateValue = factor;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit defence rate value to {3}" , type , attributeAffectType , metaId , factor ) );
                }
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {                
                if ( ownerDebuff.armorValue < m )
                {
                    ownerDebuff.armorValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit defence rate value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                ownerDebuff.armorRateEffects.Remove( this );
                ownerDebuff.armorRateValue = GetMaxFactorFromEffects( ownerDebuff.armorRateEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} rate value has been revert to {3}" , owner.id , type , attributeAffectType , ownerDebuff.armorRateValue ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.armorEffects.Remove( this );
                ownerDebuff.armorValue = GetMaxValueFromEffects( ownerDebuff.armorEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerDebuff.armorValue ) );
            }
        }
    }
}
