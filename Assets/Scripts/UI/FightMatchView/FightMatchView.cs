using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using Resource;
using Utils;
using Data;

namespace UI
{
    public class FightMatchView : ViewBase
    {
        public enum FightMatchUIType
        {
            None = -1,
            MatchUI = 0,//into match UI 
            MatchingUI = 1,//begin match and find friend and enemy
            MatchSucceedUI = 2,//match succeed 
        }

        #region Component Name

        //RoomPanel
        private Transform roomPanel;
        private Text buttonText, titleText_match, titleText_matching, timerText_room;
        private Button matchButton, backButton, cancelButton;
        private GameObject timerObj;

        private Transform friendTran;
        private Image friendIconImage;
        private GridLayoutGroup friendUnitGroup;
        private Text friendNameText;

        private Transform myselfTran;
        private Image myselfIconImage;
        private ToggleGroup armyToggleGroup;
        private Toggle armyToggle1, armyToggle2, armyToggle3;
        private GridLayoutGroup myselfUnitGroup;
        private Text myselfNameText, runeLevelText, armyTgText1, armyTgText2, armyTgText3;
        private Dropdown runeDropdown, instituteSkillDropdown;

        //FriendPanel
        private Transform friendPanel;
        private Toggle friendToggle, nearbyToggle;
        private Text friendText, nearbyText;
        private ScrollRect dragFriendItem;
        private GridLayoutGroup friendItemGroup;

        //MatchSucceedPanel
        private Transform matchSucceedPanel;
        private Text timerText_match;
        public Button intoGameButton;
        private GridLayoutGroup myselfSucceedGroup, enemySucceedGroup;

		//MatchChatPanel
		private MatchChatView matchChatPanel;

        #endregion

        #region Path

        //private const string MATCH_UNIT_ITEM_PATH = "Prefabs/UI/FightMatchItem/DeckUnitItem";
        //private const string FRIEND_ITEM_PATH = "Prefabs/UI/FightMatchItem/FriendItem";
        //private const string MATCH_LOAD_ITEM_PATH = "Prefabs/UI/FightMatchItem/MatchLoadItem";
        //private const string MATCH_SUCCEED_ITEM_PATH = "Prefabs/UI/FightMatchItem/MatchSucceedItem";

        //private const string PLAYER_ICON_PATH = "UITexture/Avatar_icon/";

        #endregion

        public long timeLeft_room;
        public long timeLeft_matchSucceed;

        private FightMatchController controller;
        private int currentArmyIndex, currentRuneIndex, currentInstituteSkillIndex;
        private bool isPVE;
        public bool beInvited;
        private BattleType currentBattleType;
        public FightMatchUIType currentUIType;

        private MatchFriendScrollView matchFriendScrollView;

        private List<MatchUnitItem> myselfUnitItems = new List<MatchUnitItem>();
        private List<MatchUnitItem> friendUnitItems = new List<MatchUnitItem>();
        public List<MatchSucceedItem> mySideSucceedItems = new List<MatchSucceedItem>();
        public List<MatchSucceedItem> enemySideSucceedItems = new List<MatchSucceedItem>();

        public List<FightMatchController.MatcherData> friendDatas = new List<FightMatchController.MatcherData>();
        public List<FightMatchController.MatcherData> enemyDatas = new List<FightMatchController.MatcherData>();

        public void EnterFightUI( bool isPVE, BattleType type, bool beInvited = false, List<Data.MatcherReadyData> dataList = null )
        {
            if ( !openState )
                OnEnter();

            this.isPVE = isPVE;
            this.beInvited = beInvited;
            currentBattleType = type;
            controller.SetBattleType( type );

            controller.SendMatchReadyDataC2S( type );

            if ( beInvited )
            {
                SetFriendUI( true );
                controller.SetMatcherDatas( dataList );
            }
            intoGameButton.interactable = true;

            StartCoroutine( "UpdataInvitationList" );
        }

        public override void OnExit( bool isGoBack )
        {
            base.OnExit( isGoBack );

            StopCoroutine( "UpdataInvitationList" );
        }

