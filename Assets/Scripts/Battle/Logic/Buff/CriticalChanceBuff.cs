using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class CriticalChanceBuff : Buff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.criticalChanceEffects.Add( this );
            }

            SetMainValue( mainValue );
        }

        public override void SetMainValue( int m )
        {
            if ( calculateType == CalculateType.Percent )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( " {0} {1} buff didn't support {2} for calculate. " , type , attributeAffectType, calculateType ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                if ( ownerBuff.criticalChanceValue < m )
                {
                    ownerBuff.criticalChanceValue = m;
                    DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been increased the unit value to {3}" , type , attributeAffectType , metaId , m ) );
                }
            }
        }

        public override void Detach()
        {
            DebugUtils.Log( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} {2} has been detached" , type , attributeAffectType , metaId ) );

            base.Detach();

            if ( calculateType == CalculateType.Percent )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( " {0} {1} didn't support Percent for calculate. " , type , attributeAffectType ) );
            }
            else if ( calculateType == CalculateType.NaturalNumber )
            {
                ownerBuff.criticalChanceEffects.Remove( this );
                ownerBuff.criticalChanceValue = GetMaxValueFromEffects( ownerBuff.criticalChanceEffects );
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "the unit {0} {1} {2} value has been revert to {3}" , owner.id , type , attributeAffectType , ownerBuff.criticalChanceValue ) );
            }
        }
    }
}
