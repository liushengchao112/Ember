using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

using Data;
using Resource;
using Network;
using Utils;
using Constants;

namespace UI
{
    public enum UIType
    {
        None = 0,
        MainLeftBar,
        MainTopBar,
        MainBottomBar,
        MainMenu,
        StoreScreen,
        StorePopUpUI,
        Background,
        PlayerInfoUI,
        ArmyManagementScreen,
        LoginScreen,
        BattleScreen,
        SettingScreen,
        FightMatchScreen,
        BlackMarketScreen,
        BattleResultScreen,
        RuneMainUI,
        RunePopView,
        UnitMainUI,
		UnitInfoUI,
		ChatMainUI,
        PlayerBagView,
        GainItemView,
        SocialScreen,
        MailPopUpUI,
		RankView,
		BulletinBoardUI,
        SignView,
        TutorialModeUI,
        PlayBackUI,
    }

    public enum UIMenuDepth
    {
        Background = 0,

        // Menu
        LayerOne = 1,
        LayerTwo = 2,
        LayerThree = 3,
        LayerFour = 4,
        LayerFive = 5,
        LayerSix = 6,

        // Global Dialog
        LayerGlobalDialog = 10,

        //loudspeaker
        Loudspeaker = 11,

        // Mask
        Mask = 20,

        // Alert
        Alert = 30,

        //newbie
        LayerNewbie = 35,
    }

    public enum UITransitionMode
    {
        /// <summary>
        /// Belonges to filter UI
        /// </summary>
        None = -1,

        /// <summary>
        /// recycle all UI excpet main left bar and main top bar and bottom bar
        /// </summary>
        Low = 0,

        /// <summary>
        /// recycle all UI excpet  main top bar and left bar
        /// </summary>
        Middle = 1,

        /// <summary>
        /// recycle all UI excpet mian top bar
        /// </summary>
        High = 2,

        /// <summary>
        /// recycle all UI
        /// </summary>
        All = 3,
    }

    public enum UIManagerLocateState
    {
        None,
        Login,
        MainMenu,
        BattleModeView,
        Battle,
        PlayBackView,
    }

    public delegate void OnGetViewBaseHandler( ViewBase ui, System.Object param );

    /// <summary>
    /// UIManager now is attached to MainMenu'UIRoot and Battle'UIRoot
    /// Set static variable locateState will decide which window will be show on scene start
    /// When scene change, all ui instance, buffer, layout will be destroy, and will be rebuild at next scene begin.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager instance;

        public static UIManager Instance
        {
            get
            {
                if ( instance == null )
                {
                    instance = GameObject.Find( "UIRoot" ).GetComponent<UIManager>();
                }
                return instance;
            }
        }

        public static UIManagerLocateState locateState = UIManagerLocateState.None;

        // Common window
        private AlertView alertUI;
        private FriendInvationAlertView friendInvationAlertUI;
        private LoudspeakerView loudspeakerView;
        private NewbieGuideView newbieGuideView;

        private Canvas mainCanvas;
        private GameObject inactiveUIPools;
        private Dictionary<UIType, string> _uiRegisterDic;
        private Dictionary<UIType, ViewBase> _activeUIDic;
        private Dictionary<UIType, ViewBase> _inactiveUIDic;
        private Dictionary<UIType, OnGetViewBaseHandler> _waitingUILoadedDic;
        private Transform[] _depthLayoutAry;
        private string curSceneName;

        public void GetUIByType( UIType type, OnGetViewBaseHandler onGetViewBaseHandler, bool syncLoad = false )
        {
            if ( _activeUIDic.ContainsKey( type ) )
            {
                // Current UI are active.
                onGetViewBaseHandler.Invoke( _activeUIDic[type], type );
            }
            else
            {
                if ( _inactiveUIDic.ContainsKey( type ) )
                {
                    // Current UI game object can be find in uiPool.
                    ResumeView( type, _inactiveUIDic[type], onGetViewBaseHandler );
                }
                else
                {
                    // Get current UI at first time.
                    if ( _waitingUILoadedDic.ContainsKey( type ) )
                    {
                        _waitingUILoadedDic[type] += onGetViewBaseHandler;
                    }
                    else
                    {
                        _waitingUILoadedDic.Add( type, onGetViewBaseHandler );
                        StartLoadView( type, syncLoad );
                    }
                }
            }
        }

