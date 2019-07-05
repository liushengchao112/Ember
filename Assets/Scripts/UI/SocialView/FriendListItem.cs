using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

using Resource;
using Utils;

namespace UI
{
	public class FriendListItem : ScrollViewItemBase
	{
		#region FriendListItem values

		public Action<long> deleteFriendEvent;
		public Action<long> pullBlackEvent;
		public Action<long> giveMoneyEvent;
		public Action<long> giveGiftEvent; 
		public Action currentAction;

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
        private bool canSendGift;

		private bool isJoinedGuild;

		private string noticeTitle = "提示";
		private string deleteFriendStr = "是否删除好友?";
		private string pullBlackStr = "是否拉入黑名单?";

		#endregion

		#region Component

		private Button playerInfoButton;
		private Button invitePlayerInGuildButton;
		private Button chatButton;
		private Button giveMoneyButton;
		private Button giftButton;
		private Button deleteFriendButton;
		private Button blockListButton;

		private Image playerVipIcon;
		private Image playerRankIcon;

		private Text playerNameText;
		private Text playerLevelText;
		private Text playerNetworkStatusText;

		#endregion

		#region FriendListItem default functions;

		private void Initialize()
		{
			isInited = true;

			playerInfoButton = transform.Find( "PlayerInfoButton" ).GetComponent<Button>();
			playerInfoButton.onClick.AddListener( OnPlayerInfoButtonClicked );

			chatButton = transform.Find("ButtonPanel/ChatButton").GetComponent<Button>();
			chatButton.onClick.AddListener( OnChatButtonClicked );

			giveMoneyButton = transform.Find("ButtonPanel/MoneyButton").GetComponent<Button>();
			giveMoneyButton.onClick.AddListener( OnGiveMoneyButtonClicked );

			giftButton = transform.Find("ButtonPanel/GiftButton").GetComponent<Button>();
			giftButton.onClick.AddListener( OnGiftButtonClicked );

			deleteFriendButton = transform.Find("ButtonPanel/DeleteButton").GetComponent<Button>();
			deleteFriendButton.onClick.AddListener( OnDeleteFriendButtonClicked );

			invitePlayerInGuildButton = transform.Find( "BlockListButton" ).GetComponent<Button>();
			invitePlayerInGuildButton.onClick.AddListener( OnBlockListButtonClickEvent );

			playerVipIcon = transform.Find( "VipIcon" ).GetComponent<Image>();
			playerRankIcon = transform.Find( "RankIcon" ).GetComponent<Image>();

			playerNameText = transform.Find( "PlayerNameText" ).GetComponent<Text>();
			playerLevelText = transform.Find( "PlayerLevel" ).GetComponent<Text>();
			playerNetworkStatusText = transform.Find( "NetworkStatusText" ).GetComponent<Text>();

			gameResourceLoadManager = GameResourceLoadManager.GetInstance();
		}

		public override void UpdateItemData( object dataObj )
		{
			base.UpdateItemData( dataObj );
			if( dataObj == null )
			{
				return;
			}
			SetData( ( FriendListDataStruct )dataObj );
			Show();
		}

		public void SetData( FriendListDataStruct data )
		{
			if( !isInited )
			{
				Initialize();
			}
			this.playerUID = data.playerUID;
            this.canSendGift = data.canGiveGold;
			this.playerInfoButtonIconName = data.portrait;
			this.playerName = data.playerName;
			this.playerLevel = data.playerLevel;
			this.playerVipIconID = data.vipLevel;
			this.playerRankIconID = data.grade;
			this.playerNetworkStatus = data.playerNetWorkStatus;
			this.isJoinedGuild = data.isPlayerJionedGuild;
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
				DebugUtils.LogError( DebugUtils.Type.UI_SocialScreen, string.Format( "PlayerInfoButtonIconName or playerInfoIconPath is null, Check that." ) );
			}

            if (!this.canSendGift)
            {
                giveMoneyButton.GetComponent<Image>().SetGray(true);
                giveMoneyButton.onClick.RemoveAllListeners();
            }
            else
            {
                giveMoneyButton.GetComponent<Image>().SetGray(false);
                if (giveMoneyButton.onClick==null)
                {
                    giveMoneyButton.onClick.AddListener(OnGiveMoneyButtonClicked);
                }
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

		private void OnGiveMoneyButtonClicked()
		{
			if( giveMoneyEvent != null )
			{
                giveMoneyEvent( playerUID );
			}
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The OnGiveMoneyButtonClicked." ) );
		}

		private void OnGiftButtonClicked()
		{
			if( giveGiftEvent != null )
			{
				giveGiftEvent( playerUID );
			}
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The OnGiftButtonClicked." ) );
		}

		private void OnDeleteFriendButtonClicked()
		{
			currentAction = SendDeleteFriendMessage;
			MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, currentAction, UI.AlertType.ConfirmAndCancel, deleteFriendStr, noticeTitle );
		}

		private void SendDeleteFriendMessage()
		{
			if( deleteFriendEvent != null  )
			{
				deleteFriendEvent( playerUID );
			}
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "SendDeleteFriendMessage is send player id:{0}", playerUID ) );
		}

		private void OnBlockListButtonClickEvent()
		{
			currentAction = SendPullBlackMessage;
			MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, currentAction, UI.AlertType.ConfirmAndCancel, pullBlackStr, noticeTitle );
		}

		private void SendPullBlackMessage()
		{
			if( pullBlackEvent != null )
			{
				pullBlackEvent( playerUID );
			}
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "The SendPullBlackMessage is send player id:{0}", playerUID ) );
		}

		#endregion
	}
}
