using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class FightRecordScrollView : ScrollViewBase
    {
        public delegate void OnCreateItem( FightRecordItem item );
        public OnCreateItem onCreateItemHandler;

        public override GameObject InstantiateCell()
        {
            FightRecordItem item = base.InstantiateCell().AddComponent<FightRecordItem>();
            if ( onCreateItemHandler != null )
                onCreateItemHandler( item );
            return item.gameObject;
        }
    }
}
