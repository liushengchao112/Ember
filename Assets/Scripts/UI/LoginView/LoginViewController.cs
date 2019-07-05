using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

using Network;
using Utils;
using Data;
using Constants;

namespace UI
{
    public enum LoginType { Guest, Login, Register }

    public class LoginViewController : ControllerBase
    {
        public DataManager datamanager;
        private LoginView view;
        private string userID;
        private LoginType loginType;
        private string password;

        public string gameServerIP;
        public int gameServerPort;
        public string socialServerIP;
        public int socialServerPort;

        private string avatar, nickname;

		private bool isNeedRemoveNoviceGuidanceServerMessageHandler;

        public LoginViewController( LoginView v )
        {
            viewBase = v;
            view = v;
            datamanager = DataManager.GetInstance();
        }

        public void Start()
        {
			//TODO:These three msgCode not remove.
            NetworkManager.RegisterServerMessageHandler( MsgCode.LoginMessage, HandleLoginFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.RegisterMessage, HandleRegisterLoginServerFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.RegisterChoiceMessage, HandleRegisterChoiceFeedback );
        }

        private void RegisterSocialServerMessage()
        {
			//TODO:This msgCode not remove.
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.LoginSocialServer, HandleLoginSocialServerFeedback );
        }

		public void CheckRemoveNoviceGuidanceServerMessageHandler()
		{
			if( isNeedRemoveNoviceGuidanceServerMessageHandler )
			{
				NetworkManager.RemoveServerMessageHandler( MsgCode.NoviceGuidanceMessage, HandleNoviceGuidanceFeedback );
			}

			NetworkManager.RemoveServerMessageHandler( MsgCode.CheckNoviceGuidanceMessage, HandleRegisterCheckNoviceGudanceFeedback );
		}

        #region Data UI

        public string GetRandomName()
        {
            List<RandomNameProto.RandomName> randomNameList = DataManager.GetInstance().randomNameProtoData;

            int randomFristNameID = Random.Range( 0, randomNameList.Count );
            int randomLastNameID = Random.Range( 0, randomNameList.Count );

            return string.Format( "{0}{1}", randomNameList[randomFristNameID].FirstName, randomNameList[randomLastNameID].LastName );
        }

        #endregion

        #region Send

        public void SendCredentials( LoginType type, string userID = "", string password = "" )
        {
            loginType = type;
            this.userID = userID;
            this.password = password;

            byte[] stream = null;
            MsgCode serverChannel = MsgCode.LoginMessage;

            switch ( type )
            {
                case LoginType.Guest:
                    {
                        return;
                    }
                case LoginType.Login:
                    {
                        LoginC2S loginData = new LoginC2S();

                        loginData.loginName = userID;
                        loginData.password = password;
                        loginData.UDID = DeviceUtil.Instance.GetDeviceUniqueIdentifier();
                        loginData.MAC = DeviceUtil.Instance.GetDeviceUniqueIdentifier();
                        loginData.ip = DeviceUtil.Instance.GetDeviceIP();

                        stream = ProtobufUtils.Serialize( loginData );
                        serverChannel = MsgCode.LoginMessage;

                        UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

                        break;
                    }
                case LoginType.Register:
                    {
                        RegisterC2S register = new RegisterC2S();

                        register.ip = DeviceUtil.Instance.GetDeviceIP();
                        register.MAC = DeviceUtil.Instance.GetDeviceUniqueIdentifier();
                        register.loginName = userID;
                        register.passWord = password;

                        stream = ProtobufUtils.Serialize( register );
                        serverChannel = MsgCode.RegisterMessage;

                        UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

                        break;
                    }
            }

            Utils.DebugUtils.Log( Utils.DebugUtils.Type.Login, string.Format( "SendCredentials {0} userID: {1} password: {2}", serverChannel.ToString(), userID, password ) );
            Utils.DebugUtils.Log( Utils.DebugUtils.Type.Login, string.Format( "Sending {0} as {1} ", type == LoginType.Register ? "RegisterC2S" : "LoginC2S", serverChannel.ToString() ) );
            NetworkManager.SendRequest( serverChannel, stream );
        }

