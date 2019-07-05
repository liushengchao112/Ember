using UnityEngine;
using System.Collections;

namespace Resource
{

    public class GAssetBundleData : GAbstractData
    {
        protected byte[] _rawData;
        public byte[] RawData
        {
            get
            {
                return _rawData;
            }
        }

        protected bool _bundleRequestInited;
        protected AssetBundle _assetBundle;
        protected AssetBundleCreateRequest _bundleRequest;

        public override void Init( string name, bool useLocal, System.Object param )
        {
            base.Init( name, useLocal, param );

            _bundleRequestInited = false;
            _bundleRequest = null;
            _assetBundle = null;
        }

        //NO CHINESE!
        /// <summary>
        /// 更新下载进度
        /// </summary>
        /// <returns>是否已经可以使用</returns>
        public override bool DataReadyToUse()
        {
            if ( !mainDataReadyToUse && _bundleRequestInited )
            {
                if ( _bundleRequest != null )
                {
                    if ( _bundleRequest.isDone )
                    {
                        LoadAssetBundleCompletedHandler( _bundleRequest.assetBundle );
                    }
                }
                else
                {
                    LoadErrorHandler();
                }
            }

            return mainDataReadyToUse;
        }

        public bool mainDataReadyToUse
        {
            get
            {
                return ( _assetBundle != null );
            }
        }

        public System.Object GetDataByName( string dataName )
        {
            if ( mainDataReadyToUse )
            {
                return _assetBundle.LoadAsset( dataName );
            }
            else
            {
                return null;
            }
        }

        public T GetDataByName<T>( string dataName ) where T : Object
        {
            if ( mainDataReadyToUse )
            {
                return _assetBundle.LoadAsset( dataName, typeof( T ) ) as T;
            }
            else
            {
                return null;
            }
        }

        public void LoadAssetBundleCompletedHandler( AssetBundle assetBundle )
        {
            _assetBundle = assetBundle;

            if ( _assetBundle == null )
            {
                LoadErrorHandler();
            }
        }

        public override void LoadErrorHandler()
        {
            base.LoadErrorHandler();
            _assetBundle = null;
        }

        public override void LoadCompleteHandler( System.Object data )
        {
            if ( _data == null )
            {
                base.LoadCompleteHandler( data );
            }

            if ( data is AssetBundle )
            {
                _assetBundle = data as AssetBundle;
            }
            else if ( data is byte[] )
            {
                _rawData = data as byte[];
            }
            else
            {
                LoadErrorHandler();
            }
        }

        public virtual void OnInstantiate()
        {

        }
        public virtual void OnDestroy()
        {

        }
    }
}
