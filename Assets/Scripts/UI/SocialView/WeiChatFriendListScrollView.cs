using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class WeiChatFriendListScrollView : ScrollViewBase 
	{
		public delegate void OnCreateItem( WeiChatFriendListItem item );
		public OnCreateItem onCreateItemHandler;

		public override GameObject InstantiateCell()
		{
			WeiChatFriendListItem item = base.InstantiateCell().AddComponent<WeiChatFriendListItem>();

			if( onCreateItemHandler != null )
			{
				onCreateItemHandler( item );
			}

			return item.gameObject;
		}
	}
}