        public void SendRegisterChoice( string nickname, string avatar )
        {
            RegisterChoiceC2S regC = new RegisterChoiceC2S();

            regC.accountId = datamanager.GetAccount().accountId;
            regC.avatarname = avatar;
            regC.nickname = nickname;

            byte[] stream = ProtobufUtils.Serialize( regC );

            this.avatar = avatar;
            this.nickname = nickname;

            Utils.DebugUtils.Log( Utils.DebugUtils.Type.Login, string.Format( "SendRegisterChoice nick: {0} avatar {1}", nickname, avatar ) );

            NetworkManager.SendRequest( MsgCode.RegisterChoiceMessage, stream );
        }

		public void SendLoginGameServerMessage()
        {
            LoginGameServerC2S message = new LoginGameServerC2S();

            message.accountId = datamanager.GetAccount().accountId;
            message.playerId = ClientTcpMessage.playerId;
            message.sessionId = ClientTcpMessage.sessionId;

            byte[] stream = ProtobufUtils.Serialize( message );

            Utils.DebugUtils.Log( Utils.DebugUtils.Type.Login, string.Format( "SendLoginGameServer playerid: {0} sessionid: {1}", message.playerId, ClientTcpMessage.sessionId ) );

            NetworkManager.SendRequest( MsgCode.LoginGameServerMessage, stream );
        }

