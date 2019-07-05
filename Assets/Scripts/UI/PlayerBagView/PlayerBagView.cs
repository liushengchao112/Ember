using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Resource;
using Utils;

namespace UI
{
    public class PlayerBagView : ViewBase
    {
        private Transform playerBagTran;

        private GridLayoutGroup bagItemGroup;
        private ToggleGroup toggleGroup;
        private ScrollRect dragPlayerBagPanel;
        private Toggle allToggle, cardToggle, debrisToggle, boxToggle, runeToggle;
        private Button sellButton, useButton;
        private Image itemIcon, itemFrameImage1, itemFrameImage2, noItemImage, goldImage, sellBackgroundImage;
        private Text allText, cardText, debrisText, boxText, runeText, itemNameText, itemCountText, itemIntroduceText, itemSellText, itemNotSellText, itemPriceText, noitemText, sellText, useText;

        //PopUpPanel
        private Transform popUpPanel;
        private Image iconImage_p, frameImage1_p, frameImage2_p;
        private InputField inputText;
        private Text titleText_p, nameText_p, countText_p, describeText_p, propText_p, comfirmText_p, cancelText_p;
        private Button addButton_p, reduceButton_p, maxButton_p, comfirmButton_p, cancelButton_p, closeButton_p;

        private List<PlayerBagItem> playerBag_Items = new List<PlayerBagItem>();

        private PlayerBagController controller;
        private int currentItemIndex = -1;
        private int currentCount_p = 1;
        private int currentItemMaxCount;
        private int currentSelectType;//1-Use      2-Sell

        private Color myGray = new Color( 172 / (float)255, 172 / (float)255, 172 / (float)255, 1 );

        private Data.BagType bagType = Data.BagType.Bag;
        private Data.BagType currentBagType
        {
            get { return bagType; }
            set
            {
                bagType = value;
                RefreshBagItem();
            }
        }

        #region Path
        private const string PLAYER_BAG_ITEM_PATH = "PlayerBagItem";
        #endregion

