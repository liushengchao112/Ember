using UnityEngine;
using UnityEngine.AI;

using BattleAgent;
using Constants;
using Utils;

namespace Logic
{
    public class SoldierFsmChase : Fsm
    {
        protected Soldier owner;

        public const int CHASE2AVOIDANCE = 1;
        public const int AVOIDANCE2CHASE = 2;

        public int subState = 0;

        public FixVector3 avoidSpeed = FixVector3.zero;

        public SoldierFsmChase() { }

        public SoldierFsmChase( Soldier soldier )
        {
            owner = soldier;
        }

        public override void OnEnter() 
        {
            subState = 0;
            avoidSpeed = owner.speed;

            if ( owner.stateListener.PostChaseStateChanged( true ) )
            {
                return;
            }
        }

        public override void Update( int deltaTime )
        {
            int state = owner.state;

            DebugUtils.Assert( state == SoldierState.CHASING, string.Format( "Soldier {0} is in state {1} when updating in FsmChase!", owner.id, state ) );

            LogicUnit target = owner.target;
            FixVector3 position = owner.position;
            PathAgent agent = owner.pathAgent;

            if( target != null && target.Alive() && target.id == owner.targetId )
            {
                long mag = FixVector3.SqrDistance( target.position, owner.targetPosition );
                //Debug.LogWarning( string.Format( "current position:{0} record position:{1} length:{2}", target.position, owner.targetPosition, mag ) );

                if ( mag > GameConstants.BEGINCHASE_DISTANCE ) // 5f is a testing distance
                {
                    //Debug.Log( string.Format( "current position:{0} record position:{1} length:{2}", target.position, owner.targetPosition, mag ) );
                    owner.Chase( owner.target );
                    return;
                }

                owner.WaypointHandler();

                FixVector3 d = owner.speed * deltaTime;
                FixVector3 newPosition = position + d * FixVector3.PrecisionFactor * FixVector3.PrecisionFactor * FixVector3.PrecisionFactor;
                FixVector3 avoidance = owner.CalculateAvoidance( owner, newPosition );

                if( avoidance != FixVector3.zero && subState != CHASE2AVOIDANCE )
                {
                    DebugUtils.LogWarning( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} enters CHASE2AVOIDANCE.", owner.id ) );
                    subState = CHASE2AVOIDANCE;
                    DebugUtils.Log( DebugUtils.Type.Avoidance, string.Format( "CHASE : soldier {0} has received an avoidance ({1}, {2}, {3}), he's speed is ({4}, {5}, {6})!", owner.id, avoidance.x, avoidance.y, avoidance.z, owner.speed.x, owner.speed.y, owner.speed.z ) );
                    avoidSpeed = d + avoidance * deltaTime;
                    FixVector3 pos = position + avoidSpeed * FixVector3.PrecisionFactor * FixVector3.PrecisionFactor;
                    bool pass = agent.DirectPassToPosition( pos, NavMesh.AllAreas );
                    if( pass )
                    {
                        agent.Move( avoidSpeed );
                        DebugUtils.Log( DebugUtils.Type.Avoidance, string.Format( "CHASE : soldier {0} has received an avoidance and moved to ({1}, {2}, {3})!", owner.id, owner.position.x, owner.position.y, owner.position.z ) );
                    }
                    else
                    {
                        //still here.
                        DebugUtils.Log( DebugUtils.Type.Avoidance, string.Format( "CHASE : soldier {0} has stopped when avoiding others!", owner.id ) );
                    }
                }
                else
                {
                    if( subState == CHASE2AVOIDANCE )
                    {
                        if( avoidance != FixVector3.zero )
                        {
                            avoidSpeed = d + avoidance * deltaTime;
                            //Vector3 pos = position + avoidSpeed * FixVector3.PrecisionFactor * FixVector3.PrecisionFactor;
                            //bool pass = agent.DirectPassToPosition( pos, NavMesh.AllAreas );
                            if ( true )//pass )
                            {
                                agent.ToughMove( avoidSpeed );
                                DebugUtils.Log( DebugUtils.Type.Avoidance, string.Format( "CHASE : soldier {0} has received an avoidance and moved to ({1}, {2}, {3})!", owner.id, owner.position.x, owner.position.y, owner.position.z ) );
                            }
                            else
                            {
                                //still here.
                                DebugUtils.Log( DebugUtils.Type.Avoidance, string.Format( "CHASE : soldier {0} has stopped when avoiding others!", owner.id ) );
                            }
                        }
                        else
                        {
                            DebugUtils.LogWarning( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} enters AVOIDANCE2CHASE.", owner.id ) );
                            subState = AVOIDANCE2CHASE;

                            agent.Move( d );
                        }
                    }
                    else
                    {
                        agent.Move( d );
                    }
                }

                position = owner.position;
                FixVector3 v = target.position;
                long distance = FixVector3.SqrDistance( target.position, position );
                long attackDistance = owner.GetAttackArea();

                if( distance < attackDistance )
                {
                    owner.Attack( owner.target );
                }
                /*
                else if( distance > chaseArea * chaseArea * 1.5f )
                {
                    DebugUtils.LogWarning( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0}'s target {1} has escaped! distance = {2}, chaseArea * chaseArea * 1.5f = {3}", id, target.id, distance, chaseArea * chaseArea * 1.5f ) );
                    //Idle();
                    Walk( destination );
                }
                */

                if ( owner.stateListener.PostMoveDistanceChanged( d.magnitude ) )
                {
                    return;
                }

                owner.stateListener.PostChasingStateChanged( distance );
            }
            else
            {
                owner.Idle();
                //Walk( destination );
            }
        }

    }
}
