using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

using Data;
using UI;
using Constants;
using Network;
using BattleAgent;
using Utils;

using BattleUnit = Data.Battler.BattleUnit;

public enum GestureType
{
    None,
    SingleTapUnit,
	SingleTapInstituteBase,
	SingleTapTowerBase,
    SingleTapGroundWalkable,
    SingleTapFlyingWalkable,
    DoubleTapMap,
    DragWithSingleFinger
}

public enum GestureState
{
    None,
    Started,
    Updated,
    Ended
}

public class InputHandler : MonoBehaviour
{
    private BattleManager battleManager;

    private float cameraViewWitdh;
    private float cameraViewHeight;

    private bool startDragUnitOnFlying = false;
    private bool startDragUnitOnGround = false;
    private bool drawingRectOnScreen = false;

    private Vector2 dragStartScreenPoint;
    private Vector2 dragStartWorldPoint;
    private Vector2 dragEndScreenPoint;
    private Vector2 dragEndWorldPoint;
    private Vector3 dragScreenRectStartPosition = Vector3.zero;
    private Vector3 dragScreenRectEndPosition = Vector3.zero;

	//When player drop the building icon, temp save the position waiting for use.
	Vector3 buildBuildingPos = Vector3.zero;

    private NavMeshQueryFilter groundFilter;
    private NavMeshQueryFilter flyFilter;

    private Vector3 lastDragOnGroundPoint;
    private Vector3 lastDragOnFlyingPoint;

    private float dragOnGroundDistance = 0;
    private float dragOnFlyingDistance = 0;

    private int groundPathPointTotalCount = 0;// Record how many path count in current drag unit operation.
    private int flyingPathPointTotalCount = 0;// Record how many path count in current drag unit operation.

    private List<Vector3> dragOnGroundPointList = new List<Vector3>();
    private List<Vector3> dragOnFlyingPointList = new List<Vector3>();

    private int raycastDistance = 40;
    private float maxHeightDifference = 0.5f;

    private enum UploadSituationType
    {
        Upload = 1,
        Request,
        Result,
    }

    #region BattleFieldBuildMode value

	//Locked building mode drag deployment code.Dwayne.
    //private BuildingItem playerUsedCheckMapObj;//Use for check which MapCheckObj is active.
	//private bool canDeployBuildingOnMap = false;
	//private LayerMask buildingLayMask;
	private int bornAraeLayMask = -1;

	#endregion

    private static InputHandler instance;