        public override void OnInit()
        {
            base.OnInit();

            controller = new PlayerBagController( this );
            _controller = controller;

            playerBagTran = transform.Find( "PlayerBagTran" );

            #region View
            bagItemGroup = playerBagTran.Find( "DragBagItemPanel/BagItemGroup" ).GetComponent<GridLayoutGroup>();
            toggleGroup = playerBagTran.Find( "DragBagItemPanel/BagItemGroup" ).GetComponent<ToggleGroup>();
            dragPlayerBagPanel = playerBagTran.Find( "DragBagItemPanel" ).GetComponent<ScrollRect>();
            itemIcon = playerBagTran.Find( "ItemIcon" ).GetComponent<Image>();
            noItemImage = playerBagTran.Find( "NoItemImage" ).GetComponent<Image>();
            itemFrameImage1 = playerBagTran.Find( "ItemFrameImage1" ).GetComponent<Image>();
            itemFrameImage2 = playerBagTran.Find( "ItemFrameImage2" ).GetComponent<Image>();
            goldImage = playerBagTran.Find( "GoldIcon" ).GetComponent<Image>();
            sellBackgroundImage = playerBagTran.Find( "Background/SellBackgroundImage" ).GetComponent<Image>();
            allText = playerBagTran.Find( "TextGroup/AllText" ).GetComponent<Text>();
            cardText = playerBagTran.Find( "TextGroup/CardText" ).GetComponent<Text>();
            debrisText = playerBagTran.Find( "TextGroup/DebrisText" ).GetComponent<Text>();
            boxText = playerBagTran.Find( "TextGroup/BoxText" ).GetComponent<Text>();
            runeText = playerBagTran.Find( "TextGroup/RuneText" ).GetComponent<Text>();
            itemNameText = playerBagTran.Find( "ItemNameText" ).GetComponent<Text>();
            itemCountText = playerBagTran.Find( "ItemCountText" ).GetComponent<Text>();
            itemIntroduceText = playerBagTran.Find( "ItemIntroduceText" ).GetComponent<Text>();
            itemSellText = playerBagTran.Find( "ItemSellText" ).GetComponent<Text>();
            itemNotSellText = playerBagTran.Find( "ItemNotSellText" ).GetComponent<Text>();
            itemPriceText = playerBagTran.Find( "ItemPriceText" ).GetComponent<Text>();
            noitemText = playerBagTran.Find( "NoItemText" ).GetComponent<Text>();
            sellText = playerBagTran.Find( "SellText" ).GetComponent<Text>();
            useText = playerBagTran.Find( "UseText" ).GetComponent<Text>();
            allToggle = playerBagTran.Find( "ToggleGroup/AllToggle" ).GetComponent<Toggle>();
            cardToggle = playerBagTran.Find( "ToggleGroup/CardToggle" ).GetComponent<Toggle>();
            debrisToggle = playerBagTran.Find( "ToggleGroup/DebrisToggle" ).GetComponent<Toggle>();
            boxToggle = playerBagTran.Find( "ToggleGroup/BoxToggle" ).GetComponent<Toggle>();
            runeToggle = playerBagTran.Find( "ToggleGroup/RuneToggle" ).GetComponent<Toggle>();
            sellButton = playerBagTran.Find( "SellButton" ).GetComponent<Button>();
            useButton = playerBagTran.Find( "UseButton" ).GetComponent<Button>();

            allToggle.AddListener( OnClickAllToggle );
            cardToggle.AddListener( OnClickCardToggle );
            debrisToggle.AddListener( OnClickDebrisToggle );
            boxToggle.AddListener( OnClickBoxToggle );
            runeToggle.AddListener( OnClickRuneToggle );
            sellButton.AddListener( OnClickSellButton );
            useButton.AddListener( OnClickUseButton );
            #endregion
            #region PopUp
            popUpPanel = playerBagTran.Find( "PopUpPanel" );
            popUpPanel.gameObject.SetActive( false );
            iconImage_p = popUpPanel.Find( "IconImage" ).GetComponent<Image>();
            frameImage1_p = popUpPanel.Find( "FrameImage1" ).GetComponent<Image>();
            frameImage2_p = popUpPanel.Find( "FrameImage2" ).GetComponent<Image>();
            inputText = popUpPanel.Find( "InputField" ).GetComponent<InputField>();
            titleText_p = popUpPanel.Find( "TitleText" ).GetComponent<Text>();
            nameText_p = popUpPanel.Find( "NameText" ).GetComponent<Text>();
            countText_p = popUpPanel.Find( "CountText" ).GetComponent<Text>();
            describeText_p = popUpPanel.Find( "DescribeText" ).GetComponent<Text>();
            propText_p = popUpPanel.Find( "PropText" ).GetComponent<Text>();
            comfirmText_p = popUpPanel.Find( "ComfirmText" ).GetComponent<Text>();
            cancelText_p = popUpPanel.Find( "CancelText" ).GetComponent<Text>();
            addButton_p = popUpPanel.Find( "AddButton" ).GetComponent<Button>();
            reduceButton_p = popUpPanel.Find( "ReduceButton" ).GetComponent<Button>();
            maxButton_p = popUpPanel.Find( "MaxButton" ).GetComponent<Button>();
            comfirmButton_p = popUpPanel.Find( "ComfirmButton" ).GetComponent<Button>();
            cancelButton_p = popUpPanel.Find( "CancelButton" ).GetComponent<Button>();
            closeButton_p = popUpPanel.Find( "CloseButton" ).GetComponent<Button>();

            inputText.onValueChanged.AddListener( OnInputValueChanged );
            addButton_p.AddListener( OnClickAdd_pButton );
            reduceButton_p.AddListener( OnClickReduce_pButton );
            maxButton_p.AddListener( OnClickMax_pButton );
            comfirmButton_p.AddListener( OnClickComfirm_pButton );
            cancelButton_p.AddListener( OnClickClose_pButton );
            closeButton_p.AddListener( OnClickClose_pButton );
            #endregion
        }

        public override void OnEnter()
        {
            base.OnEnter();

            RefreshBagItem();
        }

        #region ToggleEvent

        private void OnClickSellButton()
        {
            currentSelectType = 2;
            OpenPopUp();
        }

