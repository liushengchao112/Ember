using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

using Resource;
using Utils;
using Data;

namespace UI
{
	public class ChatMainView : ViewBase
	{
		private string buyHornPromptStr = "喇叭数量不足.是否花费{0}钻石购买";
		private string fingerShiftUpStr = "手指上移, 取消发送";
		private string fingerLoosenStr = "松开手指, 取消发送";
		private string gradeStr = "段位 ：<color=green>{0}</color>";
		private string addFriendStr = "是否添加<color=blue>{0}</color>为好友？";
		private string levelStr = "等级 : {0}";

		private string stringLengthPopTitle = "提示";
		private string stringLengthPopStr = "最多只能发送30字符";

		private Transform chatTop;
		private Transform chatBottom;
		private Transform chatMain;
		private Transform chatMiddle;
		private Transform worldChat;
		private Transform privateChat;
		private Transform privateChatBg;
		private Transform labourUnionChat;
		private Transform playerInfo;

		private Button exitButton;
		private Button hornButton;
		private Button expressionButton;
		private Button sendButton;

		private LongPressButton speakButton;

		private Toggle[] chatType;

		private Toggle voiceToggle;

		private ToggleGroup myFriendGroup;

		private InputField sendMessageInputField;

		private Text hornNumberText;
		private Text sendText;
		private Text worldText;
		private Text privateChatText;
		private Text labourUnionText;
		private Text friendNumberInfoText;
		private Text friendNameText;

		private ScrollRect chatScrollRect;
		private ScrollRect myFriendScrollRect;
		private ScrollRect privateChatScrollRect;

		private ChatScrollView chatScrollView;
		private MyFriendScrollView myFriendScrollView;
		private PrivateChatScrollView privateChatScrollView;

		private bool isNotificate = false;
		private float notificationTimer;
		private float notificationResidenceTime = 5;

		#region panel

		private Text buyHornContentText;
		private Text tipText;

		private Transform playerInfoPanel;
		private Transform shieldPlayerPanel;
		private Transform addFriendPanel;
		private Transform expressionPanel;
		private Transform buyHornPanel;
		private Transform buyDiamondPanel;
		private Transform recordingTipPanel;

		private Button addFriendButton;
		private Button lookFriendButton;
		private Button shieldFriendButton;
		private Button playerInfoPanelMaskButton;
		private Button shieldPlayerPanelEnterButton;
		private Button shieldPlayerPanelCancelButton;
		private Button addFriendPanelEnterButton;
		private Button addFriendPanelCancelButton;
		private Button expressionMaskButton;
		private Button buyHornButton;
		private Button buyHornCanelButton;
		private Button buyDiamondButton;
		private Button buyDiamondCanelButton;

		//PlayerInfoPanel
		private Text InfoPanelPlayerNameText;
		private Text InfoPanelPlayerLevelText;
		private Text InfoPanelPlayerDanText;
		private long InfoPanelPlayerId;
		private string InfoPanelPlayerName;

		//Broadcast message panel
		private Transform broadcastPanel;
		private Text playerLevelText;
		private Text playerNameText;
		private Image playerIcon;
		private InlineText messageText;

		//ShieldPlayerPanel
		private Text shieldPlayerPanelCountText;

		//AddFriendPanel
		private Text addFriendPanelCountText;

		#endregion
		private HornPanel hornPanel;

		private ChatMainController controller;

		private MyFriendItem lastSelectedMyFriendItem;

		private bool isLoadPrivateChatItemFinish = false;

		private long selectedMyFriendItemId;

		public long SelectedMyFriendItemId
		{
			get
			{
				return selectedMyFriendItemId;
			}
		}

		public enum ChatTag
		{
			WorldChat,
			PrivateChat,
			UnionChat,
		}

		public ChatTag currentChatTag = ChatTag.WorldChat;

		public override void OnEnter()
		{
			base.OnEnter ();

			ResfresData();
		}
			
		public override void OnInit()
		{
			base.OnInit ();
            controller = new ChatMainController( this );
            _controller = controller;

            InitComponent ();
			InitListener ();
			InitChatItem ();
		}

