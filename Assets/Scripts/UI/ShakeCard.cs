using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace UI
{
    public class ShakeCard : MonoBehaviour
    {        
        private bool shake;
        Vector3 orgPos;

        public Button button;
        Transform imageTransform;
        float shakeTime = 0.4f;
        float shakePauseTimer = 0f;
        float shakePause = 1f;

        float t = 0f;

        bool left;

        void Start()
        {
            button = transform.Find( "OnClickButton" ).GetComponent<Button>();
            //button.AddListener( Shake );
            imageTransform = transform.Find( "ItemImage" );

            orgPos = new Vector3( imageTransform.localPosition.x, imageTransform.localPosition.y, 0 );
        }

        void LateUpdate()
        {            
            if ( shake && shakePauseTimer == 0f )
            {                
                t += Time.deltaTime;            
                if ( left )
                {                    

                    imageTransform.localPosition = new Vector3( orgPos.x, orgPos.y, 0 );
                    imageTransform.Translate( -2.5f, 0, 0 );                    
                    left = false;
                }
                else
                {                    
                    imageTransform.localPosition = new Vector3( orgPos.x, orgPos.y, 0 );
                    imageTransform.Translate( 2.5f, 0, 0 );
                    left = true;
                }
                if ( t >= shakeTime )
                {
                    shake = false;
                    shakePauseTimer = 1f;                    
                }
            }
            else
            {
                if ( shakePauseTimer > 0 )
                    shakePauseTimer -= Time.deltaTime;
                if ( shakePauseTimer < 0 )
                    shakePauseTimer = 0;
                t = 0;
                shake = false;
                imageTransform.localPosition = new Vector3( orgPos.x, orgPos.y, 0 );
            }
        }

        public void Shake()
        {
            shake = true;            
        }
    }
}
