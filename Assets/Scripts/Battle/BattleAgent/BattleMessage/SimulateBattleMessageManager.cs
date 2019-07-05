using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using Data;
using Constants;
using Utils;

namespace BattleAgent
{
    public class SimulateBattleMessageManager : MonoBehaviour
    {
        private class BattleSituation
        {
            public int fatality;
            public int kills;
            public float mvpValue;
            public long playerId;
            public int resources;
        }

        private enum SimulateBattleStatus
        {
            None,
            Playing,
            Pause,
            End
        }

        class SimulateMessage
        {
            public MsgCode msgCode;
            public byte[] data;
            public Action onRequestSuccess;
        }

        private int frameIndex = 0;
        private List<Frame> frames;
        private long battleId = 0;
        private float logicTimer = 0;
        private int battleDurationTimer = 0;
        private static float deltaTime = 0;
        private static int deltaTimeMs = 0;
        private static Dictionary<MsgCode, Action<byte[]>> simMessageHandlers = new Dictionary<MsgCode, Action<byte[]>>();
        private static List<SimulateMessage> messageList = new List<SimulateMessage>();
        private static SimulateBattleStatus status = SimulateBattleStatus.Playing;

        // data
        private NoticeType battleResult;
        private Dictionary<MatchSide, List<BattleSituation>> sideBattleSituations;

        public void Initialize()
        {
            DataManager clientData = DataManager.GetInstance();
            frames = clientData.GetSimulateBattleData();
            battleId = clientData.GetBattleId();

            // Init battle situation data.
            deltaTime = GameConstants.LOGIC_FRAME_TIME;
            deltaTimeMs = Mathf.RoundToInt( deltaTime * 1000 );
            List<Battler> battler = clientData.GetBattlers();

            sideBattleSituations = new Dictionary<MatchSide, List<BattleSituation>>();
            sideBattleSituations.Add( MatchSide.Blue, new List<BattleSituation>() );
            sideBattleSituations.Add( MatchSide.Red, new List<BattleSituation>() );

            for ( int i = 0; i < battler.Count; i++ )
            {
                BattleSituation bs = new BattleSituation();
                long battlerId = battler[i].playerId;

                bs.playerId = battlerId;

                if ( battler[i].side == MatchSide.Blue )
                {
                    sideBattleSituations[MatchSide.Blue].Add( bs );
                }
                else if ( battler[i].side == MatchSide.Red )
                {
                    sideBattleSituations[MatchSide.Red].Add( bs );
                }
            }

            status = SimulateBattleStatus.Playing;
        }

        void Update()
        {
            if ( status == SimulateBattleStatus.Playing )
            {
                logicTimer += Time.deltaTime;

                if ( logicTimer + Time.deltaTime * 0.5f > deltaTime )
                {
                    if ( frames.Count > frameIndex )
                    {
                        BroadCastFeedBack( frames[frameIndex] );
                        logicTimer = 0;
                        frameIndex++;
                        battleDurationTimer += deltaTimeMs;
                    }
                }
            }
            else if ( status == SimulateBattleStatus.End )
            {
                Reset();
            }
        }

