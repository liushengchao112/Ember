/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: NetworkManager.cs
// description: 
// 
// created time：10/12/2016
//
//----------------------------------------------------------------*/

using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

using Constants;
using Utils;
using Data;

namespace Network
{
    public enum ClientType
    {
        None = 0,
        Tcp = 1,
        Udp = 2,
        Both = 3,
    }

    public class NetworkClient
    {
        private ClientType clientType;

        private AsyncTcpClient tcpClient;
        private AsyncUdpClient udpClient;

        public ServerType serverType;

        private Thread sendThread = null;
        private volatile bool notifySendThreadClosed;

        //Tcp
        private Queue<ClientTcpMessage> tcpSendingQueue;
        private Queue<ClientTcpMessage> tcpSendingQueueInThread;

        private bool isClearTcpMessageInThread = false;
        private bool sendingThreadClosed = true;
        private bool tcpClientClosed = true;

        private long tcpSequence;
        private byte[] tcpSendBuffer;

        //Udp
        private Queue<ClientUdpMessage> udpSendingQueue;
        private Queue<ClientUdpMessage> udpSendingQueueInThread;

        private List<ClientUdpMessage> udpWaitingMessages;

        private bool isClearUdpMessageInThread = false;
        private bool udpThreadClosed = true;
        private bool udpClientClosed = true;

        private long udpSequence;
        private long udpAck;
        private byte[] udpSendBuffer;

        //server message handlers
        private Dictionary<MsgCode, Action<byte[]>> serverMessageHandlers;

        //callback
        public Action OnRequestSuccess;
        public Action OnRequestFail;
        private Action OnShutdownSuccess;

        public NetworkClient( ClientType clientType = ClientType.Tcp )
        {
            this.clientType = clientType;

            if( clientType == ClientType.Tcp || clientType == ClientType.Both )
            {
                tcpClient = new AsyncTcpClient();
                tcpSendingQueue = new Queue<ClientTcpMessage>();
                tcpSendingQueueInThread = new Queue<ClientTcpMessage>();

                tcpSendBuffer = new byte[NetworkConstants.MAX_PACKET_SIZE];
                tcpClient.RegisterResponseHandler( TcpResponseHandler );
            }

            if( clientType == ClientType.Udp || clientType == ClientType.Both )
            {
                udpClient = new AsyncUdpClient();
                udpSendingQueue = new Queue<ClientUdpMessage>();
                udpSendingQueueInThread = new Queue<ClientUdpMessage>();
                udpWaitingMessages = new List<ClientUdpMessage>();

                //udpSendBuffer = new byte[TCP_MAX_BUFFER_SIZE];
                udpClient.RegisterResponseHandler( UdpResponseHandler );
            }
                
            serverMessageHandlers = new Dictionary<MsgCode, Action<byte[]>>();
        }

        //TODO: replace IsConnected with IsTcpConnected/IsUdpConnected
        public bool IsConnected()
        {
            return ( ( tcpClient != null && tcpClient.IsConnected() ) || ( udpClient != null && udpClient.IsConnected() ) ) && !notifySendThreadClosed;
        }

        public bool IsTcpConnected()
        {
            return tcpClient != null && tcpClient.IsConnected() && !notifySendThreadClosed;
        }

        public bool IsUdpConnected()
        {
            return udpClient != null && udpClient.IsConnected() && !notifySendThreadClosed;
        }