        public override void OnInit()
        {
            base.OnInit();

            controller = new FightMatchController( this );
            _controller = controller;

            #region RoomPanel 

            roomPanel = transform.Find( "RoomPanel" );

            timerObj = roomPanel.Find( "TimerObj" ).gameObject;
            armyToggleGroup = roomPanel.Find( "MyselfObj/ArmyToggleGroup" ).GetComponent<ToggleGroup>();
            buttonText = roomPanel.Find( "ButtonText" ).GetComponent<Text>();
            titleText_match = roomPanel.Find( "Title_MatchText" ).GetComponent<Text>();
            titleText_matching = roomPanel.Find( "Title_MatchingText" ).GetComponent<Text>();
            timerText_room = roomPanel.Find( "TimerObj/TimerText" ).GetComponent<Text>();
            matchButton = roomPanel.Find( "MatchButton" ).GetComponent<Button>();
            cancelButton = roomPanel.Find( "CancelButton" ).GetComponent<Button>();
            backButton = roomPanel.Find( "BackButton" ).GetComponent<Button>();

            friendTran = roomPanel.Find( "FriendObj" );
            friendIconImage = friendTran.Find( "FriendIconImage" ).GetComponent<Image>();
            friendIconImage.gameObject.SetActive( false );
            friendUnitGroup = friendTran.Find( "FriendDeckUnitGroup" ).GetComponent<GridLayoutGroup>();
            friendNameText = friendTran.Find( "FriendNameText" ).GetComponent<Text>();

            myselfTran = roomPanel.Find( "MyselfObj" );
            myselfIconImage = myselfTran.Find( "MyselfIconImage" ).GetComponent<Image>();
            myselfUnitGroup = myselfTran.Find( "MyselfDeckUnitGroup" ).GetComponent<GridLayoutGroup>();
            myselfNameText = myselfTran.Find( "MyselfNameText" ).GetComponent<Text>();
            runeLevelText = myselfTran.Find( "RuneLevelText" ).GetComponent<Text>();
            armyTgText1 = myselfTran.Find( "ArmyToggleText1" ).GetComponent<Text>();
            armyTgText2 = myselfTran.Find( "ArmyToggleText2" ).GetComponent<Text>();
            armyTgText3 = myselfTran.Find( "ArmyToggleText3" ).GetComponent<Text>();
            armyToggle1 = myselfTran.Find( "ArmyToggleGroup/ArmyToggle1" ).GetComponent<Toggle>();
            armyToggle2 = myselfTran.Find( "ArmyToggleGroup/ArmyToggle2" ).GetComponent<Toggle>();
            armyToggle3 = myselfTran.Find( "ArmyToggleGroup/ArmyToggle3" ).GetComponent<Toggle>();
            runeDropdown = myselfTran.Find( "RuneDropdown" ).GetComponent<Dropdown>();
            instituteSkillDropdown = myselfTran.Find( "InstituteSkillDropdown" ).GetComponent<Dropdown>();

            armyToggle1.AddListener( OnClickArmyToggleOne );
            armyToggle2.AddListener( OnClickArmyToggleTwo );
            armyToggle3.AddListener( OnClickArmyToggleThree );
            matchButton.AddListener( OnClickMatchButton, UIEventGroup.Middle, UIEventGroup.Middle );
            cancelButton.AddListener( OnClickCancelButton, UIEventGroup.Middle, UIEventGroup.Middle );
            backButton.AddListener( OnClickBackButton );

            runeDropdown.onValueChanged.AddListener( OnValueChangeRuneDropdown );
            instituteSkillDropdown.onValueChanged.AddListener( OnValueChangeInstituteSkilldown );

            #endregion

            #region FriendPanel

            friendPanel = transform.Find( "FriendPanel" );
            friendToggle = friendPanel.Find( "FriendToggle" ).GetComponent<Toggle>();
            nearbyToggle = friendPanel.Find( "NearbyToggle" ).GetComponent<Toggle>();
            friendText = friendPanel.Find( "FriendText" ).GetComponent<Text>();
            nearbyText = friendPanel.Find( "NearbyText" ).GetComponent<Text>();
            dragFriendItem = friendPanel.Find( "DragFriendItemPanel" ).GetComponent<ScrollRect>();
            friendItemGroup = friendPanel.Find( "DragFriendItemPanel/FriendItemGroup" ).GetComponent<GridLayoutGroup>();

            if ( dragFriendItem.GetComponent<MatchFriendScrollView>() == null )
                matchFriendScrollView = dragFriendItem.gameObject.AddComponent<MatchFriendScrollView>();
            else
                matchFriendScrollView = dragFriendItem.GetComponent<MatchFriendScrollView>();
            matchFriendScrollView.onCreateItemHandler = OnCreateMatchFriendItem;

            friendToggle.AddListener( OnClickFriendToggle );
            nearbyToggle.AddListener( OnClickNearbyToggle );

            #endregion

            #region MatchSucceedPanel

            matchSucceedPanel = transform.Find( "MatchSucceedPanel" );
            matchSucceedPanel.gameObject.SetActive( false );
            timerText_match = matchSucceedPanel.Find( "TimerText" ).GetComponent<Text>();
            myselfSucceedGroup = matchSucceedPanel.Find( "MyselfSideGroup" ).GetComponent<GridLayoutGroup>();
            enemySucceedGroup = matchSucceedPanel.Find( "EnemySideGroup" ).GetComponent<GridLayoutGroup>();
            intoGameButton = matchSucceedPanel.Find( "IntoGameButton" ).GetComponent<Button>();

            intoGameButton.AddListener( OnClickIntoGameButton );

            #endregion

            RefreshMatchFriendItem();

			matchChatPanel = transform.Find( "MatchChat" ).gameObject.AddComponent<MatchChatView>();
			matchChatPanel.Init();
        }

