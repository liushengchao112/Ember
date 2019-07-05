using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

using Data;
using Resource;
using Utils;

namespace UI
{
	public class RuneAvailableItem : MonoBehaviour
	{
		public string runeName;
		public int level;
		public int icon;
		public int runeId;
		public string attribute;
		public int openType;
		public Action<int> onClickEvent;

		private Button runeEquipButton;
		private Text runeNameAndLeverText, attributeText;
		private Image runeIcon;

		public void Awake()
		{
			attributeText = transform.Find ( "RuneAttributeText" ).GetComponent<Text> ();
			runeIcon = transform.Find ( "RuneAvailableIcon" ).GetComponent<Image> ();
			runeNameAndLeverText = transform.Find ("NameAndLevel").GetComponent<Text> ();
            runeEquipButton = transform.GetComponent<Button>();
            runeEquipButton.AddListener ( OnClickRuneEquipButton );
		}

		public void RefreshItem()
		{
			onClickEvent = null;
			attributeText.text = attribute;
            runeNameAndLeverText.text = string.Format("{0}" + "级" + "  {1}", level, runeName); 
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
            {
                runeIcon.SetSprite( atlasSprite );
            }, true );
        }

		private  void OnClickRuneEquipButton()
		{
			onClickEvent ( runeId );
		}
	}
}