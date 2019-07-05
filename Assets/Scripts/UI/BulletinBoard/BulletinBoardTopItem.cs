using UnityEngine;
using UnityEngine.UI;
using System;

namespace UI
{
    public class BulletinBoardTopItem : MonoBehaviour
    {
        public delegate void ClickBoardTopToggle( BulletinBoardTopItem item );
        public ClickBoardTopToggle clickTopToggleEvent;

        public int index;
        public string tgText;

        private Text toggleText;
        public Toggle clickToggle;
        private Image redPoint;

        private void Awake()
        {
            toggleText = transform.Find( "ToggleText" ).GetComponent<Text>();
            clickToggle = transform.Find( "ClickToggle" ).GetComponent<Toggle>();
            redPoint = transform.Find( "RedPoint" ).GetComponent<Image>();

            clickToggle.AddListener( ClickToggleEvent );
        }

        private void ClickToggleEvent( bool isOn )
        {
            clickToggle.interactable = !isOn;

            if ( isOn && clickTopToggleEvent != null )
            {
                clickTopToggleEvent( this );
            }
        }

        public void ResfresTopItem()
        {
            toggleText.text = tgText;
        }

        public void SetTopRedBubble( bool state )
        {
            redPoint.gameObject.SetActive( state );
        }
    }
}