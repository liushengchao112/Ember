using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Utils;
using Resource;

namespace UI
{
    public class BulletinBoardView : ViewBase
    {
        public enum BoardType
        {
            BulletinBoard = 0,
            ActivityBoard = 1,
        }

        #region Component

        //Bulletin Board View
        private GridLayoutGroup bulletinBoardTopGroup, bulletinBoardLeftGroup;
        private ToggleGroup bulletinTopTgGroup, bulletinLeftTgGroup;
        private Button exitButton;

        //Bulletin Panel
        private Transform bulletinPanelT;
        private Image bulletinImage;
        private Button finishButton;
        private Text finishBtText, titleText, contentText;
        private ScrollRect contentScrollRect;

        //Activity Panel
        private Transform activityPanelT;
        private GridLayoutGroup activityItemGroup;

        #endregion

        private BulletinBoardController controller;

        private List<BulletinBoardTopItem> boardTopItems = new List<BulletinBoardTopItem>();
        private List<BulletinBoardLeftItem> boardLeftItems = new List<BulletinBoardLeftItem>();
        private List<BulletinBoardActivityItem> activityItems = new List<BulletinBoardActivityItem>();

        private int currentLeftIndex = -1;
        private int currentTopIndex = -1;

        public override void OnEnter()
        {
            base.OnEnter();

            RefreshBulletinBoardTopItem();
            controller.SendPlacardC2S();
        }

        public override void OnInit()
        {
            base.OnInit();
            controller = new BulletinBoardController( this );
            _controller = controller;

            bulletinBoardTopGroup = transform.Find( "DragBoardTopPanel/BulletinBoardTopGroup" ).GetComponent<GridLayoutGroup>();
            bulletinBoardLeftGroup = transform.Find( "DragBoardLeftPanel/BulletinBoardLeftGroup" ).GetComponent<GridLayoutGroup>();
            bulletinTopTgGroup = transform.Find( "DragBoardTopPanel/BulletinBoardTopGroup" ).GetComponent<ToggleGroup>();
            bulletinLeftTgGroup = transform.Find( "DragBoardLeftPanel/BulletinBoardLeftGroup" ).GetComponent<ToggleGroup>();
            exitButton = transform.Find( "ExitButton" ).GetComponent<Button>();

            exitButton.AddListener( ClickExitButton );

            //Bulletin Panel
            bulletinPanelT = transform.Find( "BulletinPanel" );
            contentScrollRect = bulletinPanelT.Find( "ContentScrollView" ).GetComponent<ScrollRect>();
            bulletinImage = bulletinPanelT.Find( "BulletinImage" ).GetComponent<Image>();
            finishButton = bulletinPanelT.Find( "FinishButton" ).GetComponent<Button>();
            contentText = bulletinPanelT.Find( "ContentScrollView/ContentText" ).GetComponent<Text>();
            finishBtText = bulletinPanelT.Find( "FinishBtText" ).GetComponent<Text>();
            titleText = bulletinPanelT.Find( "TitleText" ).GetComponent<Text>();

            //Activity Panel
            activityPanelT = transform.Find( "ActivityPanel" );
            activityItemGroup = activityPanelT.Find( "DragActivityItemPanel/ActivityItemGroup" ).GetComponent<GridLayoutGroup>();
        }

        #region Button Event

        private void ClickExitButton()
        {
            OnExit( false );
        }

        #endregion

        #region Init BoardTopItem

        //TestData
        private string[] topTitleList = new string[] { "游戏公告", "活动事件" };

        private int boardTopItemCount = 2;
        private void RefreshBulletinBoardTopItem()
        {

            GameResourceLoadManager.GetInstance().LoadResource( "BoardTopToggleItem", OnLoadBulletinBoardTopItem );
        }

        private void OnLoadBulletinBoardTopItem( string name, Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<BulletinBoardTopItem>( boardTopItems );

            for ( int i = 0; i < boardTopItemCount; i++ )
            {
                BulletinBoardTopItem boardTopItem;

                if ( boardTopItemCount > boardTopItems.Count )
                {
                    boardTopItem = CommonUtil.CreateItem<BulletinBoardTopItem>( obj, bulletinBoardTopGroup.transform );
                    boardTopItems.Add( boardTopItem );
                }

                boardTopItem = boardTopItems[i];
                boardTopItem.gameObject.SetActive( true );

                boardTopItem.index = i;
                boardTopItem.tgText = topTitleList[i];
                boardTopItem.clickTopToggleEvent = ClickBoardTopToggle;
                boardTopItem.clickToggle.group = bulletinTopTgGroup;

                boardTopItem.ResfresTopItem();
            }

            if ( currentTopIndex < 0 && boardTopItems.Count > 0 )
            {
                boardTopItems[0].clickToggle.isOn = true;
                boardTopItems[0].clickToggle.interactable = false;
            }
        }

        private void ClickBoardTopToggle( BulletinBoardTopItem item )
        {
            currentTopIndex = item.index;

            currentLeftIndex = -1;

            if ( boardLeftItems.Count > 0 )
                ResetBoardLeftToggle();

            RefreshBulletinBoardLeftItem();
        }

