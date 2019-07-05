using UnityEngine;
using System.Collections;

using Data;
using Utils;
using Map;
using Constants;
using System;

namespace Render
{
    public class CrystalRender : UnitRender
    {
        public bool plusCrystal = false;

        protected override void Awake()
        {
            unitRenderType = UnitRenderType.Crystal;
            animator = gameObject.GetComponentInChildren<Animator>();

            base.Awake();
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public void SetRotation( float x, float y, float z)
        {
            transform.rotation = Quaternion.Euler( x, y, z );
        }

        public override void Reset()
        {
            id = -1;
            transform.position = new Vector3( float.MaxValue / 2, float.MaxValue / 2, float.MaxValue / 2 );                    
        }
    }
}