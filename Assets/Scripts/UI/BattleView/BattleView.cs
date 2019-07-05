using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

using Data;
using Utils;
using Resource;
using Constants;

namespace UI
{
    public class BattleView : ViewBase
    {
        private enum BattleUIType
        {            
            None = 0,
            LittleMap = 1,
            BuffUI = 2,
            BattleInfoUI = 3,
            InstitutePopUp = 4,
        }

        List<BattleUIType> battleUITypeList = new List<BattleUIType>();

        BattleViewController controller;

        private string powerUpNotificationText = "";

        public GameObject deploymentImage;

        #region ComponentName

        //Battle Left Panel
        private GameObject _leftPanel;
        private GridLayoutGroup _unitCards;
        private Button _quitButton;
        private Text _quitText;

        private Button showUnitButton;

        private GameObject diskRoot;

        //Battle Left Top Panel
        private GameObject _leftTopPanel;
        private Transform mapParent;
        private Button largerButton, resetButton , microphoneButton , loudspeakerButton;
        private GameObject largerButtonBg , resetButtonBg , microphoneButtonBg , loudspeakerButtonBg;
        private Image killImage, deathImage, emberImage;
        private Text _emberText, _killCountText, _killedCountText;

        //Battle Right Top Panel
		private Button settingButton;
        private Button fightingButton;        

        //Battle Top Panel
        private Text redSideKillCountText, blueSideKillCountText;

        //timer panel
        private Text _timerText;
        private GameObject battleTopPanel;

		//BattleChat 
		private BattleChatType chatType = BattleChatType.party;

		private Button chatChannelSwitchButton;

		private Transform chatElementsPanel;
		private Text chatChannel_PartyText;
		private Text chatChannel_AllPlayerText;
		private InputField chatInput;
        private Image chatBtnImage, districtImage, teamImage;
        private bool isClickDownChatBtn = false;
        private float clickDownChatBtnTime = 0;

		private List<Text> chatMessageTextList;

		private string playerNickName;

		private float chatTextHideTimer = 0;
		private float chatTextHideTimeLimit = 5f;

		private bool isChatTextNeedTimer = false;

        //centerpanel
        private GameObject _centerPanel;

        //Setting PopUp Panel
        private GameObject _settingPopupPanel;
        private Text redHeaderText, blueHeaderText;
        private GridLayoutGroup redSideGroup, blueSideGroup;
        private Button surrenderButton, _closeButton;
        private Text countdownText, surrenderText;

        //notification
        private GameObject _battleNotification;
        private Text _battleNotificationText;
        private Vector3 _battleNotificationReferencePoint;

        //Battle Bottom Panel
        private GameObject _bottomPanel;
        private Image _waitingSquadImage;
        private GridLayoutGroup _squadCards;
        private Transform demolisherAndCrystalCar;

        //Battle_right_panel
        private Transform battleRightPanel;
        private BattleCommandControlView battleCommandControlView;
        private Transform fetterRoot;

        //Battle Rigth Bottom Panel
        private GameObject rightBottomPanel, rightBottomPosition, rightTopPostion;
        public GameObject introductionPopUp;
        private Text titleText, contentText;

		//BuildInstitutePopUp
		[HideInInspector]
		public string buildInstitutePopUpTitleStr = "是否建造研究所？";
		[HideInInspector]
		public string buildInstitutePopUpStr = "将要消耗{0}资源建造研究所";

        //InstitutePopUp
        private GameObject institutePopUp;
		//institute now level, levelUp cost , value1 , value2, value3, value4,buttontext
		private Text instituteLevelText, levelUpCostText, institutePropText1, institutePropText2, institutePropText3, institutePropText4, institutelevelUpText;
        private Button instituteCloseButton, instituteLevelUpButton;
		private RectTransform instituteArrowIconRectTransform;
		private Vector3 instituteArrowIconLocalPos;

        private GridLayoutGroup instituteSkillGroup;
		private bool isInstituteUpgrading = false;
		private int InstituteMaxLv = 0;
		private string instituteLvStr = "LV:";
		private string instituteHpStr = "HP:";
		private List<Text> instituteHpTextList; 
		private List<Vector3> instituteHpTextRectTransformList;

        //TowerPopUP
        private GameObject towerPopUp;
        private Button towerCloseButton;
        private GridLayoutGroup towerItemGroup;

        //TapTowerPopUp
        private GameObject tapTowerPopUp;
		private TapTowerPopUp tapTowerPopUpItem;

		//BuildingBuffPanel
		private RectTransform buildingBuffPanelRect;
		private float originalPanelHeight;
		private RectTransform attributeTextRect;
		private Text attributeText;
		private float originalTextHeight;
		private Button buffPanelArrowButton;
		private float spaceText;
		private bool isSpread = false; 

        //Surrender
        private SurrenderTips surrenderTips;

        private int battleCanActivateTime;

        //NoticeView
        public NoticeView noticeView;

        //OrderNoticeView
        public CommandNoticeView commandNoticeView;

		//Player Ember
		public int playerEmber;

        //GreetingView
        private GreetingView greetingView;

        #endregion

        Dictionary<long , UnitCardItemTypeTwo> unitCardItems_TypeTwo = new Dictionary<long , UnitCardItemTypeTwo>();
        private UnitCardItemTypeTwo[] unitCardItemGroup = new UnitCardItemTypeTwo[6];

        private List<UnitCardItem> unitCard_Items = new List<UnitCardItem>();
		private List<UnitCardItem> unitCardPool = new List<UnitCardItem>();
        private List<SquadCardItem> squadCard_Items = new List<SquadCardItem>();
        private List<SpecialCardItem> specialCard_Items = new List<SpecialCardItem>();
        private List<InstituteSkillItem> instituteSkill_Items;
        private List<SettingFightingItem> fighting_Items = new List<SettingFightingItem>();
        private List<TowerItem> tower_Items = new List<TowerItem>();

        private Queue<string> _notificationQueue = new Queue<string>();

        private SquadData _waitingSquad;

        #region Path
        
        private const string UNIT_CARD_ITEM_PATH = "UnitCard_Item";
        private const string SQUAD_CARD_ITEM_PATH = "SquadCard_Item";
        private const string MAP_DOT_ITEM_PATH = "Dot_Item";
        private const string SPECIAL_CARD_ITEM_PATH = "SpecialCardItem";
        private const string SETTING_FIGHTING_ITEM_PATH = "FightingItem";
        private const string INSTITUTE_SKILL_ITEM_PATH = "InstituteSkillItem";
        private const string SETTING_FIGHTING_RED_ITEM_PATH = "FightingRedItem";
        private const string SETTING_FIGHTING_BULE_ITEM_PATH = "FightingItem";
        private const string TOWER_ITEM_PATH = "TowerItem";

        private const string CENTER_PANEL = "BattleCenterPanel";
        private const string SETTING_POPUP = "SettingPopUp";
        private const string LEFT_PANEL = "BattleLeftPanel";
        private const string LEFT_TOP_PANEL = "BattleLeftTopPanel";
        private const string RIGHT_TOP_PANEL = "BattleRightTopPanel";
        private const string BOTTOM_PANEL = "BattleBottomPanel";
        private const string BATTLE_RESULT_PANEL = CENTER_PANEL + "/BattleResult/BattleResultPanel";
        private const string TOP_PANEL = "BattleTopPanel";
        private const string BATTLE_RIGHT_BOTTOM_PANEL = "BattleRightBottomPanel";
        private const string BATTLE_RIGHT_PANEL = "BattleRightPanel";
        private const string SPECIAL_CARD_ITEM_GROUP = BATTLE_RIGHT_BOTTOM_PANEL + "/SpecialCardItemGroup";
        private const string SHRINK_BUTTON = BATTLE_RIGHT_BOTTOM_PANEL + "/ShrinkButton";
        private const string INTRODUCTION_POPUP = BATTLE_RIGHT_BOTTOM_PANEL + "/IntroductionPopUp";
        private const string INTRODUCTION_POPUP_TITLETEXT = INTRODUCTION_POPUP + "/TitleText";
        private const string INTRODUCTION_POPUP_CONTENTTEXT = INTRODUCTION_POPUP + "/ContentText";
        private const string INSTITUTE_POPUP = CENTER_PANEL + "/InstitutePopUp";
        private const string INSTITUTE_CLOSE_BUTTON = INSTITUTE_POPUP + "/CloseButton";
        private const string INSTITUTE_SKILL_GROUP = INSTITUTE_POPUP + "/DragInstituteSkillPanel/InstituteSkillGroup";
        private const string INSTITUTE_LEVEL_TEXT = INSTITUTE_POPUP + "/LevelText";
        private const string INSTITUTE_LEVELUP_TEXT = INSTITUTE_POPUP + "/LevelUpText";
        private const string INSTITUTE_LEVELUP_COST_TEXT = INSTITUTE_POPUP + "/LevelUpCostText";
        private const string INSTITUTE_PROP_TEXT1 = INSTITUTE_POPUP + "/InstitutePropText1";
        private const string INSTITUTE_PROP_TEXT2 = INSTITUTE_POPUP + "/InstitutePropText2";
		private const string INSTITUTE_PROP_TEXT3 = INSTITUTE_POPUP + "/InstitutePropText3";
		private const string INSTITUTE_PROP_TEXT4 = INSTITUTE_POPUP + "/InstitutePropText4";
		private const string INSTITUTE_ARROWICON = INSTITUTE_POPUP + "/ArrowIcon";

