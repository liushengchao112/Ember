using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BulletinBoardActivityItem : MonoBehaviour
    {
        public delegate void ClickActivityButton( BulletinBoardActivityItem item );
        public ClickActivityButton clickActivityButtonEvent;

        public int index;
        public string itemImage;

        private Button activityButton;
        private Image activityImage;

        private void Awake()
        {
            activityImage = transform.Find( "ActivityButton" ).GetComponent<Image>();
            activityButton = transform.Find( "ActivityButton" ).GetComponent<Button>();

            activityButton.AddListener( ClickButton );
        }

        private void ClickButton()
        {
            clickActivityButtonEvent( this );
        }
    }
}
