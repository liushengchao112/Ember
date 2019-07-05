using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Utils;

namespace Resource
{
    public class GGameLocalLoaderProcesser
	{
        private bool isCompleted = false;
        private IGLoaderItem item = null;
        private UnityEngine.Object loadingResource;
        private ResourceRequest resourceRequest = null;

        public GGameLocalLoaderProcesser()
        {
            
        }

        public void SetItem( IGLoaderItem item )
        {
            this.item = item;
            isCompleted = false;

            try
            {
                resourceRequest = Resources.LoadAsync( this.item.URL );
            }
            catch ( Exception )
            {
                this.item.LoadErrorHandler();
            }

        }

        public bool Update( float detlaTime )
        {
            if ( resourceRequest != null && resourceRequest.isDone )
            {
                loadingResource = resourceRequest.asset;
                isCompleted = true;

                if ( loadingResource == null )
                {
                    DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( "Load local resousrce faild, Path = {0}", this.item.URL ) );
                }
                item.LoadCompleteHandler( loadingResource );
                resourceRequest = null;
            }
            return isCompleted;
        }
	}
}
