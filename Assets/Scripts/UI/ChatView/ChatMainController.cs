using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Data;
using Utils;
using Network;

namespace UI
{
	public class ChatMainController : ControllerBase
	{
		private ChatMainView view;

		public long privateChatPlayerId = -1;

		private List<FriendInfo> myFriendList = new List<FriendInfo>();

		private Dictionary<long , List<ChatDataStruct>> privateChatDataDic;

		private List<ChatDataStruct> worldChatDataList;

		private Queue<ChatDataStruct> notificationQueue = new Queue<ChatDataStruct>();

		private DataManager dataManager;

		private int hornId = 10003;

		private ChatConsumptionType currnetChatConsumptionType;
		private string currentChatContent;
		private ChatType currentChatType;
		private long currentChatId;

		public ChatMainController( ChatMainView view )
		{
			this.view = view;
			viewBase = view;

			if( dataManager == null )
			{
				dataManager = DataManager.GetInstance();
			}

			worldChatDataList = dataManager.GetWorldChatDataList();
			privateChatDataDic = dataManager.GetPrivateChatDataList();

			RegisterServerMessageHandler();

			SendRelationList();
		}

        public override void OnDestroy()
        {
            base.OnDestroy();
            RemoveServerMessageHandler();
        }

        #region MsgCode

