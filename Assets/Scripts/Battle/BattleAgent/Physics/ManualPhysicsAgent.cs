using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Logic;

namespace BattleAgent
{
    public class ManualPhysicsAgent
    {
        public LogicUnit owner;

        private CollisionStartMethod CollisionStart;

        private CollisionEndMethod CollisionEnd;

        public ManualPhysicsAgent( LogicUnit owner )
        {
            this.owner = owner;
        }

        public void LogicUpdate()
        {
            
        }

        public void RegisterCollisionStartMethod( CollisionStartMethod method )
        {
            CollisionStart = method;
        }

        public void RegisterCollisionEndMethod( CollisionEndMethod method )
        {
            CollisionEnd = method;
        }
    }
}
