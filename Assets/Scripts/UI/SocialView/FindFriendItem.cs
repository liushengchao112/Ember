using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using Utils;
using Resource;

namespace UI
{
	//This is FindFriendPanel used general player info item.So this can use for find friend and system recommend.Dwayne
	public class FindFriendItem : ScrollViewItemBase
    {		
		#region FindFriendItem values

		public Action<long> addPlayerInFriendList;

		private bool isInited;

		private GameResourceLoadManager gameResourceLoadManager; 

		public long playerUID = -1;

		private int playerLevel;
		private string playerName;
		private string playerNetworkStatus;
		private string playerIconName;
		private int playerVipIconID = 0;
		private int playerRankIconID = 0;

		private string noticeTitle = "提示";
		private string waitingConfirmStr = "好友申请已经发送过，请耐心等待对方确认。";

		//private string playerIconPath;
		private bool alreadyApplication;

		#endregion

		#region Componet

		private Button playerInfoButton;
		private Button addFriendButton;

		private Image playerVipIcon;
		private Image playerRankIcon;

		private Text playerLevelText;
		private Text playerNameText;
		private Text playerNetworkStatusText;

		#endregion

		#region FindFriendItem default functions

		private void Initialize()
		{
			isInited = true;

			playerInfoButton = transform.Find( "PlayerIcon" ).GetComponent<Button>();
			playerInfoButton.onClick.AddListener( OnPlayerInfoButtonClicked );

			addFriendButton = transform.Find( "AddFriendButton" ).GetComponent<Button>();
			addFriendButton.onClick.AddListener( OnAddFriendButtonClicked );

			playerVipIcon = transform.Find( "VipIcon" ).GetComponent<Image>();
			playerRankIcon = transform.Find( "RankIcon" ).GetComponent<Image>();

			playerLevelText = transform.Find( "PlayerIcon/Text" ).GetComponent<Text>();
			playerNameText = transform.Find( "PlayerNameText" ).GetComponent<Text>();
			playerNetworkStatusText = transform.Find( "NetworkStatusText" ).GetComponent<Text>();

			gameResourceLoadManager = GameResourceLoadManager.GetInstance();
		}


        public override void UpdateItemData(object dataObj)
        {
            base.UpdateItemData(dataObj);

            if (dataObj == null)
            {
                return;
            }

            SetData((FriendListDataStruct)dataObj);
            Show();
        }

        //When set finished, if you want see show effect must use Show function.
        public void SetData(FriendListDataStruct friendListDataStruct )
		{
			if( !isInited )
			{
				Initialize();
			}
			this.playerUID = friendListDataStruct.playerUID;
			this.playerIconName = friendListDataStruct.portrait;
			this.playerVipIconID = friendListDataStruct.vipLevel;
			this.playerRankIconID = friendListDataStruct.grade;
			this.playerName = friendListDataStruct.playerName;
			this.playerLevel = friendListDataStruct.playerLevel;
			// isOnline data error
			if(friendListDataStruct.isOnline)
			{
				this.playerNetworkStatus = "在线";
			}
			else
			{
				this.playerNetworkStatus = "离线";
			}				
			this.alreadyApplication = friendListDataStruct.alreadyApplication;
		}

		//If you want show this item must use SetData before this function.
		public void Show()
		{
			if( !this.gameObject.activeInHierarchy )
			{
				this.gameObject.SetActive( true );
			}
			if ( !string.IsNullOrEmpty( playerIconName ) )
			{
				gameResourceLoadManager.LoadAtlasSprite( playerIconName, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
				{
					playerInfoButton.image.SetSprite( atlasSprite );
				}, true );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.UI_SocialScreen, string.Format( "The playerIconName or playerIconPath is null.Check that." ) );
			}

			//We table not have this ID,When they finished open this.Need modify.
			/*
			if ( playerVipIconID != 0 )
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

		#region ButtonFunctions

		private void OnPlayerInfoButtonClicked()
		{
			//TODO:There will jump to player detail panel to show this player information.
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, "OnPlayerInfoButtonClicked." );
		}

		private void OnAddFriendButtonClicked()
		{
			if( !alreadyApplication && addPlayerInFriendList != null )
			{
				alreadyApplication = true;
				addPlayerInFriendList( playerUID );
			}
			else
			{
				MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, waitingConfirmStr, noticeTitle );
			}
		}

		private void TryAddPlayerInFriendList()
		{
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, string.Format( "TryAddPlayerInFriendList is send player id:{0}", playerUID ) );
		}

		#endregion
	}
}
