using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Data;
using Utils;
using Network;
using UnityEngine.SceneManagement;

namespace UI
{
    public class FightMatchController : ControllerBase
    {
        public class MatcherData
        {
            public string name;
            public string icon;
            public bool isReady;
            public bool isMyself;
            public long playerId;

            public MatcherData( string name, string icon, bool isReady, bool isMyself, long playerId )
            {
                this.name = name;
                this.icon = icon;
                this.isReady = isReady;
                this.isMyself = isMyself;
                this.playerId = playerId;
            }
        }

        private FightMatchView view;
        private List<UnitsProto.Unit> unitsProto;

        private bool isTimeOut;
        private List<Matcher> allMatchers = new List<Matcher>();
        private List<InvitationFriendInfo> friendDatasList = new List<InvitationFriendInfo>();
        public BattleType currentBattleType;
        private int currentArmyIndex;
        private int currentRuneIndex;
        private int currentInstituteSkillIndex;
        private MatchFriendItem currentFriendItem;
        private List<MatcherReadyData> matcherReadyDatas;
        private long currentFriendId;
        public bool isInvite = false;

        public FightMatchController( FightMatchView v )
        {
            viewBase = v;
            view = v;

            OnCreate();
        }

        public override void OnCreate()
        {
            base.OnCreate();
            unitsProto = DataManager.GetInstance().unitsProtoData;
        }

        public override void OnResume()
        {
            base.OnResume();
            NetworkManager.RegisterServerMessageHandler( MsgCode.ApplyRoomMessage, HandleApplyRoomFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.ChangeBattleConfigMessage, HandleChangeBattleConfigFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.MatchReadyDataMessage, HandleMatchReadyDataFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.MatchReadyDataRefreshMessage, HandleMatchReadyDataRefreshFeedback );
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.InvitationListMessage, HandleInvationListFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.InvitationMatchMessage, HandleInvationMatchFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.InvitationNoticeMessage, HandleInvationNoticeFeedback );
        }