    public static InputHandler Instance
    {
        get
        {
            if ( instance == null )
            {
                instance = GameObject.Find( "Main Camera" ).GetComponent<InputHandler>();
            }
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        battleManager = GetComponent<BattleManager>();
        DataManager clientDate = DataManager.GetInstance();

        BattleMessageAgent.simulateBattle = clientDate.GetBattleSimluateState();
        BattleMessageAgent.battleType = clientDate.GetBattleType();
        BattleMessageAgent.RegisterAgentMessageHandler( MsgCode.UpdateMessage, AgentUpdateHandler );
        BattleMessageAgent.RegisterAgentMessageHandler( MsgCode.NoticeMessage, AgentNoticeHandler );
        BattleMessageAgent.RegisterAgentMessageHandler( MsgCode.SyncMessage, AgentSyncHandler );
        BattleMessageAgent.RegisterAgentMessageHandler( MsgCode.QuitBattleMessage, QuitBattleFeedBack );
        BattleMessageAgent.RegisterAgentMessageHandler( MsgCode.UploadSituationMessage, OnUploadSituationFeedBack );
        BattleMessageAgent.RegisterAgentMessageHandler( MsgCode.PveBattleResultMessage, OnPveBattleResultFeedBack );

        // play back battle message
        MessageDispatcher.AddObserver( SetBattleRuntimeSpeed, MessageType.SetPlaybackSpeed );
        MessageDispatcher.AddObserver( SetBattleRecordPlayingState, MessageType.SetPlaybackPlayingState );
        MessageDispatcher.AddObserver( RandomSetCameraTarget, MessageType.SetCameraFollowForce );

        // real battle message
        MessageDispatcher.AddObserver( GestureOnMapHandler, MessageType.GestureOnNavigateCamera );
        MessageDispatcher.AddObserver( GenerateUnitOnMap, MessageType.DeploySquad );
		MessageDispatcher.AddObserver( GenerateBuildingOnMap, MessageType.GenerateBuilding );
		MessageDispatcher.AddObserver( RecylingTower, MessageType.RecylingTower );

		//Locked building mode drag deployment code.Dwayne.
		/*MessageDispatcher.AddObserver( OpenCheckBattleFieldObj, MessageType.OpenCheckBattleFieldObj );
		MessageDispatcher.AddObserver( CheckBattleFieldObjPosChange, MessageType.CheckBattleFieldObjPositionChange );
		MessageDispatcher.AddObserver( CloseCheckBattleFieldObj, MessageType.CloseCheckBattleFieldObj );
		MessageDispatcher.AddObserver( ShowDeployBuildingAreaFromTower, MessageType.DeployBuildingAreaOpen );
		MessageDispatcher.AddObserver( CloseDeployBuildingAreaFromTower, MessageType.DeployBuildingAreaClose );*/

		MessageDispatcher.AddObserver( SendBuildingLevelUp, MessageType.BuildingLevelUP );
		MessageDispatcher.AddObserver( SendInstituteSkillLevelUp, MessageType.InstituteSkillLevelUp );
        MessageDispatcher.AddObserver( SelectObjects, MessageType.SelectObjectInBattle );
        MessageDispatcher.AddObserver( SelectAllObjects, MessageType.SelectAll );
        MessageDispatcher.AddObserver( OnBattleSituationRequest, MessageType.BattleSituationRequest );

        // TODO: need be noticed by server
        MessageDispatcher.AddObserver( QuitBattleRequest, MessageType.QuitBattleRequest );
        MessageDispatcher.AddObserver( BattleEnd, MessageType.BattleEnd );

        if ( bornAraeLayMask == -1 )
        {
            bornAraeLayMask = LayerMask.NameToLayer( LayerName.LAYER_BORNAREA );
        }
    }

    void OnDestroy()
    {
        BattleMessageAgent.RemoveAgentMessageHandler( MsgCode.UpdateMessage, AgentUpdateHandler );
        BattleMessageAgent.RemoveAgentMessageHandler( MsgCode.NoticeMessage, AgentNoticeHandler );
        BattleMessageAgent.RemoveAgentMessageHandler( MsgCode.SyncMessage, AgentSyncHandler );
        BattleMessageAgent.RemoveAgentMessageHandler( MsgCode.QuitBattleMessage, QuitBattleFeedBack );
        BattleMessageAgent.RemoveAgentMessageHandler( MsgCode.UploadSituationMessage, OnUploadSituationFeedBack );
        BattleMessageAgent.RemoveAgentMessageHandler( MsgCode.PveBattleResultMessage, OnPveBattleResultFeedBack );

        MessageDispatcher.RemoveObserver( GestureOnMapHandler, MessageType.GestureOnNavigateCamera );
        MessageDispatcher.RemoveObserver( GenerateUnitOnMap, MessageType.DeploySquad );
		MessageDispatcher.RemoveObserver( GenerateBuildingOnMap, MessageType.GenerateBuilding );
		MessageDispatcher.RemoveObserver( RecylingTower, MessageType.RecylingTower );

		//Locked building mode drag deployment code.Dwayne.
		/*MessageDispatcher.RemoveObserver( OpenCheckBattleFieldObj, MessageType.OpenCheckBattleFieldObj );
		MessageDispatcher.RemoveObserver( CheckBattleFieldObjPosChange, MessageType.CheckBattleFieldObjPositionChange );
		MessageDispatcher.RemoveObserver( CloseCheckBattleFieldObj, MessageType.CloseCheckBattleFieldObj );
		MessageDispatcher.RemoveObserver( ShowDeployBuildingAreaFromTower, MessageType.DeployBuildingAreaOpen );
		MessageDispatcher.RemoveObserver( CloseDeployBuildingAreaFromTower, MessageType.DeployBuildingAreaClose );*/

		MessageDispatcher.RemoveObserver( SendBuildingLevelUp, MessageType.BuildingLevelUP );
		MessageDispatcher.RemoveObserver( SendInstituteSkillLevelUp, MessageType.InstituteSkillLevelUp );
        MessageDispatcher.RemoveObserver( SelectObjects, MessageType.SelectObjectInBattle );
        MessageDispatcher.RemoveObserver( SelectAllObjects, MessageType.SelectAll );
        MessageDispatcher.RemoveObserver( QuitBattleRequest, MessageType.QuitBattleRequest );
        MessageDispatcher.RemoveObserver( BattleEnd, MessageType.BattleEnd );
        MessageDispatcher.RemoveObserver( OnBattleSituationRequest, MessageType.BattleSituationRequest );

        MessageDispatcher.RemoveObserver( SetBattleRuntimeSpeed, MessageType.SetPlaybackSpeed );
        MessageDispatcher.RemoveObserver( SetBattleRecordPlayingState, MessageType.SetPlaybackPlayingState );
        MessageDispatcher.RemoveObserver( RandomSetCameraTarget, MessageType.SetCameraFollowForce );
    }

    void AgentUpdateHandler( byte[] data )
    {
        DebugUtils.Assert( data != null );
        
        try
        {
            UpdateS2C message = ProtobufUtils.Deserialize<UpdateS2C>( data );

            if ( message.ops != null && message.ops.Count > 0 )
            {
                DebugUtils.Log( DebugUtils.Type.Battle, "receive update message from server! operation count = " + message.ops.Count );
            }

            DebugUtils.Assert( message.battleId == battleManager.Id, "it is not the same battle! message battle id = " + message.battleId + ", battle id = " + battleManager.Id );

            if ( message.timestamp != DataManager.GetInstance().GetFrame() + 1 )
            {
                DebugUtils.LogError( DebugUtils.Type.Important, "client timestamp = " + DataManager.GetInstance().GetFrame() + ", server timpstamp = " + message.timestamp );
            }

            DataManager.GetInstance().SetFrame( message.timestamp );

            if ( message.ops != null )
            {
                IEnumerator ie = message.ops.GetEnumerator();
                while ( ie.MoveNext() )
                {
                    Operation op = (Operation)ie.Current;
                    if ( op.opType == OperationType.ChangeTarget )
                    {
                        battleManager.ChangeForceTarget( op.playerId, op.unitId, op.targetId );
                    }
                    else if ( op.opType == OperationType.SyncPath )
                    {
                        battleManager.SyncMessageHandler( op.sync.unitId, op.sync.timestamp, op.sync, op.sync.positions );
                    }
                    else if ( op.opType == OperationType.PlaceUnit )
                    {
						//Drag deployment logic locked.Dwayne 2017.9
                        //battleManager.SpawnUnitOnMap( op.playerId, op.unitMetaId, new Vector3( op.x, op.y, op.z ) );
						battleManager.SpawnUnitOnMap( op.playerId, op.unitMetaId );
                    }
                    else if ( op.opType == OperationType.ChangeDestination )
                    {
                        battleManager.ChangeForceDestination( op.playerId, op.unitId, new Vector3( op.x, op.y, op.z ) );
                    }
					else if ( op.opType == OperationType.PlaceBuilding ) 
					{
						battleManager.SpawnBuildingOnMap( op.playerId, ( BuildingType )op.unitMetaId, new Vector3( op.x, op.y, op.z ), ( int )op.unitId );
						DebugUtils.Log( DebugUtils.Type.Building, "The PlaceBuilding opration receive complete! BuildingPos X: " + op.x + "Y: " + op.y + "Z: " + op.z );
					}
					else if ( op.opType == OperationType.BuildingLevelUp ) 
					{
						battleManager.BuildingLevelUp( op.playerId , ( BuildingType )op.unitMetaId );
					}
					else if ( op.opType == OperationType.InistituteSkillLevelUp )
					{
						//op.instituteSkillUpLevels.upLevelTimes is skill upgrade number of times
						battleManager.InstituteSkillLevelUp( op.playerId, op.instituteSkillUpLevels.skillId, op.instituteSkillUpLevels.upLevelTimes );
					}
					else if ( op.opType == OperationType.RecylingTower )
					{
						battleManager.RecylingTower( op.playerId, op.unitId );
					}
                    else if ( op.opType == OperationType.MapAreaCollision )
                    {
                        battleManager.MapAreaCollision( op.playerId, op.unitId, op.collisionType, op.collisionState );
                    }
                    else if ( op.opType == OperationType.SyncSkillTargetPosition )
                    {
                        battleManager.SyncSkillTargetPosition( op.unitId, new Vector3( op.x, op.y, op.z ), op.targetId, op.unitMetaId );
                    }
                    else
                    {
                        DebugUtils.LogError( DebugUtils.Type.Battle, "Wrong operation is received : " + op.opType );
                    }
                }
            }

			if ( DataManager.GetInstance().GetBattleType() == BattleType.BattleP1vsP1 )
			{
				battleManager.PVPBattleUpdate();
			}
			else
			{
				battleManager.PVEBattleUpdate( DataManager.GetInstance().GetBattleType() );
			}
        }
        catch ( System.Exception e )
        {
            DebugUtils.LogOnScreen( e.ToString() );
            throw;
        }
    }

    void AgentSyncHandler( byte[] data )
    {
        DebugUtils.Assert( data != null );
        //SyncS2C message = ProtobufUtils.Deserialize<SyncS2C>( data );
    }

    void AgentNoticeHandler( byte[] data )
    {
        DebugUtils.Assert( data != null );
        NoticeS2C message = ProtobufUtils.Deserialize<NoticeS2C>( data );

        DebugUtils.Log( DebugUtils.Type.Battle, "receive notice message from server! type = " + message.type );

        switch( message.type )
        {
            case NoticeType.EnterBattle:
            {
                break;
            }
            case NoticeType.BattleBegin:
            {
                break;
            }
            case NoticeType.OperationReceived:
            {
                break;
            }
            case NoticeType.BattleResultRedWin:
            case NoticeType.BattleResultBlueWin:
            {
                if ( message.battleResult == null )
                {
                    //DebugUtils.LogError( DebugUtils.Type.Battle, " Null battle result when battle end");
                    return;
                }

                BattleResultData resultData = new BattleResultData();

                resultData.battleDuration = message.battleResult.battleDuration;
                resultData.redBattleSituations = message.battleResult.redBattleSituations;
                resultData.blueBattleSituations = message.battleResult.blueBattleSituations;
                resultData.gainGold = message.battleResult.gainGold;
                resultData.gainExp = message.battleResult.gainExp;
                resultData.currentExp = message.battleResult.currentExp;
                resultData.upLevelExp = message.battleResult.upLevelExp;
                resultData.playerLevel = message.battleResult.playerLevel;

                MessageDispatcher.PostMessage( MessageType.ShowBattleResultView, message.type, resultData );
                break;
            }
            case NoticeType.PlayerOffline:
            {
                break;
            }
            default:
                break;
        }
    }

    private void OnUploadSituationFeedBack( byte[] data )
    {
        DebugUtils.Assert( data != null );
        UploadSituationS2C message = ProtobufUtils.Deserialize<UploadSituationS2C>( data );
        
        if( (UploadSituationType)message.type == UploadSituationType.Upload )        // 1-upload situantion
        {
            DebugUtils.Log( DebugUtils.Type.Battle, "receive upload situation message from server! message.type = " + UploadSituationType.Upload );
        }
        else if ( (UploadSituationType)message.type == UploadSituationType.Request )  //2-request situation
        {
            OnNoticeRespond( message );
        }
        else if ((UploadSituationType)message.type == UploadSituationType.Result )   //3 - battle result
        {
            //unused
        }
    }

    private void OnPveBattleResultFeedBack(byte[] data)
    {
        PveBattleResultS2C message = ProtobufUtils.Deserialize<PveBattleResultS2C>( data );
        if (message.result)
        {
            DataManager manager = DataManager.GetInstance();

            int d = manager.GetPlayerFatality( manager.GetForceMark() );
            int k = manager.GetPlayerKillCount( manager.GetForceMark() );
            int z = manager.GetPlayerTotalResources( manager.GetForceMark() );

            BattleResultData resultData = new BattleResultData();
            resultData.battleDuration = manager.GetBattleDuration();

            resultData.redBattleSituations = new List<BattleSituation>();
            resultData.blueBattleSituations = new List<BattleSituation>();

            BattleSituation redSituation = new BattleSituation();
            redSituation.playerId = 0;     //AI's ID must be consistent with entering the battle
            redSituation.mvpValue = 0;
            redSituation.kills = d;
            redSituation.fatality = k;
            redSituation.resources = 0;

            BattleSituation blueSituation = new BattleSituation();
            blueSituation.playerId = manager.GetPlayerId();
            blueSituation.mvpValue = Formula.BattleMVPValue( z, k, d );
            blueSituation.kills = k;
            blueSituation.fatality = d;
            blueSituation.resources = z;

            resultData.redBattleSituations.Add( redSituation );
            resultData.blueBattleSituations.Add( blueSituation );

            resultData.gainGold = message.gainGold;
            resultData.gainExp = message.gainExp;
            resultData.currentExp = message.currentExp;
            resultData.upLevelExp = message.upLevelExp;
            resultData.playerLevel = message.playerLevel;

            NoticeType noticeType = DataManager.GetInstance().PveIsVictory ? NoticeType.BattleResultBlueWin : NoticeType.BattleResultRedWin;
            
            MessageDispatcher.PostMessage( MessageType.ShowBattleResultView, noticeType, resultData );
        }
    }

    private void QuitBattleFeedBack(byte[] data)
    {
        UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.Normal );

        BattleType battleType = DataManager.GetInstance().GetBattleType();

        switch (battleType)
        {
            case BattleType.BattleP2vsP2:
            {
                QuitBattleS2C message = ProtobufUtils.Deserialize<QuitBattleS2C>( data );

                if (message.playerIds == null || message.capitulates == null) return;

                //Maximum number of teammates
                int playerNumber = 2;

                Dictionary<long, SurrenderTips.Status> dic = new Dictionary<long, SurrenderTips.Status>();

                int length = playerNumber - message.playerIds.Count < 0 ? 0 : playerNumber - message.playerIds.Count;

                //patch:Sort, there are only two players so the following method to ensure that the first row in front of the offer.
                int index = 0;
                while (true)
                {
                    if(message.capitulates[index])
                    {
                        dic[message.playerIds[index]] = SurrenderTips.Status.Yes;
                        message.playerIds.RemoveAt( index );
                        message.capitulates.RemoveAt( index );
                        break;
                    }

                    index++;

                    if(index >= message.capitulates.Count)
                    {
                        break;
                    }
                }

                for (int i = 0; i < message.playerIds.Count; i++)
                {
                    dic[message.playerIds[i]] = message.capitulates[i] ? SurrenderTips.Status.Yes : SurrenderTips.Status.No;
                }

                for (int i = 0; i < length; i++)
                {
                    dic[-1] = SurrenderTips.Status.Wait;
                }

                MessageDispatcher.PostMessage( MessageType.SurrenderTipsHandler, dic );
                break;
            }
            case BattleType.Survival:
            case BattleType.Tranining:
            {
                // PVE request server battle result
                PVEBattleResultRequest( false );
                break;
            }
            default:
            {
                break;
            }
        }
    }

