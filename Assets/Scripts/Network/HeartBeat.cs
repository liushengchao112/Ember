using System.Collections;

using Data;
using Constants;
using Utils;

namespace Network
{
    public class HeartBeat
    {
        private static float pingTime = Constants.GameConstants.PING_TIME;
        private static float pingSocialServerTime = Constants.GameConstants.PING_TIME;

        private static bool isPause = false;

        public static bool isDisconnect;
        private static bool connectSocialServer = false;

        public static void Init()
        {
            //Server Disconnect 
            AsyncTcpClient.AccidentDisconnectCallback = DisconnectedToServer;
        }

        public static void Update( float deltaTime )
        {
            ServerType type = NetworkManager.GetCurrentServerType();
            if ( ( type == ServerType.GameServer || type == ServerType.LobbyServer ) & NetworkManager.IsCurrentClientConnected() )
            {
                pingTime -= deltaTime;
                if ( pingTime <= 0 )
                {
                    pingTime = GameConstants.PING_TIME;

                    PingServer( type );
                }
            }
            if ( connectSocialServer )
            {
                if ( NetworkManager.IsClientConnected( ServerType.SocialServer ) )
                {
                    pingSocialServerTime -= deltaTime;
                    if ( pingSocialServerTime <= 0 )
                    {
                        pingSocialServerTime = GameConstants.PING_TIME;

                        PingServer( ServerType.SocialServer );
                    }
                }
            }
        }

        public static void OnApplicationPause()
        {
            isPause = !isPause;
            if ( isPause )
            {

            }
            else
            {
                ServerType type = NetworkManager.GetCurrentServerType();
                if ( type == Data.ServerType.GameServer || type == Data.ServerType.LobbyServer )
                {
                    HeartBeat.PingServer( type );
                    pingTime = GameConstants.PING_TIME;
                }
            }

            MessageDispatcher.PostMessage( MessageType.GamePause, isPause );
        }

        public static void RegisterLoginHeartMessageHandler()
        {
            //NetworkManager.RegisterServerMessageHandler( MsgCode.PingLobbyMessage, HandlePingLobbyFeedback );
        }
        
        public static void RegisterGameHeartMessageHandler()
        {
            NetworkManager.RegisterServerMessageHandler( MsgCode.PingGameMessage, HandlePingGameFeedback );

            NetworkManager.RegisterServerMessageHandler( MsgCode.ReconnectGameMessage, HandleReconnectGameServerFeedback );
        }

        public static void RegisterLobbyHeartMessageHandler()
        {
            NetworkManager.RegisterServerMessageHandler( MsgCode.PingLobbyMessage, HandlePingLobbyFeedback );
        }

        public static void RegisterBattleHeartMessageHandler()
        {
            //NetworkManager.RegisterServerMessageHandler( MsgCode.PingLobbyMessage, HandlePingLobbyFeedback );
        }

        public static void RegisterSocialHeartMessageHandler()
        {
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.PingSocialServer, HandlePingSocialFeedback );
            connectSocialServer = true;
        }

        #region Send To Server

        public static void PingServer( ServerType type )
        {
            if ( NetworkManager.IsClientConnected( type ) )
            {
                if ( type == ServerType.GameServer )
                {
                    PingGameC2S message = new PingGameC2S();
                    byte[] data = ProtobufUtils.Serialize( message );
                    NetworkManager.SendRequest( type, MsgCode.PingGameMessage, data );
                }
                else if ( type == ServerType.LobbyServer )
                {
                    PingLobbyC2S message = new PingLobbyC2S();
                    byte[] data = ProtobufUtils.Serialize( message );
                    NetworkManager.SendRequest( type, MsgCode.PingLobbyMessage, data );
                }
                else if ( type == ServerType.SocialServer )
                {
                    PingSocialC2S message = new PingSocialC2S();
                    byte[] data = ProtobufUtils.Serialize( message );
                    NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.PingSocialServer, data );
                }
                else
                {

                }
            }
        }

        #endregion

        #region Handle Feedback

        private static void HandlePingGameFeedback( byte[] data )
        {
            PingGameS2C feedback = ProtobufUtils.Deserialize<PingGameS2C>( data );

            if ( feedback.result )
            {
                isDisconnect = false;
            }
        }

        private static void HandlePingLobbyFeedback( byte[] data )
        {
            PingLobbyS2C feedback = ProtobufUtils.Deserialize<PingLobbyS2C>( data );

            if ( feedback.result )
            {
                if ( isDisconnect )
                {
                    isDisconnect = false;
                    MatchC2S message = new MatchC2S();

                    BattleType bType = DataManager.GetInstance().GetBattleType();
                    if ( bType == BattleType.BattleP1vsP1 )
                        message.matchType = MatchType.P1vsP1;
                    if ( bType == BattleType.BattleP2vsP2 )
                        message.matchType = MatchType.P2vsP2;
                    else
                        message.matchType = MatchType.Peace;
                    message.playerName = DataManager.GetInstance().GetPlayerNickName();
                    message.type = MatchClientMessageType.Applying;

                    byte[] data1 = ProtobufUtils.Serialize( message );

                    NetworkManager.SendRequest( MsgCode.MatchMessage, data1 );
                }
            }
        }

        private static void HandlePingSocialFeedback( byte[] data )
        {
            PingSocialS2C feedback = ProtobufUtils.Deserialize<PingSocialS2C>( data );

            if ( feedback.result )
            {

            }
        }

        private static void HandleReconnectGameServerFeedback( byte[] data )
        {
            ReconnectGameS2C feedback = ProtobufUtils.Deserialize<ReconnectGameS2C>( data );

            if ( feedback.requestParam == 1 )//need refresh data
            {
                DataManager.GetInstance().ReconnectGameServerFeedbackData( feedback );
            }
            else if ( feedback.requestParam == 0 )
            {

            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.HeartBeatPing, "RequestParam is wrong,it is ==>" + feedback.requestParam );
            }
        }

        #endregion Handle Feedback 

        #region Server Discounnect

        private static void DisconnectedToServer( ServerType type )
        {
            if ( type == ServerType.SocialServer )
            {
                ReconnectServer( ServerType.SocialServer );
                return;
            }
             isDisconnect = true;

            string disconnectText = "网络异常，请重试 ";
            string titleText = "提示";
            System.Action reconnect = () => ReconnectServer( type );

            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, reconnect, UI.AlertType.ConfirmAlone, disconnectText, titleText );
        }

        private static void ReconnectServer( ServerType type )
        {
            NetworkManager.Reconnect( type, ClientType.Tcp );
        }

        #endregion
    }
}
