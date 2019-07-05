using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Resource;
using Utils;

namespace UI
{
    public class MailPopUpView : ViewBase
    {
        #region Component
        private ScrollRect dragItemPanel;
        private GridLayoutGroup itemGroup;
        private Image playerIcon;
        private Text titleText, timeText, mailTitleText, senderNameText, mailContentText, clickButtonText;
        private Button bgButton, closeButton, clickButton;
        #endregion

        private MailPopUpController controller;

        private List<MailGiftItem> mailGift_Items = new List<MailGiftItem>();

        #region Path
        private const string MAIL_GIFT_ITEM_PATH = "Prefabs/UI/MailPopUpItem/MailGift_Item";
        #endregion

        public void OnEnterMailPopUp( Data.EmailInfo info )
        {
            base.OnEnter();
            controller.info = info;

            RefreshView();
            RefreshMailGiftItem();
        }

        public override void OnInit()
        {
            base.OnInit();
            controller = new MailPopUpController( this );
            _controller = controller;

            dragItemPanel = transform.Find( "DragItemPanel" ).GetComponent<ScrollRect>();
            itemGroup = transform.Find( "DragItemPanel/ItemGroup" ).GetComponent<GridLayoutGroup>();
            playerIcon = transform.Find( "PlayerIcon" ).GetComponent<Image>();
            titleText = transform.Find( "TitleText" ).GetComponent<Text>();
            timeText = transform.Find( "TimeText" ).GetComponent<Text>();
            senderNameText = transform.Find( "SenderNameText" ).GetComponent<Text>();
            mailTitleText = transform.Find( "MailTitleText" ).GetComponent<Text>();
            mailContentText = transform.Find( "MailContentText" ).GetComponent<Text>();
            clickButtonText = transform.Find( "ClickButtonText" ).GetComponent<Text>();
            bgButton = transform.Find( "BgButton" ).GetComponent<Button>();
            closeButton = transform.Find( "CloseButton" ).GetComponent<Button>();
            clickButton = transform.Find( "ClickButton" ).GetComponent<Button>();

            bgButton.AddListener( OnClickCloseButton );
            closeButton.AddListener( OnClickCloseButton );
            clickButton.AddListener( OnClickButton );
        }

        public void RefreshView()
        {
            titleText.text = controller.GetPopUpTitle();
            senderNameText.text = controller.GetSenderName();
            timeText.text = controller.GetMailTime();
            mailTitleText.text = controller.GetMailTitle();
            mailContentText.text = controller.GetMailConent();
            clickButtonText.text = ( controller.GetMailGiftCount() == 0 || controller.info.is_get ) ? "删除" : "领取并删除";

            if ( !string.IsNullOrEmpty( controller.GetSenderIcon() ) )
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( controller.GetSenderIcon(), delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    playerIcon.SetSprite( atlasSprite );
                }, true );
            }
        }

        #region Button Event

        private void OnClickCloseButton()
        {
            OnExit( false );
        }

        private void OnClickButton()
        {
            if ( controller.GetMailGiftCount() == 0 || controller.info.is_get )
                controller.SendMailDeleteC2S();
            else
                controller.SendMailAttachmentC2S();
        }

        #endregion

        #region Init Mail Gift Item

        private int mailGiftCount = 0;
        private void RefreshMailGiftItem()
        {
            mailGiftCount = controller.GetMailGiftCount();
            GameResourceLoadManager.GetInstance().LoadResource( "MailGift_Item", OnLoadMailGiftItem, true );
        }

        private void OnLoadMailGiftItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<MailGiftItem>( mailGift_Items );
            DG.Tweening.DOTween.To( () => dragItemPanel.horizontalNormalizedPosition, value => dragItemPanel.horizontalNormalizedPosition = value, 0, 0.3f );

            for ( int i = 0; i < mailGiftCount; i++ )
            {
                MailGiftItem mailGiftItem;
                if ( mailGift_Items.Count < mailGiftCount )
                {
                    mailGiftItem = CommonUtil.CreateItem<MailGiftItem>( obj, itemGroup.transform );
                    mailGift_Items.Add( mailGiftItem );
                }
                mailGiftItem = mailGift_Items[i];
                mailGiftItem.gameObject.SetActive( true );

                mailGiftItem.index = i;
                mailGiftItem.icon = controller.GetMailGiftItemIcon( i );
                mailGiftItem.count = controller.GetMailGiftItemCount( i );
                mailGiftItem.nameStr = controller.GetMailGiftItemName( i );

                mailGiftItem.RefreshItem();
            }
        }
        #endregion
    }
}
