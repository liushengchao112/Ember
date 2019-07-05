using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Utils;
using Network;
using Data;
using BattleAgent;

namespace UI
{
    public class BattleResultController : ControllerBase
    {
        public enum BattleResultType
        {
            Win,
            Lose,
        }

        private BattleResultView view;

        private bool OpenMainMenuUI;
        private BattleResultType currentResultType;
        private bool redVictroy;
        private BattleResultData resultInfo;
        private MatchSide myselfMatchSide;

        public BattleType currentBattleType;

        private List<BattleInfo> mainUIBattleInfoList = new List<BattleInfo>();
        private List<BattleInfo> mainUIBattleFriendInfoList = new List<BattleInfo>();
        private List<BattleInfo> mainUIBattleEnemyInfoList = new List<BattleInfo>();
        private BattleInfo mainUIBattleSelfInfo;
        private bool isSelfSideWin = false;

        public void SetBattleInfo( List<BattleInfo> infoList )
        {
            mainUIBattleInfoList.Clear();
            mainUIBattleFriendInfoList.Clear();
            mainUIBattleEnemyInfoList.Clear();
            for ( int i = 0; i < infoList.Count; i++ )
            {
                mainUIBattleInfoList.Add( infoList[i] );
            }

            if ( mainUIBattleInfoList.Count > 0 )
            {
                FixMainUIBattleInfo();
            }
        }

        private void FixMainUIBattleInfo()
        {
            long playerId = DataManager.GetInstance().GetPlayerId();
            for ( int i = 0; i < mainUIBattleInfoList.Count; i++ )
            {
                if ( playerId == mainUIBattleInfoList[i].playerId )
                {
                    mainUIBattleSelfInfo = mainUIBattleInfoList[i];
                    break;
                }
            }

            for ( int i = 0; i < mainUIBattleInfoList.Count; i++ )
            {
                if ( mainUIBattleSelfInfo.matchSide == mainUIBattleInfoList[i].matchSide )
                {
                    mainUIBattleFriendInfoList.Add( mainUIBattleInfoList[i] );
                }
                else
                {
                    mainUIBattleEnemyInfoList.Add( mainUIBattleInfoList[i] );
                }
            }

            if ( mainUIBattleSelfInfo.matchSide == mainUIBattleSelfInfo.winners )
            {
                isSelfSideWin = true;
            }
            else
            {
                isSelfSideWin = false;
            }

            view.RefreshBattleResult();
        }

        public bool IsSelfSideWin()
        {
            return isSelfSideWin;
        }

        public List<BattleInfo> GetMainUIBattleWinDataList()
        {
            if ( isSelfSideWin )
            {
                return mainUIBattleFriendInfoList;
            }
            else
            {
                return mainUIBattleEnemyInfoList;
            }
        }

        public List<BattleInfo> GetMainUIBattleLoseDataList()
        {
            if ( !isSelfSideWin )
            {
                return mainUIBattleFriendInfoList;
            }
            else
            {
                return mainUIBattleEnemyInfoList;
            }
        }

        public int GetMainUIBattleBlueKillNum( bool isBlue )
        {
            if ( isBlue )
            {
                return mainUIBattleSelfInfo.blueKillQuantity;
            }
            else
            {
                return mainUIBattleSelfInfo.redKillQuantity;
            }            
        }

        public string GetMainUIBattleTimeString()
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime( new DateTime( 1970 , 1 , 1 ) );
            long iTime = long.Parse( mainUIBattleSelfInfo.battleStartDate + "0000" );
            TimeSpan toNow = new TimeSpan( iTime );
            DateTime dTime = dtStart.Add( toNow );
            return dTime.ToString( "yyyy/MM/dd \t\t HH:mm:ss" );
        }

        public string GetMainUIBattleDuration()
        {
            int time = (int)( mainUIBattleSelfInfo.battleDuration * 0.001f );

            int hours = ( time / 60 ) / 60;
            int mins = ( time / 60 ) % 60;
            int secs = time % 60;

            if ( hours == 0 )
                return string.Format( "{0:00}:{1:00}" , mins , secs );
            return string.Format( "{0}:{1:00}:{2:00}" , hours , mins , secs );
        }

