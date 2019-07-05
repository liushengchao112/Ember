using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace UI
{
    public class ShakeCamera : MonoBehaviour
    {
        private int shakeNum;
        private float shakeDis;
        private int shakeTime;
        private TweenCallback callback;
        private Vector3 originalPos;
        private int num;
        
        private void Start()
        {
            originalPos = transform.localPosition;
        }

        public void ShowShake( int shakeNum)
        {
            transform.DOKill();
            transform.localPosition = originalPos;
            this.shakeTime = 30;
            shakeDis = 0.05f;
            this.shakeNum = shakeNum;
            //this.callback = callback;
            ShowShake();
        }

        public void ShowShake()
        {
            num = shakeNum;
            ShakeHandler();
        }

        private void ShakeHandler()
        {
            transform.DOLocalMoveY( originalPos.y + shakeDis, shakeTime * 0.001f ).OnComplete( delegate ()
             {
                 transform.DOLocalMoveY( originalPos.y - shakeDis, shakeTime * 0.001f ).OnComplete( delegate ()
                 {
                     --num;
                     if( num > 0)
                     {
                         ShakeHandler();
                     }
                     else
                     {
                         transform.DOLocalMoveY( originalPos.y, shakeTime * 0.001f );
                     }
                 } );
             } );
        }
    }
}