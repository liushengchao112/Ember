/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: Soldier.cs
// description: 
// 
// created time：09/28/2016
//
//----------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;

using Data;
using BattleAgent;
using Utils;
using Constants;

using SkillProto = Data.UnitSkillsProto.UnitSkill;
using UnitProto = Data.UnitsProto.Unit;
using BattlerUnit = Data.Battler.BattleUnit;

namespace Logic
{
    public class SoldierState
    {
        public const int NONE = 0;
        public const int IDLE = 1;
        public const int IDLE2WALK = 2;
        public const int IDLE2CHASE = 3;
        public const int WALK = 4;
        public const int WALK2WALK = 5;
        public const int WALK2CHASE = 6;
        public const int CHASE2CHASE = 7;
        public const int CHASING = 8;
        public const int WAIT2WALK = 9;
        public const int WAIT2CHASE = 10;
        public const int FIGHT = 11;
        public const int HURT = 12;
        public const int SKILL = 13;
        public const int DYING = 14;
        public const int DEAD = 15;
        public const int WIN = 16;
        public const int LOSE = 17;
        public const int STUN = 18;
        public const int SKILL2WALK = 19;
        public const int SKILL2CHASE = 20;
        public const int BORN = 21;
        //public const int RUN = 16;
    }

    [Flags]
    public enum SoldierAttributeFlags
    {
        None = 0,
        Cloaking = 1,
        BanAttack = 2,
        BanSkill = 4,
        BanMove = 8,
        BanCommand = 16,
    }

    public enum AttackDistanceType
    {
        None = 0,
        Melee = 1,
        Ranged = 2
    }

    public enum AttackPropertyType
    {
        None = 0,
        PhysicalAttack = 1,
        MagicAttack = 2
    }

    public class Soldier : MovableUnit
    {
        // Method handler
        protected FindOwnForceSoldiersMethod FindOwnForceSoldiers;
        protected FindFriendlySoldiersMethod FindFriendlySoldiers;
        protected FindOpponentSoldierMethod FindOpponentSoldier;
        protected FindOpponentSoldiersMethod FindOpponentSoldiers;
        protected FindOpponentBuildingMethod FindOpponentBuilding;
        protected FindOpponentBuildingsMethod FindOpponentBuildings;
        protected FindOpponentCrystalCarMethod FindOpponentCrystalCar;
        protected FindOpponentCrystalCarsMethod FindOpponentCrystalCars;
        protected FindOpponentDemolisherMethod FindOpponentDemolisher;
        protected FindOpponentDemolishersMethod FindOpponentDemolishers;
        protected FindNeutralUnitMethod FindNeutralUnit;
        protected FindNeutralUnitsMethod FindNeutralUnits;
        protected FindPowerUpMethod FindPowerUpMethod;
        protected WithinCircleAreaPredicate WithinCircleAreaPredicate;
        protected WithinFrontRectAreaPredicate WithinFrontRectAreaPredicate;
        protected WithinSectorAreaPredicate WithinSectorAreaPredicate;
        protected GetGroupSoldiersMethod GetGroupSoldiers;

        protected AttributeEffectGenerator GenerateAttributeEffect;
        protected SkillGenerator GenerateSkill;
        protected ProjectileGenerator GenerateProjectile;
        protected SummonGenerator GenerateSummon;
        public CalculateAvoidanceMethod CalculateAvoidance;
        protected TrapGenerator GenerateTrap;

        protected Town town;

        public int state = SoldierState.NONE;
        public int nextState = SoldierState.NONE;
        public UnitType unitType = UnitType.UnitNone;

        public long targetId;
        public LogicUnit target;
        public FixVector3 targetPosition;

        // Timer..
        public int fightTimer;
        public int fightDurationTimer;
        private int healthRecoverTimer = 0;

        public AttackDistanceType attackType;
        public PathType unitMoveMode;
        public int chaseArea;
        public int attackRange;
        public long attackArea;
        public int attackInterval;
        public int physicalAttack;
        public int physicalAttackVar;
        public bool dmgUnmitigated;// damage
        public int strctDmgMult;
        public int projectileId;
        public int attackRadius;
        public int timeExits;
        public int healthRegen;
        public int deployTime;
        public AttackDistanceType standardAttackType;
        public int skill1Id;
        public int skill2Id;
        public int armor;
		public int magicAttack;
        public int magicResist;
        public int criticalChance;
        public int criticalDamage;
        public int targetQuantity;
        public int damageRangeType;
        public int orginAttackInterval;
        public AttackPropertyType attackPropertyType;
        public int spawnCost;

        public int attackDuration;
        public int attackHitTime;
        public int critDuration;
        public int critHitTime;
        public int skill1Duration;
        public int skill1HitTime;
        public int skill2Duration;
        public int skill2HitTime;
        public ProfessionType professionType;

        public bool isVisiable;
        private int healthRecoverInterval = 0;

        public bool owner;
        protected int continuedWalkNum;

        public SoldierAttributeFlags attributeFlags;
        public UnitSkillHandler skillHandler;
        public UnitBehaviorListener stateListener;
        public BuffHandler buffHandler;
        public DebuffHandler debuffHandler;

        public SoldierFsmIdle fsmIdle;
        public SoldierFsmWalk fsmWalk;
        public SoldierFsmChase fsmChase;
        public SoldierFsmFight fsmFight;
        public SoldierFsmDying fsmDying;
        public SoldierFsmDead fsmDead;
        public SoldierFsmSkill fsmSkill;
        public SoldierFsmBorn fsmBorn;
        public SoldierFsmPlaceholder fsmPlaceholder;
        
        // Area effect
        private bool stayGrass = false;
        private AttributeEffect grassBuff;
        private bool stayShallowWater = false;
        private AttributeEffect shallowWaterDebuff;

        public virtual void Initialize( Town town, UnitProto proto, FixVector3 pos, BattlerUnit unitInfo, bool isNpc )
        {
            this.town = town;
            this.unitType = (UnitType)proto.UnitType;
            type = LogicUnitType.Soldier;
            mark = town.mark;
            direction = FixVector3.forward;
            position = pos;

            InitializeData( proto, unitInfo );

            stateListener = new UnitBehaviorListener( this );

            InitializeUnitSkill();

            state = InitializeState( isNpc );

            buffHandler = new BuffHandler();
            debuffHandler = new DebuffHandler();

            attackInterval = orginAttackInterval;
            hp = maxHp;
            attackArea = modelRadius + attackRange;

            if ( pathAgent != null )
            {
                pathAgent.SetPathType( unitMoveMode );
                pathAgent.SetPosition( position );
            }

            //currentWaypoint = 0;

            InitializeOwner( isNpc );

            InitializePhysicsAgent();
            gameObject.SetActive( true );

            attributeFlags = SoldierAttributeFlags.None;
        }