        private const string INSTITUTE_LEVELUP_BUTTON = INSTITUTE_POPUP + "/LevelUpButton";
        private const string TOWER_POPUP = CENTER_PANEL + "/TowerPopUp";
        private const string TOWER_ITEM_GROUP = TOWER_POPUP + "/DragTowerItemPanel/TowerItemGroup";
        private const string TOWER_CLOSE_BUTTON = TOWER_POPUP + "/CloseButton";
        private const string TOP_RIGHT_BG = RIGHT_TOP_PANEL + "/TopRightBg";

        #endregion

        private float _notificationTime = 0;
        private bool _notificate;

        private Vector3 _notificationStartPos;

        public MatchSide Side
        {
            get
            {
                return controller.side;
            }
        }

        public ForceMark Mark
        {
            get
            {
                return controller.mark;
            }
        }

        public bool CanDeploy
        {
            get
            {
                return controller.deploymentPendingSquadIndex < 0;
            }
        }
			
        private bool isClickDownShowUnitBtn = false;
        private bool isEnoughTime = false;
        private float clickDownTime;

        private BattleType currentBattleType = BattleType.NoBattle;

        void Awake()
        {
            _controller = controller = new BattleViewController( this );
            controller.Awake();

            //if ( Resource.AssetBundleTest.Instance.runtest )
            //    Resource.AssetBundleTest.Instance.RunTests();

            currentBattleType = DataManager.GetInstance().GetBattleType();
            InitBattleControllType();
        }

        public override void OnDestroy()
        {
            if ( controller != null )
                controller.OnDestroy();
            //if ( !Resource.AssetBundleTest.Instance.runtest )
            //Resource.AssetBundleManager.Instance.UnloadBundle( "battleui" );
        }

        void Update()
        {
            if ( !_notificate && _notificationQueue.Count > 0 )
                EnableBattleNotification( _notificationQueue.Dequeue() );

            if ( _notificate )
            {
                _notificationTime += Time.deltaTime;
                _battleNotification.transform.localPosition = new Vector3( Mathf.Lerp( _notificationStartPos.x, _battleNotificationReferencePoint.x, _notificationTime * 0.2f ), _battleNotificationReferencePoint.y, 0 );

                if ( _notificationTime >= Constants.GameConstants.BATTLE_NOTIFICATION_TIME )
                {
                    _battleNotification.transform.localPosition = new Vector3( _notificationStartPos.x, _notificationStartPos.y, 0 );
                    _battleNotification.SetActive( false );
                    _notificate = false;
                    _notificationTime = 0;
                    _battleNotification.transform.localPosition = new Vector3( _notificationStartPos.x, _notificationStartPos.y, 0 );
                }
            }

            if ( isClickDownShowUnitBtn && !isEnoughTime && controller.GetBattleControllType() == BattleUIControlType.TypeOne )
            {
                if ( Time.time - clickDownTime >= 0.01f )
                {
                    isEnoughTime = true;
                    _unitCards.gameObject.SetActive( true );
                }
            }

            if ( isClickDownChatBtn )
            {
                if ( Time.time - clickDownChatBtnTime >= 0.5f )
                {
                    isClickDownChatBtn = false;
                    ShowChatSwitchTypeImage();
                }
            }

			if( isChatTextNeedTimer )
			{
				ChatMessageHideTiming();
			}
        }

        private void OnShowUnitButtonClickDown( GameObject obj )
        {            
            isClickDownShowUnitBtn = true;
            isEnoughTime = false;
            clickDownTime = Time.time;
        }

        private void OnShowUnitButtonClickUp( GameObject obj )
        {
            isClickDownShowUnitBtn = false;
            _unitCards.gameObject.SetActive( false );
        }

        private void InitBattleControllType()
        {
            switch ( controller.GetBattleControllType() )
            {
                case BattleUIControlType.Normal:
                {
                    _unitCards.transform.localPosition = new Vector3( 15.0f , 258.0f , 0 );
                    showUnitButton.gameObject.SetActive( false );
                }
                break;
                case BattleUIControlType.TypeOne:
                {
                    _unitCards.gameObject.SetActive( false );                    
                    ClickHandler.Get( showUnitButton.gameObject ).onClickDown = OnShowUnitButtonClickDown;
                    ClickHandler.Get( showUnitButton.gameObject ).onClickUp = OnShowUnitButtonClickUp;
                }
                break;
                case BattleUIControlType.TypeTwo:
                {
                    _unitCards.gameObject.SetActive( false );
                    showUnitButton.gameObject.SetActive( false );
                    OnInitDiskUI();
                }
                break;
                default:
                {
                    DebugUtils.LogError( DebugUtils.Type.Battle , " None BattleControllType !" );
                }
                break;
            }
        }

        private void OnInitDiskUI()
        {            
            diskRoot.SetActive( true );
            for ( int i = 0; i < unitCardItemGroup.Length; i++ )
            {
                unitCardItemGroup[i] = diskRoot.transform.Find( "Item" + i ).gameObject.AddComponent<UnitCardItemTypeTwo>();
                if ( unitCardItemGroup[i] )
                {
                    unitCardItemGroup[i].OnInit();
                }
            }
        }

        private void SortDiskUI( long ownerId )
        {
            List<UnitCardItemTypeTwo> tempList = new List<UnitCardItemTypeTwo>();
            for ( int i = 0; i < unitCardItemGroup.Length; i++ )
            {
                if ( unitCardItemGroup[i] != null && unitCardItemGroup[i].unitId != 0 && unitCardItemGroup[i].unitId != ownerId )
                {
                    tempList.Add( unitCardItemGroup[i] );                    
                }
            }
            for ( int i = 0; i < tempList.Count; i++ )
            {
                if ( unitCardItemGroup[i] != null )
                {
                    unitCardItemGroup[i].UpdateData( tempList[i].unitId , tempList[i].data );
                    unitCardItemGroup[i].Seleceted( tempList[i].iSeleceted );
                    if ( tempList[i].UnitCurrentHp > 0 )
                    {
                        unitCardItemGroup[i].AdjustHealth( tempList[i].UnitCurrentHp , tempList[i].UnitMaxHp );
                    }
                }
            }
            for ( int i = tempList.Count; i < unitCardItemGroup.Length; i++ )
            {
                if ( unitCardItemGroup[i] != null )
                {
                    unitCardItemGroup[i].ResetUI();
                }
            }
        }

        private void RefreshDiskUIInfo( long unitId )
        {
            if ( !unitCardItems_TypeTwo.ContainsKey(unitId) )
            {
                return;
            }
            for ( int i = 0; i < unitCardItemGroup.Length; i++ )
            {
                if ( unitCardItemGroup[i] != null && unitCardItemGroup[i].unitId == 0 )
                {
                    unitCardItemGroup[i].UpdateData( unitId , unitCardItems_TypeTwo[unitId].data );
                    break;
                }
                
            }
        }