    public void SetSimUnitSkillEnable( long townId, bool enable )
    {
        battleManager.SetSimUnitSkillEnable( townId, enable );
    }

    // Used by Training mode
    public void ChangeSingleSoldierTarget( ForceMark mark, long id, Vector3 targetPosition, int pathMask )
    {
        battleManager.ChangeSingleSoldierTarget( mark, id, targetPosition, pathMask );
    }

    public long SpawnSoldierInMap( ForceMark mark, int unitMetaId, Vector3 bornPosition, BattleUnit unitInfo )
    {
       return battleManager.SpawnUnitOnMap( mark, unitMetaId, bornPosition, unitInfo );
    }

    // notice server that battle end
    private void BattleEnd( object obj )
    {
        DebugUtils.Log( DebugUtils.Type.Battle, "Battle Result message to server" );

        BattleType battleType = DataManager.GetInstance().GetBattleType();

        switch (battleType)
        {
            case BattleType.BattleP1vsP1:
            case BattleType.BattleP2vsP2:
            {
                //pvp
                DataManager clientData = DataManager.GetInstance();
                int d = clientData.GetPlayerFatality( clientData.GetForceMark() );
                int k = clientData.GetPlayerKillCount( clientData.GetForceMark() );
                int z = clientData.GetPlayerTotalResources( clientData.GetForceMark() );

                UploadSituationC2S result = new UploadSituationC2S();
                result.type = 1;
                result.battleSituation = new BattleSituation();
                result.battleSituation.playerId = clientData.GetPlayerId();
                result.battleSituation.fatality = d;
                result.battleSituation.kills = k;
                result.battleSituation.resources = z;
                result.battleSituation.mvpValue = Formula.BattleMVPValue( z, k, d );

                byte[] data = ProtobufUtils.Serialize( result );
                BattleMessageAgent.SendRequest( MsgCode.UploadSituationMessage, data );

                NoticeS2C noticeS2C = new NoticeS2C();
                noticeS2C.type = ( NoticeType )obj;
                byte[] noticeS2CData = ProtobufUtils.Serialize( noticeS2C );
                BattleMessageAgent.SendRequest( MsgCode.NoticeMessage, noticeS2CData );
            }
            break;
            case BattleType.Survival:
            case BattleType.Tranining:
            {
                // PVE request server battle result
                PVEBattleResultRequest( (NoticeType)obj == NoticeType.BattleResultBlueWin );
            }
            break;
        }
    }