        private void BroadCastFeedBack( Frame frames )
        {
            byte[] dataS2C = null;

            // Send UpdateS2C
            UpdateS2C updateS2C = new UpdateS2C();
            updateS2C.battleId = battleId;
            updateS2C.timestamp = frames.frame;

            for ( int j = 0; j < frames.operations.Count; j++ )
            {
                updateS2C.ops.Add( frames.operations[j] );
            }

            dataS2C = ProtobufUtils.Serialize( updateS2C );

            for ( int i = 0; i < updateS2C.ops.Count; i++ )
            {
                DebugUtils.Log( DebugUtils.Type.SimulateBattleMessage, string.Format( "Frame: Send feed back operation {0}", updateS2C.ops[i].opType ) );
            }

            simMessageHandlers[MsgCode.UpdateMessage]( dataS2C );

            for ( int i = messageList.Count - 1; i >= 0; i-- )
            {
                if ( messageList[i].msgCode == MsgCode.NoticeMessage )
                {
                    // Send NoticeMessage
                    NoticeC2S noticeC2S = ProtobufUtils.Deserialize<NoticeC2S>( messageList[i].data );

                    SetCurrentBattleResult( noticeC2S );
                }
                else if ( messageList[i].msgCode == MsgCode.QuitBattleMessage )
                {
                    // Send QuitBattleMessage
                    DebugUtils.Log( DebugUtils.Type.SimulateBattleMessage, string.Format( "Send {0} feed back succeed ", messageList[i].msgCode ) );

                    SetCurrentBattleResult();
                    return;
                }
                else if ( messageList[i].msgCode == MsgCode.UploadSituationMessage )
                {
                    // Send UploadSituationMessage
                    UploadSituationC2S uploadSituationC2S = ProtobufUtils.Deserialize<UploadSituationC2S>( messageList[i].data );

                    int type = uploadSituationC2S.type;
                    if ( type == 1 )
                    {
                        // send data
                        long id = uploadSituationC2S.battleSituation.playerId;
                        List<Battler> battlers = DataManager.GetInstance().GetBattlers();
                        Battler battler = battlers.Find( p => p.playerId == id );

                        if ( battler != null )
                        {
                            BattleSituation situantion = sideBattleSituations[battler.side].Find( p => p.playerId == battler.playerId );
                            ConvertSituantionData( situantion, uploadSituationC2S.battleSituation );
                        }
                        else
                        {
                            DebugUtils.LogError( DebugUtils.Type.SimulateBattleMessage, string.Format( "Can't find battler {0} in playback {1}", id, DataManager.GetInstance().GetBattleId() ) );
                        }
                    }
                    else if ( type == 2 )
                    {
                        // request data
                        UploadSituationS2C uploadSituationS2C = new UploadSituationS2C();
                        uploadSituationS2C.type = uploadSituationC2S.type;

                        FillSituantionData( MatchSide.Red, uploadSituationS2C.redBattleSituations );
                        FillSituantionData( MatchSide.Blue, uploadSituationS2C.blueBattleSituations );

                        dataS2C = ProtobufUtils.Serialize( uploadSituationS2C );

                        DebugUtils.Log( DebugUtils.Type.SimulateBattleMessage, string.Format( "Send {0} feed back succeed ", messageList[i].msgCode ) );

                        Action<byte[]> callback = null;
                        if ( simMessageHandlers.TryGetValue( MsgCode.UploadSituationMessage, out callback ) )
                        {
                            callback.Invoke( dataS2C );
                        }
                    } 
                }
                else
                {
                    //DebugUtils.LogError( DebugUtils.Type.SimulateBattleMessage, string.Format( "Can't handle this local message now message code = {0} ", messageList[i].msgCode ) );
                }

                messageList.RemoveAt( i );
            }
        }

        public static void SendRequest( MsgCode protocolCode, byte[] data, Action onRequestSuccess = null )
        {
            if ( simMessageHandlers.ContainsKey( protocolCode ) && protocolCode != MsgCode.UpdateMessage && protocolCode != MsgCode.SyncMessage )
            {
                DebugUtils.Log( DebugUtils.Type.SimulateBattleMessage, string.Format( "Send local request {0} succeed ", protocolCode ) );
                SimulateMessage msg = new SimulateMessage();
                msg.data = data;
                msg.msgCode = protocolCode;
                msg.onRequestSuccess = onRequestSuccess;

                messageList.Add( msg );
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.SimulateBattleMessage, string.Format( " Can't handle request {0} ", protocolCode ) );
            }
        }

        public static void RegisterSimulateMessageHandler( MsgCode protocolCode, Action<byte[]> handler )
        {
            if ( !simMessageHandlers.ContainsKey( protocolCode ) )
            {
                simMessageHandlers.Add( protocolCode, handler );
            }
            else
            {
                if ( simMessageHandlers[protocolCode] != null )
                {
                    Delegate[] delegates = simMessageHandlers[protocolCode].GetInvocationList();
                    foreach ( Delegate del in delegates )
                    {
                        if ( del.Equals( handler ) )
                            return;
                    }
                }
                simMessageHandlers[protocolCode] += handler;
            }
        }

