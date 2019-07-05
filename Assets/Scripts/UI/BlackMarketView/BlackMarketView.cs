using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

using Resource;
using Utils;
using DG.Tweening;

namespace UI
{
    public class BlackMarketView : ViewBase
    {
        private GameObject sellRightListUI, commodityUI, descriptionUI, changeLeftUI, buyBottomUI;

        #region BlackMarket Right List UI
        private ToggleGroup toggleGroup_RightList;
        private Toggle buyToggle, sellToggle, exchangeToggle;
        private Button videoButton;
        #endregion

        #region Commodity UI
        private GridLayoutGroup group;
        private ScrollRect dragGoodsPanel;
        private ToggleGroup toggleGroup_Goods;
        private Toggle nEMToggle, unitToggle, componentToggle, bluePrintToggle, gearToggle;
        private Text titleText_Commodity;
        private Image maskImage;
        #endregion

        #region Description UI
        private Image arrowImage;
        private Button buyButton, sellButton, addButton, lessButton;
        private Text clickText, titleText_Description, typeText, descriptionText, buyText, sellText;
        private InputField inputText;
        #endregion

        #region Change Left UI
        private Button[] changeButtons = new Button[6];
        private Image[] changeImages = new Image[6];
        private Text[] rateTexts = new Text[4];
        private Button chestButton;
        #endregion

        #region Buy Botton UI
        private Text refreshText, refreshTimeText, refreshNowText, refreshCostText;
        private Button refreshButton;
        #endregion

        private List<GoodsItem> goodsItems = new List<GoodsItem>();

        //private Data.PlayerBagItemType currentGoodsType = Data.PlayerBagItemType.GearItem;
        private BlackMarketType currentType = BlackMarketType.Buy;
        private int currentGoodsIndex = -1;
        private int sellGoodsCount = 1;

        #region Path

        #endregion

        private BlackMarketController controller;

        public override void OnEnter()
        {
            base.OnEnter();

            buyToggle.isOn = true;
            buyToggle.interactable = false;
        }

