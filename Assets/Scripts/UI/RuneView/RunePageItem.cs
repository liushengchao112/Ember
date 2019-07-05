using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class RunePageItem : MonoBehaviour
	{
		public  Action<int> onClickEvent;
		public int runePageId;
		public int runePageName;

		private Text runePageItemText;
		public Toggle runePageItemToggle;

		private void Awake()
		{			
			runePageItemToggle = transform.GetComponent<Toggle> ();
			runePageItemToggle.AddListener ( OnClickRunePageItemToggle );
		}

		private void OnClickRunePageItemToggle( bool on )
		{
			runePageItemToggle.interactable = !on;
			if( on )
            { 
				onClickEvent ( runePageId );
			}
		}
	}
}
