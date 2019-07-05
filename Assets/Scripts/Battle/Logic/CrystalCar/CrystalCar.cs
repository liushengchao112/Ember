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
    public enum CrystalCarState
    {
        NONE,
        IDLE,
        WALK,
        CHASE,
        MINING,
        DYING,
        DEATH,
        IDLE2CHASE,
        IDLE2WALK,
        CHASE2CHASE,
        WALK2CHASE
    }

    public class CrystalCar : MovableUnit
    {
        // register method
        protected WithinCircleAreaPredicate withinCircleArea;
        protected FindOpponentCrystalMethod FindOpponentCrystal;
        protected FindOpponentSoldiersMethod FindOpponentSoldiers;

        // runtime data
        public Crystal target;
        public CrystalCarState state;
        private bool owner;
        private Town town;
        private FixVector3 bornPosition;
        private AttackPropertyType hurtType;
        private int healthRegenInterval;
        private int healthRecoverTimer;

        public int emberHarvest;
        public int fightTimer;
        public int miningInterval;
        public int chaseArea;
        public int attackArea;
        public int attackRadius;
        public int armor;
        public int magicResist;
        public int deploymentCost;
        public int healthRegen;
        public PathType moveMode;

        // path...
        protected int currentWayPointIndex;

        // FSM
        public CrystalCarFsmIdle fsmIdle;
        public CrystalCarFsmWalk fsmWalk;
        public CrystalCarFsmChase fsmChase;
        public CrystalCarFsmDead fsmDead;
        public CrystalCarFsmDying fsmDying;
        public CrystalCarFsmMining fsmMining;
        public CrystalCarFsmPlaceHolder fsmPlaceHolder;

        // buff/debuff handler
        public DebuffHandler debuffHandler;
        public BuffHandler buffHandler;

        public void Initialize( Town town, long id, FixVector3 pos, bool simulate )
        {
            type = LogicUnitType.CrystalCar;

            this.id = id;
            this.town = town;
            mark = town.mark;
            position = pos;
            transform.position = pos.vector3;
            bornPosition = pos;

            InitializedData();

            if ( pathAgent != null )
            {
                pathAgent.SetPathType( moveMode );
                pathAgent.SetPosition( position );
            }

            InitializeOwner( simulate );

            debuffHandler = new DebuffHandler();
            buffHandler = new BuffHandler();

            fsmIdle = new CrystalCarFsmIdle( this );
            fsmWalk = new CrystalCarFsmWalk( this );
            fsmChase = new CrystalCarFsmChase( this );
            fsmMining = new CrystalCarFsmMining( this );
            fsmDying = new CrystalCarFsmDying( this );
            fsmDead = new CrystalCarFsmDead( this );
            fsmPlaceHolder = new CrystalCarFsmPlaceHolder( this );
            currentFsm = fsmIdle;
            ChangeState( CrystalCarState.IDLE, fsmIdle );

            DebugUtils.Log( DebugUtils.Type.AI_CrystalCar, string.Format( " Initialize crystal car, born position = {0} ", pos ) );
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
            UnitProto proto = DataManager.GetInstance().unitsProtoData.Find( p => p.ProfessionType == (int)ProfessionType.TramcarType );

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
            chaseArea = 100;// defalut whole map
            attackArea = ConvertUtils.ToLogicInt( proto.AttackRange );
            miningInterval = ConvertUtils.ToLogicInt( proto.AttackInterval );
            emberHarvest = proto.EmberHarvest;
            attackRadius = ConvertUtils.ToLogicInt( proto.AttackRadius );
            killReward = proto.killReward;
            healthRegen = proto.HealthRegen;

            hurtType = AttackPropertyType.None;
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
            rm.direction = speed.vector3;
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
        }

        public void Idle()
        {
            ChangeState( CrystalCarState.IDLE, fsmIdle );

            direction = speed.normalized;

            RenderMessage message = new RenderMessage();
            message.type = RenderMessage.Type.CrystalCarIdle;
            message.direction = direction.vector3;
            message.ownerId = id;
            PostRenderMessage( message );
        }

        public void Walk( FixVector3 d )
        {
            // ignore walk when car near by the born point
            if ( ( d - position ).sqrMagnitude <= 0.5 )
            {
                return;
            }

            if ( state == CrystalCarState.IDLE )
            {
                DebugUtils.Log( DebugUtils.Type.AI_CrystalCar, "crystal car " + id + " enters IDLE2WALK state, target position = " + d );
                ChangeState( CrystalCarState.IDLE2WALK, fsmPlaceHolder );
            }
            else 
            {
                return;
            }

            DebugUtils.Log( DebugUtils.Type.AI_CrystalCar, string.Format( " {0}'s crystal car {1} begin to go {2} ", mark, id, d ) );

            if ( owner )
            {
                pathAgent.FindPath( position, d, NavMeshAreaType.WALKABLE, OnWalkPathComplete );
            }
        }

        public void Chase( Crystal crystal )
        {
            DebugUtils.Log( DebugUtils.Type.AI_CrystalCar, string.Format( " {0}'s crystal car {1} begin to chase crystal {2} at {3} ", mark, id, crystal.id, crystal.position ) );

            target = crystal;

            if ( state == CrystalCarState.IDLE )
            {
                DebugUtils.Log( DebugUtils.Type.AI_CrystalCar, "crystal car " + id + " enters IDLE2CHASE state, target id = " + crystal.id );
                ChangeState( CrystalCarState.IDLE2CHASE, fsmPlaceHolder );
            }
            else if ( state == CrystalCarState.WALK )
            {
                DebugUtils.Log( DebugUtils.Type.AI_CrystalCar, "crystal car " + id + " enters WALK2CHASE state, target id = " + crystal.id );
                ChangeState( CrystalCarState.WALK2CHASE, fsmPlaceHolder );
            }
            else if ( state == CrystalCarState.CHASE )
            {
                DebugUtils.Log( DebugUtils.Type.AI_CrystalCar, "crystal car " + id + " enters CHASE2CHASE state, target id = " + crystal.id );
                ChangeState( CrystalCarState.CHASE2CHASE, fsmPlaceHolder );
            }
            else
            {
                // can't handle this state to chase for now
                DebugUtils.LogError( DebugUtils.Type.AI_CrystalCar, "Can't chase when crystal car in  " + state.ToString() );
            }

            if ( owner )
            {
                pathAgent.FindPath( position, crystal.position, NavMeshAreaType.WALKABLE, OnChasePathComplete );
            }
        }

        protected override void WaitChaseHandler()
        {
            base.WaitChaseHandler();

            ChangeState( CrystalCarState.CHASE, fsmChase );

            RenderMessage rm = new RenderMessage();
            rm.ownerId = id;
            rm.type = RenderMessage.Type.CrystalCarWalk;
            rm.arguments.Add( "sx", direction.x );
            rm.arguments.Add( "sy", direction.y );
            rm.arguments.Add( "sz", direction.z );

            PostRenderMessage( rm );
        }

        protected override void WaitWalkHandler( Sync s, List<Position> posList )
        {
            base.WaitWalkHandler( s, posList );

            ChangeState( CrystalCarState.WALK, fsmWalk );

            RenderMessage rm = new RenderMessage();
            rm.ownerId = id;
            rm.type = RenderMessage.Type.CrystalCarWalk;
            rm.arguments.Add( "sx", direction.x );
            rm.arguments.Add( "sy", direction.y );
            rm.arguments.Add( "sz", direction.z );

            PostRenderMessage( rm );
        }

        protected override void FinishedChase()
        {
            Idle();
        }

        protected override void FinishedWalk()
        {
            Idle();
        }

        public void FindOpponent()
        {
            Crystal crystal = FindOpponentCrystal( position, chaseArea );

            if ( crystal != null )
            {
                long distance = FixVector3.SqrDistance( crystal.position, position );

                if ( distance < GameConstants.EQUAL_DISTANCE )
                {
                    Idle();
                }
                else
                {
                    Chase( crystal );
                }
            }
            else
            {
                Walk( bornPosition );
            }
        }

        public void Mining( Crystal crystal )
        {
            target = crystal;
            direction = speed.normalized;

            ChangeState( CrystalCarState.MINING, fsmMining );

            RenderMessage message = new RenderMessage();
            message.type = RenderMessage.Type.CrystalCarMining;
            message.ownerId = id;
            message.direction = direction.vector3;
            PostRenderMessage( message );
        }

        public void Dying()
        {
            List<Soldier> soldiers = FindOpponentSoldiers( mark, position, (s)=>
            {
                return withinCircleArea( position, attackArea, s.position );
            } );

            for ( int i = 0; i < soldiers.Count; i++ )
            {
                soldiers[i].Hurt( damage, hurtType, false, this );
            }

            debuffHandler.Destory();
            buffHandler.Destory();

            RenderMessage message = new RenderMessage();
            message.type = RenderMessage.Type.CrystalCarDestroy;
            message.ownerId = id;
            message.direction = direction.vector3;
            message.arguments.Add( "mark", mark );
            PostRenderMessage( message );

            ChangeState( CrystalCarState.DEATH, fsmDead );
        }

        public void Destroy()
        {
            PostDestroy( this );
        }

        public void ChangeState( CrystalCarState state, Fsm targetFsm )
        {
            currentFsm.OnExit();
            this.state = state;
            currentFsm = targetFsm;
            currentFsm.OnEnter();

            DebugUtils.LogWarning( DebugUtils.Type.AI_CrystalCar, string.Format( " {0}'s crystal car {1} enter state {2} ", mark, id, this.state ) );
        }

        public override void AddCoin( int coin )
        {
            town.AddCoin( coin );
        }

        public override bool Alive()
        {
            return state != CrystalCarState.DEATH && state != CrystalCarState.DYING;
        }

        public override void Hurt( int hurtValue, AttackPropertyType type, bool isCrit, LogicUnit injurer )
        {
            if ( Alive() )
            {
                int value = GetActualHurtValue( hurtValue, hurtType );
                hp -= value;
                RenderMessage message = new RenderMessage();
                message.type = RenderMessage.Type.CrystalCarHurt;
                message.ownerId = id;
                message.arguments.Add( "value", value );
                PostRenderMessage( message );

                if ( hp <= 0 )
                {
                    injurer.OnKillEnemy( killReward, injurer, this );
                    Dying();
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
                DebugUtils.LogError( DebugUtils.Type.AI_CrystalCar, string.Format( "Can't handle this hurtType {0} now!", type ) );
            }

            return value;
        }

        public override bool InChaseState()
        {
            return state == CrystalCarState.CHASE;
        }

        public override bool Moveable()
        {
            return false;
        }

        public override bool InWalkState()
        {
            return state == CrystalCarState.WALK;
        }

        public override bool InSkillState()
        {
            return false;
        }

        public override bool InMovePlaceHolder()
        {
            return state == CrystalCarState.WALK2CHASE || 
                   state == CrystalCarState.IDLE2WALK || 
                   state == CrystalCarState.IDLE2CHASE;
        }

        public override void MovingPlaceHolder()
        {
            // Crystal car do nothing in moving place holder for now
        }

        protected override void FinishSkillMove()
        {
            // Crystal car do nothing in moving place holder for now
        }

        public override int GetSpeedFactor()
        {
            return speedFactor;
        }

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            DebugUtils.Assert( false, "OnKillEnemy() in CrystalCar is not implemented!" );
        }

        public override void Reset()
        {
            id = -1;

            state = CrystalCarState.NONE;
            town = null;

            hp = 0;
            maxHp = 0;
            damage = 0;

            transform.position = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
            transform.rotation = Quaternion.identity;

            PostRenderMessage = null;
            PostDestroy = null;

            FindOpponentSoldiers = null;
            FindOpponentCrystal = null;
        }

        public void RegisterFindOpponentCrystal( FindOpponentCrystalMethod method )
        {
            FindOpponentCrystal = method;
        }

        public void RegisterFindOpponentSoldiers( FindOpponentSoldiersMethod method )
        {
            FindOpponentSoldiers = method;
        }
            
        public void RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate method )
        {
            withinCircleArea = method;
        }
    }
}

