using UnityEngine;
using System.Collections.Generic;
using System;

using Data;
using Utils;
using Network;
using Constants;

namespace BattleAgent
{
    public class LocalBattleMessageManager : MonoBehaviour
    {
		private enum LocalBattleStatus
		{
			Start,
			Normaly,
			Pause,
			End
		}

        class LocalMessage
        {
            public MsgCode msgCode;
            public byte[] data;
            public Action onRequestSuccess;
        }

        private static Dictionary<MsgCode, Action<byte[]>> localMessageHandlers = new Dictionary<MsgCode, Action<byte[]>>();
        private static List<LocalMessage> messageList = new List<LocalMessage>();

        private long frameIndex = 1;
		private LocalBattleStatus status = LocalBattleStatus.Start;
    	private float logicTimer = 0;

        void Awake()
        {
            //localMessageHandlers = new Dictionary<MsgCode, Action<byte[]>>();
			MessageDispatcher.AddObserver( BattleEnd, MessageType.TutorialShowResult );
			MessageDispatcher.AddObserver( BattlePause, MessageType.LocalBattlePause );
			MessageDispatcher.AddObserver( BattleContinue, MessageType.LocalBattleContinue );
        }

        private void OnDestroy()
        {
            if ( messageList != null )
            {
                messageList.Clear();
            }
            MessageDispatcher.RemoveObserver( BattleEnd, MessageType.BattleEnd );
			MessageDispatcher.RemoveObserver( BattlePause, MessageType.LocalBattlePause );
			MessageDispatcher.RemoveObserver( BattleContinue, MessageType.LocalBattleContinue );
        }

        void Update()
        {
			if ( status == LocalBattleStatus.End | status == LocalBattleStatus.Pause )
            {
                return;
            }
    			
    		logicTimer += Time.deltaTime;

    		if( logicTimer + Time.deltaTime * 0.5f > GameConstants.LOGIC_FRAME_TIME )
    		{
    			BroadCastFeedBack( frameIndex );
    			frameIndex++;
    			logicTimer = 0;
    		}
        }

        private void BroadCastFeedBack( long frame )
        {
            byte[] dataS2C = null;

            UpdateS2C updateS2C = new UpdateS2C();
            updateS2C.battleId = 0;
            updateS2C.timestamp = frame;

			if( status == LocalBattleStatus.Start )
			{
				NoticeS2C noticeS2C = new NoticeS2C();
				noticeS2C.type = NoticeType.BattleBegin;
				dataS2C = ProtobufUtils.Serialize( noticeS2C );

				localMessageHandlers[MsgCode.NoticeMessage]( dataS2C );
				status = LocalBattleStatus.Normaly;
			}

            for ( int i = messageList.Count - 1; i >= 0; i-- )
            {
                if (messageList[i].msgCode == MsgCode.UpdateMessage)
                {
                    UpdateC2S updateC2S = ProtobufUtils.Deserialize<UpdateC2S>( messageList[i].data );
                    updateS2C.ops.Add( updateC2S.operation );
                }
                else if (messageList[i].msgCode == MsgCode.NoticeMessage)
                {
                    NoticeC2S noticeC2S = ProtobufUtils.Deserialize<NoticeC2S>( messageList[i].data );

                    NoticeS2C noticeS2C = new NoticeS2C();
                    noticeS2C.type = noticeC2S.type;
                    dataS2C = ProtobufUtils.Serialize( noticeS2C );

                    DebugUtils.Log( DebugUtils.Type.LocalBattleMessage, string.Format( "Send {0} feed back succeed ", messageList[i].msgCode ) );
                    localMessageHandlers[MsgCode.NoticeMessage]( dataS2C );

                    // battle end need to stop send updateS2CS
                    if (noticeC2S.type == NoticeType.BattleResultBlueWin ||
                        noticeC2S.type == NoticeType.BattleResultRedWin ||
                        noticeC2S.type == NoticeType.BattleResultDraw)
                    {
						status = LocalBattleStatus.End;
                    }
                }
                else if (messageList[i].msgCode == MsgCode.SyncMessage)
                {
                    SyncC2S syncC2S = ProtobufUtils.Deserialize<SyncC2S>( messageList[i].data );

                    Sync s = new Sync();
                    s.syncState = syncC2S.syncState;
                    s.unitId = syncC2S.uintId;
                    s.continuedWalkNum = syncC2S.continuedWalkNum;
                    s.timestamp = frame;
                    foreach (Position item in syncC2S.positions)
                    {
                        s.positions.Add( item );
                    }

                    Operation operation = new Operation();
                    operation.sync = s;
                    operation.opType = OperationType.SyncPath;

                    updateS2C.ops.Add( operation );
                }
                else if (messageList[i].msgCode == MsgCode.QuitBattleMessage)
                {
                    QuitBattleC2S quitBattleC2S = ProtobufUtils.Deserialize<QuitBattleC2S>( messageList[i].data );
                    QuitBattleS2C quitBattleS2C = new QuitBattleS2C();
                    quitBattleS2C.serverIp = "127.0.0.1";
                    quitBattleS2C.serverPort = 0;
                    quitBattleS2C.serverType = ServerType.GameServer;
                    dataS2C = ProtobufUtils.Serialize( quitBattleS2C );

                    DebugUtils.Log( DebugUtils.Type.LocalBattleMessage, string.Format( "Send {0} feed back succeed ", messageList[i].msgCode ) );
                    localMessageHandlers[MsgCode.QuitBattleMessage]( dataS2C );
					status = LocalBattleStatus.End;

                    return;
                }
                else if (messageList[i].msgCode == MsgCode.UploadSituationMessage)
                {
                    UploadSituationC2S uploadSituationC2S = ProtobufUtils.Deserialize<UploadSituationC2S>( messageList[i].data );

                    UploadSituationS2C uploadSituationS2C = new UploadSituationS2C();
                    uploadSituationS2C.type = uploadSituationC2S.type;
                    dataS2C = ProtobufUtils.Serialize( uploadSituationS2C );

                    DebugUtils.Log( DebugUtils.Type.LocalBattleMessage, string.Format( "Send {0} feed back succeed ", messageList[i].msgCode ) );

                    Action<byte[]> callback = null;
                    if (localMessageHandlers.TryGetValue( MsgCode.UploadSituationMessage, out callback ))
                    {
                        callback.Invoke( dataS2C );
                    }
                }
                else
                {
                    DebugUtils.LogError( DebugUtils.Type.LocalBattleMessage, string.Format("Can't handle this local message now message code = {0} ", messageList[i].msgCode ) );
                }

                messageList.RemoveAt( i );
            }

            dataS2C = ProtobufUtils.Serialize( updateS2C );

            for ( int i = 0; i < updateS2C.ops.Count; i++ )
            {
                DebugUtils.Log( DebugUtils.Type.LocalBattleMessage, string.Format( "Frame: Send feed back operation {0}", updateS2C.ops[i].opType ) );
            }

            localMessageHandlers[MsgCode.UpdateMessage]( dataS2C );
        }

