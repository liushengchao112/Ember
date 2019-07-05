using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleAgent;
using Data;
using Utils;
using Constants;

namespace Logic
{
    public abstract class MovableUnit : LogicUnit
    {
        public int speedFactor;
        public FixVector3 destination;
        public FixVector3 direction;
        public FixVector3 speed;
        public PathAgent pathAgent;
        public PathType pathType;

        protected int currentWaypoint;
        protected long lastPathFrame;
        protected List<Position> waypoints;

        // Sync move 
        protected virtual void OnWalkPathComplete( List<FixVector3> path )
        {
            DebugUtils.Log( DebugUtils.Type.PathRender, string.Format( "Current path point count {0}", path.Count ) );

            SyncC2S sync = new SyncC2S();
            sync.uintId = id;
            sync.timestamp = DataManager.GetInstance().GetFrame();
            sync.syncState = SyncState.Walk;

            GetPositionsFromPath( path, sync.positions );

            PostBattleMessage( MsgCode.SyncMessage, sync );

            DebugUtils.LogWarning( DebugUtils.Type.AI_MovableUnit, string.Format( "{0} {1} find Walk path count {1}", id, type, sync.positions.Count ) );
        }

        protected virtual void OnChasePathComplete( List<FixVector3> path )
        {
            SyncC2S sync = new SyncC2S();
            sync.uintId = id;
            sync.timestamp = DataManager.GetInstance().GetFrame();
            sync.syncState = SyncState.Chase;

            GetPositionsFromPath( path, sync.positions );

            PostBattleMessage( MsgCode.SyncMessage, sync );
        }

        public void SyncMessageHandler( long frame, Sync s, List<Position> posList )
        {
            DebugUtils.Assert( posList != null, "MoveableUnit : the new waypoints shouldn't be null!" );
            DebugUtils.Assert( frame != lastPathFrame || s.syncState == SyncState.ContinuedWalk || s.syncState == SyncState.Chase, "two paths result in the same frame " + frame + " force = " + mark + " Id = " + id );

            if ( !Alive() || !Moveable() )
            {
                DebugUtils.Log( DebugUtils.Type.AI_MovableUnit, string.Format( "MoveableUnit {0} is in unmovable state when getting the sync message.", id ) );
            }
            else if ( s.syncState == SyncState.Walk )
            {
                if ( frame > lastPathFrame )
                {
                    WaitWalkHandler( s, posList );
                    lastPathFrame = frame;
                }
            }
            else if ( s.syncState == SyncState.ContinuedWalk )
            {
                if ( frame >= lastPathFrame )
                {
                    WaitWalkHandler( s, posList );
                    lastPathFrame = frame;
                }
            }
            else if ( s.syncState == SyncState.Chase )
            {
                if ( frame > lastPathFrame )
                {
                    waypoints = posList;
                    WaitChaseHandler();
                    lastPathFrame = frame;
                }
            }
            else
            {
                //DebugUtils.LogError( DebugUtils.Type.AI_MovableUnit, string.Format( "the movableUnit {0}'s state is {1} when the sync path message arrives for {2}!", id, state, s ) );
            }
        }

        protected virtual void WaitWalkHandler( Sync sync, List<Position> posList )
        {
            if ( sync.syncState == SyncState.Walk )
            {
                //if( state == SoldierState.IDLE2WALK || state == SoldierState.WAIT2WALK || state == SoldierState.WALK2WALK )
                {
                    DebugUtils.Assert( posList.Count > 0, "MovableUnit : the new waypoints' length should be larger than 0 for a new walk!" );

                    currentWaypoint = 0;
                    waypoints = posList;

                    Position p = waypoints[currentWaypoint];
                    FixVector3 currentWayPointV = new FixVector3( p.x, p.y, p.z );
                    direction = ( currentWayPointV - position ).normalized;
                    speed = direction * GetSpeedFactor();
                }
                //else
                //{
                //    DebugUtils.LogError( DebugUtils.Type.AI_MovableUnit, string.Format( "movable unit {0} receives WALK sync message, but in state {1}!", id, state ) );
                //}
            }
            else if ( sync.syncState == SyncState.ContinuedWalk )
            {
                DebugUtils.Log( DebugUtils.Type.AI_MovableUnit, string.Format( "movable unit {0} receives CONTINUED WALK sync message in state.", id ) );

                if ( sync.continuedWalkNum == 1 )
                {
                    currentWaypoint = 0;
                    waypoints = posList;

                    Position p = waypoints[currentWaypoint];
                    FixVector3 currentWayPointV = new FixVector3( p.x, p.y, p.z );
                    direction = ( currentWayPointV - position ).normalized;
                    speed = direction * GetSpeedFactor();
                }
                else
                {
                    waypoints.AddRange( posList );
                }
            }
        }

        protected virtual void WaitChaseHandler()
        {
            currentWaypoint = 0;

            Position p = waypoints[currentWaypoint];
            FixVector3 currentWayPointV = new FixVector3( p.x, p.y, p.z );
            direction = ( currentWayPointV - position ).normalized;
            speed = direction * GetSpeedFactor();
        }

