using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Data;
using Resource;

namespace UI
{
    public class StorePopUpView : ViewBase
    {
        #region Component
        //Buy PopUp UI
        private Transform buyTran;
        private Image leftEmberIcon, leftGoldIcon, leftDiamondIcon, leftDebrisSkinIcon, leftDebrisUnitIcon, leftDebrisRuneIcon, rightEmberIcon, rightGoldIcon, rightDiamondIcon, iconImage_b, frameImage1, frameImage2;
        private Button lessButton, addButton, leftButton, rightButton, closeButton;
        private Text titleText, nameText, contentText, propText, leftCostText, rightCostText;
        private InputField inputText;

        //Give PopUp UI
        private Transform giveTran;
        private ScrollRect dargFriendPanel;
        private GridLayoutGroup friendGroup;
        private Image iconImage_g;
        private Text titleText_g, nameText_g, priceText_g, noticeText_g;
        private InputField inputText_g;
        private Button searchBt, closeBt_g;

        #endregion

		private const string FRIEND_ITEM_GIVE = "FriendItem_Give";

        public int currentId;
		public long currentFriendId;
        private StoreCostCurrecyType currentCostType;
        private int currentCount = 1;

		private List<StorePopUpGiveItem> storePopUpGiveItemList = new List<StorePopUpGiveItem> ();
		private List<FriendInfo> friendInfoList = new List<FriendInfo> ();

        private StorePopUpController controller;

        public override void OnInit()
        {
            base.OnInit();
            controller = new StorePopUpController( this );
            _controller = controller;

            #region BuyPopUp UI
            buyTran = transform.Find( "StoreBuyUI" );
            leftEmberIcon = buyTran.Find( "LeftIcons/EmberIcon" ).GetComponent<Image>();
            leftGoldIcon = buyTran.Find( "LeftIcons/GoldIcon" ).GetComponent<Image>();
            leftDiamondIcon = buyTran.Find( "LeftIcons/DiamondIcon" ).GetComponent<Image>();
            leftDebrisRuneIcon = buyTran.Find( "LeftIcons/DebrisRuneIcon" ).GetComponent<Image>();
            leftDebrisSkinIcon = buyTran.Find( "LeftIcons/DebrisSkinIcon" ).GetComponent<Image>();
            leftDebrisUnitIcon = buyTran.Find( "LeftIcons/DebrisUnitIcon" ).GetComponent<Image>();
            rightEmberIcon = buyTran.Find( "RightIcons/EmberIcon" ).GetComponent<Image>();
            rightGoldIcon = buyTran.Find( "RightIcons/GoldIcon" ).GetComponent<Image>();
            rightDiamondIcon = buyTran.Find( "RightIcons/DiamondIcon" ).GetComponent<Image>();
            iconImage_b = buyTran.Find( "IconImage" ).GetComponent<Image>();
            frameImage1 = buyTran.Find( "FrameImage1" ).GetComponent<Image>();
            frameImage2 = buyTran.Find( "FrameImage2" ).GetComponent<Image>();
            titleText = buyTran.Find( "TitleText" ).GetComponent<Text>();
            nameText = buyTran.Find( "NameText" ).GetComponent<Text>();
            contentText = buyTran.Find( "ContentText" ).GetComponent<Text>();
            propText = buyTran.Find( "PropText" ).GetComponent<Text>();
            leftCostText = buyTran.Find( "LeftCostText" ).GetComponent<Text>();
            rightCostText = buyTran.Find( "RightCostText" ).GetComponent<Text>();
            inputText = buyTran.Find( "InputField" ).GetComponent<InputField>();

            lessButton = buyTran.Find( "LessButton" ).GetComponent<Button>();
            addButton = buyTran.Find( "AddButton" ).GetComponent<Button>();
            leftButton = buyTran.Find( "LeftButton" ).GetComponent<Button>();
            rightButton = buyTran.Find( "RightButton" ).GetComponent<Button>();
            closeButton = buyTran.Find( "CloseButton" ).GetComponent<Button>();

            lessButton.AddListener( OnClickLessBt );
            addButton.AddListener( OnClickAddBt );
            leftButton.AddListener( OnClickLeftBt, UIEventGroup.Middle, UIEventGroup.Middle );
            rightButton.AddListener( OnClickRightBt, UIEventGroup.Middle, UIEventGroup.Middle );
            closeButton.AddListener( OnClickCloseBt );
            inputText.onValueChanged.AddListener( OnValueChangedInputField_b );
            #endregion

            #region GivePopUp UI
            giveTran = transform.Find( "StoreGiveUI" );
            dargFriendPanel = giveTran.Find( "DragFriendPanel" ).GetComponent<ScrollRect>();
            friendGroup = giveTran.Find( "DragFriendPanel/ItemGroup" ).GetComponent<GridLayoutGroup>();
            iconImage_g = giveTran.Find( "IconImage" ).GetComponent<Image>();
            titleText_g = giveTran.Find( "TitleText" ).GetComponent<Text>();
            nameText_g = giveTran.Find( "NameText" ).GetComponent<Text>();
            priceText_g = giveTran.Find( "PriceText" ).GetComponent<Text>();
            noticeText_g = giveTran.Find( "NoticeText" ).GetComponent<Text>();
            inputText_g = giveTran.Find( "InputField" ).GetComponent<InputField>();
            searchBt = giveTran.Find( "SearchButton" ).GetComponent<Button>();
            closeBt_g = giveTran.Find( "CloseButton" ).GetComponent<Button>();

            inputText_g.onValueChanged.AddListener( OnValueChangeInputField_g );
            searchBt.AddListener( OnClickSearchBt );
            closeBt_g.AddListener( OnClickClose_GBt );
            #endregion
        }

