using UnityEngine;

using Data;
using Constants;
using Utils;

namespace Logic
{
    public class SoldierFsmFight : Fsm
    {
        protected enum FightState
        {
            None = 0,
            StartSwingPoint,
            FrontSwing,
            HitPoint,
            BackSwing,
        }

        protected FightState fightState;
        protected FightState lastState;
        protected Soldier owner;
        protected bool isCrit = false;

        private bool currentFightFristFrame = true;
        private int fightDurationTimer = 0;
        private int hitTime;
        private int fightDuration = 1;
        private int fightInterval = 0;
        private int fightIntervalTimer = 0;
        private bool inFightInterval = false;

        public SoldierFsmFight() { }

        public SoldierFsmFight( Soldier soldier ) 
        {
            owner = soldier;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            owner.ClearPathRender();

            fightState = FightState.StartSwingPoint;
            fightInterval = 0;
            isCrit = false;
            inFightInterval = false;
            fightDurationTimer = 0;
        }

        public override void Update( int deltaTime )
        {
            LogicUnit target = owner.target;
            FixVector3 position = owner.position;

            if ( target != null && target.Alive() && owner.target.id == owner.targetId )
            {
                if ( inFightInterval )
                {
                    // Timing interval
                    fightIntervalTimer += deltaTime;

                    if ( fightIntervalTimer >= fightInterval )
                    {
                        // Enter fight
                        fightState = FightState.StartSwingPoint;
                        fightIntervalTimer = 0;
                        fightInterval = 0;
                        inFightInterval = false;
                    }
                }
                else
                {
                    fightState = GetCurrentState( fightDurationTimer );
                    fightDurationTimer += deltaTime;

                    if ( fightState == FightState.StartSwingPoint )
                    {
                        DebugUtils.Assert( owner.target.id == owner.targetId, string.Format( "Fight Status : the soldier {0}'s targetId = {1}, but its target's id = {2}!", owner.id, owner.targetId, owner.target.id ) );

                        if ( owner.stateListener.PostFightEvent() )
                        {
                            // Trigger skill, will interrupt fight.
                            return;
                        }
                        else
                        {
                            // Save the crit attack performance code
                            isCrit = Formula.TriggerCritical( owner.GetRandomNumber(), owner.GetCriticalChance() );
                            //if ( isCrit )
                            //{
                            //    hitTime = owner.critHitTime;
                            //    fightDuration = owner.critDuration;
                            //}
                            //else
                            {
                                hitTime = owner.attackHitTime;
                                fightDuration = owner.attackDuration;
                            }

                            // Reset direction at attack begin.
                            FixVector3 direction = target.position - position;
                            owner.direction = direction;
                            if ( owner.standardAttackType == AttackDistanceType.Melee )
                            {
                                RenderMessage rm = new RenderMessage();
                                rm.ownerId = owner.id;
                                rm.direction = direction.vector3;
                                rm.type = RenderMessage.Type.SoldierAttack;
                                rm.arguments.Add( "isCrit", false );
                                rm.arguments.Add( "intervalRate", 1 );
                                owner.PostRenderMessage( rm );
                            }
                            else
                            {
                                RenderMessage rm = new RenderMessage();
                                rm.type = RenderMessage.Type.SoldierSpawnProjectile;
                                rm.ownerId = owner.id;
                                rm.direction = direction.vector3;
                                rm.arguments.Add( "projectileMetaId", owner.projectileId );
                                rm.arguments.Add( "intervalRate", 1 );
                                owner.PostRenderMessage( rm );
                            }
                        }
                    }
                    else if ( fightState == FightState.HitPoint )
                    {
                        owner.Fight( isCrit );
                        owner.stateListener.PostUnitFightAfter();
                    }
                    else if ( fightState == FightState.BackSwing )
                    {
                        if ( fightDurationTimer >= fightDuration )
                        {
                            fightDurationTimer = 0;

                            fightInterval = owner.GetAttackInterval() - fightDuration;
                            inFightInterval = true;
                        }
                    }
                }

                long distance = FixVector3.SqrDistance( target.position, owner.position );
                long attackDistance = owner.GetAttackArea();

                if ( distance >= attackDistance )
                {
                    if ( ( target.position - owner.targetPosition ).magnitude > attackDistance )
                    {
                        owner.Chase( target );
                    }
                    else
                    {
                        // continue fight
                    }
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

            if (f == 0)
            {
                result = FightState.StartSwingPoint;
            }
            else if ( f < hitTime )
            {
                result = FightState.FrontSwing;
            }
            else if ( f >= hitTime && lastState == FightState.FrontSwing )
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
