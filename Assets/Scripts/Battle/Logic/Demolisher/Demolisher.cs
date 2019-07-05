using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System;

using Utils;
using Data;
using BattleAgent;
using Constants;

using UnitProto = Data.UnitsProto.Unit;

namespace Logic
{
    public enum DemolisherState
    {
        NONE,
        IDLE,
        WALK,
        CHASE,
        FIGHT,
        DYING,
        DEATH,
        IDLE2CHASE,
        IDLE2WALK,
        CHASE2CHASE,
        WALK2CHASE
    }

    public class Demolisher : MovableUnit
    {
        // register method
        protected FindOpponentBuildingMethod FindOpponentBuilding;

        // runtime data
        public DemolisherState state;
        private int healthRecoverTimer = 0;
        private int healthRegenInterval;
        private int destroyTimer;
        private int destroyTime;
        private bool owner;
        private Town town;
        private FixVector3 bornPosition;
        public LogicUnit target;

        // proto data
        public int deploymentCost;
        public int fightTimer;
        public int attackInterval;
        public int chaseArea;
        public int attackArea;
        public int attackRadius;
        public int structAddition;
        public int armor;
        public int magicResist;
        public PathType moveMode;
        public int damageVar;
        public int healthRegen;
        public AttackPropertyType hurtType;

        // path 
        protected int currentWayPointIndex;

        // FSM
        public DemolisherFsmIdle fsmIdle;
        public DemolisherFsmWalk fsmWalk;
        public DemolisherFsmChase fsmChase;
        public DemolisherFsmDead fsmDead;
        public DemolisherFsmDying fsmDying;
        public DemolisherFsmFight fsmFight;
        public DemolisherFsmPlaceHolder fsmPlaceHolder;

        // buff/debuff handler
        public DebuffHandler debuffHandler;
        public BuffHandler buffHandler;

        public void Initialize( Town town, long id, FixVector3 pos, bool simulate )
        {
            type = LogicUnitType.Demolisher;

            owner = DataManager.GetInstance().GetPlayerId() == town.id;

            this.id = id;
            this.town = town;
            mark = town.mark;
            position = pos;
            transform.position = pos.vector3;
            bornPosition = pos;

            InitializedData();

            pathAgent.SetPathType( moveMode );
            pathAgent.SetPosition( position );

            InitializeOwner( simulate );

            debuffHandler = new DebuffHandler();
            buffHandler = new BuffHandler();

            fsmIdle = new DemolisherFsmIdle( this );
            fsmWalk = new DemolisherFsmWalk( this );
            fsmChase = new DemolisherFsmChase( this );
            fsmFight = new DemolisherFsmFight( this );
            fsmDying = new DemolisherFsmDying( this );
            fsmDead = new DemolisherFsmDead( this );
            fsmPlaceHolder = new DemolisherFsmPlaceHolder( this );
            currentFsm = fsmIdle;
            ChangeState( DemolisherState.IDLE, fsmIdle );

            DebugUtils.Log( DebugUtils.Type.AI_Demolisher, string.Format( " Initialize demolisher, born position = {0} ", pos ) );
        }

