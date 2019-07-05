using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Constants;

namespace Render
{
    public class IdolRender : UnitRender
    {
        public int modelId;
        public Vector3 direction;
        public SkinnedMeshRenderer skinMeshRender;

        protected override void Awake()
        {
            unitRenderType = UnitRenderType.Idol;
            base.Awake();

            skinMeshRender = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            animator = GetComponent<Animator>();
            animator.cullingMode = AnimatorCullingMode.CullCompletely;
        }

        public void Out()
        {
            PlayEffect( GameConstants.IDOL_OUT_EFFECT, GameConstants.IDOL_OUT_EFFECT_BINDPOINT );
        }

        public void Dying()
        {
            PlayEffect( GameConstants.IDOL_DEATH_EFFECT, GameConstants.IDOL_DEATH_EFFECT_BINDPOINT );
            PlayAnimation( "Dying" );
        }

        protected override void Update()
        {
            base.Update();
        }

        public override void Reset()
        {
            
        }

        public override void Recycle()
        {
            base.Recycle();
        }

        public override void OnCullingStateChange( bool isVisibleInCamera )
        {
            base.OnCullingStateChange( isVisibleInCamera );

            if ( isVisibleInCamera )
            {
                skinMeshRender.enabled = true;
            }
            else
            {
                skinMeshRender.enabled = false;
            }
        }
    }
}

