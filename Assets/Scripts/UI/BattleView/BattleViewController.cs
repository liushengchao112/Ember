using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Network;
using Utils;
using Data;
using Constants;
using System;

using BattleUnit = Data.Battler.BattleUnit;

namespace UI
{
    public enum BattleUIControlType
    {
        Normal = 1,
        TypeOne = 2,
        TypeTwo = 3,
    }

    public class BattleViewController : ControllerBase
    {
        private BattleView _view;
		public List<InstituteSkillData> playerInstituteSkills; 

        public MatchSide side;
        public ForceMark mark;

		#region ChatUseValues

		private long friendlyPlayerID = -1;
		private string friendlyPlayerName;
		private List<string> enemyNames = new List<string>();

		#endregion
	
        public int deploymentPendingSquadIndex = -1;
        public int unitLargestAmount;

        private Action buildInstituteAction;
		private bool isBuildedInstitute;
		[HideInInspector]
		public uint buildedTowerNum = 0;

		private List<TowerProto.Tower> towerProtoData;
		public List<InstituteProto.Institute> instituteProtoData;
        private List<InstituteSkillProto.InstituteSkill> instituteSkillData;
		List<AttributeEffectProto.AttributeEffect> attributeEffectData;

        private int killCount = 0, killedCount = 0;
        private int redKillCount = 0, blueKillCount = 0;

        public Vector3 buildPosition;
		public DataManager dataManager;

        private BattleUIControlType controllType = BattleUIControlType.Normal;

		public int playerDeployedSquadNum;

		[HideInInspector]
		public List<SquadData> unitDataCache = new List<SquadData>();

        public BattleUIControlType GetBattleControllType()
        {
            return controllType;
        }

        public BattleViewController( BattleView v )
        {
            viewBase = v;
            _view = v;

			dataManager = DataManager.GetInstance();
            side = dataManager.GetMatchSide();

            //TODO:This code is temp code, when designer upgrade modelID use true modelID get towers.
            towerProtoData = GetTowerFromSide(); 

			instituteProtoData = dataManager.instituteProtoData;
			instituteSkillData = dataManager.instituteSkillProtoData;
			attributeEffectData = dataManager.attributeEffectProtoData;

            controllType = (BattleUIControlType)dataManager.unitOperationChoose;

            unitLargestAmount = GameConstants.PLAYER_CANDEPLOY_UNIT_MAXCOUNT;
        }