        public void RecycleUI( ViewBase giveBack )
        {
            try
            {
                _activeUIDic.Remove( giveBack.uiType );
                _inactiveUIDic.Add( giveBack.uiType, giveBack );

                giveBack.gameObject.SetActive( false );
                giveBack.transform.SetParent( inactiveUIPools.transform );
            }
            catch ( Exception e )
            {
                DebugUtils.LogError( DebugUtils.Type.UI, "Recycle UI failed : ui type = " + giveBack.uiType.ToString() );
                DebugUtils.LogError( DebugUtils.Type.UI, "Recycle UI failed : Exception = " + e.ToString() );
            }
        }

        #region Private
        void Awake()
        {
            instance = this;
            _uiRegisterDic = new Dictionary<UIType, string>();
            _activeUIDic = new Dictionary<UIType, ViewBase>();
            _waitingUILoadedDic = new Dictionary<UIType, OnGetViewBaseHandler>();
            _inactiveUIDic = new Dictionary<UIType, ViewBase>();

            InitLayout();
            RegisterView();
            RegisterEvent();
            ApplyLocateState();
        }

        void Start()
        {
            curSceneName = DataManager.GetInstance().SceneName();
        }

        private void ApplyLocateState()
        {
            if ( locateState == UIManagerLocateState.Login )
            {
                EnterLoginScreen();
            }
            else if ( locateState == UIManagerLocateState.MainMenu )
            {
                EnterMainMenu();
            }
            else if ( locateState == UIManagerLocateState.BattleModeView )
            {
                EnterBattleModeView();
            }
            else if ( locateState == UIManagerLocateState.Battle )
            {
                // do nothing
                // battle manager will invoke enter battle 
            }
            else
            {
                //DebugUtils.LogError( DebugUtils.Type.UI, " Illegal UIManager locate state! " );
            }
        }

