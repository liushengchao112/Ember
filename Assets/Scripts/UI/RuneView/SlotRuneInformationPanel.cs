using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

using Data;
using Resource;
using Utils;

namespace UI
{
	public class SlotRuneInformationPanel : MonoBehaviour
	{
		public string runeName;

		public int level;
		public int icon;
		public string attribute;

		private Transform runeInformationGroup;

		private Text runeNameText, runeLevelText, attributeText;

		private Image runeIcon;

		public void Awake()
		{
			runeIcon = transform.Find ("IconBackGround/RuneInformationIcon").GetComponent<Image> ();
			attributeText = transform.Find ("ContentText").GetComponent<Text> ();
			runeNameText = transform.Find ("IconBackGround/RuneNameText").GetComponent<Text> ();
			runeLevelText = transform.Find ("IconBackGround/RuneLevelText").GetComponent<Text> ();
		}

		public void RefreshPanel()
		{
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
            {
                runeIcon.SetSprite( atlasSprite );
            }, true );

            runeNameText.text = runeName;
			runeLevelText.text = level + "级";
			attributeText.text = attribute;
		}
	}
}