        public void EnterStoreBuyPopUpUI( int id, StoreCostCurrecyType type )
        {
            OnEnter();

            buyTran.gameObject.SetActive( true );
            giveTran.gameObject.SetActive( false );

            currentId = id;
            currentCostType = type;

            currentCount = 1;
            inputText.text = 1.ToString();

            SetBuyPopUpUI( type );
            RefreshBuyPopUp( id, type );
        }

        public void EnterStoreGivePopUpUI( int id, long friendId = 0 )
        {
            OnEnter();

            buyTran.gameObject.SetActive( false );
            giveTran.gameObject.SetActive( true );
            currentId = id;
            currentFriendId = friendId;

            controller.SendRelationList();
            RefreshGivePopUp( id );
        }

        public override void OnExit( bool isGoBack )
        {
            OnValueChangedInputField_b( "1" );
            base.OnExit( isGoBack );
        }

        #region Button & InputField Value Changed Event

		#endregion

        #region BuyPopUp UI

        private void OnClickLessBt()
        {
            if ( currentCount <= 1 )
                return;
            inputText.text = ( --currentCount ).ToString();
        }

        private void OnClickAddBt()
        {
            inputText.text = ( ++currentCount ).ToString();
        }

        private void OnClickLeftBt()
        {
            if ( currentCostType == StoreCostCurrecyType.OnlyRuneDebris || currentCostType == StoreCostCurrecyType.OnlySkinDebris || currentCostType == StoreCostCurrecyType.OnlyUnitDebris )
                ClickExchangeLeftBt();
            else
                ClickBuyPopUpLeftBtCallBack();
        }

        private void OnClickRightBt()
        {
            if ( currentCostType == StoreCostCurrecyType.OnlyRuneDebris || currentCostType == StoreCostCurrecyType.OnlySkinDebris || currentCostType == StoreCostCurrecyType.OnlyUnitDebris )
                OnExit( false );
            else
                ClickBuyPopUpRightBtCallBack();
        }

        private void OnClickCloseBt()
        {
            OnExit( false );
        }

        private void OnValueChangedInputField_b( string value )
        {
            inputText.text = currentCount.ToString();

            RefreshCostText( currentId, currentCostType );
        }

        #endregion

		public void RefreshFriendList()
		{

			friendInfoList.Clear();
			FriendInfo friendInfo = controller.GetfriendList().Find( p => p.friendId == currentFriendId );

			if( friendInfo != null )
			{
				friendInfoList.Add( friendInfo );
			}
			else
			{
				friendInfoList = controller.GetfriendList();
			}

			LoadStorePopUpGiveItem();
		}

