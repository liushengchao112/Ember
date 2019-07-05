using System;
using UnityEngine;
using UnityEngine.UI;

using Resource;

namespace UI
{
    public class PlayerBagItem : MonoBehaviour
    {
        public int icon, count, index;
        public bool isRune;
        public Action<int> onClickEvent;
        public Toggle onclickToggle;

        private Image iconImage, frameImage1, frameImage2;
        private Text countText;

        private void Awake()
        {
            onclickToggle = transform.Find( "OnClickToggle" ).GetComponent<Toggle>();
            iconImage = transform.Find( "IconImage" ).GetComponent<Image>();
            frameImage1 = transform.Find( "FrameImage1" ).GetComponent<Image>();
            frameImage2 = transform.Find( "FrameImage2" ).GetComponent<Image>();
            countText = transform.Find( "CountText" ).GetComponent<Text>();
        }

        private void Start()
        {
            onclickToggle.AddListener( OnClickToggle );
        }

        private void OnClickToggle( bool isOn )
        {
            onclickToggle.interactable = !isOn;
            if ( isOn )
                onClickEvent( index );
        }

        public void RefreshItem()
        {
            if ( icon != 0 )
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    iconImage.SetSprite( atlasSprite );
                }, true );
            }

            frameImage1.gameObject.SetActive( !isRune );
            frameImage2.gameObject.SetActive( isRune );
            countText.text = count.ToString();
        }
    }
}
