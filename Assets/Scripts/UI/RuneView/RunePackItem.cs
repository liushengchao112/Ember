using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

using Resource;

namespace UI
{
	class RunePackItem : MonoBehaviour
	{
		public int icon;
		public int number;
		public int runeId;
        public ToggleGroup group;

		private Image itemImage;
		private Text itemNumberText;
       
		private Toggle itemTogger;
		public  Action<int> onClickEvent;
        
		private void Awake()
		{
			itemImage = transform.Find ( "RunePackItemImage" ).GetComponent<Image> ();           
			itemNumberText = transform.Find ( "RunePackItemText" ).GetComponent<Text> ();
            itemTogger = transform.GetComponent<Toggle>();
   
            itemTogger.AddListener(OnClickShowRuneInformationButton);            
		}

		public void RefreshItem()
		{
            itemTogger.group = group;
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
            {
                itemImage.SetSprite( atlasSprite );
            }, true );
            itemNumberText.text = number.ToString();
		}

		private void OnClickShowRuneInformationButton(bool ison)
		{
			onClickEvent ( runeId );
		}
	}
}
