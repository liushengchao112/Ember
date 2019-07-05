using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Data;

using SummonProto = Data.SummonProto.Summon;

namespace Logic
{
    public enum SummonedUnitState
    {
        None,
        Born,
        Idle,
        Fight,
        Dying,
        Death,
    }

    public class SummonedUnit : LogicUnit
    {
        public FixVector3 direction;
        public SummonedUnitState state;
        public LogicUnit target;

        // proto data
        public int attackDuration;
        public int attackHitTime;
        public long attackRadius;
        public int summonType;
        public int attackInterval;
        public int physicalAttack;
        public int physicalAttackVar;
        public int brithDuration;

        // registe method
        private FindNeutralUnitsMethod findNeutralUnits;
        private FindNeutralUnitMethod findNeutralUnit;
        private FindOpponentBuildingsMethod findOpponentBuildings;
        private FindOpponentCrystalCarMethod findOpponentCrystalCar;
        private FindOpponentCrystalCarsMethod findOpponentCrystalCars;
        private FindOpponentDemolisherMethod findOpponentDemolisher;
        private FindOpponentDemolishersMethod findOpponentDemolishers;
        private FindOpponentSoldierMethod findOpponentSoldier;
        private FindOpponentSoldiersMethod findOpponentSoldiers;
        private WithinCircleAreaPredicate withInSectorArea;

        // Fsm
        private SummonedUnitFsmBorn fsmBorn;
        private SummonedUnitFsmIdle fsmIdle;
        private SummonedUnitFsmFight fsmFight;
        private SummonedUnitFsmDying fsmDying;
        private SummonedUnitFsmDeath fsmDeath;
        private Dictionary<SummonedUnitState, Fsm> fsms;

        public Skill ownerSKill;
        public Soldier ownerSoldier;

        public virtual void Initialize( Skill owner, long id, SummonProto proto, FixVector3 pos, FixVector3 rotation )
        {
            type = LogicUnitType.Summon;

            this.id = id;
            this.position = pos;
            this.direction = rotation;
            this.metaId = proto.ID;
            this.mark = owner.mark;
            this.ownerSKill = owner;
            this.ownerSoldier = owner.owner;
            this.transform.position = position.vector3;

            transform.name = string.Format( "Summon_{0}", id );

            // Init data
            InitializeData( proto );

            // Init fsm
            InitializeFsm();

            DebugUtils.Log( DebugUtils.Type.AI_Summon, string.Format( "Summon has been initialize id = {0}", id ) );
        }

        public virtual void InitializeFsm()
        {
            fsmBorn = new SummonedUnitFsmBorn(this );
            fsmIdle = new SummonedUnitFsmIdle( this );
            fsmFight = new SummonedUnitFsmFight( this );
            fsmDying = new SummonedUnitFsmDying( this );
            fsmDeath = new SummonedUnitFsmDeath( this );

            fsms = new Dictionary<SummonedUnitState, Fsm>();
            fsms.Add( SummonedUnitState.Born, fsmBorn );
            fsms.Add( SummonedUnitState.Idle, fsmIdle );
            fsms.Add( SummonedUnitState.Fight, fsmFight );
            fsms.Add( SummonedUnitState.Dying, fsmDying );
            fsms.Add( SummonedUnitState.Death, fsmDeath );

            currentFsm = fsmBorn;
            state = SummonedUnitState.Born;
        }

        public virtual void InitializeData( SummonProto proto )
        {
            attackDuration = ConvertUtils.ToLogicInt( proto.Attack_duration );
            attackHitTime = ConvertUtils.ToLogicInt( proto.Attack_hit_time );
            attackRadius = ConvertUtils.ToLogicInt( proto.Attack_radius );
            summonType = proto.Summon_type;
            attackInterval = ConvertUtils.ToLogicInt( proto.Attack_interval );
            physicalAttack = (int)proto.Physical_attack;
            physicalAttackVar = (int)( proto.Physical_attack_var );
            brithDuration = ConvertUtils.ToLogicInt( proto.Brith_duration );
        }

        public override void LogicUpdate( int deltaTime )
        {
            currentFsm.Update( deltaTime );
        }

        public virtual void Idle()
        {
            ChangeState( SummonedUnitState.Idle );
        }

        public virtual void Attack( LogicUnit unit )
        {
            target = unit;
            ChangeState( SummonedUnitState.Fight );
        }

        public virtual void Release()
        {
            DebugUtils.Assert( Alive(), string.Format( "Why summoned unit {0} be released twice", id ) );
            DebugUtils.Log( DebugUtils.Type.AI_Summon, string.Format( "Summon begin to release id = {0}", id ) );

            ChangeState( SummonedUnitState.Dying );
        }

        public virtual void Released()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Summon, string.Format( "Summon has been Released id = {0}", id ) );