        public static void SendRequest( MsgCode protocolCode, byte[] data, Action onRequestSuccess = null )
        {
            if ( MsgCode.PveBattleResultMessage == protocolCode )
            {
                // Specifically, pve needs to request server data
                NetworkManager.SendRequest( ServerType.GameServer, protocolCode, data, ClientType.Tcp, onRequestSuccess );
            }
            else if ( localMessageHandlers.ContainsKey( protocolCode ) )
            {
                DebugUtils.Log( DebugUtils.Type.LocalBattleMessage, string.Format( "Send local request {0} succeed ", protocolCode ) );
                LocalMessage msg = new LocalMessage();
                msg.data = data;
                msg.msgCode = protocolCode;
                msg.onRequestSuccess = onRequestSuccess;

                messageList.Add( msg );
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.LocalBattleMessage, string.Format( " Can't handle request {0} ", protocolCode ) );
            }
        }

        public static void RegisterLocalMessageHandler( MsgCode protocolCode, Action<byte[]> handler )
        {
            if ( MsgCode.PveBattleResultMessage == protocolCode )
            {
                // Specifically, pve needs to request server data
                NetworkManager.RegisterServerMessageHandler( ServerType.GameServer, protocolCode, handler );
            }
            else if ( !localMessageHandlers.ContainsKey( protocolCode ) )
            {
                localMessageHandlers.Add( protocolCode, handler );
            }
            else
            {
                if ( localMessageHandlers[protocolCode] != null )
                {
                    Delegate[] delegates = localMessageHandlers[protocolCode].GetInvocationList();
                    foreach ( Delegate del in delegates )
                    {
                        if ( del.Equals( handler ) )
                            return;
                    }
                }
                localMessageHandlers[protocolCode] += handler;
            }
        }

        public static void RemoveLocalMessageHandler( MsgCode protocolCode, Action<byte[]> handler )
        {
            if ( MsgCode.PveBattleResultMessage == protocolCode )
            {
                // Specifically, pve needs to request server data
                NetworkManager.RemoveServerMessageHandler( ServerType.GameServer, protocolCode, handler );
            }
            else if ( localMessageHandlers.ContainsKey( protocolCode ) )
            {
                localMessageHandlers[protocolCode] -= handler;
                if ( localMessageHandlers[protocolCode] == null )
                    localMessageHandlers.Remove( protocolCode );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.LocalBattleMessage, "Remove an unregistered handler for protocol " + (int)protocolCode );
            }
        }

        private void BattleEnd( object type )
        {
			status = LocalBattleStatus.End;
			frameIndex = 0;
        }
        
		private void BattlePause()
		{
			status = LocalBattleStatus.Pause;
		}

		private void BattleContinue()
		{
			status = LocalBattleStatus.Normaly;
		}
    }
}