    private void PVEBattleResultRequest(bool isWin)
    {
        PveBattleResultC2S msg = new PveBattleResultC2S();
        msg.isWin = DataManager.GetInstance().PveIsVictory = isWin;
        msg.attackNumber = DataManager.GetInstance().GetPveWaveNumber();

        byte[] data = ProtobufUtils.Serialize( msg );
        BattleMessageAgent.SendRequest( MsgCode.PveBattleResultMessage, data );
    }

    public void InitializeData()
    {
        // Init navmesh filter
        flyFilter = new NavMeshQueryFilter();
        flyFilter.agentTypeID = NavAgent.flySurface.agentTypeID;
        flyFilter.areaMask = NavMesh.AllAreas;

        groundFilter = new NavMeshQueryFilter();
        groundFilter.agentTypeID = NavAgent.groundSurface.agentTypeID;
        groundFilter.areaMask = NavMesh.AllAreas;

		//Locked building mode drag deployment code.Dwayne.
        //InitBuildingModeCheckBattleFieldObj();
    }

    private void OnBattleSituationRequest()
    {
        UploadSituationC2S msg = new UploadSituationC2S();
        msg.type = 2;
        byte[] data = ProtobufUtils.Serialize( msg );
        BattleMessageAgent.SendRequest( MsgCode.UploadSituationMessage, data );
    }

    private void OnNoticeRespond(UploadSituationS2C msg)
    {
        List<SettingFightingItemVo> redList = new List<SettingFightingItemVo>();
        List<SettingFightingItemVo> buleList = new List<SettingFightingItemVo>();

        DataManager manager = DataManager.GetInstance();
        
        BattleType battleType = manager.GetBattleType();

        switch (battleType)
        {
            case BattleType.BattleP1vsP1:
            case BattleType.BattleP2vsP2:
            {
                List<BattleSituation> redServerList = msg.redBattleSituations;
                List<BattleSituation> blueServerList = msg.blueBattleSituations;
                
                for (int i = 0; i < redServerList.Count; i++)
                {
                    string[] strs = GetMatcherDataById( redServerList[i].playerId );
                    SettingFightingItemVo vo = CreateSettingFightingItemVo( strs[0], redServerList[i].kills, redServerList[i].fatality, redServerList[i].resources, strs[1] , true , redServerList[i].playerId );
                    redList.Add( vo );
                }

                for (int i = 0; i < blueServerList.Count; i++)
                {
                    string[] strs = GetMatcherDataById( blueServerList[i].playerId );
                    SettingFightingItemVo vo = CreateSettingFightingItemVo( strs[0], blueServerList[i].kills, blueServerList[i].fatality, blueServerList[i].resources, strs[1] , false , blueServerList[i].playerId );
                    buleList.Add( vo );
                }
                break;
            }
			case BattleType.Tutorial: 
			{
				SettingFightingItemVo tutorialBlueVo = CreateSettingFightingItemVo( manager.GetPlayerNickName() , manager.GetPlayerKillCount( manager.GetForceMark() ) ,
					manager.GetPlayerFatality( manager.GetForceMark() ) , manager.GetPlayerTotalResources( manager.GetForceMark() ) , manager.GetPlayerHeadIcon() , false , manager.GetPlayerId() );
				buleList.Add( tutorialBlueVo );

				SettingFightingItemVo tutorialRedVo = CreateSettingFightingItemVo( "怠惰的教官" , manager.GetPlayerFatality( manager.GetForceMark() ) , manager.GetPlayerKillCount( manager.GetForceMark() ) ,
					0 , "EmberAvatar_10" , true ,0 );
				redList.Add( tutorialRedVo );

				break;
			}
            case BattleType.Survival:
            case BattleType.Tranining:
            {
                SettingFightingItemVo blueVo = CreateSettingFightingItemVo( manager.GetPlayerNickName() , manager.GetPlayerKillCount( manager.GetForceMark() ) ,
                    manager.GetPlayerFatality( manager.GetForceMark() ) , manager.GetPlayerTotalResources( manager.GetForceMark() ) , manager.GetPlayerHeadIcon() , false, manager.GetPlayerId() );
                buleList.Add( blueVo );

                //TODO : Later, the configuration table is read to replace the temporary AI data
                SettingFightingItemVo redVo = CreateSettingFightingItemVo( "疯狂的电脑" , manager.GetPlayerFatality( manager.GetForceMark() ) , manager.GetPlayerKillCount( manager.GetForceMark() ) ,
                    0 , "EmberAvatar_10" , true, 0 );
                redList.Add( redVo );
                break;
            }
        }

        MessageDispatcher.PostMessage( MessageType.BattleSituationResponse, redList, buleList );
    }

    private string[] GetMatcherDataById(long playerId)
    {
        Matcher data = DataManager.GetInstance().GetMatcherDataByID( playerId );
        string[] strs = new string[2];
        if (data != null)
        {
            strs[0] = data.name;
            strs[1] = data.portrait;
        }
        return strs;
    }

    private SettingFightingItemVo CreateSettingFightingItemVo(string name, int kills, int death, int res, string portrait , bool isRed , long playerId )
    {
        SettingFightingItemVo data = new SettingFightingItemVo();
        data.name = name;
        data.killCount = kills;
        data.deathCount = death;
        data.resourceCount = res;
        data.portrait = portrait;
        data.isRedSide = isRed;
        data.playerId = playerId;
        return data;
    }

    #region UI Operation
	private void GenerateUnitOnMap( object metaId, object buttonIndex )
    {
		battleManager.RequestSpawnUnitOnMap( (int)metaId, (int)buttonIndex );
    }

    //Drag deployment logic locked.Dwayne 2017.9
    //private void GenerateUnitOnMap( object metaId, object screenPoint )
    //{

    //Ray bornPoint = Camera.main.ScreenPointToRay( (Vector2)screenPoint );
    //RaycastHit hit;

    //bool canDeployUnitOnMap = true;

    //if ( Physics.Raycast ( bornPoint, out hit, 70f, 1 << bornAraeLayMask ))
    //{
    //Operation operation = new Operation();
    //operation.playerId = DataManager.GetInstance().GetPlayerId();
    //operation.opType = OperationType.PlaceUnit;
    //operation.unitMetaId = ( int )metaId;

    //Drag deployment logic locked.Dwayne 2017.9
    /*operation.x = hit.point.x;
        operation.y = hit.point.y;
        operation.z = hit.point.z;*/

