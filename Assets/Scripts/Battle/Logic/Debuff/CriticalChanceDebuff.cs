using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class CriticalChanceDebuff : Debuff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.criticalChanceEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( " {0} {1} didn't support {2} for calculate. " , type , attributeAffectType, calculateType ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                if ( ownerDebuff.criticalChanceValue < m )
                {
                    ownerDebuff.criticalChanceValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been decreased the unit critical chance  value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( " {0} {1} didn't support {2} for calculate. ", type , attributeAffectType, calculateType ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerDebuff.criticalChanceEffects.Remove( this );
                ownerDebuff.criticalChanceValue = GetMaxValueFromEffects( ownerDebuff.criticalChanceEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerDebuff.criticalChanceValue ) );
            }
        }
    }
}
