using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utils;
using System.IO;
using System;
using System.Text;
using System.Security.Cryptography;

namespace Resource
{
    public class DownloadResource : MonoBehaviour
    {

        #if EXTERNAL_SERVER
	    public static readonly string FILE_SERVER_URL = "http://ks3-cn-beijing.ksyun.com/ember2dev01/";
        #else
        public static readonly string FILE_SERVER_URL = "http://ks3-cn-beijing.ksyun.com/ember2dev01/";
#endif
#if TEST_BUILD
        public static readonly string SERVER_URL_BRANCK = "testbuild";
#elif DAILY_BUILD
        public static readonly string SERVER_URL_BRANCK = "dailybuild";
#else
        public static readonly string SERVER_URL_BRANCK = "testbuild";
#endif
        public static readonly string VERSION_FILE = "/version.txt";
        public static readonly string Filelist_FILE = "/fileList.txt";

#if UNITY_IOS
        //public static readonly string RESOURCE_SERVER_URL = FILE_SERVER_URL + "ios/";
#elif UNITY_ANDROID
        //public static readonly string RESOURCE_SERVER_URL = FILE_SERVER_URL + "android/";
#elif UNITY_EDITOR || UNITY_STANDALONE
        //public static readonly string RESOURCE_SERVER_URL = FILE_SERVER_URL + "editor/";
#endif

        // cos server don't have the ios and android files now! so use the editors!
        public static readonly string RESOURCE_SERVER_URL = string.Concat( FILE_SERVER_URL, SERVER_URL_BRANCK, "/", CommonUtil.GetPlatformString(), "/" );

        public static readonly string RESOURCE_PATH_TAG = "B";
        public static readonly string PROTO_PATH_TAG = "C";
        public static readonly string LUA_PATH_TAG = "L";
        public static readonly string ZIP_PATH_TAG = "Z";
        public static readonly string RESOURCE_PATH = "bundle/";
        public static readonly string PROTO_PATH = "bytes/";
        public static readonly string LUA_PATH = "lua/";
        public static readonly string ZIP_PATH = "zip/";
        public static readonly string SERVER_ASSETBUNDLE_PATH = RESOURCE_SERVER_URL + RESOURCE_PATH;
        public static readonly string SERVER_VERSION_FILE_URL = RESOURCE_SERVER_URL + "version.txt";
        public static readonly string SERVER_FILELIST_PATH = RESOURCE_SERVER_URL + "fileList.txt";
        public static readonly string SERVER_PROTO_URL = RESOURCE_SERVER_URL + PROTO_PATH;
        public static readonly string SERVER_LUA_PATH = RESOURCE_SERVER_URL + LUA_PATH;
        public static readonly string SERVER_ZIP_PATH = RESOURCE_SERVER_URL + ZIP_PATH;


        private static readonly string DownloadProgressDescription = "文件下载进度 ";
        private static readonly string DownloadFinishedDescription = "文件下载完成！ ";


        public static readonly string VERSION_CODE_KEY = "ResourceVersion";

        private Dictionary<string, string> localFileList;
        private Dictionary<string, string> serverFileList;
        private Dictionary<string, string> serverBundleDic;
        private Dictionary<string, string> localBundleDic;
        private Dictionary<string , int> serverFileLengthDic;
        //private Dictionary<string , int> serverCheckSumDic;


        public static string localAssetFolder;
        private static string localFilelistPath;
        private bool needForceUpdate = false;

        private static int serverVersionCode = -1;
        public static int localVersionCode = 0;

        private Callback onCheckFinished;
        private AssetBundleManager abManager;
        private bool isLoading = false;

        private delegate void HandleFinishDownload( WWW www );

        private int totalNeedUpdateCount = 0;
        private List<string> updatedAssetList;

        private string accessKey = "";
        private string secretKey = "";

        private static DownloadResource instance;

        public static DownloadResource Instance
        {
            get
            {
                if( instance == null )
                {
                    // TODO: move to update ui
                    instance = GameObject.Find( "UI Root/Camera/UpdateMenu" ).AddComponent<DownloadResource>();
                }
                return instance;
            }
        }

        private UISlider downloadProgerssSlider;
        private UILabel downloadProgerssLabel;
        private UILabel versionLabel;

