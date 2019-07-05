using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MailItem : ScrollViewItemBase
    {
        public delegate void OnClickMailEvent( Data.EmailInfo mailInfo, MailItem item );

        private Data.EmailInfo info;

        public OnClickMailEvent onClickMailCallBack;

        private Button clickButton;
        private Image redBubble, playerIcon, receiveImage;
        private Text nameText, mailTitleText, timeText;
        
        private void Awake()
        {
            clickButton = transform.Find( "ClickButton" ).GetComponent<Button>();
            redBubble = transform.Find( "RedBubble" ).GetComponent<Image>();
            playerIcon = transform.Find( "PlayerIcon" ).GetComponent<Image>();
            receiveImage = transform.Find( "ReceiveImage" ).GetComponent<Image>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            mailTitleText = transform.Find( "MailTitleText" ).GetComponent<Text>();
            timeText = transform.Find( "TimeText" ).GetComponent<Text>();

            clickButton.AddListener( OnClickButton, UIEventGroup.Middle, UIEventGroup.Middle );
        }

        public override void UpdateItemData( object dataObj )
        {
            base.UpdateItemData( dataObj );
            info = (Data.EmailInfo)dataObj;
            RefreshItem();
        }

        private void OnClickButton()
        {
            onClickMailCallBack.Invoke( info, this );
        }

        public void RefreshItem()
        {
            if ( info == null )
                return;

            if ( !string.IsNullOrEmpty( info.portrait ) )
                Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( info.portrait, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    playerIcon.SetSprite( atlasSprite );
                }, true );

            nameText.text = info.senderName.ToString();
            mailTitleText.text = info.titile;
            timeText.text = GetCreateTime( info.create_time );

            redBubble.gameObject.SetActive( !info.is_read );
            receiveImage.gameObject.SetActive( !info.is_get );
        }

        private string GetCreateTime( long time )
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime( new DateTime( 1970, 1, 1 ) );
            long iTime = long.Parse( time + "0000" );
            TimeSpan toNow = new TimeSpan( iTime );
            DateTime dTime = dtStart.Add( toNow );

            return dTime.ToString( "yyyy/MM/dd \t\t HH:mm" );

        }
    }
}