    //UpdateC2S deployUnitMessage = new UpdateC2S();
    //deployUnitMessage.timestamp = DataManager.GetInstance().GetFrame();
    //deployUnitMessage.operation = operation;
    //byte [] data = ProtobufUtils.Serialize( deployUnitMessage );
    //BattleMessageAgent.SendRequest( MsgCode.UpdateMessage, data );

    //canDeployUnitOnMap = true;

    //DebugUtils.Log( DebugUtils.Type.Battle, string.Format( "Send a msg to place a {0}, at {1}", ( UnitType )metaId, hit.point ) );
    //} 
    //else
    //{
    //	canDeployUnitOnMap = false;
    //}
    //MessageDispatcher.PostMessage( MessageType.BattleUIOperationFeedBack, BattleUIOperationType.DeployUnitResult, canDeployUnitOnMap );
    // }

    // TODO: UI operation need to notice server,
    private void QuitBattleRequest(object obj)
    {
        QuitBattleC2S message = new QuitBattleC2S();
        message.capitulate = (bool)obj;

        byte[] data = ProtobufUtils.Serialize( message );
        BattleMessageAgent.SendRequest( MsgCode.QuitBattleMessage, data );

        DebugUtils.Log( DebugUtils.Type.Battle, string.Format( "{0} request quit battle", DataManager.GetInstance().GetForceMark() ) );
    }

    public void SelectObjects( object ownerId )
    {
        battleManager.SelectedUnit( ( long )ownerId, true );
    }

    public void SelectAllObjects()
    {
        battleManager.SelectAllUnits();
    }

    #endregion

	#region BattleFieldBuildingFunctions

	//Locked building mode drag deployment code.Dwayne.
	/*private void InitBuildingModeCheckBattleFieldObj()
	{
		GameObject buildingIterm = Resource.GameResourceLoadManager.GetInstance().LoadResourceSync( "Prefabs/Buildings/building_buildingItem");//TODO:When we have true prefab will change this to use resourcesID find.
		playerUsedCheckMapObj = buildingIterm.GetComponent<BuildingItem>();
		playerUsedCheckMapObj.gameObject.SetActive( false );

		int groundWalkableLayMask = LayerMask.NameToLayer( LayerName.LAYER_GROUNDWALKABLE );

		if( bornAraeLayMask == -1 )
		{
			bornAraeLayMask = LayerMask.NameToLayer( LayerName.LAYER_BORNAREA );
		}
			
		buildingLayMask = ( 1 << groundWalkableLayMask ) | ( 1 << bornAraeLayMask );
	}

	private void OpenCheckBattleFieldObj( object buildingType, object pos )
	{
		Ray setPoint = Camera.main.ScreenPointToRay( ( Vector2 )pos );
		RaycastHit hit;

		if( Physics.Raycast( setPoint, out hit ) )
		{
			BuildingType type = ( BuildingType ) buildingType;

			if( type == BuildingType.Tower )
			{
				playerUsedCheckMapObj.ChangeItemType( BuildingItem.BuildingItemType.Tower );
			}
			else if( type == BuildingType.Institute )
			{
				playerUsedCheckMapObj.ChangeItemType( BuildingItem.BuildingItemType.Institute );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Building, string.Format( "Unhandle building type {0}", type) );
			}

			if( playerUsedCheckMapObj != null )
			{
				playerUsedCheckMapObj.gameObject.SetActive( true );
				playerUsedCheckMapObj.transform.position = hit.point;
				DebugUtils.Log( DebugUtils.Type.Building, "PlayerUsedCheckMapObj is open!" );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Building, "PlayerUsedCheckMapObj is null!" );
			}
		}
		else
		{
			DebugUtils.Log( DebugUtils.Type.Building, "This area can not be build." );
		}
	}

	private void CheckBattleFieldObjPosChange( object pos )
	{
		Ray bornPoint = Camera.main.ScreenPointToRay( ( Vector2 )pos );
		RaycastHit hit;

		if( Physics.Raycast( bornPoint, out hit, 70f, buildingLayMask ))
		{
			if( playerUsedCheckMapObj != null )
			{
				playerUsedCheckMapObj.transform.localPosition = hit.point;

				if ( ( battleManager.CanDeployBuildingOnMap( battleManager.mark, hit.point ) || hit.transform.gameObject.layer == bornAraeLayMask ) && playerUsedCheckMapObj.isEnterTheCannotBuildObj == false )
				{
					canDeployBuildingOnMap = true;

					//save this pos to build something
					buildBuildingPos = hit.point;

					DebugUtils.Log( DebugUtils.Type.Building, "buildBuildingPos is " + buildBuildingPos );

					playerUsedCheckMapObj.selfStatus = BuildingItem.BuildCheckObjStatus.CanBuild;
				}
				else
				{
					canDeployBuildingOnMap = false;
					playerUsedCheckMapObj.selfStatus = BuildingItem.BuildCheckObjStatus.CanNotBuild;
				}
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Building, "PlayerUsedCheckMapObj cannot be null when dragging" );
			}
		}
	}

	private void CloseCheckBattleFieldObj( object buildingType )
	{
		BuildingType type = ( BuildingType )buildingType;

		if( canDeployBuildingOnMap && type == BuildingType.Tower )
		{
			MessageDispatcher.PostMessage( Constants.MessageType.OpenTowerPopUp, buildBuildingPos );
			DebugUtils.Log( DebugUtils.Type.Building, "Post MessageType.OpenTowerPopUp" );
		}
		else if( canDeployBuildingOnMap && type == BuildingType.Institute )
		{
			GenerateBuildingOnMap( -1, BuildingType.Institute );
		}

		MessageDispatcher.PostMessage( MessageType.BattleUIOperationFeedBack, BattleUIOperationType.DeployBuildingResult, canDeployBuildingOnMap );

		DebugUtils.Log( DebugUtils.Type.Building, string.Format("TryDepolyBuilding'result = {0}", canDeployBuildingOnMap ) );

		playerUsedCheckMapObj.gameObject.SetActive( false );
		canDeployBuildingOnMap = false;
	}*/

	private void GenerateBuildingOnMap( object id, object buildingType )
	{
		Operation operation = new Operation();
		operation.playerId = DataManager.GetInstance().GetPlayerId();
		operation.opType = OperationType.PlaceBuilding;
		operation.unitMetaId = (int)buildingType;

		if( ( BuildingType )buildingType == BuildingType.Tower )//Institute only one.but tower need check id.
		{
			operation.unitId = ( long )id;
		}

		if( buildBuildingPos != Vector3.zero )
		{
			operation.x = buildBuildingPos.x;
			operation.y = buildBuildingPos.y;
			operation.z = buildBuildingPos.z;
		}
		else
		{
			DebugUtils.LogError( DebugUtils.Type.Building, "The build position can not be vector3.zero!" );
		}

		UpdateC2S deployBuildingMessage = new UpdateC2S();
		deployBuildingMessage.timestamp = DataManager.GetInstance().GetFrame();
		deployBuildingMessage.operation = operation;
		byte [] data = ProtobufUtils.Serialize( deployBuildingMessage );
		BattleMessageAgent.SendRequest( MsgCode.UpdateMessage, data );

		DebugUtils.Log( DebugUtils.Type.Building, "The client GenerateBuildingOnMap message send complete! Used MsgCode.UpdateMessage." );
	}

