using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using DG.Tweening;
using Utils;
using Data;

namespace UI
{
    public class FloatingHealth : MonoBehaviour
    {
        private Image[] imgNumbers;
        private RectTransform trans;
        private RectTransform moveTrans;

        private TweenCallback callback;
        private Vector2 startPos;
        private Canvas canvas;

        private ForceMark selfMark;
        private Sprite[] redNumbers;
        private Sprite[] greenNumbers;
        private Sprite[] purpleNumbers;
        private Transform targetTf;
        private bool hasbloodBar;
        private Vector3 offset;
        private Vector3 cameraVelocity = Vector3.zero;

        // The initialization function must be called first
        public void Init( Canvas canvas, Vector3 angle, Sprite[] redNumbers, Sprite[] greenNumbers, Sprite[] purpleNumbers )
        {
            this.canvas = canvas;
            this.redNumbers = redNumbers;
            this.greenNumbers = greenNumbers;
            this.purpleNumbers = purpleNumbers;
            trans = GetComponent<RectTransform>();
            moveTrans = trans.Find( "moveTrans" ) as RectTransform;
            imgNumbers = trans.GetComponentsInChildren<Image>( true );
            selfMark = DataManager.GetInstance().GetForceMark();
            if ( IsNeedMirror() )
            {
                trans.localEulerAngles = angle;
            }
        }

        // Set the value required for the attack display
        public void SetValue(int num, bool isCrit, Transform targetTf, Vector3 offset, bool hasbloodBar, TweenCallback callback)
        {
            if (num == 0) return;

            this.targetTf = targetTf;
            this.hasbloodBar = hasbloodBar;
            this.offset = offset;
            this.callback = callback;

            for ( var i = 1; i < imgNumbers.Length; i++ )
            {
                if( imgNumbers[i].gameObject.activeSelf )
                {
                    imgNumbers[i].gameObject.SetActive( false );
                }
            }
            
            if ( num < 0 )
            {
                string numbers = num.ToString();
                imgNumbers[0].overrideSprite = isCrit ? purpleNumbers[10] : redNumbers[10];
                imgNumbers[0].SetNativeSize();
                for ( var i = 1; i < numbers.Length; i++ )
                {
                    int index = numbers[i] - '0';
                    imgNumbers[i].overrideSprite = isCrit ? purpleNumbers[index] : redNumbers[index];
                    if ( !imgNumbers[i].gameObject.activeSelf )
                    { 
                        imgNumbers[i].gameObject.SetActive( true );
                    }
                    imgNumbers[i].SetNativeSize();
                }
            }
            else
            {
                string numbers = num.ToString();
                imgNumbers[0].overrideSprite = greenNumbers[10];
                imgNumbers[0].SetNativeSize();
                for ( var i = 1; i < numbers.Length; i++ )
                {
                    int index = numbers[i - 1] - '0';
                    imgNumbers[i].overrideSprite = greenNumbers[index];
                    if ( !imgNumbers[i].gameObject.activeSelf )
                    {
                        imgNumbers[i].gameObject.SetActive( true );
                    }
                    imgNumbers[i].SetNativeSize();
                }
            }
        }

        public void Show()
        {
            if ( hasbloodBar )
            {
                trans.localPosition = targetTf.localPosition;
            }
            else
            {
                Vector3 pos = canvas.worldCamera.WorldToScreenPoint( targetTf.position + offset );
                Vector2 vec;
                RectTransformUtility.ScreenPointToLocalPointInRectangle( canvas.transform as RectTransform, pos, canvas.worldCamera, out vec );
                trans.anchoredPosition = vec;
            }

            moveTrans.anchoredPosition = offset;
            trans.localScale = Vector3.zero;

            gameObject.SetActive( true );

            trans.DOScale( 1.5f, 0.2f ).OnComplete( delegate () { trans.DOScale( 1f, 0.3f ); } );
            moveTrans.DOLocalMoveY( moveTrans.localPosition.y + 120, 0.6f ).SetEase( Ease.InCirc ).OnComplete(
                delegate ()
                {
                    if ( callback == null )
                    {
                        DebugUtils.LogError( DebugUtils.Type.Battle, "Attack shows an error, no registration is removed" );
                    }
                    else
                    {
                        callback();
                    }
                } );
        }
     
        void Update()
        {
            if( gameObject.activeSelf )
            {
                trans.position = Vector3.SmoothDamp( transform.position, targetTf.position, ref cameraVelocity, 0 );
            }
        }

        // Destroyed when called
        public void Dispose()
        {
            trans.DOKill();
        }

		//Check is need mirror about ForceMark.
		private bool IsNeedMirror()
		{
			return selfMark == ForceMark.TopRedForce || selfMark == ForceMark.BottomRedForce;
        }
    }
}
