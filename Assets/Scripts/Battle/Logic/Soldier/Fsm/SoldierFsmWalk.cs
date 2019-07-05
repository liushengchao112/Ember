using UnityEngine;
using UnityEngine.AI;

using BattleAgent;
using Utils;

namespace Logic
{
    public class SoldierFsmWalk : Fsm
    {
        protected Soldier owner;

        public const int WALK2AVOIDANCE = 19;
        public const int AVOIDANCE2WALK = 20;

        public int subState = 0;

        public FixVector3 avoidSpeed = FixVector3.zero;

        public SoldierFsmWalk() { }

        public SoldierFsmWalk( Soldier soldier ) 
        {
            owner = soldier;
        }

        public override void OnEnter() 
        {
            subState = 0;
            avoidSpeed = owner.speed;

            owner.target = null;
            owner.targetId = 0;
        }

        public override void Update( int deltaTime )
        {
            owner.WaypointHandler();

            FixVector3 position = owner.position;
            PathAgent agent = owner.pathAgent;

            FixVector3 d = owner.speed * deltaTime;
            FixVector3 newPosition = position + d * FixVector3.PrecisionFactor * FixVector3.PrecisionFactor * FixVector3.PrecisionFactor;
            FixVector3 avoidance = owner.CalculateAvoidance( owner, newPosition );

            if ( avoidance != FixVector3.zero && subState != WALK2AVOIDANCE )
            {
                DebugUtils.LogWarning( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} enters WALK2AVOIDANCE.", owner.id ) );
                subState = WALK2AVOIDANCE;
                DebugUtils.Log( DebugUtils.Type.Avoidance, string.Format( "WALK : soldier {0} has received an avoidance ({1}, {2}, {3}), he's speed is ({4}, {5}, {6})!", owner.id, avoidance.x, avoidance.y, avoidance.z, owner.speed.x, owner.speed.y, owner.speed.z ) );
                avoidSpeed = d + avoidance * deltaTime;
                FixVector3 pos = position + avoidSpeed * FixVector3.PrecisionFactor * FixVector3.PrecisionFactor;
                bool pass = agent.DirectPassToPosition( pos, NavMesh.AllAreas );
                if ( pass )
                {
                    agent.Move( avoidSpeed );
                    owner.stateListener.PostMoveDistanceChanged( avoidance.magnitude );
                    DebugUtils.Log( DebugUtils.Type.Avoidance, string.Format( "WALK : soldier {0} has received an avoidance and move to ({1}, {2}, {3})!", owner.id, owner.position.x, owner.position.y, owner.position.z ) );
                }
                else
                {
                    //still here.
                    DebugUtils.Log( DebugUtils.Type.Avoidance, string.Format( "WALK : soldier {0} has stopped when avoiding others!", owner.id ) );
                }
            }
            else
            {
                if ( subState == WALK2AVOIDANCE )
                {
                    if ( avoidance != FixVector3.zero )
                    {
                        avoidSpeed = d + avoidance * deltaTime;
                        FixVector3 pos = position + avoidSpeed * FixVector3.PrecisionFactor * FixVector3.PrecisionFactor;
                        //bool pass = agent.DirectPassToPosition( pos, NavMesh.AllAreas );
                        if ( true )//pass )
                        {
                            //agent.Move( avoidSpeed );
                            agent.ToughMove( avoidSpeed );
                            DebugUtils.Log( DebugUtils.Type.Avoidance, string.Format( "WALK : soldier {0} has received an avoidance and wants to move to ({1}, {2}, {3}), then his position is ({4}, {5}, {6})!", owner.id, pos.x, pos.y, pos.z, owner.position.x, owner.position.y, owner.position.z ) );
                        }
                        else
                        {
                            //still here.
                            DebugUtils.Log( DebugUtils.Type.Avoidance, string.Format( "WALK : soldier {0} has stopped when avoiding others!", owner.id ) );
                        }
                    }
                    else
                    {
                        DebugUtils.LogWarning( DebugUtils.Type.AI_Soldier, string.Format( "soldier {0} enters AVOIDANCE2WALK.", owner.id ) );
                        subState = AVOIDANCE2WALK;

                        agent.Move( d );
                        owner.stateListener.PostMoveDistanceChanged( avoidance.magnitude );
                    }
                }
                else
                {
                    agent.Move( d );
                    owner.stateListener.PostMoveDistanceChanged( avoidance.magnitude );
                }

                owner.stateListener.PostMoveDistanceChanged( d.magnitude );
            }
        }
    }
}
