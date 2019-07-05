using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace UI
{
    public class AlertView : MonoBehaviour
    {
        private Text confirmText, cancelText, confirmAloneText, contentText, titleText;
        private Button confirmButton, cancelButton, confirmAloneButton;

        private AlertController controller;

        private Action confirmEvent;
        private AlertType alertType;
        public string content;
        public string title;

        private void Awake()
        {
            //controller = new AlertController( this );

            confirmText = transform.Find( "ConfirmText" ).GetComponent<Text>();
            cancelText = transform.Find( "CancelText" ).GetComponent<Text>();
            confirmAloneText = transform.Find( "ConfirmAloneText" ).GetComponent<Text>();
            contentText = transform.Find( "ContentText" ).GetComponent<Text>();
            titleText = transform.Find( "TitleText" ).GetComponent<Text>();
            confirmButton = transform.Find( "ConfirmButton" ).GetComponent<Button>();
            cancelButton = transform.Find( "CancelButton" ).GetComponent<Button>();
            confirmAloneButton = transform.Find( "ConfirmAloneButton" ).GetComponent<Button>();

            confirmButton.AddListener( OnClickConfirmButton );
            cancelButton.AddListener( OnClickCancelButton );
            confirmAloneButton.AddListener( OnClickConfirmAloneButton );
        }

        private void OnDestroy()
        {
            if ( controller != null )
                controller.Destroy();
        }

        public void Open( object confirmEvent, object type, object contentText, object titleText )
        {
            this.confirmEvent = ( confirmEvent ) as Action;
            alertType = (AlertType)type;
            content = contentText.ToString();
            title = titleText.ToString();

            RefreshAlertView();
        }

        #region Button Event

        private void OnClickConfirmButton()
        {
            confirmEvent();
            gameObject.SetActive( false );
        }

        private void OnClickCancelButton()
        {
            gameObject.SetActive( false );
        }

        private void OnClickConfirmAloneButton()
        {
            if ( confirmEvent != null )
                confirmEvent();
            gameObject.SetActive( false );
        }

        #endregion

        private void RefreshAlertView()
        {
            this.gameObject.SetActive( true );

            switch ( alertType )
            {
                case AlertType.ConfirmAlone:
                    confirmText.gameObject.SetActive( false );
                    cancelText.gameObject.SetActive( false );
                    confirmButton.gameObject.SetActive( false );
                    cancelButton.gameObject.SetActive( false );

                    confirmAloneText.gameObject.SetActive( true );
                    confirmAloneButton.gameObject.SetActive( true );
                    break;
                case AlertType.ConfirmAndCancel:
                    confirmAloneText.gameObject.SetActive( false );
                    confirmAloneButton.gameObject.SetActive( false );

                    confirmText.gameObject.SetActive( true );
                    cancelText.gameObject.SetActive( true );
                    confirmButton.gameObject.SetActive( true );
                    cancelButton.gameObject.SetActive( true );
                    break;
                default:
                    break;
            }

            titleText.text = title;
            contentText.text = content;
        }
    }
}