		private void InitComponent()
		{
			chatTop = transform.Find ( "ChatTop" );
			chatBottom = transform.Find ( "ChatBottom" );
			chatMain = transform.Find ( "ChatMain" );
			chatMiddle = transform.Find ( "ChatMiddle" );

			worldChat = chatMain.Find ( "WorldChat" );
			privateChat = chatMain.Find ( "PrivateChat" );
			privateChatBg = chatMain.Find( "PrivateChat/PrivateChatTop/BackGroundImage" );
			labourUnionChat = chatMain.Find ( "LabourUnionChat" );

			playerInfo = privateChat.Find ( "ChatPlayerInfo" );

			hornPanel = chatMiddle.Find ( "HornPanel" ).GetComponent<HornPanel>();
			hornPanel.sendHornMessageOnClick = OnClickSendHornMessageCallBack;

			playerInfoPanel = chatMiddle.Find ( "PlayerInfoPanel" );
			shieldPlayerPanel = chatMiddle.Find ( "ShieldPlayerPanel" );
			addFriendPanel = chatMiddle.Find ( "AddFriendPanel" );
			expressionPanel = chatMiddle.Find ( "ExpressionPanel" );

			hornNumberText = chatBottom.Find ( "HornNumberText" ).GetComponent<Text> ();
			sendText = chatBottom.Find ( "SendText" ).GetComponent<Text> ();

			worldText = chatTop.transform.Find ( "WorldToggle/WorldText" ).GetComponent<Text> ();
			worldText.color = Color.white;
			privateChatText = chatTop.transform.Find ( "PrivateChatToggle/PrivateChatText" ).GetComponent<Text> ();
			privateChatText.color = Color.gray;
			labourUnionText = chatTop.transform.Find ( "LabourUnionToggle/LabourUnionText" ).GetComponent<Text> ();
			labourUnionText.color = Color.gray;

			//BuyHornPanel
			buyHornPanel = chatMiddle.Find ( "BuyHornPanel" );
			buyHornContentText = buyHornPanel.Find ( "ContentText" ).GetComponent<Text> ();
			buyHornButton = buyHornPanel.Find ( "EnterButton" ).GetComponent<Button> ();
			buyHornCanelButton = buyHornPanel.Find ( "CancelButton" ).GetComponent<Button> ();

			//BuyDiamondPanel
			buyDiamondPanel = chatMiddle.Find ( "BuyDiamondPanel" );
			buyDiamondButton = buyDiamondPanel.Find ( "EnterButton" ).GetComponent<Button> ();
			buyDiamondCanelButton = buyDiamondPanel.Find ( "CancelButton" ).GetComponent<Button> ();

			//VoiceTipPanel
			recordingTipPanel = chatMiddle.Find ( "RecordingTipPanel" );
			tipText = recordingTipPanel.Find ( "TipText" ).GetComponent<Text> ();

			//PlayerInfoPanel
			addFriendButton = playerInfoPanel.Find ( "AddFriendButton" ).GetComponent<Button> ();
			lookFriendButton = playerInfoPanel.Find ( "LookFriendButton" ).GetComponent<Button> ();
			shieldFriendButton = playerInfoPanel.Find ( "ShieldFriendButton" ).GetComponent<Button> ();
			playerInfoPanelMaskButton = playerInfoPanel.Find ( "MaskButton" ).GetComponent<Button> ();
			InfoPanelPlayerNameText = playerInfoPanel.Find ( "PlayerNameText" ).GetComponent<Text> ();
			InfoPanelPlayerLevelText = playerInfoPanel.Find ( "PlayerLevelText" ).GetComponent<Text> ();
			InfoPanelPlayerDanText = playerInfoPanel.Find ( "PlayerDanText" ).GetComponent<Text> ();

			//AddFriendPanel
			addFriendPanelEnterButton = addFriendPanel.Find ( "EnterButton" ).GetComponent<Button> ();
			addFriendPanelCancelButton = addFriendPanel.Find ( "CancelButton" ).GetComponent<Button> ();
			addFriendPanelCountText = addFriendPanel.Find ( "PlayerIcon/AddPlayerText" ).GetComponent<Text> ();

			//ShieldPlayerPanel
			shieldPlayerPanelEnterButton = shieldPlayerPanel.Find ( "EnterButton" ).GetComponent<Button> ();
			shieldPlayerPanelCancelButton = shieldPlayerPanel.Find ( "CancelButton" ).GetComponent<Button> ();
			shieldPlayerPanelCountText = shieldPlayerPanel.Find ( "ShieldPlayerTextTitle/ShieldPlayerText" ).GetComponent<Text> ();

			voiceToggle = chatBottom.Find ( "VoiceToggle" ).GetComponent<Toggle> ();
			sendMessageInputField = chatBottom.Find ( "SendMessageInputField" ).GetComponent<InputField> ();
			speakButton = chatBottom.Find ( "SpeakButton" ).GetComponent<LongPressButton> ();
			speakButton.pressDownEvent = BeginRecord;
			speakButton.longPressEvent = Recording;
			speakButton.leaveEvent = PauseRecord;
			speakButton.EnterEvent = ResumeRecord;
			speakButton.pressUpEvent = BreakOffRecord;

			exitButton = transform.Find ( "MaskButton" ).GetComponent<Button> ();
			hornButton = chatBottom.Find ( "HornButton" ).GetComponent<Button> ();
			expressionButton = chatBottom.Find ( "ExpressionButton" ).GetComponent<Button> ();
			sendButton = chatBottom.Find ( "SendButton" ).GetComponent<Button> ();
			expressionMaskButton = expressionPanel.Find ( "ExpressionMaskButton" ).GetComponent<Button> ();

			myFriendScrollView = privateChat.Find ( "MyFriendScrollView" ).GetComponent<MyFriendScrollView>();
			myFriendScrollView.OnCreateItemHandler = OnCreateFriendtem;
			myFriendScrollRect = myFriendScrollView.GetComponent<ScrollRect> ();
			myFriendGroup = privateChat.Find ( "MyFriendScrollView/ViewPort/PlayerInfoGroup" ).GetComponent<ToggleGroup> ();
			friendNumberInfoText = privateChat.Find ( "PrivateChatTop/FriendNumberText" ).GetComponent<Text>();
			friendNameText = privateChat.Find ( "PrivateChatTop/FriendNameText" ).GetComponent<Text>();

			chatScrollView =  chatMain.Find ( "ChatScrollView" ).GetComponent<ChatScrollView> ();
			chatScrollView.OnCreateItemHandler = OnCreateItem;
			chatScrollRect = chatScrollView.GetComponent<ScrollRect> ();

			//show Broadcast message 
			broadcastPanel = chatScrollView.transform.Find( "BroadcastPanel" );
			playerLevelText = broadcastPanel.transform.Find( "PlayerLevelText" ).GetComponent<Text>();
			playerNameText = broadcastPanel.transform.Find( "PlayerNameText" ).GetComponent<Text>();
			playerIcon = broadcastPanel.transform.Find( "PlayerIcon" ).GetComponent<Image>();
			messageText = broadcastPanel.transform.Find( "MessageText" ).GetComponent<InlineText>();

			privateChatScrollView =  privateChat.Find ( "PrivateChatScrollView" ).GetComponent<PrivateChatScrollView> ();
			privateChatScrollView.OnCreateItemHandler = OnCreatePrivateChatItem;
			privateChatScrollRect = privateChatScrollView.GetComponent<ScrollRect> ();
		}

