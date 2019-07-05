using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Data;

using NpcData = Data.NpcProto.Npc;

namespace Logic
{
    public enum IdolState
    {
        NONE,
        IDLE,
        DYING,
        DEATH
    }

    public class Idol : LogicUnit
    {
        public IdolState state;

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
        public float attackRange;
        public float chaseArea;
        public float maxChaseDistance;
        public int emberOutPut;
        public int projectileId;
        public AttackPropertyType hurtType;

        // runtime 
        public FixVector3 brithPosition;
        public FixVector3 brithDirection;
        public bool recycled;
        public int rebornTimer = 0;

        // fsm
        public Fsm fsmDeath;
        public Fsm fsmDying;
        public Fsm fsmIdle;

        public void Initialize( long id, NpcData proto, FixVector3 p, FixVector3 r )
        {
            this.id = id;

            type = LogicUnitType.Idol;

            position = p;
            transform.position = p.vector3;
            brithPosition = p;
            brithDirection = r;
            hurtType = AttackPropertyType.PhysicalAttack;

            npcProto = proto;
            metaId = proto.ID;
            iconId = -1;
            modelId = proto.ModelID;
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
            chaseArea = proto.TargetDetectRange;
            maxChaseDistance = proto.MaxChaseDistance;
            emberOutPut = proto.EmberOutPut;
            projectileId = proto.ProjectileId;
            attackRange = proto.AttackRange;
            killReward = proto.KillReward;
            fightInterval = 1;

            hp = maxHp;
            damage = physicasAttack;

            InitializeState();

            state = IdolState.IDLE;
            currentFsm = fsmIdle;
            ChangeState( IdolState.IDLE, fsmIdle );
        }

        public void InitializeState()
        {
            fsmIdle = new IdolFsmIdle( this );
            fsmDying = new IdolFsmDying( this );
            fsmDeath = new IdolFsmDeath( this );
        }

        public override void LogicUpdate( int deltaTime )
        {
            DebugUtils.Assert( false, " Idol didn't implement logic update method " );
        }

        public override void Hurt( int hurtValue, AttackPropertyType hurtType, bool isCrit, LogicUnit injurer )
        {
            if ( Alive() )
            {
                if ( hurtType == AttackPropertyType.PhysicalAttack)
                {
                    int value = GetActualHurtValue( hurtType, hurtValue );
                    hp -= value;
                    RenderMessage message = new RenderMessage();
                    message.type = RenderMessage.Type.IdolHurt;
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
            ChangeState( IdolState.DYING, fsmDying );

            RenderMessage renderMessage = new RenderMessage();
            renderMessage.type = RenderMessage.Type.IdolDeath;
            renderMessage.ownerId = id;
            PostRenderMessage( renderMessage );

            // TODO: maybe release skill
        }

        public void Death()
        {
            PostDestroy( this );
        }

        public virtual void Reborn()
        {
            Reset();

            Initialize( id, npcProto, brithPosition, brithDirection );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.IdolOut;
            rm.ownerId = id;
            PostRenderMessage( rm );

            DebugUtils.Log( DebugUtils.Type.AI_Npc, string.Format( " {0} {1} has been reborn", npcType, id ) );
        }

        public override void AddCoin( int coin )
        {
            throw new NotImplementedException();
        }

        public override bool Alive()
        {
            return state != IdolState.DEATH && state != IdolState.DYING;
        }

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            DebugUtils.Assert( false, " Idol didn't implement OnKillEnemy() method " );
        }

        public override void Reset()
        {
            DebugUtils.Assert( false, " Idol didn't implement Reset() method " );
        }

        public void ChangeState( IdolState s, Fsm fsm )
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
    }
}
