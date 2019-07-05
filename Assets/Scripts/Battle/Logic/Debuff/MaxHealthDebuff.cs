using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class MaxHealthDebuff : Debuff
    {
        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            // TODO: about this debuff need more detail
        }

        public override void Detach()
        {
            base.Detach();
        }
    }
}
