using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

using UnitSkill = Data.UnitSkillsProto.UnitSkill;

namespace Logic
{
    public class JinGongZhiQuSkill : Skill
    {
        // Store the owner affected by buff.
        private Dictionary<long, List<AttributeEffect>> effectOwnerDic;

        public override void Initialize( long id, Soldier owner, UnitSkill skillProto, int index )
        {
            base.Initialize( id, owner, skillProto, index );

            effectOwnerDic = new Dictionary<long, List<AttributeEffect>>();
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            List<Soldier> soldiers = FindFriendlySoldiers( mark, position, radius );

            // Add buff to new affected unit
            for ( int i = 0; i < soldiers.Count; i++ )
            {
                if ( !effectOwnerDic.ContainsKey( soldiers[i].id ) )
                {
                    AddSkillBuffToUnit( soldiers[i] );
                }
            }

            Dictionary<long, List<AttributeEffect>> buffer = new Dictionary<long, List<AttributeEffect>>( effectOwnerDic );

            // Detach skill buff when soldier out of skill area.
            foreach ( long unitId in buffer.Keys )
            {
                Soldier s = soldiers.Find( p => p.id == unitId );

                if ( s == null )
                {
                    for ( int i = 0; i < effectOwnerDic[unitId].Count; i++ )
                    {
                        effectOwnerDic[unitId][i].Detach();
                    }

                    effectOwnerDic[unitId].Clear();
                    effectOwnerDic.Remove( unitId );
                }
            }

            buffer.Clear();
        }

        public override void ReleaseEnd()
        {
            base.ReleaseEnd();

            // Detach skill debuff when soldier out of skill area.
            Dictionary<long, List<AttributeEffect>> buffer = new Dictionary<long, List<AttributeEffect>>( effectOwnerDic );
            foreach ( long unitId in buffer.Keys )
            {
                for ( int i = 0; i < effectOwnerDic[unitId].Count; i++ )
                {
                    effectOwnerDic[unitId][i].Detach();
                }

                effectOwnerDic[unitId].Clear();
                effectOwnerDic.Remove( unitId );
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

            if ( attributeEffects == null || attributeEffects.Count == 0 )
            {
                return;
            }

            List<AttributeEffect> effects = new List<AttributeEffect>();

            for ( int i = 0; i < attributeEffects.Count; i++ )
            {
                int buffId = attributeEffects[i];

                AttributeEffect attributeEffect = GenerateAttributeEffect( buffId );

                // Notice skill the buff has been detached for some reason, maybe the unit has been dead
                attributeEffect.RegisterNoticeGiverDestroyMethod( ( b ) =>
                {
                    if ( effectOwnerDic.ContainsKey( b.taker.id ) )
                    {
                        effectOwnerDic[b.taker.id].Remove( b );
                        if ( effectOwnerDic[b.taker.id].Count == 0 )
                        {
                            effectOwnerDic.Remove( b.taker.id );
                        }
                    }
                } );

                attributeEffect.Attach( owner, s );
                effects.Add( attributeEffect );
            }

            effectOwnerDic.Add( s.id, effects );
        }
    }
}
