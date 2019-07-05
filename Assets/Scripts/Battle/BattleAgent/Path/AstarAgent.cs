using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Data;
using Logic;
using Utils;
using System;

namespace BattleAgent
{
    public class AstarAgent : PathAgent
    {

        public new static void Initialize()
        {

        }

        public AstarAgent( PathAgentType methodType, LogicUnit owner ) : base( PathAgentType.AStar, owner )
        {

        }

        public override void SetPathType( PathType pathType )
        {

        }

        public override void SetPosition( FixVector3 pos )
        {

        }

        public override void FindPath( FixVector3 startPosition, FixVector3 destination, int areaMask, OnPathFoundHandler handler )
        {

        }

        public override bool DirectPassToPosition( FixVector3 targetPosition, int areaMask )
        {
            return true;
        }

        public override void Move( FixVector3 speed )
        {

        }

        public override void ToughMove( FixVector3 speed )
        {

        }

        public override bool Raycast( FixVector3 targetPosition, out FixVector3 hitPosition )
        {
            hitPosition = FixVector3.zero;
            return false;
        }

        public override bool SamplePosition( FixVector3 sourcePosition, out FixVector3 nearestPoint )
        {
            nearestPoint = FixVector3.zero;
            return false;
        }
    }
}
