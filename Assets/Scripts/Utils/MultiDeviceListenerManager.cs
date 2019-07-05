using UnityEngine;
using System.Collections;

using Network;
using Data;
using Utils;
using UI;
using Constants;

public class MultiDeviceListenerManager : MonoBehaviour
{
    private static MultiDeviceListenerManager instance;

    public static void Init( GameObject go )
    {
        if( instance == null )
        {
            instance = go.AddComponent<MultiDeviceListenerManager>();
        }
    }

    void Destroy()
    {
        if( NetworkManager.IsCurrentClientConnected() )
        {
            NetworkManager.RemoveServerMessageHandler( MsgCode.NotifyClientMessage, instance.HandleNotifyClientFeedback );
        }
    }

    public static void RegisterHandler()
    {
        if( NetworkManager.IsCurrentClientConnected() )
        {
            NetworkManager.RegisterServerMessageHandler( MsgCode.NotifyClientMessage, instance.HandleNotifyClientFeedback );
        }
    }

    void HandleNotifyClientFeedback( byte[] data )
    {
        SNotifyClient feedback = ProtobufUtils.Deserialize<SNotifyClient>( data );

        if ( feedback.notifyType == 1 )
        {
            System.Action clickLogOut = LogOut;
            string logOutText = "您的账号已在其它地方登陆!";
            string titleText = "提示";
            MessageDispatcher.PostMessage( MessageType.OpenAlertWindow, clickLogOut, AlertType.ConfirmAlone, logOutText, titleText );

            NetworkManager.Shutdown();
            NetworkManager.Shutdown( ServerType.SocialServer );
        }
        else
        {
            
        }
    }

    void LogOut()
    {
        StartCoroutine( ILogOut() );
    }

    IEnumerator ILogOut()
    {
        bool isDone = false;
        ViewBase viewBase = null;

        UIManager.Instance.GetUIByType( UIType.LoginScreen, ( ViewBase ui, System.Object param ) =>
        {
            viewBase = ui;
            isDone = true;
        } );

        yield return new WaitUntil( () =>
        {
            return isDone;
        } );

        if ( viewBase != null )
        {
            LoginView view = viewBase as LoginView;
            view.OnEnter();
            view.SetPlayerChoiceWindow( false );
            view.SetTutorialWindow( false );
            view.SetLoginWindow( true );
        }

        NetworkManager.Shutdown();

        NetworkManager.Connect( ServerType.LoginServer, DataManager.GetInstance().GetLoginServerIp(), DataManager.GetInstance().GetLoginServerPort(), OnConnectLoginServer );
    }

    private void OnConnectLoginServer( ClientType clientType )
    {
        ClientTcpMessage.Reset();
        UIManager.locateState = UIManagerLocateState.Login;

        HeartBeat.RegisterLoginHeartMessageHandler();
        MultiDeviceListenerManager.RegisterHandler();
        DebugToScreen.RegisterHandler();
    }
}