        public void Connect( ServerType serverType, string url, int port, Action<ClientType> ConnectSuccessCallback = null, Action<ClientType> ConnectFailCallback = null )
        {
            if( tcpClient != null )
            {
                tcpClient.Connect( serverType, url, port, ( success ) => {
                    if( success )
                    {
                        DebugUtils.Log(DebugUtils.Type.AsyncSocket, string.Format( "Tcp client's connection to {0} {1}:{2} succeeds!", serverType, url, port ) );

                        this.serverType = serverType;
                        tcpSequence = 0;

                        Loom.QueueOnMainThread( () => {
                            tcpClientClosed = false;

                            if( udpClient == null || udpClientClosed == false )
                            {
                                StartSendThread();
                            }

                            if( ConnectSuccessCallback != null )
                            {
                                ConnectSuccessCallback( ClientType.Tcp );
                            }
                        } );
                    }
                    else
                    {
                        DebugUtils.LogWarning( DebugUtils.Type.AsyncSocket, string.Format( "Tcp client's connection to {0} {1}:{2} fails!" , serverType , url , port ) );

                        if( ConnectFailCallback != null )
                        {
                            ConnectFailCallback( ClientType.Tcp );
                        }
                    }
                } );
            }

            if( udpClient != null )
            {
                udpClient.Connect( serverType, url, port, ( success ) => {
                    if( success )
                    {
                        DebugUtils.Log(DebugUtils.Type.AsyncSocket, string.Format( "Udp client's connection to {0} {1}:{2} succeeds!", serverType, url, port ) );

                        this.serverType = serverType;
                        udpSequence = 0;

                        Loom.QueueOnMainThread( () => {
                            udpClientClosed = false;

                            if( tcpClient == null || tcpClientClosed == false )
                            {
                                StartSendThread();
                            }

                            if( ConnectSuccessCallback != null )
                            {
                                ConnectSuccessCallback( ClientType.Udp );
                            }
                        } );
                    }
                    else
                    {
                        DebugUtils.LogWarning( DebugUtils.Type.AsyncSocket, string.Format( "Udp client's connection to {0} {1}:{2} fails!" , serverType , url , port ) );

                        if( ConnectFailCallback != null )
                        {
                            ConnectFailCallback( ClientType.Udp );
                        }
                    }
                } );
            }


        }

        public void Reconnect( ClientType clientType, Action<ClientType> ReconnectSuccessCallback = null, Action<ClientType> ReconnectFailCallback = null )
        {
            if( ( clientType == ClientType.Tcp || clientType == ClientType.Both ) && ( tcpClient != null && !tcpClient.IsConnected() ) )
            {
                ClearTcpClientMessage();
                //TODO
                tcpSequence = 0;
                NetworkAlert.ResetWaitingMessages( serverType );
                tcpClient.Reconnect( ReconnectSuccessCallback, ReconnectFailCallback );
            }

            if( ( clientType == ClientType.Udp || clientType == ClientType.Both ) && ( udpClient != null && !udpClient.IsConnected() ) )
            {
                ClearUdpClientMessage();
                udpSequence = 0;
                udpClient.Reconnect( ReconnectSuccessCallback, ReconnectFailCallback );
            }
        }

        public void Shutdown( Action ShutdownSuccessCallback = null )
        {
            OnShutdownSuccess = ShutdownSuccessCallback;
            notifySendThreadClosed = true;

            if( tcpClient != null )
            {
                tcpClient.Disconnect( () => { 
                    tcpClientClosed = true;
                    if( udpClientClosed && sendingThreadClosed )
                    {
                        Loom.QueueOnMainThread( () => {
                            NetworkAlert.ResetWaitingMessages( serverType );
                            if( OnShutdownSuccess != null )
                            {
                                OnShutdownSuccess();
                                OnShutdownSuccess = null;
                            }
                        } );
                    }
                } );
            }

            if( udpClient != null )
            {
                udpClient.Disconnect( () => { 
                    udpClientClosed = true;
                    if( tcpClientClosed && sendingThreadClosed )
                    {
                        Loom.QueueOnMainThread( () => {
                            NetworkAlert.ResetWaitingMessages( serverType );
                            if( OnShutdownSuccess != null )
                            {
                                OnShutdownSuccess();
                                OnShutdownSuccess = null;
                            }
                        } );
                    }
                } );
            }
        }

        private void StartSendThread()
        {
            DebugUtils.Assert( sendThread == null, "sending thread has been initialized!" );

            sendThread = new Thread( new ThreadStart( HandleSend ) );
            sendThread.IsBackground = true;
            notifySendThreadClosed = false;
            sendingThreadClosed = false;
            sendThread.Start();
        }

