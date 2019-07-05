using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Utils;

namespace Logic
{
    public class SummonedUnitFsmFight : Fsm
    {
        protected enum FightState
        {
            None = 0,
            StartSwingPoint,
            FrontSwing,
            HitPoint,
            BackSwing,
            FightEnd,
        }

        private SummonedUnit owner;
        protected FightState fightState;
        protected FightState lastState;

        private int fightIntervalTimer = 0;
        private int fightDurationTimer = 0;
        private int fightInterval = 0;
        private int fightHitTimePoint = 0;
        private int fightDuration = 0;

        private bool inFightInterval = false;

        public SummonedUnitFsmFight( SummonedUnit summon )
        {
            owner = summon;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            fightIntervalTimer = 0;
            fightDurationTimer = 0;
            fightInterval = owner.attackInterval;
            fightHitTimePoint = owner.attackHitTime;
            fightDuration = owner.attackDuration;
            inFightInterval = false;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            LogicUnit target = owner.target;

            if ( target != null && target.Alive() )
            {
                long distance = FixVector3.SqrDistance( target.position, owner.position );
                if ( distance < owner.attackRadius )
                {
                    if ( inFightInterval )
                    {
                        fightIntervalTimer += deltaTime;

                        if ( fightIntervalTimer >= fightInterval )
                        {
                            fightState = FightState.StartSwingPoint;
                            fightIntervalTimer = 0;
                            inFightInterval = false;
                        }
                    }
                    else
                    {
                        fightState = GetCurrentState( fightDurationTimer );
                        fightDurationTimer += deltaTime;

                        if ( fightState == FightState.StartSwingPoint )
                        {
                            owner.direction = target.position - owner.position;

                            RenderMessage rm = new RenderMessage();
                            rm.ownerId = owner.id;
                            rm.direction = owner.direction.vector3;
                            rm.type = RenderMessage.Type.SummonedUnitAttack;
                            owner.PostRenderMessage( rm );
                        }
                        else if ( fightState == FightState.HitPoint )
                        {
                            owner.damage = Formula.GetAttackFloatingValue( owner.physicalAttack, owner.GetRandomNumber(), owner.GetRandomNumber( owner.physicalAttackVar ) );

                            List<LogicUnit> targets = owner.FindOpponent();

                            for ( int i = 0; i < targets.Count; i++ )
                            {
                                LogicUnit t = targets[i];

                                // Check target state and still in the attack range
                                if ( t != null && t.Alive() )
                                {
                                    t.Hurt( owner.damage, AttackPropertyType.PhysicalAttack, false, owner.ownerSoldier );
                                }
                            }
                        }
                        else if ( fightState == FightState.FightEnd )
                        {
                            inFightInterval = true;
                            fightDurationTimer = 0;
                        }
                    }
                }
                else
                {
                    owner.Idle();
                }
            }
            else
            {
                owner.Idle();
            }
        }

        private FightState GetCurrentState( int f )
        {
            FightState result;
            if ( f == 0 )
            {
                result = FightState.StartSwingPoint;
            }
            else if ( f < fightHitTimePoint )
            {
                result = FightState.FrontSwing;
            }
            else if ( f > fightHitTimePoint && lastState == FightState.FrontSwing )
            {
                result = FightState.HitPoint;
            }
            else if ( f >= fightDuration )
            {
                result = FightState.FightEnd;
            }
            else
            {
                result = FightState.BackSwing;
            }

            lastState = result;
            return result;
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