		private void InitListener()
		{
			chatType = new Toggle[ chatTop.childCount ];

			for ( int i = 0; i < chatType.Length; i++ )
			{
				chatType[ i ] = chatTop.GetChild ( i ).transform.GetComponent<Toggle> ();
			}

			chatType[ 0 ].AddListener( WorldToggleEvent );
			chatType[ 0 ].isOn = true;
			chatType[ 0 ].interactable = false;

			chatType[ 1 ].AddListener ( PrivateChatToggleEvent );
			chatType[ 2 ].AddListener ( LabourUnionChatToggleEvent );

			exitButton.AddListener ( ExitButtonEvent );
			hornButton.AddListener ( HornButtonEvent );
			buyDiamondButton.AddListener ( BuyDiamondButtonEvent );
			buyDiamondCanelButton.AddListener ( buyDiamondCanelButtonEvent );
			buyHornButton.AddListener ( BuyHornButtonEvent );
			buyHornCanelButton.AddListener ( BuyHornCanelButtonEvent );
			sendButton.AddListener ( SendButtonEvent );
			expressionButton.AddListener ( ExpressionButtonEvent );
			addFriendButton.AddListener ( AddFriendButtonEvent );
			lookFriendButton.AddListener ( LookFriendButtonEvent );
			shieldFriendButton.AddListener ( ShieldFriendButtonEvent );
			playerInfoPanelMaskButton.AddListener ( PlayerInfoPanelMeskButtonEvent );
			shieldPlayerPanelEnterButton.AddListener ( ShieldPlayerPanelEnterButtonEvent );
			shieldPlayerPanelCancelButton.AddListener ( ShieldPlayerPanelCancelButtonEvent );
			addFriendPanelEnterButton.AddListener ( AddFriendPanelEnterButtonEvent );
			addFriendPanelCancelButton.AddListener ( AddFriendPanelCancelButtonEvent );
			expressionMaskButton.AddListener ( ExpressionMaskButtonEvent );

			voiceToggle.onValueChanged.AddListener ( VoiceToggleEvent );

			sendMessageInputField.onValueChanged.AddListener ( SendMessageInputFieldEvent );
		}
			
