using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

using Resource;
using Utils;
using DG.Tweening;

namespace UI
{
    public class PlayerInfoView : ViewBase
    {
        #region Component

        #region PlayerInfoTransform
        private Transform playerInfoT;
        private Button playerIconBt, changeNameBt, playerLightBt, addFriendBt, fightRecordBt, closeButton_p;
        private Slider levelUpBarSlider;
        private Image playerIcon;
        private Text playerLVText, playerNameText, playerIdText, xpText, playerLightScoreText, playerSegmentGroupText, playerRankText, unitCountText, skinCountText, fightCountText, mode1v1WinRateText, mode2v2WinRateText, addFriendBtText;
        private ScrollRect dragFightRecordItemPanel;
        private GridLayoutGroup fightRecordItemGroup;
        private InputField inputText_sign;
        #endregion
        #region HistoricalFightRecordTransform
        private Transform historicalFightRecordT;
        private ScrollRect dragDetailedRecordItemPanel;
        private GridLayoutGroup detailedRecordItemGroup;
        private Button backButton;
        #endregion
        #region ChangePlayerPortraitTransform
        private Transform changePlayerPortraitT;
        private ScrollRect dragPlayerPortraitItemPanel;
        private ToggleGroup playerPortaitItemTgGroup;
        private GridLayoutGroup playerPortraitItemGroup;
        private Button closeButton_c, confirmButton_c;
        #endregion
        #region ChangeNameTransform
        private Transform changeNameTransform;
        private InputField input_name;
        private Text diamondCost;
        private Button confirmButton_n, closeButton_n;
        #endregion
        #region LightRuleTransform
        private Transform lightRuleTransform;
        private Text contentText_r;
        private Button closeButton_r;
        #endregion
        #region PlayerLightTransform
        private Transform playerLightTransform;
        private Text currentStateText, contentText_l, lightScoreText_l;
        private Button closeButton_l, lightRuleButton;
        #endregion

        #endregion

        private PlayerInfoController controller;
        private FightRecordScrollView fightRecordScrollView;
        private DetailedRecordScrollView detailedRecordScrollView;

        private List<PlayerPortraitItem> playerPortrait_Items = new List<PlayerPortraitItem>();

        public bool openBattleResult = false;
        public bool openDetailedRecord = false;

        private int currentPortraitIndex = -1;

        #region Path
        private const string FIGHT_RECORD_ITEM_PATH = "FightRecord_Item";
        private const string DETAILED_RECORD_ITEM_PATH = "DetailedRecord_Item";
        private const string PLAYER_PORTRAIT_ITEM_PATH = "PlayerPortrait_Item";
        #endregion