	private void SendBuildingLevelUp( object buildingType, object playerID )
	{
		Operation operation = new Operation();
		operation.playerId = DataManager.GetInstance().GetPlayerId();
		operation.opType = OperationType.BuildingLevelUp;
		operation.unitMetaId = (int)buildingType;

		if( ( BuildingType )buildingType == BuildingType.Institute )
		{
			//TODO: Maybe need more logic
		}
		else
		{
			
		}
			
		UpdateC2S buildingLevelUp = new UpdateC2S();
		buildingLevelUp.timestamp = DataManager.GetInstance().GetFrame();
		buildingLevelUp.operation = operation;
		byte [] data = ProtobufUtils.Serialize( buildingLevelUp );
		BattleMessageAgent.SendRequest( MsgCode.UpdateMessage, data );

		DebugUtils.Log( DebugUtils.Type.Building, "The client buildingLevelUp message send complete! Used MsgCode.UpdateMessage." );
	}

	private void SendInstituteSkillLevelUp( object skillID, object times )
	{
		Operation operation = new Operation();
		operation.playerId = DataManager.GetInstance().GetPlayerId();
		operation.opType = OperationType.InistituteSkillLevelUp;
		InstituteSkillUpLevel skillData = new InstituteSkillUpLevel();
		skillData.skillId = ( int )skillID;
		skillData.upLevelTimes = ( int ) times;
		operation.instituteSkillUpLevels = skillData;

		UpdateC2S instituteSkillLevelUp = new UpdateC2S();
		instituteSkillLevelUp.timestamp = DataManager.GetInstance().GetFrame();
		instituteSkillLevelUp.operation = operation;
		byte [] data = ProtobufUtils.Serialize( instituteSkillLevelUp );
		BattleMessageAgent.SendRequest( MsgCode.UpdateMessage, data );

		DebugUtils.Log( DebugUtils.Type.InstitutesSkill, "The client instituteSkillLevelUp message send complete! Used MsgCode.UpdateMessage." );
	}

	public void RecylingTower( object towerID )
	{
		Operation operation = new Operation();
		operation.playerId = DataManager.GetInstance().GetPlayerId();
		operation.opType = OperationType.RecylingTower;
		operation.unitId = ( long )towerID;

		UpdateC2S recylingTowerMessage = new UpdateC2S();
		recylingTowerMessage.timestamp = DataManager.GetInstance().GetFrame();
		recylingTowerMessage.operation = operation;
		byte [] data = ProtobufUtils.Serialize( recylingTowerMessage );
		BattleMessageAgent.SendRequest( MsgCode.UpdateMessage, data );
	}

	//Locked building mode drag deployment code.Dwayne.
	/*private void ShowDeployBuildingAreaFromTower( object mark )
	{
		battleManager.ShowDeployBuildingAreaFromTower( (ForceMark)mark );
	}

	private void CloseDeployBuildingAreaFromTower( object mark )
	{
		battleManager.CloseDeployBuildingAreaFromTower( (ForceMark)mark );
	}*/
		
	#endregion

    #region Gesture Handler
    private void GestureOnMapHandler( object type, object phase, object obj )
    {
        GestureType gestureType = (GestureType)type;
        GestureState gesturePhase = (GestureState)phase;

        if ( gestureType == GestureType.SingleTapUnit )
        {
            RaycastHit hit = (RaycastHit)obj;
            SingleTapOnUnit( hit );
        }
        else if ( gestureType == GestureType.SingleTapFlyingWalkable )
        {
            RaycastHit hit = (RaycastHit)obj;
            SingleTapOnMap( hit, PathType.Flying );
        }
        else if ( gestureType == GestureType.SingleTapGroundWalkable )
        {
            RaycastHit hit = (RaycastHit)obj;
            SingleTapOnMap( hit, PathType.Ground );
        }
        else if ( gestureType == GestureType.DoubleTapMap )
        {
            RaycastHit hit = (RaycastHit)obj;
            DoubleTap( hit );
        }
        else if ( gestureType == GestureType.DragWithSingleFinger )
        {
            Vector2 hit = (Vector2)obj;
            DragOnMap( gesturePhase, hit );
        }
		else if( gestureType == GestureType.SingleTapInstituteBase )
		{
			RaycastHit hit = ( RaycastHit )obj;
			SingleTapInstituteBase( hit );
		}
		else if( gestureType == GestureType.SingleTapTowerBase )
		{
			RaycastHit hit = ( RaycastHit )obj;
			SingleTapTowerBase( hit );
		}
    }

    long lastChangeframe;
    private void SingleTapOnUnit( RaycastHit hit )
    {
        //Forbidden player do the tap operation in same frame twice. It will happened when one finger gesture and two finger gesture alternately occur.
        long frame = DataManager.GetInstance().GetFrame();
        if ( lastChangeframe == frame )
        {
            //DebugUtils.LogError( DebugUtils.Type.Gesture, " Can't tap on map at same frame twice, current operation will be ignored " );
            return;
        }

        lastChangeframe = frame;

        DebugUtils.Log( DebugUtils.Type.Gesture, string.Format( "Single Tap, hit obj name = {0}, hit layer = {1}  ,hit point = {2}, hit obj position = {3}",
                                                                     hit.transform.name, LayerMask.LayerToName( hit.transform.gameObject.layer ), hit.point, hit.transform.position ) );

        battleManager.SelectedUnit( hit.transform.gameObject, hit.point, false, true );
    }

	private long lastTapTowerframe;
	private void SingleTapTowerBase( RaycastHit hit )
	{
		long frame = DataManager.GetInstance().GetFrame();

		if ( lastTapTowerframe == frame )
		{
			return;
		}

		lastTapTowerframe = frame;

		buildBuildingPos = hit.collider.gameObject.transform.position;

		battleManager.TapTowerBase( buildBuildingPos );
	}

	private long lastTapInstituteframe;
	private void SingleTapInstituteBase( RaycastHit hit )
	{
		long frame = DataManager.GetInstance().GetFrame();

		if ( lastTapInstituteframe == frame )
		{
			return;
		}

		lastTapInstituteframe = frame;

		buildBuildingPos = hit.collider.gameObject.transform.position;

		battleManager.TapInstituteBase( buildBuildingPos );
	}

    long lastGroundTargetChangeframe;
    long lastFlyingTargetChangeframe;
    private void SingleTapOnMap( RaycastHit hit, PathType pathType )
    {
        //Forbidden player do the tap operation in same frame twice. It will happened when one finger gesture and two finger gesture alternately occur.
        long frame = DataManager.GetInstance().GetFrame();
        if ( pathType == PathType.Ground )
        {
            if ( lastGroundTargetChangeframe == frame )
            {
                return;
            }

            lastGroundTargetChangeframe = frame;
        }
        else if ( pathType == PathType.Flying )
        {
            if ( lastFlyingTargetChangeframe == frame )
            {
                return;
            }

            lastFlyingTargetChangeframe = frame;
        }
        
        DebugUtils.Log( DebugUtils.Type.Gesture, string.Format( "Single Tap, hit obj name = {0}, hit layer = {1} ,hit point = {2}, hit obj position = {3} pathType = {4}", hit.transform.name, LayerMask.LayerToName( hit.transform.gameObject.layer ), hit.point, hit.transform.position, pathType ) );
        battleManager.TapGround( hit.point, pathType );
    }

