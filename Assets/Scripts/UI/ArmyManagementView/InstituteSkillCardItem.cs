using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using Resource;

namespace UI
{
    public class InstituteSkillCardItem : MonoBehaviour
    {
        public int index, id, icon, unlockLevel;
        public string nameStr;
        public bool isLock, isEquip;
        public Action<InstituteSkillCardItem> onClickSkillCard;

        public Image glowImage;
        private Image skillImage;
        private Button onClickButton;
        private Text nameText, unlockLevelText;

        private void Awake()
        {
            skillImage = transform.Find( "SkillImage" ).GetComponent<Image>();
            glowImage = transform.Find( "GlowImage" ).GetComponent<Image>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            unlockLevelText = transform.Find( "LockLevelText" ).GetComponent<Text>();

            onClickButton = transform.Find( "OnClickButton" ).GetComponent<Button>();
        }

        private void Start()
        {
            onClickButton.AddListener( OnClickButton );
        }

        private void OnClickButton()
        {
            onClickSkillCard( this );
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
            glowImage.gameObject.SetActive( false );

            SetLock( !isLock );
            if ( !isLock )
                SetEquip( isEquip );
        }

        private void SetLock( bool unlock )
        {
            if ( !unlock )
            {
                unlockLevelText.text = "解锁等级:" + unlockLevel;
                unlockLevelText.color = Color.white;
            }
            unlockLevelText.gameObject.SetActive( !unlock );
            skillImage.color = unlock ? Color.white : Color.gray;
        }

        private void SetEquip( bool isEquip )
        {
            if ( isEquip )
            {
                unlockLevelText.text = "已装备";
                unlockLevelText.color = Color.green;
            }
            unlockLevelText.gameObject.SetActive( isEquip );
            skillImage.color = !isEquip ? Color.white : Color.gray;
        }
    }
}