        public override void OnInit()
        {
            base.OnInit();
            _controller = new PlayerInfoController( this );
            controller = ( _controller as PlayerInfoController );

            #region PlayerInfoTransform
            playerInfoT = transform.Find( "PlayerInfoTransform" );
            dragFightRecordItemPanel = playerInfoT.Find( "DragFightRecordItemPanel" ).GetComponent<ScrollRect>();
            fightRecordItemGroup = playerInfoT.Find( "DragFightRecordItemPanel/FightRecordItemGroup" ).GetComponent<GridLayoutGroup>();
            playerIcon = playerInfoT.Find( "PlayerIconBt" ).GetComponent<Image>();
            playerIconBt = playerInfoT.Find( "PlayerIconBt" ).GetComponent<Button>();
            changeNameBt = playerInfoT.Find( "ChangeNameBt" ).GetComponent<Button>();
            playerLightBt = playerInfoT.Find( "PlayerLightBt" ).GetComponent<Button>();
            addFriendBt = playerInfoT.Find( "AddFriendButton" ).GetComponent<Button>();
            fightRecordBt = playerInfoT.Find( "FightRecordButton" ).GetComponent<Button>();
            closeButton_p = playerInfoT.Find( "CloseButton" ).GetComponent<Button>();
            levelUpBarSlider = playerInfoT.Find( "LevelUpBar/SliderMask/LevelUpBarSlider" ).GetComponent<Slider>();
            playerLVText = playerInfoT.Find( "PlayerLVText" ).GetComponent<Text>();
            playerNameText = playerInfoT.Find( "PlayerNameText" ).GetComponent<Text>();
            playerIdText = playerInfoT.Find( "PlayerIDText" ).GetComponent<Text>();
            xpText = playerInfoT.Find( "XPText" ).GetComponent<Text>();
            playerLightScoreText = playerInfoT.Find( "PlayerLightScoreText" ).GetComponent<Text>();
            playerSegmentGroupText = playerInfoT.Find( "PlayerRankText" ).GetComponent<Text>();
            playerRankText = playerInfoT.Find( "PlayerRankScoreText" ).GetComponent<Text>();
            unitCountText = playerInfoT.Find( "UnitCountText" ).GetComponent<Text>();
            skinCountText = playerInfoT.Find( "SkinCountText" ).GetComponent<Text>();
            fightCountText = playerInfoT.Find( "FightCountText" ).GetComponent<Text>();
            mode1v1WinRateText = playerInfoT.Find( "Mode1v1WinRateText" ).GetComponent<Text>();
            mode2v2WinRateText = playerInfoT.Find( "Mode2v2WinRateText" ).GetComponent<Text>();
            addFriendBtText = playerInfoT.Find( "AddFriendBtText" ).GetComponent<Text>();
            inputText_sign = playerInfoT.Find( "InputField" ).GetComponent<InputField>();

            if ( dragFightRecordItemPanel.GetComponent<FightRecordScrollView>() == null )
                fightRecordScrollView = dragFightRecordItemPanel.gameObject.AddComponent<FightRecordScrollView>();
            else
                fightRecordScrollView = dragFightRecordItemPanel.GetComponent<FightRecordScrollView>();
            fightRecordScrollView.onCreateItemHandler = OnCreateFightRecordItem;

            playerIconBt.AddListener( ClickPlayerIconButton );
            changeNameBt.AddListener( ClickChangeNameButton );
            playerLightBt.AddListener( ClickPlayerLightButton );
            addFriendBt.AddListener( ClickAddFriendButton );
            fightRecordBt.AddListener( ClickFightRecordButton );
            inputText_sign.onEndEdit.AddListener( ChangeSignValue );
            closeButton_p.AddListener( ClickCloseButton_p );
            #endregion
            #region HistoricalFightRecordTransform
            historicalFightRecordT = transform.Find( "HistoricalFightRecordTransform" );
            dragDetailedRecordItemPanel = historicalFightRecordT.Find( "DragDetailedRecordItemPanel" ).GetComponent<ScrollRect>();
            detailedRecordItemGroup = historicalFightRecordT.Find( "DragDetailedRecordItemPanel/DetailedRecordItemGroup" ).GetComponent<GridLayoutGroup>();
            backButton = historicalFightRecordT.Find( "BackButton" ).GetComponent<Button>();

            if ( dragDetailedRecordItemPanel.GetComponent<DetailedRecordItem>() == null )
                detailedRecordScrollView = dragDetailedRecordItemPanel.gameObject.AddComponent<DetailedRecordScrollView>();
            else
                detailedRecordScrollView = dragDetailedRecordItemPanel.GetComponent<DetailedRecordScrollView>();
            detailedRecordScrollView.onCreateItemHandler = OnCreateDetailedRecordItem;

            backButton.AddListener( ClickBackButton );
            #endregion
            #region ChangePlayerPortraitTransform
            changePlayerPortraitT = transform.Find( "ChangePlayerPortraitTransform" );
            dragPlayerPortraitItemPanel = changePlayerPortraitT.Find( "DragPlayerPortraitItemPanel" ).GetComponent<ScrollRect>();
            playerPortraitItemGroup = changePlayerPortraitT.Find( "DragPlayerPortraitItemPanel/PlayerPortraitItemGroup" ).GetComponent<GridLayoutGroup>();
            playerPortaitItemTgGroup= changePlayerPortraitT.Find( "DragPlayerPortraitItemPanel/PlayerPortraitItemGroup" ).GetComponent<ToggleGroup>();
            closeButton_c = changePlayerPortraitT.Find( "CloseButton" ).GetComponent<Button>();
            confirmButton_c = changePlayerPortraitT.Find( "ConfirmButton" ).GetComponent<Button>();

            closeButton_c.AddListener( ClickCloseButton_C );
            confirmButton_c.AddListener( ClickConfirmButton_C, UIEventGroup.Middle, UIEventGroup.Middle );
            #endregion
            #region ChangeNameTransform
            changeNameTransform = transform.Find( "ChangeNameTransform" );
            diamondCost = changeNameTransform.Find( "DiamondCost" ).GetComponent<Text>();
            input_name = changeNameTransform.Find( "InputField" ).GetComponent<InputField>();
            confirmButton_n = changeNameTransform.Find( "ConfirmButton" ).GetComponent<Button>();
            closeButton_n = changeNameTransform.Find( "CloseButton" ).GetComponent<Button>();

            input_name.onValueChanged.AddListener( ChangeNameValue );
            confirmButton_n.AddListener( ClickConfirmButton_N, UIEventGroup.Middle, UIEventGroup.Middle );
            closeButton_n.AddListener( ClickCloseButton_N );
            #endregion
            #region LightRuleTransform
            lightRuleTransform = transform.Find( "PlayerLightRuleTransform" );
            contentText_r = lightRuleTransform.Find( "ContentText" ).GetComponent<Text>();
            closeButton_r = lightRuleTransform.Find( "CloseButton" ).GetComponent<Button>();

            closeButton_r.AddListener( ClickCloseButton_R );
            #endregion
            #region PlayerLightTransform
            playerLightTransform = transform.Find( "PlayerLightTransform" );
            currentStateText = playerLightTransform.Find( "CurrentStateText" ).GetComponent<Text>();
            contentText_l = playerLightTransform.Find( "ContentText" ).GetComponent<Text>();
            lightScoreText_l = playerLightTransform.Find( "LightScoreText" ).GetComponent<Text>();
            closeButton_l = playerLightTransform.Find( "CloseButton" ).GetComponent<Button>();
            lightRuleButton = playerLightTransform.Find( "RuleButton" ).GetComponent<Button>();

            closeButton_l.AddListener( ClickCloseButton_L );
            lightRuleButton.AddListener( ClickLightRuneButton );
            #endregion

            RefreshFightRecordItem();
            RefreshDetailRecordItem();
            RefreshPlayerPortraitItem();
        }