        public override void OnInit()
        {
            base.OnInit();
            controller = new BlackMarketController( this );
            _controller = controller;

            sellRightListUI = transform.Find( "SellRightListUI" ).gameObject;
            commodityUI = transform.Find( "CommodityUI" ).gameObject;
            descriptionUI = transform.Find( "DescriptionUI" ).gameObject;
            changeLeftUI = transform.Find( "ChangeLeftUI" ).gameObject;
            buyBottomUI = transform.Find( "BuyBottomUI" ).gameObject;

            changeLeftUI.gameObject.SetActive( false );

            #region BlackMarket Right List UI
            Transform tran_RightList = transform.Find( "SellRightListUI" );

            toggleGroup_RightList = tran_RightList.Find( "ToggleGroup" ).GetComponent<ToggleGroup>();
            buyToggle = tran_RightList.Find( "ToggleGroup/BuyToggle" ).GetComponent<Toggle>();
            sellToggle = tran_RightList.Find( "ToggleGroup/SellToggle" ).GetComponent<Toggle>();
            exchangeToggle = tran_RightList.Find( "ToggleGroup/ExchangeToggle" ).GetComponent<Toggle>();
            videoButton = tran_RightList.Find( "VideoButton" ).GetComponent<Button>();

            buyToggle.AddListener( OnClickBuyToggle );
            sellToggle.AddListener( OnClickSellToggle );
            exchangeToggle.AddListener( OnClickExchangeToggle );
            videoButton.AddListener( OnClickVideoButton );
            #endregion

            #region Commodity UI
            Transform tran_CommodityUI = transform.Find( "CommodityUI" );

            group = tran_CommodityUI.Find( "DragGoodsPanel/GoodsItemGroup" ).GetComponent<GridLayoutGroup>();
            dragGoodsPanel = tran_CommodityUI.Find( "DragGoodsPanel" ).GetComponent<ScrollRect>();
            toggleGroup_Goods = tran_CommodityUI.Find( "DragGoodsPanel/GoodsItemGroup" ).GetComponent<ToggleGroup>();
            nEMToggle = tran_CommodityUI.Find( "ToggleGroup/NEMToggle" ).GetComponent<Toggle>();
            unitToggle = tran_CommodityUI.Find( "ToggleGroup/UnitToggle" ).GetComponent<Toggle>();
            componentToggle = tran_CommodityUI.Find( "ToggleGroup/ComponentToggle" ).GetComponent<Toggle>();
            bluePrintToggle = tran_CommodityUI.Find( "ToggleGroup/BluePrintToggle" ).GetComponent<Toggle>();
            gearToggle = tran_CommodityUI.Find( "ToggleGroup/GearToggle" ).GetComponent<Toggle>();
            titleText_Commodity = tran_CommodityUI.Find( "TitleText" ).GetComponent<Text>();
            maskImage = tran_CommodityUI.Find( "MaskImage" ).GetComponent<Image>();

            nEMToggle.AddListener( OnClickNEMToggle );
            unitToggle.AddListener( OnClickUnitToggle );
            componentToggle.AddListener( OnClickComponentToggle );
            bluePrintToggle.AddListener( OnClickBluePrintToggle );
            gearToggle.AddListener( OnClickGearToggle );
            #endregion

            #region Description UI
            Transform tran_DescriptionUI = transform.Find( "DescriptionUI" );

            arrowImage = tran_DescriptionUI.Find( "ArrowImage" ).GetComponent<Image>();
            clickText = tran_DescriptionUI.Find( "ClickText" ).GetComponent<Text>();
            titleText_Description = tran_DescriptionUI.Find( "TitleText" ).GetComponent<Text>();
            typeText = tran_DescriptionUI.Find( "TypeText" ).GetComponent<Text>();
            descriptionText = tran_DescriptionUI.Find( "DescriptionText" ).GetComponent<Text>();
            buyText = tran_DescriptionUI.Find( "BuyText" ).GetComponent<Text>();
            sellText = tran_DescriptionUI.Find( "SellText" ).GetComponent<Text>();
            inputText = tran_DescriptionUI.Find( "InputField" ).GetComponent<InputField>();
            buyButton = tran_DescriptionUI.Find( "BuyButton" ).GetComponent<Button>();
            sellButton = tran_DescriptionUI.Find( "SellButton" ).GetComponent<Button>();
            addButton = tran_DescriptionUI.Find( "AddButton" ).GetComponent<Button>();
            lessButton = tran_DescriptionUI.Find( "LessButton" ).GetComponent<Button>();

            sellButton.gameObject.SetActive( false ); sellText.gameObject.SetActive( false );
            descriptionText.gameObject.SetActive( false ); titleText_Description.gameObject.SetActive( false ); typeText.gameObject.SetActive( false );

            buyButton.AddListener( OnClickBuyButton, UIEventGroup.Middle, UIEventGroup.Middle );
            sellButton.AddListener( OnClickSellButton, UIEventGroup.Middle, UIEventGroup.Middle );
            addButton.AddListener( OnClickAddButton );
            lessButton.AddListener( OnClickLessButton );

            inputText.onValueChanged.AddListener( OnInputValueChanage );
            #endregion

            #region Change Left UI
            Transform tran_ChangeLeft = transform.Find( "ChangeLeftUI" );

            changeImages[0] = tran_ChangeLeft.Find( "ItemGroup/ItemImage_1" ).GetComponent<Image>();
            changeImages[1] = tran_ChangeLeft.Find( "ItemGroup/ItemImage_2" ).GetComponent<Image>();
            changeImages[2] = tran_ChangeLeft.Find( "ItemGroup/ItemImage_3" ).GetComponent<Image>();
            changeImages[3] = tran_ChangeLeft.Find( "ItemGroup/ItemImage_4" ).GetComponent<Image>();
            changeImages[4] = tran_ChangeLeft.Find( "ItemGroup/ItemImage_5" ).GetComponent<Image>();
            changeImages[5] = tran_ChangeLeft.Find( "ItemGroup/ItemImage_6" ).GetComponent<Image>();
            rateTexts[0] = tran_ChangeLeft.Find( "RateText_1" ).GetComponent<Text>();
            rateTexts[1] = tran_ChangeLeft.Find( "RateText_2" ).GetComponent<Text>();
            rateTexts[2] = tran_ChangeLeft.Find( "RateText_3" ).GetComponent<Text>();
            rateTexts[3] = tran_ChangeLeft.Find( "RateText_4" ).GetComponent<Text>();
            changeButtons[0] = tran_ChangeLeft.Find( "ItemGroup/Item_1" ).GetComponent<Button>();
            changeButtons[1] = tran_ChangeLeft.Find( "ItemGroup/Item_2" ).GetComponent<Button>();
            changeButtons[2] = tran_ChangeLeft.Find( "ItemGroup/Item_3" ).GetComponent<Button>();
            changeButtons[3] = tran_ChangeLeft.Find( "ItemGroup/Item_4" ).GetComponent<Button>();
            changeButtons[4] = tran_ChangeLeft.Find( "ItemGroup/Item_5" ).GetComponent<Button>();
            changeButtons[5] = tran_ChangeLeft.Find( "ItemGroup/Item_6" ).GetComponent<Button>();
            chestButton = tran_ChangeLeft.Find( "ChestButton" ).GetComponent<Button>();

            changeButtons[0].AddListener( OnClickFirstItemButton, UIEventGroup.Middle, UIEventGroup.Middle | UIEventGroup.Top | UIEventGroup.Left );
            changeButtons[1].AddListener( OnClickSecondItemButton, UIEventGroup.Middle, UIEventGroup.Middle | UIEventGroup.Top | UIEventGroup.Left );
            changeButtons[2].AddListener( OnClickThirdItemButton, UIEventGroup.Middle, UIEventGroup.Middle | UIEventGroup.Top | UIEventGroup.Left );
            changeButtons[3].AddListener( OnClickFourthItemButton, UIEventGroup.Middle, UIEventGroup.Middle | UIEventGroup.Top | UIEventGroup.Left );
            changeButtons[4].AddListener( OnClickFifthItemButton, UIEventGroup.Middle, UIEventGroup.Middle | UIEventGroup.Top | UIEventGroup.Left );
            changeButtons[5].AddListener( OnClickSixthItemButton, UIEventGroup.Middle, UIEventGroup.Middle | UIEventGroup.Top | UIEventGroup.Left );
            chestButton.AddListener( OnClickChestButton, UIEventGroup.Middle, UIEventGroup.Middle | UIEventGroup.Top | UIEventGroup.Left );
            #endregion

            #region Buy Bottom UI
            Transform tran_Bottom = transform.Find( "BuyBottomUI" );

            refreshText = tran_Bottom.Find( "RefreshText" ).GetComponent<Text>();
            refreshTimeText = tran_Bottom.Find( "RefreshTimeText" ).GetComponent<Text>();
            refreshNowText = tran_Bottom.Find( "RefreshNowText" ).GetComponent<Text>();
            refreshCostText = tran_Bottom.Find( "RefreshCostText" ).GetComponent<Text>();
            refreshButton = tran_Bottom.Find( "RefreshButton" ).GetComponent<Button>();

            refreshButton.AddListener( OnClickRefreshButton, UIEventGroup.Middle, UIEventGroup.Middle | UIEventGroup.Top | UIEventGroup.Left );
            #endregion
        }