		private void InitChatItem()
		{
            GameResourceLoadManager.GetInstance().LoadResource( "ChatItem", OnLoadChatItem, true );
            GameResourceLoadManager.GetInstance().LoadResource( "MyFriendItem", OnLoadMyFriendItem, true );
            GameResourceLoadManager.GetInstance().LoadResource( "PrivateChatItem", OnLoadPrivateChatItem, true );
        }

		public void ResfresData()
		{
			hornNumberText.text = string.Format( "X{0}",  controller.GetHornNumber ());
		}

		#region Recording

		private void BeginRecord()
		{
			OpenRecordingTipPanel ();
			tipText.text = fingerShiftUpStr;
		}

		private void Recording( int time )
		{
			
		}

		private void PauseRecord()
		{
			tipText.text = fingerLoosenStr;
		}

		private void ResumeRecord()
		{
			tipText.text = fingerShiftUpStr;
		}

		private void BreakOffRecord()
		{
			recordingTipPanel.gameObject.SetActive ( false );
		}
	
		#endregion

		void Update()
		{
			if( !isNotificate && controller.GetNotificationQueue().Count > 0 )
			{
				ShowBroadcastMessage( controller.GetNotificationQueue().Dequeue() );
			}

			if( isNotificate )
			{
				notificationTimer += Time.deltaTime;
				if( notificationTimer >= notificationResidenceTime )
				{
					HideShowBroadcastMessage();
				}
			}
		}

		#region ResfresItem

		public void ResfresChatItemData( IList dataList )
		{
			chatScrollView.gameObject.SetActive( true );
			chatScrollView.InitializeWithData( dataList );
			chatScrollView.dataCount = dataList.Count;
			chatScrollView.GoDown();
			chatScrollView.UpdateScrollView( Vector2.zero );
		}

		public void ResfresMyFriendItemData( List<FriendInfo> dataList, long playerId )
		{
			myFriendScrollView.gameObject.SetActive( true );
			myFriendScrollView.InitializeWithData( dataList );
			myFriendScrollView.GoTop();

			int index = 0;

			for ( int i = 0; i < dataList.Count; i++ )
			{
				if( dataList [ i ].friendId == playerId )
				{
					index = i;
					break;
				}
			}

			myFriendScrollView.ShowItemByDataIndex( index );

			MyFriendItem myFriendItem = myFriendScrollView.FindMyFriendItemByPlayerId( playerId );
			myFriendItem.selectedMyFriendItemIndex = myFriendItem.GetPlayerId();
			OnClickMyFriendItemCallBack( myFriendItem );
			friendNumberInfoText.text = string.Format( "{0}/{1}", controller.GetFriendOnLineNumber(), dataList.Count ); 
			myFriendScrollView.UpdateScrollView( Vector2.zero );
		}

		public void ResfresPrivateChatItemData( IList dataList )
		{
			if( dataList.Count < 1 )
			{
				privateChatScrollView.gameObject.SetActive( false );
				return;
			}

			privateChatScrollView.gameObject.SetActive( true );
			privateChatScrollView.InitializeWithData( dataList );
			privateChatScrollView.dataCount = dataList.Count;
			privateChatScrollView.GoDown();
			privateChatScrollView.UpdateScrollView( Vector2.zero );
		}

