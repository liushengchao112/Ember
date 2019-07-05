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
using System.Net;
using System.Threading;

using Data;
using Utils;

namespace Network
{
    public class AsyncUdpClient
    {
        private const int BUFFERSIZE = 16 * 1024 * 10;
        private const int TIMEOUT = 5000;

        private event Action<ServerUdpMessage[]> ResponseHandler;

        private Action<bool> OnConnect;

        public static Action<ServerType> AccidentDisconnectCallback = null;

        private Socket socket;
        private ServerType serverType;
        private string serverUrl;
        private int port;
        private EndPoint server;


        private byte[] buffer = new byte[BUFFERSIZE];
        private byte[] unusedBuffer = null;

        private List<Segment> waitingSegments = new List<Segment>();

        private bool isConnected = false;

        public AsyncUdpClient() {}

        ~AsyncUdpClient()
        {
            Disconnect();
        }

        public bool IsConnected()
        {
            return ( isConnected && socket != null );
        }

        /// <summary>
        /// mark the server
        /// </summary>
        public void Connect( ServerType serverType, string serverUrl, int port, Action<bool> connectCallback )
        {
            if( socket != null )
            {
                DebugUtils.Log( DebugUtils.Type.AsyncSocket, String.Format( "Async udp socket has already talked to {0} {1}.", this.serverType, this.serverUrl ) );
                Disconnect();
            }

            try 
            {
                DebugUtils.Log( DebugUtils.Type.AsyncSocket, String.Format( "Async udp socket prepares for {0} {1}:{2}", serverType, serverUrl, port ) );

                this.serverType = serverType;
                this.serverUrl = serverUrl;
                this.port = port;
                this.server = new IPEndPoint( IPAddress.Parse( serverUrl ), port );

                socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
                //socket.SetSocketOption( SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true );
                //socket.BeginConnect( serverUrl, port, new AsyncCallback(ConnectCallback), socket );

                socket.BeginReceiveFrom( buffer, 0, BUFFERSIZE, SocketFlags.None, ref server, new AsyncCallback( ReadCallback ), socket );
            }
            catch( Exception e )
            {
                Disconnect();

                if( connectCallback != null )
                {
                    connectCallback( false );
                }

                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async udp socket error when preparing: " + e );
            }
        }

        public void Disconnect( Action callback = null )
        {               
            try
            {
                if( socket != null )
                {
                    isConnected = false;
                    socket.Close();
                    DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Async udp socket is closing!" );
                } 
                else
                {
                    DebugUtils.LogWarning( DebugUtils.Type.AsyncSocket, "Disconnect a closed async udp socket!" );
                }
            } 
            catch( Exception e )
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Close async udp socket error: " + e );
            }
            finally
            {
                isConnected = false;
                socket = null;

                Loom.QueueOnMainThread( callback );
            }
        }

