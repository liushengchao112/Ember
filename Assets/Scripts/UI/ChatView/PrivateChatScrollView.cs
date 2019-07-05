using UnityEngine;
using UnityEngine.UI;
using Utils;
using System.Collections.Generic;
using Resource;
using System.Collections;

namespace UI
{
	public class PrivateChatScrollView : ScrollViewBase
	{
		public delegate void OnCreateItem( PrivateChatItem item );
		public OnCreateItem OnCreateItemHandler;

		public int dataCount;

		public override GameObject InstantiateCell()
		{
			PrivateChatItem item = base.InstantiateCell().AddComponent<PrivateChatItem>();
			if ( OnCreateItemHandler != null )
			{
				OnCreateItemHandler( item );
			}
			return item.gameObject;
		}

		public override void GoDown()
		{
			if( dataCount >= VisibleCellsRowCount )
			{
				SetContentMinimumPostion ();
			}
		}
	}
}