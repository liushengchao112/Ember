using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utils;
using UnityEngine.UI;
using Logic;

using Data;

namespace UI
{
    public class HealthBar : MonoBehaviour
    {
        private Image imgBg;
        private Image imgBar;
        private Slider slider;
        private Transform targetTrans;
        public RectTransform rectTrans;
        
        private Vector3 offsetPos;
        private float smoothTime = 0.001f;
        private Vector3 cameraVelocity;

        private ForceMark selfMark;

        private Sprite[] healthBarBgs;
        private Sprite[] healthBars;

        // The initialization function must be called first
        public void Init( Vector3 angles, Sprite[] healthBarBgs, Sprite[] healthBars )
        {
            this.healthBarBgs = healthBarBgs;
            this.healthBars = healthBars;
            rectTrans = GetComponent<RectTransform>();
            slider = GetComponent<Slider>();
            imgBg = transform.Find( "imgBg" ).GetComponent<Image>();
            imgBar = transform.Find( "imgBar" ).GetComponent<Image>();
            cameraVelocity = Vector3.zero;

            selfMark = DataManager.GetInstance().GetForceMark();
			if( IsNeedMirror() )
			{
                rectTrans.localEulerAngles = angles;
            }
        }

        public void Refresh( int index )
        {
            imgBg.overrideSprite = healthBarBgs[index];
            imgBg.type = Image.Type.Sliced;
            imgBar.overrideSprite = healthBars[index];
            imgBar.type = Image.Type.Sliced;
        }

        /// <summary>
        /// Set the following target
        /// </summary>
        /// <param name="tf">target transform</param>
        /// <param name="offset">offset</param>
        public void SetTarget( Transform tf, Vector3 offset )
        {
            offsetPos = offset;

            // When the object is activated, you can get the offset, which is wrong at this time
            if (!CommonUtil.CheckVector3Value( offsetPos ))
            {
                offsetPos = Vector3.zero;
            }

            // patch,if you set the coordinates float.MaxValue, you can not do the operation
            if (CommonUtil.CheckVector3Value( tf.position ))
            {
                targetTrans = tf;

                SetBarPos();
            }
        }

        public void SetActive( bool active )
        {
            if(gameObject.activeSelf != active)
            {
                gameObject.SetActive( active );
            }
        }

        void Update()
        {
            if (gameObject.activeSelf && targetTrans != null)
            {
                SetBarPos();
            }
        }

        private void SetBarPos()
        {
            DebugUtils.Assert( rectTrans != null, "Must first call Init function" );

            rectTrans.position = Vector3.SmoothDamp( transform.position, targetTrans.position + offsetPos, ref cameraVelocity, smoothTime );
        }

        // set Blood bar value
        public void SetHealth( int hp, int maxHp )
        {
            if ( imgBar == null ) return;

            slider.value = hp / ( float )maxHp;
        }

        public void Dispose()
        {
            targetTrans = null;
        }

		//Check is need mirror about ForceMark.
		private bool IsNeedMirror()
		{
			return selfMark == ForceMark.TopRedForce || selfMark == ForceMark.BottomRedForce;
		}

    }
}
