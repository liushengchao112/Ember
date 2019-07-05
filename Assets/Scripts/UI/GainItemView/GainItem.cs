using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GainItem : MonoBehaviour
    {
        public int icon, count;
        public string nameStr;

        private Image itemIcon;
        private Text nameText, countText;

        private void Awake()
        {
            itemIcon = transform.Find( "ItemIcon" ).GetComponent<Image>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            countText = transform.Find( "CountText" ).GetComponent<Text>();
        }

        public void RefreshItem()
        {
            if ( icon != 0 )
                Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
                {
                    itemIcon.SetSprite( atlasSprite );
                }, true );

            nameText.text = nameStr;
            countText.text = "X" + count;
        }
    }
}