        private void HandleSend()
        {
            while( !notifySendThreadClosed )
            {
                if( tcpClient != null )
                {
                    if( isClearTcpMessageInThread )
                    {
                        tcpSendingQueueInThread.Clear();
                        isClearTcpMessageInThread = false;
                    }
                    else if( tcpSendingQueueInThread.Count > 0 && tcpClient.IsConnected() )
                    {
                        int length = AssembleTcpMessage();
                        if( length > 0 )
                        {
                            tcpClient.SendRequest( tcpSendBuffer, length, OnRequestSuccess, OnRequestFail );
                            OnRequestSuccess = null;
                            OnRequestFail = null;
                        }
                    }

                    //sleep 5 ms between each sending
                    Thread.Sleep( 5 );
                }

                if( udpClient != null )
                {
                    if( isClearUdpMessageInThread )
                    {
                        udpSendingQueueInThread.Clear();
                        isClearUdpMessageInThread = false;
                    }
                    else if( udpSendingQueueInThread.Count > 0 && udpClient.IsConnected() )
                    {
                        udpClient.ResendRequests();
                        while( udpSendingQueueInThread.Count > 0 )
                        {
                            long seq = -1;
                            byte[] data = AssembleUdpMessage( ref seq );
                            if( data != null )
                            {
                                udpClient.SendRequest( seq, data, data.Length, OnRequestSuccess, OnRequestFail );
                                OnRequestSuccess = null;
                                OnRequestFail = null;
                            }
                        }
                    }

                    //sleep 5 ms between each sending
                    Thread.Sleep( 5 );
                }
            }

            if( tcpSendingQueueInThread != null )
                tcpSendingQueueInThread.Clear();

            if( udpSendingQueueInThread != null )
                udpSendingQueueInThread.Clear();

            sendingThreadClosed = true;
            tcpSequence = 0;
            udpSequence = 0;
            udpAck = 0;
            sendThread = null;

            Loom.QueueOnMainThread( () => {
                if( tcpClientClosed && udpClientClosed && OnShutdownSuccess != null )
                {
                    OnShutdownSuccess();
                    OnShutdownSuccess = null;
                }
            } );
        }

        private int AssembleTcpMessage()
        {
            if ( tcpSendingQueueInThread.Count <= 0 )
                return 0;

            int length = 0;

            while( length < NetworkConstants.MAX_PACKET_SIZE && tcpSendingQueueInThread.Count > 0 )
            {
                ClientTcpMessage message = tcpSendingQueueInThread.Peek();
                Byte[] data = message.Encode();
                int dataLength = data.Length;

                DebugUtils.Assert( dataLength <= NetworkConstants.MAX_PACKET_SIZE, string.Format( "The length {0} of the tcp client message {1} exceeds the limitation!", dataLength, message.protocalCode ) );

                if ( length + dataLength < NetworkConstants.MAX_PACKET_SIZE )
                {
                    //DebugUtils.Log( DebugUtils.Type.SocketNet, "Assemble protocol = " + message.protocolCol );
                    OnRequestSuccess += message.OnRequestSuccess;
                    OnRequestFail += message.OnRequestFailed;
                    Buffer.BlockCopy( data, 0, tcpSendBuffer, length, dataLength );
                    length += dataLength;
                    tcpSendingQueueInThread.Dequeue();
                }
                else
                {
                    break;
                }
            }

            return length;
        }

        private byte[] AssembleUdpMessage( ref long seq )
        {
            if ( udpSendingQueueInThread.Count <= 0 )
                return null;

            ClientUdpMessage message = udpSendingQueueInThread.Dequeue();
            seq = message.sequence;
            return message.Encode();
        }

        public void ClearTcpClientMessage()
        {
            tcpSendingQueue.Clear();
            isClearTcpMessageInThread = true;
        }

        public void ClearUdpClientMessage()
        {
            udpSendingQueue.Clear();
            udpWaitingMessages.Clear();
            isClearUdpMessageInThread = true;
        }

        public void Update()
        {
            if( tcpClient != null && tcpSendingQueue.Count > 0 )
            {
                SwitchTcpMessageQueue();
            }

            if( udpClient != null && udpSendingQueue.Count > 0 )
            {
                SwitchUdpMessageQueue();
            }
        }

        private void SwitchTcpMessageQueue()
        {
            if( tcpSendingQueueInThread.Count == 0 )
            {
                Queue<ClientTcpMessage> temp = null;

                temp = tcpSendingQueueInThread;
                tcpSendingQueueInThread = tcpSendingQueue;
                tcpSendingQueue = temp;
            }
        }