        public override void OnEnter()
        {
            base.OnEnter();

            playerInfoT.gameObject.SetActive( !openDetailedRecord );
            historicalFightRecordT.gameObject.SetActive( openDetailedRecord );
            changePlayerPortraitT.gameObject.SetActive( false );
            changeNameTransform.gameObject.SetActive( false );
            lightRuleTransform.gameObject.SetActive( false );
            playerLightTransform.gameObject.SetActive( false );

            openDetailedRecord = false;
            controller.SendAccountInfomationC2S();
        }

        #region Button & InputField Event

        #region PlayerInfoTransform

        private void ClickPlayerIconButton()
        {
            changePlayerPortraitT.gameObject.SetActive( true );
        }

        private void ClickChangeNameButton()
        {
            changeNameTransform.gameObject.SetActive( true );
        }

        private void ClickPlayerLightButton()
        {
            playerLightTransform.gameObject.SetActive( true );
        }

        private void ClickAddFriendButton()
        {

        }

        private void ClickFightRecordButton()
        {
            controller.SendBattleRecordC2S();
        }

        private void ClickCloseButton_p()
        {
            if ( openBattleResult )
                UIManager.Instance.EnterMainMenu();
            else
                OnExit( true );
        }

        private void ChangeSignValue( string value )
        {
            controller.SendSignC2S( value );
        }

        #endregion

        #region HistoricalFightRecordTransform

        private void ClickBackButton()
        {
            historicalFightRecordT.gameObject.SetActive( false );
            playerInfoT.gameObject.SetActive( true );

            RefreshFightRecordListData();
        }

        #endregion

        #region ChangePlayerPortraitTranform

        private void ClickCloseButton_C()
        {
            changePlayerPortraitT.gameObject.SetActive( false );
        }

        private void ClickConfirmButton_C()
        {
            if ( currentPortraitIndex >= 0 )
                controller.SendChangePortraitC2S( currentPortraitIndex );
        }

        #endregion

        #region ChangeNameTranform

        private string inputText_name;
        private void ChangeNameValue( string value )
        {
            inputText_name = value;
        }

        private void ClickConfirmButton_N()
        {
            controller.SendChangeNameC2S( inputText_name );
        }

        private void ClickCloseButton_N()
        {
            changeNameTransform.gameObject.SetActive( false );
        }

        #endregion

        #region LightRuleTranform

        private void ClickCloseButton_R()
        {
            lightRuleTransform.gameObject.SetActive( false );
        }

        #endregion

        #region PlayerLightTranform

        private void ClickCloseButton_L()
        {
            playerLightTransform.gameObject.SetActive( false );
        }

        private void ClickLightRuneButton()
        {
            playerLightTransform.gameObject.SetActive( false );
            lightRuleTransform.gameObject.SetActive( true );
        }

        #endregion

        #endregion

        #region Init Fight Record Item

        private void OnCreateFightRecordItem( FightRecordItem item )
        {

        }

        private void RefreshFightRecordItem()
        {
            Resource.GameResourceLoadManager.GetInstance().LoadResource( FIGHT_RECORD_ITEM_PATH, OnLoadFightRecordItem, true );
        }

        private void OnLoadFightRecordItem( string name, UnityEngine.Object obj, System.Object param )
        {
            fightRecordScrollView.InitDataBase( dragFightRecordItemPanel, obj, 1, 660, 60, 0, new Vector3( 0, 105, 0 ) );
            if ( controller.GetFightRecordDataList().Count > 0 )
                RefreshFightRecordListData();
        }

        private void RefreshFightRecordListData()
        {
            fightRecordScrollView.InitializeWithData( controller.GetFightRecordDataList() );
        }

        #endregion

        #region Init Detailed Record Item

