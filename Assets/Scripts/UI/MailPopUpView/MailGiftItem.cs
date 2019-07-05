using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MailGiftItem : MonoBehaviour
    {
        public int index, icon, count;
        public string nameStr;

        private Image iconImage;
        private Text nameText, countText;

        private void Awake()
        {
            iconImage = transform.Find( "IconImage" ).GetComponent<Image>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            countText = transform.Find( "CountText" ).GetComponent<Text>();
        }

        public void RefreshItem()
        {
            if ( icon != 0 )
                Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    iconImage.SetSprite( atlasSprite );
                }, true );

            nameText.text = nameStr;
            countText.text = "X" + count;
        }
    }
}
