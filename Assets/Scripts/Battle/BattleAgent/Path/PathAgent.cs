/*----------------------------------------------------------------
// Copyright (C) 2017 Jiawen(Kevin)
//
// file name: PathAgent.cs
// description: 
// 
// created time：06/16/2017
//
//----------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Data;
using Logic;
using Utils;

namespace BattleAgent
{
    public enum PathAgentType
    {
        None = 0,
        NavMesh,
        AStar,
    }

    public enum PathType
    {
        None = 0,
        Ground,
        Flying,
    }

    public delegate void OnPathFoundHandler( List<FixVector3> positions );

    public abstract class PathAgent
    {
        public PathAgentType pathAgentType;

        public PathType agentType;

        protected LogicUnit owner;

        public static void Initialize()
        {
            NavAgent.Initialize();
        }

        public PathAgent( PathAgentType methodType, LogicUnit owner )
        {
            this.pathAgentType = methodType;
            this.owner = owner;
        }

        public abstract void SetPathType( PathType pathType );

        public abstract void SetPosition( FixVector3 pos );

        public abstract void FindPath( FixVector3 startPosition, FixVector3 destination, int areaMask, OnPathFoundHandler handler );

        public abstract bool DirectPassToPosition( FixVector3 targetPosition, int areaMask );

        public abstract void Move( FixVector3 speed );

        public abstract void ToughMove( FixVector3 speed );

        public abstract bool Raycast( FixVector3 targetPosition, out FixVector3 hitPosition );

        public abstract bool SamplePosition( FixVector3 sourcePosition, out FixVector3 nearestPoint );
    }
}
