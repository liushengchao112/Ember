using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Resource;

namespace UI
{
    public class SkillDeckCardItem : MonoBehaviour
    {
        public int index;
        public int icon;
        public string nameStr;
        public Action<SkillDeckCardItem> onClickSkillDeckCard;

        private Image skillImage;
        private Button onClickButton;
        private Text nameText;

        private void Awake()
        {
            skillImage = transform.Find( "SkillImage" ).GetComponent<Image>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            onClickButton = transform.Find( "OnClickButton" ).GetComponent<Button>();
        }

        private void Start()
        {
            onClickButton.AddListener( OnClickButton, UIEventGroup.Middle, UIEventGroup.Middle );
        }

        private void OnClickButton()
        {
            onClickSkillDeckCard( this );
        }

        public void RefreshItem()
        {
            if ( icon != 0 )
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
                {
                    skillImage.SetSprite( atlasSprite );
                }, true );
            }

            nameText.text = nameStr;
        }
    }
}
