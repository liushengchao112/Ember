using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class DetailedRecordScrollView : ScrollViewBase
    {
        public delegate void OnCreateItem( DetailedRecordItem item );
        public OnCreateItem onCreateItemHandler;

        public override GameObject InstantiateCell()
        {
            DetailedRecordItem item = base.InstantiateCell().AddComponent<DetailedRecordItem>();
            if ( onCreateItemHandler != null )
                onCreateItemHandler( item );
            return item.gameObject;
        }
    }
}