		public void LoadStorePopUpGiveItem()
		{
			GameResourceLoadManager.GetInstance ().LoadResource ( FRIEND_ITEM_GIVE , OnLoadFriendItemGiveItem , true );
		}

		#region Match Item

		private void OnLoadFriendItemGiveItem( string name, UnityEngine.Object obj, System.Object param )
		{
			Utils.CommonUtil.ClearItemList<StorePopUpGiveItem> ( storePopUpGiveItemList );

			for ( int i = 0; i < friendInfoList.Count; i++ )
			{
				StorePopUpGiveItem giveItem;
				FriendInfo friendInfo = friendInfoList[ i ];

				if( i >= storePopUpGiveItemList.Count )
				{
					giveItem = Utils.CommonUtil.CreateItem<StorePopUpGiveItem> ( obj , friendGroup.transform );
					storePopUpGiveItemList.Add ( giveItem );
				}

				giveItem = storePopUpGiveItemList[ i ];
				giveItem.gameObject.SetActive ( true );

				giveItem.icon = friendInfo.portrait;
				giveItem.playerId = friendInfo.friendId;
				giveItem.nameStr = friendInfo.name;
                giveItem.moreThen10LV = controller.MoreThanLV10();
				giveItem.ClickGiveBtEvent = OnGiveItemClickCallBack;
				giveItem.RefreshItem ();
			}
		}

		#endregion

		#region onclick call back

		public void OnGiveItemClickCallBack( long playerId )
		{
			controller.SendGiftItems( playerId, currentId );
		}

		#endregion

        #region Give PopUp UI

        private void OnClickSearchBt()
        {
			friendInfoList.Clear();
			FriendInfo friendInfo = controller.GetfriendList().Find( p => p.name == inputText_g.text );

			if( friendInfo != null )
			{
				friendInfoList.Add( friendInfo );
			}

			LoadStorePopUpGiveItem();
        }

        private void OnClickClose_GBt()
        {
            OnExit( false );
        }

        private void OnValueChangeInputField_g( string value )
        {

        }

        #endregion

        #region Buy UI

        public void RefreshBuyPopUp( int id, StoreCostCurrecyType type )
        {
            if ( type == StoreCostCurrecyType.OnlyRuneDebris || type == StoreCostCurrecyType.OnlySkinDebris || type == StoreCostCurrecyType.OnlyUnitDebris )
            {
                titleText.text = "兑换";
                nameText.text = controller.GetDebrisItemName( id );
                contentText.text = controller.GetDebrisItemDescribe( id );
                propText.text = "";

                frameImage1.gameObject.SetActive( true );
                frameImage2.gameObject.SetActive( false );

                LoadAtlasSprite( controller.GetDebrisItemIcon( id ), iconImage_b );
            }
            else
            {
                titleText.text = "购买";
                nameText.text = controller.GetStoreItemName( id );
                contentText.text = controller.GetStoreItemDescribe( id );
                propText.text = controller.GetStoreItemProp( id );

                bool isRune = controller.GetStoreItemType( id ) == 3;
                frameImage1.gameObject.SetActive( !isRune );
                frameImage2.gameObject.SetActive( isRune );

                LoadAtlasSprite( controller.GetStoreItemIcon( id ), iconImage_b );
            }
            RefreshCostText( id, type );
        }

        private void RefreshCostText( int id, StoreCostCurrecyType type )
        {
            if ( currentCostType == StoreCostCurrecyType.OnlyRuneDebris || currentCostType == StoreCostCurrecyType.OnlySkinDebris || currentCostType == StoreCostCurrecyType.OnlyUnitDebris )
            {
                leftCostText.text = ( controller.GetDebrisItemPrice( id ) * currentCount ).ToString();
                rightCostText.text = "取消";
                return;
            }

            if ( currentCount == 0 )
            {
                leftButton.interactable = false;
                rightButton.interactable = ( type != StoreCostCurrecyType.GoldAndDiamonds && type != StoreCostCurrecyType.GoldAndEmber );
            }
            else
            {
                leftButton.interactable = true;
                rightButton.interactable = true;
            }

            bool isAlone = ( type != StoreCostCurrecyType.GoldAndDiamonds && type != StoreCostCurrecyType.GoldAndEmber );

            leftCostText.text = ( controller.GetStoreItemCostLeft( id ) * currentCount ).ToString();
            rightCostText.text = isAlone ? "取消" : ( controller.GetStoreItemCostRight( id ) * currentCount ).ToString();
        }

