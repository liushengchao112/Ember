using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class FindFriendListScrollView : ScrollViewBase
	{
		public delegate void OnCreateItem( FindFriendItem item );
		public OnCreateItem onCreateItemHandler;

		public override GameObject InstantiateCell()
		{
            FindFriendItem item = base.InstantiateCell().AddComponent<FindFriendItem>();

			if( onCreateItemHandler != null )
			{
				onCreateItemHandler( item );
			}
			return item.gameObject;
		}
	}
}