        public bool GetMainUIIsMvpWin( int index )
        {
            float socre = GetMainUIBattleWinDataList()[index].battleScore;
            for ( int i = 0; i < GetMainUIBattleWinDataList().Count; i++ )
            {
                if ( socre < GetMainUIBattleWinDataList()[i].battleScore )
                {
                    return false;
                }
            }
            return true;
        }

        public bool GetMainUIIsMvpLose( int index )
        {
            float socre = GetMainUIBattleLoseDataList()[index].battleScore;
            for ( int i = 0; i < GetMainUIBattleLoseDataList().Count; i++ )
            {
                if ( socre < GetMainUIBattleLoseDataList()[i].battleScore )
                {
                    return false;
                }
            }
            return true;
        }

        public BattleResultController( BattleResultView v )
        {
            viewBase = v;
            view = v;

            myselfMatchSide = DataManager.GetInstance().GetMatchSide();
            currentBattleType = DataManager.GetInstance().GetBattleType();
        }

        public override void OnResume()
        {
            base.OnResume();
            NetworkManager.RegisterServerMessageHandler( ServerType.BattleServer, MsgCode.PlayBackMessage, RecievePlayBackData );
        }

        public override void OnPause()
        {
            base.OnPause();
            NetworkManager.RemoveServerMessageHandler( ServerType.BattleServer, MsgCode.PlayBackMessage, RecievePlayBackData );
        }

        #region DataUI

        public void EnterMainMenu()
        {
            OpenMainMenuUI = true;
            ConnectGameServer();
        }

        public void EnterBattleMode()
        {
            OpenMainMenuUI = false;
            ConnectGameServer();
        }

        public string GetBattleTypeString()
        {
            string battleType = "";

            switch ( currentBattleType )
            {
                case BattleType.BattleP1vsP1:
                    battleType = "1V1模式";
                    break;
                case BattleType.BattleP2vsP2:
                    battleType = "2V2模式";
                    break;
                case BattleType.Survival:
                    battleType = "生存模式";
                    break;
                case BattleType.Tranining:
                    battleType = "训练模式";
                    break;
            }
            return battleType;
        }

        public string GetBattleResultType()
        {
            switch ( currentResultType )
            {
                case BattleResultType.Win:
                    return "胜利";
                case BattleResultType.Lose:
                    return "失败";
            }
            return "";
        }

        public int GetCurrentExp()
        {
            return resultInfo.currentExp;
        }

        public int GetGainExp()
        {
            return resultInfo.gainExp;
        }

        public int GetUpLevelExt()
        {
            return resultInfo.upLevelExp;
        }

        public float GetExpBarValue()
        {
            return resultInfo.currentExp / (float)resultInfo.upLevelExp;
        }

        public int GetPlayerLevel()
        {
            return resultInfo.playerLevel;
        }

        public int GetGainGold()
        {
            return resultInfo.gainGold;
        }

        public int GetWaveCount()
        {
            return DataManager.GetInstance().GetPveWaveNumber();
        }

        public int GetBlueSideKillCount()
        {
            return DataManager.GetInstance().GetPlayerKillCount( ForceMark.BottomBlueForce ) + DataManager.GetInstance().GetPlayerKillCount( ForceMark.TopBlueForce );
        }

        public int GetRedSideKillCount()
        {
            return DataManager.GetInstance().GetPlayerKillCount( ForceMark.BottomRedForce ) + DataManager.GetInstance().GetPlayerKillCount( ForceMark.TopRedForce );
        }

        public string GetBattleDuration()
        {
            int time = resultInfo.battleDuration;

            int hours = ( time / 60 ) / 60;
            int mins = ( time / 60 ) % 60;
            int secs = time % 60;

            if ( hours == 0 )
                return string.Format( "{0:00}:{1:00}", mins, secs );
            return string.Format( "{0}:{1:00}:{2:00}", hours, mins, secs );
        }

        public string GetBattleDate()
        {
            int year = System.DateTime.Now.Year;
            int mouth = System.DateTime.Now.Month;
            int day = System.DateTime.Now.Day;

            return string.Format( "{0}/{1:00}/{2:00}", year, mouth, day );
        }

        //Victory Data
        public List<BattleSituation> GetVictoryBattleSituationsList()
        {
            if ( redVictroy )
                return resultInfo.redBattleSituations;

            return resultInfo.blueBattleSituations;

        }

