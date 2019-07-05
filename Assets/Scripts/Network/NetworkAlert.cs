using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Data;
using Utils;

namespace Network
{
    public class NetworkAlert
    {
        private const float MESSAGE_WAIT_TIME = 0.5f;
        private const float MESSAGE_ERROR_TIME = 30f;

        private const string NET_WAIT_TIP = "Waiting...";
        private const string NET_ERROR_TIP = "Network problem, please check your Network...";

        public enum NetworkItemState
        {
            None,
            BeginToWait,
            Waiting,
            OOT,
        }

        public class NetworkWaitingItem
        {
            public ClientTcpMessage message;
            public float timer;
            public int resendTimes = 1;
            public NetworkItemState state = NetworkItemState.None;
        }

        private static NetworkAlert instance;

        public static NetworkAlert Instance
        {
            get
            {
                if ( instance == null )
                {
                    instance = new NetworkAlert();
                }

                return instance;
            }
        }

        public static Action TimeoutCallback;

        private static Dictionary<ServerType, int> maskCounters = new Dictionary<ServerType, int>();
        private static Dictionary<ServerType, bool> displayWaitingMasks = new Dictionary<ServerType, bool>();
        private static Dictionary<ServerType, bool> displayErrorMasks = new Dictionary<ServerType, bool>();

        private static Dictionary<ServerType, Dictionary<long, NetworkWaitingItem>> waitingMessages = new Dictionary<ServerType, Dictionary<long, NetworkWaitingItem>>();

        //display
        private static GameObject root;
        private static Transform waitResponseMaskTrans;
        private static UILabel maskTips;
        private static UIButton btn_OK;
        private static UIButton btn_Continue;

        private NetworkAlert() { }

        public static void Init()
        {
            for( int j = 1; j < (int)ServerType.ServerTypeCount; j++ )
            {
                ServerType type = (ServerType)j;
                waitingMessages[type] = new Dictionary<long, NetworkWaitingItem>();
                maskCounters[type] = 0;
                displayWaitingMasks[type] = false;
                displayErrorMasks[type] = false;
            }

            root = GameObject.Find( "Boot/GlobalUI/NetworkWarning" );
            waitResponseMaskTrans = root.transform.Find( "WarningMask" );
            waitResponseMaskTrans.gameObject.SetActive( false );
            maskTips = waitResponseMaskTrans.Find( "Label" ).GetComponent<UILabel>();

            btn_OK = waitResponseMaskTrans.Find( "btn_OK" ).GetComponent<UIButton>();
            btn_OK.gameObject.SetActive( false );

            btn_Continue = waitResponseMaskTrans.Find( "btn_Continue" ).GetComponent<UIButton>();
            btn_Continue.gameObject.SetActive( false );

            EventDelegate.Set( btn_OK.onClick, OnClickOK );
            EventDelegate.Set( btn_Continue.onClick, OnClickContinue );
        }

        public static void Update()
        {
            Dictionary<ServerType, Dictionary<long, NetworkWaitingItem>>.Enumerator enumerator = waitingMessages.GetEnumerator();

            float time = Time.deltaTime;

            while( enumerator.MoveNext() )
            {
                if ( enumerator.Current.Value.Count == 0 )
                {
                    continue;
                }

                ServerType type = enumerator.Current.Key;

                Dictionary<long, NetworkWaitingItem> messages = enumerator.Current.Value;

                bool displayErrorMask = false;

                foreach( KeyValuePair<long, NetworkWaitingItem> it in messages )
                {
                    NetworkWaitingItem item = it.Value;
                    item.timer += time;

                    if ( item.state == NetworkItemState.BeginToWait && item.timer >= MESSAGE_WAIT_TIME )
                    {
                        maskCounters[type]++;
                        item.state = NetworkItemState.Waiting;
                        DebugUtils.LogWarning( DebugUtils.Type.NetAlert, String.Format( "wait for the protocol {0} with seq {1}", item.message.protocalCode, item.message.sequence ) );
                    }
                    else if( item.state == NetworkItemState.Waiting && item.timer >= MESSAGE_ERROR_TIME )
                    {
                        displayErrorMask = true;
                        item.state = NetworkItemState.OOT;
                    }
                }

                if ( displayErrorMask && !displayErrorMasks[type] /*|| !DeviceUtil.CheckNetworkState()*/ )
                {
                    DisplayErrorMask( type, true );
                    displayErrorMasks[type] = true;

                    //don't just resend the message, it needs the sequence number to avoid the same op on server again. if reconnect to the server, need to sync the data.
                    /*
                    if ( waitingMessages[i].resendTimes >= 1 || waitingMessages[i].timer >= showNetErrorMaskTime )
                    {
                        ShowNetErrorMask( string.Format( "resendOverTime: {0}", waitingMessages[i].message.protocalCode ) );
                    }
                    else
                    {
                        ResendMessage( waitingMessages[i] );
                    }
                    */


                }
                else if( maskCounters[type] > 0 && !displayWaitingMasks[type] )
                {
                    DisplayWaitingMask( type, true );
                }
            }
        }

        public static void StartWaiting( ServerType type, ClientTcpMessage msg, int resendTimes )
        {
            NetworkWaitingItem item = new NetworkWaitingItem();
            item.message = msg;
            item.timer = 0;
            item.resendTimes = resendTimes;
            item.state = NetworkItemState.BeginToWait;
            waitingMessages[type].Add( msg.sequence, item );
            DebugUtils.Log( DebugUtils.Type.NetAlert, String.Format( "sending protocol {0} with seq {1}", msg.protocalCode, msg.sequence ) );
            //Debug.Log( string.Format( "StartWaiting: {0} resendTimes:{1} waitingBackMsgQueue.Count:{2}", msg.protocalCode, resendTimes, waitingBackMsgQueue.Count ) );
        }
 