        public override void OnInit()
        {
            base.OnInit();

            //if ( !Resource.AssetBundleTest.Instance.runtest )
            //    Resource.AssetBundleManager.Instance.PreloadAssetBundleAsync( "battle ui" );

            //Battle Left Panel
            _leftPanel = transform.Find( LEFT_PANEL ).gameObject;
            _unitCards = transform.Find( LEFT_PANEL + "/DragUnitCardPanel/UnitCardItemGroup" ).GetComponent<GridLayoutGroup>();

			for ( int i = 0; i < _unitCards.transform.childCount; i++ )
			{
				Transform cardItem = _unitCards.transform.GetChild ( i );
				UnitCardItem unitcard = cardItem.gameObject.AddComponent<UnitCardItem>();
				unitcard.InitComponent ();
                unitcard.gameObject.SetActive( false );
                unitCardPool.Add( unitcard );
            }

            showUnitButton = _leftPanel.transform.Find( "DragUnitCardPanel/ShowUnitButton" ).GetComponent<Button>();
            diskRoot = _leftPanel.transform.Find( "DiskRoot" ).gameObject;
            diskRoot.SetActive( false );

            //Battle Left Top Panel       
            _leftTopPanel = transform.Find( LEFT_TOP_PANEL ).gameObject;
            mapParent = transform.Find( LEFT_TOP_PANEL + "/MiniMapBg" );
            //_emberText = transform.Find( LEFT_TOP_PANEL + "/EmberText" ).GetComponent<Text>();
            //_killCountText = transform.Find( LEFT_TOP_PANEL + "/KillCountText" ).GetComponent<Text>();
            //_killedCountText = transform.Find( LEFT_TOP_PANEL + "/KilledCountText" ).GetComponent<Text>();
            _emberText = transform.Find( TOP_RIGHT_BG + "/EmberText" ).GetComponent<Text>();
            _killCountText = transform.Find( TOP_RIGHT_BG + "/KillCountText" ).GetComponent<Text>();
            _killedCountText = transform.Find( TOP_RIGHT_BG + "/KilledCountText" ).GetComponent<Text>();
            //killImage = transform.Find( LEFT_TOP_PANEL + "/KillImage" ).GetComponent<Image>();
            //deathImage = transform.Find( LEFT_TOP_PANEL + "/DeathImage" ).GetComponent<Image>();
            //emberImage = transform.Find( LEFT_TOP_PANEL + "/EmberImage" ).GetComponent<Image>();
            largerButton = transform.Find( LEFT_TOP_PANEL + "/LargerButton" ).GetComponent<Button>();
            resetButton = transform.Find( LEFT_TOP_PANEL + "/ResetButton" ).GetComponent<Button>();
            resetButton.gameObject.SetActive( false );
            resetButtonBg = transform.Find( LEFT_TOP_PANEL + "/ResetButtonBg" ).gameObject;
            resetButtonBg.SetActive( false );
            microphoneButton = transform.Find( LEFT_TOP_PANEL + "/MicrophoneButton" ).GetComponent<Button>();
            loudspeakerButton = transform.Find( LEFT_TOP_PANEL + "/LoudspeakerButton" ).GetComponent<Button>();
            largerButtonBg = transform.Find( LEFT_TOP_PANEL + "/LargerButtonBg" ).gameObject;
            microphoneButtonBg = transform.Find( LEFT_TOP_PANEL + "/MicrophoneButtonBg" ).gameObject;
            loudspeakerButtonBg = transform.Find( LEFT_TOP_PANEL + "/LoudspeakerButtonBg" ).gameObject;

            largerButton.AddListener( OnClickLargerButton );
            resetButton.AddListener( OnClickResetButton );
            microphoneButton.AddListener( OnClickMicrophoneButton );
            loudspeakerButton.AddListener( OnClickLoudspeakerButton );

            //Battle Right Top Panel
            settingButton = transform.Find( TOP_RIGHT_BG + "/SettingButton" ).GetComponent<Button>();
            fightingButton = transform.Find( TOP_RIGHT_BG + "/FightingButton" ).GetComponent<Button>();

            settingButton.AddListener( OnClickSettingButton );
            fightingButton.AddListener( OnClickFightingButton );

            //Battle Top panel
            _timerText = transform.Find( TOP_PANEL + "/TimeText" ).GetComponent<Text>();
            redSideKillCountText = transform.Find( TOP_PANEL + "/RedSideKillCountText" ).GetComponent<Text>();
            blueSideKillCountText = transform.Find( TOP_PANEL + "/BlueSideKillCountText" ).GetComponent<Text>();
            battleTopPanel = transform.Find( TOP_PANEL ).gameObject;

            //center panel
            _centerPanel = transform.Find( CENTER_PANEL ).gameObject;

            //Setting PopUp UI
            _settingPopupPanel = transform.Find( CENTER_PANEL + "/" + SETTING_POPUP ).gameObject;
            redHeaderText = transform.Find( CENTER_PANEL + "/" + SETTING_POPUP + "/RedHeaderText" ).GetComponent<Text>();
            blueHeaderText = transform.Find( CENTER_PANEL + "/" + SETTING_POPUP + "/BlueHeaderText" ).GetComponent<Text>();
            redSideGroup = transform.Find( CENTER_PANEL + "/" + SETTING_POPUP + "/RedSideGroup" ).GetComponent<GridLayoutGroup>();
            blueSideGroup = transform.Find( CENTER_PANEL + "/" + SETTING_POPUP + "/BlueSideGroup" ).GetComponent<GridLayoutGroup>();
            surrenderButton = transform.Find( CENTER_PANEL + "/" + SETTING_POPUP + "/SurrenderButton" ).GetComponent<Button>();
            _closeButton = transform.Find( CENTER_PANEL + "/" + SETTING_POPUP + "/CloseButton" ).GetComponent<Button>();
            countdownText = transform.Find( CENTER_PANEL + "/" + SETTING_POPUP + "/SurrenderButton/CountdownText" ).GetComponent<Text>();
            surrenderText = surrenderButton.transform.Find( "SurrenderText" ).GetComponent<Text>();
            surrenderText.gameObject.SetActive( false );

            //12s later activate surrenderButton
            surrenderButton.interactable = false;
            battleCanActivateTime = GameConstants.BATTLE_CAN_SURRENDER_TIME;

            _closeButton.AddListener( OnClickCloseButton );

            //notification        
            _battleNotification = transform.Find( CENTER_PANEL + "/BattleNotificationPanel" ).gameObject;
            _battleNotificationText = transform.Find( CENTER_PANEL + "/BattleNotificationPanel/BattleNotificationText" ).GetComponent<Text>();
            _battleNotificationReferencePoint = transform.Find( "BattleNotificationReferencePoint" ).localPosition;

            //Battle Bottom Panel
            _bottomPanel = transform.Find( BOTTOM_PANEL ).gameObject;
            _squadCards = transform.Find( BOTTOM_PANEL + "/SquadCardItemGroup" ).GetComponent<GridLayoutGroup>();
            _waitingSquadImage = transform.Find( BOTTOM_PANEL + "/NextCard/NextCardImage" ).GetComponent<Image>();
            demolisherAndCrystalCar = _bottomPanel.transform.Find( "DemolisherAndCrystalCar" );

            _notificationStartPos = new Vector3( _battleNotification.transform.localPosition.x, _battleNotification.transform.localPosition.y, 0 );

            //BATTLE_RIGHT_PANEL
            battleRightPanel = transform.Find( BATTLE_RIGHT_PANEL );
            battleCommandControlView = battleRightPanel.Find( "ControlRoot" ).gameObject.AddComponent<BattleCommandControlView>();
            battleCommandControlView.OnInit();

            //Battle Right Bottom Panel
            rightBottomPanel = transform.Find( BATTLE_RIGHT_BOTTOM_PANEL ).gameObject;
            rightBottomPosition = transform.Find( "BattleRightBottomPostion" ).gameObject;
			rightTopPostion = transform.Find ( "BattleRightTopPostion" ).gameObject;
            introductionPopUp = transform.Find( INTRODUCTION_POPUP ).gameObject;
            titleText = transform.Find( INTRODUCTION_POPUP_TITLETEXT ).GetComponent<Text>();
            contentText = transform.Find( INTRODUCTION_POPUP_CONTENTTEXT ).GetComponent<Text>();

            introductionPopUp.gameObject.SetActive( false );

            //InstitutePopUp
            institutePopUp = transform.Find( INSTITUTE_POPUP ).gameObject;
            institutePopUp.SetActive( false );
            instituteLevelText = transform.Find( INSTITUTE_LEVEL_TEXT ).GetComponent<Text>();
            levelUpCostText = transform.Find( INSTITUTE_LEVELUP_COST_TEXT ).GetComponent<Text>();
            institutePropText1 = transform.Find( INSTITUTE_PROP_TEXT1 ).GetComponent<Text>();
            institutePropText2 = transform.Find( INSTITUTE_PROP_TEXT2 ).GetComponent<Text>();
			institutePropText3 = transform.Find( INSTITUTE_PROP_TEXT3 ).GetComponent<Text>();
			institutePropText4 = transform.Find( INSTITUTE_PROP_TEXT4 ).GetComponent<Text>();

			instituteHpTextList = new List<Text>();
			instituteHpTextList.Add( institutePropText1 );
			instituteHpTextList.Add( institutePropText2 );
			instituteHpTextList.Add( institutePropText3 );
			instituteHpTextList.Add( institutePropText4 );

			instituteArrowIconRectTransform = transform.Find( INSTITUTE_ARROWICON ).GetComponent<RectTransform>();
			instituteArrowIconLocalPos = instituteArrowIconRectTransform.localPosition;

			instituteHpTextRectTransformList = new List<Vector3>();
			instituteHpTextRectTransformList.Add( institutePropText1.GetComponent<RectTransform>().localPosition );
			instituteHpTextRectTransformList.Add( institutePropText2.GetComponent<RectTransform>().localPosition );
			instituteHpTextRectTransformList.Add( institutePropText3.GetComponent<RectTransform>().localPosition );
			instituteHpTextRectTransformList.Add( institutePropText4.GetComponent<RectTransform>().localPosition );

            institutelevelUpText = transform.Find( INSTITUTE_LEVELUP_TEXT ).GetComponent<Text>();

            instituteCloseButton = transform.Find( INSTITUTE_CLOSE_BUTTON ).GetComponent<Button>();
            instituteLevelUpButton = transform.Find( INSTITUTE_LEVELUP_BUTTON ).GetComponent<Button>();
            instituteSkillGroup = transform.Find( INSTITUTE_SKILL_GROUP ).GetComponent<GridLayoutGroup>();

            instituteCloseButton.AddListener( CloseInstitutePopUp );
			instituteLevelUpButton.AddListener( OnClickInstituteLevelUpButton );

            //TowerPopUp
            towerPopUp = transform.Find( TOWER_POPUP ).gameObject;
            towerPopUp.gameObject.SetActive( false );
            towerItemGroup = transform.Find( TOWER_ITEM_GROUP ).GetComponent<GridLayoutGroup>();
            towerCloseButton = transform.Find( TOWER_CLOSE_BUTTON ).GetComponent<Button>();

            towerCloseButton.AddListener( CloseTowerPopUp );

            //TapTowerPopUp
            tapTowerPopUp = transform.Find( "TapTowerPopUp" ).gameObject;
            tapTowerPopUp.gameObject.SetActive( false );

            //NoticeView
            noticeView = transform.Find( CENTER_PANEL + "/NoticePanel" ).gameObject.AddComponent<NoticeView>();
            noticeView.Init();

            //OrderNoticeView
            commandNoticeView = transform.Find( CENTER_PANEL + "/CommandNoticePanel" ).gameObject.AddComponent<CommandNoticeView>();
            commandNoticeView.OnInit();

            //greetingView
            greetingView = transform.Find( CENTER_PANEL + "/GreetingPanel" ).gameObject.AddComponent<GreetingView>();
            greetingView.OnInit();

            //BuildingBuffPanel
            buildingBuffPanelRect = transform.Find( "BuildingBuffPanel" ).GetComponent<RectTransform>();
			originalPanelHeight = buildingBuffPanelRect.rect.height;
			attributeTextRect = buildingBuffPanelRect.transform.Find( "AttributeText" ).GetComponent<RectTransform>();
			attributeText = attributeTextRect.transform.GetComponent<Text> ();
			originalTextHeight = attributeTextRect.rect.height;
			spaceText = originalPanelHeight - originalTextHeight;
			buffPanelArrowButton = buildingBuffPanelRect.transform.Find( "BuffPanelArrowButton" ).GetComponent<Button>();
			buffPanelArrowButton.AddListener ( BuffPanelArrowButtonEvent );
			buildingBuffPanelRect.gameObject.SetActive ( false );

			//BattleChat
			chatElementsPanel = transform.Find( "BattleChat/Chat/BattleChatInputElements" );

			chatChannelSwitchButton = chatElementsPanel.Find( "ChannelSwitchButton" ).GetComponent<Button>();

			chatChannel_PartyText = chatChannelSwitchButton.transform.Find( "PartyChannelText" ).GetComponent<Text>();
			chatChannel_AllPlayerText = chatChannelSwitchButton.transform.Find( "AllPlayerChannelText" ).GetComponent<Text>();

			chatMessageTextList = new List<Text>();
			Transform chatTextsTransform = transform.Find( "BattleChat/Chat/ChatMessageTexts" );

			for( int i = 0; i < chatTextsTransform.childCount; i++ )
			{
				chatMessageTextList.Add( chatTextsTransform.GetChild( i ).GetComponent<Text>() );
			}
				
			chatInput = transform.Find( "BattleChat/Chat/BattleChatInputElements/ChatInputButton" ).GetComponent<InputField>();
			chatInput.onEndEdit.AddListener( delegate { OnClickChatSendMessageButton( chatInput );});

            chatBtnImage = chatInput.GetComponent<Image>();
            ClickHandler.Get( chatBtnImage.gameObject ).onClickDown = OnClickDownChatBtnImage;
            ClickHandler.Get( chatBtnImage.gameObject ).onClickUp = OnClickUpChatBtnImage;
            districtImage = chatBtnImage.transform.Find( "DistrictImage" ).GetComponent<Image>();
            ClickHandler.Get( districtImage.gameObject ).onEnter = OnEnterDistrictImage;
            teamImage = chatBtnImage.transform.Find( "TeamImage" ).GetComponent<Image>();
            ClickHandler.Get( teamImage.gameObject ).onEnter = OnEnterTeamImage;

            chatChannelSwitchButton.AddListener( OnClickChatChannelSwitchButton );

            Localization();
        }

