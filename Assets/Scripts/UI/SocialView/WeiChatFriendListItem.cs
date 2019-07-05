using System.Collections;
using System.Collections.Generic;
using System;

using Utils;
using UnityEngine.UI;
using Resource;
using UnityEngine;

namespace UI
{
	public class WeiChatFriendListItem : ScrollViewItemBase
	{
		#region WeiChatFriendListItem values

		private Action deleteFriendEvent;

		private bool isInited;

		private GameResourceLoadManager gameResourceLoadManager;

		private const string PLAYER_ICON_PATH = "UITexture/Avatar_icon/";

		private long playerUID;

		private string playerInfoButtonIconName;
		private string playerName;
		private string playerNetworkStatus;

		private int playerLevel;
		private int playerVipIconID;
		private int playerRankIconID;

		//Not confirm picture load mode, temp keep this code.
		//private int inviteGuildButtonIconID;
		//private int chartButtonIconID;
		//private int giveMoneyButtonIconID;
		//private int giftButtonIconID;
		//private int deleteFriendButtonIconID;

		private bool isJoinedGuild;

		private string noticeTitle = "提示";
		private string deleteFriendStr = "是否删除好友?";

		#endregion

		#region Component

		private Button playerInfoButton;
		private Button invitePlayerInGuildButton;
		private Button chatButton;
		private Button giveMoneyButton;
		private Button giftButton;
		private Button deleteFriendButton;

		private Image playerVipIcon;
		private Image playerRankIcon;

		private Text playerNameText;
		private Text playerLevelText;
		private Text playerNetworkStatusText;

		#endregion

		#region WeiChatFriendListItem default functions;

		private void Initialize()
		{
			isInited = true;

			playerInfoButton = transform.Find( "PlayerInfoButton" ).GetComponent<Button>();
			playerInfoButton.onClick.AddListener( OnPlayerInfoButtonClicked );

			invitePlayerInGuildButton = transform.Find( "GuildButton" ).GetComponent<Button>();
			invitePlayerInGuildButton.onClick.AddListener( OnInvitePlayerInGuildButtonClicked );

			chatButton = transform.Find( "ChatButton" ).GetComponent<Button>();
			chatButton.onClick.AddListener( OnChatButtonClicked );

			giveMoneyButton = transform.Find( "MoneyButton" ).GetComponent<Button>();
			giveMoneyButton.onClick.AddListener( OnGiveMoenyButtonClicked );

			giftButton = transform.Find( "GiftButton" ).GetComponent<Button>();
			giftButton.onClick.AddListener( OnGiftButtonClicked );

			deleteFriendButton = transform.Find( "DeleteButton" ).GetComponent<Button>();
			deleteFriendButton.onClick.AddListener( OnDeleteFriendButtonClicked );

			playerVipIcon = transform.Find( "VipIcon" ).GetComponent<Image>();
			playerRankIcon = transform.Find( "RankIcon" ).GetComponent<Image>();

			playerNameText = transform.Find( "PlayerNameText" ).GetComponent<Text>();
			playerLevelText = transform.Find( "PlayerInfoButton/Text" ).GetComponent<Text>();
			playerNetworkStatusText = transform.Find( "NetworkStatusText" ).GetComponent<Text>();

			gameResourceLoadManager = GameResourceLoadManager.GetInstance();
		}
			
		public void Show()
		{
			if( !this.gameObject.activeInHierarchy )
			{
				this.gameObject.SetActive( true );
			}

			if ( !string.IsNullOrEmpty( playerInfoButtonIconName ) )
			{
				gameResourceLoadManager.LoadAtlasSprite( PLAYER_ICON_PATH + playerInfoButtonIconName, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
				{
					playerInfoButton.image.SetSprite( atlasSprite );
				}, true );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.UI_SocialScreen, string.Format( "PlayerInfoButtonIconName or playerInfoIconPath is null, Check that." ) );
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

			if( isJoinedGuild )
			{
				if( invitePlayerInGuildButton.gameObject.activeInHierarchy )
				{
					invitePlayerInGuildButton.gameObject.SetActive( false );
				}
			}
			else
			{
				if( invitePlayerInGuildButton.gameObject.activeInHierarchy )
				{
					invitePlayerInGuildButton.gameObject.SetActive( true );
				}
			}
		}

		#endregion

		#region FriendListItem button functions

		private void OnPlayerInfoButtonClicked()
		{
			//TODO:There need jump to player information screen and show this player info.
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The OnPlayerInfoButtonClicked." ) );
		}

		private void OnInvitePlayerInGuildButtonClicked()
		{
			//TODO:There need jump to guild.
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The OnInvitePlayerInGuildButtonClicked." ) );
		}

		private void OnChatButtonClicked()
		{
			UIManager.Instance.GetUIByType( UIType.MainLeftBar, ( ViewBase ui, System.Object param ) => { ( ui as MainLeftBarView ).SetLeftBarNoClick(); } );
			UIManager.Instance.GetUIByType( UIType.ChatMainUI , EnterChatUI );
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The OnChartButtonClicked." ) );
		}

		//Jump to chat view and tell controller playerUID;
		private void EnterChatUI( ViewBase ui, System.Object param )
		{
			if ( ui != null )
			{
				if ( !ui.openState )
				{
					ui.OnEnter();
					(( ChatMainView ) ui ).GetController().SetPrivateChatPlayerId( playerUID );
				}
			}
		}

		private void OnGiveMoenyButtonClicked()
		{
			//TODO:There will jump to store and lockon this player reday to give gold.
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The OnGiveMoenyButtonClicked." ) );
		}

		private void OnGiftButtonClicked()
		{
			//TODO: There will jump to store and lockon this player reday to send a gift.
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The OnGiftButtonClicked." ) );
		}

		//TODO: Maybe weiChat can't delete friend list.
		private void OnDeleteFriendButtonClicked()
		{
			deleteFriendEvent = SendDeleteFriendMessage;

			MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, deleteFriendEvent, UI.AlertType.ConfirmAndCancel, deleteFriendStr, noticeTitle );

			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The OnDeleteFriendButtonClicked." ) );
		}

		private void SendDeleteFriendMessage()
		{
			//TODO:There will send server a delete friend message;
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The SendDeleteFriendMessage function used." ) );
		}

		#endregion
	}
}
