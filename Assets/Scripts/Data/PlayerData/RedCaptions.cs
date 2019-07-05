using System.Collections.Generic;

using Network;
using Utils;

namespace Data
{
    public class RedCaption
    {
        public CaptionType type;
        public int captionNum;

        public RedCaption( CaptionType type, int captionNum )
        {
            this.type = type;
            this.captionNum = captionNum;
        }
    }

    public class RedCaptions
    {
        public List<RedCaption> captions;

        public RedCaptions()
        {
            captions = new List<Data.RedCaption>();

            captions.Add( new Data.RedCaption( CaptionType.EmailCaption, 0 ) );
            captions.Add( new Data.RedCaption( CaptionType.RelationApplicationCaption, 0 ) );
        }

        public RedCaption GetRedCaption( CaptionType type )
        {
            return captions.Find( p => p.type == type );
        }

        public void RegisterRedCaptionMessageHandler()
        {
            NetworkManager.RegisterServerMessageHandler( MsgCode.CaptionMessage, HandleCaptionFeedback );
        }

        public void RegisterRedCaptionSocialMessageHandler()
        {
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.CaptionMessage, HandleCaptionSocialFeedback );
        }

        public void HandleCaptionFeedback( byte[] data )
        {

            MessageDispatcher.PostMessage( Constants.MessageType.RefreshGameServerRedBubble );
        }

        public void HandleCaptionSocialFeedback( byte[] data )
        {
            CaptionS2C feedback = ProtobufUtils.Deserialize<CaptionS2C>( data );

            if ( feedback.captions.Find( p => p.captionType == CaptionType.EmailCaption ) != null )
            {
                RedCaption emailCaption = captions.Find( p => p.type == CaptionType.EmailCaption );
                emailCaption.captionNum = feedback.captions.Find( p => p.captionType == CaptionType.EmailCaption ).captionNum;
            }

            if ( feedback.captions.Find( p => p.captionType == CaptionType.RelationApplicationCaption ) != null )
            {
                RedCaption relationApplicationCaption = captions.Find( p => p.type == CaptionType.RelationApplicationCaption );
                relationApplicationCaption.captionNum = feedback.captions.Find( p => p.captionType == CaptionType.RelationApplicationCaption ).captionNum;
            }

            MessageDispatcher.PostMessage( Constants.MessageType.RefreshSocialServerRedBubble );
        }
    }
}