        public int GetVictoryCount()
        {
            return GetVictoryBattleSituationsList().Count;
        }

        public int GetVictoryKillCount( int index )
        {
            return GetVictoryBattleSituationsList()[index].kills;
        }

        public int GetVictoryKilledCount( int index )
        {
            return GetVictoryBattleSituationsList()[index].fatality;
        }

        public float GetVictoryMvpValue( int index )
        {
            return GetVictoryBattleSituationsList()[index].mvpValue;
        }

        public bool IsMVP_Victory( int index )
        {
            if ( currentBattleType == BattleType.BattleP2vsP2 )
            {
                float max = GetVictoryMvpValue( index );

                if ( max == 0 )
                    return false;

                for ( int i = 0; i < GetVictoryCount(); i++ )
                {
                    if ( GetVictoryMvpValue( i ) > max )
                        return false;
                }
                return true;
            }
            return false;
        }

        public string GetVictoryName( int index )
        {
            List<BattleSituation> list = GetVictoryBattleSituationsList();
            Matcher data = DataManager.GetInstance().GetMatcherDataByID( list[index].playerId );
            return data == null ? "" : data.name;
        }

        public int GetVictoryResources( int index )
        {
            return GetVictoryBattleSituationsList()[index].resources;
        }

        public string GetVictoryIcon( int index )
        {
            List<BattleSituation> list = GetVictoryBattleSituationsList();
            Matcher data = DataManager.GetInstance().GetMatcherDataByID( list[index].playerId );
            return data == null ? "" : data.portrait;
        }

        public MatchSide GetVictorySide(int index)
        {
            List<BattleSituation> list = GetVictoryBattleSituationsList();
            Matcher data = DataManager.GetInstance().GetMatcherDataByID( list[index].playerId );
            return data == null ? MatchSide.Red : data.side;
        }

        //Fail Data
        public List<BattleSituation> GetFailBattleSituationsList()
        {
            if ( redVictroy )
                return resultInfo.blueBattleSituations;

            return resultInfo.redBattleSituations;
        }

        public int GetFailCount()
        {
            return GetFailBattleSituationsList().Count;
        }

        public int GetFailKillCount( int index )
        {
            return GetFailBattleSituationsList()[index].kills;
        }

        public int GetFailKilledCount( int index )
        {
            return GetFailBattleSituationsList()[index].fatality;
        }

        public float GetFailMvpValue( int index )
        {
            return GetFailBattleSituationsList()[index].mvpValue;
        }

        public bool IsMVP_Fail( int index )
        {
            if ( currentBattleType == BattleType.BattleP2vsP2 )
            {
                float max = GetFailMvpValue( index );

                if ( max == 0 )
                    return false;

                for ( int i = 0; i < GetFailCount(); i++ )
                {
                    if ( GetFailMvpValue( i ) > max )
                        return false;
                }
                return true;
            }
            return false;
        }

        public string GetFailName( int index )
        {
            List<BattleSituation> list = GetFailBattleSituationsList();
            Matcher data = DataManager.GetInstance().GetMatcherDataByID( list[index].playerId );
            return data == null ? "" : data.name;
        }

        public int GetFailResources( int index )
        {
            return GetFailBattleSituationsList()[index].resources;
        }

        public string GetFailIcon( int index )
        {
            List<BattleSituation> list = GetFailBattleSituationsList();
            Matcher data = DataManager.GetInstance().GetMatcherDataByID( list[index].playerId );
            return data == null ? "" : data.portrait;
        }

        public MatchSide GetFailSide( int index )
        {
            List<BattleSituation> list = GetFailBattleSituationsList();
            Matcher data = DataManager.GetInstance().GetMatcherDataByID( list[index].playerId );
            return data == null ? MatchSide.Red : data.side;
        }