		public override void OnEnter()
		{
			base.OnEnter();

			SetBattleUI( true );

			InitInstitutePanel();
		}

        private void ClearBattleUITypeList()
        {
            battleUITypeList.Clear();
        }

        private void OpenBattleUI( BattleUIType type )
        {
            for ( int i = 0; i < battleUITypeList.Count; i++ )
            {
                CloseBattleUI( battleUITypeList[i] );
            }
            battleUITypeList.Clear();
            battleUITypeList.Add( type );
        }

        private void RemoveBattleUI( BattleUIType type )
        {
            if ( battleUITypeList.Contains( type ) )
            {
                battleUITypeList.Remove( type );
            }
        }

        private void CloseBattleUI( BattleUIType type )
        {
            if ( !battleUITypeList.Contains( type ) )
            {
                return;
            }
            switch ( type )
            {
                case BattleUIType.LittleMap:
                    OnClickResetButton();
                break;
                case BattleUIType.BuffUI:
                    BuffPanelArrowButtonEvent();
                break;
                case BattleUIType.BattleInfoUI:
                    OnClickCloseButton();
                break;
                case BattleUIType.InstitutePopUp:
                    CloseInstitutePopUp();
                break;
                default:
                break;
            }
        }


        public void SetSurrenderButtonStateTime( int time)
        {
            if (surrenderButton.interactable) return;

            battleCanActivateTime -= 1;

            if (battleCanActivateTime <= 0)
            {
                countdownText.gameObject.SetActive( false );
                surrenderText.gameObject.SetActive( true );
                surrenderButton.interactable = true;

                surrenderButton.AddListener( OnClickSurrenderButton );
            }
            else
            {
                countdownText.text = string.Format( "({0}s)", battleCanActivateTime );
            }
        }

        void Localization()
        {
            _battleNotificationText.text = "Battle";
            powerUpNotificationText = "Power Up Deployed";
        }

        #region Set & Get Data

		//Drag deployment logic locked.Dwayne 2017.9
		/*
        public void DestroyDragImage()
        {
            Destroy( deploymentImage );
            Utils.MessageDispatcher.PostMessage( Constants.MessageType.CloseDeployAreas, controller.mark );
            Utils.MessageDispatcher.PostMessage( Constants.MessageType.ChangeUIGestureState, false );
        }*/

        public void SetWaitingSquadImage( SquadData data )
        {
            _waitingSquad = data;

            if ( _waitingSquad == null )
            {
                _waitingSquadImage.gameObject.SetActive( false );
            }
            else
            {
                _waitingSquadImage.gameObject.SetActive( true );
                
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( _waitingSquad.protoData.Icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
                {
                    _waitingSquadImage.SetSprite( atlasSprite );
                }, true );
            }
        }

        public void SetInitialSquads( List<SquadData> currentCards, SquadData waitingCard )
        {
            DebugUtils.Assert( currentCards.Count == _squadCards.transform.childCount, "Battle UI Squad Card's count not equal with data" );

            for ( int i = 0; i < _squadCards.transform.childCount; i++ )
            {
                SquadData squadData = currentCards[i];

                GameObject go = _squadCards.transform.GetChild( i ).gameObject;
                SquadCardItem card = go.AddComponent<SquadCardItem>();
                squadCard_Items.Add( card );
				card.OnInit( i );
                card.SetValues( squadData );

				//Drag deployment logic locked.Dwayne 2017.9
               //go.GetComponent<UnitDragHandler>().SetSquadCardItem( card );
            }

            SetWaitingSquadImage( waitingCard );
        }

		public void FillSquad( SquadData fillCard, SquadData waitingCard, int buttonIndex )
        {
            if ( fillCard != null )
            {
                for ( int i = 0; i < squadCard_Items.Count; i++ )
                {
					if ( squadCard_Items[i].buttonIndex == buttonIndex && !squadCard_Items[i].active && !squadCard_Items[i].isTiming ) 
					{
						SquadData temp = squadCard_Items[i].data;
                        squadCard_Items[i].SetValues( fillCard );
						if( temp != null )
						{
							controller.unitDataCache.Add( temp );
						}
                    }
                }
            }
	
            SetWaitingSquadImage( waitingCard );
        }

		public void AddHoldeSquad()
		{
			controller.playerDeployedSquadNum++;

			if( controller.playerDeployedSquadNum == GameConstants.PLAYER_CANDEPLOY_UNIT_MAXCOUNT )
			{
				for( int i = 0; i < squadCard_Items.Count; i++ )
				{
					squadCard_Items[i].isLimited = true;
					squadCard_Items[i].SetCanDeploy( false );
				}
			}
		}