        public static void RemoveSimulateMessageHandler( MsgCode protocolCode, Action<byte[]> handler )
        {
            if ( simMessageHandlers.ContainsKey( protocolCode ) )
            {
                simMessageHandlers[protocolCode] -= handler;
                if ( simMessageHandlers[protocolCode] == null )
                    simMessageHandlers.Remove( protocolCode );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.SimulateBattleMessage, "Remove an unregistered handler for protocol " + (int)protocolCode );
            }
        }

        public static void SetBattleSpeed( float speed )
        {
            DebugUtils.Assert( speed > 0, "Why playback speed lower than 0?");
            Time.timeScale = speed;
            deltaTime = GameConstants.LOGIC_FRAME_TIME / speed;
        }

        private void FillSituantionData( MatchSide side, List<Data.BattleSituation> list )
        {
            List<BattleSituation> sideSituations = sideBattleSituations[side];
            for ( int index = 0; index < sideSituations.Count; index++ )
            {
                BattleSituation battleSituation = sideSituations[index];

                Data.BattleSituation situation = new Data.BattleSituation();
                situation.fatality = battleSituation.fatality;
                situation.kills = battleSituation.kills;
                situation.mvpValue = battleSituation.mvpValue;
                situation.playerId = battleSituation.playerId;
                situation.resources = battleSituation.resources;

                list.Add( situation );
            }
        }

        private void ConvertSituantionData( BattleSituation situation, Data.BattleSituation data )
        {
            situation.fatality = data.fatality;
            situation.kills = data.kills;
            situation.mvpValue = data.mvpValue;
            situation.playerId = data.playerId;
            situation.resources = data.resources;
        }

        private void Reset()
        {
            messageList.Clear();
            SetBattleSpeed( 1 );
        }

        private void SetCurrentBattleResult( NoticeC2S noticeC2S = null )
        {
            NoticeS2C noticeS2C = new NoticeS2C();
            NoticeType type = NoticeType.BattleResultBlueWin;

            if ( noticeC2S != null )
            {
                type = noticeC2S.type;
            }
            else
            {
                type = NoticeType.BattleResultBlueWin;
            }          

            DebugUtils.Log( DebugUtils.Type.SimulateBattleMessage, string.Format( "Send {0} feed back succeed ", MsgCode.NoticeMessage ) );

            // battle end need to stop send updateS2CS
            if ( type == NoticeType.BattleResultBlueWin ||
                type == NoticeType.BattleResultRedWin ||
                type == NoticeType.BattleResultDraw )
            {
                DataManager clientData = DataManager.GetInstance();

                NoticeS2C.BattleResult resultData = new NoticeS2C.BattleResult();

                resultData.battleDuration = (int)( battleDurationTimer * 0.001f );

                FillSituantionData( MatchSide.Red, resultData.redBattleSituations );
                FillSituantionData( MatchSide.Blue, resultData.blueBattleSituations );

                resultData.gainGold = 0;
                resultData.gainExp = 0;
                resultData.currentExp = 0;
                resultData.upLevelExp = 0;
                resultData.playerLevel = 0;

                noticeS2C.battleResult = resultData;
                status = SimulateBattleStatus.End;
                DebugUtils.Log( DebugUtils.Type.SimulateBattleMessage, string.Format( "Send battle result feed back succeed: {0}", type ) );
            }

            noticeS2C.type = type;
            byte[] dataS2C = ProtobufUtils.Serialize( noticeS2C );
            simMessageHandlers[MsgCode.NoticeMessage]( dataS2C );
        }

        public static void SetBattleRecordPlayState( int state )
        {
            switch ( state )
            {
                case 1:
                {
                    // play
                    status = SimulateBattleStatus.Playing;
                    DebugUtils.Log( DebugUtils.Type.SimulateBattleMessage, string.Format( "Set simulate battle state {0}", status ) );
                    break;
                }
                case 2:
                {
                    // pause or continue
                    if ( status == SimulateBattleStatus.Pause )
                    {
                        status = SimulateBattleStatus.Playing;
                    }
                    else
                    {
                        status = SimulateBattleStatus.Pause;
                    }
                    DebugUtils.Log( DebugUtils.Type.SimulateBattleMessage, string.Format( "Set simulate battle state {0}", status ) );

                    break;
                }
                case 3:
                {
                    // quit
                    DebugUtils.Log( DebugUtils.Type.SimulateBattleMessage, string.Format( "Set simulate battle state {0}", status ) );
                    break;
                }
            }
        }
    }
}