        protected void InitializeOwner( bool sim )
        {
            if ( sim )
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

        private void InitializedData()
        {
            UnitProto proto = DataManager.GetInstance().unitsProtoData.Find( p => p.ProfessionType == (int)ProfessionType.DemolisherType );

            metaId = proto.ID;
            modelId = proto.Model;
            iconId = proto.Icon;
            modelRadius = ConvertUtils.ToLogicInt( proto.ModelRadius );
            deploymentCost = proto.DeploymentCost;
            maxHp = proto.Health;
            armor = proto.Armor;
            magicResist = proto.MagicResist;
            speedFactor = ConvertUtils.ToLogicInt( proto.MoveSpeed );
            moveMode = (PathType)proto.MoveMode;
            chaseArea = 100;// default all unit
            attackArea = ConvertUtils.ToLogicInt( proto.AttackRange );
            attackInterval = ConvertUtils.ToLogicInt( proto.AttackInterval );
            damage = (int)proto.PhysicalAttack;
            damageVar = ConvertUtils.ToLogicInt( proto.MeleeDmgVar );
            attackRadius = ConvertUtils.ToLogicInt( proto.AttackRadius );
            structAddition = ConvertUtils.ToLogicInt( proto.StrctDmgMult );
            killReward = proto.killReward;
            healthRegen = proto.HealthRegen;

            hurtType = AttackPropertyType.PhysicalAttack;
            hp = maxHp;

            healthRegenInterval = GameConstants.HP_RECOVERY_INTERVAL_MILLISECOND;
        }

        public override void LogicUpdate( int deltaTime )
        {
            currentFsm.Update( deltaTime );

            SyncPosition();
            HealthRecover( deltaTime );

            debuffHandler.LogicUpdate( deltaTime );
            buffHandler.LogicUpdate( deltaTime );
        }

        public void SyncPosition()
        {
            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SyncPosition;
            rm.ownerId = id;
            rm.arguments.Add( "type", (int)type );
            rm.position = position.vector3;
            rm.direction = speed.normalized.vector3;
            PostRenderMessage( rm );
        }

        protected void HealthRecover( int deltaTime )
        {
            if ( Alive() && healthRegen > 0 )
            {
                if ( hp < maxHp )
                {
                    if ( healthRecoverTimer >= healthRegenInterval )
                    {
                        hp += healthRegen;
                        healthRecoverTimer = 0;

                        RenderMessage rm = new RenderMessage();
                        rm.type = RenderMessage.Type.SyncHP;
                        rm.ownerId = id;
                        rm.arguments.Add( "type", (int)type );
                        rm.arguments.Add( "hp", hp );
                        rm.arguments.Add( "mark", mark );
                        rm.arguments.Add( "maxHp", maxHp );

                        PostRenderMessage( rm );
                    }
                    else
                    {
                        healthRecoverTimer += deltaTime;
                    }
                }
            }
        }

        public void Idle()
        {
            ChangeState( DemolisherState.IDLE, fsmIdle );

            direction = speed.normalized;

            RenderMessage message = new RenderMessage();
            message.type = RenderMessage.Type.DemolisherIdle;
            message.direction = direction.vector3;
            message.ownerId = id;
            PostRenderMessage( message );
        }

        public void Walk( FixVector3 d )
        {
            // ignore walk when demolisher near by the born point
            if ( ( d - position ).magnitude <= 500 )
            {
                return;
            }

            if ( state == DemolisherState.IDLE )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Demolisher, "demolisher " + id + " enters IDLE2WALK state, target position = " + d );
                ChangeState( DemolisherState.IDLE2WALK, fsmPlaceHolder );
            }
            else
            {
                return;
            }

            DebugUtils.Log( DebugUtils.Type.AI_Demolisher, string.Format( " {0}'s demolisher {1} begin to go {2} ", mark, id, d ) );

            if ( owner )
            {
                pathAgent.FindPath( position, d, NavMeshAreaType.WALKABLE, OnWalkPathComplete );
            }
        }

