using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


using Utils;
using Data;
using BattleAgent;
using Constants;

using NpcData = Data.NpcProto.Npc;

namespace Logic
{
    public enum NpcState
    {
        NONE,
        IDLE,
        WALK,
        CHASE,
        ATTACK,
        DYING,
        DEATH
    }

    public enum NpcType
    {
        NONE,
        WildMonster,
        Idol,
        IdolGuard
    }

    public enum NpcAttackType
    {
        None = 0,
        Melee = 1,
        Ranged = 2
    }

    public class Npc : MovableUnit
    {
        public NpcState state;

        // proto data
        public NpcData npcProto;
        public NpcType npcType;
        public NpcAttackType attackType;
        public int fightInterval;
        public int rebornInterval;
        public int physicasAttack;
        public int armor;
        public int magicResist;
        public int healthRegen;
        public int attackRange;
        public int chaseArea;
        public int maxChaseDistance;
        public int emberOutPut;
        public int projectileId;
        public int attackDuration;
        public int attackHitTime;
        public AttackPropertyType hurtType;

        // runtime 
        public FixVector3 brithPosition;
        public FixVector3 brithDirection;
        public LogicUnit target;
        public int fightTimer;
        public int healthRecoverTimer = 0;
        public int rebornTimer = 0;
        private int healthRecoverInterval = 0;

        // register method 
        protected FindSoldierMethod FindSoldierMethod;
        protected ProjectileGenerator GenerateProjectile;

        // fsm
        public Fsm fsmAttack;
        public Fsm fsmChase;
        public Fsm fsmDeath;
        public Fsm fsmDying;
        public Fsm fsmIdle;
        public Fsm fsmWalk;

        // buff/debuff handler
        public DebuffHandler debuffHandler;
        public BuffHandler buffHandler;

        public void Initialize( long id, NpcData proto, FixVector3 p, FixVector3 r )
        {
            this.id = id;

            type = LogicUnitType.NPC;
          
            position = p;

            transform.position = p.vector3;
            transform.rotation = Quaternion.Euler( r.vector3 );

            brithPosition = p;
            brithDirection = r;
            hurtType = AttackPropertyType.PhysicalAttack;

            npcProto = proto;
            iconId = -1;
            metaId = proto.ID;
            modelId = proto.ModelID;
            modelRadius = ConvertUtils.ToLogicInt( proto.ModelRadius );
            npcType = (NpcType)proto.NPCType;
            attackType = (NpcAttackType)proto.StandardAttack;
            rebornInterval = ConvertUtils.ToLogicInt( proto.RebornInterval );
            physicasAttack = (int)proto.PhysicsAttack;
            armor = (int)proto.Armor;
            magicResist = (int)proto.MagicResist;
            speedFactor = ConvertUtils.ToLogicInt( proto.Speed );
            healthRegen = proto.HealthRegen;

            maxHp = proto.Health;
            chaseArea = ConvertUtils.ToLogicInt( proto.TargetDetectRange );
            maxChaseDistance = ConvertUtils.ToLogicInt( proto.MaxChaseDistance );
            emberOutPut = proto.EmberOutPut;
            projectileId = proto.ProjectileId;
            attackRange = ConvertUtils.ToLogicInt( proto.AttackRange );
            killReward = proto.KillReward;
            attackDuration = ConvertUtils.ToLogicInt( proto.AttackDuration );
            attackHitTime = ConvertUtils.ToLogicInt( proto.AttackHitDuration );
            fightInterval = 1;

            hp = maxHp;
            damage = physicasAttack;
            healthRecoverInterval = GameConstants.HP_RECOVERY_INTERVAL_MILLISECOND;

            InitializeState();
            InitializePathAgent();

            debuffHandler = new DebuffHandler();
            buffHandler = new BuffHandler();

            state = NpcState.IDLE;
            currentFsm = fsmIdle;
        }

        public virtual void InitializeState()
        {
            // TODO: NPC only has wild monster now!
            fsmAttack = new WildMonsterFsmAttack( this );
            fsmChase = new WildMonsterFsmChase( this );
            fsmDeath = new WildMonsterFsmDeath( this );
            fsmDying = new WildMonsterFsmDying( this );
            fsmIdle = new WildMonsterFsmIdle( this );
            fsmWalk = new WildMonsterFsmWalk( this );
        }

        public virtual void InitializePathAgent()
        {
            if ( pathAgent == null )
            {
                pathAgent = new NavAgent( this );
                pathAgent.SetPathType( PathType.Ground );
                pathAgent.SetPosition( position );
            }
        }

        public override void LogicUpdate( int deltaTime )
        {
            currentFsm.Update( deltaTime );

            SyncPosition();
            debuffHandler.LogicUpdate( deltaTime );
            buffHandler.LogicUpdate( deltaTime );
        }

        private void SyncPosition()
        {
            if ( Alive() )
            {
                RenderMessage rm = new RenderMessage();
                rm.type = RenderMessage.Type.SyncPosition;
                rm.ownerId = id;
                rm.arguments.Add( "type", (int)type );
                rm.position = position.vector3;
                rm.direction = speed.vector3;
                PostRenderMessage( rm );
            }
        }