        #region Button and Toggle Event

        #region BlackMarket Right List UI

        private void OnClickBuyToggle( bool isOn )
        {
            buyToggle.interactable = !isOn;
            if ( !isOn ) return;

            currentType = BlackMarketType.Buy;

            SetBuyUI();
            SetDescriptionUI( false );
            SetGoodsCountUI( false );

            RefreshGoodsItem();

            currentGoodsIndex = -1;
        }

        private void OnClickSellToggle( bool isOn )
        {
            sellToggle.interactable = !isOn;
            if ( !isOn ) return;

            currentType = BlackMarketType.Sell;

            SetSellUI();
            SetDescriptionUI( false );
            SetGoodsCountUI( false );

            RefreshGoodsItem();

            currentGoodsIndex = -1;
        }

        private void OnClickExchangeToggle( bool isOn )
        {
            exchangeToggle.interactable = !isOn;
            if ( !isOn ) return;

            gearToggle.isOn = true;
            gearToggle.interactable = false;

            currentType = BlackMarketType.ExChange;

            SetExchangeUI();
            SetDescriptionUI( false );
            SetGoodsCountUI( false );

            ClearExChangeList();
            RefreshGoodsItem();
            RefreshExChangeGoods();

            currentGoodsIndex = -1;
        }

        private void OnClickVideoButton()
        {

        }

