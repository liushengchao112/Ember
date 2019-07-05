using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace UI
{
	public class HornPanel : MonoBehaviour
	{
		private Button sendButton, exitButton;
		private InputField hornInputField;
		private Text remainText, hornNumberText;
		public Action<string> sendHornMessageOnClick;

		private string remainStr = "你还可以输入{0}个文字";

		private int maxTextLength = 30;

		public void Awake()
		{
			sendButton = transform.Find ( "SendButton" ).GetComponent<Button> ();
			exitButton = transform.Find ( "ExitButton" ).GetComponent<Button> ();

			hornInputField = transform.Find ( "HornInputField" ).GetComponent<InputField> ();
			hornNumberText = transform.Find ( "HornNumberText" ).GetComponent<Text> ();
			remainText = transform.Find ( "RemainText" ).GetComponent<Text> ();

			sendButton.AddListener ( SendButtonEvent );
			exitButton.AddListener ( ExitButtonEvent );
			hornInputField.onValueChanged.AddListener ( ContentChangedEvent );
		}

		private void ContentChangedEvent( string text )
		{
			int remain = maxTextLength -text.Length;
			remainText.text = string.Format ( remainStr , remain );
		}

		public void RefreshPanel( int  number )
		{
			hornNumberText.text = "X" + number.ToString ();
		}

		private void SendButtonEvent()
		{
			if( !string.IsNullOrEmpty( hornInputField.text ) && sendHornMessageOnClick != null)
			{
				sendHornMessageOnClick( hornInputField.text );
			}	
		}

		private void ExitButtonEvent()
		{
			gameObject.SetActive ( false );
		}
	}
}