using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using Resource;
using Utils;

namespace UI
{
	public class BlackListItem : ScrollViewItemBase
	{
		#region BlaickListItem values

		public Action<long> deleteBlackListPlayerEvent;
		public Action<long> addPlayerInFriendListEvent;

		private Action action;

		private bool isInited;

		private GameResourceLoadManager gameResourceLoadManager; 

		//private const string PLAYER_ICON_PATH = "UITexture/Avatar_icon/";

		private long playerUID;

		private string playerInfoButtonIconName;
		private string playerName;
		private string playerNetworkStatus;

		private int playerLevel;
		private int playerVipIconID;
		private int playerRankIconID;

		private string noticeTitle = "提示";
		private string deleteBlackListPlayerStr = "是否解除对该玩家的屏蔽？";
		private string addInFriendListStr = "是否添加该玩家为好友？";

		#endregion 

		#region Component

		private Button playerInfoButton;
		private Button addInFriendButton;
		private Button deleteBlackListPlayerButton;

		private Text playerNameText;
		private Text playerLevelText;
		private Text playerNetworkStatusText;

		private Image playerVipIcon;
		private Image playerRankIcon;

		#endregion

		#region BlackListItem default functions

		private void Initialize()
		{
			isInited = true;

			gameResourceLoadManager = GameResourceLoadManager.GetInstance();

			addInFriendButton = transform.Find( "FriendButton" ).GetComponent<Button>();
			addInFriendButton.onClick.AddListener( OnAddPlayerInFriendListButtonClicked );

			deleteBlackListPlayerButton = transform.Find( "RemoveButton" ).GetComponent<Button>();
			deleteBlackListPlayerButton.onClick.AddListener( OnDeleteBlackListPlayerButtonClicked );

			playerInfoButton = transform.Find( "PlayerInfoButton" ).GetComponent<Button>();

			playerNameText = transform.Find( "PlayerNameText" ).GetComponent<Text>();
			playerLevelText = transform.Find( "PlayerInfoButton/Text" ).GetComponent<Text>();
			playerNetworkStatusText = transform.Find( "NetworkStatusText" ).GetComponent<Text>();

			playerVipIcon = transform.Find( "VipIcon" ).GetComponent<Image>();
			playerRankIcon = transform.Find( "RankIcon" ).GetComponent<Image>();
		}

		public override void UpdateItemData( object dataObj )
		{
			base.UpdateItemData( dataObj );

			if( dataObj == null )
			{
				return;
			}

			SetData( ( FriendListDataStruct ) dataObj );
			Show();
		}

		public void SetData( FriendListDataStruct data )
		{
			if( !isInited )
			{
				Initialize();
			}

			this.playerUID = data.playerUID;

			this.playerInfoButtonIconName = data.portrait;
			this.playerName = data.playerName;
			this.playerLevel = data.playerLevel;
			this.playerRankIconID = data.grade;
			this.playerNetworkStatus = data.playerNetWorkStatus;
		}

		public void Show()
		{
			if( !this.gameObject.activeInHierarchy )
			{
				this.gameObject.SetActive( true );
			}

			if ( !string.IsNullOrEmpty( playerInfoButtonIconName ) )
			{
				gameResourceLoadManager.LoadAtlasSprite( playerInfoButtonIconName, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
				{
					playerInfoButton.image.SetSprite( atlasSprite );
				}, true );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.UI_SocialScreen, string.Format( "PlayerInfoButtonIconName or PLAYER_ICON_PATH is null, Check that." ) );
			}

			/*if ( playerVipIconID != 0 )
			{
				GameResourceLoadManager.GetInstance().LoadAtlasSprite( playerVipIconID, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
				{
					playerVipIcon.SetSprite( atlasSprite );
				}, true );
			}

			if ( playerRankIconID != 0 )
			{
				GameResourceLoadManager.GetInstance().LoadAtlasSprite( playerRankIconID, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
				{
					playerRankIcon.SetSprite( atlasSprite );
				}, true );
			}*/

			playerNameText.text = playerName;
			playerLevelText.text = playerLevel.ToString();
			playerNetworkStatusText.text = playerNetworkStatus;
		}

		#endregion

		#region BlackListItem button functions

		//Empty function waiting confirm form designer.
		private void OnAddPlayerInFriendListButtonClicked()
		{
			//action = SendDeleteBlackListPlayerMessage;
			MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, addPlayerInFriendListEvent, UI.AlertType.ConfirmAndCancel, addInFriendListStr, noticeTitle );

			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The OnAddPlayerInFriendListButtonClicked." ) );
		}

		//Empty function waiting confirm form designer.
		private void SendAddPlayerInFriendListMessage()
		{
			if( addPlayerInFriendListEvent != null )
			{
				addPlayerInFriendListEvent( playerUID );
			}
				
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The SendAddPlayerInFriendListMessage." ) );
		}

		private void OnDeleteBlackListPlayerButtonClicked()
		{
			action = SendDeleteBlackListPlayerMessage;
			MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, action, UI.AlertType.ConfirmAndCancel, deleteBlackListPlayerStr, noticeTitle );

			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The OnDeleteFriendButtonClicked." ) );
		}

		private void SendDeleteBlackListPlayerMessage()
		{
			if( deleteBlackListPlayerEvent != null )
			{
				deleteBlackListPlayerEvent( playerUID );
			}
						
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The SendDeleteFriendMessage function used." ) );
		}

		#endregion
	}
}
