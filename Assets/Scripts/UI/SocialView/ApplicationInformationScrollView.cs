using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class ApplicationInformationScrollView:ScrollViewBase
	{
		public delegate void OnCreateItem( ApplicationInformationItem item );
		public OnCreateItem onCreateItemHandler;

		public override GameObject InstantiateCell()
		{
			ApplicationInformationItem item = base.InstantiateCell().AddComponent<ApplicationInformationItem>();

			if( onCreateItemHandler != null )
			{
				onCreateItemHandler( item );
			}

			return item.gameObject;
		}
	}
}
