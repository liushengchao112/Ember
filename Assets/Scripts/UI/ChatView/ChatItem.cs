using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using Data;
using Resource;

namespace UI
{
	public class ChatItem : ScrollViewItemBase
	{
		private ChatDataStruct data;

		public System.Action<ChatDataStruct> onClickItemHandle;

		private InlineText messageText;

		// left
		private Transform left;
		private Text leftPlayerNameText;
		private Text leftPlayerLevelText;
		private RectTransform leftChatFrame;
		private Image leftPlayerIcon;
		private Vector2 leftMessagePos = new Vector2 ( 10 , -19 );

		//right
		private Transform right;
		private Text rightPlayerNameText;
		private Text rightPlayerLevelText;
		private RectTransform rightChatFrame;
		private Image rightPlayerIcon;
		private Vector2 rightMessagePos = new Vector2 ( -10, -19 );

		private Button itemButton;

		void Awake()
		{				
			right = transform.Find ( "Right" ).transform;
			rightPlayerNameText = right.Find ( "PlayerNameText" ).GetComponent<Text> ();
			rightChatFrame = right.Find ( "ChatFrame" ).GetComponent<RectTransform> ();
			rightPlayerLevelText = right.Find ( "PlayerLevelText" ).GetComponent<Text> ();
			rightPlayerIcon = right.Find( "PlayerIcon" ).GetComponent<Image>();

			left = transform.Find ( "Left" ).transform;
			leftChatFrame = left.Find ( "ChatFrame" ).GetComponent<RectTransform> ();
			leftPlayerNameText = left.Find ( "PlayerNameText" ).GetComponent<Text> ();
			leftPlayerLevelText = left.Find ( "PlayerLevelText" ).GetComponent<Text> ();
			leftPlayerIcon = left.Find( "PlayerIcon" ).GetComponent<Image>();

			messageText = transform.Find ( "MessageText" ).GetComponent<InlineText> ();
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
				
			this.data = ( ChatDataStruct )dataObj;

			messageText.text = data.message;

			itemButton.interactable = data.isLeft;

			if( data.isLeft )
			{
				right.gameObject.SetActive( false );
				left.gameObject.SetActive( true );
				messageText.transform.localPosition = leftMessagePos;
				messageText.alignment = TextAnchor.MiddleLeft;
				leftPlayerNameText.text = data.chatPlayerInfo.name;
				leftPlayerLevelText.text = data.chatPlayerInfo.level.ToString();
				leftChatFrame.sizeDelta = new Vector2( messageText.preferredWidth + 40, leftChatFrame.rect.height );

				GameResourceLoadManager.GetInstance().LoadAtlasSprite( data.chatPlayerInfo.portrait, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
				{
					leftPlayerIcon.SetSprite( atlasSprite );
				}, true );
			}
			else
			{
				left.gameObject.SetActive( false );
				right.gameObject.SetActive( true );
				messageText.transform.localPosition = rightMessagePos;
				rightPlayerNameText.text =  data.chatPlayerInfo.name;
				rightPlayerLevelText.text = data.chatPlayerInfo.level.ToString();
				messageText.alignment = TextAnchor.MiddleRight;
				rightChatFrame.sizeDelta = new Vector2( messageText.preferredWidth + 40, rightChatFrame.rect.height );

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
				onClickItemHandle ( data );
			}
		}
	}
}

