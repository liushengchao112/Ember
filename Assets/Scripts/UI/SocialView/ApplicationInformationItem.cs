using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using Utils;
using Resource;

namespace UI
{
	public class ApplicationInformationItem : ScrollViewItemBase
	{
		#region ApplicationInformationItem values

		private bool isInited;

		private GameResourceLoadManager gameResourceLoadManager;
	
		private long playerUID;

		private string playerInfoButtonIconName;
		private string playerName;
		private string playerNetworkStatus;

		private int playerLevel;
		private int playerVipIconID;
		private int playerRankIconID;

		public Action<long> agreeEvent;
		public Action<long> refuseEvent;
		public Action currentAction;

		#endregion

		#region Component

		private Button playerInfoButton;
		private Button confirmApplicationButton;
		private Button refuseButton;

		private Image playerVipIcon;
		private Image playerRankIcon;

		private Text playerLevelText;
		private Text playerNameText;
		private Text playerNetworkStatusText;

		#endregion

		#region ApplicationInformation default functions

		private void Initialize()
		{
			isInited = true;

			playerInfoButton = transform.Find( "PlayerInfoButton" ).GetComponent<Button>();
			playerInfoButton.onClick.AddListener( OnPlayerInfoButtonClicked );

			confirmApplicationButton = transform.Find( "ConfirmButton" ).GetComponent<Button>();
			confirmApplicationButton.onClick.AddListener( OnConfirmButtonClicked );

			refuseButton = transform.Find( "RefuseButton" ).GetComponent<Button>();
			refuseButton.onClick.AddListener( OnRefuseButtonClicked );

			playerVipIcon = transform.Find( "VipIcon" ).GetComponent<Image>();
			playerRankIcon = transform.Find( "RankIcon" ).GetComponent<Image>();

			playerLevelText = transform.Find( "PlayerLevel" ).GetComponent<Text>();
			playerNameText = transform.Find( "PlayerNameText" ).GetComponent<Text>();
			playerNetworkStatusText = transform.Find( "NetworkStatusText" ).GetComponent<Text>();

			gameResourceLoadManager = GameResourceLoadManager.GetInstance();
		}

		public override void UpdateItemData( object dataObj )
		{
			base.UpdateItemData( dataObj );

			if( dataObj == null)
			{
				return;
			}

			SetData( ( FriendListDataStruct ) dataObj );
			Show();
		}

		private void SetData( FriendListDataStruct data )
		{
			if( !isInited )
			{
				Initialize();
			}

			this.playerUID = data.playerUID;

			this.playerInfoButtonIconName = data.portrait;
			this.playerName = data.playerName;
			this.playerLevel = data.playerLevel;
			this.playerVipIconID = data.vipLevel;
			this.playerRankIconID = data.grade;
			this.playerNetworkStatus = data.playerNetWorkStatus;
		}

		private void Show()
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
				
			playerNameText.text = playerName;
			playerLevelText.text = playerLevel.ToString();
			playerNetworkStatusText.text = playerNetworkStatus;
		}

		#endregion

		#region ApplicationButton functions

		private void OnPlayerInfoButtonClicked()
		{
			//TODO:There need jump to playerInformation panel show this player info.
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, "OnPlayerInfoButtonClicked" );
		}

		private void OnConfirmButtonClicked()
		{
			currentAction = SendAddFriendMessage;
			MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, currentAction, UI.AlertType.ConfirmAndCancel, "是否添加该玩家为好友？", "提示" );
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "OnConfirmButtonClicked:send player id:{0}", playerUID ) );
		}

		public void SendAddFriendMessage()
		{
			if ( agreeEvent != null) {
				agreeEvent( playerUID );
			}
		}

		private void OnRefuseButtonClicked()
		{
			currentAction = SendRefuseFriendMessage;
			MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, currentAction, UI.AlertType.ConfirmAndCancel, "是否拒绝该玩家为好友？", "提示" );
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "OnRefuseButtonClicked:send player id:{0}", playerUID ) );
		}

		public void SendRefuseFriendMessage()
		{
			if ( refuseEvent != null) {
				refuseEvent( playerUID );
			}
		}

		#endregion
	}
}
