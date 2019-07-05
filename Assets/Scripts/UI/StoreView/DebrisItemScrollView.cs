using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class DebrisItemScrollView : ScrollViewBase
    {
        public delegate void OnCreateItem( DebrisItem item );
        public OnCreateItem onCreateItemHandler;

        public override GameObject InstantiateCell()
        {
            DebrisItem item = base.InstantiateCell().AddComponent<DebrisItem>();
            if ( onCreateItemHandler != null )
                onCreateItemHandler( item );
            return item.gameObject;
        }
    }
}