        protected virtual void InitializeData( UnitProto proto, BattlerUnit unitInfo )
        {
            metaId = proto.ID;
            modelId = proto.Model;
            modelRadius = ConvertUtils.ToLogicInt( proto.ModelRadius );
            iconId = proto.Icon;
            spawnCost = proto.DeploymentCost;
            maxHp = proto.Health;
            standardAttackType = (AttackDistanceType)proto.StandardAttack;
            armor = proto.Armor;
			magicAttack = proto.MagicAttack;
            magicResist = proto.MagicResist;
            speedFactor = ConvertUtils.ToLogicInt( proto.MoveSpeed );
            unitMoveMode = (PathType)proto.MoveMode;
            chaseArea = ConvertUtils.ToLogicInt( proto.TargetDetectRange );
            attackRange = ConvertUtils.ToLogicInt( proto.AttackRange );
            attackDuration = ConvertUtils.ToLogicInt( proto.attack_duration );
            orginAttackInterval = ConvertUtils.ToLogicInt( proto.AttackInterval );
            physicalAttack = (int)proto.PhysicalAttack;
            physicalAttackVar = (int)proto.MeleeDmgVar;
            dmgUnmitigated = proto.DmgUnmitigated;
            strctDmgMult = ConvertUtils.ToLogicInt( proto.StrctDmgMult );
            skill1Id = proto.SkillID;
            skill2Id = proto.SkillID2;
            attackRadius = ConvertUtils.ToLogicInt( proto.AttackRadius );
            timeExits = proto.TimeExits;
            healthRegen = proto.HealthRegen;
            deployTime = ConvertUtils.ToLogicInt( proto.DeployTime );
            projectileId = proto.Projectile_Id;

            professionType = (ProfessionType)proto.ProfessionType;
            killReward = proto.killReward;

            criticalChance = ConvertUtils.ToLogicInt( proto.CriticalChance );
            criticalDamage = ConvertUtils.ToLogicInt( proto.CriticalDamage );
            targetQuantity = proto.TargetQuantity;
            damageRangeType = proto.DamageRange;

            attackDuration = ConvertUtils.ToLogicInt( proto.attack_duration);
            attackHitTime = ConvertUtils.ToLogicInt( proto.attack_hit_time );
            critDuration = ConvertUtils.ToLogicInt( proto.crit_duration );
            critHitTime = ConvertUtils.ToLogicInt( proto.crit_hit_time );
            skill1Duration = ConvertUtils.ToLogicInt( proto.skill1_duration );
            skill1HitTime = ConvertUtils.ToLogicInt( proto.skill1_hit_time );
            skill2Duration = ConvertUtils.ToLogicInt( proto.skill2_duration );
            skill2HitTime = ConvertUtils.ToLogicInt( proto.skill2_hit_time );

            attackPropertyType = AttackPropertyType.PhysicalAttack;

            healthRecoverInterval = GameConstants.HP_RECOVERY_INTERVAL_MILLISECOND;

            SetUnitProperty( unitInfo );
        }

        public virtual void InitializePathAgent()
        {
            pathAgent = new NavAgent( this );
        }

        public virtual void InitializePhysicsAgent()
        {
            if ( owner && physicsAgent == null )
            {
                physicsAgent = new PhysicsAgent( PhysicsAgentType.Unity, this );
                physicsAgent.RegisterCollisionStartMethod( OnCollisionStart );
                physicsAgent.RegisterCollisionEndMethod( OnCollisionEnd );
            }
        }

        protected virtual int InitializeState( bool isNpc )
        {
            if ( isNpc )
            {
                fsmIdle = new SimSoldierFsmIdle( this );
                fsmWalk = new SimSoldierFsmWalk( this );
                fsmChase = new SimSoldierFsmChase( this );
                fsmFight = new SimSoldierFsmFight( this );
                fsmDying = new SimSoldierFsmDying( this );
                fsmDead = new SimSoldierFsmDead( this );
                fsmSkill = new SimSoldierFsmSkill( this );
                fsmBorn = new SimSoldierFsmBorn( this );
                fsmPlaceholder = new SimSoldierFsmPlaceholder( this );
            }
            else
            {
                fsmIdle = new SoldierFsmIdle( this );
                fsmWalk = new SoldierFsmWalk( this );
                fsmChase = new SoldierFsmChase( this );
                fsmFight = new SoldierFsmFight( this );
                fsmDying = new SoldierFsmDying( this );
                fsmDead = new SoldierFsmDead( this );
                fsmSkill = new SoldierFsmSkill( this );
                fsmBorn = new SoldierFsmBorn( this );
                fsmPlaceholder = new SoldierFsmPlaceholder( this );
            }

            currentFsm = fsmBorn;
            currentFsm.OnEnter();
            return SoldierState.BORN;
        }

        protected virtual void InitializeOwner( bool setAsNpc )
        {
            if ( setAsNpc )
            {
                owner = true;
            }
            else
            {
                if ( mark == DataManager.GetInstance().GetForceMark() )
                {
                    owner = true;
                }
                else
                {
                    owner = false;
                }
            }
        }

        public override void LogicUpdate( int deltaTime )
        {
            //DebugUtils.LogWarning( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0}'s state is {1}, position is ({2}, {3}, {4}), transform.position is ({5}, {6}, {7}).", id, state, position.x, position.y, position.z, transform.position.x, transform.position.y, transform.position.z ) );

            //Important: the soldier doesn't move or find target when in middle state for now, so these states won't be disturbed by internal logic, but changing target command from player can.
            currentFsm.Update( deltaTime );

            buffHandler.LogicUpdate( deltaTime );
            debuffHandler.LogicUpdate( deltaTime );
            skillHandler.LogicUpdate( deltaTime );
            HealthRecover( deltaTime );
            SyncPosition( deltaTime );
        }

        public void ChangeState( int state, Fsm targetFsm )
        {
            currentFsm.OnExit();
            this.state = state;
            currentFsm = targetFsm;
            currentFsm.OnEnter();
        }

        public void SyncPosition( int deltaTime )
        {
			if( Alive() )
			{
                bool RotatingImmediately = false;
                float deltaDistance = 0;
                if ( state == SoldierState.FIGHT || state == SoldierState.SKILL )
                {
                    RotatingImmediately = true;
                }

                // Draw path point in walk state
                if ( InWalkState() )
                {
                    deltaDistance = ( speed.vector3 * FixVector3.PrecisionFactor * deltaTime * FixVector3.PrecisionFactor ).magnitude;
                }

                RenderMessage rm = new RenderMessage();
				rm.type = RenderMessage.Type.SyncPosition;
				rm.ownerId = id;
                rm.position = position.vector3;
                rm.direction = direction.vector3;
                rm.arguments.Add( "RotatingImmediately", RotatingImmediately );
                rm.arguments.Add( "type", (int)type );
                rm.arguments.Add( "move", deltaDistance );

                PostRenderMessage( rm );
			}
        }

