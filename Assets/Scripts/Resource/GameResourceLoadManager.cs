using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Utils;
using Data;
using Constants;

namespace Resource
{
    public class GameResourceLoadManager : MonoBehaviour
    {
        #region Thread-safety singleton
        private volatile static GameResourceLoadManager _instance = null;
        private static readonly Object _lockHelper = new Object();

        public static GameResourceLoadManager GetInstance()
        {
            if ( _instance == null )
            {
                lock ( _lockHelper )
                {
                    if ( _instance == null )
                    {
                        GameObject go = GameObject.Find( "Boot" );
                        if( go != null )
                        {
                            _instance = go.GetComponent<GameResourceLoadManager>();
                        }
                    }
                }
            }
            return _instance;
        }
        #endregion

        private Dictionary<string, GAbstractData> _dataMap = new Dictionary<string, GAbstractData>(); // Loaded resource cache
        private List<GAbstractData> _loadingResList = new List<GAbstractData>();
        private List<GAbstractData> _unloadDataList = new List<GAbstractData>();
        private List<ResourcesProto.Resources> resourceList;
        private AssetBundleManager abManager;
   
        private void Awake()
        {
            _instance = this;
            _listnerQueue = new List<IGDataListener>();
        }

        public void Startup( AssetBundleManager abManager )
        {
            GGameDataLoader.GetInstance().Startup();
            resourceList = DataManager.GetInstance().resourcesProtoData;
            this.abManager = abManager;
        }

        public void Terminate( System.Object param = null )
        {
            GGameDataLoader.GetInstance().Terminate();
            Unload();
        }

#region new api
        public T LoadAsset<T>( int id ) where T : UnityEngine.Object
        {
            ResourcesProto.Resources res = GetResourcePath( id );
            if ( res == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( "Id = {0} can't be find in ResourceTableProto! Please check!", id ) );
                return null;
            }
            return LoadAsset<T>( res );
        }

