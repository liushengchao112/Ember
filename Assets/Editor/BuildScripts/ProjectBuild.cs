using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;
using System.Text;

using Constants;
using Utils;

public class ProjectBuild : Editor
{
    static BuildTarget buildTarget;
    static string packetPath;
    static string packetName;
    
    static string version;
    static string channel;
    static bool isDebug = true;
    static string defineSymbols;

    static string[] CommandLineArgs
    {
        get
        {
            string[] arr = null;
            foreach ( string arg in System.Environment.GetCommandLineArgs() )
            {
                if ( arg.StartsWith( "parameter-" ) )
                {
                    arr = arg.Replace( "parameter-", "" ).Split( ","[0] );
                    break;
                }
            }
            return arr;
        }
    }

    [MenuItem( "Tools/BuildProject" )]
    static void BuildProject()
    {
        if(EditorApplication.isCompiling)
        {
            return;
        }
        Debug.Log( "build Assetbundle start!" );

        string tailed = "";
#if UNITY_ANDROID
        buildTarget = BuildTarget.Android;
        tailed = ".apk";
#elif UNITY_IOS
        buildTarget = BuildTarget.iOS;
        tailed = "xcode";
#else
        buildTarget = BuildTarget.StandaloneWindows;
        tailed = ".exe";
#endif
        string dir = Environment.GetFolderPath( Environment.SpecialFolder.Desktop );
        packetPath = dir + "/emberPackage/";
        if ( Directory.Exists( packetPath ) )
        {
            Directory.Delete( packetPath, true );
        }
        Directory.CreateDirectory( packetPath );

        Resource.CreateAssetBundles.BuildAllAssetBundle( buildTarget, true );

        Debug.Log( "build project start!" );

        string sym = "TEST_BUILD";
        PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Android, sym );
        PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.iOS, sym );
        PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Standalone, sym );

        try
        {
            var scenes = EditorBuildSettings.scenes.Where( s => s.enabled ).Select( s => s.path ).ToArray();
            BuildOptions buildOptions = BuildOptions.CompressWithLz4 | BuildOptions.ConnectWithProfiler | BuildOptions.Development | BuildOptions.AllowDebugging;
            packetPath = packetPath + "ember" + tailed;
            BuildPipeline.BuildPlayer( scenes, packetPath, buildTarget, buildOptions );
            Debug.Log( "build done!" );
        }
        catch(Exception e)
        {
            Debug.LogError( "build project err : " + e.Message );
        }
        finally
        {
            sym = "";
            PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Android, sym );
            PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.iOS, sym );
            PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Standalone, sym );
        }
    }

    //build ab
    static void BuildAssetBundle()
	{
		BuildTarget target;
		#if UNITY_ANDROID
		target = BuildTarget.Android;
		#elif UNITY_IOS
		target = BuildTarget.iOS;
		#else
		target = BuildTarget.StandaloneWindows;
		#endif
		Resource.CreateAssetBundles.BuildAllAssetBundle( target );
	}

	//start build project
    static void PerformBuild()
    {
        SetBuildData();
        CopyFile();
        SetConfig();
        StartBuild();
    }

    static void SetBuildData()
    {
        string[] arr = CommandLineArgs;

        if ( arr != null )
        {
            Debug.Log( "Command line args:" + string.Join( " ", arr ) );

            version = arr[0];
            channel = arr[1];
            isDebug = arr[2] == "true" ? true : false;
            packetPath = arr[3];
            packetName = arr[4];
            defineSymbols = arr[5];
        }
        else
        {
            Debug.LogError( "Command line args is null" );
            return;
            //TODO : test data
            //version = "automatedProcessing";
            //channel = "Ember";
            //isDebug = true;
            //packetPath = Application.dataPath.Replace( "Assets", "../Build/PC/" );
            //packetName = "dfasdf.exe";
        }


#if UNITY_ANDROID
        buildTarget = BuildTarget.Android;
#elif UNITY_IOS
        buildTarget = BuildTarget.iOS;
#else
        buildTarget = BuildTarget.StandaloneWindows;
#endif
    }

    static void CopyFile()
    {

    }

    static void SetConfig()
    {
        UpdataGameConfig();

        PlayerSettings.bundleVersion = version;

#if UNITY_ANDROID
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel16;
        PlayerSettings.Android.bundleVersionCode = 10000;
#elif UNITY_IOS
        PlayerSettings.iOS.targetOSVersionString = "8.0";
#else

#endif

        PlayerSettings.strippingLevel = StrippingLevel.StripByteCode;

        PlayerSettings.SetScriptingBackend( BuildTargetGroup.Android, ScriptingImplementation.IL2CPP );
        PlayerSettings.SetScriptingBackend( BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP );
        PlayerSettings.SetScriptingBackend( BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x );

        string sym = defineSymbols;
        PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Android, sym );
        PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.iOS, sym );
        PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Standalone, sym );

        AssetDatabase.Refresh();
    }

    static void StartBuild()
    {
        Debug.Log( "start build:" + buildTarget + "  now:" + DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss,fff" ) );
        if ( !Directory.Exists( packetPath ) )
        {
            Directory.CreateDirectory( packetPath );
        }
        packetPath += packetName;
        if ( packetName.Contains( "." ) )
        {
            if ( File.Exists( packetPath ) )
            {
                File.Delete( packetPath );
            }
        }
        else
        {
            if ( Directory.Exists( packetPath + "/" ) )
            {
                Directory.Delete( packetPath + "/", true );
            }
        }

        // get all scenes
        var scenes = EditorBuildSettings.scenes.Where( s => s.enabled ).Select( s => s.path ).ToArray();

        BuildOptions buildOptions;
        if ( isDebug )
        {
            buildOptions = BuildOptions.CompressWithLz4 | BuildOptions.ConnectWithProfiler | BuildOptions.Development | BuildOptions.AllowDebugging;
        }
        else
        {
            buildOptions = BuildOptions.CompressWithLz4;
        }

        //Build proeject
        string err = BuildPipeline.BuildPlayer( scenes, packetPath, buildTarget, buildOptions );

        if ( !string.IsNullOrEmpty( err ) )
        {
            Debug.LogError( "Build project failure! error messag:" + err );
        }
        else
        {
            Debug.Log( "Build project succeed!" );
        }

        string sym = "";
        PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Android, sym );
        PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.iOS, sym );
        PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Standalone, sym );
    }

    static void UpdataGameConfig()
    {
        string sdkConfigPath = GameConfigPath();

        FileInfo fileInfo = new FileInfo( sdkConfigPath );
        if ( !fileInfo.Directory.Exists )
        {
            Directory.CreateDirectory( fileInfo.DirectoryName );
        }
        if ( !fileInfo.Exists )
        {
            using ( StreamWriter writer = new StreamWriter( File.Create( sdkConfigPath ) ) )
            {
                writer.Close();
            }
        }

        string sdkConfigStr = File.ReadAllText( sdkConfigPath, Encoding.UTF8 );

        BuildConfig gameConfig = null;
        if ( !string.IsNullOrEmpty( sdkConfigStr ) )
        {
            try
            {
                gameConfig = JsonUtility.FromJson<BuildConfig>( sdkConfigStr );
            }
            catch ( Exception e )
            {
                Debug.LogError( "Parse game config json error, msg:" + e.Message );
                gameConfig = new BuildConfig();
            }
        }
        else
        {
            gameConfig = new BuildConfig();
        }

        if ( version == "automatedProcessing" )
        {
            string[] versionNumbers = gameConfig.version.Split( '.' );
            if ( versionNumbers.Length == 4 )
            {
                versionNumbers[2] = ( int.Parse( versionNumbers[2] ) + 1 ).ToString();
                gameConfig.version = version = string.Join( ".", versionNumbers );

                Debug.Log( "updata version value : " + gameConfig.version );

                string[] names = packetName.Split( '_' );
                if ( names.Length > 2 )
                {
                    names[1] = gameConfig.version;
                    packetName = string.Join( "_", names );
                }

                Debug.Log( "updata packetName : " + packetName );
            }
            else
            {
                Debug.LogError( "version format error, value:" + gameConfig.version );
            }
        }
        else if ( version != null )
        {
            gameConfig.version = version;
        }


        string configJson = JsonUtility.ToJson( gameConfig );
        Debug.Log( configJson );

        using ( StreamWriter streamWriter = new StreamWriter( sdkConfigPath ) )
        {
            streamWriter.Write( configJson );
            streamWriter.Flush();
            streamWriter.Close();
        }

        //write version number
        string tempPath = Path.Combine( fileInfo.DirectoryName, "temp.txt" );
        FileInfo tempFileInfo = new FileInfo( tempPath );
        if ( !tempFileInfo.Directory.Exists )
        {
            Directory.CreateDirectory( tempFileInfo.DirectoryName );
        }
        if ( !tempFileInfo.Exists )
        {
            using ( StreamWriter writer = new StreamWriter( File.Create( tempPath ) ) )
            {
                writer.Close();
            }
        }
        using ( StreamWriter stream = new StreamWriter( tempPath ) )
        {
            stream.Write( gameConfig.version );
            stream.Flush();
            stream.Close();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log( "updata game config complete!" );
    }

    static string GameConfigPath()
    {
        if ( string.IsNullOrEmpty( channel ) )
        {
            channel = "Ember";
        }

        string platformFolder = "";
#if UNITY_ANDROID
        platformFolder = "Android";
#elif UNITY_IOS
        platformFolder = "iOS";
#else
        platformFolder = "PC";
#endif
        return Application.dataPath + "/../../../SDKConfig/" + platformFolder + "/" + channel + "/config.txt";
    }

}
