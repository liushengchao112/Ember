using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using System;
using Resource;
using System.IO;

namespace Resource
{
    public class CreateAssetBundlesWindow : EditorWindow
    {
        private static string loadAssetWayPath { get { return Application.dataPath + "/../AssetBundles/editorLoadAsset.txt"; } }
        private string[] toolbarNames = { "Windows", "Android", "iOS" };
        private string[] bundleType = { "人物、建筑、NPC、人物特效、地图特效、建筑通用特效、UI特效、Shader", "UI加载图", "背景音乐", "战斗场景音效", "非战斗场景音效", "通用的音效" };
        private bool[] selectBundle;
        private bool selectBundles;
        private int selectBar = 0;
        private bool loadAssetWay;

        [MenuItem( "Tools/Create Assetbundle" )]
        static void Init()
        {
            CreateAssetBundlesWindow window = ( CreateAssetBundlesWindow )EditorWindow.GetWindow( typeof( CreateAssetBundlesWindow ), true, "打AB工具", true );
            window.Show();
        }

        void OnEnable()
        {
            selectBundles = false;
            selectBundle = new bool[bundleType.Length];
#if UNITY_ANDROID
            selectBar = 1;
#elif UNITY_IOS
            selectBar = 2;
#else
            selectBar = 0;
#endif
            loadAssetWay = LoadAssetWay;
        }