        // Handler way point per frame
        public void WaypointHandler()
        {
            DebugUtils.Assert( waypoints != null, type.ToString() + id + "' waypoints is null!" );
            DebugUtils.Assert( waypoints.Count > 0, type.ToString() + id + "' waypoints' count is 0!" );
            DebugUtils.Assert( waypoints.Count > currentWaypoint, type.ToString() + id + "' currentWaypoint " + currentWaypoint + " is larger than waypoints' count!" + waypoints.Count );

            //DebugUtils.Log( DebugUtils.Type.AI_MovableUnit, string.Format( "waypointhandler1 : movableUnit {0}'s speed = ({1}, {2}, {3}).", id, speed.x, speed.y, speed.z ) );
            Position p = waypoints[currentWaypoint];
            FixVector3 vp = new FixVector3( p.x, p.y, p.z );
            FixVector3 v = vp - position;
            int d = v.magnitude;

            if ( d <= GameConstants.EQUAL_DISTANCE )
            {
                DebugUtils.LogWarning( DebugUtils.Type.AI_MovableUnit, "movableUnit " + id + " has reached way point " + currentWaypoint );

                ++currentWaypoint;

                if ( currentWaypoint == waypoints.Count )
                {
                    if ( InWalkState() )
                    {
                        DebugUtils.LogWarning( DebugUtils.Type.AI_MovableUnit, string.Format( "movableUnit {0} finished walk", id ) );
                        FinishedWalk();
                    }
                    else if ( InChaseState() )
                    {
                        DebugUtils.LogWarning( DebugUtils.Type.AI_MovableUnit, string.Format( "movableUnit {0} finished chase", id ) );
                        FinishedChase();
                    }
                    else if ( InSkillState() )
                    {
                        DebugUtils.LogWarning( DebugUtils.Type.AI_MovableUnit, string.Format( "movableUnit {0} finished skill move", id ) );
                        FinishSkillMove();
                    }
                    else if ( InMovePlaceHolder() ) // there is no CHASE2WALK for now.
                    {
                        DebugUtils.LogWarning( DebugUtils.Type.AI_MovableUnit, string.Format( "movableUnit {0} finished move but in place holder state", id ) );
                        MovingPlaceHolder();
                    }
                    else
                    {
                        DebugUtils.LogError( DebugUtils.Type.AI_MovableUnit, string.Format( "It's strange! movableUnit {0}'s state is unknowstate when entering WaypointHandler!", id ) );
                    }
                }
                /*
                else if( currentWaypoint == waypoints.Count - 1 )
                {
                    OnTargetClosed();
                }
                */
                else
                {
                    p = waypoints[currentWaypoint];
                    vp = new FixVector3( p.x, p.y, p.z );
                    v = vp - position;
                    d = v.magnitude;
                    if ( d > GameConstants.EQUAL_DISTANCE )
                    {
                        //DebugUtils.LogWarning( DebugUtils.Type.AI_MovableUnit, string.Format( "movableUnit {0}'s next way point is ({1}, {2}, {3}).", id, vp.x, vp.y, vp.z ) );
                        direction = v.normalized;
                        speed = direction * GetSpeedFactor();
                        //DebugUtils.Log( DebugUtils.Type.AI_MovableUnit, string.Format( "waypointhandler2 : movableUnit {0}'s desiredVelocity = ({1}, {2}, {3}).", id, speed.x, speed.y, speed.z ) );
                    }
                    else
                    {
                        DebugUtils.LogWarning( DebugUtils.Type.AI_MovableUnit, string.Format( "AI_MovableUnit {0}'s {1} way point is too close! distance = {2}.", id, currentWaypoint, d ) );
                    }
                }
            }
            else
            {
                direction = v.normalized;
                speed = direction * GetSpeedFactor();
            }
        }

        public bool CurrentPathAlreadyFinished()
        {
            return waypoints.Count <= currentWaypoint;
        }

        protected void GetPositionsFromPath( List<FixVector3> path, List<Position> posList )
        {
            if ( path.Count > 0 )
            {
                int j = 0;
                long distance = FixVector3.SqrDistance( path[j], position );

                if ( distance <= GameConstants.EQUAL_DISTANCE )
                {
                    DebugUtils.LogWarning( DebugUtils.Type.PathFinding, string.Format( "movable unit {0}'s first way point is too close! distance = {1}", id, distance ) );
                    j = 1;
                }

                for ( ; j < path.Count; j++ )
                {
                    Position pos = new Position();
                    FixVector3 v = path[j];
                    //DebugUtils.LogWarning( DebugUtils.Type.AI_MovableUnit, string.Format( "movable unit {0}'s {1} way point is ({2}, {3}, {4})", id, j, v.x, v.y, v.z ) );
                    pos.x = v.vector3.x;
                    pos.y = v.vector3.y;
                    pos.z = v.vector3.z;
                    posList.Add( pos );
                }
            }
        }

        public abstract bool Moveable();

        public abstract Boolean InWalkState();

        public abstract Boolean InChaseState();

        public abstract Boolean InMovePlaceHolder();

        public abstract Boolean InSkillState();

        protected abstract void FinishedWalk();

        protected abstract void FinishedChase();

        protected abstract void FinishSkillMove();

        public abstract void MovingPlaceHolder();

        public abstract int GetSpeedFactor();

        // Base class abstract method
        public override void LogicUpdate( int deltaTime )
        {
            DebugUtils.Assert( false, "Movable unit didn't implement LogicUpdate()" );
        }

        public override void Hurt( int hurtValue, AttackPropertyType hurtType, bool isCrit, LogicUnit injurer )
        {
            DebugUtils.Assert( false, "Movable unit didn't implement Hurt()" );
        }

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            DebugUtils.Assert( false, "Movable unit didn't implement OnKillEnemy()" );
        }

        public override void AddCoin( int coin )
        {
            DebugUtils.Assert( false, "Movable unit didn't implement AddCoin()" );
        }

        public override bool Alive()
        {
            DebugUtils.Assert( false, "Movable unit didn't implement Alive()" );
            return true;
        }

        public override void Reset()
        {
            currentWaypoint = 0;

            if ( waypoints != null )
            {
                waypoints.Clear();
                waypoints = null;
            }

            DebugUtils.Assert( false, "Movable unit didn't implement Reset()" );
        }
    }
}
