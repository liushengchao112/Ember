 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleAgent;
using Constants;
using Utils;
using Data;

namespace Logic
{
    public class YueGuangChongCiSkill : Skill
    {
        private enum State
        {
            None,
            Fire,
            Sprint,
            DashStart,
            Dash,
            DashEnd,
            ReleaseEnd,
        }

        private State state;

        private int tickTimer;
        private int tickTimerLimit = -1;
        private State waitState;
        private FixVector3 targetPosition;
        private FixVector3 deltaSpeed;
        private int stepLength;
        private int dashSpeedFactor = Mathf.RoundToInt( 32 * GameConstants.LOGIC_FIXPOINT_PRECISION );
        private bool waitingChangeState = false;

        public override void Fire()
        {
            state = State.Fire;
            base.Fire();

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SoldierWalk;
            rm.ownerId = owner.id;
            rm.direction = owner.direction.vector3;
            PostRenderMessage( rm );

            owner.FindChasePath( owner.target.position );
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            if ( state == State.Sprint )
            {
                SprintState( deltaTime );
            }
            else if ( state == State.DashStart )
            {
                if ( !waitingChangeState )
                {
                    waitingChangeState = true;
                    DashStart( deltaTime );
                }
                else
                {
                    Moving( deltaTime );
                }
            }
            else if ( state == State.Dash )
            {
                if ( !waitingChangeState )
                {
                    waitingChangeState = true;
                    Dash();
                }
                else
                {
                    Moving( deltaTime );
                }
            }
            else if ( state == State.DashEnd )
            {
                if ( !waitingChangeState )
                {
                    waitingChangeState = true;
                    DashEnd();
                }
            }
            else if ( state == State.ReleaseEnd )
            {
                ReleaseEnd();
            }

            Tick( deltaTime );
        }

        private void SprintState( int deltaTime )
        {
            LogicUnit target = owner.target;
            FixVector3 position = owner.position;
            PathAgent agent = owner.pathAgent;

            if ( target != null && target.Alive() && target.id == owner.target.id )
            {
                if ( FixVector3.SqrDistance( target.position, owner.targetPosition ) > GameConstants.BEGINCHASE_DISTANCE ) // 5f is a testing distance
                {
                    // if target leave the position too far, need to refresh chase path
                    owner.targetPosition = target.position;
                    owner.FindChasePath( owner.target.position );
                    return;
                }

                if ( TargetWithInAttackArea() )
                {
                    if ( target != null && target.Alive() && target.id == owner.target.id )
                    {

                        if ( playerOwner )
                        {
                            FixVector3 direction = ( target.position - position ).normalized;
                            FixVector3 destination = owner.position + direction * 5f;// Temp distance.

                            FixVector3 hitPosition = FixVector3.zero;
                            FixVector3 simpleOnMapPoint = FixVector3.zero;

                            // mapping destination on navmesh
                            bool sampleResult = agent.SamplePosition( destination, out simpleOnMapPoint );
                            if ( sampleResult )
                            {
                                destination = simpleOnMapPoint;
                            }

                            bool result = agent.Raycast( destination, out hitPosition );
                            if ( result )
                            {
                                destination = hitPosition;
                            }

                            DataManager clientData = DataManager.GetInstance();

                            UpdateC2S message = new UpdateC2S();
                            message.timestamp = clientData.GetFrame();
                            Operation op = new Operation();
                            op.playerId = clientData.GetPlayerId();
                            op.unitId = id;
                            op.targetId = owner.id; // test 
                            op.unitMetaId = metaId; // test 
                            op.opType = OperationType.SyncSkillTargetPosition; // wrong type
                            op.x = destination.vector3.x;
                            op.y = destination.vector3.y;
                            op.z = destination.vector3.z;
                            message.operation = op;
                            PostBattleMessage( MsgCode.UpdateMessage, message );

                            DebugUtils.Log( DebugUtils.Type.AI_Skill, string.Format( "YueGuangChongCi sync destination : {0}", destination ) );
                        }
                    }
                    else
                    {
                        Stop();
                    }
                }
                else
                {
                    if ( !owner.CurrentPathAlreadyFinished() )
                    {
                        owner.WaypointHandler();
                        FixVector3 d = owner.speed * deltaTime;
                        agent.Move( d );
                    }
                    else
                    {
                        // wait for chase path
                    }
                }
            }
            else
            {
                // Skill will be shut down when target death.
                Stop();
                owner.Idle();
            }
        }