        void Awake()
        {

            localFilelistPath = string.Concat( Application.persistentDataPath, "/", SERVER_URL_BRANCK, Filelist_FILE );
            localAssetFolder = string.Concat( Application.persistentDataPath, "/", SERVER_URL_BRANCK, "/GameResources/" );

            DebugUtils.Log( DebugUtils.Type.DownloadResource, "localFilelistPath = " + localFilelistPath );
            DebugUtils.Log( DebugUtils.Type.DownloadResource, "localAssetFolder = " + localAssetFolder );
            DebugUtils.Log( DebugUtils.Type.DownloadResource, "SERVER_FILELIST_PATH = " + SERVER_FILELIST_PATH );

            localFileList = new Dictionary<string, string>();
            serverFileList = new Dictionary<string, string>();
            localBundleDic = new Dictionary<string, string>();
            serverBundleDic = new Dictionary<string, string>();
            serverFileLengthDic = new Dictionary<string , int>();
            //serverCheckSumDic = new Dictionary<string , int>();

            //needDownloadFiles = new List<string>();

            updatedAssetList = new List<string>();

            if( !PlayerPrefs.HasKey( VERSION_CODE_KEY ) )
            {
                PlayerPrefs.SetInt( VERSION_CODE_KEY, localVersionCode );
            }

            FileInfo fileInfo = new FileInfo( localFilelistPath );

            if ( !Directory.Exists( string.Concat( Application.persistentDataPath, "/", SERVER_URL_BRANCK ) ) )
            {
                Directory.CreateDirectory(string.Concat( Application.persistentDataPath, "/", SERVER_URL_BRANCK ));
            }

            if ( !File.Exists( localFilelistPath ) )
            {
                FileStream fs = new FileStream( localFilelistPath, FileMode.Create );
                fs.Flush();
                fs.Close();

                // if local file list didn't exist , then need to download
                needForceUpdate = true;
            }

            DebugUtils.Log( DebugUtils.Type.DownloadResource, "localFilelistPath: " + localFilelistPath );

            if( !Directory.Exists( localAssetFolder ) )
            {
                Directory.CreateDirectory( localAssetFolder );
            }

            DebugUtils.Log( DebugUtils.Type.DownloadResource, "localAssetFolder: " + localAssetFolder );

            if( !Directory.Exists( localAssetFolder + RESOURCE_PATH ) )
            {
                Directory.CreateDirectory( localAssetFolder + RESOURCE_PATH );
            }

            DebugUtils.Log( DebugUtils.Type.DownloadResource, "localAssetFolder: " + localAssetFolder + RESOURCE_PATH );


            if( !Directory.Exists( localAssetFolder + PROTO_PATH ) )
            {
                Directory.CreateDirectory( localAssetFolder + PROTO_PATH );
            }

            DebugUtils.Log( DebugUtils.Type.DownloadResource, "localAssetFolder: " + localAssetFolder + PROTO_PATH );


            if( !Directory.Exists( localAssetFolder + LUA_PATH ) )
            {
                Directory.CreateDirectory( localAssetFolder + LUA_PATH );
            }

            DebugUtils.Log( DebugUtils.Type.DownloadResource, "localAssetFolder: " + localAssetFolder + LUA_PATH );

            if ( !Directory.Exists( localAssetFolder + ZIP_PATH ) )
            {
                Directory.CreateDirectory( localAssetFolder + ZIP_PATH );
            }

            DebugUtils.Log( DebugUtils.Type.DownloadResource, "localAssetFolder: " + localAssetFolder + ZIP_PATH );

            versionLabel = transform.Find( "Label_Version" ).gameObject.GetComponent<UILabel>();
            downloadProgerssLabel = transform.Find( "Label_Progress" ).gameObject.GetComponent<UILabel>();
            downloadProgerssSlider = transform.Find( "Slider" ).gameObject.GetComponent<UISlider>();
        }

        // Use URL QueryString send the signature
        // Authorization = “KSS YourAccessKey:Signature”
        private string BuildAuthorization()
        {
            // temp
            string url = "";
            string expires = "";// needs to be GMT

            return string.Format( "{0}?KSSAccessKeyId={1}&Expires={1}&Signature={3}", url, accessKey, expires, BuildSignature() );
        }

