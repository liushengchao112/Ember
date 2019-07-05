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
    public class NetworkManager
    {
        private static NetworkManager instance = new NetworkManager();

        private Dictionary<ServerType, NetworkClient> clients;

        //current client can only be login/game/lobby/battle server
        private NetworkClient currentClient;

        private ServerType currentServerType;

        private NetworkManager()
        {
            clients = new Dictionary<ServerType, NetworkClient>();
            currentServerType = ServerType.NoneServer;

            for( ServerType type = ServerType.LoginServer; type <= ServerType.SocialServer; type++ )
            {
                NetworkClient client = new NetworkClient();
                clients.Add( type, client );
            }

            MessageDispatcher.AddObserver( OnChangePlayerId, MessageType.ChangePlayerId );
        }

        public static void Update()
        {
            Dictionary<ServerType, NetworkClient>.Enumerator enumerator = instance.clients.GetEnumerator();
            while( enumerator.MoveNext() )
            {
                enumerator.Current.Value.Update();
            }
        }

        public static void Connect( ServerType type, string url, int port, Action<ClientType> ConnectSuccessCallback = null, Action<ClientType> ConnectFailCallback = null )
        {
            NetworkClient client = null;
            if( !instance.clients.TryGetValue( type, out client ) )
            {
                client = new NetworkClient();
                instance.clients.Add( type, client );
            }

            if( client.IsConnected() )
            {
                DebugUtils.LogError( DebugUtils.Type.Network, string.Format( "the server {0} has already been connected!", type ) );
            }
            else
            {
                client.Connect( type, url, port, ConnectSuccessCallback, ConnectFailCallback );
                if( instance.currentClient != client && ( type == ServerType.LoginServer || type == ServerType.GameServer || type == ServerType.LobbyServer || type == ServerType.BattleServer ) )
                {
                    DebugUtils.Assert( instance.currentClient == null || !instance.currentClient.IsConnected(), string.Format( "the current server {0} is stll connected when connecting to {1}!", instance.currentServerType, type ) );

                    instance.currentClient = client;
                    instance.currentServerType = type;
                }
            }
        }

        public static void Reconnect( ServerType type, ClientType clientType, Action<ClientType> ReconnectSuccessCallback = null, Action<ClientType> ReconnectFailCallback = null )
        {
            NetworkClient client = null;
            if( instance.clients.TryGetValue( type, out client ) )
            {
                if( type == ServerType.LoginServer || type == ServerType.GameServer || type == ServerType.LobbyServer || type == ServerType.BattleServer )
                {
                    if ( type != instance.currentServerType )
                    {
                        ReconnectSuccessCallback += ( ClientType ct ) => {
                            DebugUtils.Assert( instance.currentClient == null || !instance.currentClient.IsConnected(), string.Format( "the current server {0} is stll connected when reconnecting to {1}!", instance.currentServerType, type ) );

                            instance.currentClient = client;
                            instance.currentServerType = type;
                        };
                    }
                }
                client.Reconnect( clientType, ReconnectSuccessCallback, ReconnectFailCallback );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Network, string.Format( "the server {0} which is reconnected doesn't exist!", instance.currentServerType ) );
            }
        }

        public static void Shutdown( ServerType type, Action ShutdownSuccessCallback = null )
        {
            NetworkClient client = null;
            instance.clients.TryGetValue( type, out client );
            if( client != null )
            {
                client.Shutdown( ShutdownSuccessCallback );
                if( client == instance.currentClient )
                {
                    instance.currentClient = null;
                    instance.currentServerType = ServerType.NoneServer;
                }
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Network, string.Format( "the server {0} which is shutdown doesn't exist!", type ) );
            }
        }

        public static void Shutdown( Action ShutdownSuccessCallback = null )
        {
            if( instance.currentClient != null )
            {
                instance.currentClient.Shutdown( ShutdownSuccessCallback );
                instance.currentServerType = ServerType.NoneServer;
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Network, string.Format( "the current server {0} which is shutdown doesn't exist!", instance.currentServerType ) );
            }
        }

        public static void ShutdownAll( Action ShutdownSuccessCallback )
        {
            Dictionary<ServerType, NetworkClient>.Enumerator enumerator = instance.clients.GetEnumerator();
            while( enumerator.MoveNext() )
            {
                enumerator.Current.Value.Shutdown();
            }

            instance.currentClient = null;
            instance.currentServerType = ServerType.NoneServer;

            if( ShutdownSuccessCallback != null )
            {
                ShutdownSuccessCallback();
            }
        }

        public static void SendRequest( ServerType type, MsgCode protocolCode, byte[] data, ClientType clientType = ClientType.Tcp, Action onRequestSuccess = null, Action onResquestFailed = null, int resendTimes = 1 )
        {
            NetworkClient client = null;
            instance.clients.TryGetValue( type, out client );
            if( client != null )
            {
                client.SendRequest( protocolCode, data, clientType, onRequestSuccess, onResquestFailed, resendTimes );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Network, string.Format( "the server {0} sent request to doesn't exist!", type ) );
            }
        }

        public static void SendRequest( MsgCode protocolCode, byte[] data, ClientType clientType = ClientType.Tcp, Action onRequestSuccess = null, Action onResquestFailed = null, int resendTimes = 1 )
        {      
            if( instance.currentClient != null )
            {
                instance.currentClient.SendRequest( protocolCode, data, clientType, onRequestSuccess, onResquestFailed, resendTimes );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Network, string.Format( "the current server {0} sent request to doesn't exist!", instance.currentServerType ) );
            }
        }

        public static bool IsCurrentClientConnected()
        {
            return instance.currentClient != null && instance.currentClient.IsConnected();
        }

        public static bool IsClientConnected( ServerType type )
        {
            NetworkClient client = null;
            instance.clients.TryGetValue( type, out client );
            if( client != null )
            {
                return client.IsConnected();
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Network, string.Format( "the server {0} doesn't exist!", type ) );
                return false;
            }
        }

        public static ServerType GetCurrentServerType()
        {
            return instance.currentServerType;
        }

        /*
        public static void ChangeCurrentClient( ServerType type )
        {
            NetworkClient client = null;
            instance.clients.TryGetValue( type, out client );
            if( client != null )
            {
                instance.currentClient = client;
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Network, string.Format( "the server {0} you want to change to doesn't exist!", type ) );
            }
        }
        */

        public static void ClearClientMessage( ServerType type )
        {
            instance.clients[type].ClearTcpClientMessage();
        }

        //There are two ways to register/remove server message handlers. The first is to register handlers right after connecting and 
        //to remove handlers when disconnecting. The second is to initialize all the servers and to register handlers at the beginning. 
        //The first one is more accurate and the second one is more convenient.
        public static void RegisterServerMessageHandler( ServerType type, MsgCode protocolCode, Action<byte[]> handler )
        {
            NetworkClient client = null;
            instance.clients.TryGetValue( type, out client );
            if( client != null )
            {
                client.RegisterServerMessageHandler( protocolCode, handler );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Network, string.Format( "the server {0} registered {1} handler to doesn't exist!", type, protocolCode ) );
            }
        }

        public static void RegisterServerMessageHandler( MsgCode protocolCode, Action<byte[]> handler )
        {
            if( instance.currentClient != null )
            {
                instance.currentClient.RegisterServerMessageHandler( protocolCode, handler );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Network, string.Format( "the current server {0} registered {1} handler to doesn't exist!", instance.currentServerType, protocolCode ) );
            }
        }

        public static void RemoveServerMessageHandler( ServerType type, MsgCode protocolCode, Action<byte[]> handler )
        {
            NetworkClient client = null;
            instance.clients.TryGetValue( type, out client );
            if( client != null )
            {
                client.RemoveServerMessageHandler( protocolCode, handler );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Network, string.Format( "the server {0} removed {1} handler from doesn't exist!", type, protocolCode ) );
            }
        }

        public static void RemoveServerMessageHandler( MsgCode protocolCode, Action<byte[]> handler )
        {
            if( instance.currentClient != null )
            {
                instance.currentClient.RemoveServerMessageHandler( protocolCode, handler );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Network, string.Format( "the server {0} removed {1} handler from doesn't exist!", instance.currentServerType, protocolCode ) );
            }
        }

        void OnChangePlayerId( object obj )
        {
            ClientTcpMessage.playerId = (long)obj;
        }

    }
}
