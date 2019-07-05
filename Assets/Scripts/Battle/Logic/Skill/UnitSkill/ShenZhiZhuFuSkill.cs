using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

using UnitSkill = Data.UnitSkillsProto.UnitSkill;

namespace Logic
{
    // Aura type skills
    public class ShenZhiZhuFuSkill : Skill
    {
        // Store the owner affected by buff.
        private Dictionary<long, List<AttributeEffect>> attributeEffectOwnerDic;

        public override void Initialize( long id, Soldier owner, UnitSkill skillProto, int index )
        {
            base.Initialize( id, owner, skillProto, index );

            attributeEffectOwnerDic = new Dictionary<long, List<AttributeEffect>>();
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            List<Soldier> soldiers = FindFriendlySoldiers( mark, position, radius );

            // Add buff to new affected unit
            for ( int i = 0; i < soldiers.Count; i++ )
            {
                if ( !attributeEffectOwnerDic.ContainsKey( soldiers[i].id ) )
                {
                    AddSkillBuffToUnit( soldiers[i] );
                }
            }

            Dictionary<long, List<AttributeEffect>> buffer = new Dictionary<long, List<AttributeEffect>>( attributeEffectOwnerDic );

            // Detach skill buff when soldier out of skill area.
            foreach ( long unitId in buffer.Keys )
            {
                Soldier s = soldiers.Find( p => p.id == unitId );

                if ( s == null )
                {
                    for ( int i = 0; i < attributeEffectOwnerDic[unitId].Count; i++ )
                    {
                        attributeEffectOwnerDic[unitId][i].Detach();
                    }

                    attributeEffectOwnerDic[unitId].Clear();
                    attributeEffectOwnerDic.Remove( unitId );
                }
            }

            buffer.Clear();
        }

        public override void ReleaseEnd()
        {
            base.ReleaseEnd();

            // Detach skill debuff when soldier out of skill area.
            Dictionary<long, List<AttributeEffect>> buffer = new Dictionary<long, List<AttributeEffect>>( attributeEffectOwnerDic );
            foreach ( long unitId in buffer.Keys )
            {
                for ( int i = 0; i < attributeEffectOwnerDic[unitId].Count; i++ )
                {
                    attributeEffectOwnerDic[unitId][i].Detach();
                }

                attributeEffectOwnerDic[unitId].Clear();
                attributeEffectOwnerDic.Remove( unitId );
            }
            buffer.Clear();
            buffer = null;
        }

        private void AddSkillBuffToUnit( Soldier s )
        {
            if ( s == null || !s.Alive() )
            {
                return;
            }

            List<AttributeEffect> attributeEffects = new List<AttributeEffect>();

            for ( int i = 0; i < base.attributeEffects.Count; i++ )
            {
                int buffId = base.attributeEffects[i];

                AttributeEffect ae = GenerateAttributeEffect( buffId );

                // Notice skill the buff has been detached for some reason, maybe the unit has been dead
                ae.RegisterNoticeGiverDestroyMethod( ( b ) =>
                {
                    if ( attributeEffectOwnerDic.ContainsKey( b.taker.id ) )
                    {
                        attributeEffectOwnerDic[b.taker.id].Remove( b );
                        if ( attributeEffectOwnerDic[b.taker.id].Count == 0 )
                        {
                            attributeEffectOwnerDic.Remove( b.taker.id );
                        }
                    }
                } );

                ae.Attach( owner, s );

                attributeEffects.Add( ae );
            }

            attributeEffectOwnerDic.Add( s.id, attributeEffects );
        }
    }
}
