using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

using Resource;

namespace UI
{
    public class CombatDeckCardItem : MonoBehaviour
    {
        public Image armyImage;
        private Button onClickButton;
        
        public int id;
        public int index;
        public int unitIconImage;
        public Action<CombatDeckCardItem> onCombatDeckItemClicked;

        #region Path
        //private const string ITEM_IMAGE_PATH = "UITexture/UnitModelIcon/";
        #endregion

        private void Awake()
        {
            armyImage = transform.Find( "CombatDeckImage" ).GetComponent<Image>();
            onClickButton = transform.Find( "OnClickButton" ).GetComponent<Button>();
        }

        private void Start()
        {
            onClickButton.AddListener( OnClickButton, UIEventGroup.Middle, UIEventGroup.Middle | UIEventGroup.Top );
        }

        private void OnClickButton()
        {
            onCombatDeckItemClicked( this );
        }

        public void RefreshItem()
        {
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( unitIconImage, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
            {
                armyImage.SetSprite( atlasSprite );
            }, true );
        }
    }
}
