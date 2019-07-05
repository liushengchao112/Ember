using UnityEngine;
using System.Collections;
using System;

using Utils;
using System.Collections.Generic;

namespace UI
{
    public class UnitAnimatorEvent : MonoBehaviour
    {
        private Animator animator;
        private AnimationClip animationClip;
        private Dictionary<string, Callback<string>> callBackDic;

        void Start()
        {
        }

        public void Init()
        {
            callBackDic = new Dictionary<string, Callback<string>>();
            animator = gameObject.GetComponent<Animator>();
            if ( animator == null )
            {
                DebugUtils.LogError( DebugUtils.Type.UI, "not found \"Animator\" Component!" );
                return;
            }
            animationClip = GetAnimationClips( "out" );
        }

        public void AddEvent( float time, string data, Callback<string> callback )
        {
            string key = callback.GetHashCode().ToString();
            callBackDic[key] = callback;
            AnimationEvent evt = new AnimationEvent();
            evt.time = time;
            evt.functionName = "OnFrameEvent";
            evt.stringParameter = string.Concat( data, "_", key );
            animationClip.AddEvent( evt );
        }

        private void OnFrameEvent( string stringParameter )
        {
            string[] arr = stringParameter.Split( '_' );
            Callback<string> callback;
            if ( callBackDic.TryGetValue( arr[1], out callback ) )
            {
                callback( arr[0] );
            }
        }

        /// <summary>
        /// get AnimationClip
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AnimationClip GetAnimationClips( string name )
        {
            AnimationClip animationClip = null;
            for ( int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; ++i )
            {
                if ( animator.runtimeAnimatorController.animationClips[i].name == name )
                {
                    animationClip = animator.runtimeAnimatorController.animationClips[i];
                    break;
                }
            }

            if ( animationClip == null )
            {
                DebugUtils.LogError( DebugUtils.Type.UI, "not found AnimationClip! name is " + name );
            }

            return animationClip;
        }


    }
}