        private void SetBuyPopUpUI( StoreCostCurrecyType type )
        {
            leftEmberIcon.gameObject.SetActive( false );
            leftGoldIcon.gameObject.SetActive( false );
            leftDiamondIcon.gameObject.SetActive( false );
            leftDebrisRuneIcon.gameObject.SetActive( false );
            leftDebrisSkinIcon.gameObject.SetActive( false );
            leftDebrisUnitIcon.gameObject.SetActive( false );
            rightEmberIcon.gameObject.SetActive( false );
            rightGoldIcon.gameObject.SetActive( false );
            rightDiamondIcon.gameObject.SetActive( false );

            switch ( type )
            {
                case StoreCostCurrecyType.OnlyEmber:
                    leftEmberIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.OnlyDiamonds:
                    leftDiamondIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.OnlyGold:
                    leftGoldIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.OnlySkinDebris:
                    leftDebrisSkinIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.GoldAndEmber:
                    leftGoldIcon.gameObject.SetActive( true );
                    rightEmberIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.GoldAndDiamonds:
                    leftGoldIcon.gameObject.SetActive( true );
                    rightDiamondIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.OnlyUnitDebris:
                    leftDebrisUnitIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.OnlyRuneDebris:
                    leftDebrisRuneIcon.gameObject.SetActive( true );
                    break;
            }
        }

        private void ClickExchangeLeftBt()
        {
            if ( controller.GetDebrisItemPrice( currentId ) > controller.GetDebrisCostCount( currentId ) )
                OpenNotEnoughAlert( "碎片不足" );
            else
            {
                controller.SendStoreExchangeC2S( controller.GetExchangeId( currentId ) );
            }
        }

        private void ClickBuyPopUpLeftBtCallBack()
        {
            if ( currentCount > controller.GetStoreItemLeftMaxCount( currentId ) )
                OpenNotEnoughAlert( "" );

            else
            {
                UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );
                Data.CurrencyType type = (Data.CurrencyType)controller.GetStoreItemLeftCostType( currentId );
                controller.SendStoreBuyC2S( type, currentId, currentCount );
            }
        }

        private void ClickBuyPopUpRightBtCallBack()
        {
            if ( controller.GetStoreItemRightCostType( currentId ) == 0 )
            {
                OnExit( false );
                return;
            }

            if ( currentCount > controller.GetStoreItemRightMaxCount( currentId ) )
                OpenNotEnoughAlert( "" );

            else
            {
                UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );
                Data.CurrencyType type = (Data.CurrencyType)controller.GetStoreItemRightCostType( currentId );
                controller.SendStoreBuyC2S( type, currentId, currentCount );
            }
        }

        #endregion

        #region Give UI

        private void RefreshGivePopUp( int id )
        {
            titleText.text = "赠送";
            nameText_g.text = controller.GetStoreItemName( id );
            priceText_g.text = controller.GetStoreItemCostLeft( id ).ToString();
            LoadAtlasSprite( controller.GetStoreItemIcon( id ), iconImage_g );

            noticeText_g.gameObject.SetActive( !controller.MoreThanLV10() );
        }

        #endregion

        private void OpenNotEnoughAlert( string content )
        {
            if ( string.IsNullOrEmpty( content ) )
                content = "货币不足，请充值";

            string titleText = "提示";

            Utils.MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, AlertType.ConfirmAlone, content, titleText );

            OnExit( false );
        }

        private void LoadAtlasSprite( int icon, Image iconImage )
        {
            if ( icon != 0 )
            {
                Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    iconImage.SetSprite( atlasSprite );
                }, true );
            }
        }

    }
}