using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Data;

using NpcData = Data.NpcProto.Npc;

namespace Logic
{
    public enum IdolGuardState
    {
        NONE,
        IDLE,
        ATTACK,
        DYING,
        DEATH
    }

    public class IdolGuard : LogicUnit
    {
        public IdolGuardState state;

        // proto data
        public NpcData npcProto;
        public NpcType npcType;
        public NpcAttackType attackType;
        public int fightInterval;
        public int rebornInterval;
        public int physicasAttack;
        public int armor;
        public int magicResist;
        public float speedFactor;
        public int healthRegen;
        public int attackRange;
        public int chaseArea;
        public int maxChaseDistance;
        public int emberOutPut;
        public int projectileId;
        public AttackPropertyType hurtType;

        // runtime 
        public FixVector3 brithPosition;
        public FixVector3 brithDirection;
        public bool recycled;

        public int fightTimer;
        public int rebornTimer = 0;
        public LogicUnit target;
        public long targetId;

        // fsm
        public Fsm fsmAttack;
        public Fsm fsmDeath;
        public Fsm fsmDying;
        public Fsm fsmIdle;

        // register method
        protected FindSoldierMethod FindSoldierMethod;
        protected ProjectileGenerator GenerateProjectile;

        public void Initialize( long id, NpcData proto, FixVector3 p, FixVector3 r )
        {
            this.id = id;

            type = LogicUnitType.IdolGuard;

            position = p;
            transform.position = p.vector3;
            brithPosition = p;
            brithDirection = r;
            hurtType = AttackPropertyType.PhysicalAttack;

            npcProto = proto;
            metaId = proto.ID;
            modelId = proto.ModelID;
            iconId = -1;
            modelRadius = ConvertUtils.ToLogicInt( proto.ModelRadius );
            npcType = (NpcType)proto.NPCType;
            attackType = (NpcAttackType)proto.StandardAttack;
            rebornInterval = ConvertUtils.ToLogicInt( proto.RebornInterval );
            physicasAttack = (int)proto.PhysicsAttack;
            armor = (int)proto.Armor;
            magicResist = (int)proto.MagicResist;
            speedFactor = proto.Speed;
            healthRegen = proto.HealthRegen;
            maxHp = proto.Health;
            chaseArea = ConvertUtils.ToLogicInt( proto.TargetDetectRange );
            maxChaseDistance = ConvertUtils.ToLogicInt( proto.MaxChaseDistance );
            emberOutPut = proto.EmberOutPut;
            projectileId = 1/*proto.ProjectileId*/;
            attackRange = ConvertUtils.ToLogicInt( proto.AttackRange );
            fightInterval = ConvertUtils.ToLogicInt(1);

            hp = maxHp;
            damage = physicasAttack;

            InitializeState();

            state = IdolGuardState.IDLE;
            currentFsm = fsmIdle;
            ChangeState( IdolGuardState.IDLE, fsmIdle );
        }

        public void InitializeState()
        {
            fsmAttack = new IdolGuardFsmAttack( this );
            fsmIdle = new IdolGuardFsmIdle( this );
            fsmDeath = new IdolGuardFsmDeath( this );
            fsmDying = new IdolGuardFsmDying( this );
        }

        public override void LogicUpdate( int deltaTime )
        {
            currentFsm.Update( deltaTime );
        }

        public void Fight()
        {
            if ( target != null && target.Alive() && target.id == targetId )
            {
                damage = physicasAttack;

                DebugUtils.Log( DebugUtils.Type.AI_Npc, "the " + npcType + " npc " + id + " begins to fire to target " + target.id );
                Projectile projectile = GenerateProjectile( this, projectileId, position, target );
                projectile.RegisterRenderMessageHandler( PostRenderMessage );
                projectile.RegisterDestroyHandler( PostDestroy );

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

        public override void Hurt( int hurtValue, AttackPropertyType hurtType, bool isCrit, LogicUnit injurer )
        {
            if ( Alive() )
            {
                if ( hurtType == AttackPropertyType.PhysicalAttack )
                {
                    int value = GetActualHurtValue( hurtType, hurtValue );
                    hp -= value;
                    RenderMessage message = new RenderMessage();
                    message.type = RenderMessage.Type.IdolGuardHurt;
                    message.ownerId = id;
                    message.position = position.vector3;
                    message.arguments.Add( "value", value );
                    PostRenderMessage( message );

                    if ( hp <= 0 )
                    {
                        injurer.OnKillEnemy( killReward, injurer, this );
                        Dying();
                    }
                }
            }
        }

        public void Dying()
        {
            ChangeState( IdolGuardState.DYING, fsmDying );

            RenderMessage renderMessage = new RenderMessage();
            renderMessage.type = RenderMessage.Type.IdolGuardDeath;
            renderMessage.ownerId = id;

            PostRenderMessage( renderMessage );
        }

        public void Death()
        {
            ChangeState( IdolGuardState.DEATH, fsmDeath );
        }

        public override void Reset()
        {
            target = null;
            state = IdolGuardState.NONE;
            targetId = 0;

            // TODO: add more reset code
        }

        public void Reborn()
        {
            Reset();

            Initialize( id, npcProto, brithPosition, brithDirection );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnIdolGuard;
            rm.ownerId = id;

            rm.direction = brithDirection.vector3;
            rm.position = position.vector3;
            rm.arguments.Add( "maxHp", maxHp );
            rm.arguments.Add( "mark", mark );
            rm.arguments.Add( "modelId", modelId );
            PostRenderMessage( rm );

            DebugUtils.Log( DebugUtils.Type.AI_Npc, string.Format( " {0} {1} has been reborn", npcType, id ) );
        }

        public override void AddCoin( int coin )
        {
            DebugUtils.Assert( false, "Idol Guard didn't implement AddCoin()" );
        }

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            // TODO: Idol Guard will do nothing when kill enemy
        }

        public override bool Alive()
        {
            return state != IdolGuardState.DYING && state != IdolGuardState.DEATH;
        }

        public void ChangeState( IdolGuardState s, Fsm fsm )
        {
            DebugUtils.Assert( fsm != null, "Can't translate to a null npc state!" );

            currentFsm.OnExit();
            currentFsm = fsm;
            state = s;

            DebugUtils.LogWarning( DebugUtils.Type.AI_Npc, string.Format( "{0} {1} enter {2} state", npcType, id, state ) );
            currentFsm.OnEnter();
        }


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

        public void FindOppenont()
        {
            LogicUnit unit = FindSoldierMethod( position, chaseArea );

            if ( unit == null )
            {

            }

            if ( unit != null )
            {
                target = unit;
                targetId = unit.id;
                ChangeState( IdolGuardState.ATTACK, fsmAttack );
            }
        }

        public void RegisterProjectileGenerator( ProjectileGenerator generator )
        {
            GenerateProjectile = generator;
        }

        public void RegisterFindSoldier( FindSoldierMethod method )
        {
            FindSoldierMethod = method;
        }
    }
}