        public override void OnPause()
        {
            base.OnPause();
            if ( NetworkManager.GetCurrentServerType() == ServerType.GameServer )
            {
                NetworkManager.RemoveServerMessageHandler( MsgCode.ApplyRoomMessage, HandleApplyRoomFeedback );
                NetworkManager.RemoveServerMessageHandler( MsgCode.ChangeBattleConfigMessage, HandleChangeBattleConfigFeedback );
                NetworkManager.RemoveServerMessageHandler( MsgCode.MatchReadyDataMessage, HandleMatchReadyDataFeedback );
                NetworkManager.RemoveServerMessageHandler( MsgCode.MatchReadyDataRefreshMessage, HandleMatchReadyDataRefreshFeedback );
                NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.InvitationListMessage, HandleInvationListFeedback );
                NetworkManager.RemoveServerMessageHandler( MsgCode.InvitationMatchMessage, HandleInvationMatchFeedback );
                NetworkManager.RemoveServerMessageHandler( MsgCode.InvitationNoticeMessage, HandleInvationNoticeFeedback );
            }
        }

        public void SetMatcherDatas( List<MatcherReadyData> dataList )
        {
            matcherReadyDatas = dataList;
        }

        public void SetBattleType( BattleType type )
        {
            currentBattleType = type;
            DataManager dataManager = DataManager.GetInstance();
            dataManager.SetBattleType( type, false );
        }

        #region Data UI

        #region My Match Ready Data

        private MatcherReadyData GetMyMatcherReadyData()
        {
            return matcherReadyDatas.Find( p => p.playerId == DataManager.GetInstance().GetPlayerId() );
        }

        public string GetMyselfName()
        {
            return GetMyMatcherReadyData().name;
        }

        public string GetMyselfIcon()
        {
            return GetMyMatcherReadyData().portrait;
        }

        public int GetMyselfArmyIcon( int index )
        {
            int unitId = GetMyMatcherReadyData().unitIds[index];

            return unitsProto.Find( p => p.ID == unitId ).Icon_box;
        }

        private void MatchSucceedUIData( List<Matcher> matchers )
        {
            DataManager clientData = DataManager.GetInstance();
            allMatchers = matchers;
            clientData.SetMatchers( allMatchers );

            long id = clientData.GetPlayerAccountId();
            MatchSide myselfSide = matchers.Find( p => p.playerId == id ).side;

            bool myselfReady = matchers.Find( p => p.playerId == id ).ready;
            view.intoGameButton.interactable = !myselfReady;

            List<MatcherData> friendData = new List<UI.FightMatchController.MatcherData>();
            List<MatcherData> enemyData = new List<UI.FightMatchController.MatcherData>();

            List<Matcher> friends = matchers.FindAll( p => p.side == myselfSide );
            List<Matcher> enemys = matchers.FindAll( p => p.side != myselfSide );

            long myId = DataManager.GetInstance().GetPlayerId();
            foreach ( Matcher friend in friends )
            {
                MatcherData data = new MatcherData( friend.name, friend.portrait, friend.ready, friend.playerId == myId, friend.playerId );
                friendData.Add( data );
            }

            foreach ( Matcher enemy in enemys )
            {
                MatcherData data = new MatcherData( enemy.name, enemy.portrait, enemy.ready, false, enemy.playerId );
                enemyData.Add( data );
            }

            view.friendDatas = friendData;
            view.enemyDatas = enemyData;

            view.InitSucceedItem();
        }

        private void MatcherCancelledUIData( List<Matcher> matchers )
        {
            List<MatchSucceedItem> cancellItem = new List<MatchSucceedItem>();

            List<MatchSucceedItem> items = new List<MatchSucceedItem>();
            items.AddRange( view.mySideSucceedItems );
            items.AddRange( view.enemySideSucceedItems );

            foreach ( MatchSucceedItem item in items )
            {
                if ( matchers.Find( p => p.name == item.name ) == null )
                    cancellItem.Add( item );
            }

            //foreach ( MatchSucceedItem item in cancellItem )
            //{
            //    item.SetItemCancell();
            //}

            view.SetMatcherCancelled();
        }

        public List<string> GetRuneDropdownTextList()
        {
            List<string> textList = new List<string>();

            List<RunePageInfo> infoList = DataManager.GetInstance().GetRunePageList();

            for ( int i = 0; i < infoList.Count; i++ )
            {
                string name = infoList[i].runePageName;
                if ( name == "0" )
                    name = "灵石页" + ( i + 1 );

                textList.Add( name );
            }

            return textList;
        }

        public List<string> GetInstituteSkillDropdownTextList()
        {
            List<string> textList = new List<string>() { };

            DataManager dataManager = DataManager.GetInstance();

            for ( int i = 0; i < 3; i++ )
            {
                if ( dataManager.GetPlayerSetedPackageInstituteSkills( i ).Count > 0 )
                    textList.Add( "技能页" + ( i + 1 ) );
            }

            return textList;
        }

        public int GetRuneTotalLevel()
        {
            return GetMyMatcherReadyData().runePageTotalLevel;
        }

        public int GetCurrentArmyIndex( BattleType type )
        {
            return GetMyMatcherReadyData().armyPage;
        }

        public int GetCurrentRuneIndex( BattleType type )
        {
            return GetMyMatcherReadyData().runePage;
        }

        public int GetCurrentInstituteSkillIndex( BattleType type )
        {
            return GetMyMatcherReadyData().sciencePage;
        }

        #endregion

        #region Friend Match Ready Data

        private MatcherReadyData GetFriendMatcherReadyData()
        {
            if ( matcherReadyDatas.Count <= 1 )
                return null;
            return matcherReadyDatas.FindAll( p => p.playerId != DataManager.GetInstance().GetPlayerId() )[0];
        }

        public int GetFriendArmyIcon( int index )
        {
            if ( GetFriendMatcherReadyData() == null )
                return 0;

            int unitId = GetFriendMatcherReadyData().unitIds[index];

            return unitsProto.Find( p => p.ID == unitId ).Icon_box;
        }

        public string GetFriendName()
        {
            if ( GetFriendMatcherReadyData() == null )
                return "";

            return GetFriendMatcherReadyData().name;
        }

        public string GetFriendIcon()
        {
            if ( GetFriendMatcherReadyData() == null )
                return "";

            return GetFriendMatcherReadyData().portrait;
        }

		public List<long> GetFriendsID()
		{
			if ( matcherReadyDatas ==null || matcherReadyDatas.Count <= 1 )
				return null;

			List<MatcherReadyData>datas = matcherReadyDatas.FindAll( p => p.playerId != DataManager.GetInstance().GetPlayerId() );

			List<long> ids = new List<long>();

			for( int i = 0; i < datas.Count; i++ )
			{
				ids.Add( datas[i].playerId );
			}

			return ids;
		}

        #endregion

        #region Friend List Data

        public List<InvitationFriendInfo> GetFriendDataList()
        {
            return friendDatasList;
        }

        #endregion

        #endregion

        #region Send

        private MatchType GetCurrentMatchType()
        {
            BattleType bType = currentBattleType;
            if ( bType == BattleType.BattleP1vsP1 )
                return MatchType.P1vsP1;
            if ( bType == BattleType.BattleP2vsP2 )
                return MatchType.P2vsP2;
            return MatchType.Peace;
        }

        public void SendApplyRoomC2S()
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

            ApplyRoomC2S applyRoomData = new ApplyRoomC2S();

            applyRoomData.matchType = GetCurrentMatchType();

            byte[] data = ProtobufUtils.Serialize( applyRoomData );
            NetworkManager.SendRequest( MsgCode.ApplyRoomMessage, data );
        }

        private void SendApplyingMatchC2S()
        {
            SendMatchC2S( MatchClientMessageType.Applying, currentFriendId );
        }

        public void SendCancelingMatch()
        {
            SendMatchC2S( MatchClientMessageType.Canceling );
        }

        public void SendReadyMatch()
        {
            SendMatchC2S( MatchClientMessageType.PlayerReady );
        }

        private void SendMatchC2S( MatchClientMessageType type, long friendId = 0 )
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

            MatchC2S message = new MatchC2S();

            message.matchType = GetCurrentMatchType();
            message.playerName = DataManager.GetInstance().GetPlayerNickName();
            message.friendId = friendId;
            message.type = type;

            byte[] data = ProtobufUtils.Serialize( message );

            NetworkManager.SendRequest( MsgCode.MatchMessage, data );
        }

        public void SendChangeArmyC2S( int armyIndex, BattleType type )
        {
            currentBattleType = type;
            currentArmyIndex = armyIndex;

            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

            ChangeBattleConfigC2S message = new ChangeBattleConfigC2S();

            message.config = new BattleTypeConfigInfo.BattleTypeConfigParamInfo();
            message.config.battleType = type;
            message.configType = 1;// 1-army 2-rune 3-instituteSkill
            message.matchWithFriend = ( matcherReadyDatas.Count > 1 );
            message.config.labelPage = armyIndex;

            byte[] data = ProtobufUtils.Serialize( message );

            NetworkManager.SendRequest( MsgCode.ChangeBattleConfigMessage, data );
        }

        public void SendChangeRuneC2S( int runeIndex, BattleType type )
        {
            currentBattleType = type;
            currentRuneIndex = runeIndex;

            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

            ChangeBattleConfigC2S message = new ChangeBattleConfigC2S();

            message.config = new BattleTypeConfigInfo.BattleTypeConfigParamInfo();
            message.config.battleType = type;
            message.configType = 2;// 1-army 2-rune 3-instituteSkill
            message.matchWithFriend = ( matcherReadyDatas.Count > 1 );
            message.config.labelPage = runeIndex;

            byte[] data = ProtobufUtils.Serialize( message );

            NetworkManager.SendRequest( MsgCode.ChangeBattleConfigMessage, data );
        }

        public void SendChangeInstituteSkillC2S( int instituteSkillIndex, BattleType type )
        {
            currentBattleType = type;
            currentInstituteSkillIndex = instituteSkillIndex;

            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

            ChangeBattleConfigC2S message = new ChangeBattleConfigC2S();

            message.config = new BattleTypeConfigInfo.BattleTypeConfigParamInfo();
            message.config.battleType = type;
            message.configType = 3;// 1-army 2-rune 3-instituteSkill
            message.matchWithFriend = ( matcherReadyDatas.Count > 1 );
            message.config.labelPage = instituteSkillIndex;

            byte[] data = ProtobufUtils.Serialize( message );

            NetworkManager.SendRequest( MsgCode.ChangeBattleConfigMessage, data );
        }

        public void SendNoticeC2S( int loadingRate )
        {
            //NoticeC2S message = new NoticeC2S();

            //message.type = NoticeType.BattleLoding;
            //message.lodingRate = loadingRate;

            //byte[] data = ProtobufUtils.Serialize( message );

            //NetworkManager.SendRequest( MsgCode.NoticeMessage, data );
        }

        public void SendMatchReadyDataC2S( BattleType type )
        {
            MatchReadyDataC2S message = new MatchReadyDataC2S();

            message.battleType = type;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.MatchReadyDataMessage, data );
        }

        public void SendInvitationListC2S()
        {
            InvitationListC2S message = new InvitationListC2S();

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.InvitationListMessage, data );
        }

        public void SendInvitationFriend( MatchFriendItem item )
        {
            if ( currentFriendItem != null )
                return;
            if ( currentBattleType != BattleType.BattleP2vsP2 )
                return;
            if ( matcherReadyDatas.Count > 1 )
                return;
            currentFriendItem = item;
            SendInvitationC2S( item.info.playerId, currentBattleType, InvitationState.SendInvitation );
        }

        public void SendCancelInvitation()
        {
            if ( currentFriendItem == null && !view.beInvited )
            {
                currentFriendId = 0;
                view.OnExit( true );
                return;
            }

            long playerId;

            if ( view.beInvited )
                playerId = matcherReadyDatas.FindAll( p => p.playerId != DataManager.GetInstance().GetPlayerId() )[0].playerId;
            else
                playerId = currentFriendItem.info.playerId;

            SendInvitationC2S( playerId, currentBattleType, InvitationState.CancelInvitation );
            currentFriendItem = null;
            currentFriendId = 0;
        }

        public void SendReady()
        {
            long friendId = matcherReadyDatas.Find( p => p.playerId != DataManager.GetInstance().GetPlayerId() ).playerId;

            SendInvitationC2S( friendId, currentBattleType, InvitationState.FriendReady );
        }

        public void SendCancelReady()
        {
            long friendId = matcherReadyDatas.Find( p => p.playerId != DataManager.GetInstance().GetPlayerId() ).playerId;

            SendInvitationC2S( friendId, currentBattleType, InvitationState.FriendCancelReady );
        }

        public void SendStartMatch()
        {
            if ( matcherReadyDatas.Count <= 1 )
            {
                SendApplyRoomC2S();
                return;
            }
            long friendId = matcherReadyDatas.Find( p => p.playerId != DataManager.GetInstance().GetPlayerId() ).playerId;

            SendInvitationC2S( friendId, currentBattleType, InvitationState.StartMatch );
        }

        private void SendInvitationC2S( long friendId, BattleType type, InvitationState state )
        {
            InvitationMatchC2S message = new InvitationMatchC2S();

            message.friendId = friendId;
            message.battleType = type;
            message.state = state;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.InvitationMatchMessage, data );
        }

        #endregion

        #region Reponse Handle

        private void HandleMatchFeedback( byte[] data )
        {
            UILockManager.ResetGroupState( UIEventGroup.Middle );

            MatchS2C feedback = ProtobufUtils.Deserialize<MatchS2C>( data );

            switch ( feedback.result )
            {
                case MatchServerMessageType.RoomCreatedSucceed:
                    DebugUtils.Log( DebugUtils.Type.Match, "Room Created Succeed~" );
                    //view.timeLeft_room = feedback.matchTime / 1000;

                    view.OpenMatchingUI();
                    break;
                case MatchServerMessageType.RoomCreatedFail:
                    DebugUtils.Log( DebugUtils.Type.Match, "Match Room Created Fail~" );
                    break;
                case MatchServerMessageType.RoomFound:
                    DebugUtils.Log( DebugUtils.Type.Match, "Room Found~" );

                    break;
                case MatchServerMessageType.RoomReady:
                    DebugUtils.Log( DebugUtils.Type.Match, "Room Ready~" );
                    view.OpenMatchSucceedUI();
                    view.timeLeft_matchSucceed = feedback.matchTime / 1000;
                    MatchSucceedUIData( feedback.matchers );
                    break;
                case MatchServerMessageType.Cancelled:
                case MatchServerMessageType.MatchingTimeout:
                    DebugUtils.Log( DebugUtils.Type.Match, feedback.result.ToString() + "~" );

                    isTimeOut = false;
                    ConnectGameServer( feedback );
                    matcherReadyDatas.Remove( matcherReadyDatas.Find( p => p.playerId != DataManager.GetInstance().GetPlayerId() ) );
                    view.SetFriendUI( false );
                    view.beInvited = false;
                    currentFriendItem = null;
                    currentFriendId = 0;
                    view.OpenMatchUI();
                    view.SetStartMatchState( false );
                    break;

                case MatchServerMessageType.CancelFailed:
                    DebugUtils.Log( DebugUtils.Type.Match, "Match Cancel Failed~" );
                    break;

                case MatchServerMessageType.MatcherFound:
                    DebugUtils.Log( DebugUtils.Type.Match, "Matcher Found~" );

                    break;

                case MatchServerMessageType.MatcherCancelled:
                    DebugUtils.Log( DebugUtils.Type.Match, "Matcher Cancelled~" );
                    MatcherCancelledUIData( feedback.matchers );
                    //view.timeLeft_room = feedback.matchTime / 1000;
                    //TODO: if the friend cancelled  ,  need do something

                    break;

                case MatchServerMessageType.MatcherReady:
                    DebugUtils.Log( DebugUtils.Type.Match, "Matcher Ready~" );
                    MatchSucceedUIData( feedback.matchers );
                    break;
                default:
                    DebugUtils.Log( DebugUtils.Type.Match, "Match result is wrong~" );
                    break;
            }
        }

        private void HandleBattleMessageFeedback( byte[] data )
        {
            UILockManager.ResetGroupState( UIEventGroup.Middle );

            BattleS2C feedback = ProtobufUtils.Deserialize<BattleS2C>( data );

            if ( feedback.result )
            {
                DataManager dataManager = DataManager.GetInstance();
                if ( feedback.battleId != 0 )
                {
                    // receive this message second time 
                    dataManager.SetBattleServerIp( feedback.serverIp );
                    dataManager.SetBattleServerPort( feedback.serverPort );
                    dataManager.SetBattleId( feedback.battleId );
                    dataManager.SetSeed( feedback.seed );

                    ConnectBattleServer();
                }
                else
                {
                    // receive this message first time 
                    dataManager.SetBattlers( feedback.battlers );
                }
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Match, " HandleBattleMessage result = false, maybe some error occured! " );
            }
        }

        private void HandleApplyRoomFeedback( byte[] data )
        {
            UILockManager.ResetGroupState( UIEventGroup.Middle );

            ApplyRoomS2C feedback = ProtobufUtils.Deserialize<ApplyRoomS2C>( data );

            if ( feedback.result )
            {
                NetworkManager.Shutdown( () => MatchLobbyServer( feedback.lobbyServerIp, feedback.lobbyServerPort ) );

                DataManager.GetInstance().SetLobbyServerIp( feedback.lobbyServerIp );
                DataManager.GetInstance().SetLobbyServerPort( feedback.lobbyServerPort );
            }
        }

        private void HandleChangeBattleConfigFeedback( byte[] data )
        {
            UILockManager.ResetGroupState( UIEventGroup.Middle );

            ChangeBattleConfigS2C feedback = ProtobufUtils.Deserialize<ChangeBattleConfigS2C>( data );

            if ( feedback.result )
            {
                DataManager dataManager = DataManager.GetInstance();

                switch ( feedback.configType )
                {
                    case 1://1-army 2-rune 3-instituteSkill
                        dataManager.SetBattleConfigArmyIndex( feedback.config.battleType, feedback.config.labelPage );
                        break;
                    case 2://1-army 2-rune 3-instituteSkill
                        dataManager.SetBattleConfigRuneIndex( feedback.config.battleType, feedback.config.labelPage );
                        break;
                    case 3://1-army 2-rune 3-instituteSkill
                        dataManager.SetBattleConfigInstituteSkillIndex( feedback.config.battleType, feedback.config.labelPage );
                        break;
                }
            }
        }

        private void HandleNoticeMessageFeedback( byte[] data )
        {
            UILockManager.ResetGroupState( UIEventGroup.Middle );

            NoticeS2C feedback = ProtobufUtils.Deserialize<NoticeS2C>( data );

            if ( feedback.type == NoticeType.BattleBegin )
            {
                view.currentUIType = FightMatchView.FightMatchUIType.None;
                view.OnExit( true );
            }
        }

        private void HandleMatchReadyDataFeedback( byte[] data )
        {
            MatchReadyDataS2C feedback = ProtobufUtils.Deserialize<MatchReadyDataS2C>( data );

            if ( feedback.result )
            {
                if ( matcherReadyDatas == null )
                {
                    matcherReadyDatas = feedback.matcherReadyDatas;
                    view.SetStartMatchBtState( true );
                }
                else
                {
                    long playerId = feedback.matcherReadyDatas[0].playerId;
                    if ( matcherReadyDatas.Find( p => p.playerId == playerId ) != null )
                        matcherReadyDatas.Remove( matcherReadyDatas.Find( p => p.playerId == playerId ) );
                    matcherReadyDatas.Add( feedback.matcherReadyDatas[0] );
                }

                if ( matcherReadyDatas.Count <= 1 )
                {
                    view.SetFriendUI( false );
                }
                view.OpenMatchUI();
            }
        }

        private void HandleMatchReadyDataRefreshFeedback( byte[] data )
        {
            MatchReadyDataRefreshS2C feedback = ProtobufUtils.Deserialize<MatchReadyDataRefreshS2C>( data );

            if ( feedback != null )
            {
                if ( matcherReadyDatas == null )
                {
                    matcherReadyDatas = feedback.matcherReadyDatas;
                }
                else
                {
                    long playerId = feedback.matcherReadyDatas[0].playerId;
                    if ( matcherReadyDatas.Find( p => p.playerId == playerId ) != null )
                        matcherReadyDatas.Remove( matcherReadyDatas.Find( p => p.playerId == playerId ) );
                    matcherReadyDatas.Add( feedback.matcherReadyDatas[0] );
                }

                view.SetFriendUI( matcherReadyDatas.Count > 1 );
                view.InitMatchMyselfUnitItem();
				if( matcherReadyDatas.Count > 1 )
				{
					view.InitMatchFriendUnitItem();
					view.SetChatFriends();
				}
            }
        }

        private void HandleInvationListFeedback( byte[] data )
        {
            InvitationListS2C feedback = ProtobufUtils.Deserialize<InvitationListS2C>( data );

            if ( feedback.result )
            {
                friendDatasList = feedback.invitationFriendInfos;
                view.RefrshMatchFriendListData();
            }
        }

        private void HandleInvationMatchFeedback( byte[] data )
        {
            InvitationMatchS2C feedback = ProtobufUtils.Deserialize<InvitationMatchS2C>( data );

            if ( feedback.result )
            {
                switch ( feedback.state )
                {
                    case InvitationState.FriendInBattle:
                        view.PopFriendInBattle();
                        SendInvitationListC2S();
                        currentFriendItem = null;
                        break;
                    case InvitationState.FriendInMatching:
                        view.PopFriendInMatching();
                        SendInvitationListC2S();
                        currentFriendItem = null;
                        break;
                    case InvitationState.WaitingProcess:
                        currentFriendItem.SetInvationButtonState( false );
                        isInvite = true;
                        break;
                    case InvitationState.AcceptInvitation:
                        break;
                    case InvitationState.RefuseInvitation:
                        break;
                    case InvitationState.CancelInvitation:
                        matcherReadyDatas.Clear();
                        currentFriendItem = null;
                        view.OnExit( true );
                        break;
                    case InvitationState.FriendReady:
                        view.SetFriendReadyState( true );
                        break;
                    case InvitationState.FriendCancelReady:
                        view.SetFriendReadyState( false );
                        break;
                    case InvitationState.StartMatch:
                        currentFriendId = GetFriendMatcherReadyData().playerId;
                        SendApplyRoomC2S();
                        break;
                    case InvitationState.RepeatSendInvitation:
                        break;
                    case InvitationState.DestroyInvitation:
                        break;
                }
            }
        }

        private void HandleInvationNoticeFeedback( byte[] data )
        {
            InvitationNoticeS2C feedback = ProtobufUtils.Deserialize<InvitationNoticeS2C>( data );
            switch ( feedback.state )
            {
                case InvitationState.FriendInBattle:
                    break;
                case InvitationState.WaitingProcess:
                    MessageDispatcher.PostMessage( Constants.MessageType.OpenFriendInvationAlert, feedback.friendId, feedback.battleType, feedback.friendName, feedback.friendPortrait );
                    break;
                case InvitationState.AcceptInvitation:
                    view.SetStartMatchBtState( false );
                    break;
                case InvitationState.RefuseInvitation:
                    view.PopFriendRefuse( feedback.friendName );
                    currentFriendItem.SetInvationButtonState( true );
                    currentFriendItem = null;
                    break;
                case InvitationState.FriendOffline:
                case InvitationState.CancelInvitation:
                    RemoveData( feedback.friendId );
                    view.SetFriendUI( false );
                    SendInvitationListC2S();
                    break;
                case InvitationState.FriendReady:
                    view.SetStartMatchBtState( true );
                    break;
                case InvitationState.FriendCancelReady:
                    view.SetStartMatchBtState( false );
                    break;
                case InvitationState.StartMatch:
                    currentFriendId = feedback.friendId;
                    SendApplyRoomC2S();
                    view.SetStartMatchState( true );
                    break;
                case InvitationState.DestroyInvitation:
                    break;
            }
        }

        private void RemoveData( long friendId )
        {
            currentFriendId = 0;
            currentFriendItem = null;
            matcherReadyDatas.Remove( matcherReadyDatas.Find( p => p.playerId == friendId ) );
            view.beInvited = false;
            view.OpenMatchUI();
        }

        #endregion  

        #region Connect Lobby Server

        private void MatchLobbyServer( string ip, int port )
        {
            NetworkManager.Connect( ServerType.LobbyServer, ip, port, OnConnectLobbyServer, OnConnectLobbyServerFailed );
        }

        private void OnConnectLobbyServer( ClientType clientType )
        {
            DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Success! Connected to LobbyServer " + clientType );

            HeartBeat.RegisterLobbyHeartMessageHandler();
            MultiDeviceListenerManager.RegisterHandler();
            DebugToScreen.RegisterHandler();

            NetworkManager.RemoveServerMessageHandler( ServerType.GameServer, MsgCode.MatchReadyDataMessage, HandleMatchReadyDataFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.GameServer, MsgCode.MatchReadyDataRefreshMessage, HandleMatchReadyDataRefreshFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.GameServer, MsgCode.ApplyRoomMessage, HandleApplyRoomFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.GameServer, MsgCode.ChangeBattleConfigMessage, HandleChangeBattleConfigFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.GameServer, MsgCode.InvitationMatchMessage, HandleInvationMatchFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.GameServer, MsgCode.InvitationNoticeMessage, HandleInvationNoticeFeedback );

            NetworkManager.RegisterServerMessageHandler( MsgCode.MatchMessage, HandleMatchFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.BattleMessage, HandleBattleMessageFeedback );
            //NetworkManager.RegisterServerMessageHandler( MsgCode.NoticeMessage, HandleNoticeMessageFeedback );

            SendApplyingMatchC2S();
        }

        private void OnConnectLobbyServerFailed( ClientType clientType )
        {
            string connectLoginServerFailed = "连接LobbyServer失败，请重试" + clientType;
            string titleText = "提示";
            System.Action reconnect = ConnectLobbyServer;

            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, reconnect, UI.AlertType.ConfirmAlone, connectLoginServerFailed, titleText );
        }

        private void ConnectLobbyServer()
        {
            MatchLobbyServer( DataManager.GetInstance().GetLobbyServerIp(), DataManager.GetInstance().GetLobbyServerPort() );
        }

        #endregion

        #region Connect Game Server

        private void ConnectGameServer( MatchS2C feedback )
        {
            NetworkManager.Shutdown( () => MatchGameServer( feedback.serverIp, feedback.serverPort ) );
        }

        private void MatchGameServer( string ip, int port )
        {
            NetworkManager.Connect( ServerType.GameServer, ip, port, OnConnectGameServer, OnConnectGameServerFailed );
        }

        private void OnConnectGameServer( ClientType clientType )
        {
            NetworkManager.RemoveServerMessageHandler( ServerType.LobbyServer, MsgCode.MatchMessage, HandleMatchFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.LobbyServer, MsgCode.BattleMessage, HandleBattleMessageFeedback );

            NetworkManager.RegisterServerMessageHandler( MsgCode.InvitationMatchMessage, HandleInvationMatchFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.InvitationNoticeMessage, HandleInvationNoticeFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.ApplyRoomMessage, HandleApplyRoomFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.ChangeBattleConfigMessage, HandleChangeBattleConfigFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.MatchReadyDataMessage, HandleMatchReadyDataFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.MatchReadyDataRefreshMessage, HandleMatchReadyDataRefreshFeedback );

            DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Success! Connected to GameServer " + clientType );

            //DataManager.GetInstance().RegisterDataServerMessageHandler();

            HeartBeat.RegisterGameHeartMessageHandler();
            MultiDeviceListenerManager.RegisterHandler();
            DebugToScreen.RegisterHandler();

            view.OpenMatchUI();
        }

        private void OnConnectGameServerFailed( ClientType clientType )
        {
            string connectGameServerFailed = "连接GameServer失败，请重试" + clientType;
            string titleText = "提示";
            System.Action reconnect = ConnectGameServer;

            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, reconnect, UI.AlertType.ConfirmAlone, connectGameServerFailed, titleText );
        }

        private void ConnectGameServer()
        {
            MatchGameServer( DataManager.GetInstance().GetGameServerIp(), DataManager.GetInstance().GetGameServerPort() );
        }

        #endregion

        #region Connect Battle Server

        private void ConnectBattleServer()
        {
            NetworkManager.Shutdown( () => MatchBattleServer( DataManager.GetInstance().GetBattleServerIp(), DataManager.GetInstance().GetBattleServerPort() ) );
        }

        private void MatchBattleServer( string ip, int port )
        {
            NetworkManager.Connect( ServerType.BattleServer, ip, port, OnConnectBattleServer, OnConnectBattleServerFailed );
        }

        private void OnConnectBattleServer( ClientType clientType )
        {
            NetworkManager.RemoveServerMessageHandler( ServerType.LobbyServer, MsgCode.MatchMessage, HandleMatchFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.LobbyServer, MsgCode.BattleMessage, HandleBattleMessageFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.InvitationListMessage, HandleInvationListFeedback );
            DebugUtils.Log( DebugUtils.Type.AsyncSocket, "Success! Connected to BattleServer " + clientType );
            HeartBeat.RegisterBattleHeartMessageHandler();
            MultiDeviceListenerManager.RegisterHandler();
            DebugToScreen.RegisterHandler();

            SceneManager.LoadSceneAsync( "Loading" );
            UIManager.locateState = UIManagerLocateState.None;

            SendNoticeC2S( 0 );
        }

        private void OnConnectBattleServerFailed( ClientType clientType )
        {
            string connectBattleServerFailed = "连接BattleServer失败，请重试" + clientType;
            string titleText = "提示";
            System.Action reconnect = ConnectBattleServer;

            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, reconnect, UI.AlertType.ConfirmAlone, connectBattleServerFailed, titleText );
        }
        #endregion
    }
}