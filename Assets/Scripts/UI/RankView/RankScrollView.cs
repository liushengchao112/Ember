using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RankScrollView : ScrollViewBase
    {
        public delegate void OnCreateItem( RankViewItem item );
        public OnCreateItem OnCreateItemHandler;

        public override void InitDataBase( ScrollRect rect , Object pre , int numberOfPerLine , float cellWidth , float cellHeight , float cellPath , Vector3 firstCellPos )
        {
            base.InitDataBase( rect , pre , numberOfPerLine , cellWidth , cellHeight , cellPath , firstCellPos );
        }

        public override GameObject InstantiateCell()
        {
            RankViewItem item = base.InstantiateCell().AddComponent<RankViewItem>();
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
    }
}


