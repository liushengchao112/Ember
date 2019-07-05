using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

using Data;
using Resource;
using Utils;

namespace UI
{
	public class RuneInformationPanel : MonoBehaviour
	{
		public string runeName;
		public int runeNumber;
		public int level;
		public int icon;
		public string content;
        public int runeType;

		private Text runeNameText, runeLevelText, runeNumberText, contentText;

		private Image runeIcon;

		public void Awake()
		{          
            runeIcon = transform.Find ("IconBackGround/RuneInformationIcon").GetComponent<Image> ();
			runeNameText = transform.Find ("IconBackGround/RuneNameText").GetComponent<Text> ();
			runeLevelText =transform.Find ("IconBackGround/RuneLevelText").GetComponent<Text> ();
			contentText = transform.Find ("ContentText").GetComponent<Text>();
		}

		public void RefreshPanel()
		{          
            GameResourceLoadManager.GetInstance().LoadAtlasSprite(icon, delegate (string name, AtlasSprite atlasSprite, System.Object param)
            {
                runeIcon.SetSprite(atlasSprite);
            }, true);

            runeNameText.text = runeName;
			runeLevelText.text = level + "级";
			contentText.text = content;			
		}       
	}
}

