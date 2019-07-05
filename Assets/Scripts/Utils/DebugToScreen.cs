using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Data;
using Network;
using Utils;

/// <summary>
/// Send Exception Log to Screen and send to server
/// </summary>
public class DebugToScreen : MonoBehaviour {

    #region GUILayout Properties

    private Rect windowRect;
    private Vector2 scrollPosition;

    #endregion


    private static bool banShowOnScreen = false;

    private static bool isPause;
    private static ArrayList msgList = ArrayList.Synchronized(new ArrayList());

    private static DebugToScreen instance;

    public static void Init( GameObject go )
    {
        if( instance == null )
        {
            instance = go.AddComponent<DebugToScreen>();
        }
    }

    public static void PostException( string message )
    {
        if( isPause )
        {
            return;
        }

        if( NetworkManager.IsCurrentClientConnected() )
        {
            DebugInfoC2S debugData = new DebugInfoC2S();
            debugData.debugInfo = message;
            debugData.playerId = DataManager.GetInstance().GetAccount().accountId;

            byte[] data = ProtobufUtils.Serialize( debugData );
            NetworkManager.SendRequest( MsgCode.DebugInfoMessage, data );
        }

        msgList.Add(" i = " + msgList.Count.ToString() + " " + message);
    }

    private void Awake()
    {
        windowRect = new Rect( 10, 10, 880, 700 );
        scrollPosition = new Vector2( 850, 400 );
    }

    public static void RegisterHandler()
    {
        if( DebugUtils.DebugMode && NetworkManager.IsCurrentClientConnected() )
        {
            NetworkManager.RegisterServerMessageHandler( MsgCode.DebugInfoMessage, instance.DebugLogCallback );
        }
    }

    private void OnGUI()
    {
        if ( banShowOnScreen || msgList == null || msgList.Count <= 0 )
        {
            return;
        }

        windowRect = GUI.Window( 1, windowRect, DoWindow, "Debug" );
    }

    private void DoWindow(int windowId)
    {
        // Log View ...
        scrollPosition = GUILayout.BeginScrollView( scrollPosition );

        for ( int i = 0; i < msgList.Count; i++ )
        {
            GUILayout.Label( msgList[i].ToString() );
        }

        GUILayout.EndScrollView();

        // Buttons
        if ( GUILayout.Button( "Clear" ) )
        {
            msgList.Clear();
        }

        if ( GUILayout.Button( "Close" ) )
        {
            msgList.Clear();
            //DestroyImmediate( gameObject );
        }

        if ( GUILayout.Button( "Pause" ) )
        {
            isPause = !isPause;
        }

        GUI.DragWindow();
    }

    private void OnDestroy()
    {
        isPause = true;
        instance = null;
        msgList = null;

        if( NetworkManager.IsCurrentClientConnected() )
        {
            NetworkManager.RemoveServerMessageHandler( MsgCode.DebugInfoMessage, DebugLogCallback );
        }
    }

    public void DebugLogCallback( byte[] data )
    {
        //Debug.Log( "收到服务器回执" );
    }
}