		#endregion

		#region create item

		private void OnCreateItem( ChatItem item )
		{
			item.onClickItemHandle = OpenPlayerInfoPanel;
		}

		private void OnCreatePrivateChatItem( PrivateChatItem item )
		{
			item.onClickItemHandle = OpenPlayerInfoPanel;
		}

		private void OnCreateFriendtem( MyFriendItem item )
		{
			item.onClickItemHandle = OnClickMyFriendItemCallBack;
		}
			
		#endregion

		#region click item callback

		private void OnClickMyFriendItemCallBack(  MyFriendItem item )
		{
			if( lastSelectedMyFriendItem != item && lastSelectedMyFriendItem != null )
			{
				lastSelectedMyFriendItem.CancelSelected ();
			}

			item.SelectedItem ();

			lastSelectedMyFriendItem = item;

			ShowPrivateChatScrollView ( item.GetPlayerId() );

			selectedMyFriendItemId = item.GetPlayerId();

			friendNameText.text = item.GetPlayerName ();

			ResfresPrivateChatItemData ( controller.GetPrivateChatData ( selectedMyFriendItemId ) );
		}

		private void OnClickSendHornMessageCallBack( string message )
		{
			controller.SendChatMessage( ChatType.WorldChat, message, controller.GetPlayerId() , ChatConsumptionType.ChatItem );
		}

		#endregion
			
		#region Match Item

		private void OnLoadChatItem( string name, Object obj, System.Object param )
		{
			chatScrollView.InitDataBase( chatScrollRect , obj , 1 , 883, 97, 10, new Vector2 ( 441 , -46 ) );
			ResfresChatItemData ( controller.GetWorldChatDataList() );
		}

		private void OnLoadMyFriendItem( string name, Object obj, System.Object param )
		{
			myFriendScrollView.InitDataBase ( myFriendScrollRect , obj , 1 , 232 , 97 , 10 , new Vector2 ( 116 , -49 ) );

			isLoadPrivateChatItemFinish = true;
			EnterPrivateChat( controller.GetPrivateChatPlayerId() );
		}

		private void OnLoadPrivateChatItem( string name, Object obj, System.Object param )
		{
			privateChatScrollView.InitDataBase ( privateChatScrollRect , obj , 1 , 647 , 99 , 15 , new Vector2 ( 323 , -49 ) );
		}

		#endregion

		#region ButtonEvent

		private void ExitButtonEvent()
		{
			OnExit ( true );
		}

		private void AddFriendButtonEvent()
		{
			playerInfoPanel.gameObject.SetActive ( false );
			ChildChange ( addFriendPanel.parent, addFriendPanel.transform.name );
			addFriendPanelCountText.text = string.Format( addFriendStr, InfoPanelPlayerName );
		}

		private void LookFriendButtonEvent()
		{
			playerInfoPanel.gameObject.SetActive ( false );
		}

		private void ShieldFriendButtonEvent()
		{
			playerInfoPanel.gameObject.SetActive ( false );
			ChildChange ( shieldPlayerPanel.parent, shieldPlayerPanel.name );
			shieldPlayerPanelCountText.text = InfoPanelPlayerName;
		}

		private void PlayerInfoPanelMeskButtonEvent()
		{
			playerInfoPanel.gameObject.SetActive ( false );
		}

		private void HornButtonEvent()
		{
			if( controller.GetHornNumber() > 0 )
			{
				OpenHornPanel ();
			}
			else
			{
				if( controller.GetPlayerEmber () < controller.GetHornDiamondCost() )
				{
					OpenBuyDiamondPanel ();
				}
				else
				{
					OpenBuyHornPanel ();
				}
			}
		}

		private void BuyHornButtonEvent()
		{

		}

		private void BuyDiamondButtonEvent()
		{
			OpenBuyDiamondPanel ();
		}

		private void buyDiamondCanelButtonEvent()
		{
			buyDiamondPanel.gameObject.SetActive ( false );
		}

		private void BuyHornCanelButtonEvent()
		{
			buyHornPanel.gameObject.SetActive ( false );
		}

