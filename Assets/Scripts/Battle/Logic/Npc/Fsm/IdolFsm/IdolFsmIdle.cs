using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public class IdolFsmIdle : Fsm
    {
        private Idol owner;

        public IdolFsmIdle( Idol i )
        {
            owner = i;
        }
    }

}
