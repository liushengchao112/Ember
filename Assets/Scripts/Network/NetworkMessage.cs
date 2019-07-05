/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: NetworkMessage.cs
// description: 
// 
// created time：10/12/2016
//
//----------------------------------------------------------------*/


using System;
using System.Collections.Generic;

using Data;
using Constants;
using Utils;

namespace Network
{
    public enum NetMessageState
    {
        Normal,
        Error,
        Retry,
        Wait,
        Complete,
    }

    public class ClientUdpMessage
    {
        public static short version = 127;
        public static long playerId = -1; 
        public static int sessionId = -1;

        public long sequence; // from 0
        public long ack;
        public int protocalCode;
        public long endSequence;
        public int checkSum;
        public byte[] data;

        public Action OnRequestSuccess;
        public Action OnRequestFailed;

        private ClientUdpMessage( int protocalCode, long sequence, long ack, long endSequence, byte[] data )
        {
            this.protocalCode = protocalCode;
            this.sequence = sequence;
            this.ack = ack;
            this.endSequence = endSequence;
            this.data = data;

            this.OnRequestSuccess += OnSendingSuccess;
            this.OnRequestFailed += OnSendingFailed;
        }

        public byte[] Encode()
        {
            using( ByteStreamWriter writer = new ByteStreamWriter() )
            {
                writer.WriteShort( data.Length + 48 ); //version(2)+roleId(8)+SessionID(4)+sequence(8)+ack(8)+protocalCode(4)+endSequence(8)+checkSum(4) = 46
                writer.WriteShort( version );
                writer.WriteLong( playerId );
                writer.WriteInt( sessionId );
                writer.WriteLong( sequence );
                writer.WriteLong( ack );
                writer.WriteInt( protocalCode );
                writer.WriteLong( endSequence );
                //TODO: calculate checkSum.
                writer.WriteInt( checkSum );
                writer.WriteBytes( data );

                //Jiawen: optimise GetBuff function to save some new operations.
                return writer.GetBuffer();
            }
        }

        public static ClientUdpMessage[] CreateUdpMessages( int protocalCode, ref long sequence, long ack, byte[] data )
        {
            int length = data.Length;
            int num = ( length - 1 ) / NetworkConstants.MAX_SEGMENT_SIZE;
            if( num >= 0 )
            {
                num += 1;
                ClientUdpMessage[] messages = new ClientUdpMessage[num];
                if( num == 1 )
                {
                    messages[0] = new ClientUdpMessage( protocalCode, sequence, ack, sequence++, data );
                }
                else
                {
                    int totalSize = 0;
                    long endSequence = sequence + num;
                    for( int j = 0; j < num; j++ )
                    {
                        if( j < num - 1 )
                        {
                            byte[] d = new byte[NetworkConstants.MAX_SEGMENT_SIZE];
                            Buffer.BlockCopy( data, totalSize, d, 0, NetworkConstants.MAX_SEGMENT_SIZE );
                            messages[j] = new ClientUdpMessage( protocalCode, sequence++, ack, endSequence, d );
                            totalSize += NetworkConstants.MAX_SEGMENT_SIZE;
                        }
                        else
                        {
                            DebugUtils.Assert( sequence == endSequence, string.Format( "the ending sequence {0} isn't right! the num = {1}", endSequence, num ) );
                            int len = length - totalSize;
                            byte[] d = new byte[len];
                            Buffer.BlockCopy( data, totalSize, d, 0, len );
                            messages[j] = new ClientUdpMessage( protocalCode, sequence++, ack, endSequence, d );
                        }
                    }
                }

                return messages;
            }
            else
            {
                return null;
            }
        }

        void OnSendingSuccess()
        {
            DebugUtils.Log( DebugUtils.Type.Protocol, String.Format( "sending protocol {0} successfully!", protocalCode ) );
        }

        void OnSendingFailed()
        {
            DebugUtils.LogError( DebugUtils.Type.Protocol, String.Format( "sending protocol {0} failed!", protocalCode ) );
        }
    }

