using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerPortraitItem : MonoBehaviour
    {
        public delegate void OnClickToggle( int index );
        public OnClickToggle clickToggleCallBack;

        public int index;
        public string icon;

        private Image playerIcon;
        public Toggle playerPortraitToggle;

        private void Awake()
        {
            playerIcon = transform.Find( "PlayerIcon" ).GetComponent<Image>();
            playerPortraitToggle = transform.Find( "SelectToggle" ).GetComponent<Toggle>();

            playerPortraitToggle.AddListener( ClickToggle );
        }

        private void ClickToggle( bool isOn )
        {
            playerPortraitToggle.interactable = !isOn;
            if ( isOn )
            {
                clickToggleCallBack.Invoke( index );
            }
        }

        public void RefreshItem()
        {
            if ( !string.IsNullOrEmpty( icon ) )
                Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    playerIcon.SetSprite( atlasSprite );
                }, true );
        }
    }
}
