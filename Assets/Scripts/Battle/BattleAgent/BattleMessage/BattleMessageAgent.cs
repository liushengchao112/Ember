using System.Collections.Generic;

using Data;
using System;
using Network;
using Utils;

namespace BattleAgent
{
    public class BattleMessageAgent
    {
        public static BattleType battleType;
        public static bool simulateBattle;

        private static Dictionary<MsgCode, int> messageLength = new Dictionary<MsgCode, int>();
        private static Dictionary<OperationType, int> operationLength = new Dictionary<OperationType, int>();

        public static void SendRequest( MsgCode protocolCode, byte[] data, Action onRequestSuccess = null, Action onResquestFailed = null, int resendTimes = 1 )
        {
            // data package analyze
            MessageAnalyze( protocolCode, data );

            if ( !simulateBattle )
            {
                switch ( battleType )
                {
                    case BattleType.BattleP1vsP1:
                    case BattleType.BattleP2vsP2:
                    {
                        DebugUtils.Log( DebugUtils.Type.MessageAnalyze, string.Format( "send message, msgcode {0}, length {1}", protocolCode, data.Length ) );
                        NetworkManager.SendRequest( protocolCode, data, ClientType.Tcp, onRequestSuccess, onResquestFailed, resendTimes );
                        break;
                    }
                    case BattleType.Survival:
                    case BattleType.Tranining:
					case BattleType.Tutorial:
                    {
                        LocalBattleMessageManager.SendRequest( protocolCode, data, onRequestSuccess );
                        break;
                    }
                    default:
                    {
                        DebugUtils.LogError( DebugUtils.Type.Battle, "Can't handle this battleType now! type = " + battleType );
                        break;
                    }
                }
            }
            else
            {
                SimulateBattleMessageManager.SendRequest( protocolCode, data, onRequestSuccess );
            }
        }

        public static void RegisterAgentMessageHandler( MsgCode protocolCode, Action<byte[]> handler )
        {
            if ( !simulateBattle )
            {
                switch ( battleType )
                {
                    case BattleType.BattleP1vsP1:
                    case BattleType.BattleP2vsP2:
                    {
                        NetworkManager.RegisterServerMessageHandler( protocolCode, handler );
                        break;
                    }
                    case BattleType.Survival:
                    case BattleType.Tranining:
					case BattleType.Tutorial:
                    {
                        LocalBattleMessageManager.RegisterLocalMessageHandler( protocolCode, handler );
                        break;
                    }
                    case BattleType.NoBattle:
                    {
                        DebugUtils.LogWarning( DebugUtils.Type.Battle, "Preview mode!" );
                        break;
                    }
                    default:
                    {
                        DebugUtils.LogError( DebugUtils.Type.Battle, "Can't handle this battleType now! type = " + battleType );
                        break;
                    }
                }
            }
            else
            {
                SimulateBattleMessageManager.RegisterSimulateMessageHandler( protocolCode, handler );
            }
        }

        public static void RemoveAgentMessageHandler( MsgCode protocolCode, Action<byte[]> handler )
        {
            if ( !simulateBattle )
            {
                switch ( battleType )
                {
                    case BattleType.BattleP1vsP1:
                    case BattleType.BattleP2vsP2:
                    {
                        NetworkManager.RemoveServerMessageHandler( ServerType.BattleServer, protocolCode, handler );
                        break;
                    }
					case BattleType.Tutorial:
                    case BattleType.Survival:
                    case BattleType.Tranining:
                    {
                        LocalBattleMessageManager.RemoveLocalMessageHandler( protocolCode, handler );
                        break;
                    }
                    case BattleType.NoBattle:
                    {
                        DebugUtils.LogWarning( DebugUtils.Type.Battle, "Preview mode!" );
                        break;
                    }
                    default:
                    {
                        DebugUtils.LogError( DebugUtils.Type.Battle, "Can't handle this battleType now! type = " + battleType );
                        break;
                    }
                }
            }
            else
            {
                SimulateBattleMessageManager.RemoveSimulateMessageHandler( protocolCode, handler );
            }
        }

        public static void SetBattleSpeed( float speed )
        {
            if ( !simulateBattle )
            {
                switch ( battleType )
                {
                    case BattleType.BattleP1vsP1:
                    case BattleType.BattleP2vsP2:
                    case BattleType.Survival:
                    case BattleType.Tranining:
					case BattleType.Tutorial:
                    case BattleType.NoBattle:
                    default:
                    {
                        DebugUtils.LogError( DebugUtils.Type.Battle, "Can't set battle speed in real battle" );
                        break;
                    }
                }
            }
            else
            {
                SimulateBattleMessageManager.SetBattleSpeed( speed );
            }
        }

        public static void SetBattleRecordPlayState( int state )
        {
            if ( !simulateBattle )
            {
                switch ( battleType )
                {
                    case BattleType.BattleP1vsP1:
                    case BattleType.BattleP2vsP2:
                    case BattleType.Survival:
                    case BattleType.Tranining:
					case BattleType.Tutorial:
                    case BattleType.NoBattle:
                    default:
                    {
                        DebugUtils.LogError( DebugUtils.Type.Battle, "Can't set battle play state in real battle" );
                        break;
                    }
                }
            }
            else
            {
                SimulateBattleMessageManager.SetBattleRecordPlayState( state );
            }
        }

        public static void MessageAnalyze( MsgCode protocolCode, byte[] data )
        {
            DebugUtils.Log( DebugUtils.Type.MessageAnalyze, string.Format( "send protocolCode {0} total length:{1}", protocolCode, data.Length ) );

            if ( protocolCode == MsgCode.UpdateMessage )
            {
                UpdateC2S u = ProtobufUtils.Deserialize<UpdateC2S>( data );
                DebugUtils.LogWarning( DebugUtils.Type.MessageAnalyze, string.Format( "operation: {0} total length:{1}", u.operation.opType, data.Length ) );
            }
        }
    }
}
