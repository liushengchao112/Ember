using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MailScrollView : ScrollViewBase
    {
        public delegate void OnCreateItem( MailItem item );
        public OnCreateItem onCreateItemHandler;

        public override GameObject InstantiateCell()
        {
            MailItem item = base.InstantiateCell().AddComponent<MailItem>();
            if ( onCreateItemHandler != null )
                onCreateItemHandler( item );
            return item.gameObject;
        }
    }
}
