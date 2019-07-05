using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class BlackListScrollView : ScrollViewBase
	{
		public delegate void OnCreatItem( BlackListItem item );
		public OnCreatItem onCreatItemHandler;

		public override GameObject InstantiateCell()
		{
			BlackListItem item = base.InstantiateCell().AddComponent<BlackListItem>();

			if( onCreatItemHandler != null )
			{
				onCreatItemHandler( item );
			}

			return item.gameObject;
		}
	}
}