    public class ClientTcpMessage
    {
        public static short version = 127;
        //don't change playerId and sessionId's initial values.
        public static long playerId = -1; 
        public static int sessionId = -1;
        public static int gameVersion = 0;
        public static int resVersion = 0;
        public static byte platform = 0;

        public int protocalCode;
        public byte[] data;
        public long sequence;
        public bool compressed;
        public bool encrypted;

        public Action OnRequestSuccess;
        public Action OnRequestFailed;

        public static void Reset()
        {
            version = 127;
            playerId = -1;
            sessionId = -1;
            gameVersion = 0;
            resVersion = 0;
            platform = 0;
        }

        public ClientTcpMessage( int protocalCode, byte[] data, long sequence, bool compressed = false, bool encrypted = false )
        {
            this.protocalCode = protocalCode;
            this.data = data;
            this.sequence = sequence;
            this.compressed = compressed;
            this.encrypted = encrypted;
            this.OnRequestSuccess += OnSendingSuccess;
            this.OnRequestFailed += OnSendingFailed;
        }

        public byte[] Encode()
        {
            using( ByteStreamWriter writer = new ByteStreamWriter() )
            {
                writer.WriteShort( data.Length + 35 ); //version(2)+roleId(8)+SessionID(4)+sequence(8)+gameVersion(2)+resVersion(4)+platform(1)+protocalCode(4) = 33
                writer.WriteShort( version );
                writer.WriteLong( playerId );
                writer.WriteInt( sessionId );
                writer.WriteLong( sequence );
                writer.WriteShort( gameVersion );
                writer.WriteInt( resVersion );
                writer.WriteByte( platform );
                writer.WriteInt( protocalCode );
                writer.WriteBytes( data );
                //Jiawen: optimise GetBuff function to save some new operations.
                return writer.GetBuffer();
            }
        }

        void OnSendingSuccess()
        {
            DebugUtils.Log( DebugUtils.Type.Protocol, String.Format( "sending protocol {0} successfully!", protocalCode ) );
        }

        void OnSendingFailed()
        {
            DebugUtils.LogError( DebugUtils.Type.Protocol, String.Format( "sending protocol {0} failed!", protocalCode ) );
        }
    }

    public class ServerUdpMessage
    {
        
    }

    public class ServerTcpMessage
    {
        public ServerType serverType;
        public MsgCode protocalCode;
        public long sequence;
        public byte[] data;
        //public bool needCompress;
        //public bool needEncrypt;

        private ServerTcpMessage( MsgCode protocalCode, long seq, byte[] data )
        {
            this.protocalCode = protocalCode;
            this.sequence = seq;
            this.data = data;
        }

        public static ServerTcpMessage[] Unpack( /*ServerType serverType,*/ byte[] responseData )
        {
            DebugUtils.Assert( responseData != null && responseData.Length > 0, "Response data is null!" );

            using( ByteStreamReader reader = new ByteStreamReader( responseData ) )
            {
                long serverId = reader.ReadLong();
                ClientTcpMessage.sessionId = reader.ReadInt(); //if sessionId is 4 bytes, that's OK.
                int msgNum = reader.ReadByte();
                ServerTcpMessage[] responseMessages = new ServerTcpMessage[msgNum];
                for ( int i = 0;i < msgNum; i++ )
                {
                    int len = reader.ReadShort();
                    int code = reader.ReadInt();
                    long seq = reader.ReadLong();

                    if ( Enum.IsDefined( typeof( MsgCode ), code ) )
                    {
                        responseMessages[i] = new ServerTcpMessage( (MsgCode)code, seq, reader.ReadBytes( len ) );
                        DebugUtils.Log( DebugUtils.Type.Protocol, "Receive network message, protocol code " + code );
                    }
                    else
                    {
                        DebugUtils.LogError( DebugUtils.Type.Protocol, "For now, the client can't recognize the received protocol code " + code );
                    }
                }

                return responseMessages;
            }
        }
    }

}
