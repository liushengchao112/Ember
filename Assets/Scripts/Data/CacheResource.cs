using System;
using System.Collections.Generic;

using Constants;
using Utils;
using Resource;
using UObject = UnityEngine.Object;

namespace Data
{
    public class CacheResource
    {
        public Dictionary<string, UObject> mainMenuCacheObjDic; //Store the source file loaded in the Resources folder, can not be a clone file
        public List<string> mainMenuCacheNameList; //Store the name of the resource within AB
        public List<int> mainMenuCacheIdList;      //The resource ID in AB is stored

        public CacheResource()
        {
            MessageDispatcher.AddObserver( OnDisposeBundleCache, MessageType.DisposeBundleCache );

            mainMenuCacheNameList = new List<string>();
            mainMenuCacheIdList = new List<int>();
            mainMenuCacheObjDic = new Dictionary<string, UObject>();
        }

        private void OnDisposeBundleCache()
        {
            GameResourceLoadManager loadManager = GameResourceLoadManager.GetInstance();
            if ( loadManager == null ) return;

            if ( mainMenuCacheNameList != null )
            {
                for ( int i = 0; i < mainMenuCacheNameList.Count; i++ )
                {
                    loadManager.UnloadBundle( mainMenuCacheNameList[i] );
                }
                mainMenuCacheNameList.Clear();
            }

            if ( mainMenuCacheIdList != null )
            {
                for ( int i = 0; i < mainMenuCacheIdList.Count; i++ )
                {
                    loadManager.UnloadBundle( mainMenuCacheIdList[i] );
                }
                mainMenuCacheIdList.Clear();
            }

            if ( mainMenuCacheObjDic != null )
            {
                var keys = new List<string>( mainMenuCacheObjDic.Keys );
                for ( int i = 0; i < keys.Count; i++ )
                {
                    mainMenuCacheObjDic[keys[i]] = null;
                }
                mainMenuCacheObjDic.Clear();
            }

            GC.Collect();
        }

        public void Clear()
        {
            MessageDispatcher.RemoveObserver( OnDisposeBundleCache, MessageType.DisposeBundleCache );

            if ( mainMenuCacheNameList != null )
            {
                mainMenuCacheNameList.Clear();
                mainMenuCacheNameList = null;
            }

            if ( mainMenuCacheIdList != null )
            {
                mainMenuCacheIdList.Clear();
                mainMenuCacheIdList = null;
            }

            if ( mainMenuCacheObjDic != null )
            {
                mainMenuCacheObjDic.Clear();
                mainMenuCacheObjDic = null;
            }
        }
    }
}