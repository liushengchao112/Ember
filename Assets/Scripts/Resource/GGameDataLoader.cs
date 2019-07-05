using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Resource
{
    public sealed class GGameDataLoader
    {
        #region 线程安全的单例模式
        private volatile static GGameDataLoader _instance = null;
        private static readonly Object _lockHelper = new Object();

        public static GGameDataLoader GetInstance()
        {
            if ( _instance == null )
            {
                lock ( _lockHelper )
                {
                    if ( _instance == null )
                        _instance = new GGameDataLoader();
                }
            }
            return _instance;
        }
        #endregion

        private Queue<GGameDataLoaderProcesser> _idleQueue = new Queue<GGameDataLoaderProcesser>();

        private List<GGameDataLoaderProcesser> _runningList = new List<GGameDataLoaderProcesser>();

        private List<IGLoaderItem> _waitQueue = new List<IGLoaderItem>();

        private Queue<GGameLocalLoaderProcesser> _ioIdleQueue = new Queue<GGameLocalLoaderProcesser>();

        private List<GGameLocalLoaderProcesser> _ioRunningList = new List<GGameLocalLoaderProcesser>();

        private List<IGLoaderItem> _readWaitQueue = new List<IGLoaderItem>();

        private int _currentThreadNum;

        private int _ioThreadNum = 1;

        private bool isUseLocal = true;

        private GGameDataLoader()
        {
            //_currentThreadNum = GameConfig.SystemConfig.MaxThreadNum;
            _currentThreadNum = 5;
        }

        public void Startup( System.Object param = null )
        {
            GGameDataLoaderProcesser processer;

            for ( int i = 0; i < _currentThreadNum; i++ )
            {
                processer = new GGameDataLoaderProcesser();
                _idleQueue.Enqueue( processer );
            }
            _runningList.Clear();

            if ( isUseLocal )
            {

                GGameLocalLoaderProcesser localProcesser;
                for ( int i = 0; i < _ioThreadNum; i++ )
                {
                    localProcesser = new GGameLocalLoaderProcesser();
                    _ioIdleQueue.Enqueue( localProcesser );
                }
                _ioRunningList.Clear();
            }
        }

        public void Terminate( System.Object param = null )
        {
            foreach ( GGameDataLoaderProcesser processer in _runningList )
            {
                //TO-DO clean
            }

            _runningList.Clear();
            _idleQueue.Clear();
        }

        public int setThreadNum( int num )
        {
            return _currentThreadNum = Mathf.Min( num, _currentThreadNum );
        }

        public void StartDownload( IGLoaderItem item )
        {
            if ( isUseLocal )
            {
                _readWaitQueue.Add( item );
            }
            else
            {
                _waitQueue.Add( item );
            }
        }

        public bool removeItem( IGLoaderItem item )
        {

            if ( isUseLocal )
            {
                if ( _readWaitQueue.Remove( item ) == true )
                {
                    //TODO :clean
                    return true;
                }
                return false;
            }
            else
            {
                if ( _waitQueue.Remove( item ) == true )
                {
                    //TODO :clean
                    return true;
                }
                return false;
            }
        }

        float _lastWriteTime = 0.0f;
        float MaxWaitTime = 1.0f;
        public void FrameUpdate( float deltaTime )
        {

            GGameDataLoaderProcesser processer;

            for ( int i = 0; i < _runningList.Count; )
            {
                processer = _runningList[i];

                if ( processer.update( deltaTime ) )
                {
                    _runningList.RemoveAt( i );
                    _idleQueue.Enqueue( processer );
                }
                else
                {
                    ++i;
                }
            }

            while ( _waitQueue.Count > 0 && _idleQueue.Count > 0 && _runningList.Count < _currentThreadNum )
            {
                IGLoaderItem item = _waitQueue[0];
                if ( item != null )
                {
                    processer = _idleQueue.Dequeue();
                    processer.setItem( item );
                    _runningList.Add( processer );
                    removeItem( item );
                }
            }

            #region Local
            if ( isUseLocal )
            {
                //// 磁盘IO更新
                GGameLocalLoaderProcesser localProcesser;
                //更新正在运行的IO线程
                for ( int i = 0; i < _ioRunningList.Count; )
                {
                    localProcesser = _ioRunningList[i];

                    if ( localProcesser.Update( deltaTime ) )
                    {
                        _ioRunningList.RemoveAt( i );
                        _ioIdleQueue.Enqueue( localProcesser );
                    }
                    else
                    {
                        ++i;
                    }
                }

                //如果有空闲IO线程则分配任务
                while ( _readWaitQueue.Count > 0 && _ioIdleQueue.Count > 0 &&
                    _ioRunningList.Count < _ioThreadNum )
                {
                    if ( _readWaitQueue.Count > 0 )
                    {
                        try
                        {
                            localProcesser = _ioIdleQueue.Dequeue();
                            localProcesser.SetItem( _readWaitQueue[0] );
                            _ioRunningList.Add( localProcesser );
                        }
                        catch ( System.Exception ex )
                        {
                            _readWaitQueue[0].LoadErrorHandler();

                        }
                        _readWaitQueue.RemoveAt( 0 );
                    }
                }
            }
            #endregion

        }

        public bool isInProcesser( GAbstractData data )
        {
            GGameDataLoaderProcesser processer;

            for ( int i = 0; i < _runningList.Count; ++i )
            {
                processer = _runningList[i];

                if ( processer.isDataProcess( data ) )
                {
                    return true;
                }
            }

            return false;
        }

        public int getLoadQueueNum()
        {
            return _waitQueue.Count;
        }
    }
}