        private void OnCreateDetailedRecordItem( DetailedRecordItem item )
        {
            item.clickBattleResultEvent = ClickDetailedRecordBattleResultButtonEvent;
        }

        private void ClickDetailedRecordBattleResultButtonEvent( DetailedRecordItem item )
        {
            controller.SendBattleDetailRecordC2S( item.info.battleId );
        }

        private void RefreshDetailRecordItem()
        {
            Resource.GameResourceLoadManager.GetInstance().LoadResource( DETAILED_RECORD_ITEM_PATH, OnLoadDetailedRecordItem, true );
        }

        private void OnLoadDetailedRecordItem( string name, UnityEngine.Object obj, System.Object param )
        {
            detailedRecordScrollView.InitDataBase( dragDetailedRecordItemPanel, obj, 1, 1066.7f, 82.7f, 0, new Vector3( 0, 208, 0 ) );
        }

        public void RefreshDetailedRecordListData()
        {
            playerInfoT.gameObject.SetActive( false );
            historicalFightRecordT.gameObject.SetActive( true );

            detailedRecordScrollView.InitializeWithData( controller.GetDetailedRecordDataList() );
        }

        #endregion

        #region Init Change Player Portrait Item

        private int playerPortraitCount = 12;
        private void RefreshPlayerPortraitItem()
        {
            Resource.GameResourceLoadManager.GetInstance().LoadResource( PLAYER_PORTRAIT_ITEM_PATH, OnLoadPlayerPortraitItem, true );
        }

        private void OnLoadPlayerPortraitItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<PlayerPortraitItem>( playerPortrait_Items );

            for ( int i = 0; i < playerPortraitCount; i++ )
            {
                PlayerPortraitItem portraitItem;
                if ( playerPortrait_Items.Count < playerPortraitCount )
                {
                    portraitItem = CommonUtil.CreateItem<PlayerPortraitItem>( obj, playerPortraitItemGroup.transform );

                    playerPortrait_Items.Add( portraitItem );
                }
                portraitItem = playerPortrait_Items[i];
                portraitItem.gameObject.SetActive( true );

                portraitItem.index = i;
                portraitItem.icon = controller.GetPortraitIcon( i );
                portraitItem.clickToggleCallBack = ClickToggleCallBack;
                portraitItem.playerPortraitToggle.group = playerPortaitItemTgGroup;

                portraitItem.RefreshItem();
            }
        }

        private void ClickToggleCallBack( int index )
        {
            currentPortraitIndex = index;
        }

        #endregion

        private void SetAddFriendBt( bool state )
        {
            addFriendBt.gameObject.SetActive( state );
            addFriendBtText.gameObject.SetActive( state );
        }

        public void RefreshPlayerInfoView()
        {
            string icon = controller.GetPlayerIcon();
            if ( !string.IsNullOrEmpty( icon ) )
                Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    playerIcon.SetSprite( atlasSprite );
                }, true );
            levelUpBarSlider.value = controller.GetXPValue();

            RefreshName( controller.GetPlayerName() );
            playerLVText.text = controller.GetPlayerLv();
            xpText.text = controller.GetPlayerXP();

            if ( !string.IsNullOrEmpty( controller.GetPlayerSign() ) )
                inputText_sign.text = controller.GetPlayerSign();

            playerIdText.text = controller.GetPlayerId();
            playerSegmentGroupText.text = controller.GetPlayerSegmentGroup();
            playerRankText.text = controller.GetPlayerRank();
            unitCountText.text = controller.GetPlayerUnitCount();
            skinCountText.text = controller.GetPlayerSkinCount();
            fightCountText.text = controller.GetPlayerFightCount();
            mode1v1WinRateText.text = controller.GetPlayer1v1WinRate();
            mode2v2WinRateText.text = controller.GetPlayer2v2WinRate();

            addFriendBt.gameObject.SetActive( !controller.IsMyself() );
            addFriendBtText.gameObject.SetActive( !controller.IsMyself() );

            RefreshFightRecordListData();
        }

        public void RefreshName( string name )
        {
            playerNameText.text = name;
        }

        public void CloseChangeNameUI()
        {
            changeNameTransform.gameObject.SetActive( false );
        }

        public void CloseChangePortraitUI()
        {
           changePlayerPortraitT.gameObject.SetActive( false );
        }

        public void OpenBattleResultView( List<Data.BattleInfo> dataList )
        {
            UIManager.Instance.GetUIByType( UIType.BattleResultScreen, ( ViewBase ui, System.Object param ) => {
                if ( !ui.openState )
                {
                    ui.OnEnter();
                    openBattleResult = true;
                    ( ui as BattleResultView ).SetIsMainUIOpen( true );
                    ( ui as BattleResultView ).SetBattleInfo( dataList );
                }
            } );
        }
    }
}