        public void EnterLoginScreen()
        {
            ViewBase.ClearViewStack();
            GetUIByType( UIType.LoginScreen, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); }, true );

            locateState = UIManagerLocateState.Login;
        }
        
        public void EnterMainMenu()
        {
            ViewBase.ClearViewStack();
            ViewBase.blockViewlist = new List<UIType> { UIType.Background, UIType.MainTopBar, UIType.MainLeftBar, UIType.MainBottomBar };

            // Don't change this sequence unless the ui layout changed
            GetUIByType( UIType.Background, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); }, true );
            GetUIByType( UIType.MainTopBar, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); } );
            GetUIByType( UIType.MainLeftBar, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); } );
            GetUIByType( UIType.MainBottomBar, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); } );
            GetUIByType( UIType.MainMenu, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); } );

            locateState = UIManagerLocateState.MainMenu;

            //preload unit model resouses
            Invoke( "PreloadedResources", 1f );
        }

        public void EnterBattleModeView()
        {
            ViewBase.ClearViewStack();
            ViewBase.blockViewlist = new List<UIType> { UIType.Background, UIType.MainTopBar, UIType.MainLeftBar, UIType.MainBottomBar };

            // Don't change this sequence unless the ui layout changed
            GetUIByType( UIType.Background, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); } );
            GetUIByType( UIType.MainTopBar, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); } );
            GetUIByType( UIType.MainLeftBar, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); } );
            GetUIByType( UIType.MainBottomBar, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); } );

            locateState = UIManagerLocateState.BattleModeView;
        }

        // TODO: Temp code EnterBattle must be posted in InputHandler, and after get the matchlist from server
        public void EnterBattleMenu( Action onComplete = null )
        {
            ViewBase.ClearViewStack();
            ViewBase.blockViewlist = new List<UIType>();

            if ( DataManager.GetInstance().GetBattleSimluateState() )
            {
                locateState = UIManagerLocateState.PlayBackView;

                GetUIByType( UIType.PlayBackUI, ( ViewBase ui , System.Object param ) =>
                {
                    ui.OnEnter();
                    if ( onComplete != null )
                    {
                        onComplete();
                    }
                } );
            }
            else
            {
                GetUIByType( UIType.BattleScreen, ( ViewBase ui, System.Object param ) =>
                {
                    ui.OnEnter();
                    if ( onComplete != null )
                    {
                        onComplete();
                    }
                } );

                locateState = UIManagerLocateState.Battle;
                DataManager.GetInstance().SetPlayerIsInBattle( true );
            }

            SoundManager.Instance.PlayMusic( GameConstants.BGM_BATTLE_ID );
        }

        private void InitLayout()
        {
            mainCanvas = transform.Find( "EmberUICanvas" ).GetComponent<Canvas>();
            inactiveUIPools = transform.Find( "UIPool" ).gameObject;

            _depthLayoutAry = new Transform[(int)UIMenuDepth.LayerNewbie + 1];
            _depthLayoutAry[(int)UIMenuDepth.Background] = new GameObject( "Layer_Background" ).transform;
            _depthLayoutAry[(int)UIMenuDepth.LayerOne] = new GameObject( "Layer_One" ).transform;
            _depthLayoutAry[(int)UIMenuDepth.LayerTwo] = new GameObject( "Layer_Two" ).transform;
            _depthLayoutAry[(int)UIMenuDepth.LayerThree] = new GameObject( "Layer_Three" ).transform;
            _depthLayoutAry[(int)UIMenuDepth.LayerFour] = new GameObject( "Layer_Four" ).transform;
            _depthLayoutAry[(int)UIMenuDepth.LayerFive] = new GameObject( "Layer_Five" ).transform;

            _depthLayoutAry[(int)UIMenuDepth.LayerGlobalDialog] = new GameObject( "Layer_GlobalDialog" ).transform;
            _depthLayoutAry[(int)UIMenuDepth.Loudspeaker] = new GameObject( "Layer_Loudspeaker" ).transform;
            _depthLayoutAry[(int)UIMenuDepth.Mask] = new GameObject( "Mask" ).transform;
            _depthLayoutAry[(int)UIMenuDepth.Alert] = new GameObject( "Alert" ).transform;
            _depthLayoutAry[(int)UIMenuDepth.LayerNewbie] = new GameObject( "Layer_Newbie" ).transform;

            for ( int i = 0; i < _depthLayoutAry.Length; i++ )
            {
                if ( _depthLayoutAry[i] != null )
                {
                    _depthLayoutAry[i].SetParent( mainCanvas.transform );
                    _depthLayoutAry[i].localPosition = Vector3.zero;
                    _depthLayoutAry[i].localRotation = Quaternion.identity;
                    _depthLayoutAry[i].localScale = Vector3.one;

                    // this step will destoryed the transform that attached on gameobject before
                    RectTransform nodeT = _depthLayoutAry[i].gameObject.AddComponent<RectTransform>();

                    nodeT.offsetMax = Vector2.zero;
                    nodeT.offsetMin = Vector2.zero;
                    nodeT.pivot = Vector2.one / 2;
                    nodeT.anchorMax = Vector2.one;
                    nodeT.anchorMin = Vector2.zero;

                    _depthLayoutAry[i] = nodeT.transform;
                }
            }
        }

        private void RegisterView()
        {
            _uiRegisterDic = new Dictionary<UIType, string>();
            _uiRegisterDic.Add( UIType.Background, "MainBackgroundUI" );
            _uiRegisterDic.Add( UIType.MainLeftBar, "MainUILeftBar" );
            _uiRegisterDic.Add( UIType.MainTopBar, "MainUITopBar" );
            _uiRegisterDic.Add( UIType.MainBottomBar, "MainUIBottomBar" );
            _uiRegisterDic.Add( UIType.MainMenu, "MainMenuView" );
            _uiRegisterDic.Add( UIType.StoreScreen, "StoreScreen" );
            _uiRegisterDic.Add( UIType.StorePopUpUI, "StorePopUpUI" );
            _uiRegisterDic.Add( UIType.PlayerInfoUI, "PlayerInfoUI" );
            _uiRegisterDic.Add( UIType.ArmyManagementScreen, "ArmyManagementScreen" );
            _uiRegisterDic.Add( UIType.LoginScreen, "LoginScreen" );
            _uiRegisterDic.Add( UIType.BattleScreen, "BattleScreen" );
            _uiRegisterDic.Add( UIType.SettingScreen, "SettingScreen" );
            _uiRegisterDic.Add( UIType.FightMatchScreen, "FightMatchScreen" );
            _uiRegisterDic.Add( UIType.BlackMarketScreen, "BlackMarketScreen" );
            _uiRegisterDic.Add( UIType.BattleResultScreen, "BattleResultUI" );
            _uiRegisterDic.Add( UIType.RuneMainUI, "RuneMainUI" );
			_uiRegisterDic.Add( UIType.UnitMainUI, "UnitMainUI" );
			_uiRegisterDic.Add( UIType.UnitInfoUI, "UnitInfoUI" );
			_uiRegisterDic.Add( UIType.ChatMainUI, "ChatMainUI" );
            _uiRegisterDic.Add( UIType.PlayerBagView, "PlayerBagView" );
            _uiRegisterDic.Add( UIType.GainItemView, "GainItemPopUpUI" );
			_uiRegisterDic.Add( UIType.SocialScreen, "SocialScreen" );
            _uiRegisterDic.Add( UIType.MailPopUpUI, "MailPopUpUI" );
			_uiRegisterDic.Add( UIType.RankView, "RankUI" );
			_uiRegisterDic.Add( UIType.BulletinBoardUI , "BulletinBoardUI" );
            _uiRegisterDic.Add( UIType.SignView, "SignUI" );
            _uiRegisterDic.Add( UIType.TutorialModeUI, "TutorialModeUI" );
            _uiRegisterDic.Add( UIType.PlayBackUI, "PlayBackUI" );
            _uiRegisterDic.Add( UIType.RunePopView, "RunePopView");
        }

        private void StartLoadView( UIType uiType, bool syncLoad )
        {
            if( syncLoad )
            {
                GameObject go = GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( _uiRegisterDic[uiType] );
                OnLoadedView( go, uiType );
            }
            else
            {
                GameResourceLoadManager.GetInstance().LoadAssetAsync<GameObject, UIType>( _uiRegisterDic[uiType], OnLoadedView, uiType );
            }
        }

        private void ResumeView( UIType uiType, ViewBase ui, OnGetViewBaseHandler onResumUI )
        {
            if ( ui != null )
            {
                ChangeUIViewDepthBySetParent( ui.transform, ui.uiMenuDepth );
                ui.transform.localPosition = Vector3.zero;
                ui.transform.localScale = Vector3.one;

                _inactiveUIDic.Remove( ui.uiType );
                _activeUIDic.Add( ui.uiType, ui );

                onResumUI( ui, ui.uiType );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.UI, string.Format( " Can't find inactive ui, uiType = {0}", uiType ) );
            }
        }

        private void ChangeUIViewDepthBySetParent( Transform uiTrans, UIMenuDepth menuDepth )
        {
            if ( _depthLayoutAry.Length >= (int)menuDepth )
            {
                Transform t = _depthLayoutAry[(int)menuDepth];
                if ( t != null )
                {
                    uiTrans.SetParent( t );
                }
                else
                {
                    DebugUtils.LogError( DebugUtils.Type.UI, string.Format( " Didn't exist MenuDepth {0} layout, default add to mainCanvas ", menuDepth.ToString() ) );
                    uiTrans.SetParent( mainCanvas.transform );
                }
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.UI, string.Format( " MenuDepth {0} is out of layout, default add to mainCanvas ", menuDepth.ToString() ) );
                uiTrans.SetParent( mainCanvas.transform );
            }
            uiTrans.localPosition = Vector2.zero;
            uiTrans.localScale = Vector3.one;
        }

        void OnDestroy()
        {
            if ( curSceneName.Equals( "MainMenu" ) )
            {
                MessageDispatcher.PostMessage( MessageType.DisposeBundleCache );
            }
            curSceneName = null;
            ViewBase.ClearViewStack();
            this.RemoveEvent();
        }
        #endregion

        #region show common view

        private void RegisterEvent()
        {
            MessageDispatcher.AddObserver( OpenAlertWindow, MessageType.OpenAlertWindow );
            MessageDispatcher.AddObserver( OpenFreindAlert, MessageType.OpenFriendInvationAlert );
            MessageDispatcher.AddObserver( OpenLoudspeaker, MessageType.OpenLoudspeakerView );
            MessageDispatcher.AddObserver( OpenNewbieGuide, MessageType.OpenNewbieGuide );
        }

        private void RemoveEvent()
        {
            MessageDispatcher.RemoveObserver( OpenAlertWindow, MessageType.OpenAlertWindow );
            MessageDispatcher.RemoveObserver( OpenFreindAlert, MessageType.OpenFriendInvationAlert );
            MessageDispatcher.RemoveObserver( OpenLoudspeaker, MessageType.OpenLoudspeakerView );
            MessageDispatcher.RemoveObserver( OpenNewbieGuide, MessageType.OpenNewbieGuide );
        }

        private void OpenNewbieGuide( object index, object lastIndex )
        {
            if ( newbieGuideView == null )
            {
                GameResourceLoadManager.GetInstance().LoadAssetAsync( "NewbieUI", delegate ( GameObject data )
                {
                    GameObject obj = Instantiate( data ) as GameObject;
                    newbieGuideView = obj.AddComponent<NewbieGuideView>();
                    ChangeUIViewDepthBySetParent( newbieGuideView.gameObject.transform, UIMenuDepth.LayerNewbie );

                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localScale = Vector3.one;

                    RectTransform rectTrans = obj.GetComponent<RectTransform>();
                    rectTrans.sizeDelta = Vector2.zero;

                    MessageDispatcher.PostMessage( MessageType.InitNewbieGuideSucceed );

                    newbieGuideView.Open( index, lastIndex );
                } );
            }
            else
            {
                newbieGuideView.gameObject.SetActive( true );
                newbieGuideView.Open( index, lastIndex );
            }
        }

        private void OpenLoudspeaker()
        {
            if ( loudspeakerView == null )
            {
                //show LoudspeakerUI
                GameResourceLoadManager.GetInstance().LoadAssetAsync( "LoudspeakerUI", delegate ( GameObject data )
                {
                    GameObject obj = Instantiate( data ) as GameObject;
                    loudspeakerView = obj.AddComponent<LoudspeakerView>();
                    ChangeUIViewDepthBySetParent( loudspeakerView.gameObject.transform, UIMenuDepth.Loudspeaker );

                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localScale = Vector3.one;

                    RectTransform rectTrans = obj.GetComponent<RectTransform>();
                    rectTrans.sizeDelta = Vector2.zero;

                    loudspeakerView.ShowLoudspeakerPanel();
                } );
            }
            else
            {
                loudspeakerView.gameObject.SetActive( true );
                loudspeakerView.ShowLoudspeakerPanel();
            }
        }

        private void OpenFreindAlert( object freindId, object battleType, object friendName, object friendPortrait )
        {
            long id = ( long )freindId;
            BattleType type = ( BattleType )battleType;
            string name = friendName.ToString();
            string portrait = friendPortrait.ToString();

            if ( friendInvationAlertUI == null )
            {
                //show FriendInvationAlert UI ,post MessageType: OpenFriendInvationAlert
                GameResourceLoadManager.GetInstance().LoadAssetAsync( "FriendInvationAlertUI", delegate ( GameObject data )
                {
                    GameObject obj = Instantiate( data ) as GameObject;
                    friendInvationAlertUI = obj.AddComponent<FriendInvationAlertView>();
                    ChangeUIViewDepthBySetParent( friendInvationAlertUI.gameObject.transform, UIMenuDepth.Alert );

                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localScale = Vector3.one;

                    RectTransform rectTrans = obj.GetComponent<RectTransform>();
                    rectTrans.sizeDelta = Vector2.zero;

                    friendInvationAlertUI.OnEnterAlert( id, type, name, portrait );
                } );
            }
            else
            {
                friendInvationAlertUI.gameObject.SetActive( true );
                friendInvationAlertUI.OnEnterAlert( id, type, name, portrait );
            }
        }

        private void OpenAlertWindow( object confirmEvent, object type, object contentText, object titleText )
        {
            if ( alertUI == null )
            {
                //show Alert UI ,post MessageType: OpenAlertWindow
                GameResourceLoadManager.GetInstance().LoadAssetAsync<GameObject>( "AlertUI", delegate ( GameObject go )
                {
                    GameObject obj = Instantiate( go ) as GameObject;
                    alertUI = obj.AddComponent<AlertView>();
                    ChangeUIViewDepthBySetParent( alertUI.gameObject.transform, UIMenuDepth.Alert );

                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localScale = Vector3.one;

                    RectTransform rectTrans = obj.GetComponent<RectTransform>();
                    rectTrans.sizeDelta = Vector2.zero;

                    alertUI.Open( confirmEvent, type, contentText, titleText );
                } );
            }
            else
            {
                alertUI.gameObject.SetActive( true );
                alertUI.Open( confirmEvent, type, contentText, titleText );
            }
        }

        #endregion

        #region Load CallBack

        private void OnLoadedView( GameObject data, UIType type )
        {
            if ( data == null )
            {
                DebugUtils.LogError( DebugUtils.Type.UI, string.Format( " Load UI Failed! uiResource is {0}, UIType is {1} ", data != null, type) );
                return;
            }

            GameObject uiObj = Instantiate( data );
            uiObj.name = data.name;
            uiObj.SetActive( false );
            
            _activeUIDic.Add( type, AddViewBaseComponent( uiObj, type ) );

            if ( _waitingUILoadedDic.ContainsKey( type ) )
            {
                _waitingUILoadedDic[type].Invoke( _activeUIDic[type], type );
                _waitingUILoadedDic.Remove( type );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.UI, string.Format( " Can't find the UI callback! UI = {0}", type ) );
            }

            ChangeUIViewDepthBySetParent( uiObj.transform, _activeUIDic[type].uiMenuDepth );

            // reset anchor
            RectTransform rectTrans = uiObj.GetComponent<RectTransform>();
            rectTrans.sizeDelta = Vector2.zero;
        }

        private ViewBase AddViewBaseComponent( GameObject go, UIType uiType )
        {
            ViewBase value = null;
            switch ( uiType )
            {
                case UIType.Background:
                    value = go.AddComponent<MainBackgroundView>();
                    value.uiMenuDepth = UIMenuDepth.Background;
                    value.uiTransitionMode = UITransitionMode.None;
                    break;
                case UIType.MainLeftBar:
                    value = go.AddComponent<MainLeftBarView>();
                    value.uiMenuDepth = UIMenuDepth.LayerThree;
                    value.uiTransitionMode = UITransitionMode.None;
                    break;
                case UIType.MainTopBar:
                    value = go.AddComponent<MainTopBarView>();
                    value.uiMenuDepth = UIMenuDepth.LayerFour;
                    value.uiTransitionMode = UITransitionMode.None;
                    break;
                case UIType.MainBottomBar:
                    value = go.AddComponent<MainBottomBarView>();
                    value.uiMenuDepth = UIMenuDepth.LayerThree;
                    value.uiTransitionMode = UITransitionMode.None;
                    break;
                case UIType.MainMenu:
                    value = go.AddComponent<MainMenuView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
                    value.uiTransitionMode = UITransitionMode.Low;
                    break;
                case UIType.StoreScreen:
                    value = go.AddComponent<StoreView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
                    value.uiTransitionMode = UITransitionMode.Middle;
                    break;
                case UIType.StorePopUpUI:
                    value = go.AddComponent<StorePopUpView>();
                    value.uiMenuDepth = UIMenuDepth.LayerFive;
                    value.uiTransitionMode = UITransitionMode.None;
                    break;
                case UIType.PlayerInfoUI:
                    value = go.AddComponent<PlayerInfoView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
                    value.uiTransitionMode = UITransitionMode.All;
                    break;
                case UIType.ArmyManagementScreen:
                    value = go.AddComponent<ArmyManagementView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
                    value.uiTransitionMode = UITransitionMode.Middle;
                    break;
                case UIType.LoginScreen:
                    value = go.AddComponent<LoginView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
                    value.uiTransitionMode = UITransitionMode.All;
                    break;
                case UIType.BattleScreen:
                    value = go.AddComponent<BattleView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
                    value.uiTransitionMode = UITransitionMode.All;
                    break;
                case UIType.SettingScreen:
                    value = go.AddComponent<SettingView>();
                    value.uiMenuDepth = UIMenuDepth.LayerFive;
                    value.uiTransitionMode = UITransitionMode.None;
                    break;
                case UIType.FightMatchScreen:
                    value = go.AddComponent<FightMatchView>();
                    value.uiMenuDepth = UIMenuDepth.LayerTwo;
                    value.uiTransitionMode = UITransitionMode.All;
                    break;
                case UIType.BlackMarketScreen:
                    value = go.AddComponent<BlackMarketView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
                    value.uiTransitionMode = UITransitionMode.Low;
                    break;
                case UIType.BattleResultScreen:
                    value = go.AddComponent<BattleResultView>();
                    value.uiMenuDepth = UIMenuDepth.LayerTwo;
                    value.uiTransitionMode = UITransitionMode.All;
                    break;
                case UIType.RuneMainUI:
                    value = go.AddComponent<RuneMainView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
                    value.uiTransitionMode = UITransitionMode.Middle;
                    break;
				case UIType.UnitMainUI:
					value = go.AddComponent<UnitMainView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
					value.uiTransitionMode = UITransitionMode.Middle;
                    break;
				case UIType.UnitInfoUI:
					value = go.AddComponent<UnitInfoView>();
					value.uiMenuDepth = UIMenuDepth.LayerTwo;
					value.uiTransitionMode = UITransitionMode.Middle;
					break;
				case UIType.ChatMainUI:
					value = go.AddComponent<ChatMainView>();
					value.uiMenuDepth = UIMenuDepth.LayerTwo;
					value.uiTransitionMode = UITransitionMode.All;
					break;
                case UIType.PlayerBagView:
                    value = go.AddComponent<PlayerBagView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
                    value.uiTransitionMode = UITransitionMode.Middle;
                    break;
                case UIType.GainItemView:
                    value = go.AddComponent<GainItemView>();
                    value.uiMenuDepth = UIMenuDepth.LayerFive;
                    value.uiTransitionMode = UITransitionMode.None;
                    break;
				case UIType.SocialScreen:
					value = go.AddComponent<SocialView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
                    value.uiTransitionMode = UITransitionMode.Middle;
                    break;
                case UIType.MailPopUpUI:
                    value = go.AddComponent<MailPopUpView>();
                    value.uiMenuDepth = UIMenuDepth.LayerFive;
                    value.uiTransitionMode = UITransitionMode.None;
                    break;
		        case UIType.RankView:
                    value = go.AddComponent<RankView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
                    value.uiTransitionMode = UITransitionMode.Middle;
                    break;
				case UIType.BulletinBoardUI:
					value = go.AddComponent<BulletinBoardView>();
					value.uiMenuDepth = UIMenuDepth.LayerFour;
					value.uiTransitionMode = UITransitionMode.None;
					break;
                case UIType.SignView:
                    value = go.AddComponent<SignView>();
                    value.uiMenuDepth = UIMenuDepth.LayerFive;
                    value.uiTransitionMode = UITransitionMode.None;
                    break;
                case UIType.TutorialModeUI:
                    value = go.AddComponent<TutorialModeView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
                    value.uiTransitionMode = UITransitionMode.Low;
                    break;
                case UIType.PlayBackUI:
                    value = go.AddComponent<PlayBackView>();
                    value.uiMenuDepth = UIMenuDepth.LayerOne;
                    value.uiTransitionMode = UITransitionMode.All;
                    break;
                default:
                    DebugUtils.LogError( DebugUtils.Type.UI, string.Format( "UnKnow uiType! type = {0}", uiType.ToString() ) );
                    return null;
            }
            value.uiType = uiType;
            return value;
        }

        #endregion

        private void PreloadedResources()
        {
            GameResourceLoadManager.GetInstance().LoadAssetAsync<GameObject>( "UnitModelParent", delegate ( GameObject go )
            {
                DataManager.GetInstance().AddMainMenuCacheObj( go.name, go );
            } );
            UnitsProto.Unit unit = DataManager.GetInstance().unitsProtoData.Find( p => p.ID == GameConstants.PRELOADED_SHOW_UNIT_ID );
            DataManager.GetInstance().AddMainMenuCacheId( unit.Icon_bust );
            DataManager.GetInstance().AddMainMenuCacheId( unit.show_model_res );
            DataManager.GetInstance().AddMainMenuCacheId( unit.show_effect_res );
            GameResourceLoadManager.GetInstance().LoadAssetAsync<GameObject>( unit.Icon_bust );
            GameResourceLoadManager.GetInstance().LoadAssetAsync<GameObject>( unit.show_model_res );
            GameResourceLoadManager.GetInstance().LoadAssetAsync<GameObject>( unit.show_effect_res );
        }

    }
}