        private void RegisterServerMessageHandler()
		{
			NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.RelationListMessage, HandleRelationListFeedback );
			NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.SendChatMessage, HandleSendChatFeedback );
			NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.RelationApplicationMessage, HandleRelationApplicationFeedback );

			MessageDispatcher.AddObserver( RefreshChatData, Constants.MessageType.RefreshPlayerChatData );
			MessageDispatcher.AddObserver( RefreshHornNotificationData, Constants.MessageType.RefreshHornNotificationData );
		}

		private void RemoveServerMessageHandler()
		{
			NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.RelationListMessage, HandleRelationListFeedback );
			NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.SendChatMessage, HandleSendChatFeedback );
			NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.RelationApplicationMessage, HandleRelationApplicationFeedback );

			MessageDispatcher.RemoveObserver( RefreshChatData, Constants.MessageType.RefreshPlayerChatData );
			MessageDispatcher.RemoveObserver( RefreshHornNotificationData, Constants.MessageType.RefreshHornNotificationData );
		}

		#endregion

		#region send

		public void SendRelationList()
		{
			RelationListC2S message = new RelationListC2S();
			message.listType = 1;
			byte[] stream = ProtobufUtils.Serialize( message );
			NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.RelationListMessage, stream );
		}

		public void SendChatMessage( ChatType type, string chatContent, long playerId, ChatConsumptionType chatConsumptionType )
		{
			SendChatC2S message = new SendChatC2S();
			message.chatType = type;
			message.chatContent = chatContent;
			message.chatConsumptionType = chatConsumptionType;

			currnetChatConsumptionType = chatConsumptionType;
			currentChatType = type;
			currentChatContent = chatContent;
			currentChatId = playerId;

			if( playerId != 0 )
			{
				message.playerId = playerId;
			}

			byte[] stream = ProtobufUtils.Serialize( message );
			NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.SendChatMessage, stream );
		}

		public void SendRelationApplicationMessage( RelationApplicationType  type, long friendId )
		{
			RelationApplicationC2S message = new RelationApplicationC2S();
			message.friendId = friendId;
			message.applciationType = type;

			byte[] stream = ProtobufUtils.Serialize( message );
			NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.RelationApplicationMessage, stream );
		}

		#endregion

		#region Handle

		private void HandleRelationApplicationFeedback( byte[] data )
		{
			RelationApplicationS2C feedback = ProtobufUtils.Deserialize<RelationApplicationS2C>( data );

			if( feedback.result )
			{
				switch( feedback.applciationType )
				{
					case RelationApplicationType.ApplyingRelation:

						MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, "好友申请已经发送，请耐心等待对方确认", "提示" );

						break;

					case RelationApplicationType.BlockList:

						for ( int i = 0; i < worldChatDataList.Count; i++ )
						{
							ChatDataStruct chatData = worldChatDataList [ i ];
							if( chatData.chatPlayerInfo.playerId == feedback.friendId )
							{
								worldChatDataList.Remove( chatData );
								i--;
							}
						}

						MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, "已将该玩家拉入黑名单", "提示" );

						break;
				}
			}
		}

		private void HandleSendChatFeedback( byte[] data )
		{
			SendChatS2C feedback = ProtobufUtils.Deserialize<SendChatS2C>( data );
			if( feedback.result )
			{
				if( currnetChatConsumptionType == ChatConsumptionType.ChatItem )
				{
					view.HideHornPanel();
				}

				if(  view.currentChatTag == ChatMainView.ChatTag.PrivateChat  )
				{
					ChatDataStruct chatData = new ChatDataStruct();

					ChatPlayerInfo chatPlayerInfo = new ChatPlayerInfo();
					chatPlayerInfo.name = dataManager.GetPlayerNickName();
					chatPlayerInfo.portrait = dataManager.GetPlayerHeadIcon();
					chatPlayerInfo.level = dataManager.GetPlayerLevel();
					chatPlayerInfo.playerId = currentChatId;

					chatData.chatPlayerInfo = chatPlayerInfo;
					chatData.message = currentChatContent;
					chatData.playerId = currentChatId;

					AddPrivateChatData( currentChatId, chatData );
					view.ResfresPrivateChatItemData( GetPrivateChatData( currentChatId ) );
				}
			}
		}

		private void HandleRelationListFeedback( byte[] data )
		{
			RelationListS2C feedback = ProtobufUtils.Deserialize<RelationListS2C>( data );

			if( feedback.result )
			{
				myFriendList = feedback.friends;	
				myFriendList.Sort( SortMyFriend );
			}
		}
			
		#endregion

		#region Data

		public List<FriendInfo> GetMyFriendData()
		{
			return myFriendList;
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

		private static int SortMyFriend(FriendInfo x, FriendInfo y)
		{
			if( y.isOnline.CompareTo(x.isOnline) != 0 )
			{
				return y.isOnline.CompareTo ( x.isOnline );

			}else if( y.grade.CompareTo ( x.grade ) != 0  )
			{
				return y.grade.CompareTo ( x.grade );
			}
			else if( y.level.CompareTo ( x.level ) != 0  )
			{
				return y.level.CompareTo ( x.level );
			}
			else if( y.name.CompareTo ( x.name ) != 0  )
			{
				return y.name.CompareTo ( x.name );
			}
			return 1;
		}
			
		//this data is false data
		public List<int> GetPlayerData( int type )
		{
			List<int> playerIdList = new List<int> ();

			switch ( type )
			{
				case 1:

					for ( int i = 2; i < 3; i++ )
					{
						playerIdList.Add ( i );
					}
					 
					break;

				case 2:

					for ( int i = 2; i < 10; i++ )
					{
						playerIdList.Add ( i );
					}

					break;
			}

			return playerIdList;
		}

		public int GetHornDiamondCost()
		{
			return dataManager.itemsProtoData.Find( p => p.ID == hornId ).Price;
		}

		public int GetHornNumber()
		{
			ItemInfo itemInfo = dataManager.GetPlayerBag( BagType.ComplexBag ).itemList.Find( p => p.itemId == hornId );

			if( itemInfo == null )
			{
				return 0;
			}

			return itemInfo.count;
		}

		public int GetPlayerEmber()
		{
			return DataManager.GetInstance ().GetPlayerEmber();
		}

		public string GetPlayerName()
		{
			return DataManager.GetInstance ().GetPlayerNickName ();
		}

		public long GetPlayerId()
		{
			return dataManager.GetPlayerId();
		}

		public int GetPlayerLevel()
		{
			return dataManager.GetPlayerLevel();
		}

		public void SetPrivateChatPlayerId( long playerId )
		{
			privateChatPlayerId = playerId;

			view.EnterPrivateChat( playerId );
		}

		public long GetPrivateChatPlayerId( )
		{
			return privateChatPlayerId;
		}

		public List<ChatDataStruct> GetWorldChatDataList()
		{
			return worldChatDataList;
		}

		public bool IsFriendByPlayerId( long playerId )
		{
			for ( int i = 0; i < myFriendList.Count; i++ )
			{
				if( myFriendList[i].friendId == playerId )
				{
					return true;
				}
			} 	
			return false;
		}

		public Queue<ChatDataStruct> GetNotificationQueue()
		{
			return notificationQueue;
		}

		public int GetFriendOnLineNumber()
		{
			int number = 0;
			for ( int i = 0; i < myFriendList.Count; i++ )
			{
				if( myFriendList[i].isOnline )
				{
					number++;
				}
			}
			return number;
		}

		#endregion

		#region Observer

		private void RefreshChatData( object type, object playerId )
		{
			ChatType chatType = ( ChatType )type;
			long sendPlayerId = ( long )playerId;

			switch( chatType )
			{
				case ChatType.WorldChat:
					if( view.currentChatTag == ChatMainView.ChatTag.WorldChat )
					{
						view.ResfresChatItemData( worldChatDataList );
					}
					break;

				case ChatType.FriendsChat:
					if( view.currentChatTag == ChatMainView.ChatTag.PrivateChat && view.SelectedMyFriendItemId == sendPlayerId )
					{
						view.ResfresPrivateChatItemData( GetPrivateChatData( sendPlayerId ) );
					}
					break;

				case ChatType.GuildChat:

					break;
			}
		}

		private void RefreshHornNotificationData( object obj )
		{
			notificationQueue.Enqueue( ( ChatDataStruct )obj );
		}

		#endregion
	}
}