    private void DoubleTap( RaycastHit hit )
    {
        if ( hit.transform == null )
        {
            return;
        }

        DebugUtils.Log( DebugUtils.Type.Gesture, string.Format( "Double Tap, hit obj name = {0}, hit layer = {1}  ,hit point = {2}, hit obj position = {3}",
                                                                             hit.transform.name, LayerMask.LayerToName( hit.transform.gameObject.layer ), hit.point, hit.transform.position ) );
        if ( hit.transform.gameObject.layer == LayerMask.NameToLayer( LayerName.LAYER_UNIT ) )
        {
            battleManager.DoubleTapOperationOnUnit( hit.transform.gameObject );
        }
    }

    private long currentDragUnitId = -2;
    // TODO: add more different operation 
    private void DragOnMap( GestureState gesturePhase, Vector2 hit )
    {
        if ( gesturePhase == GestureState.Started )
        {
            dragStartScreenPoint = hit;
            dragScreenRectStartPosition = dragStartScreenPoint;
            
            Ray beginPointRay = Camera.main.ScreenPointToRay( dragStartScreenPoint );
            RaycastHit beginRaycastHit;
            if ( Physics.Raycast( beginPointRay, out beginRaycastHit ) )
            {
                // drag start at a soldier or other unit, will move the unit.
                // drag start at other layer will begin to draw the rectangle on screen
                DebugUtils.Log( DebugUtils.Type.Map, string.Format( " Hit soldier Point = {0}", beginRaycastHit.transform.position ) );

                if ( beginRaycastHit.transform.gameObject.layer == LayerMask.NameToLayer( LayerName.LAYER_UNIT ) )
                {
                    currentDragUnitId = battleManager.UnitCanBeDrag( beginRaycastHit.transform.gameObject );

                    if ( currentDragUnitId != -2 )
                    {
                        drawingRectOnScreen = false;

                        startDragUnitOnGround = true;
                        startDragUnitOnFlying = true;

                        groundPathPointTotalCount = 0;
                        flyingPathPointTotalCount = 0;

                        dragOnGroundPointList.Clear();// will not add path point in this hit, because this ray was hit on model! not map
                        dragOnFlyingPointList.Clear();

                        battleManager.ClearUnitsPath();
                    }
                    else
                    {
                        drawingRectOnScreen = true;

                        //dragStartWorldPoint = new Vector2( beginRaycastHit.transform.position.x, beginRaycastHit.transform.position.z );
                        MessageDispatcher.PostMessage( MessageType.DragFingerToMoveCamera, gesturePhase, hit );
                    }
                }
                else
                {
                    drawingRectOnScreen = true;

                    //dragStartWorldPoint = new Vector2( beginRaycastHit.transform.position.x, beginRaycastHit.transform.position.z );
                    MessageDispatcher.PostMessage( MessageType.DragFingerToMoveCamera, gesturePhase, hit );
                }

                lastDragOnGroundPoint = beginRaycastHit.transform.position;
                lastDragOnFlyingPoint = beginRaycastHit.transform.position;
            }
            else
            {
                MessageDispatcher.PostMessage( MessageType.DragFingerToMoveCamera, gesturePhase, hit );
            }
        }
        else if ( gesturePhase == GestureState.Updated )
        {
            dragEndScreenPoint = hit;

            if ( drawingRectOnScreen )
            {
                MessageDispatcher.PostMessage( MessageType.DragFingerToMoveCamera, gesturePhase, hit );
            }
            else
            {
                Ray endPointRay = Camera.main.ScreenPointToRay( dragEndScreenPoint );
                RaycastHit endRaycastHit;

                // when drag on unit - chase current unit 
                bool dragOnUnitToChase = false;
                if ( Physics.Raycast( endPointRay, out endRaycastHit, raycastDistance, ( 1 << LayerMask.NameToLayer( LayerName.LAYER_UNIT ) ) ) )
                {
                    if ( battleManager.UnitCanBeChase( endRaycastHit.transform.gameObject ) )
                    {
                        dragOnUnitToChase = true;
                        DragUnitEnd( PathType.Ground, currentDragUnitId, dragOnGroundPointList, groundPathPointTotalCount, startDragUnitOnGround );
                        battleManager.ChangeUnitTarget( currentDragUnitId, endRaycastHit.transform.gameObject );
                    }
                }

                // drag on map - Ground 
                if ( startDragUnitOnGround )
                {
                    if ( !dragOnUnitToChase )
                    {
                        if ( Physics.Raycast( endPointRay, out endRaycastHit, raycastDistance, ( 1 << LayerMask.NameToLayer( LayerName.LAYER_GROUNDWALKABLE ) ) ) )
                        {
                            if ( !DraggingUnit( PathType.Ground, currentDragUnitId, endRaycastHit.point, lastDragOnGroundPoint, dragOnGroundDistance, dragOnGroundPointList, groundPathPointTotalCount ) )
                            {
                                // dragging meet a gap 
                                DragUnitEnd( PathType.Ground, currentDragUnitId, dragOnGroundPointList, groundPathPointTotalCount, startDragUnitOnGround );
                            }
                        }
                        else
                        {
                            if ( Physics.Raycast( endPointRay, out endRaycastHit ) )
                            {
                                // hit on something( rock, map, obstacle...) will cancel drag
                                if ( endRaycastHit.transform.gameObject.layer != LayerMask.NameToLayer( LayerName.LAYER_UNIT ) )
                                {
                                    DebugUtils.Log( DebugUtils.Type.InputPathPoint, "Dragging to a obstacle, will cancel drag operation on Ground! " );

                                    DragUnitEnd( PathType.Ground, currentDragUnitId, dragOnGroundPointList, groundPathPointTotalCount, startDragUnitOnGround );
                                }
                                else
                                {
                                    // hit on Unit.. continue
                                }
                            }
                            else
                            {
                                DebugUtils.Log( DebugUtils.Type.InputPathPoint, "hit nothing will cancel drag operation on Ground! " );

                                DragUnitEnd( PathType.Ground, currentDragUnitId, dragOnGroundPointList, groundPathPointTotalCount, startDragUnitOnGround );
                            }
                        }
                    }
                }

                // Drag on map - Flying
                if ( startDragUnitOnFlying )
                {
                    if ( !dragOnUnitToChase )
                    {
                        if ( Physics.Raycast( endPointRay, out endRaycastHit, raycastDistance, ( 1 << LayerMask.NameToLayer( LayerName.LAYER_FLYINGWALKABLE ) ) ) )
                        {
                            if ( !DraggingUnit( PathType.Flying, currentDragUnitId, endRaycastHit.point, lastDragOnFlyingPoint, dragOnFlyingDistance, dragOnFlyingPointList, flyingPathPointTotalCount ) )
                            {
                                DragUnitEnd( PathType.Flying, currentDragUnitId, dragOnFlyingPointList, flyingPathPointTotalCount, startDragUnitOnFlying );
                            }
                        }
                        else
                        {
                            if ( Physics.Raycast( endPointRay, out endRaycastHit ) )
                            {
                                // hit on something( rock, map, obstacle...) will cancel drag
                                if ( endRaycastHit.transform.gameObject.layer != LayerMask.NameToLayer( LayerName.LAYER_UNIT ) )
                                {
                                    DebugUtils.Log( DebugUtils.Type.InputPathPoint, "Dragging to a obstacle, will cancel drag operation on FlyingWalkable! " );

                                    DragUnitEnd( PathType.Flying, currentDragUnitId, dragOnFlyingPointList, flyingPathPointTotalCount, startDragUnitOnFlying );
                                }
                                else
                                {
                                    // hit on Unit.. continue
                                }
                            }
                            else
                            {
                                DebugUtils.Log( DebugUtils.Type.InputPathPoint, "hit nothing will cancel drag operation on FlyingWalkable! " );

                                DragUnitEnd( PathType.Flying, currentDragUnitId, dragOnFlyingPointList, flyingPathPointTotalCount, startDragUnitOnFlying );
                            }
                        }
                    }
                }
            }

            dragScreenRectEndPosition = dragEndScreenPoint;
        }
        else if ( gesturePhase == GestureState.Ended )
        {
            if ( drawingRectOnScreen )
            {
                MessageDispatcher.PostMessage( MessageType.DragFingerToMoveCamera, gesturePhase, hit );
            }
            else
            {
                if ( startDragUnitOnGround )
                {
                    DragUnitEnd( PathType.Ground, currentDragUnitId, dragOnGroundPointList, groundPathPointTotalCount, startDragUnitOnGround );
                    startDragUnitOnGround = true;
                }

                if ( startDragUnitOnFlying )
                {
                    DragUnitEnd( PathType.Flying, currentDragUnitId, dragOnFlyingPointList, flyingPathPointTotalCount, startDragUnitOnFlying );
                    startDragUnitOnFlying = false;
                }
            }

            currentDragUnitId = -2;
            dragStartScreenPoint = Vector2.zero;
            dragEndScreenPoint = Vector2.zero;
        }
    }

