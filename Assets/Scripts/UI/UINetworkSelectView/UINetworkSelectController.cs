using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

using Network;
using Constants;
using Utils;
using Data;

namespace UI
{
	public class UINetworkSelectController
	{
        private GameObject boot;
		private UINetworkSelectView uiNetworkSelectView;

        public UINetworkSelectController( UINetworkSelectView view, GameObject boot )
		{
            this.boot = boot;
			uiNetworkSelectView = view;
		}

		public void GoIAliCloudNetwork( GameObject go )
		{
            NetworkManager.Connect( ServerType.LoginServer, NetworkConstants.EXTERNAL_SERVER_URL1, NetworkConstants.LOGIN_SERVER_PORT, GoRegister );

            DataManager.GetInstance().SetLoginServerIp( NetworkConstants.EXTERNAL_SERVER_URL1 );
            DataManager.GetInstance().SetLoginServerPort( NetworkConstants.LOGIN_SERVER_PORT );
        }

        public void GoSunMacServerNetwork( GameObject go )
		{
            NetworkManager.Connect( ServerType.LoginServer, NetworkConstants.EXTERNAL_SERVER_URL2, NetworkConstants.INTERNAL_SERVER_PORT, GoRegister );

            DataManager.GetInstance().SetLoginServerIp( NetworkConstants.EXTERNAL_SERVER_URL2);
            DataManager.GetInstance().SetLoginServerPort( NetworkConstants.INTERNAL_SERVER_PORT );
		}

		public void GoJiawenServerNetwork( GameObject go )
		{
            NetworkManager.Connect( ServerType.LoginServer, NetworkConstants.INTERNAL_SERVER_URL1, NetworkConstants.INTERNAL_SERVER_PORT, GoRegister );

            DataManager.GetInstance().SetLoginServerIp( NetworkConstants.INTERNAL_SERVER_URL1 );
            DataManager.GetInstance().SetLoginServerPort( NetworkConstants.INTERNAL_SERVER_PORT );
       }

        public void GoArcherServerNetwork( GameObject go )
		{

            NetworkManager.Connect( ServerType.LoginServer, NetworkConstants.INTERNAL_SERVER_URL2, NetworkConstants.INTERNAL_SERVER_PORT, GoRegister );

            DataManager.GetInstance().SetLoginServerIp( NetworkConstants.INTERNAL_SERVER_URL2 );
            DataManager.GetInstance().SetLoginServerPort( NetworkConstants.INTERNAL_SERVER_PORT );
        }

        public void GoFangJSServerNetwork( GameObject go )
        {
            NetworkManager.Connect( ServerType.LoginServer, NetworkConstants.INTERNAL_SERVER_URL3, NetworkConstants.INTERNAL_SERVER_PORT, GoRegister );

            DataManager.GetInstance().SetLoginServerIp( NetworkConstants.INTERNAL_SERVER_URL3 );
            DataManager.GetInstance().SetLoginServerPort( NetworkConstants.INTERNAL_SERVER_PORT );
        }

        void GoRegister( ClientType clientType )
		{
            //DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Begin to Connect LoginServer IP = " + curServerIp );
            SceneManager.LoadScene( "MainMenu" );
            UIManager.locateState = UIManagerLocateState.Login;

            HeartBeat.RegisterLoginHeartMessageHandler();

            MultiDeviceListenerManager.Init( boot );
            MultiDeviceListenerManager.RegisterHandler();


            NetworkAlert.Init();
        }
    }
}