		private void SendButtonEvent()
		{
			if( string.IsNullOrEmpty( sendMessageInputField.text ) )
			{
				return;
			}
			
			switch ( currentChatTag )
			{
				case ChatTag.WorldChat:

					controller.SendChatMessage( ChatType.WorldChat, sendMessageInputField.text, 0, ChatConsumptionType.ChatGeneral );

					break;

				case ChatTag.PrivateChat:

					controller.SendChatMessage( ChatType.FriendsChat, sendMessageInputField.text, selectedMyFriendItemId, ChatConsumptionType.ChatGeneral );

					break;

				case ChatTag.UnionChat:

					break;
			}

			sendMessageInputField.text = "";
		}

		private void ExpressionButtonEvent()
		{
			OpenExpressionPanel ();
		}
			
		private void ShieldPlayerPanelEnterButtonEvent()
		{
			controller.SendRelationApplicationMessage( RelationApplicationType.BlockList, InfoPanelPlayerId );
			shieldPlayerPanel.gameObject.SetActive ( false );
		}

		private void ShieldPlayerPanelCancelButtonEvent()
		{
			shieldPlayerPanel.gameObject.SetActive ( false );
		}

		private void AddFriendPanelEnterButtonEvent()
		{
			controller.SendRelationApplicationMessage( RelationApplicationType.ApplyingRelation, InfoPanelPlayerId );
			addFriendPanel.gameObject.SetActive ( false );
		}

		private void AddFriendPanelCancelButtonEvent()
		{
			addFriendPanel.gameObject.SetActive ( false );
		}

		private void ExpressionMaskButtonEvent()
		{
			expressionPanel.gameObject.SetActive ( false );
		}

		#endregion

		#region ToggleEvent

		private void WorldToggleEvent( bool IsOn )
		{
			chatType[ 0 ].interactable = !IsOn;

			if( !IsOn )
			{
				return;
			}
				
			worldChat.gameObject.SetActive( true );
			privateChat.gameObject.SetActive( false );
			labourUnionChat.gameObject.SetActive( false );
			chatBottom.gameObject.SetActive( true );

			worldText.color = Color.white;
			privateChatText.color = Color.gray;
			labourUnionText.color = Color.gray;

			currentChatTag = ChatTag.WorldChat;

			controller.SendRelationList();

			hornButton.interactable = true;
			ResfresChatItemData( controller.GetWorldChatDataList() );
		}

		private void PrivateChatToggleEvent( bool IsOn )
		{
			chatType[ 1 ].interactable = !IsOn;

			if( !IsOn )
			{
				return;
			}

			worldChat.gameObject.SetActive ( false );
			privateChat.gameObject.SetActive ( true );
			labourUnionChat.gameObject.SetActive ( false );
			chatScrollView.gameObject.SetActive(false);

			worldText.color = Color.gray;
			privateChatText.color = Color.white;
			labourUnionText.color = Color.gray;

			currentChatTag = ChatTag.PrivateChat;

			List<FriendInfo> data = controller.GetMyFriendData ();

			if( data.Count > 0 )
			{
				privateChatBg.gameObject.SetActive( true );
				chatBottom.gameObject.SetActive ( true );
				ResfresMyFriendItemData ( controller.GetMyFriendData () , data[ 0 ].friendId );
			}
			else
			{
				privateChatBg.gameObject.SetActive( false );
				chatBottom.gameObject.SetActive ( false );
			}

			hornButton.interactable = false;
		}

		private void LabourUnionChatToggleEvent( bool IsOn )
		{
			chatType[ 2 ].interactable = !IsOn;

			if( !IsOn )
			{
				return;
			}

			worldChat.gameObject.SetActive ( false );
			privateChat.gameObject.SetActive ( false );
			labourUnionChat.gameObject.SetActive ( true );
			chatScrollView.gameObject.SetActive(false);
			chatBottom.gameObject.SetActive ( false );

			worldText.color = Color.gray;
			privateChatText.color = Color.gray;
			labourUnionText.color = Color.white;

			currentChatTag = ChatTag.UnionChat;

			controller.SendRelationList();

			hornButton.interactable = false;
		}

