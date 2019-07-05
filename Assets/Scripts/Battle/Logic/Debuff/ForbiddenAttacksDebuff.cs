using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class ForbiddenAttacksDebuff : Debuff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );
        }

        public override void Detach()
        {
            base.Detach();
        }
    }
}