        public T LoadAsset<T>( string assetName ) where T : UnityEngine.Object
        {
            ResourcesProto.Resources res = GetResourcePath( assetName );
            if ( res == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( "assetName = {0} can't be find in ResourceTableProto! Please check!", assetName ) );
                return null;
            }
            return LoadAsset<T>( res );
        }

        public T LoadAsset<T>( ResourcesProto.Resources res ) where T : UnityEngine.Object
        {
            if ( res.BunldeName.Equals( "null" ) )
            {
                return Resources.Load<T>( res.ResourcePath );
            }
            else
            {
#if UNITY_EDITOR
                if ( GameConstants.LoadAssetByEditor )
                {
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<T>( res.ResourcePath );
                }
#endif
                return abManager.GetAsset<T>( res.BunldeName.ToLower() + GameConstants.BundleExtName, res.Name );
            }
        }

        public void LoadAssetAsync<T>( string assetName, Callback<T> callBack = null ) where T : UnityEngine.Object
        {
            ResourcesProto.Resources res = GetResourcePath( assetName );
            if ( res == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( "assetName = {0} can't be find in ResourceTableProto! Please check!", assetName ) );
                if ( callBack != null )
                {
                    callBack( null );
                }
                return;
            }
            LoadAssetAsync( res, callBack );
        }

        public void LoadAssetAsync<T>( int id, Callback<T> callBack = null ) where T : UnityEngine.Object
        {
            ResourcesProto.Resources res = GetResourcePath( id );
            if ( res == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( "id = {0} can't be find in ResourceTableProto! Please check!", id ) );
                if ( callBack != null )
                {
                    callBack( null );
                }
                return;
            }
            LoadAssetAsync( res, callBack );
        }

        public void LoadAssetAsync<T, U>( string assetName, Callback<T, U> callBack, U data ) where T : UnityEngine.Object
        {
            ResourcesProto.Resources res = GetResourcePath( assetName );
            if ( res == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( "Id = {0} can't be find in ResourceTableProto! Please check!", assetName ) );
                if ( callBack != null )
                {
                    callBack( null, data );
                }
                return;
            }
            LoadAssetAsync( res, delegate ( T t )
            {
                if ( callBack != null )
                {
                    callBack( t, data );
                }
            } );
        }

        public void LoadAssetAsync<T, U>( int id, Callback<T, U> callBack, U data ) where T : UnityEngine.Object
        {
            ResourcesProto.Resources res = GetResourcePath( id );
            if ( res == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( "Id = {0} can't be find in ResourceTableProto! Please check!", id ) );
                if ( callBack != null )
                {
                    callBack( null, data );
                }
                return;
            }
            LoadAssetAsync( res, delegate ( T t )
            {
                if( callBack != null )
                {
                    callBack( t, data );
                }
            } );
        }

        public void LoadAssetAsync<T, U, V>( int id, Callback<T, U, V> callBack, U data1, V data2 ) where T : UnityEngine.Object
        {
            ResourcesProto.Resources res = GetResourcePath( id );
            if ( res == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( "Id = {0} can't be find in ResourceTableProto! Please check!", id ) );
                if ( callBack != null )
                {
                    callBack( null, data1, data2 );
                }
                return;
            }
            LoadAssetAsync( res, delegate ( T t )
            {
                if ( callBack != null )
                {
                    callBack( t, data1, data2 );
                }
            } );
        }

        public void LoadAssetAsync<T, U, V>( string assetName, Callback<T, U, V> callBack, U data1, V data2 ) where T : UnityEngine.Object
        {
            ResourcesProto.Resources res = GetResourcePath( assetName );
            if ( res == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( "assetName = {0} can't be find in ResourceTableProto! Please check!", assetName ) );
                if ( callBack != null )
                {
                    callBack( null, data1, data2 );
                }
                return;
            }
            LoadAssetAsync( res, delegate ( T t )
            {
                if ( callBack != null )
                {
                    callBack( t, data1, data2 );
                }
            } );
        }

        public void LoadAssetAsync<T>( ResourcesProto.Resources res, Callback<T> callBack ) where T : UnityEngine.Object
        {
            if ( res.BunldeName.Equals( "null" ) )
            {
                LoadResourceByPath( res.ResourcePath, delegate ( string name, GAbstractData gAbstractData, System.Object param )
                {
                    if ( callBack != null )
                    {
                        callBack( ( T )gAbstractData.Data );
                    }
                }, true );
            }
            else
            {
#if UNITY_EDITOR
                if ( GameConstants.LoadAssetByEditor )
                {
                    if( callBack != null )
                    {
                        callBack( UnityEditor.AssetDatabase.LoadAssetAtPath<T>( res.ResourcePath ) );
                    }
                    return;
                }
#endif
                abManager.GetAssetAsync( res.BunldeName.ToLower() + GameConstants.BundleExtName, res.Name,
                    delegate ( Object asset )
                    {
                        if ( callBack != null )
                        {
                            callBack( ( T )asset );
                        }
                    } );
            }
        }
        
        #endregion
        public UI.AtlasSprite LoadAtlasSprite( string name )
        {
            GameObject go = LoadAsset<GameObject>( name );
            UI.AtlasSprite sprite = null;
            if ( go != null )
            {
                sprite = go.GetComponent<UI.AtlasSprite>();
            }
            return sprite;
        }

        public UI.AtlasSprite LoadAtlasSprite( int id )
        {
            GameObject go = LoadAsset<GameObject>( id );
            UI.AtlasSprite sprite = null;
            if ( go != null)
            {
                sprite = go.GetComponent<UI.AtlasSprite>();
            }
            return sprite;
        }

        public void LoadAtlasSprite( int id, OnLoadedAtlasSpriteComplete onComplete, System.Object param = null )
        {
            ResourcesProto.Resources res = GetResourcePath( id );
            if ( res == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( "id = {0} can't be find in ResourceTableProto! Please check!", id ) );
                return;
            }
            LoadAtlasSprite( res, onComplete, param );
        }

        public void LoadAtlasSprite( string name, OnLoadedAtlasSpriteComplete onComplete, System.Object param = null )
        {
            ResourcesProto.Resources res = GetResourcePath( name );
            if ( res == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( "name = {0} can't be find in ResourceTableProto! Please check!", name ) );
                return;
            }
            LoadAtlasSprite( res, onComplete, param );
        }

        public void LoadAtlasSprite( ResourcesProto.Resources res, OnLoadedAtlasSpriteComplete onComplete, System.Object param = null )
        {
            LoadAssetAsync<GameObject>( res, delegate ( GameObject go )
              {
                  if( go == null )
                  {
                      onComplete( res.Name, null, param );
                      DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( " Load Unity.Object failed! path : {0}", res.ResourcePath ) );
                  }
                  else
                  {
                      UI.AtlasSprite atlasSprite = go.GetComponent<UI.AtlasSprite>();
                      if ( atlasSprite != null )
                      {
                          onComplete( res.Name, atlasSprite, param );
                      }
                      else
                      {
                          onComplete( res.Name, null, param );
                          DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( " Load Unity.Object failed! path : {0}", res.ResourcePath ) );
                      }
                  }
              } );
        }
        
        [System.Obsolete( "Use GameResourceLoadManager.LoadAssetAsync" )]
        public void LoadResource( string name, OnLoadedObjectComplete onComplete, System.Object param = null )
        {
            LoadAssetAsync<UnityEngine.Object>( name, delegate ( UnityEngine.Object go )
              {
                  if ( go != null )
                  {
                      onComplete( name, go, param );
                  }
                  else
                  {
                      onComplete( name, null, param );
                      DebugUtils.LogError( DebugUtils.Type.Resource, string.Format( " Load Unity.Object failed! path : {0}", name ) );
                  }
              } );
        }

        /// <summary>
        /// Load raw resource by path
        /// </summary>
        /// <param name="path"> resource path </param>
        /// <param name="onComplete"> callback function when resource loaded </param>
        /// <param name="useLocal"> true: use file from Resource folder, false: use assetBundle </param>
        /// <param name="param"></param>
        private void LoadResourceByPath( string path, OnLoadedComplete onComplete, bool useLocal, System.Object param = null )
        {
            if ( !string.IsNullOrEmpty( path ) )
            {
                GAbstractData waitLoadResource = null;

                if ( _dataMap.ContainsKey( path ) )
                {
                    waitLoadResource = _dataMap[path];
                    waitLoadResource.AddRef();
                    waitLoadResource.AddDataHandler( onComplete );
                }
                else
                {
                    if ( useLocal )
                    {
                        waitLoadResource = new GGameLocalData();
                    }
                    else
                    {
                        waitLoadResource = new GAssetBundleData();
                    }

                    _dataMap.Add( path, waitLoadResource );
                    _loadingResList.Add( waitLoadResource );

                    waitLoadResource.AddRef();
                    waitLoadResource.AddDataHandler( onComplete );
                    waitLoadResource.Init( path, useLocal, param );
                }

            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Resource, "GetGameData Can't input the null path!" );
            }
        }

        public void ReleaseGameData( string name )
        {
            if ( !_dataMap.ContainsKey( name ) )
            {
                return;
            }

            GAbstractData tempData = _dataMap[name];
            if ( tempData != null )
            {
                tempData.RemoveRef();
                if ( tempData.GetRefCount() <= 0 )
                {
                    tempData.Unload();
                }
            }
        }

        public void ReleaseAsset( int id )
        {
            ResourcesProto.Resources rr = GetResourcePath( id );
            ReleaseAsset( rr );
        }

        public void ReleaseAsset( string assetName )
        {
            ResourcesProto.Resources rr = GetResourcePath( assetName );
            ReleaseAsset( rr );
        }

        public void ReleaseAsset( ResourcesProto.Resources res )
        {
            if ( res != null )
            {
                abManager.ReleaseAsset( res.BunldeName.ToLower() + GameConstants.BundleExtName, res.Name );
            }
        }

        public void UnloadBundle( int id )
        {
            ResourcesProto.Resources rr = GetResourcePath( id );
            ReleaseAsset( rr );
        }

        public void UnloadBundle( string assetName )
        {
            ResourcesProto.Resources rr = GetResourcePath( assetName );
            UnloadBundle( rr );
        }

        public void UnloadBundle( ResourcesProto.Resources res )
        {
            if ( res != null )
            {
                abManager.UnloadBundle( res.BunldeName.ToLower() + GameConstants.BundleExtName );
            }
        }

        public void Update()
        {
            FrameUpdate( Time.deltaTime );
        }

        public void FrameUpdate( float deltaTime )
        {
            for ( int i = 0; i < _loadingResList.Count; )
            {
                GAbstractData tempData = _loadingResList[i];

                if ( tempData.Update( deltaTime ) )
                {
                    _loadingResList.RemoveAt( i );
                    DataCompletedHandler( tempData );
                }
                else
                {
                    ++i;
                }

                // don't need progress for now
                //dataProgressHandler( tempData );
            }

            GGameDataLoader.GetInstance().FrameUpdate( deltaTime );
        }

