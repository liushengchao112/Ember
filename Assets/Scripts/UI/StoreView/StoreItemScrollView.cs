using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class StoreItemScrollView : ScrollViewBase
    {
        public delegate void OnCreateItem( StoreItem item );
        public OnCreateItem onCreateItemHandler;

        public override GameObject InstantiateCell()
        {
            StoreItem item = base.InstantiateCell().AddComponent<StoreItem>();
            if ( onCreateItemHandler != null )
                onCreateItemHandler( item );
            return item.gameObject;
        }
    }
}
