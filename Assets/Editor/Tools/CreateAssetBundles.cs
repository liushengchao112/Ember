using System;
using UnityEditor;
using System.IO;
using SevenZip.Compression.LZMA;
using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Ionic.Zip;
//using Ionic.Zip;

using Utils;
using Constants;
using UObject = UnityEngine.Object;

namespace Resource
{
    public enum BundleResourceType
    {
        Characters,
        Buildings,
        NPC,
        Particle_Character,
        Particle_Map,
        Particle_UI,
        Particle_Building,
        UI_Load,            //UI load prefab
        Sound_BGM,
        Sound_Battle,
        Sound_MainMenu,
        Sound_Common,
        Shaders,
    }


    public enum AssetBundleCompression { chunk, clzf, lzma };

    public class CreateAssetBundles
    {
        enum BundleDataSearchType
        {
            CombineAll = 0,  //Combine all the files generate a AB
            CombineFolder,   //Combine all files in a folder to generate a AB
            AloneFile,       //Make the file generate a AB
        }

        public static AssetBundleCompression compressionType = AssetBundleCompression.lzma;

        public static string savePath( bool local )
        {
            return local ? Application.streamingAssetsPath + "/" : Directory.GetCurrentDirectory() + "/AssetBundles";
        }

        class AssetBundleBuildData
        {
            public string bundleResourceType;
            public List<AssetBundleBuild> buildList;
        }
        static List<AssetBundleBuildData> assetBundleBuildDataList = new List<AssetBundleBuildData>();

        static string bundleResourceDetails = "bundleResourceDetails.txt";

        static string tempAssetListTxt = "tempAssetList.txt";
        static string tempHeadSymbol = "(<*&*>)";
        static string tempBodySymbol = "(<~%~>)";

        static string common_depend_asset = "common_depend_asset";

        static string bundleConfigPath = "Assets/Editor/BuildScripts/buildAssetBundleConfig.asset";
        static BuildAssetBundleConfig buildAssetBundleConfig = null;
        static int changeBundleNum = 0;

        static string Platform( BuildTarget target )
        {
            switch ( target )
            {
                case BuildTarget.iOS: return "ios";
                case BuildTarget.Android: return "android";
                case BuildTarget.StandaloneOSXIntel64: return "osx";
                case BuildTarget.WebGL: return "webgl";
                case BuildTarget.StandaloneWindows64:
                default:
                return "windows";
            }
        }

        //[MenuItem( "Assets/Build AssetBundles" )]
        static void BuildAllAssetBundles()
        {
            try { BuildIOSAssetBundles(); }
            catch ( Exception e ) { UnityEngine.Debug.LogError( e.Message ); }

            try { BuildAndroidAssetBundles(); }
            catch ( Exception e ) { UnityEngine.Debug.LogError( e.Message ); }

            try { BuildWindows64AssetBundles(); }
            catch ( Exception e ) { UnityEngine.Debug.LogError( e.Message ); }
        }

        //[MenuItem( "Assets/Build IOS AssetBundles" )]
        static void BuildIOSAssetBundles() { BuildAllAssetBundle( BuildTarget.iOS ); }

        //[MenuItem( "Assets/Build Android AssetBundles" )]
        static void BuildAndroidAssetBundles() { BuildAllAssetBundle( BuildTarget.Android ); }

        //[MenuItem( "Assets/Build Windows 64 AssetBundles" )]
        static void BuildWindows64AssetBundles() { BuildAllAssetBundle( BuildTarget.StandaloneWindows ); }

        public static void BuildAllAssetBundle( BuildTarget target, bool isLocal = false)
        {
            List<BundleResourceType> list = new List<BundleResourceType>();
            foreach ( BundleResourceType type in Enum.GetValues( typeof( BundleResourceType ) ) )
            {
                list.Add( type );
            }
            BuildAssetBundles( target, list.ToArray(), isLocal );
        }

