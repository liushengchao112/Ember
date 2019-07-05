using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using System;
using Utils;
using Network;

namespace UI
{
    public class MailPopUpController : ControllerBase
    {
        private MailPopUpView view;

        private List<ItemBaseProto.ItemBase> itemBaseProtoData;

        public EmailInfo info;

        public MailPopUpController( MailPopUpView v )
        {
            viewBase = v;
            view = v;

            itemBaseProtoData = DataManager.GetInstance().itemsProtoData;
        }

        public override void OnResume()
        {
            base.OnResume();
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.EmailAttachmentMessage, HandleMailAttachmentS2CFeedback );
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.EmailDelMessage, HandleMailDeleteS2CFeedback );
        }

        public override void OnPause()
        {
            base.OnPause();
            NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.EmailAttachmentMessage, HandleMailAttachmentS2CFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.EmailDelMessage, HandleMailDeleteS2CFeedback );
        }

        #region Data UI

        public string GetPopUpTitle()
        {
            switch ( info.emailType )
            {
                case EmailType.SysEmail:
                    return "系统邮件";
                case EmailType.FriendEmail:
                    return "好友邮件";
            }
            return "";
        }

        public string GetMailTime()
        {
            return GetCreateTime( info.create_time );
        }

        public string GetSenderName()
        {
            return info.senderName;
        }

        public string GetSenderIcon()
        {
            return info.portrait;
        }

        private string GetCreateTime( long time )
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime( new DateTime( 1970, 1, 1 ) );
            long iTime = long.Parse( time + "0000" );
            TimeSpan toNow = new TimeSpan( iTime );
            DateTime dTime = dtStart.Add( toNow );

            return dTime.ToString( "yyyy/MM/dd \t\t HH:mm" );
        }

        public string GetMailTitle()
        {
            return info.titile;
        }

        public string GetMailConent()
        {
            return info.content;
        }

        public int GetMailGiftCount()
        {
            return info.attachments.Count;
        }

        public int GetMailGiftItemIcon( int mailGiftIndex )
        {
            switch ( info.attachments[mailGiftIndex].type )
            {
                case AttachmentType.ItemAttachment:
                    int itemId = info.attachments[mailGiftIndex].itemId;
                    return itemBaseProtoData.Find( p => p.ID == itemId ).Icon;
                case AttachmentType.DiamondAttachment:
                    //TODO:Need Diamond Icon Path
                    return 0;
                case AttachmentType.GoldAttachment:
                    //TODO:Need Gold Icon Path
                    return 0;
                case AttachmentType.EmberAttachment:
                    //TODO:Need Ember Icon Path
                    return 0;
            }
            return 0;
        }

        public string GetMailGiftItemName( int mailGiftIndex )
        {
            switch ( info.attachments[mailGiftIndex].type )
            {
                case AttachmentType.ItemAttachment:
                    int itemId = info.attachments[mailGiftIndex].itemId;
                    return itemBaseProtoData.Find( p => p.ID == itemId ).Name;
                case AttachmentType.DiamondAttachment:
                    return "钻石";
                case AttachmentType.GoldAttachment:
                    return "金币";
                case AttachmentType.EmberAttachment:
                    return "余烬币";
            }
            return "";
        }

        public int GetMailGiftItemCount( int mailGiftIndex )
        {
            return info.attachments[mailGiftIndex].quantity;
        }

        #endregion

        #region Send

        public void SendMailAttachmentC2S()
        {
            EmailAttachmentC2S message = new EmailAttachmentC2S();

            message.emailId = info.emailId;
            message.isGetAll = false;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.EmailAttachmentMessage, data );
        }

        public void SendMailDeleteC2S()
        {
            EmailDelC2S message = new EmailDelC2S();

            message.emailId = info.emailId;
            message.isDelAll = false;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.EmailDelMessage, data );
        }

        #endregion

        #region ReponseHandling

        private void HandleMailAttachmentS2CFeedback( byte[] data )
        {
            EmailAttachmentS2C feedback = ProtobufUtils.Deserialize<EmailAttachmentS2C>( data );

            if ( feedback.result )
            {
                SendMailDeleteC2S();
            }
        }

        private void HandleMailDeleteS2CFeedback( byte[] data )
        {
            EmailDelS2C feedback = ProtobufUtils.Deserialize<EmailDelS2C>( data );

            if ( feedback.result )
            {
                view.OnExit( false );
            }
        }

        #endregion
    }
}
