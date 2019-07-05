using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Constants;
using Data;
using Utils;

namespace Logic
{
    public class WildMonsterFsmAttack : Fsm
    {
        protected enum FightState
        {
            None = 0,
            StartSwingPoint,
            FrontSwing,
            HitPoint,
            BackSwing,
        }

        public Npc owner;

        protected FightState state;
        protected FightState lastState;

        private int fightInterval;
        private int fightIntervalTimer;
        private int fightDuration;
        private int fightDurationTimer;
        private int fightHitTime;
        private bool inFightInterval ;

        public WildMonsterFsmAttack( Npc n )
        {
            owner = n;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            inFightInterval = false;
            fightIntervalTimer = 0;
            fightDurationTimer = 0;
            fightInterval = owner.fightInterval;
            fightDuration = owner.attackDuration;
            fightHitTime = owner.attackHitTime;
            state = FightState.StartSwingPoint;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            if ( owner.target != null && owner.target.Alive() )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Npc, string.Format( "{0} {1} begin to attack target {2}", owner.npcType, owner.id, owner.target.id ) );
                if ( inFightInterval )
                {
                    fightIntervalTimer += deltaTime;

                    if ( owner.fightTimer >= owner.fightInterval )
                    {
                        state = FightState.StartSwingPoint;
                        fightIntervalTimer = 0;
                        fightInterval = 0;
                        inFightInterval = false;
                    }
                }
                else
                {
                    state = GetCurrentState( fightDurationTimer );
                    fightDurationTimer += deltaTime;

                    if ( state == FightState.StartSwingPoint )
                    {
                        FixVector3 direction = owner.target.position - owner.position;
                        owner.direction = direction.normalized;

                        RenderMessage rm = new RenderMessage();
                        rm.ownerId = owner.id;
                        rm.direction = owner.direction.vector3;
                        rm.type = RenderMessage.Type.NpcFight;
                        rm.arguments.Add( "intervalRate", 1 );
                        owner.PostRenderMessage( rm );
                    }
                    else if ( state == FightState.HitPoint )
                    {
                        owner.target.Hurt( owner.damage, AttackPropertyType.PhysicalAttack, false, owner );

                        RenderMessage rm = new RenderMessage();
                        rm.ownerId = owner.id;
                        rm.type = RenderMessage.Type.NpcFightHit;
                        owner.PostRenderMessage( rm );
                    }
                    else if ( state == FightState.BackSwing )
                    {
                        if ( fightDurationTimer >= fightDuration )
                        {
                            fightDurationTimer = 0;

                            fightInterval = owner.fightInterval - fightDuration;
                            inFightInterval = true;
                        }
                    }
                }
            }
            else
            {
                if ( FixVector3.SqrDistance( owner.position, owner.brithPosition ) > GameConstants.EQUAL_DISTANCE )
                {
                    owner.Walk( owner.brithPosition );
                }
                else
                {
                    owner.ChangeState( NpcState.IDLE, owner.fsmIdle );
                }
            }

            long distance = FixVector3.SqrDistance( owner.target.position, owner.position );
            long attackDistance = owner.GetAttackAera();

            DebugUtils.LogWarning( DebugUtils.Type.AI_Npc, string.Format( "distance = {0}, attackDis = {1}", distance, attackDistance ) );
            if ( distance < ( attackDistance ) )
            {
                // continue fight..
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.AI_Npc, string.Format( "{0} {1}'s target out of range, begin to chase  {2}", owner.npcType, owner.id, owner.target.id ) );
                owner.Chase( owner.target );
            }
        }

        private FightState GetCurrentState( int f )
        {
            FightState result;

            if ( f == 0 )
            {
                result = FightState.StartSwingPoint;
            }
            else if ( f < fightHitTime )
            {
                result = FightState.FrontSwing;
            }
            else if ( f >= fightHitTime && lastState == FightState.FrontSwing )
            {
                result = FightState.HitPoint;
            }
            else
            {
                result = FightState.BackSwing;
            }

            lastState = result;
            return result;
        }
    }
}

