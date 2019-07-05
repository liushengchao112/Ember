using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace ECTools
{
    public class ClearLocalCache : Editor
    {
        [MenuItem( "Tools/ClearLocalCache" )]
        public static void ClearCache()
        {
            Caching.CleanCache();
            if ( Directory.Exists( Application.persistentDataPath ) )
            {
                Directory.Delete( Application.persistentDataPath, true );
            }
        }
    }
}