        private void OnClickUseButton()
        {
            Data.BagType type = controller.GetPlayerBagItemType( currentBagType, currentItemIndex );

            if ( type == Data.BagType.RuneBag )
            {
                UIManager.Instance.GetUIByType( UIType.RuneMainUI, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); } );
                UIManager.Instance.GetUIByType( UIType.MainLeftBar, ( ViewBase ui, System.Object param ) => { ( ui as MainLeftBarView ).SetRuneClick(); } );
            }
            else if ( type == Data.BagType.DebrisBag )
            {
                UIManager.Instance.GetUIByType( UIType.StoreScreen, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); } );
                UIManager.Instance.GetUIByType( UIType.MainLeftBar, ( ViewBase ui, System.Object param ) => { ( ui as MainLeftBarView ).SetLeftBarNoClick(); } );
            }
            else
            {
                if ( controller.IsLoudspeaker( currentBagType, currentItemIndex ) )
                {
                    MessageDispatcher.PostMessage( Constants.MessageType.OpenLoudspeakerView );
                }
                else
                {
                    currentSelectType = 1;
                    OpenPopUp();
                }
            }
        }

        private void OnClickAllToggle( bool isOn )
        {
            allToggle.interactable = !isOn;
            if ( !isOn )
                return;

            SetTextToGray();
            allText.color = Color.white;

            currentBagType = Data.BagType.Bag;
        }

        private void OnClickCardToggle( bool isOn )
        {
            cardToggle.interactable = !isOn;
            if ( !isOn )
                return;

            SetTextToGray();
            cardText.color = Color.white;

            currentBagType = Data.BagType.ComplexBag;
        }

        private void OnClickDebrisToggle( bool isOn )
        {
            debrisToggle.interactable = !isOn;
            if ( !isOn )
                return;

            SetTextToGray();
            debrisText.color = Color.white;

            currentBagType = Data.BagType.DebrisBag;
        }

        private void OnClickBoxToggle( bool isOn )
        {
            boxToggle.interactable = !isOn;
            if ( !isOn )
                return;

            SetTextToGray();
            boxText.color = Color.white;

            currentBagType = Data.BagType.BoxBag;
        }

        private void OnClickRuneToggle( bool isOn )
        {
            runeToggle.interactable = !isOn;
            if ( !isOn )
                return;

            SetTextToGray();
            runeText.color = Color.white;

            currentBagType = Data.BagType.RuneBag;
        }

        private void OnClickAdd_pButton()
        {
            if ( currentCount_p >= currentItemMaxCount )
                return;
            ++currentCount_p;
            inputText.text = currentCount_p.ToString();
        }

        private void OnClickReduce_pButton()
        {
            if ( currentCount_p <= 1 )
                return;
            --currentCount_p;
            inputText.text = currentCount_p.ToString();
        }

        private void OnClickMax_pButton()
        {
            currentCount_p = currentItemMaxCount;
            inputText.text = currentCount_p.ToString();
        }

        private void OnClickComfirm_pButton()
        {
            Data.BagType type = controller.GetPlayerBagItemType( currentBagType, currentItemIndex );
            long itemId = controller.GetPlayerBagItemId( currentBagType, currentItemIndex );

            if ( currentSelectType == 2 )//Sell
                controller.SendSellItem( type, currentCount_p, itemId );
            else if ( currentSelectType == 1 )//Use
                controller.SendUseItem( type, currentCount_p, itemId );

            OnClickClose_pButton();

            RefreshBagView( currentBagType, 0 );
        }

        private void OnClickClose_pButton()
        {
            currentCount_p = 1;
            inputText.text = currentCount_p.ToString();
            popUpPanel.gameObject.SetActive( false );
        }

        private void OnInputValueChanged( string input )
        {
            currentCount_p = int.Parse( input );

            if ( currentCount_p <= 1 )
                currentCount_p = 1;
            else if ( currentCount_p >= currentItemMaxCount )
                currentCount_p = currentItemMaxCount;

            inputText.text = currentCount_p.ToString();
        }

        private void SetTextToGray()
        {
            allText.color = cardText.color = debrisText.color = boxText.color = runeText.color = myGray;
        }

        #endregion

        #region Init Item

        private int bagItemCount;

        public void RefreshBagItem()
        {
            bagItemCount = controller.GetPlayerBagListCount( currentBagType );
            SetItemState( bagItemCount > 0 );

            GameResourceLoadManager.GetInstance().LoadResource( "PlayerBagItem", OnLoadBagItem, true );
        }

        private void OnLoadBagItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<PlayerBagItem>( playerBag_Items );
            ResetItem();

            DG.Tweening.DOTween.To( () => dragPlayerBagPanel.verticalNormalizedPosition, value => dragPlayerBagPanel.verticalNormalizedPosition = value, 1, 0.3f );

            for ( int i = 0; i < bagItemCount; i++ )
            {
                PlayerBagItem bagItem;
                if ( playerBag_Items.Count < bagItemCount )
                {
                    bagItem = CommonUtil.CreateItem<PlayerBagItem>( obj, bagItemGroup.transform );

                    playerBag_Items.Add( bagItem );
                }
                bagItem = playerBag_Items[i];
                bagItem.gameObject.SetActive( true );

                bagItem.onClickEvent = OnBagItemClickCallBack;
                bagItem.index = i;
                bagItem.isRune = controller.IsRune( currentBagType, i );
                bagItem.icon = controller.GetPlayerBagItemIcon( currentBagType, i );
                bagItem.count = controller.GetPlayerBagItemCount( currentBagType, i );
                bagItem.onclickToggle.group = toggleGroup;

                bagItem.RefreshItem();
            }

            if ( currentItemIndex <= 0 && bagItemCount > 0 )
            {
                playerBag_Items[0].onclickToggle.isOn = true;
                playerBag_Items[0].onclickToggle.interactable = false;
                OnBagItemClickCallBack( 0 );
            }
        }

        private void OnBagItemClickCallBack( int index )
        {
            currentItemIndex = index;
            currentItemMaxCount = controller.GetPlayerBagItemCount( currentBagType, index );
            RefreshBagView( currentBagType, index );
        }

        #endregion

        #region Set PopUp

        private void OpenPopUp()
        {
            if ( currentSelectType == 1 )//Use
                titleText_p.text = "使用";
            else//Sell
                titleText_p.text = "出售";

            nameText_p.text = controller.GetPlayerBagItemName( currentBagType, currentItemIndex );
            countText_p.text = "<color=#d5d5d5>拥有:</color> " + controller.GetPlayerBagItemCount( currentBagType, currentItemIndex );
            describeText_p.text = controller.GetPlayerBagItemDescribe( currentBagType, currentItemIndex );
            propText_p.text = controller.GetItemProp( currentBagType, currentItemIndex );

            bool isRune = controller.IsRune( currentBagType, currentItemIndex );
            frameImage1_p.gameObject.SetActive( !isRune );
            frameImage2_p.gameObject.SetActive( isRune );

            int icon = controller.GetPlayerBagItemIcon( currentBagType, currentItemIndex );
            if ( icon != 0 )
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    iconImage_p.SetSprite( atlasSprite );
                }, true );
            }

            popUpPanel.gameObject.SetActive( true );
        }

        #endregion

        private void ResetItem()
        {
            currentItemIndex = -1;
            toggleGroup.allowSwitchOff = true;
            for ( int i = 0; i < playerBag_Items.Count; i++ )
            {
                playerBag_Items[i].onclickToggle.isOn = false;
            }
            toggleGroup.allowSwitchOff = false;
        }

        public void RefreshBagView( Data.BagType type, int index )
        {
            itemNameText.text = controller.GetPlayerBagItemName( type, index );
            itemCountText.text = "<color=#d5d5d5>拥有:</color> " + controller.GetPlayerBagItemCount( type, index );
            itemIntroduceText.text = controller.GetPlayerBagItemDescribe( type, index );
            itemPriceText.text = controller.GetPlayerBagItemPrice( type, index ).ToString();

            int icon = controller.GetPlayerBagItemIcon( type, index );
            if ( icon != 0 )
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    itemIcon.SetSprite( atlasSprite );
                }, true );
            }

            bool canShell = controller.GetPlayerBagItemIsShell( type, index ) == 1;
            sellButton.interactable = canShell;
            itemSellText.gameObject.SetActive( canShell );
            goldImage.gameObject.SetActive( canShell );
            itemNotSellText.gameObject.SetActive( !canShell );
            itemPriceText.gameObject.SetActive( canShell );

            bool notUse = controller.GetPlayerBagItemUseType( type, index ) == 0;
            useButton.gameObject.SetActive( !notUse );
            useText.gameObject.SetActive( !notUse );

            bool isRune = controller.IsRune( type, index );
            itemFrameImage1.gameObject.SetActive( !isRune );
            itemFrameImage2.gameObject.SetActive( isRune );
        }

        public void SetItemState( bool haveItem )
        {
            noItemImage.gameObject.SetActive( !haveItem );
            noitemText.gameObject.SetActive( !haveItem );
            itemFrameImage1.gameObject.SetActive( haveItem );
            itemFrameImage2.gameObject.SetActive( haveItem );
            itemIcon.gameObject.SetActive( haveItem );
            goldImage.gameObject.SetActive( haveItem );
            sellBackgroundImage.gameObject.SetActive( haveItem );
            sellButton.gameObject.SetActive( haveItem );
            itemNotSellText.gameObject.SetActive( haveItem );
            sellText.gameObject.SetActive( haveItem );
            useButton.gameObject.SetActive( haveItem );
            useText.gameObject.SetActive( haveItem );
            itemNameText.gameObject.SetActive( haveItem );
            itemCountText.gameObject.SetActive( haveItem );
            itemIntroduceText.gameObject.SetActive( haveItem );
            itemSellText.gameObject.SetActive( haveItem );
            itemPriceText.gameObject.SetActive( haveItem );
        }
    }
}
