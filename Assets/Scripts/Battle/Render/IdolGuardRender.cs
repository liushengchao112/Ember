using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Constants;

namespace Render
{
    public class IdolGuardRender : BuildingRender
    {
        public int modelId;

        private float deathTimer = 0;
        private bool inDeathTiming = false;

        protected override void Awake()
        {
            base.Awake();

            unitRenderType = UnitRenderType.IdolGuard;
        }

        public override void Dying()
        {
            base.Dying();
            inDeathTiming = true;
        }

        protected override void Update()
        {
            if ( inDeathTiming )
            {
                if ( deathTimer > 1f )
                {
                    Destroy();
                }
                else
                {
                    deathTimer += Time.deltaTime;
                }
            }
            else
            {
                inDeathTiming = false;
            }
        }
    }
}
