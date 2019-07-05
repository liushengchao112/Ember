using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using Resource;
using Utils;
using Data;

namespace UI
{
    public enum GreetingType
    {
        None = 0,
        ZhiJiao = 1,
        NiceWork = 2,
        Fault = 3,
        GoodLuck = 4,
        GoodGame = 5,
    }

    public class GreetingView : MonoBehaviour
    {
        private Transform greetingRoot;
        private Image greetingHead;
        private Text desText;

        private MatchSide selfSide;

        public void OnInit()
        {
            greetingRoot = transform.Find( "Ani/GreetingRoot" );
            greetingHead = greetingRoot.Find( "GreetingHeadBg/GreetingHead" ).GetComponent<Image>();
            desText = greetingRoot.Find( "DesText" ).GetComponent<Text>();
            desText.text = "";
            greetingRoot.gameObject.SetActive( false );

            DataManager client = DataManager.GetInstance();
            selfSide = client.GetMatchSide();
        }

        private void OnDestroy()
        {
            
        }

        public void ShowGreetingPanel()
        {
            greetingRoot.gameObject.SetActive( true );
        }

        public void HideGreetingPanel()
        {
            greetingRoot.gameObject.SetActive( false );
        }

        public void RefreshPlayerHeadIcon( string orderIconStr )
        {
            if ( string.IsNullOrEmpty( orderIconStr ) )
            {
                DebugUtils.LogError( DebugUtils.Type.UI , "orderIconStr IsNullOrEmpty" );
            }
            else
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( orderIconStr , delegate ( string name , AtlasSprite atlasSprite , System.Object param )
                {
                    greetingHead.SetSprite( atlasSprite );
                } , true );
            }
        }

        private bool GetIsSameSide( ForceMark orderForceMark )
        {
            if ( GetSideFromMark( orderForceMark ) == selfSide )
            {
                return true;
            }
            return false;
        }

        private MatchSide GetSideFromMark( ForceMark mark )
        {
            if ( mark <= ForceMark.BottomRedForce )
            {
                return MatchSide.Red;
            }
            else if ( mark <= ForceMark.BottomBlueForce )
            {
                return MatchSide.Blue;
            }
            else
            {
                return MatchSide.NoSide;
            }
        }
    }
}