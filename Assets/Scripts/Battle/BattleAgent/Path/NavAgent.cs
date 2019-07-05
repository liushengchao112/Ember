/*----------------------------------------------------------------
// Copyright (C) 2017 Jiawen(Kevin)
//
// file name: NavAgent.cs
// description: 
// 
// created time：06/16/2017
//
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Data;
using Logic;
using Constants;
using Utils;

namespace BattleAgent
{
    public class NavAgent : PathAgent
    {
        public static NavMeshSurface groundSurface;
        public static NavMeshSurface flySurface;

        public NavMeshAgent agent;
        public NavMeshQueryFilter filter;

        public new static void Initialize()
        {
            GameObject ground = GameObject.Find( "NavMeshSurface/Ground" );
            if( ground != null )
            {
                groundSurface = ground.GetComponent<NavMeshSurface>();
            }

            GameObject fly = GameObject.Find( "NavMeshSurface/Fly" );
            if( fly != null )
            {
                flySurface = fly.GetComponent<NavMeshSurface>();
            }
        }

        public NavAgent( LogicUnit owner ) : base( PathAgentType.NavMesh, owner )
        {
            agent = owner.gameObject.AddComponent<NavMeshAgent>();
            agent.agentTypeID = -1;
            agent.areaMask = NavMeshAreaType.WALKABLE;// Set default areaMask as Walkable;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

            filter = new NavMeshQueryFilter();
            filter.agentTypeID = -1;
        }

        public override void SetPathType( PathType pathType )
        {
            DebugUtils.Log( DebugUtils.Type.PathFinding, String.Format( "logic uint {0}'s path type = {1}.", owner.id, pathType ) );

            if( pathType == PathType.Ground && groundSurface != null )
            {
                agent.agentTypeID = groundSurface.agentTypeID;
                filter.agentTypeID = groundSurface.agentTypeID;
            }
            else if( pathType == PathType.Flying && flySurface != null )
            {
                agent.agentTypeID = flySurface.agentTypeID;
                filter.agentTypeID = flySurface.agentTypeID;
            }
            else
            {
                agent.agentTypeID = -1;
                filter.agentTypeID = -1;
                DebugUtils.LogError( DebugUtils.Type.PathFinding, String.Format( "NavAgent : The path {0} type doesn't exist, or the ground/fly Surface is null!", pathType ) );
            }
        }

        public override void SetPosition( FixVector3 pos )
        {
            agent.transform.position = pos.vector3;
            agent.Warp( pos.vector3 );
        }

        public override void FindPath( FixVector3 startPosition, FixVector3 destination, int areaMask, OnPathFoundHandler handler )
        {
            NavMeshPath p = new NavMeshPath();
            filter.areaMask = areaMask;
            agent.areaMask = areaMask;
            NavMesh.CalculatePath( startPosition.vector3, destination.vector3, filter, p );

            DebugUtils.Assert( p.corners != null, string.Format( "the path for soldier {0} is null!", owner.id ) );
            DebugUtils.Assert( p.corners.Length > 0, string.Format( "the path for soldier {0} is 0 length!", owner.id ) );

            List<FixVector3> path = new List<FixVector3>();
            for ( int i = 0; i < p.corners.Length; i++ )
            {
                path.Add( new FixVector3( p.corners[i] ) );
            }

            handler( path );
        }

        public override bool DirectPassToPosition( FixVector3 targetPosition, int areaMask )
        {
            NavMeshPath p = new NavMeshPath();
            filter.areaMask = 1;
            NavMesh.CalculatePath( owner.position.vector3, targetPosition.vector3, filter, p );
            return p != null && p.corners.Length == 2 && p.status == NavMeshPathStatus.PathComplete;
        }

        public override void Move( FixVector3 speed )
        {
            // direction * speedfactor * deltaTime 
            Vector3 s = speed.vector3 * FixVector3.PrecisionFactor * FixVector3.PrecisionFactor;
            agent.Move( s );
            owner.position = (FixVector3)owner.transform.position;
        }

        public override void ToughMove( FixVector3 speed )
        {
            FixVector3 s = speed * FixVector3.PrecisionFactor * FixVector3.PrecisionFactor;
            owner.position += s;
            agent.Warp( owner.position.vector3 );
        }

        public override bool Raycast( FixVector3 targetPosition, out FixVector3 hitPosition )
        {
            NavMeshHit hit;
            bool result = NavMesh.Raycast( owner.position.vector3, targetPosition.vector3, out hit, NavMesh.AllAreas );
            if ( result )
            {
                // hit obstruction...
                hitPosition = (FixVector3)hit.position;
            }
            else
            {
                // hit nothing...
                hitPosition = (FixVector3)Vector3.zero;
            }

            return result;
        }

        public override bool SamplePosition( FixVector3 sourcePosition, out FixVector3 nearestPoint )
        {
            NavMeshHit hit;
            bool result = NavMesh.SamplePosition( sourcePosition.vector3, out hit, 2, NavMesh.AllAreas );
            if ( result )
            {
                // hit obstruction...
                nearestPoint = new FixVector3( hit.position );
            }
            else
            {
                // hit nothing...
                nearestPoint = FixVector3.zero;
            }

            return result;
        }
    }
}