        void OnGUI()
        {
            GUILayout.Label( "注意：打包平台设置", EditorStyles.boldLabel );

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.Label( "    工具1：", EditorStyles.boldLabel );
            GUILayout.Label( "    根据平台打包全部AB", EditorStyles.boldLabel );
            GUILayout.Label( "    AB的输出路径为：" + CreateAssetBundles.savePath( false ), EditorStyles.boldLabel );
            EditorGUILayout.BeginHorizontal();
            if ( GUILayout.Button( "build windows assetbundles", GUILayout.Width( 200 ), GUILayout.Height( 40 ) ) && CanRun )
            {
                CloseThis ();
                BuildHandler ( BuildTarget.StandaloneWindows );
            }
            if ( GUILayout.Button( "build android assetbundles", GUILayout.Width( 200 ), GUILayout.Height( 40 ) ) && CanRun )
            {
                CloseThis ();
                BuildHandler ( BuildTarget.Android );
            }
            if ( GUILayout.Button( "build ios assetbundles", GUILayout.Width( 200 ), GUILayout.Height( 40 ) ) && CanRun )
            {
                CloseThis ();
                BuildHandler ( BuildTarget.iOS );
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            loadAssetWay = GUILayout.Toggle( !loadAssetWay, "勾选为编辑器加载，不勾选为AB加载，请选择！(关闭界面保存)", GUILayout.Height( 20 ) );
        
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.Label( "    工具2：", EditorStyles.boldLabel );
            GUILayout.Label( "    打AssetBundle并存入StreamingAssets文件夹", EditorStyles.boldLabel );
            GUILayout.Label( "    AB的输出路径为：" + CreateAssetBundles.savePath( true ), EditorStyles.boldLabel );
            EditorGUILayout.BeginVertical();
            selectBar = GUILayout.Toolbar( selectBar, toolbarNames, GUILayout.Width( 500 ), GUILayout.Height( 40 ) );
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            for ( int i = 0; i < bundleType.Length; i++ )
            {
                selectBundles = GUILayout.Toggle( selectBundles, bundleType[i], GUILayout.Height( 20 ) );
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if ( GUILayout.Button( "打包所选择的AB", GUILayout.Width( 200 ), GUILayout.Height( 40 ) ) && CanRun )
            {
                CloseThis ();
                OnSelectPlatform ();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.Label( "    工具3：", EditorStyles.boldLabel );
            GUILayout.Label( "    生成Resources内的资源详情信息", EditorStyles.boldLabel );
            GUILayout.Label( "    输出路径为：Assets/StreamingAssets/ResourcesDetails.txt" );
            if ( GUILayout.Button( "开始生成", GUILayout.Width( 200 ), GUILayout.Height( 40 ) ) )
            {
                GenerateResourceDetails();
            }
        }

        static void GenerateResourceDetails()
        {
            string filePath = Application.dataPath + "/Resources/";
            string writePath = Application.dataPath + "/StreamingAssets/ResourcesDetails.txt";

            if ( !File.Exists( writePath ) )
            {
                using ( StreamWriter writer = new StreamWriter( File.Create( writePath ) ) )
                {
                    writer.Close();
                }
            }

            File.WriteAllText( writePath, "" );

            string[] files = Directory.GetFiles( filePath, "*.*", SearchOption.AllDirectories );
            using ( StreamWriter stream = new StreamWriter( writePath, true ) )
            {
                foreach ( var file in files )
                {
                    string path = file.Replace( "\\", "/" ); 
                    if ( !path.EndsWith( ".DS_Store" ) 
                        && !path.EndsWith( ".meta" ) 
                        && !path.EndsWith(".renderTexture") 
                        && !path.Contains( "/Materials/" ) )
                    {
                        path = Path.ChangeExtension( path, "" );
                        path = path.Remove( path.Length - 1 );
                        path = path.Replace( Application.dataPath + "/Resources/", "" );
                        stream.WriteLine( path );
                    }
                }
                stream.Flush();
                stream.Close();
            }

            AssetDatabase.Refresh ();
            Debug.Log( "Generate ResourcesDetails.txt done" );
        }

        private void BuildHandler( BuildTarget target )
        {
            CreateAssetBundles.BuildAllAssetBundle( target );
        }

        private void OnSelectPlatform()
        {

            BuildTarget target = BuildTarget.StandaloneWindows;
            string pingtai = toolbarNames[selectBar];
            switch ( pingtai )
            {
                case "Windows":
                {
                    target = BuildTarget.StandaloneWindows;
                    break;
                }
                case "Android":
                {
                    target = BuildTarget.Android;
                    break;
                }
                case "iOS":
                {
                    target = BuildTarget.iOS;
                    break;
                }
            }

            for ( int i = 0; i < selectBundle.Length; i++ )
            {
                selectBundle[i] = selectBundles;
            }

            bool select = false;
            foreach ( var s in selectBundle )
                select = select || s;
            if ( !select )
            {
                Debug.Log( "请勾选任何AB类型!" );
                return;
            }

            List<BundleResourceType> typeList = new List<BundleResourceType> ();
   
            //0 "人物、建筑、人物特效、地图特效、建筑通用特效、UI特效、Shader"
            if ( selectBundle[ 0 ] )
            {
                typeList.Add ( BundleResourceType.Characters );
                typeList.Add ( BundleResourceType.Buildings );
                typeList.Add ( BundleResourceType.NPC );
                typeList.Add ( BundleResourceType.Particle_Character );
                typeList.Add ( BundleResourceType.Particle_Map );
                typeList.Add ( BundleResourceType.Particle_Building );
                typeList.Add ( BundleResourceType.Particle_UI );
                typeList.Add ( BundleResourceType.Shaders );
            }
            //1 "UI加载图"
            if ( selectBundle[ 1 ] )
            {
                typeList.Add ( BundleResourceType.UI_Load );
            }
            //2 "背景音乐", 
            if ( selectBundle[ 2 ] )
            {
                typeList.Add ( BundleResourceType.Sound_BGM );
            }
            //3 "战斗场景音效", 
            if ( selectBundle[ 3 ] )
            {
                typeList.Add ( BundleResourceType.Sound_Battle );
            }
            //4 "非战斗场景音效",
            if ( selectBundle[ 4 ] )
            {
                typeList.Add ( BundleResourceType.Sound_MainMenu );
            }
            //5 "通用的音效",
            if ( selectBundle[ 5 ] )
            {
                typeList.Add ( BundleResourceType.Sound_Common );
            }
            CreateAssetBundles.BuildAssetBundles( target, typeList.ToArray(), true );
        }

        static bool CanRun
        {
            get
            {
                if( EditorApplication.isCompiling )
                {
                    Debug.Log ( "Compiling! Wait for a moment please" );
                    return false;
                }
                return true;
            }
        }
        
        static bool LoadAssetWay
        {
            set
            {
                if ( !File.Exists( loadAssetWayPath ) )
                {
                    using ( StreamWriter writer = new StreamWriter( File.Create( loadAssetWayPath ) ) )
                    {
                        writer.Write( value ? "1" : "0" );
                        writer.Close();
                        writer.Dispose();
                    }
                }
                else
                {
                    File.WriteAllText( loadAssetWayPath, value ? "1" : "0" );
                }
            }
            get
            {
                bool isEditor = true;
                FileInfo fileInfo = new FileInfo( loadAssetWayPath );
                if ( !fileInfo.Directory.Exists )
                {
                    fileInfo.Directory.Create();
                }
                if ( !fileInfo.Exists )
                {
                    using ( StreamWriter writer = fileInfo.CreateText() )
                    {
                        writer.Write( isEditor ? "1" : "0" );
                        writer.Close();
                        writer.Dispose();
                    }
                }
                else
                {
                    isEditor = File.ReadAllText( loadAssetWayPath ) == "1" ? true : false;
                }
                return isEditor;
            }
        }

        private void OnDestroy()
        {
            LoadAssetWay = loadAssetWay;
        }

        static void CloseThis ()
        {
            CreateAssetBundlesWindow window = (CreateAssetBundlesWindow)EditorWindow.GetWindow ( typeof ( CreateAssetBundlesWindow ), true, "打AB工具", true );
            window.Close ();
        }

    }
}