        private void ResetBoardLeftToggle()
        {
            bulletinLeftTgGroup.allowSwitchOff = true;
            foreach ( BulletinBoardLeftItem item in boardLeftItems )
            {
                item.clickToggle.isOn = false;
            }
            bulletinLeftTgGroup.allowSwitchOff = false;
        }

        #endregion

        #region Init BoardLeftItem

        private int boardLeftItemCount = 1;
        public void RefreshBulletinBoardLeftItem()
        {
            if ( currentTopIndex == 0 )
                boardLeftItemCount = controller.GetBulletinBoardCount();
            else
                boardLeftItemCount = 1;

            GameResourceLoadManager.GetInstance().LoadResource( "BoardLeftToggleItem", OnLoadBulletinBoardLeftItem );
        }

        private void OnLoadBulletinBoardLeftItem( string name, Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<BulletinBoardLeftItem>( boardLeftItems );

            for ( int i = 0; i < boardLeftItemCount; i++ )
            {
                BulletinBoardLeftItem boardLeftItem;
                if ( boardLeftItems.Count < boardLeftItemCount )
                {
                    boardLeftItem = CommonUtil.CreateItem<BulletinBoardLeftItem>( obj, bulletinBoardLeftGroup.transform );
                    boardLeftItems.Add( boardLeftItem );
                }
                boardLeftItem = boardLeftItems[i];
                boardLeftItem.gameObject.SetActive( true );

                boardLeftItem.index = i;
                boardLeftItem.type = (BoardType)currentTopIndex;
                boardLeftItem.tgText = controller.GetBulletinTitle( i );
                boardLeftItem.clickLeftToggleEvent = ClickBoardLeftToggle;
                boardLeftItem.clickToggle.group = bulletinLeftTgGroup;

                boardLeftItem.RefreshBoardLeftItem();
            }
            if ( currentLeftIndex < 0 && boardLeftItems.Count > 0 )
            {
                boardLeftItems[0].clickToggle.isOn = true;
            }
        }

        private void ClickBoardLeftToggle( BulletinBoardLeftItem item )
        {
            currentLeftIndex = item.index;

            if ( currentTopIndex == 0 )
                RefreshBulletinBoard();
            else
            {
                bulletinPanelT.gameObject.SetActive( false );
                activityPanelT.gameObject.SetActive( true );

                RefreshActivityItem();
            }
        }

        #endregion

        #region Init BoardActivityItem

        private int activityItemCount = 1;
        public void RefreshActivityItem()
        {

            GameResourceLoadManager.GetInstance().LoadResource( "BoardActivityItem", OnLoadActivityItem );
        }

        private void OnLoadActivityItem( string name, Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<BulletinBoardActivityItem>( activityItems );

            for ( int i = 0; i < activityItemCount; i++ )
            {
                BulletinBoardActivityItem activityItem;
                if ( activityItems.Count < activityItemCount )
                {
                    activityItem = CommonUtil.CreateItem<BulletinBoardActivityItem>( obj, activityItemGroup.transform );

                    activityItems.Add( activityItem );
                }
                activityItem = activityItems[i];
                activityItem.gameObject.SetActive( true );

                activityItem.index = i;
                activityItem.clickActivityButtonEvent = ClickActivityButton;
            }
        }

        private void ClickActivityButton( BulletinBoardActivityItem item )
        {
            UIManager.Instance.GetUIByType( UIType.SignView, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); } );
            OnExit( false );
        }

        #endregion

        public void RefreshView_Test()
        {
            RefreshBulletinBoardTopItem();
            RefreshBulletinBoardLeftItem();

            RefreshBulletinBoard();
        }

        private void RefreshBulletinBoard()
        {
            bulletinPanelT.gameObject.SetActive( true );
            activityPanelT.gameObject.SetActive( false );

            SetBulletinUI();
            titleText.text = controller.GetBulletinTitle( currentLeftIndex );

            switch ( controller.GetBulletinType( currentLeftIndex ) )
            {
                case BulletinBoardController.BulletinType.TextMode:
                    contentText.text = controller.GetBulletinContent( currentLeftIndex );
                    break;
                case BulletinBoardController.BulletinType.ImageMode:
                    if ( !string.IsNullOrEmpty( controller.GetBulletinContent( currentLeftIndex ) ) )
                    {
                        GameResourceLoadManager.GetInstance().LoadAtlasSprite( controller.GetBulletinContent( currentLeftIndex ), delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                            bulletinImage.SetSprite( atlasSprite );
                        }, true );
                    }
                    break;
            }

        }

        private void SetBulletinUI()
        {
            //set button is false
            finishButton.gameObject.SetActive( false );
            finishBtText.gameObject.SetActive( false );

            switch ( controller.GetBulletinType( currentLeftIndex ) )
            {
                case BulletinBoardController.BulletinType.TextMode:
                    bulletinImage.gameObject.SetActive( false );
                    contentScrollRect.gameObject.SetActive( true );
                    titleText.gameObject.SetActive( true );
                    break;
                case BulletinBoardController.BulletinType.ImageMode:
                    bulletinImage.gameObject.SetActive( true );
                    contentScrollRect.gameObject.SetActive( false );
                    titleText.gameObject.SetActive( false );
                    break;
            }
        }
    }
}