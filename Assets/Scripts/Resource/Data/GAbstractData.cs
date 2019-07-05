using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

using Utils;

namespace Resource
{
    public delegate void OnLoadedComplete( string name, GAbstractData data, System.Object param );
    public delegate void OnLoadedObjectComplete( string name, UnityEngine.Object data, System.Object param );
    public delegate void OnLoadedSpriteComplete( string name, Sprite data, System.Object param );
    public delegate void OnLoadedAtlasSpriteComplete( string name, UI.AtlasSprite data, System.Object param = null );


    public class GAbstractData : IGLoaderItem
    {
        private bool isLocalResource = true;

        protected System.Object _data;
        protected System.Object _param;

        private bool _assetLoadError;  
        private int _refCount = 0;

        private int _dataLength;
        public int DataLength
        {
            get
            {
                return _dataLength;
            }
        }

        private string _resName = "";
        public virtual string ResName
        {
            get
            {
                return _resName;
            }
        }

        private string _filePath = "";
        public string FilePath
        {
            get
            {
                return _filePath;
            }
        }

        public int AddRef()
        {
            _refCount++;
            return _refCount;
        }

        public int RemoveRef()
        {
            _refCount--;
            return _refCount;
        }

        public int GetRefCount()
        {
            return _refCount;
        }

        #region Resource Load event

        public event OnLoadedComplete OnDataReady;

        public void AddDataHandler( OnLoadedComplete handler )
        {
            if ( handler == null )
            {
                return;
            }

            if ( readyToUse )
            {
                handler.Invoke( _resName, this, _param );
            }
            else
            {
                OnDataReady += handler;
            }
        }

        public void RemoveDataHandler( OnLoadedComplete handler )
        {
            if ( handler == null )
            {
                return;
            }

            OnDataReady -= handler;
        }
        #endregion

        public virtual void Init( string path, bool isLocal, System.Object param )
        {
            this._resName = Path.GetFileNameWithoutExtension( path );
            this._param = param;

            if ( isLocalResource == true )
            {
                _filePath = path;
            }
            else
            {
                _filePath = CommonUtil.getWWWURLFromTypeAndName( path );
            }

            if ( !AlreadyDownloaded() )
            {
                _data = null;
                _assetLoadError = false;

                GGameDataLoader.GetInstance().StartDownload( this );
            }
        }

        /// <summary>
        /// Frame update, check resource is downloaded
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns> Return can be delect from downloading list </returns>
        public bool Update( float deltaTime )
        {
            if ( _readyToUse == false )
            {
                readyToUse = dataReadyToUse;
            }

            return _readyToUse || _assetLoadError;
        }

        public virtual bool DataReadyToUse()
        {
            return dataReadyToUse;
        }

        private bool _readyToUse;

        /// <summary>
        /// 游戏资源已经可以使用
        /// </summary>
        public bool readyToUse
        {
            get
            {
                return _readyToUse;
            }
            protected set
            {
                if ( _readyToUse != value )
                {
                    _readyToUse = value;
                    if ( _readyToUse && OnDataReady != null )
                    {
                        OnDataReady.Invoke( _resName, this, _param );
                    }
                }
            }
        }

        public bool dataReadyToUse
        {
            get
            {
                return ( _data != null );
            }
        }

        private float _progress;
        public float Progress
        {
            get
            {
                return _progress;
            }
        }

        public System.Object Data
        {
            get
            {
                return _data;
            }
        }

        public bool error
        {
            get
            {
                return _assetLoadError;
            }
        }

        #region 加载接口

        private string _url = "";
        public string URL
        {
            get
            {
                if ( _url.Length == 0 )
                {
                    if ( !isLocalResource )
                    {
                        _url = CommonUtil.getWWWURLFromTypeAndName( _filePath );
                    }
                    else
                    {
                        _url = _filePath;
                    }
                }
                return _url;
            }
        }

        public virtual void LoadCompleteHandler( System.Object data )
        {
            _data = data;
            if ( _data == null || _data.ToString() == "null" )
            {
                LoadErrorHandler();
            }
        }

        public virtual void LoadErrorHandler()
        {
            _data = null;
            _assetLoadError = true;
        }

        public void GetLoadingProgress( float progress )
        {
            _progress = progress;
        }

        public void GetLoadingDataLength( string dataLength )
        {
            _dataLength = int.Parse( dataLength );
        }

        #endregion

        public virtual void Unload()
        {
            if ( GGameDataLoader.GetInstance().isInProcesser( this ) )
            {
                return;
            }
            UnloadImpl();
        }

        protected virtual void UnloadImpl()
        {
            if ( GetRefCount() <= 0 )
            {
                if ( readyToUse )
                {
                    GameResourceLoadManager.GetInstance().ReleaseGameData( _resName );
                }
                else
                {
                    if ( GGameDataLoader.GetInstance().removeItem( this ) )
                    {
                        GameResourceLoadManager.GetInstance().ReleaseGameData( ResName );
                    }
                    else
                    {
                        if ( error )
                        {
                            //NO CHINESE!
                            //出错的话，删掉
                            GameResourceLoadManager.GetInstance().ReleaseGameData( ResName );
                        }
                        else
                        {
                            if ( GGameDataLoader.GetInstance().isInProcesser( this ) )
                            {
                                //NO CHINESE!
                                //TODO：正在加载中，会不会有问题
                            }
                            else
                            {
                                GameResourceLoadManager.GetInstance().ReleaseGameData( ResName );
                            }
                        }
                    }
                }
            }
        }

        private bool AlreadyDownloaded()
        {
            bool dataExist = GameResourceLoadManager.GetInstance().DataExist( _resName );

            if ( dataExist )
            {
                GAbstractData preDownloadData = GameResourceLoadManager.GetInstance().GetGameData( _resName );
                _data = preDownloadData._data;
                return true;
            }

            return false;
        }
    }
}

