using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using Resource;
using SignInfo = Data.DailyLoginS2C.SignInfo;
using Data;
using ItemBase = Data.ItemBaseProto.ItemBase;

namespace UI
{
    public class SignItem : MonoBehaviour
    {
        #region UIComponet
        private GameObject orangeFrame;
        private GameObject purpleFrame;
        private Image iconImage;
        private Text countText;
        private Text dateText;
        private Text desTextBlue;
        private Text desTextOrange;
        private GameObject signImage;
        #endregion

        private SignInfo signInfo;
        private ItemBase itemBase;

        private void Awake()
        {
            #region FindUIComponet
            orangeFrame = transform.Find( "FrameImage1" ).gameObject;
            purpleFrame = transform.Find( "FrameImage2" ).gameObject;
            iconImage = transform.Find( "IconImage" ).GetComponent<Image>();
            countText = transform.Find( "CountText" ).GetComponent<Text>();
            dateText = transform.Find( "DateText" ).GetComponent<Text>();
            desTextBlue = transform.Find( "DesText0" ).GetComponent<Text>();
            desTextOrange = transform.Find( "DesText1" ).GetComponent<Text>();
            signImage = transform.Find( "SignImage" ).gameObject;
            #endregion
        }

        public void UpdateData( SignInfo info )
        {
            signInfo = info;
            countText.text = desTextBlue.text = desTextOrange.text = "";
            if ( signInfo == null )
            {
                return;
            }

            if ( signInfo.itemId != 0 )
            {
                itemBase = DataManager.GetInstance().itemsProtoData.Find( p => p.ID == signInfo.itemId );
                if ( itemBase != null )
                {
                    SetIconImage( itemBase.Icon );
                    desTextBlue.text = desTextOrange.text = itemBase.Name;
                }                
            }
            SetItemState( signInfo.isGet );
        }

        private void SetIconImage( int icon )
        {            
            if ( icon != 0 )
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon , delegate ( string name , AtlasSprite atlasSprite , System.Object param )
                {
                    iconImage.SetSprite( atlasSprite );
                } , true );
            }
        }

        private void SetItemState( bool isGet )
        {
            if ( isGet )
            {
                signImage.SetActive( true );
            }
            else
            {
                signImage.SetActive( false );
            }
        }
    }
}
