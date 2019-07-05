using UnityEngine;
using System.Collections;

using Network;
using Utils;
using System.Collections.Generic;

namespace Data
{
	
	public struct ChatDataStruct
	{
		public ChatPlayerInfo chatPlayerInfo;
		public long playerId;
		public string message;
		public bool isLeft;
	}

	public class PlayerChatMessages
	{

		public Dictionary<long , List<ChatDataStruct>> privateChatDataDic;

		public List<ChatDataStruct> worldChatDataList;

		public PlayerChatMessages()
		{
			privateChatDataDic = new Dictionary<long, List<ChatDataStruct>>();
			worldChatDataList = new List<ChatDataStruct>();
		}

		public void RegisterForwardChatSocialServerMessageHandler()
		{
			NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.ForwardChatsMessage, HandForwardChatFeedback );
		}

		#region Handle bags data feedback

		private void HandForwardChatFeedback( byte[] data )
		{
			ForwardChatS2C feedback = ProtobufUtils.Deserialize<ForwardChatS2C>( data );

			ChatPlayerInfo chatPlayerInfo = feedback.chatPlayerInfo;
			ChatDataStruct chatData = new ChatDataStruct();
			chatData.chatPlayerInfo = chatPlayerInfo;
			chatData.message = feedback.chatContent;
			chatData.playerId = feedback.sendPlayerId;

			if(  DataManager.GetInstance().GetPlayerId() != feedback.sendPlayerId )
			{
				chatData.isLeft = true;
			}
			else
			{
				chatData.isLeft = false;
			}

			if( feedback.chatConsumptionType == ChatConsumptionType.ChatItem )
			{
				MessageDispatcher.PostMessage( Constants.MessageType.RefreshHornNotificationData, chatData );
				return;
			}

			switch( feedback.chatType )
			{
				case ChatType.WorldChat:

					worldChatDataList.Add( chatData );

					break;

				case ChatType.FriendsChat:

					AddPrivateChatData( feedback.sendPlayerId, chatData );

					break;

				case ChatType.GuildChat:

					break;
			}

			MessageDispatcher.PostMessage( Constants.MessageType.RefreshPlayerChatData, feedback.chatType, feedback.sendPlayerId );
		}

		#endregion

		#region Data

		public void AddPrivateChatData( long playerId,  ChatDataStruct chatData )
		{
			if( privateChatDataDic.ContainsKey ( playerId ) )
			{
				privateChatDataDic[ playerId ].Add ( chatData );
			}
			else
			{
				privateChatDataDic.Add ( playerId , new List<ChatDataStruct> (){ chatData } );
			}
		}

		public List<ChatDataStruct> GetPrivateChatData( long playerId )
		{
			List<ChatDataStruct> dataList;

			if( privateChatDataDic.ContainsKey ( playerId ) )
			{
				dataList = privateChatDataDic[ playerId ];
			}
			else
			{
				dataList = new List<ChatDataStruct> ();
				privateChatDataDic.Add ( playerId , dataList );
			}
			return dataList;
		}

		#endregion
	}
}