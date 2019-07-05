using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Utils;
using Pathfinding.Ionic.Zip;
using UObject = UnityEngine.Object;

namespace Resource
{
    public class AssetBundleRef
    {
        public string bundleName;
        public AssetBundle assetBundle { get; set; }
        public string[] dependencies;
        private Dictionary<string, int> objectRefs = new Dictionary<string, int>();
        private List<string> dependencyRefs = new List<string>();
        private Callback<string[], string> unDependenciesBundle;

        public AssetBundleRef( string bundleName, string[] dependencies, Callback<string[], string> unDependenciesBundle )
        {
            this.bundleName = bundleName;
            this.dependencies = dependencies;
            this.unDependenciesBundle = unDependenciesBundle;
        }

        public int IncreaseDepCount()
        {
            return dependencyRefs.Count;
        }

        public void AddIncreaseDep( string name )
        {
            if ( !dependencyRefs.Contains( name ) )
            {
                dependencyRefs.Add( name );
            }
        }

        public void ReduceDepCount( string name )
        {
            if ( dependencyRefs.Contains( name ) )
            {
                DebugUtils.Log( DebugUtils.Type.AssetBundle, string.Format( "DepCount {0} before {1} after {2}", bundleName, dependencyRefs.Count, dependencyRefs.Count > 0 ? dependencyRefs.Count - 1 : 0 ) );
                dependencyRefs.Remove( name );
            }
           
            if ( dependencyRefs.Count == 0 )
            {
                UnloadBundle();
            }
        }

        public void AddRef( string objName )
        {
            if ( objectRefs.ContainsKey( objName ) )
            {
                objectRefs[objName]++;
            }
            else
            {
                objectRefs.Add( objName, 1 );
            }
        }

        public void RemoveRef( string objName )
        {
            if ( objectRefs.ContainsKey( objName ) )
            {
                int num = objectRefs[objName];
                objectRefs[objName] = num = num > 0 ? num - 1 : 0;
                
                if ( num <= 0 )
                {
                    objectRefs.Remove( objName );
                }

                if ( objectRefs.Count == 0 )
                {
                    UnloadBundle();
                }
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AssetBundle, string.Format( "The bundle {0} does not have a refcounter for {1}", bundleName, objName ) );
            }
        }