        #endregion

        #region Commodity UI

        private void OnClickNEMToggle( bool isOn )
        {
            nEMToggle.interactable = !isOn;
            if ( !isOn ) return;

            //currentGoodsType = Data.PlayerBagItemType.Nem;

            RefreshGoodsItem();
        }

        private void OnClickUnitToggle( bool isOn )
        {
            unitToggle.interactable = !isOn;
            if ( !isOn ) return;

            //currentGoodsType = Data.PlayerBagItemType.ExpItem;

            RefreshGoodsItem();
        }

        private void OnClickComponentToggle( bool isOn )
        {
            componentToggle.interactable = !isOn;
            if ( !isOn ) return;

            //currentGoodsType = Data.PlayerBagItemType.Component;

            RefreshGoodsItem();
        }

        private void OnClickBluePrintToggle( bool isOn )
        {
            bluePrintToggle.interactable = !isOn;
            if ( !isOn ) return;

            //currentGoodsType = Data.PlayerBagItemType.BluePrintItem;

            RefreshGoodsItem();
        }

        private void OnClickGearToggle( bool isOn )
        {
            gearToggle.interactable = !isOn;
            if ( !isOn ) return;

            //currentGoodsType = Data.PlayerBagItemType.GearItem;

            RefreshGoodsItem();
        }

        #endregion

        #region Description UI

        private void OnClickBuyButton()
        {
            SoundManager.Instance.PlaySound( 60009, true );

            if ( controller.HaveEnoughCurrencyBuyGoods( currentGoodsIndex ) )
                controller.SendBuyC2S( currentGoodsIndex );
        }

        private void OnClickSellButton()
        {
            SoundManager.Instance.PlaySound( 60009, true );

            //if ( currentGoodsIndex >= 0 )
                //controller.SendSellC2S( currentGoodsIndex, currentGoodsType, currentGoodsType, sellGoodsCount );
        }

        private void OnInputValueChanage( string input )
        {
            sellGoodsCount = int.Parse( input );

            //int goodsCount = controller.GetGoodsItemQuantity( currentType, currentGoodsType, currentGoodsIndex );

            if ( sellGoodsCount <= 1 ) sellGoodsCount = 1;
            else if ( sellGoodsCount >= goodsCount ) sellGoodsCount = goodsCount;

            inputText.text = sellGoodsCount.ToString();
        }

        private void OnClickAddButton()
        {
            //int goodsCount = controller.GetGoodsItemQuantity( currentType, currentGoodsType, currentGoodsIndex );

            if ( sellGoodsCount >= goodsCount ) return;
            ++sellGoodsCount;
            inputText.text = sellGoodsCount.ToString();
        }

        private void OnClickLessButton()
        {
            if ( sellGoodsCount <= 1 ) return;
            --sellGoodsCount;
            inputText.text = sellGoodsCount.ToString();
        }

        #endregion

        #region Change Left UI

        private void OnClickFirstItemButton()
        {
            ExChangeGoods( 1, changeImages[0] );
        }

        private void OnClickSecondItemButton()
        {
            ExChangeGoods( 2, changeImages[1] );
        }

        private void OnClickThirdItemButton()
        {
            ExChangeGoods( 3, changeImages[2] );
        }