        // Signature = Base64(HMAC-SHA1(YourSecretKey, UTF-8-Encoding-Of( StringToSign ) ) );
        // StringToSign = HTTP-Verb + "\n" +
        //                Content-MD5 + "\n" +
        //                Content-Type + "\n" +
        //                Date + "\n" +
        //                [CanonicalizedKssHeaders + "\n" +]
        //                CanonicalizedResource;
        private string BuildSignature()
        {
            string stringToSign = "";

            string httpVerb = "GET";
            string contentMD5 = "";// base64 
            string contentType = "";
            string date = DateTime.Now.ToString( "r" ); // datetime needs to be GMT;
            string canonicalizedKssHeaders = "";
            string canonicalizedResource = "";

            stringToSign = string.Format( "{0}\n{1}\n{2}\n{3}\n{4}\n{5}", httpVerb, contentMD5, contentType, date, canonicalizedKssHeaders, canonicalizedResource );

            HMACSHA1 h = new HMACSHA1( UTF8Encoding.UTF8.GetBytes( string.Format( "{0}{1}", secretKey, stringToSign ) ) );

            return Convert.ToBase64String( h.Hash );
        }

        public void CheckResourceVersion( Callback onCheckFinishend , AssetBundleManager abManager )
        {
            downloadProgerssLabel.text = "检查更新...";
            this.onCheckFinished = onCheckFinishend;
            this.abManager = abManager;
            localVersionCode = PlayerPrefs.GetInt( VERSION_CODE_KEY );
            StartCoroutine( GetVersionCode() );
        }

        private IEnumerator GetVersionCode()
        {
            WWW www = new WWW( SERVER_VERSION_FILE_URL );
            yield return www;

            if( !string.IsNullOrEmpty( www.error ) )
            {
                DebugUtils.LogError( DebugUtils.Type.DownloadResource, string.Format( "GetVersionCode from server {0} failed! error: {1} ", SERVER_VERSION_FILE_URL, www.error ) );

                LoadComplete ();
            }
            else
            {

                if( !int.TryParse( www.text, out serverVersionCode ) )
                {
                    DebugUtils.LogError( DebugUtils.Type.DownloadResource, string.Format( "Server version code format failed : version code = {0}", www.text ) );
                }

                DebugUtils.Log( DebugUtils.Type.DownloadResource, string.Format( "Server versionCode = {0}", www.text ) );

                www.Dispose();
                www = null;

                // test code!
                //localVersionCode = 0;

                if( localVersionCode != serverVersionCode || needForceUpdate )
                {
                    DebugUtils.Log( DebugUtils.Type.DownloadResource, string.Format( "Update Now! localVersion:{0} serverVersion:{1} ", localVersionCode, serverVersionCode ) );
                    abManager.RemoveLocalZipListFile();
                    StartCoroutine( DownloadFileList() );
                }
                else
                {
                    DebugUtils.Log( DebugUtils.Type.DownloadResource, string.Format( "Don't need update! localVersion:{0} serverVersion:{1} ", localVersionCode, serverVersionCode ) );
                    //gameObject.SetActive( false );
                    LoadComplete ();
                }
            }
            yield return null;
        }

        private IEnumerator DownloadFileList()
        {
            StartCoroutine( DownLoadFile( "file:///" + localFilelistPath, delegate( WWW www ) {
                if( www == null )
                {
                    DebugUtils.LogError( DebugUtils.Type.DownloadResource, string.Format( "Load local filelist failed! {0}", localFilelistPath ) );
                }
                else
                {

                    if(!string.IsNullOrEmpty(www.text))
                    {
                        ParseFileList( www.text, localFileList, localBundleDic );
                    }
                    StartCoroutine( DownLoadFile( SERVER_FILELIST_PATH, delegate( WWW serverWWW ) {
                        if( serverWWW == null )
                        {
                            DebugUtils.LogError( DebugUtils.Type.DownloadResource, string.Format( "Load server filelist failed! {0}", localFilelistPath ) );
                        }
                        else
                        {
                            ParseFileList( serverWWW.text, serverFileList, serverBundleDic );
                            CompareFileList();
                        }
                    } ) );
                }
            } ) );
            yield return null;
        }

