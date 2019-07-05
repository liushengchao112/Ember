using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Network;
using Utils;

namespace UI
{
    public class FriendInvationAlertController
    {
        private FriendInvationAlertView view;

        private List<MatcherReadyData> matcherReadyDatas;

        public FriendInvationAlertController( FriendInvationAlertView v )
        {
            view = v;

            //MessageDispatcher.AddObserver( OpenFreindAlert, Constants.MessageType.OpenFriendInvationAlert );
        }

        public void OnDestroy()
        {
            //MessageDispatcher.RemoveObserver( OpenFreindAlert, Constants.MessageType.OpenFriendInvationAlert );
        }

        public void OnEnter()
        {
            NetworkManager.RegisterServerMessageHandler( MsgCode.InvitationMatchMessage, HandleInvationMatchFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.InvitationNoticeMessage, HandleInvationNoticeFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.MatchReadyDataRefreshMessage, HandleMatchReadyDataRefreshFeedback );
        }

        public void OnExit()
        {
            NetworkManager.RemoveServerMessageHandler( MsgCode.InvitationMatchMessage, HandleInvationMatchFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.InvitationNoticeMessage, HandleInvationNoticeFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.MatchReadyDataRefreshMessage, HandleMatchReadyDataRefreshFeedback );
        }

        //private void OpenFreindAlert( object freindId, object battleType, object friendName, object friendPortrait )
        //{
        //    long id = (long)freindId;
        //    BattleType type = (BattleType)battleType;
        //    string name = friendName.ToString();
        //    string portrait = friendPortrait.ToString();
        //    view.OnEnterAlert( id, type, name, portrait );
        //}

        #region Send

        public void SendAccept( long friendId, BattleType type )
        {
            SendInvitationC2S( friendId, type, InvitationState.AcceptInvitation );
        }

        public void SendRefuse( long friendId, BattleType type )
        {
            SendInvitationC2S( friendId, type, InvitationState.RefuseInvitation );
        }

        private void SendInvitationC2S( long friendId, BattleType type, InvitationState state )
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

            InvitationMatchC2S message = new InvitationMatchC2S();

            message.friendId = friendId;
            message.battleType = type;
            message.state = state;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.InvitationMatchMessage, data );
        }

        #endregion

        #region Reponse Handle

        private void HandleMatchReadyDataRefreshFeedback( byte[] data )
        {
            MatchReadyDataRefreshS2C feedback = ProtobufUtils.Deserialize<MatchReadyDataRefreshS2C>( data );

            if ( feedback != null )
            {
                matcherReadyDatas = feedback.matcherReadyDatas;
            }
        }

        private void HandleInvationMatchFeedback( byte[] data )
        {
            UILockManager.ResetGroupState( UIEventGroup.Middle );

            InvitationMatchS2C feedback = ProtobufUtils.Deserialize<InvitationMatchS2C>( data );

            if ( feedback.result )
            {
                switch ( feedback.state )
                {
                    case InvitationState.AcceptInvitation:
                        view.OpenFightMatchView( matcherReadyDatas );
                        break;
                    case InvitationState.RefuseInvitation:
                    case InvitationState.DestroyInvitation:
                        view.CloseView();
                        break;
                    case InvitationState.FriendInBattle:
                        view.CloseView();
                        OpenPopUp( "提示", "您的好友已进入战斗" );
                        break;
                    case InvitationState.FriendInMatching:
                        view.CloseView();
                        OpenPopUp( "提示", "您的好友已在匹配中" );
                        break;
                }
            }
        }

        private void HandleInvationNoticeFeedback( byte[] data )
        {
            InvitationNoticeS2C feedback = ProtobufUtils.Deserialize<InvitationNoticeS2C>( data );

            if ( feedback.state == InvitationState.CancelInvitation )
            {
                view.CloseView();
            }
        }

        private void OpenPopUp( string title, string content )
        {
            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, content, title );
        }

        #endregion
    }
}