		public void ResetSquadDeployedLimitStatus()
		{
			for( int i = 0; i < squadCard_Items.Count; i++ )
			{
				squadCard_Items[i].isLimited = false;
			}
		}

        public void SetEmber( int amount )
        {
			playerEmber = amount;
            _emberText.text = amount.ToString();
            CanDeploySpecialCards( amount );
			RefreshInstitutePanelStatus( amount );
        }

        public void SetKillCount( int killCount )
        {
            _killCountText.text = killCount.ToString();
        }

        public void SetRedKillCount( int count )
        {
            redSideKillCountText.text = count.ToString();
        }

        public void SetBlueKillCount( int count )
        {
            blueSideKillCountText.text = count.ToString();
        }

        public void SetKilledCount( int killedCount )
        {
            _killedCountText.text = killedCount.ToString();
        }

        public void CanDeployUnits( int amount )
        {
            for ( int i = 0; i < squadCard_Items.Count; i++ )
            {
                bool canDeploy;
                switch ( controller.GetBattleControllType() )
                {
                    case BattleUIControlType.Normal:                    
                    case BattleUIControlType.TypeOne:
                    default:
                    {
                        canDeploy = amount >= squadCard_Items[i].data.protoData.DeploymentCost && unitCard_Items.Count < controller.unitLargestAmount;
                    }
                    break;
                    case BattleUIControlType.TypeTwo:
                    {
                        canDeploy = amount >= squadCard_Items[i].data.protoData.DeploymentCost && unitCardItems_TypeTwo.Count < controller.unitLargestAmount;
                    }
                    break;                                        
                }
                
                squadCard_Items[i].SetCanDeploy( canDeploy );
            }
        }

        public void SetLockSquadItems( bool lockState )
        {
            for ( int i = 0; i < squadCard_Items.Count; i++ )
            {
                squadCard_Items[i].SetCanDeploy( lockState );
            }
        }

        public void SetTimer( int time )
        {
            _timerText.text = GetTimeString( time );
        }

        private string GetTimeString( int time )
        {
            return string.Format( "{0:00} : {1:00}", time / 60, time % 60 );
        }

        public void SetSettingPopup( bool state )
        {
            _settingPopupPanel.SetActive( state );
            if ( state )
            {
                OpenBattleUI( BattleUIType.BattleInfoUI );
            }
            else
            {
                RemoveBattleUI( BattleUIType.BattleInfoUI );
            }
        }

        public void SetBattleNotification( bool state )
        {
            _battleNotification.SetActive( state );
        }

        private void EnableBattleNotification( string message )
        {
            _battleNotificationText.text = message;
            SetBattleNotification( true );
            _notificate = true;
        }

        public void ReceiveNotification( string message )
        {
            if ( string.IsNullOrEmpty( message ) )
                message = powerUpNotificationText;
            _notificationQueue.Enqueue( message );
        }

        public void OpenBattleResult( NoticeType resType, BattleResultData resInfo )
        {
            DisableAll();

            StartCoroutine( GenerateBanner( resType, resInfo ) );
        }

        public void OpenTutorialResult()
        {
            DisableAll();
            StartCoroutine( TutorialBanner() );
        }

