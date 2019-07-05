/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: DebugAssistant.cs
// description: 
// 
// created time：10/08/2016
//
//----------------------------------------------------------------*/
using UnityEngine;
using System.Collections;

using Data;
using BattleAgent;
using Utils;

namespace Logic
{
    public enum LogicUnitType
    {
        None = 0,
        Soldier = 1,
        Town = 2,
        Tower = 3,
        Projectile = 4,
        Crystal = 5,
        CrystalCar = 7,
        PowerUp = 8,
        Skill = 9,
		institute = 10,
        Demolisher = 11,
        NPC = 12,
        Summon = 13,
        Idol = 14,
        IdolGuard = 15,
        Trap = 16,
    }

//#if DEBUG
    public abstract class LogicUnit : MonoBehaviour, IBattlePoolUnit
    {
        static Transform debugObjectRoot;
        static Transform debugHiddenRoot;
//#else
//  public abstract class LogicUnit
//  {
//#endif

        public LogicUnitType type = LogicUnitType.None;
        public ForceMark mark = ForceMark.NoneForce;
        //public MatchSide side;

        public long id;
        public int hp;
        public int maxHp;
        public int metaId;
        public int modelId;
        public int iconId;
        public int damage;
        public int modelRadius;
        public int killReward; // Ember

        public Fsm currentFsm;
        public FixVector3 position;
        public PhysicsAgent physicsAgent;

        public RenderMessageHandler PostRenderMessage;
        protected DestroyHandler PostDestroy;
        public GetRandomNumberMethod GetRandomNumber;
        protected BattleMessageHandler PostBattleMessage;

        public BattlePool pool { get; set; }

        public static T Create<T>( string name ) where T : LogicUnit
        {
//#if DEBUG
            if( debugObjectRoot == null )
            {
                debugObjectRoot = GameObject.Find( "DebugObjects" ).transform;
                debugHiddenRoot = debugObjectRoot.transform.Find( "HiddenObjects" );
            }

            GameObject go = new GameObject( name );
            if( name == "0" )
            {
                go.transform.parent = debugHiddenRoot;
            }
            else
            {
                go.transform.parent = debugObjectRoot;
            }
            T t = go.AddComponent<T>();

            return t;
//#else
//          T t = new T();
//          return t;
//#endif
        }

#if DEBUG
        public void ChangeRoot()
        {
            transform.parent = debugObjectRoot;
        }
#endif

        public abstract void LogicUpdate( int deltaTime );

        public abstract void Hurt( int hurtValue, AttackPropertyType hurtType, bool isCrit, LogicUnit injurer );

        public abstract void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead ); 

        public abstract void AddCoin( int coin );

        public abstract bool Alive();

        public abstract void Reset();

        public virtual void Recycle()
        {
            pool.Recycle( this );
        }

        public virtual void Clear()
        {
            
        }

        public void RegisterBattleAgentMessageHandler( BattleMessageHandler handler )
        {
            PostBattleMessage = handler;
        }

        public void RegisterRenderMessageHandler( RenderMessageHandler handler )
        {
            PostRenderMessage = handler;
        }

        public void RegisterDestroyHandler( DestroyHandler handler )
        {
            PostDestroy += handler;
        }

        public void RegisterRandomMethod( GetRandomNumberMethod method )
        {
            GetRandomNumber = method;
        }
    }
}