#region resource listener

        private List<IGDataListener> _listnerQueue;

        public void AddDataListener( IGDataListener listener )
        {
            _listnerQueue.Add( listener );
        }

        public void RemoveDataListener( IGDataListener listener )
        {
            _listnerQueue.Remove( listener );
        }

        public void DataCompletedHandler( GAbstractData gameData )
        {
            if ( gameData != null )
            {
                if ( gameData.error )
                {
                    foreach ( IGDataListener listener in _listnerQueue )
                    {
                        listener.dataError( gameData.ResName );
                    }
                }
                else
                {
                    foreach ( IGDataListener listener in _listnerQueue )
                    {
                        listener.dataLoaded( gameData.ResName );
                    }
                }
            }
        }

        public void DataProgressHandler( string name )
        {
            foreach ( IGDataListener listener in _listnerQueue )
            {
                listener.DataProgress( name );
            }
        }

#endregion

        public void Unload()
        {
            foreach ( GAbstractData data in _unloadDataList )
            {
                if ( !GGameDataLoader.GetInstance().isInProcesser( data ) )
                {
                    data.Unload();
                }
            }
            _unloadDataList.Clear();
        }

        public void AddUnloadData( GAbstractData data )
        {
            GAbstractData resData = _unloadDataList.Find(
                delegate ( GAbstractData tmpData )
                {
                    return tmpData.ResName == data.ResName;
                }
             );

            if ( resData == null )
            {
                _unloadDataList.Add( data );
            }
        }

        public void RemoveUnloadData( GAbstractData data )
        {
            _unloadDataList.Remove( data );
        }

        public bool DataExist( string name )
        {
            return _dataMap.ContainsKey( name );
        }

        public GAbstractData GetGameData( string name )
        {
            if ( _dataMap.ContainsKey( name ) )
            {
                return _dataMap[name];
            }

            return null;
        }

        // Utils
        public ResourcesProto.Resources GetResourcePath( string assetName )
        {
            ResourcesProto.Resources r = null;
            for ( int i = 0; i < resourceList.Count; i++ )
            {
                if ( resourceList[i].Name == assetName )
                {
                    r = resourceList[i];
                    break;
                }
            }
            return r;
        }

        private ResourcesProto.Resources GetResourcePath( int id )
        {
            ResourcesProto.Resources r = null;
            for ( int i = 0; i < resourceList.Count; i++ )
            {
                if ( resourceList[i].ID == id )
                {
                    r = resourceList[i];
                    break;
                }
            }
            return r;
        }
    }
}