        private IEnumerator TutorialBanner()
        {
            GameObject banner;
            banner = Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "BannerVictory" ) );
            banner.transform.SetParent( transform.parent , false );
            yield return new WaitForSeconds( 3.0f );
            OnClickContinueBattleResult();
        }

        private IEnumerator GenerateBanner( NoticeType resType, BattleResultData resInfo )
        {
            yield return new WaitForSeconds( 3.0f );
            GameObject banner;

            switch ( resType )
            {
                case NoticeType.BattleResultBlueWin:
                {
                    if ( DataManager.GetInstance().GetMatchSide() == MatchSide.Red )
                    {
                        banner = Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "BannerDefeat" ) );

                        SoundManager.Instance.PlayMusic( GameConstants.SOUND_DEFEAT_ID, false );
                    }
                    else
                    {
                        banner = Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "BannerVictory" ) );

                        SoundManager.Instance.PlayMusic( GameConstants.SOUND_VICTORY_ID, false );
                    }
                    break;
                }
                case NoticeType.BattleResultRedWin:
                {
                    if ( DataManager.GetInstance().GetMatchSide() == MatchSide.Red )
                    {
                        banner = Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "BannerVictory" ) );

                        SoundManager.Instance.PlayMusic( GameConstants.SOUND_VICTORY_ID, false );
                    }
                    else
                    {
                        banner = Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "BannerDefeat" ) );

                        SoundManager.Instance.PlayMusic( GameConstants.SOUND_DEFEAT_ID, false );
                    }
                    break;
                }
                default:
                {
                    banner = Instantiate( Resources.Load( "Prefabs/UI/BattleScreenItem/banners/banner_draw" ) as GameObject );

                    SoundManager.Instance.PlayMusic( GameConstants.SOUND_VICTORY_ID, false );
                    break;
                }
            }

            //banner.transform.Find( "Image" ).gameObject.SetActive( false );
            banner.transform.SetParent( transform.parent, false );

            StartCoroutine( EnableBattleResult( resType, resInfo, banner ) );
        }

        private IEnumerator EnableBattleResult( NoticeType resType, BattleResultData resInfo, GameObject banner )
        {
            yield return new WaitForSeconds( 3f );

            Destroy( banner );

            UIManager.Instance.GetUIByType( UIType.BattleResultScreen, ( ViewBase ui, System.Object param ) => {
                if ( !ui.openState )
                {
                    ui.OnEnter();
                    ( ui as BattleResultView ).SetIsMainUIOpen( false );
                    ( ui as BattleResultView ).SetBattleResultData( resType, resInfo );
                }
            } );
        }

        private void DisableAll()
        {
            _leftPanel.SetActive( false );
            _leftTopPanel.SetActive( false );
            _bottomPanel.SetActive( false );
            battleTopPanel.SetActive( false );
            rightBottomPanel.SetActive( false );
            towerPopUp.SetActive( false );
            SetSettingPopup( false );
            SetBattleNotification( false );
            battleRightPanel.gameObject.SetActive( false );
        }

        public void SetBattleUI( bool state )
        {
            _leftPanel.SetActive( state );
            _leftTopPanel.SetActive( state );
            _bottomPanel.SetActive( state );
            battleTopPanel.SetActive( state );
            rightBottomPanel.SetActive( state );
            SetSettingPopup( false );
            SetBattleNotification( false );
            battleRightPanel.gameObject.SetActive( state );
        }

        public void ShowSurrenderTips(object obj)
        {
            if (surrenderTips == null)
            {
                GameObject go = Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "SurrenderTips" ) );
                surrenderTips = go.AddComponent<SurrenderTips>();
                surrenderTips.Trans.SetParent( transform.Find( RIGHT_TOP_PANEL ) );
                surrenderTips.Trans.localScale = Vector3.one;
                surrenderTips.Trans.anchoredPosition = new Vector2( -244, -49 );
                surrenderTips.Trans.localRotation = Quaternion.identity;
            }
            surrenderTips.Show( obj );
        }

        public void Squad_AdjustHealth( long unitId, int hp, int maxHp )
        {
            int adjustment = maxHp - hp;
            DebugUtils.Log( DebugUtils.Type.UI, string.Format( "Unit adjusthealth: id {0} adjustment {1}", unitId, adjustment ) );
            UnitCardItemBase unitcard;

            switch ( controller.GetBattleControllType() )
            {
                case BattleUIControlType.Normal:                
                case BattleUIControlType.TypeOne:
                default:
                {
                    unitcard = unitCard_Items.Find( p => p.unitId == unitId );
                }
                break;
                case BattleUIControlType.TypeTwo:
                {
                    unitcard = GetItemByUnitId( unitId );
                }
                break;                                
            }

			if( unitcard != null )
			{
				unitcard.AdjustHealth( hp, maxHp );
			}
        }

        private UnitCardItemTypeTwo GetItemByUnitId( long unitId )
        {
            for ( int i = 0; i < unitCardItemGroup.Length; i++ )
            {
                if ( unitCardItemGroup[i] != null && unitCardItemGroup[i].unitId == unitId)
                {
                    return unitCardItemGroup[i];
                }
            }
            return null;
        }

        public void DestroyUnitcard( BattleUIControlType controllerType, long ownerId )
        {
            if ( controllerType == BattleUIControlType.TypeTwo )
            {
                if ( unitCardItems_TypeTwo.ContainsKey( ownerId ) )
                {
                    unitCardItems_TypeTwo.Remove( ownerId );
                }
                SortDiskUI( ownerId );
            }
            else 
            {
                UnitCardItem unitcard = unitCard_Items.Find( p => p.unitId == ownerId );
                DebugUtils.Log( DebugUtils.Type.UI, string.Format( "Destroy UnitCard: id {0}", unitcard.unitId ) );
				unitcard.Unselect();
				unitcard.gameObject.SetActive( false );
                unitCard_Items.Remove( unitcard );
				unitCardPool.Add ( unitcard );
            }
        }

        public void DeployUnitCard( long unitId, int deploymentPendingSquadIndex )
        {
            Utils.DebugUtils.Log( DebugUtils.Type.UI, string.Format( "DeploySquad on field: id {0}", unitId ) );

			//Drag deployment logic locked.Dwayne 2017.9
            //SquadCardItem squad = squadCard_Items.Find( p => p.data.index == deploymentPendingSquadIndex );
			SquadData squad = controller.unitDataCache[0];
			controller.unitDataCache.RemoveAt( 0 );
            DebugUtils.Assert( squad != null, "Cant find squad data on DeploySquadApproved" );

            switch ( controller.GetBattleControllType() )
            {
                case BattleUIControlType.Normal:                
                case BattleUIControlType.TypeOne:
                {
                    InstantiateUnitCardItem( unitId, squad );
                }
                break;
                case BattleUIControlType.TypeTwo:
                {
                    UnitCardItemTypeTwo temp = new UnitCardItemTypeTwo( unitId , squad );
                    unitCardItems_TypeTwo.Add( unitId, temp );
                    RefreshDiskUIInfo( unitId );
                }
                break;
                default:
                {
                    DebugUtils.LogError( DebugUtils.Type.Battle , " None BattleControllType !" );
                }
                break;
            }

            //Hide squadCardItem, because squad has been deployed.
			//Drag deployment logic locked.Dwayne 2017.9
            //squad.SetActive( false );
        }

        private void InstantiateUnitCardItem( long unitId , SquadData squadData )
        {
			if( unitCardPool.Count > 0 )
			{
				UnitCardItem unitcard = unitCardPool[0];
				unitcard.gameObject.SetActive ( true );
				unitCardPool.Remove ( unitcard );
				unitcard.SetValues( squadData, unitId );
                unitCard_Items.Add( unitcard );

                if ( currentBattleType == BattleType.Tutorial && unitCard_Items.Count == 3 && DataManager.GetInstance().GetTutorialStage() == PVE.TutorialModeManager.TutorialModeStage.NormallyControlOperation_Stage )
                    MessageDispatcher.PostMessage( Constants.MessageType.OpenNewbieGuide, 4, 12 );
            }
        }

		//Drag deployment logic locked.Dwayne 2017.9
		/*public void SendDeploySquad( int metaId, Vector2 position, int index )
		{
			controller.deploymentPendingSquadIndex = index;
			controller.SendDeploySquad( metaId, position );
		}*/

		public void SendDeploySquad( int metaId, int index, int buttonIndex )
        {
            controller.deploymentPendingSquadIndex = index;
			controller.SendDeploySquad( metaId, buttonIndex );
        }

        #endregion

        #region ButtonEvents

        private void OnClickSurrenderButton()
        {
            SetSettingPopup( false );

            if (surrenderTips != null && surrenderTips.IsShow)
            {
                return;
            }

            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );
            MessageDispatcher.PostMessage( Constants.MessageType.QuitBattleRequest, true );
        }

        private void OnClickCloseButton()
        {
            SetSettingPopup( false );
        }

        private void OnClickContinueBattleResult()
        {
            //UnityEngine.SceneManagement.SceneManager.LoadScene( "MainMenu" );
            //UI.UIManager.locateState = UI.UIManagerLocateState.MainMenu;
            controller.GoBackToMainMenu();
        }

        private void OnClickInstituteLevelUpButton()
        {
			if ( instituteLevelUpButton.IsInteractable() )
			{
				instituteLevelUpButton.interactable = false;
				isInstituteUpgrading = true;
			}

			controller.InstituteLevelUp();
        }

        private void OnClickLargerButton()
        {
            SetLeftTopBaseDataUI( false );

            mapParent.transform.localScale = new Vector3( 1.8f, 1.8f, 1 );

            OpenBattleUI( BattleUIType.LittleMap );
        }

        private void OnClickResetButton()
        {
            SetLeftTopBaseDataUI( true );

            mapParent.transform.localScale = Vector3.one;

            RemoveBattleUI( BattleUIType.LittleMap );
        }

        private void OnClickMicrophoneButton()
        {
            //TODO
        }

        private void OnClickLoudspeakerButton()
        {
            //TODO
        }

		#region ChatEvents

        private void OnClickDownChatBtnImage( GameObject go )
        {
            //chatBtnImage
            isClickDownChatBtn = true;
            clickDownChatBtnTime = Time.time;
        }

        private void ShowChatSwitchTypeImage()
        {
            if ( chatType == BattleChatType.party )
            {
                districtImage.gameObject.SetActive( true );
                teamImage.gameObject.SetActive( false );
            }
            else if ( chatType == BattleChatType.everyone )
            {
                teamImage.gameObject.SetActive( true );
                districtImage.gameObject.SetActive( false );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.UI , string.Format( "Can't Show this BattleChatType, becases chatType is {0}" , chatType ) );
            }
        }

        private void OnClickUpChatBtnImage( GameObject go )
        {
            isClickDownChatBtn = false;
            districtImage.gameObject.SetActive( false );
            teamImage.gameObject.SetActive( false );
        }

        private void OnEnterDistrictImage( GameObject go )
        {
            districtImage.gameObject.SetActive( false );
            chatType = BattleChatType.everyone;
        }

        private void OnEnterTeamImage( GameObject go )
        {
            teamImage.gameObject.SetActive( false );
            chatType = BattleChatType.party;
        }

        private void OnClickChatButton()
        {
			if( !chatElementsPanel.gameObject.activeInHierarchy )
			{
				chatElementsPanel.gameObject.SetActive( true );
			}
			else
			{
				chatElementsPanel.gameObject.SetActive( false );
			}
        }

		private void OnClickChatChannelSwitchButton()
		{
			//Look like toggle, if have more chatType must be change this to toggle and modify some logic.
			if( chatType == BattleChatType.party )
			{
				chatChannel_PartyText.gameObject.SetActive( false );
				chatType = BattleChatType.everyone;
				chatChannel_AllPlayerText.gameObject.SetActive( true );
			}
			else if( chatType == BattleChatType.everyone  )
			{
				chatChannel_AllPlayerText.gameObject.SetActive( false );
				chatType = BattleChatType.party;
				chatChannel_PartyText.gameObject.SetActive( true );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Chat, string.Format( "This type {0} can't use here.", chatType ));
			}
		}

		private void OnClickChatSendMessageButton( InputField chatInput )
		{
			if( string.IsNullOrEmpty( chatInput.text ) )
			{
				return;
			}

			string str = chatInput.text;
			string strII = "";
			chatInput.text = "";

			if( string.IsNullOrEmpty( playerNickName ))
			{
				playerNickName = controller.dataManager.GetPlayerNickName();
			}

			if( chatType == BattleChatType.party )
			{
				strII =	string.Format( "【队伍】" + playerNickName + ":" + str );
			}
			else if( chatType == BattleChatType.everyone )
			{
				strII =	string.Format( "【全体】" + playerNickName + ":" + str );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.UI, string.Format( "Can't send this message, becases chatType is {0}", chatType ) );
			}

			//Just send 30 length string.
			if( strII.Length > 30 )
			{
				strII = strII.Substring( 0, 30 );
			}

			controller.SendChatMessage( strII, chatType );

			ShowChatMessage( strII, false );
		}
			
		public void ShowChatMessage( string str, bool isEnemy )
		{
			chatMessageTextList[3].text = chatMessageTextList[2].text;
			chatMessageTextList[2].text = chatMessageTextList[1].text;
			chatMessageTextList[1].text = chatMessageTextList[0].text;

			isChatTextNeedTimer = true;
			chatTextHideTimer = 0;

			ResetChatMessagesAlpha();

			if( isEnemy )
			{
				chatMessageTextList[0].text = string.Format( "<color=#ff0000>" + str + "</color>" );
			}
			else
			{
				chatMessageTextList[0].text = string.Format( "<color=#1a96ff>" + str + "</color>" );
			}
		}

		private void ChatMessageHideTiming()
		{
			chatTextHideTimer += Time.deltaTime;

			if( chatTextHideTimer >= chatTextHideTimeLimit )
			{
				AlphaHideChatMessages();
				isChatTextNeedTimer = false;
				chatTextHideTimer = 0;
			}
		}

		private void ResetChatMessagesAlpha()
		{
			for( int i = 0; i < chatMessageTextList.Count; i++ )
			{
				chatMessageTextList[ i ].CrossFadeAlpha( 1, 0.1f, false );
			}
		}

		private void AlphaHideChatMessages()
		{
			for( int i = 0; i < chatMessageTextList.Count; i++ )
			{
				chatMessageTextList[ i ].CrossFadeAlpha( 0, 2f, false );
			}
		}

		#endregion

        private void OnClickSettingButton()
        {
            UIManager.Instance.GetUIByType( UIType.SettingScreen, ( v, p ) => { v.OnEnter(); } );
        }

        private void OnClickFightingButton()
        {
            MessageDispatcher.PostMessage( MessageType.BattleSituationRequest );

            SetSettingPopup( true );
        }

		private void BuffPanelArrowButtonEvent()
		{
			isSpread = !isSpread;

            if ( isSpread )
            {
                OpenBattleUI( BattleUIType.BuffUI );
            }
            else
            {
                RemoveBattleUI( BattleUIType.BuffUI );
            }

			if( isSpread && attributeText.preferredHeight > originalTextHeight)
			{
				buildingBuffPanelRect.sizeDelta = new Vector2 ( buildingBuffPanelRect.sizeDelta.x,  attributeText.preferredHeight + spaceText );
				attributeTextRect.sizeDelta = new Vector2 ( attributeTextRect.sizeDelta.x,  attributeText.preferredHeight );
			}
			else
			{
				buildingBuffPanelRect.sizeDelta = new Vector2 ( buildingBuffPanelRect.sizeDelta.x,  originalPanelHeight );
				attributeTextRect.sizeDelta = new Vector2 ( attributeTextRect.sizeDelta.x,  originalTextHeight );
			}

			buffPanelArrowButton.transform.Rotate( new Vector3( 0, 0, 180 ) );
		}

        private void OnClickVoiceButton()
        {
            DebugUtils.Log( DebugUtils.Type.Battle, "Click Voice Button---------" );
        }

        private void OnClickHornButton()
        {
            DebugUtils.Log( DebugUtils.Type.Battle, "Click Horn Button---------" );
        }

        #endregion ButtonEvents

        #region Init Special Card Item
		//Locked building mode drag deployment code.Dwayne.
		//private int sprcailItemCount = 4;
		private int sprcailItemCount = 2;
	 	
        public void InitSpecialCard()
        {            
            for ( int i = 0; i < 2; i++ )
            {
                GameObject go = demolisherAndCrystalCar.Find( "SpecialCard" + i ).gameObject;
                SpecialCardItem specialCardItem = go.AddComponent<SpecialCardItem>();                

                specialCardItem.controller = controller;
                specialCardItem.cardType = (SpecialCardType)i;

                specialCardItem.SetItemValue();
                specialCardItem.SetSpecialItem();

                specialCard_Items.Add( specialCardItem );
            }            
        }
			
        private void CanDeploySpecialCards( int amount )
        {
            for ( int i = 0; i < specialCard_Items.Count; i++ )
            {
				if ( amount >= specialCard_Items[i].GetCardCost() && specialCard_Items[i].canDeploy ) 
				{
					specialCard_Items[i].SetCanDeploy();
				}
				else if( amount < specialCard_Items[i].GetCardCost() && specialCard_Items[i].canDeploy )
				{
					specialCard_Items[i].SetCanNotDeploy();
				}
            }
        }

		//Locked building mode drag deployment code. Dwayne.
        /*public void BuildInstitute()
        {
			specialCard_Items[0].AddNumber();
        }

        public void DestroyedInstitute()
        {
			if( institutePopUp.activeInHierarchy )
			{
				institutePopUp.SetActive( false );
			}

			specialCard_Items[0].RemoveNumber();

			ResetInstitutePopUpData();
        }

		public void BuildTower()
		{
			specialCard_Items[1].AddNumber();
		}

		public void DestroyedTower()
		{
			if( tapTowerPopUp.activeInHierarchy )
			{
				tapTowerPopUp.SetActive( false );
			}

			specialCard_Items[1].RemoveNumber();
		}*/

		public void DeployTramCar()
		{
			//Locked building mode drag deployment code. Dwayne.
			//specialCard_Items[2].AddNumber();
			specialCard_Items[0].AddNumber();
		}

		public void DestroyedTramCar()
		{
			//Locked building mode drag deployment code. Dwayne.
			//specialCard_Items[2].RemoveNumber();
			specialCard_Items[0].AddNumber();
		}
			
		public void DeployDemolisher()
		{
			//Locked building mode drag deployment code. Dwayne.
			//specialCard_Items[3].AddNumber();
			specialCard_Items[1].AddNumber();
		}

		public void DestroyedDemolisher()
		{
			//Locked building mode drag deployment code. Dwayne.
			//specialCard_Items[3].RemoveNumber();
			specialCard_Items[1].AddNumber();
		}

		public void DestroyTown( long id )
		{
			if( controller.dataManager.GetPlayerId() == id )
			{
				for( int i = 0; i < squadCard_Items.Count; i++ )
				{
					squadCard_Items[i].SwitchTownDestroyedStatus();
				}
			}
		}
			
        #endregion

        #region Init Setting Fighting Item

        public void InitFightingItem(List<SettingFightingItemVo> redDataList, List<SettingFightingItemVo> blueDataList)
        {
            CommonUtil.ClearItemList<SettingFightingItem>( fighting_Items );

			int index = 0;
			int count = blueDataList.Count + redDataList.Count;

			GameResourceLoadManager.GetInstance ().LoadResource ( SETTING_FIGHTING_BULE_ITEM_PATH , delegate (string name, UnityEngine.Object obj, System.Object param )
			{
				OnLoadFightingCardItem ( obj , blueDataList , blueSideGroup.transform , count , ref index );
			} , true );

			GameResourceLoadManager.GetInstance ().LoadResource ( SETTING_FIGHTING_RED_ITEM_PATH , delegate (string name, UnityEngine.Object obj, System.Object param )
			{
				OnLoadFightingCardItem ( obj , redDataList , redSideGroup.transform , count , ref index );
			} , true );
        }

        private void OnLoadFightingCardItem(UnityEngine.Object obj, List<SettingFightingItemVo> list, Transform trans, int count, ref int index)
        {
            if (list == null) return;

            int length = list.Count;

            for (int i = 0; i < length; i++)
            {
                SettingFightingItem fightingItem;
                if (fighting_Items.Count < count)
                {
                    fightingItem = CommonUtil.CreateItem<SettingFightingItem>( obj, trans );

                    fighting_Items.Add( fightingItem );
                }

                fightingItem = fighting_Items[index++];
                fightingItem.gameObject.SetActive( true );

                fightingItem.playerName = list[i].name;
                fightingItem.killCount = list[i].killCount;
                fightingItem.deathCount = list[i].deathCount;
                fightingItem.resourceCount = list[i].resourceCount;
                fightingItem.playerIcon = list[i].portrait;

                fightingItem.isRedSide = list[i].isRedSide;
                fightingItem.playerId = list[i].playerId;
                fightingItem.SetSettingFightItem();
            }
        }

        #endregion

        #region Set Introduction PopUp

        private void SetIntroductionPopUp( string title, string content, Vector3 position )
        {
            titleText.text = title;
            contentText.text = content;
            introductionPopUp.transform.position = new Vector3( introductionPopUp.transform.position.x, position.y + 20, 0 );

            introductionPopUp.SetActive( true );
        }

        private void CloseIntroductionPopUp()
        {
            introductionPopUp.SetActive( false );
        }

        #endregion

        #region Institute ui

 		private void InitInstitutePanel()
		{
			isInstituteUpgrading = false;
			instituteLevelText.text = "Level:1";
			levelUpCostText.text = controller.instituteProtoData[0].LevelUpCost.ToString();

			for( int i = 0; i < instituteHpTextList.Count; i++ )
			{
				instituteHpTextList[ i ].text = string.Format( "{0}{1}        {2}{3}", instituteLvStr, i + 1, instituteHpStr, controller.instituteProtoData[i].Health.ToString() );
				instituteHpTextList[ i ].color = Color.grey;
			}

			institutePropText1.color = Color.green;

			for( int i = 0; i < instituteHpTextRectTransformList.Count; i++ )
			{
				instituteHpTextList[i].rectTransform.localPosition = instituteHpTextRectTransformList[i];
			}

			instituteArrowIconRectTransform.localPosition = instituteArrowIconLocalPos;
			instituteArrowIconRectTransform.gameObject.SetActive( true );
		}

		private void RefreshInstitutePanelStatus( int money )
		{
			if( isInstituteUpgrading )
			{
				return;
			}

			int instituteLv = controller.dataManager.GetInstituteLV();

			if( InstituteMaxLv == 0 )
			{
				InstituteMaxLv = controller.instituteProtoData[0].MaxLevel;
			}

			if( instituteLv < InstituteMaxLv && money >= int.Parse( levelUpCostText.text ) )
			{
				if( instituteLevelUpButton.interactable == false )
				{
					instituteLevelUpButton.interactable = true;
				}
			}
			else if( instituteLv < InstituteMaxLv && money < int.Parse( levelUpCostText.text ) )
			{
				if( instituteLevelUpButton.interactable == true )
				{
					instituteLevelUpButton.interactable = false;
				}
			}

			if( instituteSkill_Items != null )
			{
				if( instituteSkill_Items.Count > 0  )
				{
					for( int i = 0; i < instituteSkill_Items.Count; i++ )
					{
						instituteSkill_Items[i].CanUpgradeInstituteSkill( instituteLv, money );
					}
				}
			}
		}
			
		public void InstituteLevelUpComplete()
		{
			isInstituteUpgrading = false;

			int nowInstituteLevel =  controller.dataManager.GetInstituteLV();
			instituteLevelText.text = "Level:" + nowInstituteLevel;
			instituteHpTextList[ nowInstituteLevel - 1 ].color = Color.green;

			DebugUtils.Log( DebugUtils.Type.InstitutesSkill, "Now instituteLevel is " + nowInstituteLevel );

			if( nowInstituteLevel < InstituteMaxLv )
			{
				levelUpCostText.text = controller.instituteProtoData[nowInstituteLevel - 1].LevelUpCost.ToString();
				InstituteUpgradeEffect( nowInstituteLevel );
			}
			else
			{
				levelUpCostText.text = "0";
				instituteLevelUpButton.gameObject.SetActive( false );
				institutelevelUpText.gameObject.SetActive( false );
				instituteArrowIconRectTransform.gameObject.SetActive( false );
			}
				
			for( int i = 0; i < instituteSkill_Items.Count; i++ )
			{
				instituteSkill_Items[i].InstituteLevelUpSucceeded( nowInstituteLevel );
			}

			RefreshInstitutePanelStatus( controller.emberCount );

			DebugUtils.Log( DebugUtils.Type.BuildingLevelUp, "The InstituteLevelUP completed." );
		}

		private void InstituteUpgradeEffect( int instituteLV )
		{
			float temp;
			temp = instituteHpTextList[ instituteLV - 1 ].rectTransform.localPosition.y;
			instituteHpTextList[ instituteLV - 1 ].rectTransform.localPosition = new Vector3( instituteHpTextList[ instituteLV - 1 ].rectTransform.localPosition.x, instituteArrowIconRectTransform.localPosition.y ,instituteHpTextList[ instituteLV - 1 ].rectTransform.localPosition.z  );
			instituteArrowIconRectTransform.localPosition = new Vector3( instituteArrowIconRectTransform.localPosition.x, temp, instituteArrowIconRectTransform.localPosition.z );
		}

		public void InstituteSkillLevelUpComplete( int applySkillID, int upgradeSkillID )
		{
			for( int i = 0; i < instituteSkill_Items.Count; i++ )
			{
				if( instituteSkill_Items[i].nextInstituteSkill.skillID == applySkillID && instituteSkill_Items[i].nextInstituteSkill.nextSkillID == upgradeSkillID )
				{
					instituteSkill_Items[i].TheSkillUpgrade( applySkillID, upgradeSkillID );
				}
				else if( instituteSkill_Items[i].nextInstituteSkill.skillID == applySkillID )
				{
					instituteSkill_Items[i].TheSkillRepeatedlyUpgrade( upgradeSkillID );
				}
			}
		}

		private List<int> playerSetedInstituteSkills;

        public void InitInstituteItem()
        {
			if( playerSetedInstituteSkills == null )
			{
				playerSetedInstituteSkills = controller.dataManager.GetPlayerSetedPackageInstituteSkills( controller.dataManager.GetBattleConfigInsituteSkillIndex( controller.dataManager.GetBattleType() ));
			}

            GameResourceLoadManager.GetInstance().LoadResource( INSTITUTE_SKILL_ITEM_PATH, OnLoadInstituteItem, true );
        }

        private void OnLoadInstituteItem( string name, UnityEngine.Object obj, System.Object param )
        {
			instituteSkill_Items = new List<InstituteSkillItem>();

			for ( int i = 0; i < playerSetedInstituteSkills.Count; i++ )
            {
				InstituteSkillItem instituteSkillitem = CommonUtil.CreateItem<InstituteSkillItem>( obj, instituteSkillGroup.transform );	
                instituteSkillitem.gameObject.SetActive( true );

                instituteSkillitem.name = "InstituteSkillItem_" + i;
                instituteSkillitem.clickUpButton = InstituteSkillApplyUpgrade;
                instituteSkillitem.clickDownButton = InstituteSkillApplyUpgrade;

				instituteSkillitem.InitSlotSkills( controller.playerInstituteSkills[i] );
				instituteSkillitem.RefreshInsituteSkillItem( false );
				instituteSkillitem.SetInsituteSkillUI(i);

				instituteSkill_Items.Add( instituteSkillitem );
            }
        }

        public void OpenInstitutePopUp()
        {
            institutePopUp.SetActive( true );

			if( instituteSkill_Items == null )
			{
				InitInstituteItem();
			}
            OpenBattleUI( BattleUIType.InstitutePopUp );
        }

        private void CloseInstitutePopUp()
        {
            institutePopUp.SetActive( false );
            RemoveBattleUI( BattleUIType.InstitutePopUp );
        }


        private void InstituteSkillApplyUpgrade( int levelUpSkillID )
        {
			controller.InstituteSkillApplyUpgrade( levelUpSkillID );
        }

		private void InstituteSkillApplyUpgrade( int levelUpSkillID, int levelUpNum )
        {
			controller.InstituteSkillApplyUpgrade( levelUpSkillID, levelUpNum );
        }

		private void ResetInstitutePopUpData()
		{
			instituteLevelUpButton.gameObject.SetActive( true );
			institutelevelUpText.gameObject.SetActive( true );

			InitInstitutePanel();
			if( instituteSkill_Items!= null )
			{
				for( int i = 0; i < instituteSkill_Items.Count; i++ )
				{
					instituteSkill_Items[i].ResetSkillData( i );
				}
			}
		}

		public void DestroyInstitute()
		{
			if( institutePopUp.activeInHierarchy )
			{
				institutePopUp.SetActive( false );
			}
		}

        #endregion

        #region Init Tower Item

        private int towerItemCount;

        public void InitTowerItem()
        {
            towerItemCount = controller.GetSelfTowersCount();

            GameResourceLoadManager.GetInstance().LoadResource( TOWER_ITEM_PATH, OnLoadTowerItem, true );
        }

        private void OnLoadTowerItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<TowerItem>( tower_Items );

            for ( int i = 0; i < towerItemCount; i++ )
            {
                TowerItem towerItem;
                if ( towerItemCount > tower_Items.Count )
                {
                    towerItem = CommonUtil.CreateItem<TowerItem>( obj, towerItemGroup.transform );
                    tower_Items.Add( towerItem );
                }

                towerItem = tower_Items[i];
                towerItem.gameObject.SetActive( true );
				towerItem.view = this;
                towerItem.index = i;
                towerItem.name = "TowerItem_" + i;
                towerItem.clickButtonEvent = ClickDeployTowerButton;
                towerItem.buildingID = controller.GetTowersId( i );
                towerItem.nameString = controller.GetTowerName( i );
                towerItem.icon = controller.GetTowerIcon( i );
                towerItem.propString1 = controller.GetTowerPropString1( i );
                towerItem.propString2 = controller.GetTowerPropString2( i );
                towerItem.cost = controller.GetTowerCost( i );

                towerItem.RefreshItem();
            }
        }

        private void ClickDeployTowerButton()
        {

        }

        public void OpenTowerPopUp()
        {
            towerPopUp.gameObject.SetActive( true );

            InitTowerItem();
        }

        public void CloseTowerPopUp()
        {
            towerPopUp.SetActive( false );
        }

		public uint GetBuildedTowerNum()
		{
			return controller.buildedTowerNum;
		}

        #endregion

		#region Tap Tower PopUp

		public void OpenTapTowerPopUp( int cost, long id, Transform pos )
		{
			if( tapTowerPopUpItem == null )
			{
				tapTowerPopUpItem = tapTowerPopUp.AddComponent<TapTowerPopUp>();
			}

			tapTowerPopUpItem.headerTra = pos;
			tapTowerPopUpItem.RefreshItem( cost, id );
		}

        #endregion

        #region Set MiniMap UI

        private void SetLeftTopBaseDataUI( bool isOpen )
        {
            //_emberText.gameObject.SetActive( isOpen );
            //_killCountText.gameObject.SetActive( isOpen );
            //_killedCountText.gameObject.SetActive( isOpen );

            //killImage.gameObject.SetActive( isOpen );
            //deathImage.gameObject.SetActive( isOpen );
            //emberImage.gameObject.SetActive( isOpen );

            largerButton.gameObject.SetActive( isOpen );
            resetButton.gameObject.SetActive( !isOpen );
            resetButtonBg.SetActive( !isOpen );
            largerButtonBg.SetActive( isOpen );
            microphoneButtonBg.SetActive( isOpen );
            loudspeakerButtonBg.SetActive( isOpen );
            microphoneButton.gameObject.SetActive( isOpen );
            loudspeakerButton.gameObject.SetActive( isOpen );
        }

        #endregion

		#region Set BuffPanel

		public void SetBuffPanelText( string text )
		{
			attributeText.text = text;
			buildingBuffPanelRect.gameObject.SetActive ( true );
		}

		#endregion
    }
}