        private IEnumerator DownLoadFile( string url, HandleFinishDownload onFinishedDownload )
        {
            DebugUtils.Log( DebugUtils.Type.DownloadResource, string.Format( "Down load file url :{0}", url ) );
            WWW www = new WWW( url );

            yield return www;

            if ( !www.isDone || !string.IsNullOrEmpty( www.error ) )
            {
                DebugUtils.LogError( DebugUtils.Type.DownloadResource, string.Format( "Down load file failed! error msg:{0} isDone:{1} url = {2}", www.error, www.isDone, url ) );

                onFinishedDownload( null );
            }
            else
            {
                onFinishedDownload( www );
            }

            www.Dispose();
            www = null;
        }


        private void ParseFileList( string text, Dictionary<string , string> versionDic, Dictionary<string , string> assetPathDic )
        {

            string[] items = text.Split( '\n' );

            string[] buffer;

            string fileMd5 = string.Empty;
            string filePath = string.Empty;
            string fileVersion = string.Empty;

            for( int i = 0; i < items.Length; i++ )
            {
                if ( string.IsNullOrEmpty( items[i] ) )
                {
                    continue;
                }
                try
                {
                    buffer = items[i].Split( '#' );

                    if( buffer != null )
                    {
                        //buffer may contain    md5--fileType--versionCode--length--checksum
                        //buffer.Length == 4  no checksum
                        if ( buffer.Length == 3 )//local
                        {                            
                            versionDic.Add( buffer[0] , buffer[2] );
                            assetPathDic.Add( buffer[0] , buffer[1] );
                        }
                        else if ( buffer.Length == 4 || buffer.Length == 5 )//server
                        {
                            versionDic.Add( buffer[0], buffer[2] );
                            assetPathDic.Add( buffer[0] , buffer[1] );
                            serverFileLengthDic.Add( buffer[0] , int.Parse( buffer[3] ) );
                            //serverCheckSumDic.Add( buffer[0] , int.Parse( buffer[4] ) );
                        }
                        else
                        {
                            DebugUtils.LogError( DebugUtils.Type.DownloadResource, string.Format( "{0} buffer.Length {1} Not Match 4 or 5", buffer[0].ToString(), buffer.Length ) );
                        }
                    }
                }
                catch( Exception e )
                {
                    DebugUtils.LogError( DebugUtils.Type.DownloadResource, string.Format( "{0} error data :{1}", items[i] , e.ToString() ) );
                }
            }
        }

        private void CompareFileList()
        {
            List<string> needDownloadFiles = new List<string>();

            // this code used to compare version code for every file.
            foreach( KeyValuePair<string, string> item in serverFileList )
            {
                string serverFileMd5 = item.Key;
                string serverFileVersionCode = item.Value;
                if( !localFileList.ContainsKey( serverFileMd5 ) )
                {
                    needDownloadFiles.Add( serverFileMd5 );                    
                    DebugUtils.Log( DebugUtils.Type.DownloadResource, string.Format( "add new file : {0} ", serverFileMd5 ) );
                }
                else
                {
                    string localFileVersionCode = localFileList[serverFileMd5];
                    if( !localFileVersionCode.Equals( serverFileVersionCode ) )
                    {
                        needDownloadFiles.Add( serverFileMd5 );                        
                        DebugUtils.Log( DebugUtils.Type.DownloadResource, string.Format( "update already exsit file : {0} ", serverFileMd5 ) );
                    }
                }
            }

            // now all the resource server will be download!
            //foreach ( KeyValuePair<string, string> item in serverFileList )
            //{
            //    needDownloadFiles.Add( item.Key );
            //}

            // begin the download
            if( needDownloadFiles.Count > 0 )
            {
                totalNeedUpdateCount = needDownloadFiles.Count;
                DebugUtils.Log( DebugUtils.Type.DownloadResource, string.Format( "Begin download update asset! Total: {0} ", totalNeedUpdateCount ) );              
                isLoading = true;

                DownLoadAssetBundle( needDownloadFiles );
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.DownloadResource, string.Format( "Nothing to update!" ) );
                //gameObject.SetActive( false );
                LoadComplete ();
            }
        }

