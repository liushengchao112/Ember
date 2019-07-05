/*----------------------------------------------------------------
// Copyright (C) 2017 Jiawen(Kevin)
//
// file name: PhysicsAgent.cs
// description: 
// 
// created time：06/20/2017
//
//----------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Logic;
using Utils;

namespace BattleAgent
{
    public enum PhysicsAgentType
    {
        None,
        Unity,
        Manual,
    }

    public enum CollisionType
    {
        None,
        Grass,
        ShallowWater,
    }

    public delegate void CollisionStartMethod( CollisionType collisionType );

    public delegate void CollisionEndMethod( CollisionType collisionType );

    public class PhysicsAgent
    {
        public PhysicsAgentType agentType;

        public LogicUnit owner;

        private UnityPhysicsAgent unityPhysicsAgent;

        private ManualPhysicsAgent manualPhysicsAgent;

        public PhysicsAgent( PhysicsAgentType agentType, LogicUnit owner )
        {
            this.agentType = agentType;
            this.owner = owner;

            if( agentType == PhysicsAgentType.Unity )
            {
                unityPhysicsAgent = owner.gameObject.AddComponent<UnityPhysicsAgent>();
                unityPhysicsAgent.Initialize( owner );
            }
            else if( agentType == PhysicsAgentType.Manual )
            {
                manualPhysicsAgent = new ManualPhysicsAgent( owner );
            }
            else  
            {
                DebugUtils.LogError( DebugUtils.Type.Physics, string.Format( "This physics agent type {0} is unknown!", agentType ) );
            }
        }

        public void LogicUpdate()
        {
            if( unityPhysicsAgent != null )
            {
                unityPhysicsAgent.LogicUpdate();
            }

            if( manualPhysicsAgent != null )
            {
                manualPhysicsAgent.LogicUpdate();
            }
        }

        public void RegisterCollisionStartMethod( CollisionStartMethod method )
        {
            if( unityPhysicsAgent != null )
            {
                unityPhysicsAgent.RegisterCollisionStartMethod( method );
            }

            if( manualPhysicsAgent != null )
            {
                manualPhysicsAgent.RegisterCollisionStartMethod( method );
            }
        }

        public void RegisterCollisionEndMethod( CollisionEndMethod method )
        {
            if( unityPhysicsAgent != null )
            {
                unityPhysicsAgent.RegisterCollisionEndMethod( method );
            }

            if( manualPhysicsAgent != null )
            {
                manualPhysicsAgent.RegisterCollisionEndMethod( method );
            }
        }
    }
}
