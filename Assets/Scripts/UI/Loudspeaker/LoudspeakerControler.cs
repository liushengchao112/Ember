using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Data;
using Network;

namespace UI
{
    public class LoudspeakerControler
    {
        private LoudspeakerView view;

        private DataManager dataManager;

        private readonly int hornId = 10003;

        public LoudspeakerControler( LoudspeakerView v )
        {
            view = v;            

            dataManager = DataManager.GetInstance();

            //MessageDispatcher.AddObserver( OpenLoudspeaker, Constants.MessageType.OpenLoudspeakerView );
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer , MsgCode.SendChatMessage , HandleSendChatFeedback );            
        }

        public void OnDestroy()
        {            
            //MessageDispatcher.RemoveObserver( OpenLoudspeaker , Constants.MessageType.OpenLoudspeakerView );
            NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer , MsgCode.SendChatMessage , HandleSendChatFeedback );            
        }

        //private void OpenLoudspeaker()
        //{
        //    view.ShowLoudspeakerPanel();
        //}

        public int GetLoudspeakerCount()
        {
            ItemInfo itemInfo = dataManager.GetPlayerBag( BagType.ComplexBag ).itemList.Find( p => p.itemId == hornId );

            if ( itemInfo == null )
            {
                return 1;
            }

            return itemInfo.count;
        }

        public long GetPlayerId()
        {
            return dataManager.GetPlayerId();
        }

        public void SendChatMessage( ChatType type , string chatContent , long playerId , ChatConsumptionType chatConsumptionType )
        {
            SendChatC2S message = new SendChatC2S();
            message.chatType = type;
            message.chatContent = chatContent;
            message.chatConsumptionType = chatConsumptionType;

            if ( playerId != 0 )
            {
                message.playerId = playerId;
            }

            byte[] stream = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( ServerType.SocialServer , MsgCode.SendChatMessage , stream );
        }

        private void HandleSendChatFeedback( byte[] data )
        {
            SendChatS2C feedback = ProtobufUtils.Deserialize<SendChatS2C>( data );
            if ( feedback.result )
            {
                MessageDispatcher.PostMessage( Constants.MessageType.RefreshBagView );
                view.ExitButtonEvent();
            }
        }
    }
}