        private void DownLoadAssetBundle( List<string> downloadList )
        {
            if( downloadList.Count == 0 )
            {
                // Download completed !
                // update local version file
                WriteNewFileListToLocal( true );
                return;
            }

            //  some ui operation

            //  download operation
            string currentDownloadFile = downloadList[0];
            downloadList.RemoveAt( 0 );

            string assetBundlePathTag = string.Empty;
            string serverUrl = string.Empty;

            if( serverBundleDic.ContainsKey( currentDownloadFile ) )
            {
                assetBundlePathTag = serverBundleDic[currentDownloadFile];
            }

            if( assetBundlePathTag.Equals( PROTO_PATH_TAG ) )
            {
                serverUrl = SERVER_PROTO_URL;
            }
            else if( assetBundlePathTag.Equals( LUA_PATH_TAG ) )
            {
                serverUrl = SERVER_LUA_PATH;
            }
            else if ( assetBundlePathTag.Equals( RESOURCE_PATH_TAG ) )
            {                
                serverUrl = SERVER_ASSETBUNDLE_PATH;
                AssetBundleManager.LoadBundleList.Add( currentDownloadFile ); //TODO: do this in a cleaner way                
            }
            else
            {
                serverUrl = SERVER_ZIP_PATH;
                abManager.AddLoadZipName( currentDownloadFile ); //TODO: do this in a cleaner way 
            }

            string currentAssetBundleName = string.Format( "{0}{1}", serverUrl, currentDownloadFile );

            try
            {
                StartCoroutine( DownLoadFile( currentAssetBundleName, delegate( WWW www ) {
                    if( www != null )
                    {
                        if( www.bytes.Length != 0 )
                        {
                            //if ( www.bytes.Length != serverFileLengthDic[currentDownloadFile] )
                            //{
                            //    DebugUtils.LogError( DebugUtils.Type.DownloadResource ,
                            //        string.Format( "{0} type {1} Length Not Match {2},{3}", 
                            //        currentDownloadFile, serverBundleDic[currentDownloadFile], www.bytes.Length, serverFileLengthDic[currentDownloadFile] ) );
                            //}
                            //if ( GetFileCheckSum( www.bytes ) != serverCheckSumDic[currentDownloadFile] )
                            //{
                            //    DebugUtils.LogError( DebugUtils.Type.DownloadResource ,
                            //        string.Format( "{0}Chectsum Not Match {1} {2}", currentDownloadFile, GetFileCheckSum( www.bytes ), serverCheckSumDic[currentDownloadFile] ) );
                            //}
                            DebugUtils.Log( DebugUtils.Type.DownloadResource, string.Format( " {0} has been downloaded! ", currentAssetBundleName ) );

                            string resPath = currentAssetBundleName.Replace( RESOURCE_SERVER_URL, "" );
                            
                            string resName = currentAssetBundleName.Replace( RESOURCE_SERVER_URL, "" );
                            

                            ReplaceLocalRes( resPath, www.bytes );
                            UpdateLocalAssetFilelist( Path.GetFileName( resName ) );
                        }
                        else
                        {
                            DebugUtils.LogError( DebugUtils.Type.DownloadResource, string.Format( " {0}'s size is zero! ", currentAssetBundleName ) );
                        }

                        DownLoadAssetBundle( downloadList );
                    }
                    else
                    {
                        DebugUtils.LogError( DebugUtils.Type.DownloadResource, string.Format( " {0} downloaded! failed! ", currentAssetBundleName ) );
                        DebugUtils.LogOnScreen( string.Format( " {0} downloaded! failed! ", currentAssetBundleName ) );
                    }

                } ) );
            }
            catch
            {
                Debug.LogError( "throw an exception!" );
                WriteNewFileListToLocal( false );
            }
        }

        private void UpdateLocalAssetFilelist( string name )
        {
            if( localFileList.ContainsKey( name ) )
            {
                localFileList[name] = serverFileList[name];
            }
            else
            {
                localFileList.Add( name, serverFileList[name] );
                localBundleDic.Add( name, serverBundleDic[name] );
            }
        }

