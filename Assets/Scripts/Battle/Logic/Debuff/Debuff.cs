using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Utils;
using Constants;

namespace Logic
{
    public enum DebuffType
    {
        None = 0,
        PhysicalDamage = 1,
        MagicDamage = 2,
        PhysicalAttack = 3,
        MagicAttack = 4,
        Armor = 5,
        MagicResist = 6,
        CriticalChance = 7,
        CriticalDamage = 8,
        Speed = 9,
        AttackSpeed = 10,
        MaxHealth = 11,
        HealthRecover = 12,
        ForbiddenMoves = 13,
        Slience = 14,
        ForbiddenAttacks = 15,
        Stun = 16,
        //AdditionalDamage = 17
        Sneer = 17
    }

    public class Debuff : AttributeEffect
	{
        public DebuffType type;
        public DebuffHandler ownerDebuff;

        public override void Init( int id, int type, int metaId, float lifeTime, float mainValue , int calculateType )
		{
            this.id = id;
            this.type = (DebuffType)type;
            this.metaId = metaId;
            this.calculateType = (CalculateType)calculateType;
            this.duration = Mathf.RoundToInt( lifeTime * GameConstants.LOGIC_FIXPOINT_PRECISION );

            if ( this.calculateType == CalculateType.NaturalNumber )
            {
                this.mainValue = Mathf.RoundToInt( mainValue );
            }
            else if ( this.calculateType == CalculateType.Percent )
            {
                this.mainValue = ConvertUtils.ToLogicInt( mainValue );
            }

            attributeAffectType = AttributeAffectType.Debuff;

            if ( this.duration == infiniteTimeTag )
            {
                infinite = true;
            }
            else
            {
                infinite = false;
            }
        }

		public override void Attach( LogicUnit g, LogicUnit t )
		{
            DebugUtils.Log( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} has been attached a {2} {3} {4} to {5} mark unit {6}" , g.type , g.id , type , attributeAffectType , metaId , t.mark , t.id ) );
            state = AttributeEffectState.StartRelease;

            giver = g;
            taker = t;
            // owner will always be taker...
            owner = taker;

            if ( t.type == LogicUnitType.Soldier )
            {
                Soldier s = (Soldier)taker;
                ownerDebuff = s.debuffHandler;
            }
            else if ( t.type == LogicUnitType.NPC )
            {
                Npc n = (Npc)taker;
                ownerDebuff = n.debuffHandler;
            }
            else if ( t.type == LogicUnitType.Demolisher )
            {
                Demolisher d = (Demolisher)taker;
                ownerDebuff = d.debuffHandler;
            }
            else if ( t.type == LogicUnitType.CrystalCar )
            {
                CrystalCar c = (CrystalCar)taker;
                ownerDebuff = c.debuffHandler;
            }
            else
            {
                DebugUtils.LogWarning( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} can't handle {2} now!" , t.type , t.id , attributeAffectType ) );
            }

            if ( ownerDebuff != null && effectResId != 0 )
            {
                RenderMessage rm = new RenderMessage();
                rm.type = RenderMessage.Type.AttachAttributeEffect;
                rm.ownerId = id;
                rm.arguments.Add( "HolderType", (int)t.type );
                rm.arguments.Add( "HolderId", t.id );
                rm.arguments.Add( "resId", effectResId );
                rm.arguments.Add( "bindPointName", effectBindPoint );

                PostRenderMessage( rm );
            }
        }

		public override void LogicUpdate( int delatTime )
		{
			if ( state == AttributeEffectState.StartRelease )
			{
				StartRelease();
			}
			else if ( state == AttributeEffectState.Relaseing )
			{
				Releasing( delatTime );

                if ( !infinite )
                {
                    if ( lifeTimer < duration )
                    {
                        lifeTimer += delatTime;
                    }
                    else
                    {
                        state = AttributeEffectState.Released;
                    }
                }
			}
			else if ( state == AttributeEffectState.Released )
			{
				Released();
			}
		}

		public override void StartRelease()
		{
			state = AttributeEffectState.Relaseing;
		}

		public override void Releasing( int deltaTime )
		{

		}

		public override void Released()
		{
            this.Detach();

			if ( NoticeGiverDestroyMethod != null )
			{
				NoticeGiverDestroyMethod( this );
			}
		}

		public override void Detach()
		{
            DebugUtils.Log( DebugUtils.Type.AI_AttributeEffect , string.Format( " {0}'s {1} {2} has been detached " , owner.id , type , attributeAffectType ) );

            if ( ownerDebuff != null && effectResId != 0 )
            {
                RenderMessage rm = new RenderMessage();
                rm.type = RenderMessage.Type.DetachAttributeEffect;
                rm.ownerId = id;
                rm.arguments.Add( "HolderType", (int)taker.type );
                rm.arguments.Add( "HolderId", taker.id );
                rm.arguments.Add( "resId", effectResId );

                PostRenderMessage( rm );
            }
        }

        public override void SetMainValue( int m )
        {
            DebugUtils.Assert( false , " SetMainValue Fuction Is Not Override!" );
        }

        #region Utils
        protected int GetMaxValueFromEffects( List<Debuff> effects )
        {
            int value = 0;
            for ( int i = 0; i < effects.Count; i++ )
            {
                if ( value < effects[i].MainValue )
                {
                    value = effects[i].MainValue;
                }
            }

            return value;
        }

        protected FixFactor GetMaxFactorFromEffects( List<Debuff> effects )
        {
            FixFactor value = FixFactor.zero;
            for ( int i = 0; i < effects.Count; i++ )
            {
                if ( value < effects[i].MainFactor )
                {
                    value = effects[i].MainFactor;
                }
            }

            return value;
        }
        #endregion
    }
}