		private void VoiceToggleEvent( bool isOn)
		{
			speakButton.gameObject.SetActive ( isOn );
			sendMessageInputField.gameObject.SetActive ( !isOn );
			expressionButton.interactable = !isOn;
			sendButton.interactable = !isOn;
			sendText.color = isOn ? Color.gray : Color.white;
		}

		#endregion

		#region InputEvent

		public void SendMessageInputFieldEvent( string text )
		{
			if( text.Length >= 30 )
			{
				MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, stringLengthPopStr, stringLengthPopTitle );
			}
		}

		#endregion

		private void ChildChange(Transform parent , string name )
		{
			for ( int i = 0; i < parent.childCount; i++ )
			{
				GameObject go = parent.GetChild ( i ).gameObject;

				if( go.name == name )
				{
					go.gameObject.SetActive ( true );
				}
				else
				{
					go.gameObject.SetActive ( false );
				}
			}
		}

		#region OpenPanel

		private void OpenHornPanel()
		{
			ChildChange ( chatMiddle , hornPanel.name );
			hornPanel.RefreshPanel ( controller.GetHornNumber () );
		}

		public void HideHornPanel()
		{
			hornPanel.gameObject.SetActive( false );
		}

		private void OpenExpressionPanel()
		{
			ChildChange ( chatMiddle ,expressionPanel.name );
		}

		private void OpenPlayerInfoPanel( ChatDataStruct chatData)
		{
			ChildChange ( playerInfoPanel.parent ,playerInfoPanel.gameObject.name );

			InfoPanelPlayerNameText.text = chatData.chatPlayerInfo.name;
			InfoPanelPlayerLevelText.text = string.Format( levelStr, chatData.chatPlayerInfo.level.ToString() );
			InfoPanelPlayerDanText.text = "最强王者";//string.Format( gradeStr, chatData.chatPlayerInfo.grade ); Waiting we have true data, use this functions.
			InfoPanelPlayerName = chatData.chatPlayerInfo.name;
			InfoPanelPlayerId = chatData.chatPlayerInfo.playerId;

			if( controller.IsFriendByPlayerId( InfoPanelPlayerId ) )
			{
				addFriendButton.interactable = false;
			}
			else
			{
				addFriendButton.interactable = true;
			}
		}

		private void OpenBuyHornPanel()
		{
			ChildChange ( chatMiddle ,buyHornPanel.gameObject.name );
			buyHornContentText.text = string.Format ( buyHornPromptStr , controller.GetHornDiamondCost () );
		}

		private void OpenBuyDiamondPanel()
		{
			ChildChange ( chatMiddle ,buyDiamondPanel.gameObject.name );
		}

		private void OpenRecordingTipPanel()
		{
			ChildChange ( chatMiddle , recordingTipPanel.gameObject.name );
		}

		private void ShowBroadcastMessage( ChatDataStruct chatData )
		{
			broadcastPanel.gameObject.SetActive( true );
			playerLevelText.text = chatData.chatPlayerInfo.level.ToString();
			playerNameText.text = chatData.chatPlayerInfo.name;
			messageText.text = chatData.message;
			GameResourceLoadManager.GetInstance().LoadAtlasSprite( chatData.chatPlayerInfo.portrait, delegate( string name, AtlasSprite atlasSprite, System.Object param )
			{
				if ( atlasSprite != null ) {
					playerIcon.SetSprite( atlasSprite);
				}
			}, true );

			isNotificate = true;
		}

		private void HideShowBroadcastMessage()
		{
			isNotificate = false;
			notificationTimer = 0;
			broadcastPanel.gameObject.SetActive( false );
		}

		private void ShowPrivateChatScrollView( long playerId ) 
		{
			privateChatScrollView.gameObject.SetActive( true );
		}

		#endregion

		public ChatMainController GetController()
		{
			if( controller != null )
			{
				return controller;
			}
			else
			{
				DebugUtils.LogError ( DebugUtils.Type.UI, string.Format( "Can't find ChatMainController" ) );
				return null;
			}
		}

		public void EnterPrivateChat( long playerId )
		{
			if( playerId != -1 && isLoadPrivateChatItemFinish )
			{
				chatType[ 1 ].isOn = true;
				ResfresMyFriendItemData( controller.GetMyFriendData(), playerId );
				controller.privateChatPlayerId = -1;
			}
		}
	}
}