        #region Button & Toggle & Dropdown Event

        //RoomPanel
        private void OnClickArmyToggleOne( bool isOn )
        {
            armyToggle1.interactable = !isOn;
            if ( !isOn )
                return;

            armyToggle2.interactable = true;
            armyToggle3.interactable = true;

            currentArmyIndex = 0;

            controller.SendChangeArmyC2S( currentArmyIndex, currentBattleType );
        }

        private void OnClickArmyToggleTwo( bool isOn )
        {
            armyToggle2.interactable = !isOn;
            if ( !isOn )
                return;

            armyToggle1.interactable = true;
            armyToggle3.interactable = true;

            currentArmyIndex = 1;

            controller.SendChangeArmyC2S( currentArmyIndex, currentBattleType );
        }

        private void OnClickArmyToggleThree( bool isOn )
        {
            armyToggle3.interactable = !isOn;
            if ( !isOn )
                return;

            armyToggle1.interactable = true;
            armyToggle2.interactable = true;

            currentArmyIndex = 2;

            controller.SendChangeArmyC2S( currentArmyIndex, currentBattleType );
        }

        private void OnClickMatchButton()
        {
            if ( isPVE )
                ConnectPVE( currentBattleType );
            else
            {
                if ( beInvited )
                    controller.SendReady();
                else
                    controller.SendStartMatch();
            }
        }

        private void OnClickCancelButton()
        {
            if ( beInvited )
                controller.SendCancelReady();
            else
                controller.SendCancelingMatch();
        }

        private void OnClickBackButton()
        {
            controller.SendCancelInvitation();
        }

        private void OnValueChangeRuneDropdown( int index )
        {
            currentRuneIndex = index;
            controller.SendChangeRuneC2S( index, currentBattleType );
        }

        private void OnValueChangeInstituteSkilldown( int index )
        {
            currentInstituteSkillIndex = index;
            controller.SendChangeInstituteSkillC2S( index, currentBattleType );
        }

        //FriendPanel
        private void OnClickFriendToggle( bool isOn )
        {
            friendToggle.interactable = !isOn;
            if ( !isOn )
                return;


        }

        private void OnClickNearbyToggle( bool isOn )
        {
            nearbyToggle.interactable = !isOn;
            if ( !isOn )
                return;
        }

        //MatchSucceedPanel
        private void OnClickIntoGameButton()
        {
            controller.SendReadyMatch();
        }

        #endregion

        #region Init Match Unit Item

        private int unitCount = 9;