        public void ResendRequests()
        {
            if( isConnected && socket != null )
            {
                try
                {
                    for( int j = 0; j < waitingSegments.Count; j++ )
                    {
                        Segment segment = waitingSegments[j];
                        socket.BeginSendTo( segment.data, 0, segment.data.Length, SocketFlags.None, server, new AsyncCallback( RewriteCallback ), socket );
                    }
                }
                catch( SocketException e )
                {
                    DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async udp socket has SocketException when resending request: " + e + ", error code = " + e.ErrorCode + ", native error code = " + e.NativeErrorCode + ", socket error code = " + e.SocketErrorCode );
                    Disconnect();
                    return;
                }
                catch( Exception e ) 
                {
                    DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async udp socket has Exception when resending requeset: " + e );
                    Disconnect();
                    return;
                }
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, string.Format( "Udp socket to {0} is not prepared, but you still try to resend message!", serverType ) );
            }
        }

        private void RewriteCallback( IAsyncResult asyncWrite )
        {
            try
            {
                socket.EndSendTo( asyncWrite );
            } 
            catch( SocketException e )
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async udp socket has SocketException when resending request callback: " + e + ", error code = " + e.ErrorCode + ", native error code = " + e.NativeErrorCode + ", socket error code = " + e.SocketErrorCode );
                Disconnect();
            }
            catch( Exception e )
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async udp socket has exception when resending request callback : " + e );
                Disconnect();
            } 
        }

        private ManualResetEvent sendTimeoutObject = new ManualResetEvent( false );

        /// <summary>
        /// the functions sending the request
        /// </summary>
        public void SendRequest( long seq, byte[] data, int dataLength, Action sendRequestSuccessCallback, Action sendRequestFailedCallback )
        {
            if( isConnected && socket != null )
            {
                //DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Send Start " + data.Length + " : " + DebugBuffer( data ) + ", " + DateTime.Now );

                try
                {
                    Segment segment = new Segment();
                    segment.seq = seq;
                    segment.data = data;
                    waitingSegments.Add( segment );

                    WriteSocketObject obj = new WriteSocketObject();
                    obj.socket = socket;
                    obj.WriteSuccessCallback = sendRequestSuccessCallback;
                    obj.WriteFailedCallback = sendRequestFailedCallback;

                    sendTimeoutObject.Reset();
                    socket.BeginSendTo( data, 0, dataLength, SocketFlags.None, server, new AsyncCallback( WriteCallback ), obj );

                    bool signalled = sendTimeoutObject.WaitOne( TIMEOUT, false );

                    if( !signalled )
                    {
                        const string msg = "Write async udp socket timeout!";
                        throw new Exception( msg );
                    } 
                    else
                    {
                        //DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Async socket's BeginSend is complete!" );
                    }
                }
                catch( SocketException e )
                {
                    DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async udp socket has SocketException when sending request: " + e + ", error code = " + e.ErrorCode + ", native error code = " + e.NativeErrorCode + ", socket error code = " + e.SocketErrorCode );
                    Disconnect( sendRequestFailedCallback );
                    //ProcessDisconnnectInGame();
                    return;
                }
                catch( Exception e ) 
                {
                    DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async udp socket has Exception when sending requeset: " + e );
                    Disconnect( sendRequestFailedCallback );
                    return;
                }
            }
            else
            {
                string msg = string.Format( "Udp socket to {0} is not prepared, but you still try to send message!", serverType );
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
                    int len = socket.EndSendTo( asyncWrite );
                    obj.WriteSuccessCallback();
                }
            } 
            catch( SocketException e )
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async udp socket has SocketException when sending request callback: " + e + ", error code = " + e.ErrorCode + ", native error code = " + e.NativeErrorCode + ", socket error code = " + e.SocketErrorCode );
                Disconnect( obj.WriteFailedCallback );
                ProcessDisconnnectInGame();
            }
            catch( Exception e )
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async udp socket has exception when sending request callback : " + e );
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

                if ( !isConnected )
                {
                    asyncRead.AsyncWaitHandle.Close();
                    DebugUtils.LogWarning( DebugUtils.Type.AsyncSocket, "Async udp socket doesn't want to listening when receiving data!" );
                    return;
                }

                EndPoint tempServer = null;
                int readCount = socket.EndReceiveFrom( asyncRead, ref tempServer );

                // the code doesn't check the port number.
                if( ( tempServer.AddressFamily == AddressFamily.InterNetwork || tempServer.AddressFamily == AddressFamily.InterNetworkV6 ) && ((IPEndPoint)tempServer).Address == ((IPEndPoint)server).Address )
                {
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
                        DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async udp socket receives 0 byte!" );
                        return;
                    }
                }

                socket.BeginReceiveFrom( buffer, 0, BUFFERSIZE, SocketFlags.None, ref server, new AsyncCallback( ReadCallback ), socket );
            }
            catch( System.Net.Sockets.SocketException e )
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async udp socket has SocketException when reading: " + e + ", error code = " + e.ErrorCode + ", native error code = " + e.NativeErrorCode + ", socket error code = " + e.SocketErrorCode );
                Disconnect( this.ProcessDisconnnectInGame );
                return;
            } 
            catch( Exception e )
            {
                DebugUtils.LogError( DebugUtils.Type.AsyncSocket, "Async udp socket has Exception when reading socket: " + e );
                Disconnect();
                return;
            }
        }

        /// <summary>
        /// the functions reading the data from socket.
        /// </summary>
        private void ReadData( byte[] data )
        {
            DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Async udp socket has received network data, data length " + data.Length );

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
                            DebugUtils.LogWarning( DebugUtils.Type.AsyncSocket, "Async udp socket: There are separated packets!" );
                            int separatedLength = dataLength - startPos;
                            unusedBuffer = new byte[separatedLength];
                            Buffer.BlockCopy( data, startPos, unusedBuffer, 0, separatedLength );
                            break;
                        }

                        short version = reader.ReadShort(); //version == 127

//                        byte[] response = reader.ReadBytes( length - 4 );
//                        ServerTcpMessage[] messages = ServerTcpMessage.Unpack( response );
//                        if( ResponseHandler != null )
//                        {
//                            ResponseHandler( messages );
//                        }

                        startPos += length;
                    }
                } 
                catch( Exception e )
                {
                    DebugUtils.LogError( DebugUtils.Type.AsyncSocket, e.ToString() );
                }
            }
        }

        public void RegisterResponseHandler( Action<ServerUdpMessage[]> handler )
        {
            ResponseHandler += handler;
        }

        public void UnregisterResponseHandler( Action<ServerUdpMessage[]> handler )
        {
            ResponseHandler -= handler;
        }

        private void ProcessDisconnnectInGame()
        {
            DebugUtils.LogWarning( DebugUtils.Type.AsyncSocket, "Async udp socket processes disconnecting!" );

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
                    DebugUtils.Log(DebugUtils.Type.AsyncSocket, "Async udp socket reconnect successfully!");
                    if( ReconnectSuccessCallback != null )
                        ReconnectSuccessCallback( ClientType.Udp );
                } 
                else
                {
                    //TODO: show Disconnect message when reconnecting.
                    if( ReconnectFailCallback != null )
                        ReconnectFailCallback( ClientType.Udp );
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

        public class Segment
        {
            public long seq;
            public byte[] data;
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
