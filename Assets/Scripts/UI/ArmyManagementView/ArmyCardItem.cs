using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

using Resource;
using Utils;

namespace UI
{
    public class ArmyCardItem : MonoBehaviour
    {
        public Image glowImage;
        private Image armyImage;
        private Button onClickButton;
        private Text numberText;

        public int id;
        public int unitIconImage;
        public int armyNumber;
        public Action<ArmyCardItem> onArmyItemClicked;

        private void Awake()
        {
            armyImage = transform.Find( "ArmyImage" ).GetComponent<Image>();
            glowImage = transform.Find( "GlowImage" ).GetComponent<Image>();
            numberText = transform.Find( "NumberText" ).GetComponent<Text>();

            onClickButton = transform.Find( "OnClickButton" ).GetComponent<Button>();
        }

        private void Start()
        {
            onClickButton.AddListener( OnClickButton, UIEventGroup.Middle, UIEventGroup.Middle | UIEventGroup.Top );
        }

        private void OnClickButton()
        {
            glowImage.gameObject.SetActive( true );

            onArmyItemClicked( this );
        }

        public void RefreshItem()
        {
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( unitIconImage, delegate( string name, AtlasSprite atlasSprite, System.Object param )
            {
                armyImage.SetSprite( atlasSprite );
            }, true );

            numberText.text = armyNumber.ToString();
            glowImage.gameObject.SetActive( false );
        }
    }
}
