using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class FriendListScrollView : ScrollViewBase
	{
		public delegate void OnCreateItem( FriendListItem item );
		public OnCreateItem onCreateItemHandler;

		public override GameObject InstantiateCell()
		{
			FriendListItem item = base.InstantiateCell().AddComponent<FriendListItem>();

			if( onCreateItemHandler != null )
			{
				onCreateItemHandler( item );
			}
			return item.gameObject;
		}
	}
}
