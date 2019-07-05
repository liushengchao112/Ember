using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class MatchFriendScrollView : ScrollViewBase
    {
        public delegate void OnCreateItem( MatchFriendItem item );
        public OnCreateItem onCreateItemHandler;

        public override GameObject InstantiateCell()
        {
            MatchFriendItem item = base.InstantiateCell().AddComponent<MatchFriendItem>();
            if ( onCreateItemHandler != null )
                onCreateItemHandler( item );
            return item.gameObject;
        }
    }
}