        public static void ResetWaitingMessages( ServerType type )
        {
            // Due to the waitingMessages use msg.seq to as main key
            // So need to clear the waitingMessages when reset sequence ( change server ).
            waitingMessages[type].Clear();

            displayErrorMasks[type] = false;
            displayWaitingMasks[type] = false;
            maskCounters[type] = 0;
        }

        public static void OnReceiving( ServerType type, ServerTcpMessage serverMessage )
        {
			DebugUtils.Log( DebugUtils.Type.NetAlert, "server message " + serverMessage.protocalCode + " with seq " + serverMessage.sequence + " has arrived!" );

            Dictionary<long, NetworkWaitingItem> messages = waitingMessages[type];
            if( serverMessage.sequence != -1 && messages.Count > 0 )
            {
                NetworkWaitingItem item = null;

                if( messages.TryGetValue( serverMessage.sequence, out item ) )
                {
					DebugUtils.Log( DebugUtils.Type.NetAlert, string.Format( "the client message {0} with seq {1} has received its response {2}!", item.message.protocalCode, serverMessage.sequence, serverMessage.protocalCode ) );
                    messages.Remove( item.message.sequence );
                    maskCounters[type]--;
                }

                if( maskCounters[type] == 0 && displayWaitingMasks[type] )
                {
                    DisplayWaitingMask( type, false );
                }
            }
        }

        public static void DisplayWaitingMask( ServerType type, bool display, bool isInMainThread = true, string msg = null )
        {
            Action m = () => {
				if( !displayErrorMasks[type] )
				{
	                if( display )
	                {
	                    if( !displayWaitingMasks[type] )
	                    {
                            displayWaitingMasks[type] = true;
	                        maskTips.text = string.IsNullOrEmpty( msg ) ? NET_WAIT_TIP : msg;
	                        waitResponseMaskTrans.gameObject.SetActive( true );
	                    }
	                    else if( !string.IsNullOrEmpty( msg ) )
	                    {
	                        maskTips.text = msg;
	                    }
	                }
	                else
	                {
                        if( displayWaitingMasks[type] )
	                    {
                            displayWaitingMasks[type] = false;
	                        waitResponseMaskTrans.gameObject.SetActive( false );
	                    }
	                    else
	                    {
	                        DebugUtils.LogError( DebugUtils.Type.NetAlert, "hide the waiting mask when it's already hidden!" );
	                    }
	                } 
				}};

            if( isInMainThread )
            {
                m();
            }
            else
            {
                DebugUtils.Log(DebugUtils.Type.NetAlert, "Display the network waiting message from a sub-thread!" );
                Loom.QueueOnMainThread( m );
            }
        }

        public static void DisplayErrorMask( ServerType type, bool display, bool isInMainThread = true, string msg = null )
        {
            Action m = () => {
                if( display )
                {
                    if( !displayErrorMasks[type] )
                    {
                        displayErrorMasks[type] = true;
                        displayWaitingMasks[type] = false;
                        NetworkManager.ClearClientMessage( type );
                        ResetWaitingMessages( type );

                        if( TimeoutCallback != null )
                        {
                            TimeoutCallback();
                        }
                        /*
                        maskTips.text = string.IsNullOrEmpty( msg ) ? NET_ERROR_TIP : msg;
                        waitResponseMaskTrans.gameObject.SetActive( true );
                        btn_OK.gameObject.SetActive( true );
                        //btn_Continue.gameObject.SetActive( true );
                        */
                    }
                    else if( !string.IsNullOrEmpty( msg ) )
                    {
                        maskTips.text = msg;
                    }
                }
                else
                {
                    if( displayErrorMasks[type] )
                    {
                        displayErrorMasks[type] = false;
                        waitResponseMaskTrans.gameObject.SetActive( false );
                        btn_OK.gameObject.SetActive( false );
                        //btn_Continue.gameObject.SetActive( false );
                    }
                    else
                    {
                        DebugUtils.LogError( DebugUtils.Type.NetAlert, "hide the error mask when it's already hidden!" );
                    }
                }
            };

            if( isInMainThread )
            {
                m();
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.NetAlert, "Display the network error message from a sub-thread!" );
                Loom.QueueOnMainThread( m );
            }
        }

        private static void OnClickOK()
        {
            //TODO: try to reconnect, if succeeds, sync the data.
#if !UNITY_EDITOR
            //Application.Quit();
#endif
        }

        private static void OnClickContinue()
        {
            /*
            waitingMessages.Clear();
            waitResponseMaskTrans.gameObject.SetActive( false );
            isDisplayingMask = false;
            */
        }

        /*
        private void ResendMessage( NetworkWaitingItem item )
        {
            if ( waitingMessages.Exists( p => p.message.protocalCode == item.message.protocalCode ) )
            {
                waitingMessages.Remove( item );
            }

            if ( !curWaitingMsgQueue.Exists( p => p.message.protocalCode == item.message.protocalCode ) )
            {
                curWaitingMsgQueue.Add( item );
            }

            item.resendTimes += 1;

            NetworkManager.SendRequest( item.message.protocalCode, item.message.data, item.message.OnRequestSuccess, item.message.OnRequestFailed, item.resendTimes );
        }
        */
    }
   
}
