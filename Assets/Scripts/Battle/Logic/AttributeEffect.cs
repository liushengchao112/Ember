using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Constants;

namespace Logic
{
    public enum AttributeAffectType
    {
        None = 0,
        Buff = 1,
        Debuff = 2,
    }
    
    public enum AttributeEffectState
    {
        StartRelease,
        Relaseing,
        Released
    }

    public abstract class AttributeEffect
    {
        public int id;

        public int metaId;// For search proto
        public AttributeAffectType attributeAffectType = AttributeAffectType.None;

        public LogicUnit giver;
        public LogicUnit taker;

        public LogicUnit owner;

        protected CalculateType calculateType = CalculateType.None;
        protected int duration = 0;
        protected int lifeTimer = 0;
        protected bool infinite = false;
        protected int infiniteTimeTag = Mathf.RoundToInt( -1 * GameConstants.LOGIC_FIXPOINT_PRECISION );

        protected int effectResId = 0;
        protected string effectBindPoint = string.Empty;

        protected int mainValue;
        public int MainValue
        {
            get
            {
                return mainValue;
            }
        }

        protected FixFactor mainFactor;
        public FixFactor MainFactor
        {
            get
            {
                return mainFactor;
            }
        }

        protected AttributeEffectState state;
        protected Action<AttributeEffect> NoticeGiverDestroyMethod;
        protected Action<AttributeEffect> NoticeTakerDestroyMethod;

        protected RenderMessageHandler PostRenderMessage;

        public abstract void Init( int id, int type, int metaId, float lifeTime, float mainValue, int calculateType );

        public abstract void Attach( LogicUnit g, LogicUnit t );

        public abstract void LogicUpdate( int delatTime );

        public abstract void StartRelease();

        public abstract void Releasing( int delatTime );

        public abstract void Released();

        public abstract void Detach();

        public virtual void SetMainValue( int m )
        {
            if ( this.calculateType == CalculateType.NaturalNumber )
            {
                this.mainValue = m;
            }
            else if ( this.calculateType == CalculateType.Percent )
            {
                this.mainFactor = ChangeMainValueToFactor(m);
            }
        }

        // TODO: Temp interface wait to proto coloumn finished
        public virtual void SetEffectInfo( int resId, string bindPoint )
        {
            effectResId = resId;
            effectBindPoint = bindPoint;
        }

        protected FixFactor ChangeMainValueToFactor( int m )
        {
            long precision = Mathf.RoundToInt( GameConstants.LOGIC_FIXPOINT_PRECISION );
            return new FixFactor( m, precision );
        }

        public void RegisterNoticeGiverDestroyMethod( Action<AttributeEffect> method )
        {
            NoticeGiverDestroyMethod = method;
        }

        public void RegisterNoticeTakerDestroyMethod( Action<AttributeEffect> method )
        {
            NoticeTakerDestroyMethod = method;
        }

        public void RegisterRenderMessageHandler( RenderMessageHandler handler )
        {
            PostRenderMessage = handler;
        }
    }
}
