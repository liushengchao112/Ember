using System.Collections;
using System.Collections.Generic;

using Network;
using Data;
using Utils;

namespace UI
{
	public class MatchChatController
	{
		private MatchChatView view;

		public MatchChatController( MatchChatView view )
		{
			this.view = view;
		}

		public void OnEnable()
		{
			NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.BattleChatsMessage, ChatFeedBack );
			NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.ForwardBattleChatsMessage, ForwardChatsFeedback );
		}

		public void OnDisable()
		{
			NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.BattleChatsMessage, ChatFeedBack );
			NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.ForwardBattleChatsMessage, ForwardChatsFeedback );
		}

		public void SendChatMessage( string str )
		{
			BattleChatC2S message = new BattleChatC2S();
			message.battleChatType = BattleChatType.match;

			//Just send 30 length string.
			if( str.Length > 30 )
			{
				str = str.Substring( 0, 30 );
			}

			message.chatContent = str;

			for( int i = 0; i < view.firendID.Count; i++ )
			{
				message.playerId.Add( view.firendID[i] ); 
			}

			byte[] stream = ProtobufUtils.Serialize( message );
			NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.BattleChatsMessage, stream );
		}
			
		public void ChatFeedBack( byte[] feedback )
		{
			BattleChatS2C data = ProtobufUtils.Deserialize<BattleChatS2C>( feedback );

			if( data.result )
			{
				DebugUtils.Log( DebugUtils.Type.Chat, "Message send complete." );
			}
		}

		public void ForwardChatsFeedback( byte[] feedback )
		{
			ForwardBattleChatS2C data = ProtobufUtils.Deserialize<ForwardBattleChatS2C>( feedback );

			if( data.battleChatType != BattleChatType.match )
			{
				DebugUtils.LogError( DebugUtils.Type.Chat, string.Format( "This message type {0} is not match chat", data.battleChatType ) );
				return;
			}
				
			if( !string.IsNullOrEmpty( data.chatContent ) )
			{
				string str = string.Format( data.sendPlayerName + " : {0}", data.chatContent );

				view.RefreshMatchCahtShowEffect( str );
			}
		}
	}
}
