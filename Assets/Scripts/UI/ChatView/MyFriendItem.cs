using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using Data;

namespace UI
{
	public class MyFriendItem : ScrollViewItemBase
	{
		public System.Action<MyFriendItem> onClickItemHandle;

		public long selectedMyFriendItemIndex;

		private FriendInfo data;

		private Transform friendRemain, check;

		private Button itemButton;

		private Image playerIcon;

		private Text levalText, playerNameText, vipText, isOnlineText;

		private string OffLineStr = "离线";
		private string OnLineStr = "在线";

		private Color32 onLineStrColor = new Color32( 13, 148, 225, 255 );
		private Color32 offLineStrColor = new Color32( 127, 127, 127, 255 );

		void Awake()
		{
			levalText = transform.Find ( "PlayerLevelText" ).GetComponent<Text> ();
			playerNameText = transform.Find ( "PlayerNameText" ).GetComponent<Text> ();
			vipText = transform.Find ( "VipText" ).GetComponent<Text> ();
			isOnlineText = transform.Find ( "IsOnlineText" ).GetComponent<Text> ();
			isOnlineText = transform.Find ( "IsOnlineText" ).GetComponent<Text> ();
			friendRemain = transform.Find ( "FriendremindImg" );
			check = transform.Find ( "CheckImage" );
			playerIcon = transform.Find( "PlayerIcon" ).GetComponent<Image>();

			itemButton = transform.GetComponent<Button> ();
			itemButton.AddListener ( ItemButtonEvent );

			selectedMyFriendItemIndex = -1;
		}

		public override void UpdateItemData( object dataObj )
		{
			base.UpdateItemData ( dataObj );

			if( dataObj == null )
			{
				return;
			}

			data = ( FriendInfo ) dataObj;
			playerNameText.text = data.name;
			levalText.text = data.level.ToString ();
			vipText.text = data.vipLevel.ToString ();

			if( data.isOnline )
			{
				isOnlineText.text = OnLineStr;
				isOnlineText.color = onLineStrColor;
			}
			else
			{
				isOnlineText.text = OffLineStr;
				isOnlineText.color = offLineStrColor;
			}

			if( selectedMyFriendItemIndex ==  data.friendId )
			{
				check.gameObject.SetActive ( true );
			}
			else
			{
				check.gameObject.SetActive ( false );
			}

			Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( data.portrait, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
			{
				playerIcon.SetSprite( atlasSprite );
			}, true );
		}

		private void ItemButtonEvent()
		{
			if( onClickItemHandle != null && selectedMyFriendItemIndex != data.friendId )
			{
				onClickItemHandle ( this );
			}
		}

		public void SelectedItem()
		{
			selectedMyFriendItemIndex = data.friendId;
			check.gameObject.SetActive ( true );
		}

		public void CancelSelected()
		{
			selectedMyFriendItemIndex = -1;
			check.gameObject.SetActive ( false );
		}

		public long GetPlayerId()
		{
			if( data != null )
			{
				return data.friendId;
			}
			return -1;
		}

		public string GetPlayerName()
		{
			return data.name;
		}
	}
}