        public virtual void Fight( bool isCrit )
        {
            // calculate value;
            int baseDamage = GetDamageValue();
            int critAddition = 0;

            if ( isCrit )
            {
                critAddition = Formula.GetCriticalDamage( damage, criticalDamage + buffHandler.criticalDamageValue - debuffHandler.criticalDamageValue );
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "{0} unit {1}'s Crit fight ", mark, id ) );
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "{0} unit {1}'s normal fight ", mark, id ) );
            }

            DebugUtils.Log( DebugUtils.Type.Soldier_Properties, string.Format( "{0} unit {1}'s base damage {2} critAddition {3} ", mark, id, baseDamage, critAddition ) );

            damage = baseDamage + critAddition;

            // apply attack
            if ( standardAttackType == AttackDistanceType.Melee )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, "the " + unitType + " soldier " + id + " begins to attack target " + target.id + " damage " + damage );
                target.Hurt( damage, attackPropertyType, isCrit, this );

                RenderMessage rm = new RenderMessage();
                rm.type = RenderMessage.Type.SoldierHitTarget;
                rm.ownerId = id;
                PostRenderMessage( rm );
            }
            else if ( standardAttackType == AttackDistanceType.Ranged )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, "the " + unitType + " soldier " + id + " begins to fire to target " + target.id );

                Projectile projectile = GenerateProjectile( this, projectileId, position, target );
                projectile.RegisterRenderMessageHandler( PostRenderMessage );
                projectile.RegisterDestroyHandler( PostDestroy );
                projectile.RegisterRandomMethod( GetRandomNumber );
                projectile.RegisterFindOpponentSoldiers( FindOpponentSoldiers );
                projectile.RegisterFindOpponentCrystalCars( FindOpponentCrystalCars );
                projectile.RegisterFindOpponentBuildings( FindOpponentBuildings );
                projectile.RegisterFindOpponentDemolishers( FindOpponentDemolishers );
                projectile.RegisterGenerateAttributeEffect( GenerateAttributeEffect );
                projectile.RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate );
                projectile.RegisterWithinFrontRectangleAreaPredicate( WithinFrontRectAreaPredicate );
                projectile.SetHurtInfo( damage, isCrit );

                RenderMessage rm = new RenderMessage();
                rm.type = RenderMessage.Type.SpawnProjectile;
                rm.ownerId = projectile.id;
                rm.position = projectile.position.vector3;
                rm.direction = projectile.speed.vector3;
                rm.arguments.Add( "mark", projectile.mark );
                rm.arguments.Add( "metaId", projectile.metaId );
                rm.arguments.Add( "holderType", (int)projectile.owner.type );
                rm.arguments.Add( "holderId", projectile.owner.id );
                PostRenderMessage( rm );
            }
        }

        private void Walk( FixVector3 d, int pathMask = 1 )
        {
            if ( GetFlag( SoldierAttributeFlags.BanMove ) )
            {
                return;
            }

            destination = d;

            if ( state == SoldierState.WALK )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " enters walk2walk state." );
                ChangeState( SoldierState.WALK2WALK, fsmPlaceholder );
            }
            else // use IDLE2WALK for any other states( IDLE, IDLE2WALK, WAIT2WALK, WALK2WALK, SKILL2WALK, SKILL2CHASE )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " enters idle2walk state." );
                ChangeState( SoldierState.IDLE2WALK, fsmPlaceholder );
            }

            if ( owner )
            {
                pathAgent.FindPath( position, destination, pathMask, OnWalkPathComplete );
            }
        }

        protected override void OnWalkPathComplete( List<FixVector3> fixPath )
        {
            base.OnWalkPathComplete( fixPath );

            List<Vector3> path = new List<Vector3>();
            for ( int i = 0; i < fixPath.Count; i++ )
            {
                path.Add( fixPath[i].vector3 );
            }

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SyncUnitPathPoint;
            rm.ownerId = id;
            rm.arguments.Add( "type", 1 );
            rm.arguments.Add( "mark", mark );
            rm.arguments.Add( "path", path );
            PostRenderMessage( rm );
        }

        public virtual void Idle()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " enters idle state." );

            ChangeState( SoldierState.IDLE, fsmIdle );

            RenderMessage message = new RenderMessage();
            message.type = RenderMessage.Type.SoldierIdle;
            message.ownerId = id;
            message.direction = direction.vector3;
            PostRenderMessage( message );
        }

        public virtual void Attack( LogicUnit unit )
        {
            if ( state == SoldierState.STUN )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " still in stun state. can't enter attack state" );
                return;
            }

            DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " enters attack state." );

            ChangeState( SoldierState.FIGHT, fsmFight );
            fightTimer = 0;
            targetId = unit.id;
            target = unit;
        }

        public virtual void Chase( LogicUnit unit )
        {
            if ( state == SoldierState.STUN || GetFlag( SoldierAttributeFlags.BanMove ) )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " still in can't move state. can't enter Chase state" );
                return;
            }

            targetId = unit.id;
            target = unit;
            targetPosition = target.position;

            if ( state == SoldierState.WALK )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " enters walk2chase state, target id = " + unit.id );
                ChangeState( SoldierState.WALK2CHASE, fsmPlaceholder );
            }
            else if ( state == SoldierState.CHASING )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " enters chase2chase state, target id = " + unit.id );
                ChangeState( SoldierState.CHASE2CHASE, fsmPlaceholder );
            }
            else // use IDLE2CHASE for any other states( IDLE, IDLE2WALK, WAIT2WALK, WALK2WALK )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " enters idle2chase state, target id = " + unit.id );
                ChangeState( SoldierState.IDLE2CHASE, fsmPlaceholder );
            }

            FindChasePath( targetPosition );
        }

        public void FindChasePath( FixVector3 targetPosition )
        {
            if ( owner )
            {
                pathAgent.FindPath( position, targetPosition, NavMeshAreaType.WALKABLE, OnChasePathComplete );
            }
        }

        public virtual void Stun()
        {
            if ( Alive() )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " enters stun state." );

                ChangeState( SoldierState.STUN, fsmPlaceholder );

                RenderMessage message = new RenderMessage();
                message.type = RenderMessage.Type.SoldierStuned;
                message.ownerId = id;
                message.arguments.Add( "value", true );
                PostRenderMessage( message );
            }
        }

        public virtual void StunEnd()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " exit stun state." );

            if ( Alive() )
            {
                RenderMessage message = new RenderMessage();
                message.type = RenderMessage.Type.SoldierStuned;
                message.ownerId = id;
                message.arguments.Add( "value", false );
                PostRenderMessage( message );

                Idle();
            }
        }

        public virtual void Dying()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " enters dying state." );

            buffHandler.Destory();
            debuffHandler.Destory();

            ChangeState( SoldierState.DYING, fsmDying );

            RenderMessage message = new RenderMessage();
            message.type = RenderMessage.Type.SoldierDeath;
            message.ownerId = id;
            message.direction = direction.vector3;
            message.arguments.Add( "mark", mark );
            PostRenderMessage( message );
        }

        public virtual void Dead()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " enters dead state." );

            ChangeState( SoldierState.DEAD, fsmDead );

            PostDestroy( this );
        }

        public override void Hurt( int hurtValue, AttackPropertyType hurtType, bool isCrit, LogicUnit injurer )
        {
            if ( Alive() )
            {
                int trueHurtValue = GetActualHurtValue( hurtType , hurtValue );
                hp -= trueHurtValue;

                if ( hp <= 0 )
                {
                    injurer.OnKillEnemy( killReward, injurer, this );
                    Dying();
                }
                else
                {
                    stateListener.PostHurtEvent();

                    if ( isCrit )
                    {
                        RenderMessage message = new RenderMessage();
                        message.type = RenderMessage.Type.CritHurtUnit;
                        message.ownerId = id;
                        message.arguments.Add( "unitType", (int)type );
                        message.arguments.Add( "value", trueHurtValue );
                        PostRenderMessage( message );
                    }
                    else
                    {
                        RenderMessage message = new RenderMessage();
                        message.type = RenderMessage.Type.SoldierHurt;
                        message.ownerId = id;
                        message.arguments.Add( "value", trueHurtValue );
                        message.arguments.Add( "mark", mark );
                        PostRenderMessage( message );
                    }

                    if ( state == SoldierState.IDLE)
                    {
                        LogicUnit chaseTarget;
                        if ( target == null )
                        {
                            chaseTarget = injurer;
                        }
                        else
                        {
                            chaseTarget = target;
                        }

                        Chase( chaseTarget );
                    }
                }
            }
        }

        public virtual int Heal( int healValue )
        {
            if ( Alive() )
            {
                int value = healValue + hp;
                int maxHp = GetMaxHp();
                if ( value > maxHp )
                {
                    hp = maxHp;
                }
                else
                {
                    hp += healValue;
                }

                RenderMessage message = new RenderMessage();
                message.type = RenderMessage.Type.SoldierHeal;
                message.ownerId = id;
                message.arguments.Add( "value", healValue );
                message.arguments.Add( "mark", mark );
                PostRenderMessage( message );
            }
            return 0;
        }

        public void Rooted()
        {

        }

        public virtual Skill ReleaseSkill( SkillProto skillProto, int index )
        {
            DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} release skill {1} skill meta id = {2}", id, skillProto.Name, skillProto.ID ) );

            Skill skill = null;

            skill = GenerateSkill( this, skillProto, index );

            skill.RegisterRenderMessageHandler( PostRenderMessage );
            skill.RegisterBattleAgentMessageHandler( PostBattleMessage );
            skill.RegisterGenerateAttributeEffectMethod( GenerateAttributeEffect );

            skill.RegisterProjectileGenerator( GenerateProjectile );
            skill.RegisterSummonGenerator( GenerateSummon );
            skill.RegisterDestroyHandler( PostDestroy );
            skill.RegisterRandomMethod( GetRandomNumber );
            skill.RegisterTrapGenerator( GenerateTrap );

            skill.RegisterFindOwnForceSoldiers( FindOwnForceSoldiers );
            skill.RegisterFindFriendlySoldiers( FindFriendlySoldiers );
            skill.RegisterFindOpponentSoldier( FindOpponentSoldier );
            skill.RegisterFindOpponentBuilding( FindOpponentBuilding );
            skill.RegisterFindOpponentBuildings( FindOpponentBuildings );
            skill.RegisterFindOpponentSoldiers( FindOpponentSoldiers );
            skill.RegisterFindOpponentCrystalCar( FindOpponentCrystalCar );
            skill.RegisterFindOpponentCrystalCars( FindOpponentCrystalCars );
            skill.RegisterFindOpponentDemolisher( FindOpponentDemolisher );
            skill.RegisterFindOpponentDemolishers( FindOpponentDemolishers );
            skill.RegisterFindNeutralUnit( FindNeutralUnit );
            skill.RegisterFindNeutralUnits( FindNeutralUnits );

            skill.RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate );
            skill.RegisterWithinFrontRectangleAreaPredicate( WithinFrontRectAreaPredicate );
            skill.RegisterWithinFrontSectorAreaPredicate( WithinSectorAreaPredicate );

            skill.handler = skillHandler;

            skill.Fire();

            return skill;
        }

        public override bool Alive()
        {
            return state != SoldierState.DYING && state != SoldierState.DEAD;
        }

        public Boolean InStunState()
        {
            return state == SoldierState.STUN;
        }

        public override Boolean InWalkState()
        {
            return state == SoldierState.WALK;
        }

        public override Boolean InChaseState()
        {
            return state == SoldierState.CHASING;
        }

        public override Boolean InSkillState()
        {
            return state == SoldierState.SKILL || state == SoldierState.SKILL2CHASE || state == SoldierState.SKILL2WALK;
        }

        public override bool Moveable()
        {
            return state != SoldierState.STUN && state != SoldierState.WIN && state != SoldierState.LOSE && !GetFlag( SoldierAttributeFlags.BanMove );
        }

        public override Boolean InMovePlaceHolder()
        {
            return state == SoldierState.WALK2WALK || state == SoldierState.WALK2CHASE || state == SoldierState.CHASE2CHASE || 
                   state == SoldierState.SKILL2CHASE || state == SoldierState.SKILL2WALK;
        }

        public void SetFlag( SoldierAttributeFlags flag )
        {
            DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} add {1} flag", id, flag ) );

            attributeFlags = attributeFlags | flag;
        }

        public bool GetFlag( SoldierAttributeFlags flag )
        {
            return ( attributeFlags & flag ) == flag;
        }

        public void RemoveFlag( SoldierAttributeFlags flag )
        {
            if ( ( attributeFlags & flag ) == flag )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} remove {1} flag", id, flag ) );

                attributeFlags = attributeFlags ^ flag;
            }
        }

        protected override void FinishedWalk()
        {
            if ( continuedWalkNum > 0 )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} enter {1} state", id, SoldierState.WAIT2WALK ) );

                ChangeState( SoldierState.WAIT2WALK, fsmPlaceholder );
            }
            else
            {
                Idle();
            }
        }

        protected override void FinishedChase()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} finished chase", id ) );

            Idle();
        }

        protected override void FinishSkillMove()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} finished skill move", id ) );

            skillHandler.FinishSkillMove();
        }

        public override void MovingPlaceHolder()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} enter {1} state", id, SoldierState.WAIT2WALK ) );

            // there is no CHASE2WALK for now.
            ChangeState( SoldierState.WAIT2WALK, fsmPlaceholder );
        }

        public override void AddCoin( int coin )
        {
            DebugUtils.Log( DebugUtils.Type.AI_Soldier, "the soldier " + id + " gain " + coin + " ember by attack crystal" );

            town.AddCoin( coin );
        }

        protected override void WaitWalkHandler( Sync sync, List<Position> posList )
        {
            base.WaitWalkHandler( sync, posList );

            if ( sync.syncState == SyncState.Walk )
            {
                //if( state == SoldierState.IDLE2WALK || state == SoldierState.WAIT2WALK || state == SoldierState.WALK2WALK )
                {
                    DebugUtils.Assert( posList.Count > 0, "soldier : the new waypoints' length should be larger than 0 for a new walk!" );

                    DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} enters walk state, velocity = ({1}, {2}, {3}).", id, speed.x, speed.y, speed.z ) );
                    ChangeState( SoldierState.WALK, fsmWalk );

                    RenderMessage message = new RenderMessage();
                    message.type = RenderMessage.Type.SoldierWalk;
                    message.ownerId = id;
                    message.direction = speed.vector3;
                    PostRenderMessage( message );
                }
                //else
                //{
                //    DebugUtils.LogError( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} receives WALK sync message, but in state {1}!", id, state ) );
                //}
            }
            else if( sync.syncState == SyncState.ContinuedWalk )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} receives CONTINUED WALK sync message in state.", id, state ) );

                if( sync.continuedWalkNum == 1 )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} enters walk state, velocity = ({1}, {2}, {3}).", id, speed.x, speed.y, speed.z ) );
                    ChangeState( SoldierState.WALK, fsmWalk );

                    RenderMessage message = new RenderMessage();
                    message.type = RenderMessage.Type.SoldierWalk;
                    message.ownerId = id;
                    message.direction = speed.vector3;
                    PostRenderMessage( message );
                }
            }
        }

        protected override void WaitChaseHandler()
        {
            base.WaitChaseHandler();

            // target sometime will be null,  wait to double check
            // DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} enters chasing state, target = {1}, velocity = ({2}, {3}, {4}).", id, target.id, speed.x, speed.y, speed.z ) );
            if ( InSkillState() )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} enters chase state, velocity = ({1}, {2}, {3}).", id, speed.x, speed.y, speed.z ) );

                skillHandler.WaitChaseHandlerInSkill();
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} enters chase state, velocity = ({1}, {2}, {3}).", id, speed.x, speed.y, speed.z ) );

                ChangeState( SoldierState.CHASING, fsmChase );
            }

            //DebugUtils.LogWarning( DebugUtils.Type.AI_Soldier, "soldier " + id + "begin to chase, he's position = (" + position.x + ", " + position.y + ", " + position.z + "), next waypoint = (" + waypoints[currentWaypoint].x + ", " + waypoints[currentWaypoint].y + ", " + waypoints[currentWaypoint].z + ")" );

            RenderMessage message = new RenderMessage();
            message.type = RenderMessage.Type.SoldierWalk;
            message.ownerId = id;
            message.direction = speed.vector3;
            PostRenderMessage( message );
        }

        private void OnTargetClosed()
        {
            //List<Soldier> soldiers = GetGroupSoldiers();
            //AstarPath.active.astarData.gridGraph.GetNearest( Vector3.zero );
            //AstarPath.active.UpdateGraphs( new Bounds() );
            //path.CanTraverse( new GraphNode( AstarPath.active ) );

            Position p = waypoints[currentWaypoint];
            FixVector3 v = new FixVector3( p.x - position.x, p.y - position.y, p.z - position.z );
            direction = v.normalized;
            //speed = direction * ( ( speedFactor * ( 1 + buffHandler.speedRateValue - debuffHandler.speedRateValue ) ) + buffHandler.speedValue - debuffHandler.speedValue );
        }

        //TODO:if change target is a movable unit , use this function
        public void ChangeForceTarget( LogicUnit unit )
        {
            if ( Alive() && Moveable() && !GetFlag( SoldierAttributeFlags.BanCommand ) )
            {
                targetId = unit.id;
                target = unit;
                targetPosition = target.position;

                if ( !stateListener.PostAttackOrderEvent() )
                {
                    Chase( unit );
                }
            }
        }

        public void ChangeForceTarget( FixVector3 d )
        {
            if ( Alive() && Moveable() && !GetFlag( SoldierAttributeFlags.BanCommand ) )
            {
                Walk( d );
            }
        }

        public void ChangeForceTarget( FixVector3 d, int pathMask )
        {
            if ( Alive() && Moveable() && !GetFlag( SoldierAttributeFlags.BanCommand ) )
            {
                Walk( d, pathMask );
            }
        }

        public void ChangeForceTarget( List<FixVector3> p, Boolean isLast )
        {
            if ( Alive() && Moveable() && !GetFlag( SoldierAttributeFlags.BanCommand ) )
            {
                DebugUtils.Assert( owner, String.Format( "This soldier {0} shouldn't be controlled by you!", id ) );

                continuedWalkNum++;

                SyncC2S sync = new SyncC2S();
                sync.uintId = id;
                sync.timestamp = DataManager.GetInstance().GetFrame();
                sync.syncState = SyncState.ContinuedWalk;

                GetPositionsFromPath( p, sync.positions );

                sync.continuedWalkNum = continuedWalkNum;

                PostBattleMessage( MsgCode.SyncMessage, sync );

                if( isLast )
                {
                    continuedWalkNum = 0;
                }

                /*
                if( !InWalkState() )
                {
                    state = SoldierState.IDLE2WALK;
                }
                */
            }
        }

        public void MapAreaCollisionStart( int t )
        {
            CollisionType collisionType = (CollisionType)t;
            DebugUtils.LogWarning( DebugUtils.Type.Physics, string.Format( "unit {0} collision start, collisionType: {1} ", id, collisionType ) );

            if ( collisionType == CollisionType.Grass )
            {
                if ( !stayGrass )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "unit {0} attach grass hiding buff", id ) );

                    AttributeEffect buff = GenerateAttributeEffect( GameConstants.GRASS_BUFF_ID );
                    buff.Attach( this, this );
                    grassBuff = buff;

                    stayGrass = true;
                }
            }
            else if ( collisionType == CollisionType.ShallowWater )
            {
                if ( !stayShallowWater )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "unit {0} attach ShallowWater low speed debuff", id ) );

                    AttributeEffect debuff = GenerateAttributeEffect( GameConstants.SHALLOWWATER_DEBUFF_ID );
                    debuff.Attach( this, this );
                    shallowWaterDebuff = debuff;

                    stayShallowWater = true;
                }
            }
        }

        public void MapAreaCollisionEnd( int t )
        {
            CollisionType collisionType = (CollisionType)t;
            DebugUtils.LogWarning( DebugUtils.Type.Physics, string.Format( "unit {0} collision end, collisionType: {1} ", id, collisionType ) );

            if ( collisionType == CollisionType.Grass )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "unit {0} exit grass", id ) );

                if ( grassBuff != null )
                {
                    grassBuff.Detach();
                }

                stayGrass = false;
            }
            else if ( collisionType == CollisionType.ShallowWater )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "unit {0} exit ShallowWater", id ) );

                if ( shallowWaterDebuff != null )
                {
                    shallowWaterDebuff.Detach();
                }

                stayShallowWater = false;
            }
        }

        public LogicUnit FindOpponentUnit()
        {
            LogicUnit unit = FindOpponentSoldier( mark, position, chaseArea );

            if ( unit == null )
            {
                unit = FindOpponentCrystalCar( mark, position, chaseArea );
            }

            if ( unit == null )
            {
                unit = FindOpponentDemolisher( mark, position, chaseArea );
            }

            if ( unit == null )
            {
                unit = FindOpponentBuilding( mark, position, chaseArea );
            }

            //if ( unit == null )
            //{
            //    unit = FindNeutralUnit( transform.position, chaseArea );
            //}

            return unit;
        }

        public void FindOpponent()
        {
            LogicUnit unit = FindOpponentUnit();

            if ( unit != null )
            {
				long distance = FixVector3.SqrDistance( unit.position, position);
                long attackDistance = GetAttackArea();

                DebugUtils.LogWarning( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} finds the target {1} of type {2}, distance = {3}, chaseArea = {4}, attackDistance = {5}.", id, unit.id, unit.type, distance, chaseArea, attackDistance ) );

                if( distance < attackDistance )
                {
                    Attack( unit );
                }
                else
                {
                    if ( !GetFlag( SoldierAttributeFlags.BanMove ) )
                    {
                        Chase( unit );
                    }
                }
            }
        }

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            town.OnKillEnemy( emberReward, this, dead );

            stateListener.PostKillEnemyEvent();
        }

        public override void Reset()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + id + " is reset." );

            target = null;
            targetId = 0;

            owner = false;
            skillHandler.Reset();

            hp = 0;
            maxHp = 0;

            damage = 0;
			speedFactor = 0;
            speed = FixVector3.zero;
            direction = FixVector3.zero;
            position = new FixVector3( float.MaxValue, float.MaxValue, float.MaxValue );
            transform.position = position.vector3;
            destination = position;
            transform.rotation = Quaternion.identity;

            if( waypoints != null )
            {
                waypoints.Clear();
                waypoints = null;
            }

            grassBuff = null;

            stayGrass = false;

            shallowWaterDebuff = null;

            stayShallowWater = false;

            buffHandler.Destory();
            debuffHandler.Destory();

            id = -1;
            unitType = UnitType.UnitNone;
            ChangeState( SoldierState.NONE, fsmPlaceholder );
            town = null;
            mark = ForceMark.NoneForce;
            PostRenderMessage = null;
            PostDestroy = null;
            GenerateProjectile = null;
            GenerateAttributeEffect = null;
            GenerateSkill = null;
            GetRandomNumber = null;
            FindOpponentSoldier = null;
            FindOpponentSoldiers = null;
            FindOpponentBuilding = null;
            FindOpponentBuildings = null;
            FindFriendlySoldiers = null;
            FindNeutralUnit = null;
            FindOpponentBuilding = null;
            FindOpponentBuildings = null;
            FindOpponentCrystalCar = null;
            FindOpponentDemolisher = null;
            FindOpponentDemolishers = null;
            FindOwnForceSoldiers = null;
            GetGroupSoldiers = null;
            FindPowerUpMethod = null;

            stateListener.Release();
            gameObject.SetActive( false );
        }

        private void OnCollisionStart( CollisionType collisionType )
        {
            DebugUtils.LogWarning( DebugUtils.Type.Physics, string.Format( "unit {0} collision start, collisionType: {1} ", id, collisionType ) );

            if ( collisionType == CollisionType.Grass )
            {
                if ( !stayGrass )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "unit {0} enter grass area", id ) );

                    UpdateC2S message = new UpdateC2S();
                    message.timestamp = DataManager.GetInstance().GetFrame();

                    Operation op = new Operation();
                    op.opType = OperationType.MapAreaCollision;
                    op.playerId = town.id;
                    op.unitId = id;
                    op.collisionState = 1; // 1 is Start
                    op.collisionType = 1; // 1 is Grass
                    message.operation = op;

                    PostBattleMessage( MsgCode.UpdateMessage, message );
                }
            }
            else if ( collisionType == CollisionType.ShallowWater )
            {
                if ( !stayShallowWater )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "unit {0} enter ShallowWater", id ) );

                    UpdateC2S message = new UpdateC2S();
                    message.timestamp = DataManager.GetInstance().GetFrame();

                    Operation op = new Operation();
                    op.opType = OperationType.MapAreaCollision;
                    op.playerId = town.id;
                    op.unitId = id;
                    op.collisionState = 1; // 1 is Start
                    op.collisionType = 2; // 1 is Grass
                    message.operation = op;

                    PostBattleMessage( MsgCode.UpdateMessage, message );
                }
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Physics, string.Format( "Can't handle this CollisionType {0}", collisionType ) );
            }
        }

        private void OnCollisionEnd( CollisionType collisionType )
        {
            DebugUtils.LogWarning( DebugUtils.Type.Physics, string.Format( "unit {0} collision end, collisionType: {1} ", id, collisionType ) );

            if ( collisionType == CollisionType.Grass )
            {
                UpdateC2S message = new UpdateC2S();
                message.timestamp = DataManager.GetInstance().GetFrame();

                Operation op = new Operation();
                op.opType = OperationType.MapAreaCollision;
                op.playerId = town.id;
                op.unitId = id;
                op.collisionState = 2; // 2 is collisionEnd
                op.collisionType = 1; // 1 is Grass
                message.operation = op;

                PostBattleMessage( MsgCode.UpdateMessage, message );
            }
            else if ( collisionType == CollisionType.ShallowWater )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "unit {0} exit ShallowWater", id ) );

                UpdateC2S message = new UpdateC2S();
                message.timestamp = DataManager.GetInstance().GetFrame();

                Operation op = new Operation();
                op.opType = OperationType.MapAreaCollision;
                op.playerId = town.id;
                op.unitId = id;
                op.collisionState = 2; // 2 is collisionEnd
                op.collisionType = 2; // 2 is shallowWater
                message.operation = op;

                PostBattleMessage( MsgCode.UpdateMessage, message );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Physics, string.Format( "Can't handle this CollisionType {0}", collisionType ) );
            }
        }

        public void SetVisible( bool v )
        {
            if ( v )
            {
                RemoveFlag( SoldierAttributeFlags.Cloaking );
            }
            else
            {
                SetFlag( SoldierAttributeFlags.Cloaking );
            }

            RenderMessage rm = new RenderMessage();
            rm.ownerId = id;
            rm.position = position.vector3;
            rm.arguments.Add( "mark", mark );
            rm.arguments.Add( "value", v );
            rm.type = RenderMessage.Type.SoldierVisibleChange;
            PostRenderMessage( rm );
        }

        public void SetMoveableState( bool banMove )
        {
            if ( banMove )
            {
                SetFlag( SoldierAttributeFlags.BanMove );

                Idle();
            }
            else
            {
                RemoveFlag( SoldierAttributeFlags.BanMove );
            }
        }

		#region Soldier born complete show render

		public void BornComplete()
		{
			RenderMessage rm = new RenderMessage();
			rm.ownerId = id;
			rm.type = RenderMessage.Type.SoldierBornComplete;
			PostRenderMessage( rm );
		}
			
		#endregion

        // unit be sneer
        public void ForceAttack()
        {

        }

        public void PickUpPowerUp( PowerUp powerup )
        {
            if ( powerup != null )
            {
                RenderMessage message = new RenderMessage();
                message.type = RenderMessage.Type.SoldierPickUpPowerUp;
                message.ownerId = id;
                message.arguments.Add( "powerUpType", powerup.id );
                message.arguments.Add( "activeEffectPath", powerup.proto.ActiveEffect );
                message.arguments.Add( "persistantEffectPath", powerup.proto.PersistantEffect );
                message.arguments.Add( "deactivateEffectPath", powerup.proto.DeactivateEffect );
                message.arguments.Add( "LifeTime", powerup.lifeTime );

                PostRenderMessage( message );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( "Soldier : Can't find the powerUp proto data, type = {0}", type ) );
            }
        }

        protected void HealthRecover( int deltaTime )
        {
            if ( Alive() )
            {
                int maxHp = GetMaxHp();

                if ( healthRecoverTimer >= healthRecoverInterval )
                {
                    if ( hp < maxHp )
                    {
                        int value = buffHandler.healthRecoverValue - debuffHandler.healthRecoverValue + healthRegen;
                        value += ( ( buffHandler.healthRecoverRateValue - debuffHandler.healthRecoverRateValue ) * maxHp ).integer;
                        hp += value;
                        healthRecoverTimer = 0;

                        HealthChangeCheck();
                    }
                    else
                    {
                        hp = maxHp;
                    }

                    RenderMessage rm = new RenderMessage();
                    rm.type = RenderMessage.Type.SyncHP;
                    rm.ownerId = id;
                    rm.arguments.Add( "type", (int)type );
                    rm.arguments.Add( "hp", hp );
                    rm.arguments.Add( "maxHp", maxHp );
                    rm.arguments.Add( "mark", mark );

                    PostRenderMessage( rm );
                }
                else
                {
                    healthRecoverTimer += deltaTime;
                }
            }
        }

        protected void HealthChangeCheck()
        {
            stateListener.PostHealthChangedEvent( hp, GetMaxHp() );
        }

        protected int GetDamageValue()
        {
            int attackAttribute;
            if ( professionType == ProfessionType.WizardType || professionType == ProfessionType.AssistantType )
            {
                attackAttribute = GetMagicAttackAttribute();
            }
            else
            {
                attackAttribute = GetPhyiscalAttackAttribute();
            }

            int damage = Formula.GetAttackFloatingValue( attackAttribute, GetRandomNumber(), GetRandomNumber( physicalAttackVar ) );

            return damage;
        }

        public long GetAttackArea()
        {
            long baseAttackArea = attackArea;

            if ( target != null )
            {
                long targetModelRadius = target.modelRadius;

                return baseAttackArea + targetModelRadius;
            }

            return baseAttackArea;
        }

        public override int GetSpeedFactor()
        {
            int value = speedFactor * ( FixFactor.one + buffHandler.speedRateValue - debuffHandler.speedRateValue ) + buffHandler.speedValue - debuffHandler.speedValue;
            DebugUtils.Log( DebugUtils.Type.Soldier_Properties, string.Format( "Soldier speed factor:{0} real speed:{1}", speedFactor, value ) );
            return value;
        }

        // Use armor and damage to calculate real damage
        private int GetActualHurtValue( AttackPropertyType type, int hurtValue )
        {
            int value = 0;

            if ( type == AttackPropertyType.PhysicalAttack )
            {
                int realArmor = buffHandler.armorValue - debuffHandler.armorValue + armor * ( FixFactor.one + buffHandler.armorRateValue - debuffHandler.armorRateValue );
                value = Formula.ActuallyPhysicalDamage( hurtValue , realArmor );
            }
            else if ( type == AttackPropertyType.MagicAttack )
            {
                int realMagicResist = buffHandler.magicResistValue - debuffHandler.magicResistValue + magicResist * ( FixFactor.one + buffHandler.magicResistRateValue - debuffHandler.magicResistRateValue );
                value = Formula.ActuallyMagicDamage( hurtValue, realMagicResist );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Soldier, string.Format( "Can't handle this hurtType {0} now!", type ) );
            }

            return value;
        }

        #region Get Soldier Properties

        public int GetAttackInterval()
        {
            return attackInterval * ( FixFactor.one - buffHandler.attackSpeedRateValue + debuffHandler.attackSpeedRateValue );
        }

        public int GetPhyiscalAttackAttribute()
        {
            return physicalAttack * ( FixFactor.one + buffHandler.physicalAtkRateValue - debuffHandler.physicalAtkRateValue ) + buffHandler.physicalAtkValue - debuffHandler.physicalAtkValue;
        }

        public int GetMagicAttackAttribute()
        {
            return magicAttack * ( FixFactor.one + buffHandler.magicAtkRateValue - debuffHandler.magicAtkRateValue ) + buffHandler.magicAtkValue - debuffHandler.magicAtkValue;
        }

        public int GetCriticalChance()
        {
            return criticalChance + ( buffHandler.criticalChanceValue - debuffHandler.criticalChanceValue );
        }

        public int GetMaxHp()
        {
            return ((( FixFactor.one + buffHandler.maxHealthRateValue - debuffHandler.maxHealthRateValue ) * maxHp).integer + buffHandler.maxHealthValue - debuffHandler.maxHealthValue );
        }
        #endregion


        public void ClearPathRender()
        {
            RenderMessage rm = new RenderMessage();
            rm.ownerId = id;
            rm.type = RenderMessage.Type.ClearPath;
            rm.arguments.Add( "type", 1 );
            PostRenderMessage( rm );
        }

        private void InitializeUnitSkill()
        {
            skillHandler = new UnitSkillHandler( this );

            // Initialize unit skill
            SkillProto skill1Proto = DataManager.GetInstance().unitSkillsProtoData.Find( p => p.ID == skill1Id );
            if ( skill1Proto != null )
            {
                skillHandler.AddSkill( skill1Proto, 1 );
            }

            SkillProto skill2Proto = DataManager.GetInstance().unitSkillsProtoData.Find( p => p.ID == skill2Id );
            if ( skill2Proto != null )
            {
                skillHandler.AddSkill( skill2Proto, 2 );
            }
        }

        public void SetSkillHandlerEnable( bool enable )
        {
            if ( skillHandler != null )
            {
                skillHandler.SetHandlerEnabled( enable );
            }
        }

        private void SetUnitProperty( BattlerUnit unitInfo )
        {
            for ( int i = 0; i < unitInfo.props.Count; i++ )
            {
                if ( unitInfo.props[i].propertyType == PropertyType.MagicAttack )
                {
					magicAttack += ( int )unitInfo.props[i].propertyValue;
                }
				else if ( unitInfo.props[i].propertyType == PropertyType.MagicResist )
				{
					magicResist += (int)unitInfo.props[i].propertyValue;
				}
				else if ( unitInfo.props[i].propertyType == PropertyType.PhysicalAttack )
				{
					physicalAttack += ( int )unitInfo.props[i].propertyValue;
				}
				else if ( unitInfo.props[i].propertyType == PropertyType.ArmorPro )
				{
					armor += ( int )unitInfo.props[i].propertyValue;
				}
				else if ( unitInfo.props[i].propertyType == PropertyType.AttackRange )
                {
                    attackRange += (int)unitInfo.props[i].propertyValue;
                }
				else if ( unitInfo.props[i].propertyType == PropertyType.AttackSpeed )
				{
					attackInterval -= (int)unitInfo.props[i].propertyValue;
				}
                else if ( unitInfo.props[i].propertyType == PropertyType.CriticalChance )
                {
					criticalChance += ( int )unitInfo.props[i].propertyValue;
                }
                else if ( unitInfo.props[i].propertyType == PropertyType.CriticalDamage )
                {
					criticalDamage += ( int )unitInfo.props[i].propertyValue;
                }
                else if ( unitInfo.props[i].propertyType == PropertyType.MaxHealth )
                {
                    maxHp += (int)unitInfo.props[i].propertyValue;
                    hp = maxHp;
                }
                else if ( unitInfo.props[i].propertyType == PropertyType.HealthRecover )
                {
                    healthRegen += ( int )unitInfo.props[i].propertyValue;
                }
                else if ( unitInfo.props[i].propertyType == PropertyType.Speed )
                {
                    speedFactor += ConvertUtils.ToLogicInt( unitInfo.props[i].propertyValue );
                }

                DebugUtils.Log( DebugUtils.Type.AI_Soldier, string.Format( "Property addictvie : type:{0}, value:{1}", unitInfo.props[i].propertyType, unitInfo.props[i].propertyValue ) );
            }
        }

        #region Register MessageHandler
        public void RegisterGenerateAttributeEffectMethod( AttributeEffectGenerator method )
        {
            GenerateAttributeEffect = method;
        }

        public void RegisterSkillGenerator( SkillGenerator method )
        {
            GenerateSkill = method;
        }

        public void RegisterProjectileGenerator( ProjectileGenerator generator )
        {
            GenerateProjectile = generator;
        }

        public void RegisterSummonGenerator( SummonGenerator generator )
        {
            GenerateSummon = generator;
        }

        public void RegisterFindOwnForceSoldiers( FindOwnForceSoldiersMethod method )
        {
            FindOwnForceSoldiers = method;
        }

        public void RegisterFindFriendlySoldiers( FindFriendlySoldiersMethod method )
        {
            FindFriendlySoldiers = method;
        }

        public void RegisterFindOpponentSoldier( FindOpponentSoldierMethod method )
        {
            FindOpponentSoldier = method;
        }

        public void RegisterFindOpponentSoldiers( FindOpponentSoldiersMethod method )
        {
            FindOpponentSoldiers = method;
        }

        public void RegisterFindOpponentBuilding( FindOpponentBuildingMethod method )
        {
            FindOpponentBuilding = method;
        }

        public void RegisterFindOpponentBuildings( FindOpponentBuildingsMethod method )
        {
            FindOpponentBuildings = method;
        }

        public void RegisterGetGroupSoldiers( GetGroupSoldiersMethod method )
        {
            GetGroupSoldiers = method;
        }

        public void RegisterCalculateAvoidance( CalculateAvoidanceMethod method )
        {
            CalculateAvoidance = method;
        }

        public void RegisterFindOpponentCrystalCar( FindOpponentCrystalCarMethod method )
        {
            FindOpponentCrystalCar = method;
        }

        public void RegisterFindOpponentCrystalCars( FindOpponentCrystalCarsMethod method )
        {
            FindOpponentCrystalCars = method;
        }

        public void RegisterFindOpponentDemolisher( FindOpponentDemolisherMethod method )
        {
            FindOpponentDemolisher = method;
        }

        public void RegisterFindOpponentDemolishers( FindOpponentDemolishersMethod method )
        {
            FindOpponentDemolishers = method;
        }

        public void RegisterFindNeutralUnit( FindNeutralUnitMethod method )
        {
            FindNeutralUnit = method;
        }

        public void RegisterFindNeutralUnits( FindNeutralUnitsMethod method )
        {
            FindNeutralUnits = method;
        }

        public void RegisterFindPowerUp( FindPowerUpMethod method )
        {
            FindPowerUpMethod = method;
        }

        public void RegisterWithinFrontRectangleAreaPredicate( WithinFrontRectAreaPredicate method )
        {
            WithinFrontRectAreaPredicate = method;
        }

        public void RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate method )
        {
            WithinCircleAreaPredicate = method;
        }

        public void RegisterWithinFrontSectorAreaPredicate( WithinSectorAreaPredicate method )
        {
            WithinSectorAreaPredicate = method;
        }

        public void RegisterTrapGenerator( TrapGenerator method )
        {
            GenerateTrap = method;
        }
        #endregion
    }
}

