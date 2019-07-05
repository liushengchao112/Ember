using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Resource
{
    /// <summary>
    /// 加载处理器，用于控制下载线程数量
    /// </summary>
    public sealed class GGameDataLoaderProcesser
    {
        private WWW _wwwRequest = null;

        private IGLoaderItem _item = null;

        private bool _completed = true;

        private sbyte MaxReloadNum = 5;

        public GGameDataLoaderProcesser()
        {

        }

        public void setItem( IGLoaderItem item )
        {
            if ( _wwwRequest != null )
            {
                _wwwRequest.Dispose();
                _wwwRequest = null;
            }

            try
            {
                _wwwRequest = new WWW( item.URL );
                _completed = false;
                _item = item;
            }
            catch
            {
                if ( _wwwRequest != null )
                {
                    _wwwRequest.Dispose();
                    _wwwRequest = null;
                }
                Debug.Log( String.Format( "[URL]_#{0}", item.URL ) );
                _item.LoadErrorHandler();
                _item = null;

                _completed = true;
            }
        }


        public bool update( float detlaTime )
        {
            if ( !_completed )
            {
                if ( _completed = ( _wwwRequest != null && _wwwRequest.isDone ) )
                {
                    try
                    {
                        if ( _wwwRequest.error == null )
                        {

                            _item.LoadCompleteHandler( _wwwRequest.assetBundle );

                        }
                        else
                        {
                            _item.LoadErrorHandler();
                            Debug.Log( string.Format( "[Error]{0}  [URL]{1}", _wwwRequest.error, _wwwRequest.url ) );
                        }
                    }
                    catch
                    {
                        _item.LoadErrorHandler();
                    }
                    finally
                    {
                        if ( _wwwRequest.responseHeaders.ContainsKey( "CONTENT-LENGTH" ) )
                        {
                            _item.GetLoadingDataLength( _wwwRequest.responseHeaders["CONTENT-LENGTH"] );

                        }
                        else
                        {
                            string param = "0";
                            _item.GetLoadingDataLength( param );
                        }

                        _wwwRequest.Dispose();
                        _wwwRequest = null;

                        _item = null;
                    }
                }
                else
                {
                    //未完成，更新进度
                    //_wwwRequest.progress
                    _item.GetLoadingProgress( _wwwRequest.progress );
                }
            }

            return _completed;
        }

        public bool isDataProcess( GAbstractData data )
        {
            if ( data.URL == _item.URL )
            {
                return true;
            }
            return false;
        }
    }
}