        private void SwitchUdpMessageQueue()
        {
            if( udpSendingQueueInThread.Count == 0 )
            {
                Queue<ClientUdpMessage> temp = null;

                temp = udpSendingQueueInThread;
                udpSendingQueueInThread = udpSendingQueue;
                udpSendingQueue = temp;
            }
        }

        public void SendRequest( MsgCode protocolCode, byte[] data, ClientType type = ClientType.Tcp, Action onRequestSuccess = null, Action onResquestFailed = null, int resendTimes = 1 )
        {
            DebugUtils.Log( DebugUtils.Type.Protocol, "try to send protocol " + protocolCode );

            if( notifySendThreadClosed )
            {
                DebugUtils.Log( DebugUtils.Type.Protocol, "Network message thread has been shutdown when trying to send protocol " + protocolCode );
                return;
            }

            if( type == ClientType.Tcp )
            {
                ClientTcpMessage message = new ClientTcpMessage( (int)protocolCode, data, tcpSequence++ );

                if( onRequestSuccess != null )
                {
                    message.OnRequestSuccess += onRequestSuccess;
                }

                if( onResquestFailed != null )
                {
                    message.OnRequestFailed += onResquestFailed;
                }

                tcpSendingQueue.Enqueue( message );

                NetworkAlert.StartWaiting( serverType, message, resendTimes );
            }
            else if( type == ClientType.Udp )
            {
                ClientUdpMessage[] messages = ClientUdpMessage.CreateUdpMessages( (int)protocolCode, ref udpSequence, udpAck, data );
                int j = 0;
                for( ; j < messages.Length; j++ )
                {
                    udpSendingQueue.Enqueue( messages[j] );
                }

                if( onRequestSuccess != null )
                {
                    messages[j].OnRequestSuccess += onRequestSuccess;
                }

                if( onResquestFailed != null )
                {
                    messages[j].OnRequestFailed += onResquestFailed;
                }
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, string.Format( "There is no such client type {0} to send request!", type ) );
            }
        }

        public void TcpResponseHandler( ServerTcpMessage[] messages )
        {
            //UiWaiting.InterruptWaitingTimer();

            for ( int j = 0; j < messages.Length; j++ )
            {
                ServerTcpMessage m = messages[j];

                Loom.QueueOnMainThread( () => {
                    NetworkAlert.OnReceiving( serverType, m );
                    Action<byte[]> handler;
                    if ( serverMessageHandlers.TryGetValue( m.protocalCode, out handler ) )
                    {
                        DebugUtils.Log( DebugUtils.Type.Protocol, "The client handles the received protocol " + m.protocalCode );
                        handler( m.data );
                    }
                    else
                    {
                        DebugUtils.LogError( DebugUtils.Type.Protocol, "For now, the client can't handles the received protocol " + m.protocalCode );     
                    }
                } );
            }
        }

        public void UdpResponseHandler( ServerUdpMessage[] messages )
        {
            
        }

        public void RegisterServerMessageHandler( MsgCode protocolCode, Action<byte[]> handler )
        {
            if ( !serverMessageHandlers.ContainsKey( protocolCode ) )
            {
                serverMessageHandlers.Add( protocolCode, handler );
            }
            else
            {
                if( serverMessageHandlers[protocolCode] != null )
                {
                    Delegate[] delegates = serverMessageHandlers[protocolCode].GetInvocationList();
                    foreach( Delegate del in delegates )
                    {
                        if( del.Equals( handler ) )
                            return;
                    }
                }
                serverMessageHandlers[protocolCode] += handler;
            }
        }

        public void RemoveServerMessageHandler( MsgCode protocolCode, Action<byte[]> handler )
        {
            if( serverMessageHandlers.ContainsKey( protocolCode ) )
            {
                serverMessageHandlers[protocolCode] -= handler;
                if( serverMessageHandlers[protocolCode] == null )
                    serverMessageHandlers.Remove( protocolCode );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Protocol, "Remove an unregistered handler for protocol " + (int)protocolCode );
            }
        }
    }
}