        public override void SyncMessageHandler( FixVector3 position )
        {
            targetPosition = position;

            DebugUtils.Log( DebugUtils.Type.AI_Skill, "YueGuangChongCi begin dash" );
            state = State.DashStart;
        }


        private void DashStart( int deltaTime )
        {
            FixVector3 direction = ( targetPosition - owner.position ).normalized;
            deltaSpeed = direction * deltaTime * dashSpeedFactor;// temp speed 
            stepLength = deltaSpeed.magnitude;

            DebugUtils.Log( DebugUtils.Type.AI_Skill, string.Format( "YueGuangChongCi destination : {0}, current position: {1} stepLength = {2}", targetPosition, owner.position, stepLength ) );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SoldierDash;
            rm.ownerId = owner.id;
            rm.direction = direction.vector3;
            rm.arguments.Add( "state", 1 ); // Start Dash
            PostRenderMessage( rm );
        }

        private void Dash()
        {
            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SoldierDash;
            rm.ownerId = owner.id;
            rm.arguments.Add( "state", 2 ); // Start Dash
            PostRenderMessage( rm );
        }

        private void DashEnd()
        {
            // play end animation
            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SoldierDash;
            rm.ownerId = owner.id;
            rm.arguments.Add( "state", 3 ); // Start Dash
            PostRenderMessage( rm );

            DelayChangeState( GameConstants.UNIT_DASHEND_DURATION, State.ReleaseEnd );
        }

        private void DelayChangeState( int limitTime, State targetState )
        {
            if ( tickTimerLimit == -1 )
            {
                tickTimerLimit = limitTime;
                waitState = targetState;
            }
        }

        private void Tick( int deltaTime )
        {
            if ( tickTimerLimit == -1 )
            {
                return;
            }

            if ( tickTimer < tickTimerLimit )
            {
                tickTimer += deltaTime;
            }
            else
            {
                state = waitState;
                tickTimerLimit = -1;
                tickTimer = 0;
                waitingChangeState = false;
                DebugUtils.Log( DebugUtils.Type.AI_Skill, string.Format( "YueGuangChongCi: id = {0} tick end, change state to {1}", id, state ) );
            }
        }

        private void Moving( int deltaTime )
        {
            PathAgent agent = owner.pathAgent;
            agent.Move( deltaSpeed );

            long distance = FixVector3.SqrDistance( targetPosition, owner.position );

            if ( distance <= FixVector3.one.magnitude )
            {
                state = State.DashEnd;
                waitingChangeState = false;

                LogicUnit target = owner.target;

                if ( target != null && target.Alive() && target.id == owner.target.id )
                {
                    AttributeEffect ae = GenerateAttributeEffect( attributeEffects[0] );
                    ae.Attach( owner, target );
                }

                DebugUtils.Log( DebugUtils.Type.AI_Skill, string.Format( "YueGuangChongCi: arrive target position, change state to {0}", state ) );
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.AI_Skill, string.Format( "YueGuangChongCi: distance = {0}, step = {1}", distance, stepLength ) );
            }
        }

        public bool TargetWithInAttackArea()
        {
            long distance = FixVector3.SqrDistance( owner.target.position, owner.position );
            long attackDistance = owner.GetAttackArea();

            return distance < attackDistance;
        }

        public override void UnitFoundChasePath()
        {
            state = State.Sprint;
        }

        public override void UnitFinishedMove()
        {
            LogicUnit target = owner.target;

            if ( target != null && target.Alive() )
            {
                if ( TargetWithInAttackArea() )
                {
                    state = State.DashStart;
                }
                else
                {
                    owner.FindChasePath( target.position );
                }
            }
            else
            {
                ReleaseEnd();
            }
        }

        public override bool DependOnSkillState()
        {
            return true;
        }

        public override void Reset()
        {
            base.Reset();

            state = State.None;

            tickTimer = 0;
            tickTimerLimit = -1;
            waitState = State.None;
            targetPosition = FixVector3.zero;
            deltaSpeed = FixVector3.zero;
            stepLength = 0;
            waitingChangeState = false;
        }
    }
}
