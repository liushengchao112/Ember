using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StorePopUpGiveItem : MonoBehaviour
    {
		public delegate void OnClickGiveButton( long playerId );

		public int index;
		public string icon;
        public string nameStr;
		public long playerId;
        public bool moreThen10LV;
        public OnClickGiveButton ClickGiveBtEvent;

        private Image iconImage;
        private Text nameText, giveBtText;
        private Button giveButton;

        private void Awake()
        {
            iconImage = transform.Find( "IconImage" ).GetComponent<Image>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            giveBtText = transform.Find( "GiveBtText" ).GetComponent<Text>();
            giveButton = transform.Find( "GiveButton" ).GetComponent<Button>();
			giveButton.AddListener( OnClickGiveBt );
        }

        private void OnClickGiveBt()
        {
			ClickGiveBtEvent.Invoke( playerId );
        }

        public void RefreshItem()
        {
			if (string.IsNullOrEmpty( icon ) )
            {
                Resource.GameResourceLoadManager.GetInstance().LoadAssetAsync<GameObject>( icon, ( GameObject go ) =>
                {
                    SpriteRenderer spritePrefab = go.GetComponent<SpriteRenderer>();
                    iconImage.sprite = spritePrefab.sprite;
                } );
            }

            giveButton.interactable = moreThen10LV;
            nameText.text = nameStr;
        }
    }
}