            ChangeState( SummonedUnitState.Death );
            PostDestroy( this );
        }

        public void ChangeState( SummonedUnitState state )
        {
            currentFsm.OnExit();
            currentFsm = fsms[state];
            this.state = state;
            fsms[state].OnEnter();
            DebugUtils.Log( DebugUtils.Type.AI_Summon, string.Format( "Summon {0} enter {1} state", id, this.state ) );
        }

        public LogicUnit LookingForOpponent()
        {
            LogicUnit unit = null;

            // Find soldiers
            unit = findOpponentSoldier( mark, position, attackRadius );

            if ( unit == null )
            {
                unit = findOpponentCrystalCar( mark, position, attackRadius );
            }

            if ( unit == null )
            {
                unit = findOpponentDemolisher( mark, position, attackRadius );
            }

            if ( unit == null )
            {
                unit = findNeutralUnit( position, attackRadius );
            }

            return unit;
        }

        public List<LogicUnit> FindOpponent()
        {
            List<LogicUnit> opponents = new List<LogicUnit>();

            // Find soldiers
            List<Soldier> soldiers = findOpponentSoldiers( mark, position, (p)=>
            {
                return withInSectorArea( position, attackRadius, p.position );
            } );

            if ( soldiers != null && soldiers.Count > 0 )
            {
                for ( int i = 0; i < soldiers.Count; i++ )
                {
                    opponents.Add( (LogicUnit)soldiers[i] );
                }
            }

            // Find crystal car
            List<CrystalCar> crystalCars = findOpponentCrystalCars( mark, position, ( p ) =>
            {
                return withInSectorArea( position, attackRadius, p.position );
            } );

            if ( crystalCars != null && crystalCars.Count > 0 )
            {
                for ( int i = 0; i < crystalCars.Count; i++ )
                {
                    opponents.Add( (LogicUnit)crystalCars[i] );
                }
            }

            // Find demolisher 
            List<Demolisher> demolishers = findOpponentDemolishers( mark, position, ( p ) =>
            {
                return withInSectorArea( position, attackRadius, p.position );
            } );

            if ( crystalCars != null && crystalCars.Count > 0 )
            {
                for ( int i = 0; i < crystalCars.Count; i++ )
                {
                    opponents.Add( (LogicUnit)crystalCars[i] );
                }
            }

            // Find buildings 
            List<LogicUnit> buildings = findOpponentBuildings( mark, position, attackRadius );
            if ( buildings != null && buildings.Count > 0 )
            {
                opponents.AddRange( buildings );
            }

            // Find neutral units
            List<LogicUnit> neutralUnits = findNeutralUnits( position, ( p ) =>
            {
                return withInSectorArea( position, attackRadius, p.position );
            } );

            if ( neutralUnits != null && neutralUnits.Count > 0 )
            {
                opponents.AddRange( new List<LogicUnit>( neutralUnits ) );
            }

            return opponents;
        }

        public bool WithInSummonAttackRadius( FixVector3 targetPosition, int targetModelRadius )
        {
            long attackRange = (long)attackRadius + (long)modelRadius + (long)targetModelRadius;

            return FixVector3.SqrDistance( targetPosition, position ) <= attackRange;
        }

        public void RegisterFindOpponentSoldier( FindOpponentSoldierMethod method )
        {
            findOpponentSoldier = method;
        }

        public void RegisterFindOpponentSoldiers( FindOpponentSoldiersMethod method )
        {
            findOpponentSoldiers = method;
        }

        public void RegisterFindOpponentBuildings( FindOpponentBuildingsMethod method )
        {
            findOpponentBuildings = method;
        }

        public void RegisterFindOpponentCrystalCar( FindOpponentCrystalCarMethod method )
        {
            findOpponentCrystalCar = method;
        }

        public void RegisterFindOpponentCrystalCars( FindOpponentCrystalCarsMethod method )
        {
            findOpponentCrystalCars = method;
        }

        public void RegisterFindOpponentDemolisher( FindOpponentDemolisherMethod method )
        {
            findOpponentDemolisher = method;
        }

        public void RegisterFindOpponentDemolishers( FindOpponentDemolishersMethod method )
        {
            findOpponentDemolishers = method;
        }

        public void RegisterFindNeutralUnit( FindNeutralUnitMethod method )
        {
            findNeutralUnit = method;
        }

        public void RegisterFindNeutralUnits( FindNeutralUnitsMethod method )
        {
            findNeutralUnits = method;
        }

        public void RegisterWithinFrontSectorAreaPredicate( WithinCircleAreaPredicate method )
        {
            withInSectorArea = method;
        }

        public override void Hurt( int hurtValue, AttackPropertyType hurtType, bool isCrit, LogicUnit injurer )
        {
            
        }

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            
        }

        public override void AddCoin( int coin )
        {
            
        }

        public override bool Alive()
        {
            return state != SummonedUnitState.Dying && state != SummonedUnitState.Death;
        }

        public override void Reset()
        {
            PostDestroy = null;
            target = null;
        }
    }
}