        private void OnClickFourthItemButton()
        {
            ExChangeGoods( 4, changeImages[3] );
        }

        private void OnClickFifthItemButton()
        {
            ExChangeGoods( 5, changeImages[4] );
        }

        private void OnClickSixthItemButton()
        {
            ExChangeGoods( 6, changeImages[5] );
        }

        private void OnClickChestButton()
        {
            SoundManager.Instance.PlaySound( 60009, true );

            controller.SendOpenChestC2S();
        }

        #endregion

        #region Buy Bottom UI

        private void OnClickRefreshButton()
        {
            SoundManager.Instance.PlaySound( 60009, true );

            if ( controller.HaveEnoughCurrencyRefresh() )
                controller.SendRefreshC2S();
        }

        #endregion

        #endregion

        #region Init Item
        private int goodsCount;
        public void RefreshGoodsItem()
        {
            SetButton( false );
            SetDescriptionUI( false );
            SetGoodsCountUI( false );
            //goodsCount = controller.GetGoodsCount( currentType, currentGoodsType );

            GameResourceLoadManager.GetInstance().LoadResource( "Goods_Item", OnLoadGoodsItem, true );
        }

        private void OnLoadGoodsItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<GoodsItem>( goodsItems );

            DOTween.To( () => dragGoodsPanel.horizontalNormalizedPosition
, value => dragGoodsPanel.horizontalNormalizedPosition = value, 1, 0.3f );

            for ( int i = 0; i < goodsCount; i++ )
            {
                GoodsItem _goodsItem;
                if ( goodsItems.Count < goodsCount )
                {
                    _goodsItem = CommonUtil.CreateItem<GoodsItem>( obj, group.transform );
                    goodsItems.Add( _goodsItem );
                    _goodsItem.onClickToggle.group = toggleGroup_Goods;
                }
                _goodsItem = goodsItems[i];
                _goodsItem.gameObject.SetActive( true );
                toggleGroup_Goods.allowSwitchOff = true;
                _goodsItem.onClickToggle.isOn = false;
                toggleGroup_Goods.allowSwitchOff = false;

                _goodsItem.index = i;
                //_goodsItem.imageIcon = controller.GetGoodsItemIcon( currentType, currentGoodsType, i );
                //_goodsItem.quantity = controller.GetGoodsItemQuantity( currentType, currentGoodsType, i );
                //_goodsItem.cost = controller.GetGoodsItemCost( currentType, currentGoodsType, i );
                //_goodsItem.buyType = controller.GetGoodsItemCurrencyType( currentType, currentGoodsType, i );
                _goodsItem.isSellOut = controller.GetGoodsIsSoldOut( currentType, i );
                _goodsItem.clickGoodToggle = OnClickGoodsItem;

                _goodsItem.RefreshGoodsItem();
            }
        }

        #endregion

        #region ExChange 

        private void OnClickGoodsItem( int index, Image markImage, bool isSellOut )
        {
            //if ( ( controller.GetGoodsItemQuantity( currentType, currentGoodsType, index ) <= 0 ) || isSellOut )
            //{
            //    markImage.gameObject.SetActive( false );
            //    return;
            //}
            markImage.gameObject.SetActive( true );

            SetGoodsCountUI( currentType == BlackMarketType.Sell );
            SetDescriptionUI( true );
            SetButton( true );

            currentGoodsIndex = index;

            //titleText_Description.text = controller.GetGoodsItemName( currentType, currentGoodsType, index );
            //typeText.text = controller.GetGoodsItemType( currentType, currentGoodsType, index );
            //descriptionText.text = controller.GetGoodsItemDescription( currentType, currentGoodsType, index );
        }

