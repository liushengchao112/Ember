#if UNITY_IOS
using UnityEngine;
using System.IO;
using UnityEditor.Callbacks;
using UnityEditor;
using System.Text;
using System.Collections.Generic;
using Ember.UnityEditor.iOS.Xcode;
using Utils;

public class XCodeProjectMod
{
    static PBXProject pbxProject;
    static string channelName;

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

    [PostProcessBuild( 999999 )]
    public static void OnPostprocessBuild( BuildTarget BuildTarget, string path )
    {
        if ( BuildTarget != BuildTarget.iOS )
        {
            return;
        }

        string[] arr = CommandLineArgs;
        if ( arr != null )
        {
            Debug.Log( "Command line args:" + string.Join( " ", arr ) );

            channelName = arr[1];
        }
        else
        {
			Debug.Log( "Command line args is null" );
            return;
        }

        Debug.Log( "start build xcode : " + channelName );

        string projPath = PBXProject.GetPBXProjectPath( path );
        pbxProject = new PBXProject();
        pbxProject.ReadFromString( File.ReadAllText( projPath ) );
        string targetName = pbxProject.TargetGuidByName( PBXProject.GetUnityTargetName() );

        //set channel info
        switch ( channelName )
        {
            case "Ember":
            {
                pbxProject.SetTargetAttributes( "ProvisioningStyle", "Manual" );

                pbxProject.SetTeamId( targetName, "3MS5344X45" );

                pbxProject.SetBuildProperty( targetName, "CODE_SIGN_IDENTITY", "iPhone Developer: xu zhao (8CBK666FTK)" );

                pbxProject.SetBuildProperty( targetName, "PROVISIONING_PROFILE", "0ac332d2-0214-4d79-8716-5e1b02af7331" );
                pbxProject.SetBuildProperty( targetName, "PROVISIONING_PROFILE_SPECIFIER", "0ac332d2-0214-4d79-8716-5e1b02af7331" );

                pbxProject.SetBuildProperty( targetName, "ENABLE_BITCODE", "NO" );

            }
            break;
        }

        pbxProject.SetBuildProperty( targetName, "OTHER_LDFLAGS", "-ObjC" );
        File.WriteAllText( projPath, pbxProject.WriteToString() );

        EditorCode( path );
        EditorPlist( path );
		EditorManifestPlist ( arr[5] );
        Debug.Log( "~~~~~~~~~~~~ build xcode complete! ~~~~~~~~~~" );
    }

    static void EditorPlist( string path )
    {
        string plistPath = path + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString( File.ReadAllText( plistPath ) );
        PlistElementDict rootDict = plist.root;

        rootDict.SetString( "NSPhotoLibraryUsageDescription", "保存截图到相册，需要访问你的相册" );
        rootDict.SetString( "NSContactsUsageDescription", "使用通讯录" );
        rootDict.SetString( "NSMicrophoneUsageDescription", "使用麦克风" );
        rootDict.SetString( "NSCameraUsageDescription", "使用相机" );
        rootDict.SetString( "NSLocationWhenInUseUsageDescription", "地理位置" );
		rootDict.SetBoolean( "UIFileSharingEnabled", DebugUtils.DebugMode );

        switch ( channelName )
        {
            case "":
            {
                break;
            }
        }

        File.WriteAllText( plistPath, plist.WriteToString() );
    }
		
	static void EditorManifestPlist( string sym )
	{
		string symStr = string.Empty;
		if ( sym.Contains("DAILY_BUILD") )
		{
			symStr = "dailybuild/";
		}
		else if ( sym.Contains("TEST_BUILD") )
		{
			symStr = "testbuild/";
		}
		string plistPath = "/Users/wangamedia/Sites/" + symStr + "manifest.plist";
		PlistDocument plist = new PlistDocument();
		plist.ReadFromString( File.ReadAllText( plistPath ) );
		PlistElementDict rootDict = plist.root;

		PlistElementArray items = rootDict.values ["items"].AsArray();
		PlistElementDict item0 = items.values [0].AsDict();
		PlistElementArray assets = item0.values ["assets"].AsArray();
		PlistElementDict metadata = item0.values ["metadata"].AsDict();

		PlistElementDict assetsItem0 = assets.values [0].AsDict();

		string versionNumber = File.ReadAllText (Application.dataPath + "/../../../SDKConfig/iOS/" + channelName + "/temp.txt");
		metadata.SetString ("bundle-version", versionNumber);

		File.WriteAllText( plistPath, plist.WriteToString() );
	}

    static void EditorCode( string filePath )
    {
        switch ( channelName )
        {
            case "":
            {
                break;
            }
        }
    }

}
#endif