        public void InitMatchMyselfUnitItem()
        {
            myselfNameText.text = controller.GetMyselfName();
            runeLevelText.text = controller.GetRuneTotalLevel().ToString();

            if ( !string.IsNullOrEmpty( controller.GetMyselfIcon() ) )
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( controller.GetMyselfIcon(), delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    myselfIconImage.SetSprite( atlasSprite );
                } );
            }
            GameResourceLoadManager.GetInstance().LoadResource( "DeckUnitItem", OnLoadMySelfUnitItem, true );
        }

        public void InitMatchFriendUnitItem()
        {
            friendNameText.text = controller.GetFriendName();
            if ( !string.IsNullOrEmpty( controller.GetFriendIcon() ) )
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( controller.GetFriendIcon(), delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    friendIconImage.SetSprite( atlasSprite );
                } );
            }
            GameResourceLoadManager.GetInstance().LoadResource( "DeckUnitItem", OnLoadFriendUnitItem, true );
        }

        private void OnLoadMySelfUnitItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<MatchUnitItem>( myselfUnitItems );

            for ( int i = 0; i < unitCount; i++ )
            {
                MatchUnitItem unitItem;
                if ( myselfUnitItems.Count < unitCount )
                {
                    unitItem = CommonUtil.CreateItem<MatchUnitItem>( obj, myselfUnitGroup.transform );

                    myselfUnitItems.Add( unitItem );
                }
                unitItem = myselfUnitItems[i];
                unitItem.gameObject.SetActive( true );

                unitItem.icon = controller.GetMyselfArmyIcon( i );

                unitItem.RefreshMatchUnitItem();
            }
        }

        private void OnLoadFriendUnitItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<MatchUnitItem>( friendUnitItems );

            for ( int i = 0; i < unitCount; i++ )
            {
                MatchUnitItem unitItem;
                if ( friendUnitItems.Count < unitCount )
                {
                    unitItem = CommonUtil.CreateItem<MatchUnitItem>( obj, friendUnitGroup.transform );

                    friendUnitItems.Add( unitItem );
                }
                unitItem = friendUnitItems[i];
                unitItem.gameObject.SetActive( true );

                unitItem.icon = controller.GetFriendArmyIcon( i );

                unitItem.RefreshMatchUnitItem();
            }
        }

        #endregion

        #region Init Friend Item

        private void OnCreateMatchFriendItem( MatchFriendItem item )
        {
            item.inviationEvent = ( MatchFriendItem ite ) => { controller.SendInvitationFriend( ite ); };
        }

        private void RefreshMatchFriendItem()
        {
            GameResourceLoadManager.GetInstance().LoadResource( "FriendItem", OnLoadFriendItem, true );
        }

        private void OnLoadFriendItem( string name, UnityEngine.Object obj, System.Object param )
        {
            matchFriendScrollView.InitDataBase( dragFriendItem, obj, 1, 380, 100, 0, new Vector3( 180, -50, 0 ) );

            if ( controller.GetFriendDataList().Count > 0 )
                RefrshMatchFriendListData();
        }

        public void RefrshMatchFriendListData()
        {
            matchFriendScrollView.InitializeWithData( controller.GetFriendDataList() );
        }

        #endregion

		#region MatchChatPanel

		public void SetChatFriends()
		{
			List<long>ids = controller.GetFriendsID();
			if ( ids != null && ids.Count>0 ) 
			{
				matchChatPanel.SetFriendData( ids );
			}
		}

		#endregion

        #region Init Match Succeed Item

        private int friendsCount;
        private int enemysCount;

        public void InitSucceedItem()
        {
            friendsCount = friendDatas.Count;
            enemysCount = enemyDatas.Count;

            GameResourceLoadManager.GetInstance().LoadResource( "MatchSucceedItem", OnLoadMySideSucceedItem, true );
            GameResourceLoadManager.GetInstance().LoadResource( "MatchSucceedItem", OnLoadEnemySideSucceedItem, true );

            //RefreshMatchSucceedUIData();
        }

        private void OnLoadMySideSucceedItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<MatchSucceedItem>( mySideSucceedItems );

            for ( int i = 0; i < friendsCount; i++ )
            {
                MatchSucceedItem succeedItem;
                if ( mySideSucceedItems.Count < friendsCount )
                {
                    succeedItem = CommonUtil.CreateItem<MatchSucceedItem>( obj, myselfSucceedGroup.transform );

                    mySideSucceedItems.Add( succeedItem );
                }
                succeedItem = mySideSucceedItems[i];
                succeedItem.gameObject.SetActive( true );

                succeedItem.icon = friendDatas[i].icon;
                succeedItem.nameStr = friendDatas[i].name;
                succeedItem.isReady = friendDatas[i].isReady;
                succeedItem.isMyself = friendDatas[i].isMyself;

                succeedItem.RefreshMatch2V2SucceedItem();
            }
        }

        private void OnLoadEnemySideSucceedItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<MatchSucceedItem>( enemySideSucceedItems );

            for ( int i = 0; i < enemysCount; i++ )
            {
                MatchSucceedItem succeedItem;
                if ( enemySideSucceedItems.Count < enemysCount )
                {
                    succeedItem = CommonUtil.CreateItem<MatchSucceedItem>( obj, enemySucceedGroup.transform );

                    enemySideSucceedItems.Add( succeedItem );
                }
                succeedItem = enemySideSucceedItems[i];
                succeedItem.gameObject.SetActive( true );

                succeedItem.icon = enemyDatas[i].icon;
                succeedItem.nameStr = enemyDatas[i].name;
                succeedItem.isReady = enemyDatas[i].isReady;
                succeedItem.isMyself = enemyDatas[i].isMyself;

                succeedItem.RefreshMatch2V2SucceedItem();
            }
        }

        #endregion

        //Open Fight Match UI
        public void OpenMatchUI()
        {
            SetFightMatchUI( FightMatchUIType.MatchUI );

            SetDropdown( controller.GetRuneDropdownTextList(), controller.GetInstituteSkillDropdownTextList() );

            runeDropdown.captionText.text = controller.GetRuneDropdownTextList()[controller.GetCurrentRuneIndex( currentBattleType )];
            instituteSkillDropdown.captionText.text = controller.GetInstituteSkillDropdownTextList()[controller.GetCurrentInstituteSkillIndex( currentBattleType )];
            SetArmyToggle( controller.GetCurrentArmyIndex( currentBattleType ) );

            runeLevelText.text = controller.GetRuneTotalLevel().ToString();

            InitMatchMyselfUnitItem();

            if ( beInvited )
                InitMatchFriendUnitItem();
        }

        public void OpenMatchingUI()
        {
            SetFightMatchUI( FightMatchUIType.MatchingUI );

            SetMatchingUIData( timeLeft_room );
        }

        public void OpenMatchSucceedUI()
        {
            SetFightMatchUI( FightMatchUIType.MatchSucceedUI );
            SoundManager.Instance.PlaySound( 60114, true );
        }

        //Set Friend UI
        public void SetFriendUI( bool state )
        {
            friendIconImage.gameObject.SetActive( state );
            if ( !state )
                friendNameText.text = "???";

            if ( friendUnitItems.Count > 0 )
                for ( int i = 0; i < friendUnitItems.Count; i++ )
                {
                    friendUnitItems[i].gameObject.SetActive( state );
                }
        }

        //Set Fight Match UI
        private void SetFightMatchUI( FightMatchUIType uiType )
        {
            currentUIType = uiType;
            switch ( uiType )
            {
                case FightMatchUIType.MatchUI:
                    roomPanel.gameObject.SetActive( true );
                    friendPanel.gameObject.SetActive( true );
                    backButton.gameObject.SetActive( true );
                    matchSucceedPanel.gameObject.SetActive( false );
                    timerObj.gameObject.SetActive( false );
                    titleText_match.gameObject.SetActive( true );
                    titleText_matching.gameObject.SetActive( false );
                    armyToggleGroup.gameObject.SetActive( true );
                    armyTgText1.gameObject.SetActive( true );
                    armyTgText2.gameObject.SetActive( true );
                    armyTgText3.gameObject.SetActive( true );
                    runeDropdown.interactable = true;
                    instituteSkillDropdown.interactable = true;
                    matchButton.interactable = true;
                    buttonText.text = beInvited ? "准备" : "开始匹配";
                    matchButton.gameObject.SetActive( true );
                    cancelButton.gameObject.SetActive( false );

                    if ( currentBattleType == Data.BattleType.BattleP1vsP1 )
                    {
                        titleText_match.text = "1V1 匹配";
                        friendTran.gameObject.SetActive( false );
                    }
                    else if ( currentBattleType == Data.BattleType.BattleP2vsP2 )
                    {
                        titleText_match.text = "2V2 匹配";
                        friendTran.gameObject.SetActive( true );
                    }
                    else if ( currentBattleType == Data.BattleType.Tranining )
                    {
                        titleText_match.text = "训练模式";
                        friendTran.gameObject.SetActive( false );
                    }
                    else if ( currentBattleType == Data.BattleType.Survival )
                    {
                        titleText_match.text = "生存模式";
                        friendTran.gameObject.SetActive( false );
                    }
                    break;
                case FightMatchUIType.MatchingUI:
                    roomPanel.gameObject.SetActive( true );
                    friendPanel.gameObject.SetActive( true );
                    backButton.gameObject.SetActive( false );
                    matchSucceedPanel.gameObject.SetActive( false );
                    timerObj.gameObject.SetActive( true );
                    titleText_match.gameObject.SetActive( false );
                    titleText_matching.gameObject.SetActive( true );
                    armyToggleGroup.gameObject.SetActive( false );
                    armyTgText1.gameObject.SetActive( false );
                    armyTgText2.gameObject.SetActive( false );
                    armyTgText3.gameObject.SetActive( false );
                    runeDropdown.interactable = false;
                    instituteSkillDropdown.interactable = false;
                    buttonText.text = "取消匹配";
                    matchButton.gameObject.SetActive( false );
                    cancelButton.gameObject.SetActive( !beInvited );
                    timeLeft_room = 0;
                    break;
                case FightMatchUIType.MatchSucceedUI:
                    roomPanel.gameObject.SetActive( false );
                    friendPanel.gameObject.SetActive( false );
                    matchSucceedPanel.gameObject.SetActive( true );
                    break;
            }
        }

        //Set Matcher Cancelled
        public void SetMatcherCancelled()
        {
            intoGameButton.interactable = true;
            StartCoroutine( "ChangeUIToMatching" );
        }

        IEnumerator ChangeUIToMatching()
        {
            yield return new WaitForSeconds( 1f );

            OpenMatchingUI();
        }

        //Set Army Toggle 
        private void SetArmyToggle( int armyIndex )
        {
            armyToggle1.interactable = armyToggle2.interactable = armyToggle3.interactable = true;
            armyToggleGroup.allowSwitchOff = true;
            armyToggle1.isOn = armyToggle2.isOn = armyToggle3.isOn = false;
            switch ( armyIndex )
            {
                case 0:
                    armyToggle1.interactable = false;
                    break;
                case 1:
                    armyToggle2.interactable = false;
                    break;
                case 2:
                    armyToggle3.interactable = false;
                    break;
            }
            armyToggleGroup.allowSwitchOff = false;
        }

        //Set Dropdown Data
        private void SetDropdown( List<string> runeDropTextList, List<string> skillDropTextList )
        {
            runeDropdown.ClearOptions();
            instituteSkillDropdown.ClearOptions();

            runeDropdown.AddOptions( runeDropTextList );

            instituteSkillDropdown.AddOptions( skillDropTextList );
        }

        //Load Resources Test
        int rate = 0;
        private void PlusLoadRate()
        {
            rate += 10;
            if ( rate > 100 )
            {
                rate = 100;
                currentUIType = FightMatchUIType.None;
            }
            controller.SendNoticeC2S( rate );
        }

        private void SetMatchingUIData( long timeLeft )
        {
            timerText_room.text = string.Format( "匹配用时 <color=#c40902>{0}</color> 秒", timeLeft );
        }

        private void SetMatchSucceedUIData( long timeLeft )
        {
            timerText_match.text = string.Format( "{0}:{1}", timeLeft / 60, string.Format( "{0:00}", timeLeft % 60 ) );
        }

        private void RefreshMatchSucceedUIData()
        {
            //int count = friendsCount + enemysCount;
            //int readyNum = 0;
            //foreach ( FightMatchController.MatcherData friend in friendDatas )
            //{
            //    if ( friend.isReady )
            //        readyNum++;
            //}
            //foreach ( FightMatchController.MatcherData enemy in enemyDatas )
            //{
            //    if ( enemy.isReady )
            //        readyNum++;
            //}
        }

        #region Timer&&Loading

        float oneInt;
        float loadingTime;
        private void Update()
        {
            if ( currentUIType == FightMatchUIType.MatchingUI )
            {
                oneInt -= Time.deltaTime;
                if ( oneInt < 0 )
                {
                    oneInt = 1;

                    SetMatchingUIData( timeLeft_room++ );
                }
            }
            else if ( currentUIType == FightMatchUIType.MatchSucceedUI )
            {
                oneInt -= Time.deltaTime;
                if ( oneInt < 0 )
                {
                    oneInt = 1;
                    if ( timeLeft_matchSucceed <= 0 )
                        timeLeft_matchSucceed = 0;
                    SetMatchSucceedUIData( timeLeft_matchSucceed-- );
                }
            }
            else
            {
                //  
            }
        }

        #endregion

        #region PVE

        private void ConnectPVE( BattleType type )
        {
            SetPveBattleData( type );
            
            StartCoroutine( EnterLoadingSene() );
        }

        private void SetPveBattleData( BattleType type )
        {
            DataManager clientData = DataManager.GetInstance();
            clientData.ResetMatchers();

            // TODO: test data
            Matcher robot = new Matcher();
            robot.playerId = 0;     //AI's ID must be consistent with entering the battle
            robot.name = "疯狂的电脑";
            robot.side = MatchSide.Red;
            robot.portrait = "EmberAvatar_40";

            Matcher myself = new Matcher();
            myself.playerId = clientData.GetPlayerId();
            myself.name = clientData.GetPlayerNickName();
            myself.side = MatchSide.Blue;
            myself.portrait = clientData.GetPlayerHeadIcon();

            clientData.SetMatcher( robot );
            clientData.SetMatcher( myself );
        }

        private IEnumerator EnterLoadingSene()
        {
            AsyncOperation async = SceneManager.LoadSceneAsync( "Loading" );
            yield return async;
        }

        #endregion

        #region Invation Match 

        private void OpenPopUp( string title, string concent, System.Action callBack, AlertType alertType )
        {
            string concentText = concent;
            string titleText = title;
            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, callBack, alertType, concentText, titleText );
        }

        public void PopFriendInBattle()
        {
            OpenPopUp( "提示", "您邀请的好友已进入游戏", null, AlertType.ConfirmAlone );
        }

        public void PopFriendInMatching()
        {
            OpenPopUp( "提示", "您邀请的好友在组队中", null, AlertType.ConfirmAlone );
        }

        public void PopFriendRefuse( string friendName )
        {
            OpenPopUp( "提示", string.Format( "<size=25>{0}</size>拒绝了您的邀请", friendName ), null, AlertType.ConfirmAlone );
        }

        public void SetStartMatchBtState( bool state )
        {
            matchButton.interactable = state;
            backButton.gameObject.SetActive( !state );
        }

        public void SetFriendReadyState( bool state )
        {
            buttonText.text = state ? "取消准备" : "准备";
            backButton.gameObject.SetActive( !state );
            matchButton.gameObject.SetActive( !state );
            cancelButton.gameObject.SetActive( state );
        }

        public void SetStartMatchState( bool state )
        {
            buttonText.gameObject.SetActive( !state );
            matchButton.gameObject.SetActive( !state );
            cancelButton.gameObject.SetActive( !state );
        }

        private IEnumerator UpdataInvitationList()
        {
            while ( currentUIType != FightMatchUIType.MatchSucceedUI )
            {
                controller.SendInvitationListC2S();

                yield return new WaitForSeconds( 60f );
            }
        }

        #endregion
    }
}
