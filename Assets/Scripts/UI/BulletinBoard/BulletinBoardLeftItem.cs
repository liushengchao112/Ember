using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BulletinBoardLeftItem : MonoBehaviour
    {
        public delegate void ClickBoardLeftToggle( BulletinBoardLeftItem item );
        public ClickBoardLeftToggle clickLeftToggleEvent;

        public int index;
        public string tgText;
        public BulletinBoardView.BoardType type;

        private Text toggleText;
        public Toggle clickToggle;
        private Image tgImage, tgClickImage, redPoint;

        private void Awake()
        {
            toggleText = transform.Find( "ToggleText" ).GetComponent<Text>();
            clickToggle = transform.Find( "ClickToggle" ).GetComponent<Toggle>();
            tgImage = transform.Find( "TgImage" ).GetComponent<Image>();
            tgClickImage = transform.Find( "TgClickImage" ).GetComponent<Image>();
            redPoint = transform.Find( "RedPoint" ).GetComponent<Image>();

            tgClickImage.gameObject.SetActive( false );

            clickToggle.AddListener( ClickToggleEvent );
        }

        private void ClickToggleEvent( bool isOn )
        {
            tgClickImage.gameObject.SetActive( isOn );

            if ( isOn && clickLeftToggleEvent != null )
            {
                clickLeftToggleEvent( this );
            }
        }

        public void RefreshBoardLeftItem()
        {
            switch ( type )
            {
                case BulletinBoardView.BoardType.BulletinBoard:
                    toggleText.text = tgText;
                    break;
                case BulletinBoardView.BoardType.ActivityBoard:
                    toggleText.text = "签到";
                    break;
            }
        }

        public void SetLeftRedBubble( bool state )
        {
            redPoint.gameObject.SetActive( state );
        }
    }
}