        public void Chase( LogicUnit building )
        {
            if ( target != null && target == building )
            {
                return;
            }

            DebugUtils.Log( DebugUtils.Type.AI_Demolisher, string.Format( " {0}'s demolisher {1} begin to chase building {2} at {3} ", mark, id, building.id, building.position ) );

            target = building;

            if ( state == DemolisherState.IDLE )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Demolisher, "demolisher " + id + " enters IDLE2CHASE state, target id = " + building.id );
                ChangeState( DemolisherState.IDLE2CHASE, fsmPlaceHolder );
            }
            else if ( state == DemolisherState.WALK )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Demolisher, "demolisher " + id + " enters WALK2CHASE state, target id = " + building.id );
                ChangeState( DemolisherState.WALK2CHASE, fsmPlaceHolder );
            }
            else if ( state == DemolisherState.CHASE )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Demolisher, "demolisher " + id + " enters CHASE2CHASE state, target id = " + building.id );
                ChangeState( DemolisherState.CHASE2CHASE, fsmPlaceHolder );
            }
            else
            {
                // can't handle this state to chase for now
                DebugUtils.LogError( DebugUtils.Type.AI_Demolisher, "Can't chase when demolisher in  " + state.ToString() );
            }

            if ( owner )
            {
                pathAgent.FindPath( position, building.position, NavMeshAreaType.WALKABLE, OnChasePathComplete );
            }
        }

        protected override void WaitChaseHandler()
        {
            base.WaitChaseHandler();

            ChangeState( DemolisherState.CHASE, fsmChase );

            RenderMessage rm = new RenderMessage();
            rm.ownerId = id;
            rm.type = RenderMessage.Type.DemolisherWalk;
            rm.arguments.Add( "sx", direction.x );
            rm.arguments.Add( "sy", direction.y );
            rm.arguments.Add( "sz", direction.z );
            PostRenderMessage( rm );
        }

        protected override void WaitWalkHandler( Sync s, List<Position> posList )
        {
            base.WaitWalkHandler( s, posList );

            ChangeState( DemolisherState.WALK, fsmWalk );

            RenderMessage rm = new RenderMessage();
            rm.ownerId = id;
            rm.type = RenderMessage.Type.DemolisherWalk;
            rm.arguments.Add( "sx", direction.x );
            rm.arguments.Add( "sy", direction.y );
            rm.arguments.Add( "sz", direction.z );
            PostRenderMessage( rm );
        }

        protected override void FinishedWalk()
        {
            Idle();
        }

        protected override void FinishedChase()
        {
            Idle();
        }

        public void FindOpponent()
        {
            LogicUnit building = FindOpponentBuilding( mark, position ,chaseArea );

            if ( building != null )
            {
                long distance = FixVector3.SqrDistance( building.position , position );

                if ( distance < GameConstants.EQUAL_DISTANCE )
                {
                    Idle();
                }
                else
                {
                    Chase( building );
                }
            }
            else
            {
                Walk( bornPosition );
            }
        }

        public void Fight( LogicUnit building )
        {
            target = building;
            direction = speed.normalized;

            ChangeState( DemolisherState.FIGHT, fsmFight );

            RenderMessage message = new RenderMessage();
            message.type = RenderMessage.Type.DemolisherFight;
            message.ownerId = id;
            message.direction = direction.vector3;
            PostRenderMessage( message );
        }

        public void Dying()
        {
            RenderMessage message = new RenderMessage();
            message.type = RenderMessage.Type.DemolisherDestroy;
            message.ownerId = id;
            message.direction = direction.vector3;
            message.arguments.Add( "mark", mark );
            PostRenderMessage( message );

            debuffHandler.Destory();
            buffHandler.Destory();

            ChangeState( DemolisherState.DEATH, fsmDead );
        }

        public void Destroy()
        {
            PostDestroy( this );
        }

        public void ChangeState( DemolisherState state, Fsm targetFsm )
        {
            currentFsm.OnExit();
            this.state = state;
            currentFsm = targetFsm;
            currentFsm.OnEnter();

            DebugUtils.LogWarning( DebugUtils.Type.AI_Demolisher, string.Format( " {0}'s demolisher {1} enter state {2} ", mark, id, this.state ) );
        }

        public override void AddCoin( int coin )
        {
            town.AddCoin( coin );
        }

        public override bool Alive()
        {
            return state != DemolisherState.DEATH && state != DemolisherState.DYING;
        }

        public override void Hurt( int hurtValue, AttackPropertyType type, bool isCrit, LogicUnit injurer )
        {
            if ( Alive() )
            {
                int value = GetActualHurtValue( hurtValue, hurtType );
                hp -= value;
                RenderMessage message = new RenderMessage();
                message.type = RenderMessage.Type.DemolisherHurt;
                message.ownerId = id;
                message.arguments.Add( "value", hurtValue );
                PostRenderMessage( message );

                if ( hp <= 0 )
                {
                    injurer.OnKillEnemy( killReward, injurer, this );
                    ChangeState( DemolisherState.DYING, fsmDying );
                }
            }
        }

        private int GetActualHurtValue( int hurtValue, AttackPropertyType type )
        {
            int value = 0;

            if ( type == AttackPropertyType.PhysicalAttack )
            {
                value = Formula.ActuallyPhysicalDamage( hurtValue, armor );
            }
            else if ( type == AttackPropertyType.MagicAttack )
            {
                value = Formula.ActuallyMagicDamage( hurtValue, magicResist );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Demolisher, string.Format( "Can't handle this hurtType {0} now!", type ) );
            }

            return value;
        }

        public override bool InChaseState()
        {
            return state == DemolisherState.CHASE;
        }

        public override bool Moveable()
        {
            return false;
        }

        public override bool InWalkState()
        {
            return state == DemolisherState.WALK;
        }

        public override Boolean InSkillState()
        {
            return false;
        }

        public override bool InMovePlaceHolder()
        {
            return state == DemolisherState.WALK2CHASE ||
                   state == DemolisherState.IDLE2WALK ||
                   state == DemolisherState.IDLE2CHASE;
        }

        public override void MovingPlaceHolder()
        {
            // Do nothing in place holder now
        }

        protected override void FinishSkillMove()
        {
            // Do nothing in place holder now
        }

        public override int GetSpeedFactor()
        {
            return speedFactor;
        }

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            town.AddCoin( emberReward );
        }

        public override void Reset()
        {
            id = -1;
            target = null;
            state = DemolisherState.NONE;
            speed = FixVector3.zero;
            healthRecoverTimer = 0;
            healthRegenInterval = 0;
            destroyTimer = 0;
            destroyTime = 0;
            owner = false;
            town = null;
            direction = FixVector3.zero;
            bornPosition = FixVector3.one;
            hp = 0;
            maxHp = 0;
            damage = 0;
            transform.position = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
            transform.rotation = Quaternion.identity;

            PostRenderMessage = null;
            PostDestroy = null;
            FindOpponentBuilding = null;
        }

        public void RegisterFindOpponentBuilding( FindOpponentBuildingMethod method )
        {
            FindOpponentBuilding = method;
        }
       
    }
}

