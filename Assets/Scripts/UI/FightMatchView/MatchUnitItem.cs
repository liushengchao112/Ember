using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

using Resource;

namespace UI
{
    public class MatchUnitItem : MonoBehaviour
    {
        public int icon;

        private Image itemImage;
        private Button bgButton;

        private void Awake()
        {
            itemImage = transform.Find( "ItemImage" ).GetComponent<Image>();
            bgButton = transform.Find( "BgButton" ).GetComponent<Button>();
        }

        public void RefreshMatchUnitItem()
        {
            if ( icon != 0 )
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    itemImage.SetSprite( atlasSprite );
                }, true );
        }
    }
}
