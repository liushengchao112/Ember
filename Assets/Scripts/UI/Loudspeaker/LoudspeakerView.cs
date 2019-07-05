using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using Data;

namespace UI
{
    public class LoudspeakerView : MonoBehaviour
    {
        private LoudspeakerControler controler;

        private string remainStr = "你还可以输入{0}个文字";

        private readonly int maxTextLength = 30;

        private Transform HornPanel;
        private Button sendButton, exitButton;
        private InputField hornInputField;
        private Text remainText, hornNumberText;

        private void Awake()
        {            
            controler = new LoudspeakerControler( this );           

            HornPanel = transform.Find( "HornPanel" );
            sendButton = HornPanel.Find( "SendButton" ).GetComponent<Button>();
            exitButton = HornPanel.Find( "ExitButton" ).GetComponent<Button>();
            hornInputField = HornPanel.Find( "HornInputField" ).GetComponent<InputField>();
            hornNumberText = HornPanel.Find( "HornNumberText" ).GetComponent<Text>();
            remainText = HornPanel.Find( "RemainText" ).GetComponent<Text>();

            sendButton.AddListener( SendButtonEvent );
            exitButton.AddListener( ExitButtonEvent );
            hornInputField.onValueChanged.AddListener( ContentChangedEvent );

        }

        void OnDestroy()
        {
           
        }

        public void ShowLoudspeakerPanel()
        {
            ResetLoudspeakerCount();
            gameObject.SetActive( true );
        }

        private void ResetLoudspeakerCount()
        {
            hornNumberText.text = "X" + controler.GetLoudspeakerCount().ToString();
        }

        private void ContentChangedEvent( string text )
        {
            int remain = maxTextLength - text.Length;
            remainText.text = string.Format( remainStr , remain );
        }

        private void SendButtonEvent()
        {
            if ( !string.IsNullOrEmpty( hornInputField.text ) )
            {
                controler.SendChatMessage( ChatType.WorldChat , hornInputField.text , controler.GetPlayerId() , ChatConsumptionType.ChatItem );
            }
        }

        public void ExitButtonEvent()
        {
            gameObject.SetActive( false );
        }
    }
}