        protected void HealthRecover( int deltaTime )
        {
            if ( Alive() )
            {
                if ( hp < maxHp )
                {
                    if ( healthRecoverTimer >= healthRecoverInterval )
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

        public virtual void Idle()
        {
            ChangeState( NpcState.IDLE, fsmIdle );

            RenderMessage rm = new RenderMessage();
            rm.ownerId = id;
            rm.type = RenderMessage.Type.NpcIdle;
            rm.direction = brithDirection.vector3;
            PostRenderMessage( rm );
        }

        public virtual void Walk( FixVector3 d )
        {
            DebugUtils.Log( DebugUtils.Type.AI_Npc, string.Format( " {0} {1} now change to destination {2} ", npcType, id, d ) );

            destination = d;
            pathAgent.FindPath( position, destination, NavMeshAreaType.WALKABLE, OnFoundLocalWalkPath );
        }

        protected void OnFoundLocalWalkPath( List<FixVector3> wp )
        {
            DebugUtils.Assert( wp.Count > 0, "MovableUnit : the new waypoints' length should be larger than 0 for a new walk!" );
            DebugUtils.Log( DebugUtils.Type.AI_MovableUnit, string.Format( "{0} {1} find Walk path complete, waypoint count = {2}, direction = {3}", type, id, waypoints.Count, direction ) );

            List<Position> positions = new List<Position>();

            if ( wp.Count > 0 )
            {
                int j = 0;
                long distance = FixVector3.SqrDistance( wp[j], position );

                if ( distance <= GameConstants.EQUAL_DISTANCE )
                {
                    DebugUtils.LogWarning( DebugUtils.Type.PathFinding, string.Format( "movable unit {0}'s first way point is too close! distance = {1}", id, distance ) );
                    j = 1;
                }

                for ( ; j < wp.Count; j++ )
                {
                    Position pos = new Position();
                    FixVector3 v = wp[j];
                    //DebugUtils.LogWarning( DebugUtils.Type.AI_MovableUnit, string.Format( "movable unit {0}'s {1} way point is ({2}, {3}, {4})", id, j, v.x, v.y, v.z ) );
                    pos.x = v.vector3.x;
                    pos.y = v.vector3.y;
                    pos.z = v.vector3.z;
                    positions.Add( pos );
                }
            }

            LocalPathHandler( positions );

            ChangeState( NpcState.WALK, fsmWalk );

            RenderMessage rm = new RenderMessage();
            rm.ownerId = id;
            rm.direction = direction.vector3;
            rm.type = RenderMessage.Type.NpcWalk;
            PostRenderMessage( rm );
        }

        protected override void FinishedWalk()
        {
            Idle();
        }

        public virtual void Chase( LogicUnit t )
        {
            DebugUtils.Log( DebugUtils.Type.AI_Npc, string.Format( " {0} {1} now chase to target {2} ", npcType, id, t.id ) );

            target = t;
            pathAgent.FindPath( position, target.position, NavMeshAreaType.WALKABLE, OnFoundLocalChasePath );
        }

        protected void OnFoundLocalChasePath( List<FixVector3> wp )
        {
            DebugUtils.Log( DebugUtils.Type.AI_MovableUnit, string.Format( "{0} {1} find chase path complete, waypoint count = {2}, direction = {3}", type, id, wp.Count, direction ) );

            List<Position> positions = new List<Position>();

            if ( wp.Count > 0 )
            {
                int j = 0;
                long distance = FixVector3.SqrDistance( wp[j], position );

                if ( distance <= GameConstants.EQUAL_DISTANCE )
                {
                    DebugUtils.LogWarning( DebugUtils.Type.PathFinding, string.Format( "movable unit {0}'s first way point is too close! distance = {1}", id, distance ) );
                    j = 1;
                }

                for ( ; j < wp.Count; j++ )
                {
                    Position pos = new Position();
                    FixVector3 v = wp[j];
                    //DebugUtils.LogWarning( DebugUtils.Type.AI_MovableUnit, string.Format( "movable unit {0}'s {1} way point is ({2}, {3}, {4})", id, j, v.x, v.y, v.z ) );
                    pos.x = v.vector3.x;
                    pos.y = v.vector3.y;
                    pos.z = v.vector3.z;
                    positions.Add( pos );
                }
            }

            LocalPathHandler( positions );

            ChangeState( NpcState.CHASE, fsmChase );

            RenderMessage rm = new RenderMessage();
            rm.ownerId = id;
            rm.direction = direction.vector3;
            rm.type = RenderMessage.Type.NpcWalk;
            PostRenderMessage( rm );
        }
      
        protected override void FinishedChase()
        {
            Attack( target );
        }

        public virtual void Attack( LogicUnit t )
        {
            ChangeState( NpcState.ATTACK, fsmAttack );

            fightTimer = 0;
            target = t;

            direction = t.position - position;

            RenderMessage message = new RenderMessage();
            message.type = RenderMessage.Type.NpcFight;
            message.ownerId = id;

            message.direction = direction.normalized.vector3;

            message.arguments.Add( "intervalRate", 1 );// default static
            PostRenderMessage( message );
        }

        public virtual void Fight()
        {
            if ( target != null && target.Alive() )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Npc, "the " + npcType + " npc " + id + " begins to attack target " + target.id );

                damage = physicasAttack;
                target.Hurt( damage, hurtType, false, this );
            }
        }

        public virtual void Dying()
        {
            ChangeState( NpcState.DYING, fsmDying );

            debuffHandler.Destory();
            buffHandler.Destory();

            RenderMessage renderMessage = new RenderMessage();
            renderMessage.type = RenderMessage.Type.NpcDeath;
            renderMessage.ownerId = id;

            PostRenderMessage( renderMessage );
        }

        public virtual void Death()
        {
            ChangeState( NpcState.DEATH, fsmDeath );
        }

        public virtual void Reborn()
        {
            Reset();

            Initialize( id, npcProto, brithPosition, brithDirection );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.NpcReborn;
            rm.ownerId = id;
            PostRenderMessage( rm );

            DebugUtils.Log( DebugUtils.Type.AI_Npc, string.Format( " {0} {1} has been reborn", npcType, id ) );
        }

        public override void Hurt( int hurtValue, AttackPropertyType hurtType, bool isCrit, LogicUnit injurer )
        {
            if ( Alive() )
            {
                int value = GetActualHurtValue( hurtType, hurtValue );
                hp -= value;

                RenderMessage message = new RenderMessage();
                message.type = RenderMessage.Type.NpcHurt;
                message.ownerId = id;
                message.position = transform.position;
                message.arguments.Add( "value", value );
                PostRenderMessage( message );

                if ( hp <= 0 )
                {
                    injurer.OnKillEnemy( killReward, injurer, this );
                    Dying();
                }
                else
                {
                    Attack( injurer );
                }
            }
        }

        private void LocalPathHandler( List<Position> wp )
        {
            DebugUtils.Assert( wp.Count > 0, "MovableUnit : the new waypoints' length should be larger than 0 for a new walk!" );

            currentWaypoint = 0;
            waypoints = wp;

            Position p = waypoints[currentWaypoint];
            FixVector3 currentWayPointV = new FixVector3( p.x, p.y, p.z );

            Debug.Log( "currentWayPointV = " + currentWayPointV + "current position = " + position );
            direction = ( currentWayPointV - position ).normalized;
            speed = direction * GetSpeedFactor();
        }

        public override bool Alive()
        {
            return state != NpcState.DEATH && state != NpcState.DYING;
        }

        public override bool InChaseState()
        {
            return state == NpcState.CHASE;
        }

        public override bool InWalkState()
        {
            return state == NpcState.WALK;
        }

        public override bool InSkillState()
        {
            return state == NpcState.CHASE;
        }

        public override bool InMovePlaceHolder()
        {
            // Cos NPC didn't has place holder now
            return false;
        }

        public override void MovingPlaceHolder()
        {
            // Cos NPC didn't has place holder now
        }

        protected override void FinishSkillMove()
        {
            // Cos NPC didn't has place holder now
        }

        public override void Reset()
        {
            state = NpcState.NONE;
        }

        public void ChangeState( NpcState s, Fsm fsm )
        {
            DebugUtils.Assert( fsm != null, "Can't translate to a null npc state!" );

            currentFsm.OnExit();
            currentFsm = fsm;
            state = s;

            DebugUtils.LogWarning( DebugUtils.Type.AI_Npc, string.Format( "{0} {1} enter {2} state", npcType, id, state ) );
            currentFsm.OnEnter();
        }

        public void FindOppenont()
        {
            LogicUnit unit = FindSoldierMethod( position, chaseArea );

            if ( unit == null )
            {

            }

            if ( unit != null )
            {
                target = unit;
                ChangeState( NpcState.ATTACK, fsmAttack );
            }
        }

        #region Properties
        protected int GetActualHurtValue( AttackPropertyType type, int hurtValue )
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
                DebugUtils.LogError( DebugUtils.Type.AI_Npc, string.Format( "Can't handle this hurtType {0} now!", type ) );
            }

            return value;
        }

        public override int GetSpeedFactor()
        {
            return speedFactor;
        }

        public long GetAttackAera()
        {
            long baseRange = attackRange;
            long npcModelRadius = modelRadius;

            if ( target != null )
            {
                return baseRange + npcModelRadius + target.modelRadius;
            }

            return baseRange + npcModelRadius;
        }
        #endregion

        public override void AddCoin( int coin )
        {
            // NPC do nothing in addCoin()
        }

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            // NPC do nothing in OnKillEnemy()
        }

        public override bool Moveable()
        {
            return false;
        }

        #region register method
        public void RegisterProjectileGenerator( ProjectileGenerator generator )
        {
            GenerateProjectile = generator;
        }

        public void RegisterFindOpponentSoldier( FindSoldierMethod method )
        {
            FindSoldierMethod = method;
        }

        #endregion
       
    }

}