        public void UnloadBundle()
        {
#if UNITY_EDITOR
            if ( objectRefs.Count > 0 )
            {
                Utils.DebugUtils.Log( DebugUtils.Type.AssetBundle, "Unloading AssetBundle with objectRefs: " + bundleName );
                foreach ( KeyValuePair<string, int> kvp in objectRefs )
                {
                    Utils.DebugUtils.Log( DebugUtils.Type.AssetBundle, "ObjectRef: " + kvp.Key + "[" + kvp.Value + "]" );
                }
            }
            else
            {
                Utils.DebugUtils.Log( DebugUtils.Type.AssetBundle, "AssetBundle unloaded " + bundleName );
            }

            if( dependencyRefs.Count > 0 )
            {
                DebugUtils.LogWarning( DebugUtils.Type.AssetBundle, string.Format( "The AssetBundle \"{0}\" dependency count is not 0, count:{1}", bundleName, dependencyRefs.Count ) );
            }
#endif
            if ( assetBundle != null )
            {
                assetBundle.Unload( true );
                assetBundle = null;
            }

            if( unDependenciesBundle != null )
            {
                unDependenciesBundle( dependencies, bundleName );
            }
        }
    }

    public class AssetBundleManager : MonoBehaviour
    {
        class LoadingAssetBundleData
        {
            public enum State
            {
                Prepare,
                LoadDepend,
                LoadBundle,
                LoadAsset,
                Complete,
                Waiting,
            }
            public State curState;
            public string assetName;
            public AssetBundleRef abRef;
            public List<AssetBundleRef> dependRefList;
            public Callback<UObject> assetLoadCompleteCall;

            public void Dispose()
            {
                curState = State.Prepare;
                assetName = null;
                abRef = null;
                if( dependRefList != null )
                {
                    dependRefList.Clear();
                    dependRefList = null;
                }
                assetLoadCompleteCall = null;
            }
        }

        private enum LoadAssetBundleStep
        {
            PreLoad,
            Loading,
            Complete,
        }

        class UnZipData
        {
            public string sourcePath;
            public string targetPath;
            public ZipInputStream stream;
        }

        private LoadAssetBundleStep loadAssetBundleStep;
        public static List<string> LoadBundleList = new List<string>();
        private Dictionary<string, AssetBundleRef> bundlesDic = new Dictionary<string, AssetBundleRef>();
        private Queue<LoadingAssetBundleData> loadingBundleQueue = new Queue<LoadingAssetBundleData>();
        private string assetPath;
        private string zipAssetPath;
        private LoadingAssetBundleData curLoadData = null;
        private const string unZip_Local_File_Key = "unZip_Local_File_Key";
        public UpZipProgressData upZipProgressData;
        public const string zipFileListTxt = "zipFileList.txt";
        public const string cacheDataTimeTxt = "cacheDataTime.txt";
        private Dictionary<string, Shader> bundleShaderDic = new Dictionary<string, Shader>();
        private string preloadFlag = "$-preload^Flag-$";
        private string localZipListPath { get { return zipAssetPath + "localZipList.txt"; } }
        private bool unzipComplete;
        private UnZipData curUnZipData;

        void Awake()
        {
            DontDestroyOnLoad( this );
            assetPath = string.Concat ( Application.persistentDataPath, "/", DownloadResource.SERVER_URL_BRANCK, "/GameResources/bundle/" );
            zipAssetPath = string.Concat ( Application.persistentDataPath, "/", DownloadResource.SERVER_URL_BRANCK, "/GameResources/zip/" );
        }

        public void AddLoadZipName( string zipName )
        {
            if ( !File.Exists( localZipListPath ) )
            {
                using ( FileStream fs = new FileStream( localZipListPath, FileMode.Create ) )
                {
                    fs.Flush();
                    fs.Close();
                }
            }

            using ( StreamWriter sr = new StreamWriter( localZipListPath, true ) )
            {
                sr.WriteLine( zipName );
                sr.Close();
            }
        }

        public void RemoveLocalZipListFile()
        {
            if ( File.Exists( localZipListPath ) )
            {
                File.Delete( localZipListPath );
            }
        }

        public void Init( Callback completeCallBack )
        {
            upZipProgressData = new UpZipProgressData();
            //decompress zip file 
            StartCoroutine( CheckLocalZipFiles( delegate ()
            {
                upZipProgressData = null;
                if ( Constants.GameConstants.LoadAssetByEditor )
                {
                    completeCallBack();
                }
                else
                {
                    SetManifestFile();
                    completeCallBack();
                    InitData();
                }
            } ) );
        }

        private void InitData()
        {
            loadAssetBundleStep = LoadAssetBundleStep.PreLoad;
        }

        private void DecompressFiles ()
        {
            float startTime = Time.realtimeSinceStartup;
            DebugUtils.Log ( DebugUtils.Type.AssetBundle, "CompressionType - " + CommonUtil.compressionType.ToString () );
            for ( int i = 0; i < LoadBundleList.Count; i++ )
            {
                string filePath = assetPath + LoadBundleList[ i ];
                CommonUtil.DecompressAssetBundle ( filePath );
            }
            DebugUtils.Log ( DebugUtils.Type.AssetBundle, string.Format ( "TotalTime: {0}", Time.realtimeSinceStartup - startTime ) );
            LoadBundleList.Clear ();
        }

        private IEnumerator CheckLocalZipFiles( Callback callBack )
        {
            Queue<string> pathQueue = new Queue<string> ();

            //Check whether the local persistentDataPath folder has a zip file 
            if ( File.Exists( localZipListPath ) )
            {
                string[] loadZipList = File.ReadAllLines( localZipListPath );
                DebugUtils.Log ( DebugUtils.Type.AssetBundle, "Persistent zip number : " + loadZipList.Length );
                for ( int i = 0; i < loadZipList.Length; i++ )
                {
                    if ( !string.IsNullOrEmpty( loadZipList[i] ) )
                    {
                        pathQueue.Enqueue( string.Concat( "file:///", zipAssetPath, loadZipList[i] ) );
                    }
                }
            }

            //Online overlay local
            //Check whether the local StreamingAssets folder has a zip file 
            bool canUnzipLocalFile = false;
            string bundDataTime = "";
            string localStreamingPath;
            if ( Application.platform == RuntimePlatform.Android)
            {
                localStreamingPath = string.Concat( Application.streamingAssetsPath, "/", zipFileListTxt );
            }
            else
            {
                localStreamingPath = string.Concat( "file:///", Application.streamingAssetsPath, "/", zipFileListTxt );
            }

            WWW streamPathWWW = new WWW( localStreamingPath );
            yield return streamPathWWW;

            if ( string.IsNullOrEmpty( streamPathWWW.error ) && !string.IsNullOrEmpty( streamPathWWW.text ) )
            {
				string[] strs = streamPathWWW.text.Split ('\n');
                bundDataTime = strs[0].Trim();
                DebugUtils.Log( DebugUtils.Type.AssetBundle, "StreamingAssets zip number : " + strs.Length );
			
                canUnzipLocalFile = true;
                if ( File.Exists( zipAssetPath + cacheDataTimeTxt ) )
                {
                    string cacheDataTimeStr = File.ReadAllText( zipAssetPath + cacheDataTimeTxt ).Trim();
                    if ( !string.IsNullOrEmpty( cacheDataTimeStr ) )
                    {
                        canUnzipLocalFile = cacheDataTimeStr.Equals( bundDataTime ) == false;
                    }
                }

                if ( canUnzipLocalFile )
                {
                    for ( int i = 1; i < strs.Length; i++ )
                    {
                        string fileName = strs[i].Trim();
                        if ( !string.IsNullOrEmpty( fileName ) )
                        {
                            if ( Application.platform == RuntimePlatform.Android )
                            {
                                pathQueue.Enqueue ( string.Concat( Application.streamingAssetsPath, "/", fileName ) );
                            }
                            else
                            {
                                pathQueue.Enqueue ( string.Concat( "file:///", Application.streamingAssetsPath, "/", fileName ) );
                            }
                        }
                    }
                }
            }

            streamPathWWW.Dispose();
            streamPathWWW = null;

            upZipProgressData.zipNum = pathQueue.Count;
            if( upZipProgressData.zipNum > 0)
            {
                StartCoroutine ( UnzipFileHandler ( pathQueue, bundDataTime, callBack ) );
            }
            else if ( callBack != null )
            {
                callBack ();
            }
        }

        private IEnumerator UnzipFileHandler ( Queue<string> queue, string bundDataTime, Callback callback )
        {
            ZipInputStream zipInputStream = null;
            ZipEntry zipEntry = null;
            WaitForEndOfFrame waitFrame = new WaitForEndOfFrame ();
            Thread unzipThread = new Thread ( new ParameterizedThreadStart ( UnzipFileThread ) );
            int delatime = (int)( Time.deltaTime * 1000 );
            unzipThread.Start ( delatime );

            while ( queue.Count != 0 )
            {
                string localUrl = queue.Dequeue ();
                WWW www = new WWW ( localUrl );
                yield return www;
                if ( !string.IsNullOrEmpty ( www.error ) )
                {
                    //DebugUtils.LogError( DebugUtils.Type.AssetBundle, string.Format( "Loading zip file failed! when decompress zip! err: {0}  url = {1}", www.error, localUrl ) );
                    www.Dispose ();
                    www = null;
                    continue;
                }

                zipInputStream = new ZipInputStream ( new MemoryStream ( www.bytes, 0, www.bytes.Length ), true );
                zipInputStream.Password = "mima";
                while ( ( zipEntry = zipInputStream.GetNextEntry () ) != null )
                {
                    curUnZipData = new UnZipData ();
                    curUnZipData.sourcePath = localUrl;
                    curUnZipData.targetPath = assetPath + zipEntry.FileName;
                    curUnZipData.stream = zipInputStream;
                    unzipComplete = true;

                    while( unzipComplete )
                    {
                        yield return waitFrame;
                    }
                }

                www.Dispose ();
                www = null;
            }
     
            DebugUtils.Log ( DebugUtils.Type.AssetBundle, "unzip all files complete!" );

            RemoveLocalZipListFile ();
            unzipThread.Abort ();
            unzipThread = null;
            using ( FileStream fs = new FileStream ( zipAssetPath + cacheDataTimeTxt, FileMode.OpenOrCreate ) )
            {
                using ( StreamWriter sw = new StreamWriter ( fs ) )
                {
                    sw.WriteLine ( bundDataTime );
                    sw.Close ();
                    sw.Dispose ();
                }
                fs.Close ();
                fs.Dispose ();
            }
            System.GC.Collect ();

            if ( callback != null )
            {
                callback ();
            }
        }
        
        private void UnzipFileThread( object obj )
        {
            int delatime = (int)obj;
            DebugUtils.Log ( DebugUtils.Type.AssetBundle, "Unzip file thread run! time:" + delatime );

            // read 2048
            int size = 2048;
            byte[] readData = new byte[size];
            while ( true )
            {
                if ( unzipComplete && curUnZipData != null )
                {
                    try
                    {
                        FileStream fs = null;
                        // del old file and create a new file
                        if ( File.Exists ( curUnZipData.targetPath ) )
                        {
                            File.Delete ( curUnZipData.targetPath );
                        }
                        fs = File.Create ( curUnZipData.targetPath );
                        while ( true )
                        {
                            size = curUnZipData.stream.Read ( readData, 0, readData.Length );
                            if ( size > 0 )
                            {
                                fs.Write ( readData, 0, size );
                            }
                            else
                            {
                                break;
                            }
                        }
                        fs.Close ();
                        fs.Dispose ();
                        DebugUtils.Log ( DebugUtils.Type.AssetBundle, "unzip complete! path:" + curUnZipData.sourcePath );
                    }
                    catch ( System.Exception e )
                    {
                        DebugUtils.LogError ( DebugUtils.Type.AssetBundle, string.Format ( "unzip err! path:{0} msg:{1}", curUnZipData.targetPath, e.Message ) );
                    }
                    curUnZipData.sourcePath = curUnZipData.sourcePath.Replace ( "file:///", "" );
                    //Delete local zip file
                    #if UNITY_EDITOR
                    File.Delete ( curUnZipData.sourcePath );
                    #else
                    if ( curUnZipData.sourcePath.StartsWith( zipAssetPath ) )
                    {
                        File.Delete( curUnZipData.sourcePath );
                    }
                    #endif
                    unzipComplete = false;
                    curUnZipData = null;
                    upZipProgressData.curIndex++;
                }
                else
                {
                    Thread.Sleep ( delatime );
                }
            }
        }

        private void ExtractProgressHandler ( object sender, ExtractProgressEventArgs e )
        {
            upZipProgressData.progress = (float)e.BytesTransferred / e.TotalBytesToTransfer;
            upZipProgressData.desc = string.Format ( "解压zip文件:{0}", e.ArchiveName );
        }

        private void SetManifestFile()
        {
            string fileName = CommonUtil.EncodingToMd5 ( Constants.GameConstants.BundleManifest );
            string localPath = assetPath + fileName;
            AssetBundle bundle = AssetBundle.LoadFromFile ( localPath );
            if ( bundle == null )
            {
                DebugUtils.LogError ( DebugUtils.Type.AssetBundle, "AssetBundleManifest is not found! path : " + localPath );
            }
            AssetBundleManifest assetBundleManifest = bundle.LoadAsset<AssetBundleManifest> ( "AssetBundleManifest" );

            string[] bundleNames = assetBundleManifest.GetAllAssetBundles ();
            for ( int i = 0; i < bundleNames.Length; i++ )
            {
                string bundleName = bundleNames[ i ];
                string[] dependencies = assetBundleManifest.GetAllDependencies( bundleName );

                if ( !bundlesDic.ContainsKey ( bundleName ) )
                {
                    bundlesDic[bundleName] = new AssetBundleRef( bundleName, dependencies, OnUnDependenciesBundle );
                }
            }

            bundle.Unload ( true );
            bundle = null;
        }

        public void ReleaseAsset( string bundleName, string assetName )
        {
            AssetBundleRef bundleRef;
            if ( bundlesDic.TryGetValue( bundleName, out bundleRef ) )
            {
                bundleRef.RemoveRef( assetName );
            }
        }

        public void UnloadBundle( string bundleName )
        {
            AssetBundleRef abr;
            if ( bundlesDic.TryGetValue( bundleName, out abr ) )
            {
                abr.UnloadBundle();
            }
#if UNITY_EDITOR
            foreach ( KeyValuePair<string, AssetBundleRef> kvp in bundlesDic )
            {
                Utils.DebugUtils.Log( DebugUtils.Type.AssetBundle, kvp.Value.bundleName );
            }
#endif
        }

        public void UnloadAllBundles()
        {
            var item = bundlesDic.GetEnumerator();
            while ( item.MoveNext() )
            {
                item.Current.Value.UnloadBundle();
            }
        }

        public T GetAsset<T>( string bundleName, string assetName ) where T : UnityEngine.Object
        {
            if ( !bundlesDic.ContainsKey( bundleName ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AssetBundle, string.Format( "AssetBundle {0} for this platform not found", bundleName ) );
                return null;
            }

            AssetBundleRef assetBundleRef = bundlesDic[bundleName];

            List<AssetBundleRef> dependbundleList = null;
            CollectDepend( assetBundleRef, out dependbundleList );

            int length = dependbundleList == null ? 0 : dependbundleList.Count;
            for ( int i = 0; i < length; i++ )
            {
                AssetBundleRef dependbundle = dependbundleList[i];
                dependbundle.AddIncreaseDep( bundleName );
                if ( dependbundle.assetBundle == null )
                {
                    DebugUtils.Log( DebugUtils.Type.AssetBundle, "Start sync loading depend bundle : " + dependbundle.bundleName );
                    string dependBundlePath = assetPath + CommonUtil.EncodingToMd5( dependbundleList[i].bundleName );
                    dependbundle.assetBundle = AssetBundle.LoadFromFile( dependBundlePath );
                    if ( dependbundle.assetBundle == null )
                    {
                        DebugUtils.LogError( DebugUtils.Type.AssetBundle, string.Format( "AssetBundle \"{0}\" for this platform not found, path : {1}", bundleName, dependBundlePath ) );
                    }
                    else
                    {
                        DebugUtils.Log( DebugUtils.Type.AssetBundle, "Sync load depend bundle done, name is：" + dependbundle.bundleName );
                    }
#if UNITY_EDITOR
                    RestAssetShader( dependbundle.assetBundle.LoadAllAssets() );
#endif
                }
            }

            string bundlePath = assetPath + CommonUtil.EncodingToMd5( bundleName );
            if ( assetBundleRef.assetBundle == null )
            {
                DebugUtils.Log( DebugUtils.Type.AssetBundle, "Start sync loading bundle : " + bundleName );
                assetBundleRef.assetBundle = AssetBundle.LoadFromFile( bundlePath );
                DebugUtils.Log( DebugUtils.Type.AssetBundle, "Sync load depend bundle done, name is：" + bundleName );
            }

            T asset = null;

            if ( assetBundleRef.assetBundle == null )
            {
                DebugUtils.LogError( DebugUtils.Type.AssetBundle, string.Format( "AssetBundle \"{0}\" for this platform not found, path : {1}", bundleName, bundlePath ) );
            }
            else
            {
                asset = assetBundleRef.assetBundle.LoadAsset<T>( assetName );
                if ( asset == null )
                {
                    DebugUtils.LogError( DebugUtils.Type.AssetBundle, string.Format( "AssetBundle \"{0}\" don't contain \"{1}\" resources, type:{2}", bundleName, assetName, typeof( T ) ) );
                }
                else
                {
#if UNITY_EDITOR
                    RestAssetShader( new UObject[] { asset } );
#endif
                    assetBundleRef.AddRef( assetName );
                }
            }        
                  
            return asset;
        }

        public void GetAssetAsync( string bundleName, string assetName, Callback<UObject> callback )
        {
            if ( !bundlesDic.ContainsKey( bundleName ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AssetBundle, string.Format( "AssetBundle \"{0}\" for this platform not found", bundleName ) );
                if ( callback != null )
                {
                    callback( null );
                }
                return;
            }
            AssetBundleRef assetBundleRef = bundlesDic[bundleName];

            LoadingAssetBundleData loadData = new LoadingAssetBundleData();
            loadData.assetName = assetName;
            loadData.abRef = assetBundleRef;
            CollectDepend( assetBundleRef, out loadData.dependRefList );
            loadData.assetLoadCompleteCall = callback;
            loadingBundleQueue.Enqueue( loadData );

            DebugUtils.Log( DebugUtils.Type.AssetBundle, string.Format( "Prepare load bundle data \"{0}\" ,assetName is \"{1}\"", loadData.abRef.bundleName, loadData.assetName ) );
        }

        private void CollectDepend( AssetBundleRef abr, out List<AssetBundleRef> abList )
        {
            if ( abr.dependencies == null || abr.dependencies.Length == 0 )
            {
                abList = null;
                return;
            }
            else
            {
                abList = new List<AssetBundleRef>();
            }
            
            for ( int i = 0; i < abr.dependencies.Length; i++ )
            {
                AssetBundleRef dependAB = bundlesDic[abr.dependencies[i]];
                if( dependAB.assetBundle == null )
                {
                    abList.Add( dependAB );
                }
            }
        }

        public void PreloadBundle( string bundleName ) 
        {
            DebugUtils.Log( DebugUtils.Type.AssetBundle, "Preload bundle : " + bundleName );
            GetAssetAsync( bundleName, preloadFlag, delegate ( UObject obj ) { } );
        }

        private IEnumerator LoadBundleAsync(string name, Callback<AssetBundle,int> calback , int index = 0 )
        {
            string path = assetPath + CommonUtil.EncodingToMd5( name );
            AssetBundleCreateRequest assetBundleRequest = AssetBundle.LoadFromFileAsync( path );
            yield return assetBundleRequest;
            if ( assetBundleRequest.assetBundle == null )
            {
                DebugUtils.LogError( DebugUtils.Type.AssetBundle, string.Format( "AssetBundle \"{0}\" for this platform not found, path : {1}", name, path ) );
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.AssetBundle, "Async load bundle done, name is：" + name );
            }
           
            if ( calback != null )
            {
                calback( assetBundleRequest.assetBundle, index );
            }
        }

        private IEnumerator LoadAssetAsync( AssetBundle ab, string assetName, Callback<UObject> calback )
        {
            AssetBundleRequest assetBundleRequest = ab.LoadAssetAsync( assetName );
            yield return assetBundleRequest;

            if ( assetBundleRequest.asset == null )
            {
                DebugUtils.LogError( DebugUtils.Type.AssetBundle, string.Format( "AssetBundle \"{0}\" don't contain \"{1}\" resources", ab.name, assetName ) );
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.AssetBundle, "Load asset done, name is : " + assetName );
            }

            if( calback != null )
            {
                calback( assetBundleRequest.asset );
            }
        }

        private IEnumerator LoadAllAssetAsync( AssetBundle ab, Callback<UObject[], int> calback, int index )
        {
            AssetBundleRequest assetBundleRequest = ab.LoadAllAssetsAsync();
            yield return assetBundleRequest;

            if ( assetBundleRequest.allAssets == null )
            {
                DebugUtils.LogError( DebugUtils.Type.AssetBundle, string.Format( "AssetBundle \"{0}\"load all resources err!", ab.name ) );
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.AssetBundle, "Load all assets done, bundle is :" + ab.name );
            }

            if ( calback != null )
            {
                calback( assetBundleRequest.allAssets, index );
            }
        }

        private void LoadAssetBundleHandler()
        {
            switch( curLoadData.curState )
            {
                case LoadingAssetBundleData.State.Prepare:
                {
                    DebugUtils.Log( DebugUtils.Type.AssetBundle, string.Format( "Prepare load bundle is \"{0}\" ,assetName is \"{1}\"", curLoadData.abRef.bundleName, curLoadData.assetName ) );
                    curLoadData.curState = LoadingAssetBundleData.State.LoadDepend;
                    LoadAssetBundleHandler();
                    break;
                }
                case LoadingAssetBundleData.State.LoadDepend:
                {
                    int length = curLoadData.dependRefList == null ? 0 : curLoadData.dependRefList.Count;

                    DebugUtils.Log( DebugUtils.Type.AssetBundle, "Prepare load depend bundles, number is : " + length );

                    if ( length == 0 )
                    {
                        curLoadData.curState = LoadingAssetBundleData.State.LoadBundle;
                        LoadAssetBundleHandler();
                        break;
                    }

                    int count = 0;
                    for ( int i = 0; i < length; i++ )
                    {
                        curLoadData.dependRefList[i].AddIncreaseDep( curLoadData.abRef.bundleName );

                        if ( curLoadData.dependRefList[i].assetBundle != null )
                        {
                            if ( ++count == length )
                            {
                                curLoadData.curState = LoadingAssetBundleData.State.LoadBundle;
                                LoadAssetBundleHandler();
                            }
                            continue;
                        }

                        DebugUtils.Log( DebugUtils.Type.AssetBundle, "Start loading depend bundle : " + curLoadData.dependRefList[i].bundleName );
                        StartCoroutine( LoadBundleAsync( curLoadData.dependRefList[i].bundleName, delegate ( AssetBundle ab, int index )
                        {
                            curLoadData.dependRefList[index].assetBundle = ab;
                            if ( ++count == length )
                            {
                                curLoadData.curState = LoadingAssetBundleData.State.LoadBundle;
                                LoadAssetBundleHandler();
                            }
                        }, i ) );
                    }

                    break;
                }
                case LoadingAssetBundleData.State.LoadBundle:
                {
                    DebugUtils.Log( DebugUtils.Type.AssetBundle, "Prepare load bundle : " + curLoadData.abRef.bundleName );

                    if ( curLoadData.abRef.assetBundle != null )
                    {
                        curLoadData.curState = LoadingAssetBundleData.State.LoadAsset;
                        LoadAssetBundleHandler();
                        break;
                    }

                    StartCoroutine( LoadBundleAsync( curLoadData.abRef.bundleName, delegate ( AssetBundle ab, int index )
                    {
                        curLoadData.abRef.assetBundle = ab;
                        curLoadData.curState = LoadingAssetBundleData.State.LoadAsset;
                        LoadAssetBundleHandler();
                    } ) );
                    break;
                }
                case LoadingAssetBundleData.State.LoadAsset:
                {
                    if( !curLoadData.assetName.Equals(preloadFlag) )
                    {
                        StartCoroutine( LoadAssetAsync( curLoadData.abRef.assetBundle, curLoadData.assetName, delegate ( UObject obj )
                        {
                            curLoadData.abRef.AddRef( curLoadData.assetName );
                            curLoadData.assetLoadCompleteCall( obj );
#if UNITY_EDITOR
                            RestAssetShader( new UObject[] { obj } );
#endif
                            curLoadData.curState = LoadingAssetBundleData.State.Complete;
                            LoadAssetBundleHandler();
                        } ) );
                    }
                    break;
                }
                case LoadingAssetBundleData.State.Complete:
                {
                    curLoadData.Dispose();
                    curLoadData = null;
                    loadAssetBundleStep = LoadAssetBundleStep.PreLoad;
                    break;
                }
            }

        }

        void Update()
        {
            if ( loadingBundleQueue.Count > 0 )
            {
                if ( loadAssetBundleStep == LoadAssetBundleStep.PreLoad )
                {
                    loadAssetBundleStep = LoadAssetBundleStep.Loading;
                    curLoadData = loadingBundleQueue.Dequeue();
                    curLoadData.curState = LoadingAssetBundleData.State.Prepare;
                }
            }
            
            if ( curLoadData != null && curLoadData.curState == LoadingAssetBundleData.State.Prepare )
            {
                LoadAssetBundleHandler();
            }
        }

