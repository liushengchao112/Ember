using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using Utils;
using Network;

namespace UI
{
    public class MailController
    {
        private MailView view;
        private List<EmailInfo> emailInfoList = new List<EmailInfo>();

        private MailItem currentMailItem;
        private EmailInfo currentMailInfo;

        public MailController( MailView v )
        {
            view = v;
        }

        public void OnResume()
        {
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.EmailMessage, HandleMailS2CFeedback );
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.EmailAttachmentMessage, HandleMailGetAllS2CFeedback );
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.EmailDelMessage, HandleMailDelAllS2CFeedback );
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.EmailReadMessage, HandleMailReadS2CFeedback );
            MessageDispatcher.AddObserver( OpenGainItemWindow, Constants.MessageType.OpenGainItemWindow );
        }

        public void OnPause()
        {
            NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.EmailMessage, HandleMailS2CFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.EmailAttachmentMessage, HandleMailGetAllS2CFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.EmailDelMessage, HandleMailDelAllS2CFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.EmailReadMessage, HandleMailReadS2CFeedback );
            MessageDispatcher.RemoveObserver( OpenGainItemWindow, Constants.MessageType.OpenGainItemWindow );
        }
        
        private void OpenGainItemWindow( object exps, object currencies, object items, object soldierInfos )
        {
            int exp = (int)exps;
            List<Currency> currency = currencies as List<Currency>;
            List<ItemInfo> item = items as List<ItemInfo>;
            List<SoldierInfo> soldier = soldierInfos as List<SoldierInfo>;

            UI.UIManager.Instance.GetUIByType( UI.UIType.GainItemView, ( UI.ViewBase ui, System.Object obj ) => { ( ui as GainItemView ).OpenGainItemView( exp, currency, item, soldier ); } );
        }

        #region Data UI

        public List<EmailInfo> GetMailItemsList()
        {
            return emailInfoList;
        }

        public bool HaveUnread()
        {
            if ( DataManager.GetInstance().GetRedBubbleNum( CaptionType.EmailCaption ) > 0 )
                return true;
            return false;
        }

        private bool HaveGift()
        {
            if ( emailInfoList.Count <= 0 )
                return false;
            for ( int i = 0; i < emailInfoList.Count; i++ )
            {
                if ( emailInfoList[i].is_get == false )
                    return true;
            }
            return false;
        }

        #endregion

        #region Send

        public void SendMailC2S()
        {
            EmailC2S message = new EmailC2S();
            message.emailType = EmailType.FriendEmail;
            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.EmailMessage, data );
        }

        public void SendMailGetAllC2S()
        {
            if ( !HaveGift() )
                return;
            currentMailInfo = null;

            EmailAttachmentC2S message = new EmailAttachmentC2S();

            message.isGetAll = true;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.EmailAttachmentMessage, data );
        }

        public void SendMailDeleteAllC2S()
        {
            currentMailInfo = null;
            EmailDelC2S message = new EmailDelC2S();

            message.isDelAll = true;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.EmailDelMessage, data );
        }

        public void SendMailReadC2S( long emailId, MailItem item, EmailInfo info )
        {
            currentMailItem = item;
            currentMailInfo = info;

            ReadEmailC2S message = new ReadEmailC2S();

            message.emailId = emailId;
            message.emailType = EmailType.FriendEmail;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.EmailReadMessage, data );
        }

        #endregion

        #region ReponseHandling

        private void HandleMailS2CFeedback( byte[] data )
        {
            EmailS2C feedback = ProtobufUtils.Deserialize<EmailS2C>( data );

            if ( feedback.result )
            {
                emailInfoList = feedback.emails;

                view.RefreshMailPanel( true );
            }
        }

        private void HandleMailGetAllS2CFeedback( byte[] data )
        {
            EmailAttachmentS2C feedback = ProtobufUtils.Deserialize<EmailAttachmentS2C>( data );

            if ( feedback.result )
            {
                if ( currentMailInfo == null )
                {
                    for ( int i = 0; i < emailInfoList.Count; i++ )
                    {
                        emailInfoList[i].is_read = true;
                        emailInfoList[i].is_get = true;
                    }
                }
                
                DataManager.GetInstance().SetRedBubbleNum( CaptionType.EmailCaption, 0 );

                view.RefreshMailPanel();
            }
        }

        private void HandleMailDelAllS2CFeedback( byte[] data )
        {
            EmailDelS2C feedback = ProtobufUtils.Deserialize<EmailDelS2C>( data );

            if ( feedback.result )
            {
                if ( currentMailInfo == null )
                    emailInfoList.Clear();
                else
                    emailInfoList.Remove( currentMailInfo );

                view.RefreshMailPanel();
            }
        }

        private void HandleMailReadS2CFeedback( byte[] data )
        {
            ReadEmailS2C feedback = ProtobufUtils.Deserialize<ReadEmailS2C>( data );

            if ( feedback.result )
            {
                currentMailInfo.is_read = true;
                currentMailItem.RefreshItem();

                DataManager dataManager = DataManager.GetInstance();
                int oldNum = dataManager.GetRedBubbleNum( CaptionType.EmailCaption );
                dataManager.SetRedBubbleNum( CaptionType.EmailCaption, oldNum - 1 );

                view.RefreshMailPanel();
            }
        }

        #endregion
    }
}
