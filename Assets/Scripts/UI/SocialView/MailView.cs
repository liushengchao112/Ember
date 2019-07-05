using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Utils;

namespace UI
{
    public class MailView : MonoBehaviour
    {
        #region Component

        private ScrollRect dragMailPanel;
        private GridLayoutGroup mailGroup;
        private Image redBubble;
        private Button receiveAllBt, deleteAllBt;
        private Text receiveAllText, deleteAllText, unreadNumText;

        #endregion

        private MailController controller;
        private MailScrollView mailScrollView;

        private List<MailItem> mail_Items = new List<MailItem>();

        #region Path

        private const string MAIL_ITEM_PATH = "Mail_Item";

        #endregion

        public void Init()
        {
            controller = new MailController( this );

            dragMailPanel = transform.Find( "DragMailPanel" ).GetComponent<ScrollRect>();
            mailGroup = transform.Find( "DragMailPanel/ItemGroup" ).GetComponent<GridLayoutGroup>();
            redBubble = transform.parent.Find( "ToggleGroup/MailToggle/RedBubble" ).GetComponent<Image>();
            receiveAllText = transform.Find( "ReceiveAllText" ).GetComponent<Text>();
            deleteAllText = transform.Find( "DeleteAllText" ).GetComponent<Text>();
            receiveAllBt = transform.Find( "ReceiveAllButton" ).GetComponent<Button>();
            deleteAllBt = transform.Find( "DeleteAllButton" ).GetComponent<Button>();

            if (dragMailPanel.GetComponent<MailScrollView>() == null)
            {
                mailScrollView = dragMailPanel.gameObject.AddComponent<MailScrollView>();
            }
            else
            {
                mailScrollView = dragMailPanel.GetComponent<MailScrollView>();
            }
            mailScrollView.onCreateItemHandler = OnCreateItem;
            receiveAllBt.AddListener( OnClickReceiveAllBt, UIEventGroup.Middle, UIEventGroup.Middle );
            deleteAllBt.AddListener( OnClickDeleteAllBt, UIEventGroup.Middle, UIEventGroup.Middle );
        }

        public void OnExit()
        {
            controller.OnPause();
        }

        public void OnEnter()
        {
            controller.OnResume();
            RefreshMailItem();
            RefreshUnreadNum();
        }

        public void EnterMailView()
        {
            controller.SendMailC2S();
        }

        public void RefreshMailPanel( bool refrshDragPanelPosition = false )
        {
            RefreshUnreadNum();
            RefreshMailList( refrshDragPanelPosition );
        }

        #region Button Event

        private void OnClickReceiveAllBt()
        {
            controller.SendMailGetAllC2S();
        }

        private void OnClickDeleteAllBt()
        {
            bool haveGift = false;
            List<Data.EmailInfo> emailInfo = controller.GetMailItemsList();

            for ( int i = 0; i < emailInfo.Count; i++ )
            {
                if ( emailInfo[i].is_get == false )
                {
                    haveGift = true;
                    break;
                }
            }

            if ( haveGift )
            {
                string deleteText = "有未领取邮件，无法删除";
                string titleText = "提示";

                MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, deleteText, titleText );
            }
            else
            {
                controller.SendMailDeleteAllC2S();
            }
        }

        #endregion

        #region Init Mail Item

        private void OnCreateItem( MailItem item )
        {
            item.onClickMailCallBack = ClickMailItem;
        }

        private void RefreshMailItem()
        {
            Resource.GameResourceLoadManager.GetInstance().LoadResource( MAIL_ITEM_PATH, OnLoadMailItem, true );
        }

        private void OnLoadMailItem( string name, UnityEngine.Object obj, System.Object param )
        {
            mailScrollView.InitDataBase( dragMailPanel, obj, 1, 1022, 102, 10, new Vector3( 0, -50, 0 ) );
        }

        private void ClickMailItem( Data.EmailInfo mailInfo, MailItem item )
        {
            controller.SendMailReadC2S( mailInfo.emailId, item, mailInfo );
            UIManager.Instance.GetUIByType( UIType.MailPopUpUI, ( ViewBase ui, System.Object param ) => { ( ui as MailPopUpView ).OnEnterMailPopUp( mailInfo ); } );
        }

        #endregion

        private void RefreshMailList( bool refrshPosition )
        {
            if ( refrshPosition )
                DG.Tweening.DOTween.To( () => dragMailPanel.verticalNormalizedPosition, value => dragMailPanel.verticalNormalizedPosition = value, 1, 0.3f );

            mailScrollView.InitializeWithData( controller.GetMailItemsList() );
        }

        private void RefreshUnreadNum()
        {
            redBubble.gameObject.SetActive( controller.HaveUnread() );
        }
    }
}
