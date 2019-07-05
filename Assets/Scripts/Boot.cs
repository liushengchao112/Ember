using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

using Utils;
using Data;
using Resource;
using Network;

public class Boot : MonoBehaviour
{
    void Awake()
    {
        DebugUtils.Init( this.gameObject );            

        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        DontDestroyOnLoad( this.gameObject );

        gameObject.AddComponent<Loom>();

        ClientTcpMessage.platform = DeviceUtil.Instance.GetPlatform();    
        ClientTcpMessage.gameVersion = VersionUtil.Instance.curVersion.Build;

        AssetBundleManager abManager = GameObject.Find ( "AssetBundleManager" ).GetComponent<AssetBundleManager> ();
#if UNITY_EDITOR
        string path = Application.dataPath + "/../AssetBundles/editorLoadAsset.txt";
        if( System.IO.File.Exists( path ) )
        {
            Constants.GameConstants.LoadAssetByEditor = System.IO.File.ReadAllText( path ) == "1" ? true : false;
        }
        else
        {
            Constants.GameConstants.LoadAssetByEditor = true;
        }
        Debug.Log( "The current AB loading method is : " + ( Constants.GameConstants.LoadAssetByEditor ? "editor!" : "assetbundle!" ) );
#endif
        DownloadResource.Instance.CheckResourceVersion( delegate()
        {
            ClientTcpMessage.resVersion = DownloadResource.localVersionCode;
            VersionUtil.Instance.SetResourceVersion ( DownloadResource.localVersionCode );
            DataManager.GetInstance ().LoadGameData ();


            GameResourceLoadManager.GetInstance ().Startup ( abManager );

            HeartBeat.Init ();

            // TODO: ban the UI for now! 
            // 20170302 
            // @Woody
            GameObject networkSelect = Instantiate ( Resources.Load ( "Prefabs/UI/NetworkSelectMenu" ) ) as GameObject;
            networkSelect.transform.parent = GameObject.Find ( "UI Root/Camera" ).transform;
            networkSelect.transform.localPosition = Vector3.zero;
            networkSelect.transform.localRotation = Quaternion.identity;
            networkSelect.transform.localScale = Vector3.one;
        }, abManager );
    }
	
	// Update is called once per frame
	void Update()
    {
        NetworkManager.Update();
        NetworkAlert.Update();
        HeartBeat.Update( Time.deltaTime );
    }

    void OnApplicationPause()
    {
        HeartBeat.OnApplicationPause();
    }

    private void OnApplicationQuit()
    {
        DebugUtils.Release();
    }

    void OnDestroy()
    {
        if( NetworkManager.IsCurrentClientConnected() )
        {
            NetworkManager.Shutdown();
        }
    }
}
