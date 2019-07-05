using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

using Utils;
using Resource;

namespace UI
{
	public class MatchChatScrollView : ScrollViewBase
	{
		public int dataCount;

		public delegate void OnCreateItem( MatchChatItem item );
		public OnCreateItem OnCreateItemHandler;

		public override void InitDataBase( ScrollRect rect , Object pre , int numberOfPerLine , float cellWidth , float cellHeight , float cellPath , Vector3 firstCellPos )
		{
			base.InitDataBase( rect , pre , numberOfPerLine , cellWidth , cellHeight , cellPath , firstCellPos );
		}
			
		public override GameObject InstantiateCell()
		{
			MatchChatItem item = base.InstantiateCell().AddComponent<MatchChatItem>();

			if ( OnCreateItemHandler != null )
			{
				OnCreateItemHandler( item );
			}
			return item.gameObject;
		}

		public override void UpdateScrollView( Vector2 vt )
		{
			base.UpdateScrollView( vt );
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