        private void SendCheckNoviceGuidanceMessage()
        {
            CheckNoviceGuidanceC2S message = new CheckNoviceGuidanceC2S();

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.CheckNoviceGuidanceMessage, data );
        }

        private void SendLoginSocialGameServerMessage()
        {
            LoginSocialServerC2S message = new LoginSocialServerC2S();

            message.accountId = datamanager.GetAccount().accountId;
            message.playerId = ClientTcpMessage.playerId;
            message.sessionId = ClientTcpMessage.sessionId;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.LoginSocialServer, data );
        }

		public void SendSkipTutorialMessage()
		{
			NetworkManager.RegisterServerMessageHandler( MsgCode.NoviceGuidanceMessage, HandleNoviceGuidanceFeedback );
			isNeedRemoveNoviceGuidanceServerMessageHandler = true;

			NoviceGuidanceC2S message = new NoviceGuidanceC2S();
			message.guideStateType = ( GuideStateType )PVE.TutorialModeManager.TutorialModeStage.SkipTutorial;
			byte[] stream = ProtobufUtils.Serialize( message );
			NetworkManager.SendRequest( ServerType.GameServer, MsgCode.NoviceGuidanceMessage, stream );
		}

        #endregion Send

        #region ReponseHandling

        private void HandleLoginFeedback( byte[] data )
        {
            UILockManager.ResetGroupState( UIEventGroup.Middle );

            LoginS2C feedback = ProtobufUtils.Deserialize<LoginS2C>( data );
            if ( feedback == null )
            {
                Utils.DebugUtils.LogError( Utils.DebugUtils.Type.Login, "HandleLoginFeedback - feedback is null" );
                return;
            }

            if ( feedback.result == 1 )
            {
                Account account = datamanager.GetAccount();
                account.userID = userID;
                account.password = password;
                datamanager.SetAccountId( feedback.accountId );
                ClientTcpMessage.sessionId = feedback.loginSessionId;

                if ( feedback.playerId < 0 )
                {
                    Utils.DebugUtils.Log( Utils.DebugUtils.Type.Login, "HandleLoginFeedback - need to chose avatar and username" );
                    view.SetPlayerChoiceWindow( true );
                }
                else
                {
                    ClientTcpMessage.sessionId = feedback.loginSessionId;
                    datamanager.SetPlayerId( feedback.playerId );
                    datamanager.SetAccountId( feedback.accountId );
                    datamanager.SetGameServerIp( feedback.gameServerIp );
                    datamanager.SetGameServerPort( feedback.gameServerPort );
                    gameServerIP = feedback.gameServerIp;
                    gameServerPort = feedback.gameServerPort;
                    socialServerIP = feedback.socialServerIp;
                    socialServerPort = feedback.socialServerPort;

                    PlayerPrefs.SetString( "userID", userID );
                    PlayerPrefs.SetString( "userPW", password );

                    Utils.DebugUtils.Log( Utils.DebugUtils.Type.Login, "HandleLoginFeedback" );

                    DisconnectLoginServerAndConnectGameServer( feedback.gameServerIp, feedback.gameServerPort );
					SwtichScene();
                }
            }
            else if ( feedback.result == 2 )
            {
                datamanager.SetAccountId( feedback.accountId );
                view.OpenPlayerChoiceWindow();
            }
            else
            {
                view.FailedLogin( feedback.tipType );
            }
        }

        private void HandleRegisterLoginServerFeedback( byte[] data )
        {
            UILockManager.ResetGroupState( UIEventGroup.Middle );

            RegisterS2C feedback = ProtobufUtils.Deserialize<RegisterS2C>( data );
            if ( feedback == null )
            {
                Utils.DebugUtils.LogError( Utils.DebugUtils.Type.Login, "HandleRegisterLoginServerFeedback - feedback is null" );
                return;
            }

            if ( feedback.result > 0 )
            {
                Account account = datamanager.GetAccount();
                account.userID = userID;
                account.password = password;
                account.accountId = feedback.accountId;
                ClientTcpMessage.sessionId = feedback.loginSessionId;

                PlayerPrefs.SetString( "userID", userID );
                PlayerPrefs.SetString( "userPW", password );

                Utils.DebugUtils.Log( Utils.DebugUtils.Type.Login, "HandleRegisterLoginServerFeedback" );

                view.SetPlayerChoiceWindow( true );
            }
            else
            {
                view.FailedRegister( feedback.tipType );
            }
        }

		private bool isRegister = false;
        private void HandleRegisterChoiceFeedback( byte[] data )
        {
            RegisterChoiceS2C feedback = ProtobufUtils.Deserialize<RegisterChoiceS2C>( data );
            if ( feedback == null )
            {
                Utils.DebugUtils.LogError( Utils.DebugUtils.Type.Login, "HandleRegisterChoiceFeedback - feedback is null" );
                return;
            }

            if ( feedback.result > 0 )
            {
				isRegister = true;

                datamanager.SetPlayerHeadIcon( avatar );
                datamanager.SetPlayerNickName( nickname );
                datamanager.SetGameServerIp( feedback.gameServerIp );
                datamanager.SetGameServerPort( feedback.gameServerPort );

                ClientTcpMessage.playerId = feedback.playerId;
                ClientTcpMessage.sessionId = feedback.loginSessionId;
                Account account = datamanager.GetAccount();
                account.accountId = feedback.accountId;

                Utils.DebugUtils.Log( Utils.DebugUtils.Type.Login, "HandleRegisterChoiceFeedback" );

                gameServerIP = feedback.gameServerIp;
                gameServerPort = feedback.gameServerPort;
                socialServerIP = feedback.socialServerIp;
                socialServerPort = feedback.socialServerPort;

				DisconnectLoginServerAndConnectGameServer( feedback.gameServerIp, feedback.gameServerPort );

                view.ToggleTutorialMode();
            }
            else
            {
                view.FailedRegisterChoice( feedback.tipType );
            }
        }

        private void HandleRegisterCheckNoviceGudanceFeedback( byte[] data )
        {
            CheckNoviceGuidanceS2C feedback= ProtobufUtils.Deserialize<CheckNoviceGuidanceS2C>( data );

            if ( feedback != null )
            {
                PlayerNoviceGuidanceData guideData = new PlayerNoviceGuidanceData();
                guideData.SetBasicOperation( feedback.basicOperation );
                guideData.SetBuildTraining( feedback.buildTraining );
                guideData.SetIsSkipGuide( feedback.isSkipGuide );
                guideData.SetNpcTraining( feedback.npcTraining );
                guideData.SetSkillTraining( feedback.skillTraining );
                guideData.SetTrainingMode( feedback.trainingMode );

				datamanager.SetPlayerNoviceGuidanceData( guideData );

                handleCheckGuidance = true;

				if( !isRegister )
				{
					SwtichScene();
				}
            }
        }

		private void HandleNoviceGuidanceFeedback( byte[] data )
		{
			NoviceGuidanceS2C feedback = ProtobufUtils.Deserialize<NoviceGuidanceS2C>( data );

			if( feedback.result )
			{
				SwtichScene();
				DebugUtils.Log( DebugUtils.Type.Tutorial, "Message saveKeyValue send complete." );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Tutorial, "Tutorial finish error, Server feedBack is false, please tell server check this NoviceGuidanceS2C data." );
			}
		}

        private void HandleLoginGameServerFeedback( byte[] data )
        {
            LoginGameServerS2C feedback = ProtobufUtils.Deserialize<LoginGameServerS2C>( data );
            if ( feedback == null )
            {
                Utils.DebugUtils.LogError( Utils.DebugUtils.Type.Login, "HandleLoginGameServerFeedback - feedback is null" );
                return;
            }

			if( feedback.result )
			{
				Utils.DebugUtils.Log( Utils.DebugUtils.Type.Login, "LoginViewController - LoginGameServer - SUCCESS" );

				datamanager.ReceiveLoginFeedbackData( feedback );
				handleLoginGameServer = true;

				//This is Training interface. 
				//if ( datamanager.GetBattleType() == BattleType.Tranining )
				//{
				//    datamanager.SimulatePVEData( BattleType.Tranining );
				//    SceneManager.LoadSceneAsync( "Loading" );
				//}

				if( !isRegister )
				{
					SwtichScene();
				}
			}
        }

        private void HandleLoginSocialServerFeedback( byte[] data )
        {
            LoginSocialServerS2C feedback = ProtobufUtils.Deserialize<LoginSocialServerS2C>( data );
            if ( feedback.result )
            {
                HeartBeat.RegisterSocialHeartMessageHandler();
            }
        }

        #region Connect GameServer

        public void DisconnectLoginServerAndConnectGameServer( string ip, int port )
        {
            NetworkManager.Shutdown( () => ConnectGameServer( ip, port ) );
        }

        private void ConnectGameServer( string ip, int port )
        {
            System.Action<ClientType> connectSuccess = ( ClientType clientType ) => {
                DataManager.GetInstance().RegisterDataServerMessageHandler();
                MultiDeviceListenerManager.RegisterHandler();
                DebugToScreen.RegisterHandler();

                HeartBeat.RegisterGameHeartMessageHandler();

                NetworkManager.RegisterServerMessageHandler( MsgCode.LoginGameServerMessage, HandleLoginGameServerFeedback );
                NetworkManager.RegisterServerMessageHandler( MsgCode.CheckNoviceGuidanceMessage, HandleRegisterCheckNoviceGudanceFeedback );
                MessageDispatcher.PostMessage( MessageType.ConnectGameServer_GM );
				SendLoginGameServerMessage();
                SendCheckNoviceGuidanceMessage();

                ConnectSocialServer( socialServerIP, socialServerPort );
            };

            NetworkManager.Connect( ServerType.GameServer, ip, port, connectSuccess, OnConnectGameServerFailed );
        }

        private void OnConnectGameServerFailed( ClientType clientType )
        {
            string connectGameServerFailed = "连接GameServer失败，请重试" + clientType;
            string titleText = "提示";
            System.Action reconnect = ReconnectGameServer;

            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, reconnect, UI.AlertType.ConfirmAlone, connectGameServerFailed, titleText );
        }

        private void ReconnectGameServer()
        {
            DisconnectLoginServerAndConnectGameServer( DataManager.GetInstance().GetGameServerIp(), DataManager.GetInstance().GetGameServerPort() );
        }

        #endregion

        #region Connect SocialServer

        private void ConnectSocialServer( string ip, int port )
        {
            System.Action<ClientType> connectSuccess = ( ClientType clientType ) => {
                SendLoginSocialGameServerMessage();
                datamanager.RegisterDataSocialServerMessageHandler();
                RegisterSocialServerMessage();
            };

            NetworkManager.Connect( ServerType.SocialServer, ip, port, connectSuccess, OnConentSocialServerFailed );
        }

        private void OnConentSocialServerFailed( ClientType clientType )
        {
            string connectSocialServerFailed = "连接SocialServer失败，请重试" + clientType;
            string titleText = "提示";
            System.Action reconnect = ReconnectSocialServer;

            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, reconnect, UI.AlertType.ConfirmAlone, connectSocialServerFailed, titleText );
        }

        private void ReconnectSocialServer()
        {
            ConnectSocialServer( socialServerIP, socialServerPort );
        }

        #endregion

        #region Swtich Scene

        private bool handleLoginGameServer = false;
        private bool handleCheckGuidance = false;

		public void SwtichScene()
        {
            if ( handleLoginGameServer && handleCheckGuidance )
            {
				int isSkipTutorial = datamanager.GetPlayerNoviceGuidanceData().GetIsSkipGuide();
				int isFinishedTutorialFirstStage = datamanager.GetPlayerNoviceGuidanceData().GetBasicOperation();
				if ( datamanager.GetBattleType() == BattleType.Tutorial || ( isSkipTutorial == 0 && isFinishedTutorialFirstStage == 0 ) )
                {
                    #region There will create tutorial data and in tutorial battle.
					datamanager.SetBattleType( BattleType.Tutorial, false );
					datamanager.SetTutorialStage( PVE.TutorialModeManager.TutorialModeStage.NormallyControlOperation_Stage );
                    datamanager.SimulatePVEData( BattleType.Tutorial );

                    datamanager.ResetMatchers();

                    Matcher robot = new Matcher();
                    robot.playerId = 0;     //AI's ID must be consistent with entering the battle
                    robot.name = "怠惰的教官";
                    robot.side = MatchSide.Red;
                    robot.portrait = "EmberAvatar_10";

                    Matcher myself = new Matcher();
                    myself.playerId = datamanager.GetPlayerId();
                    myself.name = datamanager.GetPlayerNickName();
                    myself.side = MatchSide.Blue;
                    myself.portrait = datamanager.GetPlayerHeadIcon();

                    datamanager.SetMatcher( robot );
                    datamanager.SetMatcher( myself );
                    SceneManager.LoadSceneAsync( "Loading" );

                    #endregion
                }
                else
                {
                  	UIManager.Instance.EnterMainMenu();

                    DataManager clientData = DataManager.GetInstance();

                    UIManager.Instance.GetUIByType( UIType.SettingScreen, ( ViewBase ui, System.Object param ) => {
                        ui.OnInit();
                        clientData.ReadSettingDataFromPlayerPrefs();
                        ( ui as SettingView ).SetToggleUI();
                    } );
                    if ( clientData.GetIsFirstLogin() && !clientData.GetIsSignToday() )
                    {
                        UIManager.Instance.GetUIByType( UIType.SignView, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); } );
                        clientData.SetFirstLoginOver();
                    }
                }
            }
        }

        #endregion

        #endregion ResponseHandling 
    }
}
