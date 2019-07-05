using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using Resource;

namespace UI
{
	public class MatchChatItem : ScrollViewItemBase
	{
		private object data;

		public System.Action<int> onClickItemHandle;

		private InlineText messageText;

		private Button itemButton;

		void Awake()
		{	
			messageText = transform.Find ( "MessageText" ).GetComponent<InlineText> ();
		}

		public override void UpdateItemData( object dataObj )
		{
			base.UpdateItemData ( dataObj );

			this.data = dataObj;

			messageText.text = this.data + "";
		}

		private void ItemButtonEvent()
		{
			if( onClickItemHandle != null)
			{
				onClickItemHandle ( ( int ) data );
			}
		}
	}
}

