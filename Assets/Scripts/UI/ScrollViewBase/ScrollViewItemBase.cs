using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UI
{
    public class ScrollViewItemBase : MonoBehaviour
    {
        public virtual void Init() { }

        public virtual void UpdateItemData( object dataObj )
        {
            if ( dataObj == null )
            {
                gameObject.SetActive( false );
            }
            else
            {
                gameObject.SetActive( true );
            }
        }
    }
}