        public void RequsetPlayBackData()
        {
            long battleId = DataManager.GetInstance().GetBattleId();
            DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "Request playback data battleId = {0}", battleId ) );

            //PlaybackC2S message = new PlaybackC2S();
            //message.battleId = battleId;
            //byte[] data = ProtobufUtils.Serialize( message );
            //NetworkManager.SendRequest( ServerType.BattleServer, MsgCode.PlayBackMessage, data );
            PlayBackManager.GetInstance().DownloadPlaybackData( battleId );
        }

        private void RecievePlayBackData( byte[] data )
        {
            // save request playback data code -- Woody
            //DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "Recieve playback data, bytes length = {0}", data.Length ) );

            //PlayBackManager.GetInstance().DownloadPlaybackData( DataManager.GetInstance().GetBattleId() );
        }
        #endregion

        #region Connect Game Server

        private void ConnectGameServer()
        {
            if ( currentBattleType == BattleType.Survival || currentBattleType == BattleType.Tranining )
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene( "MainMenu" );

                if ( OpenMainMenuUI )
                    UIManager.locateState = UIManagerLocateState.MainMenu;
                else
                    UIManager.locateState = UIManagerLocateState.BattleModeView;

                DataManager clientData = DataManager.GetInstance();
                clientData.ResetBattleData();
                clientData.SetPlayerIsInBattle( false );
                return;
            }
                
            NetworkManager.Shutdown( () => MatchGameServer( DataManager.GetInstance().GetGameServerIp(), DataManager.GetInstance().GetGameServerPort() ) );
        }

        private void MatchGameServer( string ip, int port )
        {
            NetworkManager.Connect( ServerType.GameServer, ip, port, OnConnectGameServer, OnConnectGameServerFailed );
        }

        private void OnConnectGameServer( ClientType clientType )
        {
            DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Success! Connected to GameServer " + clientType );

            UnityEngine.SceneManagement.SceneManager.LoadScene( "MainMenu" );

            if ( OpenMainMenuUI )
                UIManager.locateState = UIManagerLocateState.MainMenu;
            else
                UIManager.locateState = UIManagerLocateState.BattleModeView;

            DataManager clientData = DataManager.GetInstance();
            clientData.ResetBattleData();
            clientData.SetPlayerIsInBattle( false );

            HeartBeat.RegisterGameHeartMessageHandler();
            //DataManager.GetInstance().RegisterDataServerMessageHandler();
            MultiDeviceListenerManager.RegisterHandler();
            DebugToScreen.RegisterHandler();
        }

        private void OnConnectGameServerFailed( ClientType clientType )
        {
            string connectGameServerFailed = "连接GameServer失败，请重试" + clientType;
            string titleText = "提示";
            System.Action reconnect = ConnectGameServer;

            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, reconnect, UI.AlertType.ConfirmAlone, connectGameServerFailed, titleText );
        }

        #endregion

        public void SetBattleResultData( NoticeType resType, BattleResultData resInfo, bool isPlayBack = false )
        {
            resultInfo = resInfo;

            #region Judge winning or losing

            if ( resType == NoticeType.BattleResultBlueWin )
            {
                switch ( myselfMatchSide )
                {
                    case MatchSide.Red:
                        currentResultType = BattleResultType.Lose;
                        break;
                    case MatchSide.Blue:
                        currentResultType = BattleResultType.Win;
                        break;
                }
                redVictroy = false;
            }
            else if ( resType == NoticeType.BattleResultRedWin )
            {
                switch ( myselfMatchSide )
                {
                    case MatchSide.Red:
                        currentResultType = BattleResultType.Win;
                        break;
                    case MatchSide.Blue:
                        currentResultType = BattleResultType.Lose;
                        break;
                }
                redVictroy = true;
            }

            #endregion

            view.RefreshBattleResult();

            if ( !isPlayBack )
            {
                DataManager dataM = DataManager.GetInstance();

                dataM.SetPlayerExp( resInfo.currentExp );
                int currentGold = dataM.GetPlayerGold();
                dataM.SetPlayerGold( currentGold += resInfo.gainGold );
                dataM.SetPlayerLevel( resInfo.playerLevel );
            }            
        }
    }

    //Combat result structure data
    public struct BattleResultData
    {
        public int gainGold;       //获得金币
        public int gainExp;        //获得经验
        public int currentExp;     //当前玩家经验
        public int upLevelExp;     //升级所需经验
        public int playerLevel;    //玩家当前等级

        public List<BattleSituation> redBattleSituations;
        public List<BattleSituation> blueBattleSituations;

        public int battleDuration;  //战斗时间
    }
}
