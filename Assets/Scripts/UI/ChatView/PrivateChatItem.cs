using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using Data;
using Resource;

namespace UI
{
	public class PrivateChatItem : ScrollViewItemBase
	{
		private ChatDataStruct data;

		public System.Action<ChatDataStruct> onClickItemHandle;

		// left
		private Transform left;
		private Text leftPlayerNameText, leftPlayerLevelText;
		private RectTransform leftChatFrame;
		private Image leftPlayerIcon;
		public Vector2 leftMessagePos = new Vector2 ( 57 , -25 );

		//right
		private Transform right;
		private Text rightPlayerNameText, rightPlayerLevelText;
		private RectTransform rightChatFrame;
		private Image rightPlayerIcon;
		public Vector2 rightMessagePos = new Vector2 ( -54 , -25 );

		private InlineText messageText;

		private Button itemButton;
		private Transform message;

		void Awake()
		{	
			messageText = transform.Find ( "MessageText" ).GetComponent<InlineText> ();
			message = transform.Find ( "MessageText" );

			right = transform.Find ( "Right" ).transform;
			rightChatFrame = right.Find ( "ChatFrame" ).GetComponent<RectTransform> ();
			rightPlayerNameText = right.Find ( "PlayerNameText" ).GetComponent<Text> ();
			rightPlayerLevelText = right.Find ( "PlayerLevelText" ).GetComponent<Text> ();
			rightPlayerIcon = right.Find( "PlayerIcon" ).GetComponent<Image>();

			left = transform.Find ( "Left" ).transform;
			leftChatFrame = left.Find ( "ChatFrame" ).GetComponent<RectTransform> ();
			leftPlayerNameText = left.Find ( "PlayerNameText" ).GetComponent<Text> ();
			leftPlayerLevelText = left.Find ( "PlayerLevelText" ).GetComponent<Text> ();
			leftPlayerIcon = left.Find( "PlayerIcon" ).GetComponent<Image>();

			itemButton = transform.GetComponent<Button> ();
			itemButton.AddListener ( ItemButtonEvent );
		}

		public override void UpdateItemData( object dataObj )
		{
			base.UpdateItemData ( dataObj );

			if( dataObj == null )
			{
				return;
			}

			this.data = ( ChatDataStruct ) dataObj;
			messageText.text = data.message;
			itemButton.interactable = data.isLeft;

			if( data.isLeft )
			{
				right.gameObject.SetActive( false );
				left.gameObject.SetActive( true );
				message.localPosition = leftMessagePos;
				messageText.alignment = TextAnchor.MiddleLeft;
				leftPlayerNameText.text = data.chatPlayerInfo.name;
				leftPlayerLevelText.text = data.chatPlayerInfo.level.ToString();
				leftChatFrame.sizeDelta = new Vector2 ( messageText.preferredWidth + 40 , leftChatFrame.rect.height );

				GameResourceLoadManager.GetInstance().LoadAtlasSprite( data.chatPlayerInfo.portrait, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
				{
					leftPlayerIcon.SetSprite( atlasSprite );
				}, true );
			}
			else
			{
				left.gameObject.SetActive( false );
				right.gameObject.SetActive( true );
				message.localPosition = rightMessagePos;
				messageText.alignment = TextAnchor.MiddleRight;
				rightPlayerNameText.text = data.chatPlayerInfo.name;
				rightPlayerLevelText.text = data.chatPlayerInfo.level.ToString();
				rightChatFrame.sizeDelta = new Vector2 ( messageText.preferredWidth + 40 , rightChatFrame.rect.height );

				GameResourceLoadManager.GetInstance().LoadAtlasSprite( data.chatPlayerInfo.portrait, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
				{
					rightPlayerIcon.SetSprite( atlasSprite );
				}, true );
			}
		}

		private void ItemButtonEvent()
		{
			if( onClickItemHandle != null)
			{
				onClickItemHandle ( ( ChatDataStruct ) data );
			}
		}
	}
}