    private bool DraggingUnit( PathType pathType, long unitId, Vector3 position, Vector3 lastPosition, float distance, List<Vector3> pointList, int totalPointCount  )
    {
        // two point max height difference must lower than maxHeightDifference
        if ( Mathf.Abs( position.y - lastPosition.y ) > maxHeightDifference )
        {
            return false;
        }

        // two point's mid point must can be found on special navmesh
        NavMeshQueryFilter filter;
        if ( pathType == PathType.Ground )
        {
            filter = groundFilter;
        }
        else if ( pathType == PathType.Flying )
        {
            filter = flyFilter;
        }
        else
        {
            DebugUtils.LogError( DebugUtils.Type.InputPathPoint, "Can't handler this path type now" );
            return false;
        }

        float x = ( position.x + lastPosition.x ) / 2;
        float y = ( position.y + lastPosition.y ) / 2;
        float z = ( position.z + lastPosition.z ) / 2;

        Vector3 sample = new Vector3( x, y, z );

        NavMeshHit hit;
        if ( !NavMesh.SamplePosition( sample, out hit, 1, filter ) ) // 1 is a test value
        {
            DebugUtils.Log( DebugUtils.Type.InputPathPoint, string.Format( "The midpoint {0} can't be found on special navMesh", sample ) );
            return false;
        }

        distance = distance + Vector3.Distance( lastPosition, position );

        // move distance need to more than 1 (test value)
        if ( distance >= 1 )
        {
            totalPointCount++;
            pointList.Add( position );
            battleManager.DrawUnitsPathPoint( unitId, position, pathType );

            DebugUtils.LogWarning( DebugUtils.Type.InputPathPoint, string.Format( "Add Point = {0}, index = {1}, pointCount = {2}, distance = {3}", position, totalPointCount, pointList.Count, distance ) );

            if ( pointList.Count == 5 )
            {
                DebugUtils.Log( DebugUtils.Type.InputPathPoint, string.Format( "Send path at frame = {2}, index = {0}, pointCount = {1}", totalPointCount, pointList.Count, DataManager.GetInstance().GetFrame() ) );

                battleManager.ExcutePathPoint( pointList, unitId, pathType, false );
                pointList.Clear();
            }

            distance = 0;
        }

        // save info
        if ( pathType == PathType.Ground )
        {
            lastDragOnGroundPoint = position;
            dragOnGroundDistance = distance;
            groundPathPointTotalCount = totalPointCount;
        }
        else if ( pathType == PathType.Flying )
        {
            lastDragOnFlyingPoint = position;
            dragOnFlyingDistance = distance;
            flyingPathPointTotalCount = totalPointCount;
        }

        return true;
    }

    private void DragUnitEnd( PathType pathType, long unitId, List<Vector3> pointList, int totalCount, bool draggingUnit )
    {
        // send out the rest of path point
        if ( pointList.Count > 0 )
        {
            DebugUtils.Log( DebugUtils.Type.InputPathPoint, string.Format( "Send last path at frame = {2}, index = {0}, pointCount = {1}", totalCount, pointList.Count, DataManager.GetInstance().GetFrame() ) );

            battleManager.ExcutePathPoint( pointList, unitId, pathType, true );
            pointList.Clear();
        }
        else
        {
            if ( draggingUnit && totalCount > 0 )
            {
                DebugUtils.Log( DebugUtils.Type.InputPathPoint, string.Format( "Send last path at frame = {2}, index = {0}, pointCount = {1}", totalCount, pointList.Count, DataManager.GetInstance().GetFrame() ) );

                battleManager.ExcutePathPoint( pointList, unitId, pathType, true );
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.InputPathPoint, string.Format( "Didn't send last path, total  = {0}, startDragUnit = {1}", totalCount, draggingUnit ) );
            }
        }

        if ( pathType == PathType.Ground )
        {
            startDragUnitOnGround = false;

            DebugUtils.Log( DebugUtils.Type.InputPathPoint, "Drag on Ground End");
        }
        else if ( pathType == PathType.Flying )
        {
            startDragUnitOnFlying = false;

            DebugUtils.Log( DebugUtils.Type.InputPathPoint, "Drag on Flying End" );
        }
    }
    #endregion

    #region Playback
    private void SetBattleRuntimeSpeed( object speedObj )
    {
        int speed = (int)speedObj;

        DebugUtils.Log( DebugUtils.Type.Playback, string.Format( " Modified play speed:{0}", speed ) );


        BattleMessageAgent.SetBattleSpeed( speed );
    }

    private void SetBattleRecordPlayingState( object state )
    {
        BattleMessageAgent.SetBattleRecordPlayState( (int)state );
    }

    private void RandomSetCameraTarget( object markObj )
    {
        ForceMark mark = (ForceMark)markObj;
        battleManager.RandomSetCameraTarget( mark );
    }
    #endregion
}
