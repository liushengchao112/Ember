using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SignInfo = Data.DailyLoginS2C.SignInfo;
using Network;
using Utils;

namespace Data
{
    public class PlayerDailySign
    {
        private bool isFirstLogin;
        private bool isSign = true;
        private List<SignInfo> signInfos;

        public PlayerDailySign()
        {
            isFirstLogin = true;
            signInfos = new List<SignInfo>();
        }

        public bool GetIsFirstLogin()
        {
            return isFirstLogin;
        }

        public void SetFirstLoginOver()
        {
            isFirstLogin = false;
        }

        public bool GetIsDailySign()
        {
            return isSign;
        }

        private void SetSignInfos( List<SignInfo> info )
        {
            signInfos.Clear();
            for ( int i = 0; i < info.Count; i++ )
            {
                signInfos.Add( info[i] );
            }            
        }

        public List<SignInfo> GetSignInfo()
        {
            return signInfos;
        }

        public void RegisterPlayerDailySign()
        {
            NetworkManager.RegisterServerMessageHandler( ServerType.GameServer , MsgCode.DailyLoginMessage , HandlePlayerDailySignFeedback );
        }

        public void RemovePlayerDailySign()
        {
            NetworkManager.RemoveServerMessageHandler( ServerType.GameServer , MsgCode.DailyLoginMessage , HandlePlayerDailySignFeedback );
        }

        private void HandlePlayerDailySignFeedback( byte[] data )
        {
            DailyLoginS2C feedback = ProtobufUtils.Deserialize<DailyLoginS2C>( data );

            if ( feedback == null )
            {
                DebugUtils.LogError( DebugUtils.Type.UI , "DailyLoginS2C~~~~Feedback is null" );
                return;
            }

            if ( feedback.result )
            {
                isSign = feedback.isSign;
                if ( feedback.singInfos.Count > 0 )
                {
                    SetSignInfos( feedback.singInfos );
                }                                            

                MessageDispatcher.PostMessage( Constants.MessageType.RefreshSignView , isSign );
            }           
        }
    }
}