        // when update download finished or a exception was throw , write current file list to local
        // So , when downloading was interrupt, it can start update on last break point
        private void WriteNewFileListToLocal( bool isComplete )
        {
            using( FileStream fs = new FileStream( localFilelistPath, FileMode.Create ) )
            {
                StringBuilder localFileData = new StringBuilder();

                foreach( KeyValuePair<string, string> item in localFileList )
                {
                    string pathTag = localBundleDic[item.Key];

                    if( string.IsNullOrEmpty( pathTag ) )
                    {
                        localFileData.Append( item.Key ).Append( "#" ).Append( item.Value ).Append( "\n" );
                    }
                    else
                    {
                        localFileData.Append( item.Key ).Append( "#" ).Append( pathTag ).Append( "#" ).Append( item.Value ).Append( "\n" );
                    }
                }

                byte[] data = Encoding.UTF8.GetBytes( localFileData.ToString() );
                fs.Write( data, 0, data.Length );
                fs.Flush();
                fs.Close();
            }

            if( isComplete )
            {
                localVersionCode = serverVersionCode;
                PlayerPrefs.SetInt( VERSION_CODE_KEY, localVersionCode );
                PlayerPrefs.Save();

                LoadComplete ();

                DebugUtils.Log( DebugUtils.Type.DownloadResource, "Local resource file list has been updated!" );
            }
            else
            {
                // Restart!
                DebugUtils.LogError( DebugUtils.Type.DownloadResource, "Local resource file download was interrupt, need restart and download agian!" );
            }

        }

        private void ReplaceLocalRes( string fileName, byte[] bytes )
        {
            if( !string.IsNullOrEmpty( fileName ) )
            {
                string path = string.Format( "{0}{1}", localAssetFolder, fileName );

                if ( File.Exists( path ) )
                {
                    File.Delete( path );
                }

                using ( FileStream fs = new FileStream( path, FileMode.Create ) )
                {
                    fs.Write( bytes, 0, bytes.Length );
                    fs.Flush();
                    fs.Close();
                }
                updatedAssetList.Add( Path.GetFileName( fileName ) );
                DebugUtils.Log( DebugUtils.Type.DownloadResource, string.Format( " {0} has been updated! ", fileName ) );
            }
        }

        int shuliang = 0;
        void Update()
        {
            if( abManager != null && abManager.upZipProgressData != null )
            {
                if ( abManager.upZipProgressData.zipNum == 0 )
                {
                    downloadProgerssSlider.value = 1;
                    downloadProgerssLabel.text = "检查本地文件..";
                }
                else
                {
                    downloadProgerssSlider.value = ( float )abManager.upZipProgressData.curIndex / abManager.upZipProgressData.zipNum;
                    if(shuliang == 0)
                    {
                        downloadProgerssLabel.text = "正在擦拭战刀";
                    }
                    if ( shuliang == 50)
                    {
                        downloadProgerssLabel.text = "正在擦拭战刀.";
                    }
                    else if ( shuliang == 100 )
                    {
                        downloadProgerssLabel.text = "正在擦拭战刀..";
                    }
                    else if ( shuliang == 150 )
                    {
                        downloadProgerssLabel.text = "正在擦拭战刀...";
                    }
                    if( shuliang > 199)
                    {
                        shuliang = 0;
                    }
                    else
                    {
                        shuliang++;
                    }
                }
            }

            if( !isLoading )
            {
                return;
            }

            float progress = (float)updatedAssetList.Count / totalNeedUpdateCount;
            downloadProgerssSlider.value = progress;

            if( progress == 1f )
            {
                downloadProgerssLabel.text = DownloadFinishedDescription;
                //gameObject.SetActive( false );
                isLoading = false;
            }
            else
            {
                versionLabel.text = string.Format( "远端资源版本号:{0}   本地资源版本号:{1}   游戏版本号:{2}", serverVersionCode, localVersionCode ,VersionUtil.Instance.curVersion );
                downloadProgerssLabel.text = string.Format( " 下载进度： {0} {1} / {2} ", DownloadProgressDescription, updatedAssetList.Count, totalNeedUpdateCount );
            }

        }

        private void LoadComplete ()
        {
            abManager.Init ( delegate ()
            {
                gameObject.SetActive( false );
                onCheckFinished ();
                Reset ();
            } );
        }

        private void Reset()
        {
            localFileList.Clear ();
            serverFileList.Clear ();
            updatedAssetList.Clear ();
            serverBundleDic.Clear ();
            localBundleDic.Clear ();
        }

        private int GetFileCheckSum( byte[] bytes )
        {
            int i, sum = 0;
            for ( i = 0; i < bytes.Length; i++ )
            {
                sum += bytes[i];

                if ( sum > 0xff )
                {
                    sum = ~sum;
                    sum += 1;
                }

                sum = sum & 0xff;
            }
            return sum;
        }
    }

    public class UpZipProgressData
    {
        public int zipNum;
        public int curIndex;
        public string desc;
        public float progress;
    }
}