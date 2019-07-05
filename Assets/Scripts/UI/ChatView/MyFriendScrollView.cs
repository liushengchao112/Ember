using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UI
{
	public class MyFriendScrollView : ScrollViewBase
	{
		public delegate void OnCreateItem( MyFriendItem item );
		public OnCreateItem OnCreateItemHandler;

		List<MyFriendItem> myFriendItemList  = new List<MyFriendItem>();

		public override void InitDataBase(UnityEngine.UI.ScrollRect rect, Object pre, int numberOfPerLine, float cellWidth, float cellHeight, float cellPath, Vector3 firstCellPos)
		{
			base.InitDataBase (rect, pre, numberOfPerLine, cellWidth, cellHeight, cellPath, firstCellPos);
		}

		public override GameObject InstantiateCell()
		{
			MyFriendItem item = base.InstantiateCell().AddComponent<MyFriendItem>();
			myFriendItemList.Add ( item );
			if ( OnCreateItemHandler != null )
			{
				OnCreateItemHandler( item );
			}
			return item.gameObject;
		}

		public override void UpdateScrollView(Vector2 vt)
		{
			base.UpdateScrollView (vt);
		}

		public MyFriendItem FindMyFriendItemByPlayerId( long playerId )
		{
			foreach ( var item in myFriendItemList )
			{
				if( item.gameObject.activeSelf && item.GetPlayerId() == playerId )
				{
					return item;
				}
			}

			return  myFriendItemList.Find( p => p.GetPlayerId() == playerId );
		}
	}
}