        public static void BuildAssetBundles( BuildTarget target, BundleResourceType[] bundleTypes, bool isLocal )
        {
            Debug.Log( string.Format( "<color=#00FF00>Start build assetBundle! time:{0}</color>", DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss,fff" ) ) );

            string path = savePath( isLocal ) + "/" + Platform( target ) + "/";

            //root dir
            if( !Directory.Exists( path ) )
            {
                Directory.CreateDirectory( path );
            }
            else if ( isLocal )
            {
                //Directory.Delete ( path, true );
                //AssetDatabase.Refresh ();
            }

            //del temp file
            if ( File.Exists ( Directory.GetCurrentDirectory () + "/" + tempAssetListTxt ) )
            {
                File.Delete ( Directory.GetCurrentDirectory () + "/" + tempAssetListTxt );
            }
            //del last hot update file
            if ( Directory.Exists( path + "assetbundle/hotUpdate/" ) )
            {
                Directory.Delete( path + "assetbundle/hotUpdate/", true );
            }
            //del zip dir
            if ( Directory.Exists( path + "zip/" ) )
            {
                Directory.Delete( path + "zip/", true );
            }
			//del zip txt
			if ( File.Exists( path + "../" + AssetBundleManager.zipFileListTxt ) )
			{
				File.Delete( path + "../" + AssetBundleManager.zipFileListTxt );
			}

            changeBundleNum = 0;
            assetBundleBuildDataList.Clear();

            EditorUtility.DisplayProgressBar ( "准备AB数据", "当前进入:92%", 0.92f );
            foreach ( BundleResourceType type in bundleTypes )
            {
                BuildBundleHandler( target, type, isLocal );
            }
            //add common asset
            CheckMultipleAsset();
            EditorUtility.ClearProgressBar();
            //start build
            StartBuildBundles( target, isLocal );

            Debug.Log( string.Format( "<color=#00FF00>Build assetBundle done! time:{0}</color>", DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss,fff" ) ) );
        }

        static void StartBuildBundles( BuildTarget target, bool isLocal )
        {
            string tempPath = string.Format ( "{0}/{1}/{2}", savePath ( isLocal ), Platform ( target ), GameConstants.BundleManifest );
            if ( !Directory.Exists( tempPath ) )
            {
                Directory.CreateDirectory( tempPath );
            }

            List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();
            foreach ( var abdl in assetBundleBuildDataList )
            {
                if ( abdl.buildList != null )
                {
                    buildList.AddRange( abdl.buildList );
                }
            }

            BuildAssetBundleOptions op = BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.IgnoreTypeTreeChanges;
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles ( tempPath, buildList.ToArray (), op, target );

            if( manifest == null )
            {
                return;
            }
            bool first = true;
            foreach ( var abdl in assetBundleBuildDataList )
            {
                GenerateResourceDetails( tempPath, abdl.bundleResourceType, abdl.buildList, first );
                CompareBundleConfigHandler( tempPath, abdl.bundleResourceType, manifest, isLocal );
                first = false;
            }

            first = true;
            foreach ( var abdl in assetBundleBuildDataList )
            {
                CompressZip( tempPath, abdl.bundleResourceType, Platform( target ), first );
                first = false;
            }

            if( changeBundleNum > 0 )
            {
                //Compress AssetBundleManifest
                CreateZip ( tempPath + "/" + GameConstants.BundleManifest,
                            string.Concat ( tempPath, "/../zip/", GameConstants.BundleManifest, ".zip" ),
                            tempPath + "/../../" + AssetBundleManager.zipFileListTxt,
                            Platform ( target ) + "/zip/" );
            }

            //GenerateConfig( tempPath, manifest );
            if ( buildAssetBundleConfig != null )
            {
                EditorUtility.SetDirty ( buildAssetBundleConfig );
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            GC.Collect();

        }

        static void BuildBundleHandler( BuildTarget target, BundleResourceType type, bool saveLocal )
        {
            switch ( type )
            {
                case BundleResourceType.Characters:
                {
                    /**
                     * Characters
                     * The character's prefab generates a Assetbundle file, and the Assetbundle file compresses zip.
                     */
                    PrepareBundleListData( target, "Art/Character", new string[] { "*.prefab" }, type, BundleDataSearchType.AloneFile, saveLocal, null,
                        delegate ( string curFilePath, List<string> list )
                        {
                            //Some attachments need to be added, such as sound.
                            string fName = Path.GetFileNameWithoutExtension( curFilePath );
                            GetAccessory( curFilePath, "Sounds", string.Concat( "*_", fName, ".*" ), list );
                        } );
                    break;
                }
                case BundleResourceType.Buildings:
                {
                    /**
                     * Buildings
                     * The buildings's prefab generates a Assetbundle file, and the Assetbundle file compresses zip.
                     */
                    PrepareBundleListData( target, "Art/Building", new string[] { "*.prefab" }, type, BundleDataSearchType.AloneFile, saveLocal, new List<string>() { "/Bases/" } );
                    break;
                }
                case BundleResourceType.NPC:
                {
                    /**
                     * NPC
                     * The buildings's prefab generates a Assetbundle file, and the Assetbundle file compresses zip.
                     */
                    PrepareBundleListData( target, "Art/Npc", new string[] { "*.prefab" }, type, BundleDataSearchType.AloneFile, saveLocal );
                    break;
                }
                case BundleResourceType.Particle_Character:
                {
                    /**
                     * Character's effects
                     * The character of all the effects generate a Assetbundle file ,and compresses zip.
                     */
                    PrepareBundleListData( target, "Art/Particle/Character/Prefabs", new string[] { "*.prefab" }, type, BundleDataSearchType.CombineFolder, saveLocal );
                    break;
                }
                case BundleResourceType.Particle_UI:
                {
                    /**
                     * UI effects
                     * Generates a Assetbundle file and compress zip.
                     */
                    PrepareBundleListData( target, "Art/Particle/UI", new string[] { "*.prefab" }, type, BundleDataSearchType.CombineAll, saveLocal );
                    break;
                }
                case BundleResourceType.Particle_Map:
                {
                    /**
                     * Map's effects
                     * Generates a Assetbundle file and compress zip.
                     */
                    PrepareBundleListData( target, "Art/Particle/Map", new string[] { "*.prefab" }, type, BundleDataSearchType.CombineAll, saveLocal );
                    break;
                }
                case BundleResourceType.Particle_Building:
                {
                    /**
                     * Building's common effects
                     * Generates a Assetbundle file and compress zip.
                     */
                    PrepareBundleListData( target, "Art/Particle/Building", new string[] { "*.prefab" }, type, BundleDataSearchType.CombineAll, saveLocal );
                    break;
                }
                case BundleResourceType.UI_Load:
                {
                    /**
                     * UI load textrue
                     * Generate AssetBundle by folder and compress zip.
                     */
                    PrepareBundleListData( target, "Art/UI/UILoadPrefabs", new string[] { "*.prefab" }, type, BundleDataSearchType.CombineFolder, saveLocal );
                    break;
                }
                case BundleResourceType.Sound_BGM:
                {
                    /**
                     * Background music 
                     * Each BGM file generates an Assetbundle file separately
                     */
                    PrepareBundleListData( target, "Art/AudioClip/BGM", new string[] { "*.mp3", "*.wav" }, type, BundleDataSearchType.AloneFile, saveLocal );
                    break;
                }
                case BundleResourceType.Sound_Battle:
                {
                    /**
                     * Battle used sound
                     * All files are generated by an Assetbundle file
                     */
                    PrepareBundleListData( target, "Art/AudioClip/Battle", new string[] { "*.mp3", "*.wav" }, type, BundleDataSearchType.CombineAll, saveLocal );
                    break;
                }
                case BundleResourceType.Sound_MainMenu:
                {
                    /**
                     * The sound used by non-combat scenes
                     * All files are generated by an Assetbundle file
                     */
                    PrepareBundleListData( target, "Art/AudioClip/MainMenu", new string[] { "*.mp3", "*.wav" }, type, BundleDataSearchType.CombineAll, saveLocal );
                    break;
                }
                case BundleResourceType.Sound_Common:
                {
                    PrepareBundleListData( target, "Art/AudioClip/Common", new string[] { "*.mp3", "*.wav" }, type, BundleDataSearchType.CombineAll, saveLocal );
                    break;
                }
                case BundleResourceType.Shaders:
                {
                    PrepareBundleListData( target, "Art/Shaders", new string[] { "*.shader" }, type, BundleDataSearchType.CombineFolder, saveLocal, new List<string>() { "/Common" } );
                    break;
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetPath"></param>
        /// <param name="patterns"></param>
        /// <param name="bundleResourceType"></param>
        /// <param name="bundleDataSearchType"></param>
        /// <param name="isLocal"></param>
        /// <param name="screeneds"></param>
        /// <param name="callback"></param>
        static void PrepareBundleListData( BuildTarget target,
                                           string targetPath,
                                           string[] patterns,
                                           BundleResourceType bundleResourceType,
                                           BundleDataSearchType bundleDataSearchType,
                                           bool isLocal,
                                           List<string> screeneds = null,
                                           Action<string, List<string>> callback = null )
        {

            targetPath = Application.dataPath + "/" + targetPath;
            if ( !Directory.Exists( targetPath ) )
            {
                //Debug.LogError( "target path not found:" + Application.dataPath + "/" + targetPath );
                return;
            }

            List<AssetBundleBuild> list = null;

            if ( bundleDataSearchType == BundleDataSearchType.CombineAll )
            {
                list = CombineAllBundleData( target, targetPath, patterns, bundleResourceType, screeneds );
            }
            else if ( bundleDataSearchType == BundleDataSearchType.CombineFolder )
            {
                list = CombineFolderBundleData( target, targetPath, patterns, bundleResourceType, screeneds, callback );
            }
            if ( bundleDataSearchType == BundleDataSearchType.AloneFile )
            {
                list = AloneFileBundleData( target, targetPath, patterns, bundleResourceType, screeneds, callback );
            }


            if( list.Count > 0 )
            {
                OnSearchDependency( bundleResourceType, list );

                AssetBundleBuildData buildData = new AssetBundleBuildData();
                buildData.buildList = list;
                buildData.bundleResourceType = bundleResourceType.ToString();
                assetBundleBuildDataList.Add( buildData );
            }
           
        }

        static void CheckMultipleAsset()
        {
            string path = Directory.GetCurrentDirectory () + "/" + tempAssetListTxt;
            if ( !File.Exists ( path ) )
            {
                return;
            }

            string[] lines = File.ReadAllLines( path );

            Dictionary<string, List<string>> assetDic = new Dictionary<string, List<string>>();
            foreach ( var line in lines )
            {
                if ( string.IsNullOrEmpty( line ) ) continue;
                string str = line.Trim();
                string[] strs = str.Split( new string[] { tempHeadSymbol }, StringSplitOptions.None );
                string[] datas = strs[1].Split( new string[] { tempBodySymbol }, StringSplitOptions.None );
                assetDic[strs[0]] = new List<string>( datas );
            }

            List<string> assetList = new List<string>();
            List<string> tempList = new List<string>();
            foreach ( var asset in assetDic )
            {
                List<string> list = asset.Value;
                if ( list.Count == 0 ) continue;

                foreach ( var str in list )
                {
                    if ( !tempList.Contains( str ) )
                    {
                        tempList.Add( str );
                    }
                    else if( !assetList.Contains( str ) )
                    {
                        assetList.Add( str );
                    }
                }
            }
            tempList = null;

            for ( int i = 0; i < assetBundleBuildDataList.Count; i++ )
            {
                for ( var z = 0; z < assetBundleBuildDataList[i].buildList.Count; ++z )
                {
                    List<AssetBundleBuild> buildList = assetBundleBuildDataList[i].buildList;
                    if ( buildList[z].assetBundleName.EndsWith( "depend_asset" + GameConstants.BundleExtName ) )
                    {
                        List<string> assetNameList = new List<string>( buildList[z].assetNames );
                        for ( var j = 0; j < assetNameList.Count; ++j )
                        {
                            foreach ( var str in assetList )
                            {
                                if ( assetNameList[j].Equals( str ) )
                                {
                                    assetNameList.RemoveAt( j );
                                    --j;
                                    break;
                                }
                            }
                        }
                        if ( assetNameList.Count > 0 )
                        {
                            AssetBundleBuild abb = new AssetBundleBuild();
                            abb.assetBundleName = buildList[z].assetBundleName;
                            abb.assetBundleVariant = buildList[z].assetBundleVariant;
                            abb.assetNames = assetNameList.ToArray();
                            assetBundleBuildDataList[i].buildList[z] = abb;
                        }
                        else
                        {
                            assetBundleBuildDataList[i].buildList.RemoveAt( z );
                            --z;
                        }
                        break;
                    }
                }
            }

            List<string> topAllAssetList = new List<string>();
            foreach ( var objPath in assetList )
            {
                if( !topAllAssetList.Contains( objPath ) )
                {
                    topAllAssetList.Add( objPath );
                }
            }

            AssetBundleBuildData assetBuildData = new AssetBundleBuildData();
            assetBuildData.buildList = new List<AssetBundleBuild>();
            assetBuildData.bundleResourceType = common_depend_asset;
            AssetBundleBuild assetBundle = new AssetBundleBuild();
            assetBundle.assetBundleName = GetBundleNamePrefix( assetBuildData.bundleResourceType ) + "res" + GameConstants.BundleExtName;
            assetBundle.assetBundleName = CheckBundleName( assetBundle.assetBundleName );
            assetBundle.assetNames = topAllAssetList.ToArray();
            assetBuildData.buildList.Add( assetBundle );
            assetBundleBuildDataList.Insert( 0, assetBuildData );

            File.Delete( path );
        }

        static AssetBundleBuild GetAssetBundleBuildData (string type)
        {
            for ( int i = 0; i < assetBundleBuildDataList.Count; i++ )
            {
                if ( assetBundleBuildDataList[i].bundleResourceType == type )
                {
                    foreach( var b in assetBundleBuildDataList[i].buildList )
                    {
                        if(b.assetBundleName.Equals( GetBundleNamePrefix( type ) + "depend_asset" + GameConstants.BundleExtName))
                        {
                            return b;
                        }
                    }
                }
            }
            return new AssetBundleBuild();
        }

        static void SetAssetBundleBuildData( string type , AssetBundleBuild data )
        {
            for ( int i = 0; i < assetBundleBuildDataList.Count; i++ )
            {
                if ( assetBundleBuildDataList[i].bundleResourceType == type )
                {
                    for ( int j = 0; j < assetBundleBuildDataList[i].buildList.Count;j ++  )
                    {
                        if ( assetBundleBuildDataList[i].buildList[j].assetBundleName.Equals( GetBundleNamePrefix( type ) + "depend_asset" + GameConstants.BundleExtName ) )
                        {
                            assetBundleBuildDataList[i].buildList[j] = data;
                        }
                    }
                }
            }
        }

        static void OnSearchDependency( BundleResourceType type, List<AssetBundleBuild> assetBundleBuildList )
        {
            if ( type == BundleResourceType.UI_Load ) return;

            List<string> recordPathList = new List<string>();
            List<string> dependPathList = new List<string>();
            List<UObject> rootObjList = new List<UObject>();
            List<string> assetNameList = new List<string>();

            foreach ( var build in assetBundleBuildList )
            {
                foreach ( var path in build.assetNames )
                {
                    UObject obj = AssetDatabase.LoadAssetAtPath<UObject>( path );
                    UObject[] objs = EditorUtility.CollectDependencies( new UObject[] { obj } );
                    foreach ( var o in objs )
                    {
                        string p = AssetDatabase.GetAssetPath( o );

                        if ( p.Contains( "Resources/unity_builtin_extra" ) || p.Contains( "Library/unity default resources" ) )
                        {
                            Debug.Log( string.Format( "<color=#FFFF00>Find unity internal resources : {0}, name : {1}, res : {2}</color>", p, o.name, path ) );
                        }
                    }
                    rootObjList.AddRange( objs );
                }
                assetNameList.AddRange( build.assetNames );
            }

            //AssetDatabase.GetDependencies
            foreach ( var obj in rootObjList )
            {
                if ( obj == null ) continue;

                string path = AssetDatabase.GetAssetPath( obj );
                
                if ( path.Contains( "Resources/unity_builtin_extra" ) || path.Contains( "Library/unity default resources" ) )
                {
                    continue;
                }

                if ( recordPathList.Contains( path ) )
                {
                    if ( !dependPathList.Contains( path ) )
                    {
                        if (!path.EndsWith( ".cs" )
                            && !path.EndsWith( ".dll" ) 
                            && !path.Contains( "/Editor/" ) 
                            && !path.EndsWith( ".js" ) 
                            && !assetNameList.Contains( path ) )
                        {
                            dependPathList.Add( path );
                        }
                    }
                }
                else
                {
                    recordPathList.Add( path );
                }
            }
            recordPathList = null;

            if ( dependPathList.Count > 0 )
            {
                AssetBundleBuild common_asset = new AssetBundleBuild();
                common_asset.assetBundleName = GetBundleNamePrefix( type.ToString() ) + "depend_asset" + GameConstants.BundleExtName;
				common_asset.assetBundleName = CheckBundleName ( common_asset.assetBundleName );
                common_asset.assetNames = dependPathList.ToArray();
                assetBundleBuildList.Insert( 0, common_asset );
            }

            if( dependPathList.Count > 0)
            {
                GenerateDependencyAssetList( dependPathList, type.ToString() );
            }
        }
        
        static List<AssetBundleBuild> CombineAllBundleData( BuildTarget target, string targetPath, string[] patterns, BundleResourceType bundleResourceType, List<string> screeneds = null )
        {
            List<AssetBundleBuild> list = new List<AssetBundleBuild>();
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = GetBundleNamePrefix( bundleResourceType.ToString() ) + "a" + GameConstants.BundleExtName;
			build.assetBundleName = CheckBundleName (build.assetBundleName);
            List<string> assetNameList = new List<string>();
            for ( int i = 0; i < patterns.Length; i++ )
            {
                string[] files = Directory.GetFiles( targetPath, patterns[i], SearchOption.AllDirectories );
                if ( files.Length == 0 ) continue;
                foreach ( var file in files )
                {
                    //Special folder handler  
                    if( screeneds != null )
                    {
                        bool has = false;
                        foreach ( var str in screeneds )
                        {
                            if ( file.Contains( str ) )
                            {
                                has = true;
                                break;
                            }
                        }
                        if ( has )
                        {
                            continue;
                        }
                    }
                    
                    string curName = file.Replace( '\\', '/' ).Replace( Application.dataPath.Replace( "Assets", "" ), "" );
                    if ( !assetNameList.Contains( curName ) )
                    {
                        assetNameList.Add( curName );
                    }
                }
            }

            if ( assetNameList.Count > 0 )
            {
                build.assetNames = assetNameList.ToArray();
                list.Add( build );
            }
            return list;
        }

        static List<AssetBundleBuild> CombineFolderBundleData( BuildTarget target, string targetPath, string[] patterns, BundleResourceType bundleResourceType, List<string> screeneds = null, Action<string, List<string>> callback = null )
        {
            List<AssetBundleBuild> list = new List<AssetBundleBuild>();
            string[] directories = Directory.GetDirectories( targetPath );
            if ( directories.Length == 0 ) return list;
            foreach ( var dir in directories )
            {
                string dirPath = dir.Replace( '\\', '/' );
                //Special folder handler 
                if ( screeneds != null )
                {
                    bool has = false;
                    foreach ( var str in screeneds )
                    {
                        if ( dirPath.Contains( str ) )
                        {
                            has = true;
                            break;
                        }
                    }
                    if ( has )
                    {
                        continue;
                    }
                }
                
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = GetBundleNamePrefix( bundleResourceType.ToString() ) + Path.GetFileName( dirPath + GameConstants.BundleExtName );
				build.assetBundleName = CheckBundleName (build.assetBundleName);
                List<string> assetNameList = new List<string>();
                for ( int i = 0; i < patterns.Length; i++ )
                {
                    string[] files = Directory.GetFiles( dirPath, patterns[i], SearchOption.AllDirectories );
                    foreach ( var f in files )
                    {
                        string curName = f.Replace( '\\', '/' ).Replace( Application.dataPath.Replace( "Assets", "" ), "" );
                        if ( !assetNameList.Contains( curName ) )
                        {
                            assetNameList.Add( curName );
                        }
                    }
                }

                if ( assetNameList.Count > 0 )
                {
                    build.assetNames = assetNameList.ToArray();
                    list.Add( build );
                }
            }
            return list;
        }

        static List<AssetBundleBuild> AloneFileBundleData( BuildTarget target,
                                                           string targetPath, 
                                                           string[] patterns, 
                                                           BundleResourceType bundleResourceType,
                                                           List<string> screeneds = null,
                                                           Action<string, List<string>> callback = null )
        {
            List<AssetBundleBuild> list = new List<AssetBundleBuild>();
            for ( int i = 0; i < patterns.Length; i++ )
            {
                string[] files = Directory.GetFiles( targetPath, patterns[i], SearchOption.AllDirectories );
                if ( files.Length == 0 ) break;
                foreach ( var f in files )
                {
                    var file = f.Replace( '\\', '/' );

                    //Special folder handler  
                    if( screeneds != null )
                    {
                        bool has = false;
                        foreach ( var str in screeneds )
                        {
                            if ( file.Contains( str ) )
                            {
                                has = true;
                                break;
                            }
                        }
                        if ( has )
                        {
                            continue;
                        }
                    }
                                        
                    AssetBundleBuild build = new AssetBundleBuild();
                    build.assetBundleName = GetBundleNamePrefix( bundleResourceType.ToString() ) + Path.GetFileNameWithoutExtension( file ) + GameConstants.BundleExtName;
					build.assetBundleName = CheckBundleName ( build.assetBundleName );
					List<string> assetNameList = new List<string>();
                    string assetsFile = file.Replace( Application.dataPath.Replace( "Assets", "" ), "" );
                    assetNameList.Add( assetsFile );
                    if ( callback != null )
                    {
                        callback( f, assetNameList );
                    }

                    if( assetNameList.Count > 0 )
                    {
                        build.assetNames = assetNameList.ToArray();
                        list.Add( build );
                    }
                }
            }
            return list;
        }

        static string GetBundleNamePrefix( string bundleResourceType )
        {
            return bundleResourceType.ToString() + "_AB_";
        }

		static string CheckBundleName( string name )
		{
			string str = name.Replace (" ", "");
			str = str.Replace (" ", "");
			return str.Replace (" ", "");
		}

        static void CompareBundleConfigHandler( string path, string type, AssetBundleManifest manifest, bool isSaveLocal )
        {
            string outPath = path + "/hotUpdate/";
           
            if ( !Directory.Exists( outPath ) )
            {
                Directory.CreateDirectory( outPath );
            }

            if( (buildAssetBundleConfig == null || buildAssetBundleConfig.list.Count == 0) && !isSaveLocal )
            {
                ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject> ( bundleConfigPath );
                if ( obj == null )
                {
                    Debug.Log ( "create bundleConfig" );
                    buildAssetBundleConfig = ScriptableObject.CreateInstance<BuildAssetBundleConfig> ();
                    AssetDatabase.CreateAsset ( buildAssetBundleConfig, bundleConfigPath );
                    EditorUtility.SetDirty ( buildAssetBundleConfig );
                }
                else
                {
                    buildAssetBundleConfig = obj as BuildAssetBundleConfig;
                    Debug.Log( "check bundleConfig data count：" + buildAssetBundleConfig.list.Count );
                }
            }

            string bundleNamePrefix = GetBundleNamePrefix( type ) + "*.bundle";
            string[] files = Directory.GetFiles( path, bundleNamePrefix.ToLower() );

            bool hasCopy = false;
            foreach ( var file in files )
            {
               
                FileInfo fileInfo = new FileInfo( file );
                string bundleName = fileInfo.Name;
                string hashName = manifest.GetAssetBundleHash( bundleName ).ToString();

                bool change = true;
                if ( !isSaveLocal )
                {
                    change = buildAssetBundleConfig.Check ( bundleName, hashName );
                }

                if ( change )
                {
                    Debug.Log( "update file：" + file );
                    File.Copy( file, outPath + bundleName, true );
                    if( !isSaveLocal )
                    {
                        buildAssetBundleConfig.Add ( bundleName, hashName, fileInfo.Length );
                    }
                    hasCopy = true;
                }
            }

            if( hasCopy )
            {
                changeBundleNum++;
                Debug.Log( "-------------------------------------------------------" );
            }
        }


        static void CompressZip( string path, string type , string targetName, bool first )
        {
            string outPath = path + "/../zip/";
            string targetPath = path + "/hotUpdate/";
            targetName = targetName + "/zip/";
            string zipFolderPath = path + "/../../" + AssetBundleManager.zipFileListTxt;

            if ( !Directory.Exists( outPath ) )
            {
                Directory.CreateDirectory( outPath );
            }
            
            if ( !File.Exists( zipFolderPath ) )
            {
                using ( StreamWriter writer = new StreamWriter( File.Create( zipFolderPath ) ) )
                {
                    writer.WriteLine( CommonUtil.GetTimeStamp() );
                    writer.Close();
                    writer.Dispose();
                }
            }
            else if( first )
            {
                File.WriteAllText( zipFolderPath, CommonUtil.GetTimeStamp() + "\n" );
            }

            string bundleNamePrefix = GetBundleNamePrefix( type ) + "*.bundle";
            string[] files = Directory.GetFiles( targetPath, bundleNamePrefix.ToLower() );
            foreach ( var file in files )
            {
                if ( !file.EndsWith( ".DS_Store" ) && !file.EndsWith( ".txt" ) && !file.EndsWith( ".meta" ) && !file.EndsWith( ".manifest" ) )
                {
                    string zipName = Path.GetFileNameWithoutExtension( file ) + ".zip";
                    CreateZip( file, outPath + zipName, zipFolderPath, targetName );
                }
            }
        }

        static void CreateZip( string path, string outPath, string zipFolderPath, string targetName )
        {
            string[] files = null;
            if ( Directory.Exists( path ) )
            {
                files = Directory.GetFiles( path );
            }
            else
            {
                if ( File.Exists( path ) )
                {
                    files = new string[] { path };
                }
            }
            
            if ( File.Exists( outPath ) )
            {
                File.Delete( outPath );
            }

            float progress = 1.0f / files.Length;
            using ( ZipFile zip = new ZipFile() )
            {
                bool hasFile = false;
                zip.CompressionLevel = Pathfinding.Ionic.Zlib.CompressionLevel.BestCompression;
                int i = 0;
                foreach ( var file in files )
                {
                    if ( File.Exists( file ) && !file.EndsWith( ".DS_Store" ) && !file.EndsWith( ".txt" ) && !file.EndsWith( ".meta" ) && !file.EndsWith( ".manifest" ) )
                    {
                        hasFile = true;
                        string fileName = Path.GetFileName( file );
                        string md5Path = string.Format( "{0}/temp/{1}", Path.GetDirectoryName( file ), CommonUtil.EncodingToMd5( fileName ) );
                        string tempPath = Path.GetDirectoryName( md5Path ) + "/";
                        if ( !Directory.Exists( tempPath ) )
                        {
                            Directory.CreateDirectory( tempPath );
                        }
                        File.Copy( file, md5Path, true );
                        zip.AddFile( md5Path, "" );
                    }
                }
                if ( hasFile )
                {
                    if( File.Exists( outPath ) )
                    {
                        File.Delete( outPath );
                    }
                    string zipName = Path.GetFileName( outPath );
                    Debug.Log ( "zip name : " + zipName );
                    zip.Password = "mima";
                    zip.ZipError += new EventHandler<ZipErrorEventArgs>( delegate( object sender, ZipErrorEventArgs e )
                    {
                        Debug.LogError( zipName + " zip save err:" + e.Exception.Message );
                    } );
                    zip.SaveProgress += new EventHandler<SaveProgressEventArgs>( delegate( object sender, SaveProgressEventArgs e )
                    {
                        EditorUtility.DisplayProgressBar( "压缩文件", string.Format( "{0}({1}%)", zipName, e.EntriesSaved * 100 ), progress * i );
                        i++;
                    } );
                    zip.Save( outPath );

                    using ( StreamWriter writer = new StreamWriter( zipFolderPath, true) )
                    {
                        writer.WriteLine( targetName + Path.GetFileName( outPath ) );
                        writer.Close();
                    }

                }
                zip.Dispose();
            }
            EditorUtility.ClearProgressBar();
        }
        
        static void GetAccessory( string filePath, string folderName, string pattern, List<string> list )
        {
            if ( !File.Exists( filePath ) ) return;

            FileInfo fileInfo = new FileInfo( filePath );
            string targetPath = Path.Combine( fileInfo.Directory.Parent.FullName, folderName + "/" );
            if ( !Directory.Exists( targetPath ) ) return;

            string[] targetfiles = Directory.GetFiles( targetPath, pattern, SearchOption.AllDirectories );
            foreach ( var file in targetfiles )
            {
                if ( file.EndsWith( ".meta" ) || file.EndsWith( ".DS_Store" ) )
                    continue;
                string path = file.Replace( '\\', '/' );
                list.Add( path.Replace( Application.dataPath.Replace( "Assets", "" ), "" ) );
            }
        }

        static void GenerateResourceDetails( string path, string type, List<AssetBundleBuild> assetBundleBuildList, bool first )
        {
            string filePath = path + "/../" + bundleResourceDetails;

            FileInfo fileInfo = new FileInfo( filePath );

            if ( !fileInfo.Directory.Exists )
            {
                fileInfo.Directory.Create();
            }

            if ( !fileInfo.Exists )
            {
                using ( StreamWriter writer = new StreamWriter( File.Create( filePath ) ) )
                {
                    writer.Close();
                }
            }
            else if ( first )
            {
                File.WriteAllText( filePath, "" );
            }

            //check duplicate bundle name
            string fileStr = File.ReadAllText( filePath );
            foreach ( var build in assetBundleBuildList )
            {
                if ( fileStr.Contains( build.assetBundleName + ":" ) )
                {
                    Debug.LogError( "Error, find a duplicate bundle name, please check it! name:" + build.assetBundleName );
                }
            }

            string explain = string.Empty;
            switch ( type )
            {
                case "Buildings":
                {
                    explain = "建筑资源";
                    break;
                }
                case "Characters":
                {
                    explain = "人物资源";
                    break;
                }
                case "Particle_Character":
                {
                    explain = "人物特效资源";
                    break;
                }
                case "Particle_Map":
                {
                    explain = "地图特效资源";
                    break;
                }
                case "Particle_UI":
                {
                    explain = "UI特效资源";
                    break;
                }
                case "Sound_Battle":
                {
                    explain = "战斗里用的音效资源";
                    break;
                }
                case "Sound_BGM":
                {
                    explain = "背景音乐资源";
                    break;
                }
                case "Sound_Common":
                {
                    explain = "通用的音效资源";
                    break;
                }
                case "Sound_MainMenu":
                {
                    explain = "非战斗场景的音效资源";
                    break;
                }
                case "UI_Load":
                {
                    explain = "UI加载图的资源";
                    break;
                }
                case "Shaders":
                {
                    explain = "Shader资源";
                    break;
                }
                case "NPC":
                {
                    explain = "NPC资源";
                    break;
                }
                default:
                {
                    explain = "依赖资源";
                }
                break;
            }

            using ( StreamWriter stream = new StreamWriter( filePath, true ) )
            {
                stream.WriteLine( "      " );
                stream.WriteLine( explain );
                foreach ( var build in assetBundleBuildList )
                {
                    string bundleName = build.assetBundleName.Contains( "_depend_" ) ? build.assetBundleName + "(无需配表)：" : build.assetBundleName + ":";
                    stream.WriteLine( bundleName );
                    if ( build.assetNames.Length > 0 )
                    {
                        foreach ( var assetPath in build.assetNames )
                        {
                            stream.WriteLine( /*Path.GetFileNameWithoutExtension( assetPath ) +*/ "    " + assetPath );
                        }
                    }
                    else
                    {
                        stream.WriteLine( "无" );
                    }
                }
                stream.Flush();
                stream.Close();
            }
        }

        static void GenerateDependencyAssetList( List<string> list, string type )
        {
            if ( list.Count == 0 ) return;

            string saveAssetPath = Directory.GetCurrentDirectory () + "/" + tempAssetListTxt;
            if ( !File.Exists( saveAssetPath ) )
            {
                using ( StreamWriter writer = new StreamWriter( File.Create( saveAssetPath ) ) )
                {
                    writer.Close();
                }
            }

            string head = type + tempHeadSymbol;
            string body = string.Empty;
            foreach ( var path in list )
            {
                body += string.Concat( path, tempBodySymbol );
            }
            body = body.Substring( 0, body.Length - tempBodySymbol.Length );
            using ( StreamWriter sw = new StreamWriter( saveAssetPath, true ) )
            {
                sw.WriteLine( head + body );
                sw.Flush();
                sw.Close();
            }
        }

        static void Test ()
        {
            UObject[] UnityAssets = AssetDatabase.LoadAllAssetsAtPath ( "Resources/unity_builtin_extra" );
            string afdsafd = "Assets/zTest/";

            foreach ( var asset in UnityAssets )
            {
                if ( asset is Shader ) continue;

                //Debug.Log ( asset.GetType () );
                string suffix = "";
                var g = asset;
                if ( asset is Texture2D )
                {
                    suffix = ".png";
                    //Texture2D t = asset as Texture2D;
                    //Sprite s = Sprite.Create ( t, new Rect ( 0, 0, t.width, t.height ), Vector2.zero );
                    //Texture2D aa = new Texture2D ( t.width, t.height, TextureFormat.RGBA32, false, false );
                    //aa.SetPixels32 ( t.GetPixels32 () );
                    //g = aa;
                 
                }
                else if (asset is Material )
                {
                    suffix = ".mat";
                }
                else if ( asset is Sprite )
                {
                    suffix = ".png";
                }
                else if ( asset is LightmapParameters )
                {
                    suffix = ".giparams";
                }
               
                AssetDatabase.CreateAsset ( GameObject.Instantiate ( g ), afdsafd + asset.name + suffix );
                AssetImporter ai = AssetImporter.GetAtPath ( afdsafd + asset.name + suffix );
                if(g as Texture2D )
                {
                    TextureImporter ti = ai as TextureImporter;
                    ti.isReadable = true;
                }
            }
        }

        #region old
        static string GetPrefix()
        {
            switch ( compressionType )
            {
                case AssetBundleCompression.clzf: return "c.";
                case AssetBundleCompression.lzma: return "z.";
                default: return "";
            }
        }

        static string GetAntiPrefix()
        {
            switch ( compressionType )
            {
                case AssetBundleCompression.clzf: return "z.";
                case AssetBundleCompression.lzma: return "c.";
                default: return "";
            }
        }

        static char iosSep = '/';
        static char normalSep = '\\';

        static string PrefixAppend( string file, string insStr )
        {
#if UNITY_IOS
            char sep = iosSep;
#else
            char sep = normalSep;
#endif

            string[] splitFile = file.Split( sep );
            splitFile[splitFile.Length - 1] = insStr + splitFile[splitFile.Length - 1];
            string ret = splitFile[0];
            for ( int i = 1; i < splitFile.Length; i++ )
                ret += sep + splitFile[i];
            return ret;
        }

        static void CompressFilesCLZF( string platform, bool saveLocal )
        {
            string[] files = Directory.GetFiles( savePath( saveLocal ) + "/" + platform );

            foreach ( string file in files )
            {
                if ( Path.GetFileName( file ).StartsWith( GetAntiPrefix() ) )
                    continue;

                if ( file.EndsWith( ".manifest" ) )
                {
                    Directory.CreateDirectory( savePath( saveLocal ) + "/" + platform + "/manifests" );
                    string filePath = savePath( saveLocal ) + "/" + platform + "/manifests/" + Path.GetFileName( file );
                    if ( File.Exists( filePath ) )
                        File.Delete( filePath );
                    File.Move( file, filePath );
                    continue;
                }

                string output = PrefixAppend( file, GetPrefix() );
                if ( File.Exists( output ) )
                    File.Delete( output );

                byte[] input = File.ReadAllBytes( file );
                FileStream outStream = new FileStream( output, FileMode.Create );

                byte[] outComp = CLZF.Compress( input );
                outStream.Write( outComp, 0, outComp.Length );

                outStream.Flush();
                outStream.Close();

                File.Delete( file );
            }

            UnityEngine.Debug.Log( "Done creating AssetBundles" );
        }

        static void zCompressFiles( string platform, bool saveLacal )
        {
            Encoder coder = new Encoder();
            string[] files = Directory.GetFiles( savePath( saveLacal ) + "/" + platform );
            string outPath = savePath( saveLacal ) + "/" + platform + "/../a/";
            foreach ( string file in files )
            {
                if ( Path.GetFileName( file ).StartsWith( GetAntiPrefix() ) )
                    continue;

                if ( file.EndsWith( ".manifest" ) )
                {
                    Directory.CreateDirectory( savePath( saveLacal ) + "/" + platform + "/manifests" );
                    string filePath = savePath( saveLacal ) + "/" + platform + "/manifests/" + Path.GetFileName( file );
                    if ( File.Exists( filePath ) )
                        File.Delete( filePath );
                    File.Move( file, filePath );
                    continue;
                }

                string output = PrefixAppend( file, GetPrefix() );
                if ( File.Exists( output ) )
                    File.Delete( output );
                outPath += System.IO.Path.GetFileNameWithoutExtension( file );
                outPath += ".zip";
                FileStream inStream = new FileStream( file, FileMode.Open );
                FileStream outStream = new FileStream( outPath, FileMode.Create );

                coder.WriteCoderProperties( outStream );

                outStream.Write( System.BitConverter.GetBytes( inStream.Length ), 0, 8 );

                coder.Code( inStream, outStream, inStream.Length, -1, null );
                outStream.Flush();
                outStream.Close();
                inStream.Close();

                File.Delete( file );
                //System.IO.File.Move( output, file );
            }
            UnityEngine.Debug.Log( "Done creating AssetBundles" );
        }
    }
    #endregion

}
