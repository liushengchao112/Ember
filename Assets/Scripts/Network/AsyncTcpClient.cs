/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen
//
// file name: AsyncSocketClient.cs
// description:
// 
// created time： 2016.03.08
//
//----------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;

using Data;
using Utils;

namespace Network
{
    public class AsyncTcpClient
    {
        private const int BUFFERSIZE = 16 * 1024 * 10;
        private const int TIMEOUT = 5000;

        private event Action<ServerTcpMessage[]> ResponseHandler;

        private Action<bool> OnConnect;

        public static Action<ServerType> AccidentDisconnectCallback = null;

        private Socket socket;
        private ServerType serverType;
        private string serverUrl;
        private int port;

        private byte[] buffer = new byte[BUFFERSIZE];
        private byte[] unusedBuffer = null;

        private bool isConnected = false;

        public AsyncTcpClient() {}

        ~AsyncTcpClient()
        {
            Disconnect();
        }

        public bool IsConnected()
        {
            return ( isConnected && socket != null && socket.Connected );
        }

        /// <summary>
        /// connect to server
        /// </summary>
        private ManualResetEvent connectTimeoutObject = new ManualResetEvent( false );

        public void Connect( ServerType serverType, string serverUrl, int port, Action<bool> connectCallback )
        {
            if( socket != null && socket.Connected )
            {
                DebugUtils.Log( DebugUtils.Type.AsyncSocket, String.Format( "Async tcp socket has already connected {0}, reconnect to {1}.", socket.RemoteEndPoint, serverType ) );
                Disconnect();
            }

            try 
            {
                DebugUtils.Log( DebugUtils.Type.AsyncSocket, String.Format( "Async tcp socket begins to connect {0} {1}:{2}", serverType, serverUrl, port ) );

                OnConnect = connectCallback;

                connectTimeoutObject.Reset();

                this.serverType = serverType;
                this.serverUrl = serverUrl;
                this.port = port;

                socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                socket.SetSocketOption( SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true );

                socket.BeginConnect( serverUrl, port, new AsyncCallback(ConnectCallback), socket );

                bool signalled = connectTimeoutObject.WaitOne( TIMEOUT, false );
                if( !signalled)
                {
                    throw new Exception( String.Format( "Async tcp socket's connection to {0} is timeout!", serverType ) );
                } 
            }
            catch( Exception e )
            {
                Disconnect();

                if( connectCallback != null )
                {
                    connectCallback( false );
                }

                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async tcp socket meets error when connecting: " + e );
            }
        }