        private void ExChangeGoods( int changeIndex, Image changeItemIcon )
        {
            if ( currentGoodsIndex < 0 ) return;

            //int goodsIcon = controller.GetGoodsItemIcon( currentType, currentGoodsType, currentGoodsIndex );

            //controller.exChangeItemList[changeIndex - 1] = controller.GetBagGoodsList( currentGoodsType )[currentGoodsIndex];

            RefreshExChangeGoods();
            RefreshGoodsItem();

            //GameResourceLoadManager.GetInstance().LoadUISprite( goodsIcon, ( string name, Sprite item, System.Object param ) =>
            //{
            //    if ( item != null )
            //    {
            //        changeItemIcon.sprite = item;
            //    }
            //    else
            //    {
            //        Debug.LogError( "Sprite is null~~~BlackMarketChangeItemImage" );
            //    }
            //}, true );

            currentGoodsIndex = -1;

            toggleGroup_Goods.allowSwitchOff = true;
            foreach ( GoodsItem item in goodsItems )
            {
                item.onClickToggle.isOn = false;
            }
            toggleGroup_Goods.allowSwitchOff = false;
        }

        public void RefreshExChangeGoods()
        {
            chestButton.interactable = controller.CanOpenChest();

            float[] rates = controller.GetExChangeRate( controller.exChangeItemList );
            for ( int i = 0; i < rates.Length; i++ )
            {
                rateTexts[i].text = string.Format( "{0:N1}", ( rates[i] * 100 ) ) + "%";
            }
        }

        public void ClearExChangeList()
        {
            controller.exChangeItemList = new List<Data.ItemInfo>() { new Data.ItemInfo(), new Data.ItemInfo(), new Data.ItemInfo(), new Data.ItemInfo(), new Data.ItemInfo(), new Data.ItemInfo() };

            foreach ( Image image in changeImages )
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( 18005, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
                {
                    image.SetSprite( atlasSprite );
                }, true );
            }
        }

        #endregion

        #region Set UI

        public void SetButton( bool isChooseGoods )
        {
            sellButton.interactable = isChooseGoods;
            buyButton.interactable = isChooseGoods;
        }

        private void SetBuyUI()
        {
            SetButton( false );

            titleText_Commodity.text = "黑市/购买";

            sellButton.gameObject.SetActive( false );
            sellText.gameObject.SetActive( false );
            buyButton.gameObject.SetActive( true );
            buyText.gameObject.SetActive( true );
            addButton.gameObject.SetActive( false );
            lessButton.gameObject.SetActive( false );
            inputText.gameObject.SetActive( false );

            buyBottomUI.SetActive( true );
            changeLeftUI.SetActive( false );
        }

        private void SetSellUI()
        {
            SetButton( false );

            titleText_Commodity.text = "黑市/卖出";

            sellButton.gameObject.SetActive( true );
            sellText.gameObject.SetActive( true );
            buyButton.gameObject.SetActive( false );
            buyText.gameObject.SetActive( false );
            maskImage.gameObject.SetActive( true );

            buyBottomUI.SetActive( false );
            changeLeftUI.SetActive( false );
        }

        private void SetExchangeUI()
        {
            SetButton( false );

            titleText_Commodity.text = "黑市/置换";

            maskImage.gameObject.SetActive( true );

            buyBottomUI.SetActive( false );
            changeLeftUI.SetActive( true );
        }

        public void SetDescriptionUI( bool isChoose )
        {
            arrowImage.gameObject.SetActive( !isChoose );
            clickText.gameObject.SetActive( !isChoose );

            titleText_Description.gameObject.SetActive( isChoose );
            typeText.gameObject.SetActive( isChoose );
            descriptionText.gameObject.SetActive( isChoose );
        }

        private void SetGoodsCountUI( bool haveCountUI )
        {
            addButton.gameObject.SetActive( haveCountUI );
            lessButton.gameObject.SetActive( haveCountUI );
            inputText.gameObject.SetActive( haveCountUI );
        }

        #endregion

        private void Update()
        {
            System.TimeSpan times = new System.DateTime( 2017, 4, 26, 19, 10, 0 ) - new System.DateTime( 1970, 1, 1, 0, 0, 0 );
            long nextTime = System.Convert.ToInt64( times.TotalSeconds );

            refreshTimeText.text = controller.GetTimeString();
            refreshCostText.text = controller.GetRefreshGoodsItemCount().ToString();
        }
    }
}
