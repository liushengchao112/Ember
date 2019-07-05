using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

using Resource;
using Utils;

namespace UI
{
	public class MatchChatView : MonoBehaviour
	{
		private MatchChatController controller;
		private Transform chatMessageGroup;
		private Transform mathChatBottom;
		private Transform mathChatMiddle;
		private Transform expressionPanel;

		private Toggle chatToggle;

		private InputField mathChatIf;

		private Button pressOnSpeakButton;
		private Button sendMessageButton;
		private Button expressionButton;
		private Button expressionMaskButton;

		private MatchChatScrollView matchChatScrollView;

		private ScrollRect matchChatScrollRect;

		private List<string> dataList = new List<string> ();

		private string selfName;

		private string chatFontLimitedTitle = "提示";
		private string chatFontLimitedStr = "最多只能发送30字符";

		[HideInInspector]
		public List<long> firendID;

		public void Init()
		{
			controller = new MatchChatController( this );
			InitComponent();
			InitListener();
			InitItem();
		}

		#region Init

		private void InitItem()
		{
			GameResourceLoadManager.GetInstance ().LoadResource ( "MatchChatItem", OnLoadMatchChatItem , true );
		}

		private void InitComponent()
		{
			mathChatMiddle = transform.Find ( "MatchChatMiddle" );
			mathChatBottom = transform.Find ( "MatchChatBottom" );

			expressionPanel = mathChatMiddle.Find ( "ExpressionPanel" );

			mathChatIf = mathChatBottom.Find ( "InputField" ).GetComponent<InputField>();

			chatToggle = mathChatBottom.Find ( "ChatToggle" ).GetComponent<Toggle> ();

			pressOnSpeakButton = mathChatBottom.Find ( "PressOnSpeakButton" ).GetComponent<Button> ();
			sendMessageButton = mathChatBottom.Find ( "SendMessageButton" ).GetComponent<Button> ();
			expressionButton = mathChatBottom.Find ( "ExpressionButton" ).GetComponent<Button> ();
			sendMessageButton = mathChatBottom.Find ( "SendMessageButton" ).GetComponent<Button> ();
			expressionMaskButton = expressionPanel.Find ( "ExpressionMaskButton" ).GetComponent<Button> ();

			matchChatScrollView = transform.Find ( "MatchChatScrollView" ).GetComponent<MatchChatScrollView> ();
			matchChatScrollRect = matchChatScrollView.GetComponent<ScrollRect> ();
			selfName = Data.DataManager.GetInstance().GetPlayerNickName();
		}

		private void InitListener()
		{
			chatToggle.AddListener ( ChatToggleEvent );

			sendMessageButton.AddListener ( SendMessageButtonEvent );
			expressionButton.AddListener ( ExpressionButtonEvent );
			expressionMaskButton.AddListener ( ExpressionMaskButtonEvent );

			mathChatIf.onValueChanged.AddListener ( MathChatInputValueCheckEvent );
		}

		#endregion

		#region ToggleEvent

		private void ChatToggleEvent( bool isOn )
		{
			if( isOn )
			{
				mathChatIf.gameObject.SetActive ( true );
				pressOnSpeakButton.gameObject.SetActive ( false );
			}
			else
			{
				mathChatIf.gameObject.SetActive ( false );
				pressOnSpeakButton.gameObject.SetActive ( true );
			}
		}

		#endregion

		#region ButtonEvent

		private void SendMessageButtonEvent()
		{
			if( !string.IsNullOrEmpty( mathChatIf.text ) )
			{
				if( firendID != null && firendID.Count > 0 )
				{
					controller.SendChatMessage( mathChatIf.text  );
				}

				string str = string.Format( selfName + " : " + mathChatIf.text );

				RefreshMatchCahtShowEffect( str );
				mathChatIf.text = "";
			}
		}

		private void ExpressionButtonEvent()
		{
			expressionPanel.gameObject.SetActive ( true );
		}

		private void ExpressionMaskButtonEvent()
		{
			expressionPanel.gameObject.SetActive ( false );
		}

		#endregion

		private void MathChatInputValueCheckEvent( string text )
		{
			if( text.Length >= 30 )
			{
				MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, chatFontLimitedStr, chatFontLimitedTitle );
			}
		}

		#region Match Item

		private void OnLoadMatchChatItem( string name, Object obj, System.Object param )
		{
			matchChatScrollView.InitDataBase( matchChatScrollRect, obj, 1, 900, 35, 0, new Vector2( 0, 35 ) );
		}
			
		#endregion

		public void RefreshMatchCahtShowEffect( string str )
		{
			dataList.Add ( str );
			matchChatScrollView.InitializeWithData ( dataList );
			matchChatScrollView.dataCount = dataList.Count;
			matchChatScrollView.GoDown ();
			matchChatScrollView.UpdateScrollView ( Vector2.zero );
		}
			
		public void SetFriendData( List<long> firendID )
		{
			this.firendID = firendID;
		}

		void OnEnable()
		{
			if( controller != null )
			{
				controller.OnEnable();
			}

			if( dataList != null )
			{
				dataList.Clear();
				matchChatScrollView.InitializeWithData ( dataList );
			}
		}

		void OnDisable()
		{
			if( controller != null )
			{
				controller.OnDisable();
			}
		}
	}
}