        private void ConnectCallback( IAsyncResult asyncConnect )
        {
            try
            {
                Socket socket = (Socket)asyncConnect.AsyncState;
                socket.EndConnect( asyncConnect );

                if( socket.Connected )
                {
                    socket.SendBufferSize = BUFFERSIZE;
                    socket.ReceiveBufferSize = BUFFERSIZE;
                    socket.BeginReceive( buffer, 0, BUFFERSIZE, SocketFlags.None, new AsyncCallback( ReadCallback ), socket );

                    isConnected = true;

                    DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Async tcp socket has connected to " + serverType );

                    if( this.OnConnect != null )
                    {
                        OnConnect( true );
                    }
                }
                else
                {
                    DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Async tcp socket can't connect to " + serverType );
                    if( this.OnConnect != null )
                    {
                        OnConnect( false );
                    }
                }
            } 
            catch( Exception e )
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, string.Format( "Async tcp socket has exception when connecting to {0} {1}, the exception: {2}", serverType, serverUrl, e.ToString() ) );
                Disconnect();
                if( this.OnConnect != null )
                {
                    OnConnect( false );
                }
            } 
            finally
            {
                OnConnect = null;
                connectTimeoutObject.Set();
            }
        }

        public void Disconnect( Action callback = null )
        {               
            try
            {
                if( socket != null && socket.Connected )
                {
                    isConnected = false;
                    socket.Shutdown( SocketShutdown.Both );
                    socket.Close();
                    DebugUtils.Log( DebugUtils.Type.AsyncSocket, string.Format("Async tcp socket to {0} is closing!", serverType ) );
                } 
                else
                {
                    DebugUtils.LogWarning( DebugUtils.Type.AsyncSocket, string.Format( "Disconnect a closed async tcp socket to {0}!", serverType ) );
                }
            } 
            catch( Exception e )
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, string.Format( "Close async tcp socket to {0} has error: {1}", serverType, e ) );
            }
            finally
            {
                isConnected = false;
                socket = null;

                Loom.QueueOnMainThread( callback );
            }
        }

        private ManualResetEvent sendTimeoutObject = new ManualResetEvent( false );

        /// <summary>
        /// the functions sending the request
        /// </summary>
        public void SendRequest( byte[] data, int dataLength, Action sendRequestSuccessCallback, Action sendRequestFailedCallback )
        {
            if( isConnected && socket != null && socket.Connected )
            {
                //DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Send Start " + data.Length + " : " + DebugBuffer( data ) + ", " + DateTime.Now );

                try
                {
                    WriteSocketObject obj = new WriteSocketObject();
                    obj.socket = socket;
                    obj.WriteSuccessCallback = sendRequestSuccessCallback;
                    obj.WriteFailedCallback = sendRequestFailedCallback;

                    sendTimeoutObject.Reset();
                    socket.BeginSend( data, 0, dataLength, SocketFlags.None, new AsyncCallback( WriteCallback ), obj );

                    bool signalled = sendTimeoutObject.WaitOne( TIMEOUT, false );

                    if( !signalled )
                    {
                        const string msg = "Write async tcp socket timeout!";
                        throw new Exception( msg );
                    } 
                    else
                    {
                        //DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Async socket's BeginSend is complete!" );
                    }
                }
                catch( SocketException e )
                {
                    DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async tcp socket has SocketException when sending request: " + e + ", error code = " + e.ErrorCode + ", native error code = " + e.NativeErrorCode + ", socket error code = " + e.SocketErrorCode );
                    Disconnect( sendRequestFailedCallback );
                    //ProcessDisconnnectInGame();
                    return;
                }
                catch( Exception e ) 
                {
                    DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async tcp socket has Exception when sending request: " + e );
                    Disconnect( sendRequestFailedCallback );
                    return;
                }
            }
            else
            {
                string msg = string.Format( "Tcp socket to {0} is not connected, but you still try to send message!", serverType );
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, msg );
                Loom.QueueOnMainThread( sendRequestFailedCallback );
            }
        }

        private void WriteCallback( IAsyncResult asyncWrite )
        {
            WriteSocketObject obj = (WriteSocketObject)asyncWrite.AsyncState;

            try
            {
                Socket socket = obj.socket;
                if( socket != null && socket.Connected )
                {
                    int len = socket.EndSend( asyncWrite );
                    obj.WriteSuccessCallback();
                }
            } 
            catch( SocketException e )
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async tcp socket has SocketException when sending request callback: " + e + ", error code = " + e.ErrorCode + ", native error code = " + e.NativeErrorCode + ", socket error code = " + e.SocketErrorCode );
                Disconnect( obj.WriteFailedCallback );
                ProcessDisconnnectInGame();
            }
            catch( Exception e )
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async tcp socket has Exception when sending request callback: " + e );
                Disconnect();
            } 
            finally
            {
                sendTimeoutObject.Set();
            }
        }

		private void ReadCallback( IAsyncResult asyncRead )
        {
            try
            {
                Socket socket = (Socket)asyncRead.AsyncState;

                /*
                if ( socket != this.socket )
                {
                    DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "There is different async socket! The original socket address:" + this.socket.RemoteEndPoint.ToString() + ", the incoming socket address: " + socket.RemoteEndPoint.ToString() );
                }
                */

                if ( !isConnected || !socket.Connected )
                {
                    asyncRead.AsyncWaitHandle.Close();
                    DebugUtils.LogWarning( DebugUtils.Type.AsyncSocket, string.Format( "Async tcp socket to {0} isn't connected when receiving data!", serverType ) );
                    return;
                }

                int readCount = socket.EndReceive( asyncRead );
                if ( readCount > 0 )
                {
                    if ( unusedBuffer != null ) 
                    {
                        byte[] bytes = new byte[readCount + unusedBuffer.Length];
                        Buffer.BlockCopy( unusedBuffer, 0, bytes, 0, unusedBuffer.Length );
                        Buffer.BlockCopy( buffer, 0, bytes, unusedBuffer.Length, readCount );
                        unusedBuffer = null;
                        ReadData( bytes );
                    } 
                    else
                    {
                        byte[] bytes = new byte[readCount];
                        Buffer.BlockCopy( buffer, 0, bytes, 0, readCount );
                        ReadData( bytes );
                    }
                } 
                else
                {
                    isConnected = false;
                    DebugUtils.LogWarning( DebugUtils.Type.AsyncSocket, string.Format( "Async tcp socket to {0} receives 0 byte!", serverType ) );
                    return;
                }

                socket.BeginReceive( buffer, 0, BUFFERSIZE, SocketFlags.None, new AsyncCallback( ReadCallback ), socket );
            }
            catch( System.Net.Sockets.SocketException e )
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, string.Format( "Async tcp socket to {0} has SocketException when reading: {1}, error code = {2}, native error code = {3}, socket error code = {4}", serverType, e, e.ErrorCode, e.NativeErrorCode, e.SocketErrorCode ) );
                Disconnect( this.ProcessDisconnnectInGame );
                return;
            } 
            catch( Exception e )
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, string.Format( "Async tcp socket to {0} has Exception when reading socket: {1}", serverType, e ) );
                Disconnect();
                return;
            }
        }

        /// <summary>
        /// the functions reading the data from socket.
        /// </summary>
        private void ReadData( byte[] data )
        {
            DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Async tcp socket has received network data, data length " + data.Length );

            using( ByteStreamReader reader = new ByteStreamReader( data ) )
            {
                try
                {
                    int dataLength = data.Length;
                    int startPos = 0;
                    while( startPos < dataLength )
                    {
                        int length = reader.ReadShort();
                        if( startPos + length > dataLength )
                        {
                            DebugUtils.LogWarning( DebugUtils.Type.AsyncSocket, "Async tcp socket: There are separated packets!" );
                            int separatedLength = dataLength - startPos;
                            unusedBuffer = new byte[separatedLength];
                            Buffer.BlockCopy( data, startPos, unusedBuffer, 0, separatedLength );
                            break;
                        }

                        short version = reader.ReadShort(); //version == 127

                        byte[] response = reader.ReadBytes( length - 4 );
                        ServerTcpMessage[] messages = ServerTcpMessage.Unpack( response );
                        if( ResponseHandler != null )
                        {
                            ResponseHandler( messages );
                        }

                        startPos += length;
                    }
                } 
                catch( Exception e )
                {
                    DebugUtils.LogError( DebugUtils.Type.AsyncSocket, e.ToString() );
                }
            }
        }

        public void RegisterResponseHandler( Action<ServerTcpMessage[]> handler )
        {
            ResponseHandler += handler;
        }

        public void UnregisterResponseHandler( Action<ServerTcpMessage[]> handler )
        {
            ResponseHandler -= handler;
        }

        private void ProcessDisconnnectInGame()
        {
            DebugUtils.LogWarning( DebugUtils.Type.AsyncSocket, "Async tcp socket processes disconnecting!" );

            if( AccidentDisconnectCallback != null )
            {
                Loom.QueueOnMainThread( () => AccidentDisconnectCallback( serverType ) );
            }
        }

        public void Reconnect( Action<ClientType> ReconnectSuccessCallback = null, Action<ClientType> ReconnectFailCallback = null )
        {
            Connect( serverType, serverUrl, port, ( bool success ) =>
            {
                if (success)
                {
                    DebugUtils.Log(DebugUtils.Type.AsyncSocket, "Async tcp socket reconnect successully!");
                    if( ReconnectSuccessCallback != null )
                        ReconnectSuccessCallback( ClientType.Tcp );
                } 
                else
                {
                    //TODO: show Disconnect message when reconnecting.
                    if( ReconnectFailCallback != null )
                        ReconnectFailCallback( ClientType.Tcp );
                }
            } );
        }

        /// <summary>
        /// Write socket object.
        /// </summary>
        public class WriteSocketObject
        {
            public Socket socket;
            public Action WriteSuccessCallback;
            public Action WriteFailedCallback;
        }

        /// <summary>
        /// Debug function
        /// </summary>
        public static string DebugBuffer(byte[] buffer)
        {
            StringBuilder str = new StringBuilder("{");
            for (int i = 0; i < buffer.Length - 1; i++)
            {
                str.Append (buffer [i]);
                str.Append (", ");
            }

            if (buffer.Length > 0)
            {
                str.Append( buffer[buffer.Length - 1] );
            }

            str.Append( "}" );
            return str.ToString();
        }
    }

}