        public void Awake()
        {
            MessageDispatcher.AddObserver( ReceiveSquads, Constants.MessageType.InitBattleUISquads );
            MessageDispatcher.AddObserver( FillSquads, Constants.MessageType.FillBattleUISquads );
            MessageDispatcher.AddObserver( HandleUnitHpChanged, Constants.MessageType.SyncUnitHp );
            //MessageDispatcher.AddObserver( Squad_Heal, Constants.MessageType.SoldierHeal );
            MessageDispatcher.AddObserver( AdjustTimer, Constants.MessageType.BattleTimeChanged );
            MessageDispatcher.AddObserver( Unit_Destroy, Constants.MessageType.SoldierDeath );
            MessageDispatcher.AddObserver( SetEmber, Constants.MessageType.CoinChanged );
            MessageDispatcher.AddObserver( BattleResultReceived, Constants.MessageType.ShowBattleResultView );
            MessageDispatcher.AddObserver( GenerateSoldier, Constants.MessageType.GenerateSoldier );
            MessageDispatcher.AddObserver( DeploySquadMessage, Constants.MessageType.BattleUIOperationFeedBack );
            MessageDispatcher.AddObserver( PowerUpNotification, Constants.MessageType.SpawnPowerUp );
            MessageDispatcher.AddObserver( OpenInstitutePopUp, Constants.MessageType.OpenInstitutePopUp );
			MessageDispatcher.AddObserver( OpenBuildInstitutePopUp, Constants.MessageType.OpenBuildInstiutePopUp );
            MessageDispatcher.AddObserver( OpenTowerPopUp, Constants.MessageType.OpenTowerPopUp );
            MessageDispatcher.AddObserver( CloseTowerPopUp, Constants.MessageType.CloseTowerPopUp );
		
			MessageDispatcher.AddObserver( BuildTower, Constants.MessageType.BuildTower );
			MessageDispatcher.AddObserver( TapTower, Constants.MessageType.TapTower );
            MessageDispatcher.AddObserver( BuildInstitute, Constants.MessageType.BuildInstitute );
			MessageDispatcher.AddObserver( DeployTramCar, Constants.MessageType.BuildTramCar );
			MessageDispatcher.AddObserver( DeployDemolisher, Constants.MessageType.BuildDemolisher );
			MessageDispatcher.AddObserver( BuildingLevelUpFeedBack, Constants.MessageType.BuildingLevelUpOperationFeedBack );
			MessageDispatcher.AddObserver( InstituteSkillUpgradeFeedBack, Constants.MessageType.InstituteSkillOperationFeedBack );
			MessageDispatcher.AddObserver( AddBuildingBuffMessage, Constants.MessageType.AddBuildingBuffMessage );
			MessageDispatcher.AddObserver( RemoveBuildingBuffMessage, Constants.MessageType.RemoveBuildingBuffMessage );

			MessageDispatcher.AddObserver( DestroyTown, Constants.MessageType.TownDestroy );
			MessageDispatcher.AddObserver( DestroyedTower, Constants.MessageType.TowerDestroyed );
            MessageDispatcher.AddObserver( DestroyedInstitute, Constants.MessageType.InstituteDestroyed );
			MessageDispatcher.AddObserver( DestroyedTramCar, Constants.MessageType.TramCarDestroyed );
			MessageDispatcher.AddObserver( DestroyedDemolisher, Constants.MessageType.DemolisherDestroyed );

            MessageDispatcher.AddObserver( OnQuitBattleHandler, MessageType.SurrenderTipsHandler );
            MessageDispatcher.AddObserver( OnNoticeRespond, MessageType.BattleSituationResponse );
            MessageDispatcher.AddObserver( KillIdolNotice , MessageType.BattleUIKillIdolNotice );
            MessageDispatcher.AddObserver( NoticeHandle , MessageType.BattleUIKillUnitNotice );
            MessageDispatcher.AddObserver( OpenTutorialResult , MessageType.TutorialShowResult );

			NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.BattleChatsMessage, ChatFeedBack );
			NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.ForwardBattleChatsMessage, ForwardChatsFeedback );
        }

        public override void OnDestroy()
        {
            MessageDispatcher.RemoveObserver( ReceiveSquads, Constants.MessageType.InitBattleUISquads );
            MessageDispatcher.RemoveObserver( FillSquads, Constants.MessageType.FillBattleUISquads );
            MessageDispatcher.RemoveObserver( HandleUnitHpChanged, Constants.MessageType.SoldierHurt );
            //MessageDispatcher.RemoveObserver( Squad_Heal, Constants.MessageType.SoldierHeal );
            MessageDispatcher.RemoveObserver( AdjustTimer, Constants.MessageType.BattleTimeChanged );
            MessageDispatcher.RemoveObserver( Unit_Destroy, Constants.MessageType.SoldierDeath );
            MessageDispatcher.RemoveObserver( SetEmber, Constants.MessageType.CoinChanged );
            MessageDispatcher.RemoveObserver( BattleResultReceived, Constants.MessageType.ShowBattleResultView );
            MessageDispatcher.RemoveObserver( GenerateSoldier, Constants.MessageType.GenerateSoldier );
            MessageDispatcher.RemoveObserver( DeploySquadMessage, Constants.MessageType.BattleUIOperationFeedBack );
            MessageDispatcher.RemoveObserver( PowerUpNotification, Constants.MessageType.SpawnPowerUp );
			MessageDispatcher.RemoveObserver( OpenBuildInstitutePopUp, Constants.MessageType.OpenBuildInstiutePopUp );
            MessageDispatcher.RemoveObserver( OpenInstitutePopUp, Constants.MessageType.OpenInstitutePopUp );
            MessageDispatcher.RemoveObserver( OpenTowerPopUp, Constants.MessageType.OpenTowerPopUp );
            MessageDispatcher.RemoveObserver( CloseTowerPopUp, Constants.MessageType.CloseTowerPopUp );

			MessageDispatcher.RemoveObserver( BuildTower, Constants.MessageType.BuildTower );
			MessageDispatcher.RemoveObserver( TapTower, Constants.MessageType.TapTower );
            MessageDispatcher.RemoveObserver( BuildInstitute, Constants.MessageType.BuildInstitute );
			MessageDispatcher.RemoveObserver( DeployTramCar, Constants.MessageType.BuildTramCar );
			MessageDispatcher.RemoveObserver( DeployDemolisher, Constants.MessageType.BuildDemolisher );
			MessageDispatcher.RemoveObserver( BuildingLevelUpFeedBack, Constants.MessageType.BuildingLevelUpOperationFeedBack );
			MessageDispatcher.RemoveObserver( InstituteSkillUpgradeFeedBack, Constants.MessageType.InstituteSkillOperationFeedBack );
			MessageDispatcher.RemoveObserver( AddBuildingBuffMessage, Constants.MessageType.AddBuildingBuffMessage );
			MessageDispatcher.RemoveObserver( RemoveBuildingBuffMessage, Constants.MessageType.RemoveBuildingBuffMessage );

			MessageDispatcher.RemoveObserver( DestroyTown, Constants.MessageType.TownDestroy );
			MessageDispatcher.RemoveObserver( DestroyedTower, Constants.MessageType.TowerDestroyed );
            MessageDispatcher.RemoveObserver( DestroyedInstitute, Constants.MessageType.InstituteDestroyed );
			MessageDispatcher.RemoveObserver( DestroyedTramCar, Constants.MessageType.TramCarDestroyed );
			MessageDispatcher.RemoveObserver( DestroyedDemolisher, Constants.MessageType.DemolisherDestroyed );

            MessageDispatcher.RemoveObserver( OnQuitBattleHandler, MessageType.SurrenderTipsHandler );
            MessageDispatcher.RemoveObserver( OnNoticeRespond, MessageType.BattleSituationResponse );
            MessageDispatcher.RemoveObserver( KillIdolNotice , MessageType.BattleUIKillIdolNotice );
            MessageDispatcher.RemoveObserver( NoticeHandle , MessageType.BattleUIKillUnitNotice );
            MessageDispatcher.RemoveObserver( OpenTutorialResult , MessageType.TutorialShowResult );

            NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.BattleChatsMessage, ChatFeedBack );
			NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.ForwardBattleChatsMessage, ForwardChatsFeedback );
        }

        private void OnNoticeRespond(object redObj, object blueObj)
        {
            List<SettingFightingItemVo> redList = redObj as List<SettingFightingItemVo>;
            List<SettingFightingItemVo> buleList = blueObj as List<SettingFightingItemVo>;
            _view.InitFightingItem( redList, buleList );
        }

        private void OnQuitBattleHandler(object obj)
        {
            _view.ShowSurrenderTips( obj );
        }

        #region EventReceived

        private void ReceiveSquads( object cardsObj, object waitingCardObj )
        {
            List<SquadData> currentCards = (List<SquadData>)cardsObj;
            SquadData waitingCard = (SquadData)waitingCardObj;

            mark = DataManager.GetInstance().GetForceMark();

            _view.SetInitialSquads( currentCards, waitingCard );
            _view.InitSpecialCard();
        }

		private void FillSquads( object fillCardObj, object waitingCardObj, object buttonIndex )
        {
            SquadData filledSquad = (SquadData)fillCardObj;
            SquadData waitingSquad = (SquadData)waitingCardObj;

			_view.FillSquad( filledSquad, waitingSquad, (int)buttonIndex );
        }

        private void DeploySquadMessage( object deployResultObj, object canDeployObj )
        {
            //_view.DestroyDragImage();

            BattleUIOperationType deployResult = (BattleUIOperationType)deployResultObj;
            if ( deployResult != BattleUIOperationType.DeployUnitResult )
                return;

            bool canDeploy = (bool)canDeployObj;

            if ( !canDeploy )
            {
                deploymentPendingSquadIndex = -1;
                _view.SetLockSquadItems( false );
            }
        }

       private void GenerateSoldier( object markObj, object ownerObj, object posObj )
        {
            ForceMark mark = (ForceMark)markObj;
            long ownerId = (long)ownerObj;

            Utils.DebugUtils.Log( DebugUtils.Type.UI, string.Format( "GenerateSoldier: mark {0} id {1}", mark, ownerId ) );

            if ( this.mark != mark )
                return;

            _view.SetLockSquadItems( true );
            _view.CanDeployUnits( emberCount );
			//Drag deployment logic locked.Dwayne 2017.9
			_view.DeployUnitCard( ownerId, deploymentPendingSquadIndex );
        }

        public void Unit_Destroy( object markObj, object idObj )
        {
            long ownerId = (long)idObj;
            ForceMark mark = (ForceMark)markObj;

            Utils.DebugUtils.Log( DebugUtils.Type.UI, string.Format( "Unit destroyed: mark {0} id {1}", mark, ownerId ) );

            if ( IsEnemyDead( this.mark , mark ) )
            {
                if ( side == MatchSide.Red )
                {
                    _view.SetRedKillCount( ++redKillCount );
                }
                else if ( side == MatchSide.Blue )
                {
                    _view.SetBlueKillCount( ++blueKillCount );
                }
                EnemyDead();
            }
            else 
            {
                if ( this.mark == mark )
                {
                    playerDeployedSquadNum--;

                    if ( playerDeployedSquadNum == ( GameConstants.PLAYER_CANDEPLOY_UNIT_MAXCOUNT - 1 ) )
                    {
                        _view.ResetSquadDeployedLimitStatus();
                    }

                    _view.SetKilledCount( ++killedCount );
                    _view.DestroyUnitcard( controllType , ownerId );
                    _view.CanDeployUnits( emberCount );
                }

                 if ( side == MatchSide.Red )
                {
                    _view.SetBlueKillCount( ++blueKillCount );                    
                }
                else if ( side == MatchSide.Blue )
                {
                    _view.SetRedKillCount( ++redKillCount );
                }
            }
        }

        private bool IsEnemyDead( ForceMark self , ForceMark other )
        {
            if ( GetSideFromMark( self ) == MatchSide.Red && GetSideFromMark( other ) == MatchSide.Blue )
            {
                return true;
            }
            else if ( GetSideFromMark( self ) == MatchSide.Blue && GetSideFromMark( other ) == MatchSide.Red )
            {
                return true;
            }
            return false;
        }

        private MatchSide GetSideFromMark( ForceMark mark )
        {
            if ( mark <= ForceMark.BottomRedForce )
            {
                return MatchSide.Red;
            }
            else if ( mark <= ForceMark.BottomBlueForce )
            {
                return MatchSide.Blue;
            }
            else
            {
                return MatchSide.NoSide;
            }
        }

        private void EnemyDead()
        {
            //TODO
            _view.SetKillCount( ++killCount );
        }

        public void Unit_Heal( object markObj, object idObj, object healObj, object maxHpObj )
        {
            long ownerId = (long)idObj;
            int heal = (int)healObj;
            int maxHp = (int)maxHpObj;

            if ( mark != (ForceMark)markObj )
            {
                // no myself
            }
            else
            {
                _view.Squad_AdjustHealth( ownerId, heal, maxHp );
            }
        }

        public void HandleUnitHpChanged( object markObj, object idObj, object hpObj, object maxHpObj )
        {
            long unitId = (long)idObj;
            int hp = (int)hpObj;
            int maxHp = (int)maxHpObj;

            if ( mark != (ForceMark)markObj )
            {
                //EnemyHurt();
            }
            else
            {
                _view.Squad_AdjustHealth( unitId, hp, maxHp );
            }
        }

        public void KillIdolNotice( object markObj , object iconId )
        {
            _view.noticeView.ShowKillIdolNotice( (ForceMark)markObj , (int)iconId );
        }

        public void NoticeHandle( object killerMarkObj , object killerIconId , object beKillerMarkObj , object beKillerIconId )
        {
            _view.noticeView.ShowKillUnitNotice( (ForceMark)killerMarkObj , (int)killerIconId , (ForceMark)beKillerMarkObj , (int)beKillerIconId );
        }

        private void EnemyHurt()
        {
            //TODO
        }

        private void BattleResultReceived( object res, object info )
        {
            DebugUtils.Log( DebugUtils.Type.Battle, string.Format( "Received BattleResult: {0}", res ) );

            BattleResultData resInfo = (BattleResultData)info;
            NoticeType resType = (NoticeType)res;

            _view.OpenBattleResult( resType, resInfo );
        }

        private void OpenTutorialResult()
        {
            _view.OpenTutorialResult();
        }

        private int lastSecond = 0;
        private void AdjustTimer( object timeObj )
        {
            int curSecond = (int)(float)timeObj;
            if (lastSecond == curSecond) return;

            lastSecond = curSecond;

            _view.SetTimer( lastSecond );
            _view.SetSurrenderButtonStateTime( lastSecond );
        }

        private void SetEmber( object amount )
        {
            emberCount = (int)amount;
            _view.SetEmber( (int)amount );
            _view.CanDeployUnits( emberCount );
        }

        public void PowerUpNotification( object powerUpId, object powerUpType, object postion )
        {
            PowerUpType type = (PowerUpType)powerUpType;
            _view.ReceiveNotification( type.ToString() );
        }

		public void OpenBuildInstitutePopUp( object pos )
		{
			if( isBuildedInstitute == false )
			{
				string tempStr = string.Format( _view.buildInstitutePopUpStr, GetInstituteCost());

				if( buildInstituteAction == null )
				{
					buildInstituteAction = ConfirmBuildInstitute;
				}

				MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, buildInstituteAction, UI.AlertType.ConfirmAndCancel, tempStr, _view.buildInstitutePopUpTitleStr );

                if ( dataManager.GetBattleType() == BattleType.Tutorial && DataManager.GetInstance().GetTutorialStage() == PVE.TutorialModeManager.TutorialModeStage.BuildingControlOperation_Stage )
                    MessageDispatcher.PostMessage( Constants.MessageType.OpenNewbieGuide, 3, 9 );
            }
		}

		private void ConfirmBuildInstitute()
		{
			if( _view.playerEmber >= GetInstituteCost() && isBuildedInstitute == false )
			{
				MessageDispatcher.PostMessage( Constants.MessageType.GenerateBuilding, -1, BuildingType.Institute );
			}
		}

		//This is InstituteInfo PopUp
        private void OpenInstitutePopUp()
        {
            _view.OpenInstitutePopUp();
        }

        private void OpenTowerPopUp( object buildPos )
        {
			if( buildedTowerNum < GameConstants.TOWER_DEPLOYMENT_LIMIT )
			{
				buildPosition = (Vector3)buildPos;
				_view.OpenTowerPopUp();
			}
        }

        private void CloseTowerPopUp()
        {
            _view.CloseTowerPopUp();
        }

        #endregion EventReceived

        #region EventSend
		//Drag deployment logic locked.Dwayne 2017.9
		/*
        public void SendDeploySquad( int metaId, Vector2 position )
        {
            MessageDispatcher.PostMessage( Constants.MessageType.DeploySquad, metaId, position );
        }*/

		public void SendDeploySquad( int metaId, int buttonIndex )
		{
			MessageDispatcher.PostMessage( Constants.MessageType.DeploySquad, metaId, buttonIndex );
		}

        public void GoBackToMainMenu()
        {
			if ( dataManager.GetBattleType() == BattleType.Survival ||
				dataManager.GetBattleType() == BattleType.Tranining ||
				dataManager.GetBattleType() == BattleType.Tutorial )
            {
                HandleConnectServerFeedback();
            }
            else
            {
                Network.NetworkManager.Shutdown( ShutdownServerSuccess );
            }
        }

        private void ShutdownServerSuccess()
        {
            System.Action<ClientType> connectSuccess = ( ClientType clientType ) => 
            { 
                HandleConnectServerFeedback();

                //DataManager.GetInstance().RegisterDataServerMessageHandler();

                HeartBeat.RegisterGameHeartMessageHandler();
                MultiDeviceListenerManager.RegisterHandler();
                DebugToScreen.RegisterHandler();
            };

            Network.NetworkManager.Connect( ServerType.GameServer, dataManager.GetGameServerIp(), dataManager.GetGameServerPort(), connectSuccess, OnConnectGameServerFailed );
        }

        private void OnConnectGameServerFailed( ClientType clientType )
        {
            string connectGameServerFailed = "连接GameServer失败，请重试" + clientType;
            string titleText = "提示";
            System.Action<ClientType> connectSuccess = ( ClientType ct ) => 
            { 
                HandleConnectServerFeedback();
                //DataManager.GetInstance().RegisterDataServerMessageHandler();
                HeartBeat.RegisterGameHeartMessageHandler();
                MultiDeviceListenerManager.RegisterHandler();
                DebugToScreen.RegisterHandler();
            };
            System.Action reconnect = () => { Network.NetworkManager.Connect( ServerType.GameServer, dataManager.GetGameServerIp(), dataManager.GetGameServerPort(), connectSuccess, OnConnectGameServerFailed ); };

            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, reconnect, UI.AlertType.ConfirmAlone, connectGameServerFailed, titleText );
        }

        private void HandleConnectServerFeedback()
        {
            DataManager clientData = DataManager.GetInstance();
            clientData.ResetBattleData();
            clientData.SetPlayerIsInBattle( false );
            UnityEngine.SceneManagement.SceneManager.LoadScene( "MainMenu" );
            UI.UIManager.locateState = UI.UIManagerLocateState.MainMenu;
        }
        #endregion EventSend

		#region ChatMessageFunctions

		public void SendChatMessage( string str, BattleChatType type )
		{
			if( dataManager.CurBattleIsPVE() )
			{
				return;
			}
			
			BattleChatC2S message = new BattleChatC2S();

			List<long> enemyPlayerID = new List<long>();

			if( friendlyPlayerID == -1 )
			{
				List<Battler>list = dataManager.GetBattlers();
				MatchSide side = dataManager.GetMatchSide();
				long playerID = dataManager.GetPlayerId();

				for( int i = 0; i < list.Count; i++ )
				{
					if( list[i].side == side && list[i].playerId != playerID )
					{
						friendlyPlayerID = list[i].playerId;
						friendlyPlayerName = list[i].name;
					}
					else if( list[i].side != side )
					{
						enemyPlayerID.Add( list[i].playerId );
						enemyNames.Add( list[i].name );
					}
				}
			}
				
			if( type == BattleChatType.party )
			{
				message.battleChatType = BattleChatType.party;
				message.playerId.Add( friendlyPlayerID ); 
			}
			else if( type == BattleChatType.everyone )
			{
				message.battleChatType = BattleChatType.everyone;

				if( friendlyPlayerID != -1 )
				{
					message.playerId.Add( friendlyPlayerID ); 
				}
					
				for( int i = 0; i < enemyPlayerID.Count; i++ )
				{
					message.playerId.Add( enemyPlayerID[i] ); 
				}
			}
			else
			{
				//TODO: If have more type chat channel add here.
				DebugUtils.LogError( DebugUtils.Type.Chat, "Please check chat Type." );
			}

			message.chatContent = str;

			byte[] stream = ProtobufUtils.Serialize( message );
			NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.BattleChatsMessage, stream );
		}

		public void ChatFeedBack( byte[] feedback )
		{
			BattleChatS2C data = ProtobufUtils.Deserialize<BattleChatS2C>( feedback );

			if( data.result )
			{
				DebugUtils.Log( DebugUtils.Type.Chat, "Message send complete." );
			}
		}

		public void ForwardChatsFeedback( byte[] feedback )
		{
			ForwardBattleChatS2C data = ProtobufUtils.Deserialize<ForwardBattleChatS2C>( feedback );

			if( data.battleChatType == BattleChatType.match )
			{
				DebugUtils.LogError( DebugUtils.Type.Chat, string.Format( "This message type {0} is not battle chat", data.battleChatType ) );
				return;
			}

			if( !string.IsNullOrEmpty( data.chatContent ) )
			{
				string str = string.Format( data.sendPlayerName + " : {0}", data.chatContent );
				bool isEnemy = false;

				if( enemyNames.Contains( data.sendPlayerName ) )
				{
					isEnemy = true;	
				}

				_view.ShowChatMessage( str, isEnemy );
			}
		}

		#endregion

        #region Special Card Data

		[HideInInspector]
		public int emberCount;
        private int instituteCost, towerCost, tramCarCost, demolisherCost, tramCarLimitNum, demolisherLimitNum;

		public int GetSpecialCost( SpecialCardType specialIndex, out int limitNum )
        {
			//Locked building mode drag deployment code. Dwayne.
            /*if ( instituteCost == 0 )
            {
                instituteCost = instituteProtoData.Find( p => p.ID == Constants.GameConstants.INSTITUTE_ID ).DeploymentCost;

                if ( towerProtoData.Count > 0 )
                {
                    towerCost = towerProtoData[0].DeploymentCost;
                }
                else
                {
                    DebugUtils.LogError( DebugUtils.Type.UI, "There can't find towerPorotoData,check table and logic." );
                }
            }*/

            if ( tramCarCost == 0 )
            {
				UnitsProto.Unit unit = dataManager.unitsProtoData.Find( p => p.ID == Constants.GameConstants.UNIT_TRAMCAR_METAID );
				tramCarCost = unit.DeploymentCost;
				tramCarLimitNum = unit.MaxMembers;
            }

            if ( demolisherCost == 0 )
            {
				UnitsProto.Unit unit = dataManager.unitsProtoData.Find( p => p.ID == Constants.GameConstants.UNIT_DEMOLISHER_METAID );
				demolisherCost = unit.DeploymentCost;
				demolisherLimitNum = unit.MaxMembers;
            }

            int cost = 0;
            switch ( specialIndex )
            {
				//Locked building mode drag deployment code. Dwayne.
				/*
                case SpecialCardType.Institute:
                {
                    cost = instituteCost;
					limitNum = GameConstants.INSTITUTE_DEPLOYMENT_LIMIT;
                    break;
                }
                case SpecialCardType.Tower:
                {
                    cost = towerCost;
					limitNum = GameConstants.TOWER_DEPLOYMENT_LIMIT;
                    break;
                }*/
                case SpecialCardType.Tramcar:
                {
                    cost = tramCarCost;
					limitNum = tramCarLimitNum;
                    break;
                }
                case SpecialCardType.Demolisher:
                {
                    cost = tramCarCost;
					limitNum = demolisherLimitNum;
                    break;
                }
                default:
                {
                    DebugUtils.LogError( DebugUtils.Type.UI, string.Format( "Can't handle this SpecialCardType {0} now", specialIndex ) );
					limitNum = -1;
                    break;
                }
            }
            return cost;
        }

		public int GetInstituteCost()
		{
			if ( instituteCost == 0 )
            {
                instituteCost = instituteProtoData.Find( p => p.ID == Constants.GameConstants.INSTITUTE_ID ).DeploymentCost;
            }

			return instituteCost;
		}

		//Locked building mode drag deployment code. Dwayne.
		/*
		private void BuildTower( object mark, object ownerId, object pos )
		{
			if( this.mark == ( ForceMark )mark )
			{
				_view.BuildTower();
			}
		}

		private void DestroyedTower( object mark, object ownerId )
		{
			if( this.mark == ( ForceMark )mark )
			{
				_view.DestroyedTower();
			}
		}*/

		private void BuildTower( object mark, object ownerId, object pos  )
		{
			if( this.mark == ( ForceMark ) mark )
			{
				buildedTowerNum++;
			}
		}

		private void DestroyedTower( object mark, object ownerId )
		{
			if( this.mark == ( ForceMark ) mark )
			{
				_view.CloseTowerPopUp();
				buildedTowerNum--;
			}
		}

		private void BuildInstitute( object mark, object ownerId, object pos, object instituteSkillsData )
        {
            if ( this.mark == (ForceMark)mark )
            {
				if( playerInstituteSkills == null )
				{
					playerInstituteSkills = ( List<InstituteSkillData> )instituteSkillsData;
				}
				//Locked building mode drag deployment code. Dwayne.
				//_view.BuildInstitute();
				isBuildedInstitute = true;
            }
        }

        private void DestroyedInstitute( object mark, object ownerId, object pos )
        {
            if ( this.mark == (ForceMark)mark )
            {
				//Locked building mode drag deployment code. Dwayne.
				//_view.DestroyedInstitute();
				isBuildedInstitute = false;
				_view.DestroyInstitute();
            }
        }

		private void DeployTramCar( object mark, object id, object pos )
		{
			if( this.mark == ( ForceMark )mark )
			{
				_view.DeployTramCar();
			}
		}

		private void DestroyedTramCar( object mark, object id, object pos )
		{
			if( this.mark == ( ForceMark )mark )
			{
				_view.DestroyedTramCar();
			}
		}

		private void DeployDemolisher( object mark, object id, object pos )
		{
			if( this.mark == ( ForceMark )mark )
			{
				_view.DeployDemolisher();
			}
		}

		private void DestroyedDemolisher( object mark, object id, object pos )
		{
			if( this.mark == ( ForceMark )mark )
			{
				_view.DestroyedDemolisher();
			}
		}

		private void DestroyTown( object id )
		{
			_view.DestroyTown( ( long )id );
		}

        #endregion

        #region Data Tower
		//TODO: Temp code just get self side tower Dwayne
		private List<TowerProto.Tower> GetTowerFromSide()
		{
			if( dataManager.GetMatchSide() == MatchSide.Blue )
			{
				return dataManager.towerProtoData.FindAll( p => p.ModelID == 60106 );
			}
			else
			{
				return dataManager.towerProtoData.FindAll( p => p.ModelID == 60107 );
			}
		}

        public int GetSelfTowersCount()
        {
            return towerProtoData.Count;
        }

        public int GetTowersId( int towerIndex )
        {
            return towerProtoData[towerIndex].ID;
        }

        public string GetTowerName( int towerIndex )
        {
            return towerProtoData[towerIndex].Name;
        }

        public int GetTowerIcon( int towerIndex )
        {
            return towerProtoData[towerIndex].IconID;
        }

        public int GetTowerCost( int towerIndex )
        {
            return towerProtoData[towerIndex].DeploymentCost;
        }

        public string GetTowerPropString1( int towerIndex )
        {
            int dmgBase = towerProtoData[towerIndex].RangedDmgBase;
            int dmgVar = towerProtoData[towerIndex].RangedDmgVar;

            return string.Format( "伤害值：{0}~{1}", dmgBase - dmgVar, dmgBase + dmgVar );
        }

        public string GetTowerPropString2( int towerIndex )
        {
            return "";
        }

        #endregion

		#region Tap Tower PopUp

		private void TapTower(object cost,object id,object pos)
		{
			_view.OpenTapTowerPopUp( (int)cost, (long)id, (Transform)pos );
		}

		#endregion

        #region InstituteSkill functions

        public string GetInstituteSkillItemName( int instituteSkillIndex )
        {
            return instituteSkillData[instituteSkillIndex].Name;
        }

        public int GetInstituteSkillItemIcon( int instituteSkillIndex )
        {
            return instituteSkillData[instituteSkillIndex].IconID;
        }

        public int GetInstituteSkillItemCost( int instituteSkillIndex )
        {
            return instituteSkillData[instituteSkillIndex].Cost;
        }

		public void InstituteSkillApplyUpgrade( int skillID, int upgradeNum = 1 )
		{
			MessageDispatcher.PostMessage( Constants.MessageType.InstituteSkillLevelUp, skillID, upgradeNum );
		}

		private void InstituteSkillUpgradeFeedBack( object mark, object applySKillID, object upgradeSkillID )
		{
			if( ( ForceMark )mark == this.mark )
			{
				_view.InstituteSkillLevelUpComplete( ( int )applySKillID, ( int )upgradeSkillID );
				DebugUtils.Log( DebugUtils.Type.InstitutesSkill, string.Format( "BattleViewController recieve the instituteSkill levelUp complete feedBack applySkillID is {0}, upgradeSkillID is {1}", ( int )applySKillID, ( int )upgradeSkillID));
			}
		}

		private void SplitBuffMessage( object str , bool isRemoveAttribute )
		{
			string[] buffIds = str.ToString ().Split ( new char[]{ '|' } , StringSplitOptions.RemoveEmptyEntries );

			for ( int i = 0; i < buffIds.Length; i++ )
			{
				int buffid = int.Parse ( buffIds [ i ] );
				AttributeEffectProto.AttributeEffect attributeEffect = attributeEffectData.Find ( p => p.ID == buffid );
				CalculationBuffAttribute ( buffAttributes , attributeEffect.AttributeType , attributeEffect.AffectedType , attributeEffect.CalculateType , attributeEffect.MainValue , attributeEffect.DescriptionId, isRemoveAttribute );
			}

			BuildingBuffDescription();
		}

		private void BuildingBuffDescription()
		{
			string attribute = "";

			foreach ( int attributeType in buffAttributes.Keys )
			{
				float[] attributes = buffAttributes [ attributeType ];

				if( attributes [ 0 ]  != 0 )
				{
					string description  = attributes [ 0 ] > 0 ? ( ( int ) attributes [ 2 ] ).Localize () : ( ( int ) attributes [ 3 ] ).Localize ();
					attribute += string.Format ( description , Mathf.Abs ( attributes [ 0 ] ) ) + "\n";
				}

				if( attributes [ 1 ]  != 0 )
				{
					string description  = attributes [ 1 ] > 0 ? ( ( int ) attributes [ 2 ] ).Localize () : ( ( int ) attributes [ 3 ] ).Localize ();
					attribute += string.Format ( description , Mathf.Abs ( attributes [ 1 ] * 100 ) ) + "%" + "\n";
				}
			}
				
			if( attribute.LastIndexOf ( "\n" )  != -1 )
			{
				attribute = attribute.Remove ( attribute.LastIndexOf ( "\n" ) , 1 );
			}

			_view.SetBuffPanelText ( attribute );
		}

		private void CalculationBuffAttribute( Dictionary<int,float[]> attributes, int attributeType, int affectedType, int calculateType, float value, int descriptionId, bool isRemoveAttribute )
		{
			if( affectedType == 2 )
			{
				value = -value;
			}

			if( isRemoveAttribute )
			{
				value = -value;
			}

			int index = affectedType + 1;

			if( attributes.ContainsKey ( attributeType ) )
			{
				if( calculateType == 1 )
				{
					attributes[ attributeType ][ 0 ] += value;
				}
				else if( calculateType == 2 )
				{
					attributes[ attributeType ][ 1 ] += value;
				}
			}
			else if( attributeType != 0 )
			{
				if( calculateType == 1 )
				{
					attributes.Add ( attributeType , new float[]{ value, 0, 0, 0 } );
				}
				else if( calculateType == 2 )
				{
					attributes.Add ( attributeType , new float[]{ 0, value, 0, 0 } );
				}
			}

			attributes [ attributeType ] [ index ] = descriptionId;
		}

		//This function use for show institute skill effect to ui.
		Dictionary<int,float[]> buffAttributes = new Dictionary<int, float[]> ();

		public void AddBuildingBuffMessage( object str )
		{
			SplitBuffMessage( str , false );
		}

		//This function use for show institute skill effect to ui.
		public void RemoveBuildingBuffMessage( object str )
		{
			SplitBuffMessage( str , true );
		}

        #endregion

		#region BuildingLevelUp

		public void InstituteLevelUp()
		{
			MessageDispatcher.PostMessage( Constants.MessageType.BuildingLevelUP, BuildingType.Institute, dataManager.GetPlayerId() );
		}

		public void BuildingLevelUpFeedBack( object mark, object buildingType )
		{
			BuildingType type =	( BuildingType )buildingType; 	

			if( ( ForceMark )mark == this.mark )
			{
				if( type == BuildingType.Institute )
				{
					_view.InstituteLevelUpComplete();
					DebugUtils.Log( DebugUtils.Type.BuildingLevelUp, "BattleViewController received feedBack." );
				}
				else
				{
					//TODO: Maybe we will have more building can levelUp but now just institute.
				}
			}
		}

		#endregion
    }
}
