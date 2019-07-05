using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public class WildMonsterFsmIdle : Fsm
    {
        public Npc owner;

        public WildMonsterFsmIdle() { }

        public WildMonsterFsmIdle( Npc n )
        {
            owner = n;
        }
    }
}
