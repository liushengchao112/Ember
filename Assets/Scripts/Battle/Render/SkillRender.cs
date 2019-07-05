using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;

namespace Render
{
    public class SkillRender : UnitRender
    {
        public int index;
        public int recycleTime = 2; // temo number;
        public float recycleTimer = 0;
        private bool startTimingRecycle = false;

        public void Initialize( long id, ForceMark mark, int metaId, int index )
        {
            this.id = id;
            this.mark = mark;
            this.metaId = metaId;
            this.index = index;

            startTimingRecycle = false;

            unitRenderType = UnitRenderType.Skill;
        }

        public void Fire()
        {

        }

        protected override void Update()
        {
            if ( startTimingRecycle )
            {
                recycleTimer += Time.deltaTime;
                if ( recycleTimer >= recycleTime )
                {
                    gameObject.SetActive( false );
                    Destroy();
                    recycleTimer = 0;
                }
            }
        }

        public void Hit()
        {
            startTimingRecycle = true;
        }

        public override void Reset()
        {
            
        }
    }
}