#if UNITY_EDITOR
        private void RestAssetShader( UObject[] assets )
        {
            foreach ( var u in assets )
            {
                GameObject go = u as GameObject;
                if ( go != null )
                {
                    Renderer[] rs = go.GetComponentsInChildren<Renderer>( true );
                    foreach ( Renderer r in rs )
                    {
                        RestMaterialShader( r.sharedMaterials );
                    }
                }

                Material mat = u as Material;

                if(mat != null)
                {
                    RestMaterialShader( new Material[] { mat } );
                }
            }
        }

        private void RestMaterialShader( Material[] mats )
        {
            foreach(var m in mats)
            {
                if( m == null )
                {
                    continue;
                }
                var shaderName = m.shader.name;
                Shader newShader = Shader.Find( shaderName );
                if ( newShader == null )
                {
                    DebugUtils.LogError( DebugUtils.Type.AssetBundle, string.Format( "unable to refresh shader:{0} in material {1}", shaderName, m.name ) );
                    continue;
                }
                m.shader = newShader; 
            }
        }

        private void CheckShader( UObject[] assets )
        {
            if ( !DebugUtils.DebugMode ) return;

            foreach ( var u in assets )
            {
                CheckShader( u );
            }
        }

        private void CheckShader( UObject obj )
        {
            if ( !DebugUtils.DebugMode ) return;

            GameObject go = obj as GameObject;
            if ( go != null )
            {
                Renderer[] rs = go.GetComponentsInChildren<Renderer>( true );
                foreach ( Renderer r in rs )
                {
                    CheckShader( r.sharedMaterials, obj.name );
                }
            }

            AssetBundle ab = obj as AssetBundle;
            if ( ab != null )
            {
                CheckShader( ab.LoadAllAssets() );
            }

            Material mat = obj as Material;
            if ( mat != null )
            {
                CheckShader( mat, obj.name );
            }
        }

        private void CheckShader( Material[] mats, string name )
        {
            if ( !DebugUtils.DebugMode ) return;

            foreach ( var m in mats )
            {
                if ( m == null )
                {
                    continue;
                }

                CheckShader( m, name );
            }
        }

        private void CheckShader( Material m, string name )
        {
            if ( !m.shader.isSupported )
            {
                DebugUtils.LogError( DebugUtils.Type.AssetBundle, string.Format( "The shader \"{0}\" is not supported on this platform! object name :{1} ", m.shader.name, name ) );
            }
        }

#endif

        private void OnUnDependenciesBundle( string[] abs, string name )
        {
            if ( abs == null ) return;

            for ( int i = 0; i < abs.Length; i++ )
            {
                AssetBundleRef dependbundle = bundlesDic[abs[i]];
                dependbundle.ReduceDepCount( name );
            }
        }

        public void PrintBundles()
        {
            DebugUtils.Log( DebugUtils.Type.AssetBundle, "BundleCount: " + bundlesDic.Count );

            var item = bundlesDic.GetEnumerator();
            while ( item.MoveNext() )
            {
                DebugUtils.Log( DebugUtils.Type.AssetBundle, item.Current.Value.bundleName + " " + item.Current.Value.IncreaseDepCount() );
            }
        }

    }
}