/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: BattleManager.cs
// description: 
// 
// created time：09/28/2016
//
//----------------------------------------------------------------*/


using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

using Utils;
using Constants;
using Data;
using Logic;
using Render;
using Network;
using Map;
using PVE;
using BattleAgent;
using Resource;
using UI;

using Structure = Data.StructuresProto.Structure;
using EndlessBehavior = Data.EndlessBehaviorProto.EndlessBehavior;
using TrainingBehavior = Data.TrainingBehaviorProto.TrainingBehavior;
using BattleUnit = Data.Battler.BattleUnit;

public class BattleManager : MonoBehaviour
{
    private long id;

    public long Id
    {
        get
        {
            return id;
        }
    }

    public BattleType BattleType
    {
        get
        {
            return battleType;
        }
    }

    private BattleType battleType;

    private LogicWorld logicWorld;
    private RenderWorld renderWorld;
	private MapDataPVE mapDataPVE;
	private MapData1V1 mapData1V1;
	private MapData2V2 mapData2V2;
	public ForceMark mark;

    private int timer = 0;
    private int lastUpdateTime = 0;
    private int logicTimer = 0;
    private NavigateCamera mainCamera;
	private TrainingModeManager trainingModeManager;
	private EndlessModeManager endlessModeManager;
	private TutorialModeManager tutorialModeManager;

    public List<Battler> battlerList;

    private int deltaTime = (int)( GameConstants.LOGIC_FRAME_TIME * 1000 ); // s to ms

	private DataManager dataManager;

    void Awake()
	{
		if( dataManager == null )
		{
			dataManager = DataManager.GetInstance();
		}

        GameObject.Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "HealthbarControl" ) );
        logicWorld = new LogicWorld ();
		renderWorld = new RenderWorld ();
        MessageDispatcher.AddObserver( InitMap, MessageType.LoadBattleComplete );
		MessageDispatcher.AddObserver( MapShowEffectStart, MessageType.MapShowEffectStart );
    }

    void OnDestroy()
    {
        MessageDispatcher.RemoveObserver( InitMap, MessageType.LoadBattleComplete );
		MessageDispatcher.RemoveObserver( MapShowEffectStart, MessageType.MapShowEffectStart );

		if( endlessModeManager != null )
		{
			endlessModeManager.OnDestroy();
			endlessModeManager = null;
		}
		else if( trainingModeManager != null )
		{
			trainingModeManager.OnDestroy();
			trainingModeManager = null;
		}
		else if( tutorialModeManager != null )
		{
			tutorialModeManager.OnDestroy();
			tutorialModeManager = null;
		}
			
        logicWorld.Release();
        logicWorld = null;

        renderWorld.Release();
        renderWorld = null;
    }

    private void InitMap( object map )
    {
        GameObject battleMap = null;
        if( map == null )
        {
            battleMap = GameObject.Find( "BattleMap" );
        }
        else
        {
            battleMap = Instantiate( (GameObject)map );
        }

        DataManager clientData = DataManager.GetInstance();
        id = clientData.GetBattleId();
        battleType = clientData.GetBattleType();

        clientData.SetBattleStartTime();

        PathAgent.Initialize();
        InputHandler.Instance.InitializeData();

        logicWorld.RegisterRenderMessageHandler( HandleRenderMessage );
        logicWorld.RegisterAgentMessageHandler( HandleAgentMessage );

		mark = clientData.GetForceMark();

        HealthbarControl.Instance.SetWorldCamera( Camera.main );

        // TODO: Temp code, EnterBattle must be posted in InputHandler, and after get the matchlist from server
        UI.UIManager.Instance.EnterBattleMenu( () =>
        {
            ApplyBattleMode( battleMap );
            SetUpBattle();
        } );
    }

	//3DMap show effect will start( Now use for base show Start particle and animator )
	private void MapShowEffectStart()
	{
		foreach( KeyValuePair<long, TownRender> townRenders in renderWorld.townRenders )
		{
			townRenders.Value.GameStart();
		}
	}

    private void SetUpBattle()
    {
        battlerList = dataManager.GetBattlers();

        if ( battleType == BattleType.BattleP1vsP1 )
        {
			for ( int i = 0; i < battlerList.Count; i++ )
			{
				logicWorld.SetupForce( battlerList[i], mapData1V1.GetTownPosition( battlerList[i].forceMark ), mapData1V1.GetFormationPoint( battlerList[i].forceMark ) );
			}

			/*List<Transform> smallCrystals = mapData1V1.GetSmallCrystalsList();
			for ( int i = 0; i < smallCrystals.Count; i++ )
			{
				logicWorld.SetupCrystal( smallCrystals[i].position, smallCrystals[i].rotation, false );
			}*/

			List<Transform> bigCrystals = mapData1V1.GetBigCrystalsList();
			for ( int i = 0; i < bigCrystals.Count; i++ )
			{
				logicWorld.SetupCrystal( bigCrystals[i].position, bigCrystals[i].rotation, true );
			}

            List<Transform> wildMonsters = mapData1V1.GetWildMonsterPosition();
            for ( int i = 0; i < wildMonsters.Count; i++ )
            {
                logicWorld.SetupWildMonster( wildMonsters[i].position, wildMonsters[i].rotation );
            }

            List<Transform> idolGuards = mapData1V1.GetIdolGuardPosition();
            for ( int i = 0; i < idolGuards.Count; i++ )
            {
                logicWorld.SetupIdolGuard( idolGuards[i].position, idolGuards[i].rotation );
            }

            Transform idolPosition = mapData1V1.GetIdolPosition();
            logicWorld.SetupIdol( idolPosition.position, idolPosition.rotation );
        }
		else if(  battleType == BattleType.Tranining )
		{
			for ( int i = 0; i < battlerList.Count; i++ )
			{
				logicWorld.SetupForce( battlerList[i], mapDataPVE.GetTownPosition( battlerList[i].forceMark ), mapDataPVE.GetFormationPoint( battlerList[i].forceMark ) );
			}

			logicWorld.SetupCrystal( mapDataPVE.topCrystal.position, mapDataPVE.topCrystal.rotation, true );
			logicWorld.SetupCrystal( mapDataPVE.bottomCrystal.position, mapDataPVE.bottomCrystal.rotation, true );
			//logicWorld.SpawnPowerup( logicWorld.GetPowerUpType(), mapData.powerUp.position );
			logicWorld.powerUpsBornPos = mapDataPVE.powerUp.position;
		}
		else if ( battleType == BattleType.Tutorial )
		{
			for ( int i = 0; i < battlerList.Count; i++ )
			{
				logicWorld.SetupForce( battlerList[i], mapData1V1.GetTownPosition( battlerList[i].forceMark ), mapData1V1.GetFormationPoint( battlerList[i].forceMark ) );
			}

			/*List<Transform> smallCrystals = mapData1V1.GetSmallCrystalsList();
			for ( int i = 0; i < smallCrystals.Count; i++ )
			{
				logicWorld.SetupCrystal( smallCrystals[i].position, smallCrystals[i].rotation, false );
			}*/

			List<Transform> bigCrystals = mapData1V1.GetBigCrystalsList();
			for ( int i = 0; i < bigCrystals.Count; i++ )
			{
				logicWorld.SetupCrystal( bigCrystals[i].position, bigCrystals[i].rotation, true );
			}
				
			List<Transform> wildMonsters = mapData1V1.GetWildMonsterPosition();
			for ( int i = 0; i < wildMonsters.Count; i++ )
			{
				logicWorld.SetupWildMonster( wildMonsters[i].position, wildMonsters[i].rotation );
			}

			List<Transform> idolGuards = mapData1V1.GetIdolGuardPosition();
			for ( int i = 0; i < idolGuards.Count; i++ )
			{
				logicWorld.SetupIdolGuard( idolGuards[i].position, idolGuards[i].rotation );
			}

			Transform idolPosition = mapData1V1.GetIdolPosition();
			logicWorld.SetupIdol( idolPosition.position, idolPosition.rotation );
		}
        else if ( battleType == BattleType.Survival )
        {
            for ( int i = 0; i < battlerList.Count; i++ )
            {
				logicWorld.SetupForce( battlerList[i], mapDataPVE.GetTownPosition( battlerList[i].forceMark ), mapDataPVE.GetFormationPoint( battlerList[i].forceMark ) );
            }
        }
        else if ( battleType == BattleType.BattleP2vsP2 )
        {
            for ( int i = 0; i < battlerList.Count; i++ )
            {
				logicWorld.SetupForce( battlerList[i], mapData2V2.GetTownPosition( battlerList[i].forceMark ), mapData2V2.GetFormationPoint( battlerList[i].forceMark ) );
            }

            /*List<Transform> smallCrystals = mapData2V2.GetSmallCrystalsList();
            for ( int i = 0; i < smallCrystals.Count; i++ )
            {
                logicWorld.SetupCrystal( smallCrystals[i].position, smallCrystals[i].rotation, false );
            }*/

            List<Transform> bigCrystals = mapData2V2.GetBigCrystalsList();
            for ( int i = 0; i < bigCrystals.Count; i++ )
            {
                logicWorld.SetupCrystal( bigCrystals[i].position, bigCrystals[i].rotation, true );
            }

            // TODO: Comment for now, wait for New model
            List<Transform> wildMonsters = mapData2V2.GetWildMonsterPosition();
            for ( int i = 0; i < wildMonsters.Count; i++ )
            {
                logicWorld.SetupWildMonster( wildMonsters[i].position, wildMonsters[i].rotation );
            }

            List<Transform> idolGuards = mapData2V2.GetIdolGuardPosition();
            for ( int i = 0; i < idolGuards.Count; i++ )
            {
                logicWorld.SetupIdolGuard( idolGuards[i].position, idolGuards[i].rotation );
            }

            Transform idolPosition = mapData2V2.GetIdolPosition();
            logicWorld.SetupIdol( idolPosition.position, idolPosition.rotation );
        }

		MapShowEffectStart();
        MessageDispatcher.PostMessage( MessageType.EnterBattle, battlerList );
    }

    public void PVPBattleUpdate()
    {
        timer += deltaTime;

        //if( timer + Time.deltaTime * 0.5f > lastUpdateTime + GameSetting.LogicFrameTime )
        {
            logicWorld.Update( deltaTime );
            logicTimer += deltaTime;
            lastUpdateTime = timer;
        }
    }

    
	public void PVEBattleUpdate( BattleType type )
	{
		logicWorld.Update( deltaTime );

		if( type == BattleType.Tranining )
		{
			trainingModeManager.TrainingModeUpdate();
		}
		else if( type == BattleType.Survival )
		{
			endlessModeManager.EndlessModeUpdate();
		}
		else if( type == BattleType.Tutorial )
		{
			tutorialModeManager.TutorialModeManagerUpdate();
		}
	}

    private void HandleRenderMessage( RenderMessage rm )
    {
        //DebugUtils.Log( DebugUtils.Type.Battle, string.Format( "HandleRenderMessage: type = {0}, ownerId = {1}!", rm.type, rm.ownerId ) );

        if( rm.type == RenderMessage.Type.TimeChanged )
        {
            float leftTime = Convert.ToSingle( rm.arguments["value"] );
            MessageDispatcher.PostMessage( MessageType.BattleTimeChanged, leftTime );
        }
        else if( rm.type == RenderMessage.Type.SpawnTown )
        {
            ForceMark mark = (ForceMark)rm.arguments["mark"];
            int modelId = Convert.ToInt32( rm.arguments["model"]);
            int hp = Convert.ToInt32( rm.arguments["hp"] );
            renderWorld.SetupTown( modelId, mark, rm.ownerId, hp, rm.position );

			if( battleType == BattleType.Survival && mark == ForceMark.TopRedForce )
			{
				DebugUtils.Log( DebugUtils.Type.Battle, "Survival Mode not need AI Base Render" );
			}
			else
			{
                MessageDispatcher.PostMessage( MessageType.BuildTown, mark, rm.position );
            }
        }
        else if( rm.type == RenderMessage.Type.SpawnTower )
        {
            ForceMark mark = (ForceMark)rm.arguments["mark"];
			int modelID = ( int )rm.arguments["model"];
            int hp = Convert.ToInt32( rm.arguments["hp"] );
			float buildTime = Convert.ToSingle( rm.arguments[ "buildTime" ] );
			renderWorld.SetupTower( modelID, mark, buildTime, rm.ownerId, hp, rm.position );
				
            MessageDispatcher.PostMessage( MessageType.BuildTower, mark, rm.ownerId, rm.position );
        }
		else if( rm.type == RenderMessage.Type.SpawnInstitute )
		{
			ForceMark mark = ( ForceMark )rm.arguments["mark"];
			int modeID = ( int )rm.arguments["modelId"];
			object instituteSkills = rm.arguments["instituteSkillsData"];
			int hp = Convert.ToInt32( rm.arguments["hp"] );
			float buildTime = Convert.ToSingle( rm.arguments[ "buildTime" ] );

			renderWorld.SetupInstitute( modeID, mark, buildTime, rm.ownerId, hp, rm.position );

			MessageDispatcher.PostMessage( MessageType.BuildInstitute, mark, rm.ownerId, rm.position, instituteSkills );
		}
        else if( rm.type == RenderMessage.Type.SpawnSoldier )
        {
            ForceMark mark = (ForceMark)rm.arguments["mark"];
            UnitType type = (UnitType)rm.arguments["type"];
            int metaId = (int)rm.arguments["metaId"];
            int hp = Convert.ToInt32( rm.arguments["hp"] );

            renderWorld.GenerateSoldier( metaId, mark, type, rm.ownerId, hp, rm.position );

            if ( mark == DataManager.GetInstance().GetForceMark() )
            {
                SoundManager.Instance.PlaySound( GameConstants.SOUND_SPAWNUNIT_ID );
            }
            
            MessageDispatcher.PostMessage( MessageType.GenerateSoldier, mark, rm.ownerId, rm.position );
        }
        else if( rm.type == RenderMessage.Type.SpawnProjectile )
        {
            ForceMark mark = (ForceMark)Convert.ToInt32( rm.arguments["mark"] );
            int metaId = Convert.ToInt32( rm.arguments["metaId"] );
            UnitRenderType unitRenderType = (UnitRenderType)rm.arguments["holderType"];
            long holderId = Convert.ToInt64( rm.arguments["holderId"] );
            renderWorld.GenerateProjectile( mark, rm.ownerId, metaId, rm.position, rm.direction, unitRenderType, holderId );
        }
        else if( rm.type == RenderMessage.Type.SpawnCrystal )
        {
            int modelId = Convert.ToInt32( rm.arguments["modelId"] );
            bool plus = Convert.ToBoolean( rm.arguments["plus"] );
            int hp = Convert.ToInt32( rm.arguments["hp"] );
            renderWorld.SetupCrystal( modelId, rm.ownerId, plus, hp, rm.position, rm.direction );
        }
        else if( rm.type == RenderMessage.Type.SpawnCrystalCar )
        {
            ForceMark mark = (ForceMark)rm.arguments["mark"];
            int modelId = Convert.ToInt32(rm.arguments["modelId"] );
            int hp = Convert.ToInt32( rm.arguments["hp"] );
            renderWorld.SetupCrystalCar( modelId, mark, rm.ownerId, hp, rm.position );

            MessageDispatcher.PostMessage( MessageType.BuildTramCar, mark, rm.ownerId, rm.position );
        }
        else if( rm.type == RenderMessage.Type.SpawnDemolisher )
        {
            ForceMark mark = (ForceMark)rm.arguments["mark"];
            int modelId = Convert.ToInt32( rm.arguments["modelId"]);
            int hp = Convert.ToInt32( rm.arguments["hp"] );
            renderWorld.SetupDemolisher( modelId, mark, rm.ownerId, hp, rm.position );

            MessageDispatcher.PostMessage( MessageType.BuildDemolisher, mark, rm.ownerId, rm.position );
        }
        else if( rm.type == RenderMessage.Type.CoinChanged )
        {
            //TODO:add coin changed
            if( rm.ownerId == Data.DataManager.GetInstance().GetPlayerId() )
            {
                int money = (int)rm.arguments["value"];
                MessageDispatcher.PostMessage( MessageType.CoinChanged, money );
            }
        }
        //else if ( rm.type == RenderMessage.Type.SpawnPowerUp )
        //{
        //    int modelId = (int)rm.arguments["modelId"];
        //    PowerUpType powerUpType = (PowerUpType)rm.arguments["powerUpType"];
        //    ForceMark mark = (ForceMark)rm.arguments["mark"];
        //    float x = (float)rm.arguments["x"];
        //    float y = (float)rm.arguments["y"];
        //    float z = (float)rm.arguments["z"];

        //    renderWorld.SetUpPowerUp( modelId, powerUpType, mark, rm.ownerId, x, y, z );
        //    MessageDispatcher.PostMessage( MessageType.SpawnPowerUp, rm.ownerId, (int)powerUpType, new Vector3( x, y, z ) );
        //}
        else
        {
            renderWorld.HandleRenderMessage( rm );
        }
    }

    public void HandleAgentMessage( MsgCode protocolCode, object obj )
    { 
        byte[] data = ProtobufUtils.Serialize( obj );
        BattleMessageAgent.SendRequest( protocolCode, data );
    }

    public void SyncMessageHandler( long unitId, long frame, Sync sync, List<Position> positions )
    {
        logicWorld.SyncMessageHandler( unitId, frame, sync, positions );
    }

    public void ApplyBattleMode( GameObject battleMap )
    {
        DataManager clientData = DataManager.GetInstance();
        BattleType type = clientData.GetBattleType();
        mainCamera = Camera.main.GetComponent<NavigateCamera>();

        if ( clientData.GetBattleSimluateState() )
        {
            SimulateBattleMessageManager simBattleManager = this.gameObject.AddComponent<SimulateBattleMessageManager>();
            simBattleManager.Initialize();
            mainCamera.SetCameraControlType( CameraControlType.Observer );
        }
        else
        {
            mainCamera.SetCameraControlType( CameraControlType.Player );
        }

        if ( BattleType == BattleType.BattleP2vsP2 )
        {
			mapData2V2 = battleMap.AddComponent<MapData2V2>();
			mapData2V2.InitializeMapData2v2();
            mainCamera.SetCameraOriginalPosition( mapData2V2.GetCameraOriginalPosition( mark ) );
            mainCamera.SetCameraInvertMode( mapData2V2.GetCameraInvertType( mark ) );// TODO: In PVE may need some special setting
            mainCamera.SetCameraAngle( mapData2V2.GetCameraOriginalAngle( mark ) );
            mainCamera.SetCameraFieldOfViewValue( mapData2V2.GetViewField( clientData.camareViewChoose ) );
            mainCamera.SetCameraRange( mapData2V2.GetCameraHeightRange( mark ), mapData2V2.GetCameraWidthRange( mark ) );

            MessageDispatcher.PostMessage( MessageType.InitMiniMap, mapData2V2 );

            renderWorld.RenderStart( mapData2V2 );
        }
		else if ( battleType == BattleType.BattleP1vsP1 || battleType == BattleType.Tutorial )
        {
			mapData1V1 = battleMap.AddComponent<MapData1V1>();
			mapData1V1.InitializeMapData1v1();
            mainCamera.SetCameraOriginalPosition( mapData1V1.GetCameraOriginalPosition( mark ) );
            mainCamera.SetCameraInvertMode( mapData1V1.GetCameraInvertType( mark ) );// TODO: In PVE may need some special setting
            mainCamera.SetCameraAngle( mapData1V1.GetCameraOriginalAngle( mark ) );
            mainCamera.SetCameraFieldOfViewValue( mapData1V1.GetViewField( clientData.camareViewChoose ) );
            mainCamera.SetCameraRange( mapData1V1.GetCameraHeightRange( mark ), mapData1V1.GetCameraWidthRange( mark ) );

            MessageDispatcher.PostMessage( MessageType.InitMiniMap, mapData1V1 );

            renderWorld.RenderStart( mapData1V1 );

			if( battleType == BattleType.Tutorial )
			{
				InitPVEMode( battleType );
			}
        }
        else
        {
			mapDataPVE = battleMap.GetComponent<MapDataPVE>();
			mapDataPVE.InitializeMapDataPvE();
            mainCamera.SetCameraOriginalPosition( mapDataPVE.GetCameraOriginalPosition( mark ) );
            mainCamera.SetCameraFieldOfViewValue( mapDataPVE.GetViewField( clientData.camareViewChoose ) );
            mainCamera.SetCameraRange( mapDataPVE.GetCameraHeightRange(), mapDataPVE.GetCameraWidthRange() );

            mainCamera.SetCameraInvertMode();
            MessageDispatcher.PostMessage( MessageType.InitMiniMap, mapDataPVE );

            renderWorld.RenderStart( mapDataPVE );
            InitPVEMode( battleType );
        }


        DebugUtils.Log( DebugUtils.Type.Battle, "Battle type = " + type.ToString() );
    }

	private void InitPVEMode( BattleType type )
	{
		if( type == BattleType.Survival )
		{
			this.gameObject.AddComponent<LocalBattleMessageManager>();
			endlessModeManager = new EndlessModeManager();
			endlessModeManager.SetMapData( mapDataPVE );
			endlessModeManager.SetLogicWorld( logicWorld );
			endlessModeManager.InitEndlessMode();
		}
		else if( type == BattleType.Tranining )
		{
			this.gameObject.AddComponent<LocalBattleMessageManager>();
			trainingModeManager = new TrainingModeManager();
			trainingModeManager.SetMapData( mapDataPVE );
			trainingModeManager.SetLogicWorld( logicWorld );
			trainingModeManager.InitTrainingMode();
		}
		else if( type == BattleType.Tutorial )
		{
			//TODO: there need add tutorial manager, Dwayne.
			this.gameObject.AddComponent<LocalBattleMessageManager>();
			tutorialModeManager = new TutorialModeManager();
			tutorialModeManager.SetTutorialStage( dataManager.GetTutorialStage() );
			mapData1V1.SetTutorialPathPointPosition();
			tutorialModeManager.SetMapData( mapData1V1 );//Temp mapdata if have tutorial mapdata change this.
			tutorialModeManager.SetLogicworld( logicWorld );
			tutorialModeManager.SetMainCamera( this.mainCamera );
			tutorialModeManager.InitTutorialMode();
		}
		else
		{
			DebugUtils.LogError( DebugUtils.Type.Data, "We just have two PVE Mode, Can not find this mode : " + type );
		}
	}

	//Locked building mode drag deployment code.Dwayne.
	/*
	public bool CanDeployBuildingOnMap( ForceMark mark, Vector3 hitObjectPos )
	{
		DebugUtils.Log( DebugUtils.Type.Building, "The hitObjPos is" + hitObjectPos );
		return logicWorld.IsCanDeployBuildingFromTower( mark, hitObjectPos );
	}*/

	//Drag deployment logic locked.Dwayne 2017.9
    /*public void SpawnUnitOnMap( long playerId, int unitMetaId, Vector3 hitPointPos )
    {
        RenderMessage rm = new RenderMessage();
        rm.type = RenderMessage.Type.ShowDeployMouseEffect;
        rm.position = hitPointPos;
        HandleRenderMessage( rm );

        logicWorld.SpawnUnit( playerId, unitMetaId, hitPointPos, false );
    }*/


	public void RequestSpawnUnitOnMap( int unitMetaId, int buttonIndex )
    {
		logicWorld.RequestSpawnUnit( DataManager.GetInstance().GetPlayerId(), unitMetaId, buttonIndex );
    }

    public void SpawnUnitOnMap( long playerId, int unitMetaId )
	{
		RenderMessage rm = new RenderMessage();
		rm.type = RenderMessage.Type.ShowDeployMouseEffect;
		//TODO:There maby add new show effect
		//HandleRenderMessage( rm );

		logicWorld.SpawnUnit( playerId, unitMetaId );
	}

    // so SpawnUnitOnMap( MatchSide side, Vector3 bornPosition ) used for pve
    public long SpawnUnitOnMap( ForceMark mark, int unitMetaId, Vector3 bornPosition, BattleUnit unitInfo )
    {
       return logicWorld.SpawnUnit( logicWorld.forces[mark].id, unitMetaId, bornPosition, unitInfo );
    }

	//Player build building function
	public void SpawnBuildingOnMap( long playerId, BuildingType type, Vector3 bornPosition, int buildingID  )
	{
        FixVector3 fixBornPosition = (FixVector3)bornPosition;

        if ( type == BuildingType.Tower )
		{
			if( buildingID != -1 )
			{
				logicWorld.SetupTower( playerId, fixBornPosition, buildingID );
				DebugUtils.Log( DebugUtils.Type.Building, "The player spawnBuilding complete, PlayerID is ：" + playerId + "BuildingType is ：" + type );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Building, "The tower must be have a buildingID,Check this!" );
			}
		}
		else if( type == BuildingType.Institute ) 
		{
			logicWorld.SetupInstitute( playerId, fixBornPosition );
			DebugUtils.Log( DebugUtils.Type.Building, "The player spawnBuilding complete, PlayerID is ：" + playerId + "BuildingType is ：" + type );
		}
	}

    public void SetSimUnitSkillEnable( long simTownId, bool enable )
    {
        logicWorld.SetSimUnitSkillEnable( simTownId, enable );
    }

	//Locked building mode drag deployment code.Dwayne.
	//PVE AI use build building function
	/*
	public void SpawnBuildingOnMap( ForceMark mark, BuildingType type, Vector3 bornPosition, int buildingID )
	{
		if( type == BuildingType.Tower )
		{
			//TODO: If need AI build building, open this code and modify.
            //logicWorld.SetupTower( mark, logicWorld.forces[mark].id, bornPosition, TowerLocateType.None, buildingID );
		}
		else if( type == BuildingType.Institute ) 
		{
			//TODO: need add build institute logic
		}
	}
	*/

	public void BuildingLevelUp( long playerID, BuildingType type  )
	{
		if( type == BuildingType.Institute )
		{
			logicWorld.InstituteLevelUp( playerID );	
		}
		else if( type == BuildingType.Tower )
		{
			//TDOO: now we just can level up institute.
		}
	}

	public void InstituteSkillLevelUp( long playerID, int skillID, int upgradeUnm )
	{
		logicWorld.InstituteSkillLevelUp( playerID, skillID, upgradeUnm );
		DebugUtils.Log( DebugUtils.Type.InstitutesSkill, string.Format( "BattmeManager receive the InstituteSkillLevelUp message, PlayerID is : {0}, SkillID is : {1}, Upgrade number ot time : {2}.", playerID, skillID, upgradeUnm ));
	}

	//Locked building mode drag deployment code.Dwayne.
	/*
	public void ShowDeployBuildingAreaFromTower( ForceMark mark )
	{
		logicWorld.ShowDeployBuildingArea( mark );
	}

	public void CloseDeployBuildingAreaFromTower( ForceMark mark )
	{
		logicWorld.CloseDeployBuildingArea( mark );
	}*/

	public void RecylingTower( long playerID, long towerID  )
	{
		logicWorld.RecylingTower( playerID, towerID );
	}

    public void MapAreaCollision( long playerID, long unitId, int collisionType, int collisionState )
    {
        logicWorld.MapAreaCollision( playerID, unitId, collisionType, collisionState );
    }

    public void SyncSkillTargetPosition( long skillId, Vector3 position, long ownerId, int metaId )
    {
        logicWorld.SyncSkillTargetPosition( skillId, position, ownerId, metaId );
    }

    public void ClearUnitsPath( )
    {
        logicWorld.ClearFreePath( DataManager.GetInstance().GetForceMark() );
    }

    public long UnitCanBeDrag( GameObject g )
    {
        UnitRender s = g.GetComponent<UnitRender>();
        ForceMark mark = DataManager.GetInstance().GetForceMark();

        if ( mark == s.mark && s.unitRenderType == UnitRenderType.Soldier )
        {
            return s.id;
        }

        return -2;
    }

    public bool UnitCanBeChase( GameObject g )
    {
        UnitRender s = g.GetComponent<UnitRender>();
        ForceMark mark = DataManager.GetInstance().GetForceMark();

        if ( logicWorld.GetSideFromMark( mark ) == logicWorld.GetSideFromMark( s.mark ) )
        {
            return false;
        }

        return true;
    }

    public void SelectedUnit( GameObject g, Vector3 hitPoint ,bool ignoreEmeny, bool replaceGroup )
    {
        UnitRender s = g.GetComponent<UnitRender>();
        ForceMark mark = DataManager.GetInstance().GetForceMark();
        long id = s.id;

        if ( !ignoreEmeny && s.mark != mark )
        {
            // enemy
            // TODO: change the position to logic unit
            DebugUtils.Log( DebugUtils.Type.Battle, string.Format( " Set {0} {1} {2} as target, will change the unit target to this unit", s.mark, s.unitRenderType, id) );

            logicWorld.ChangeTarget( mark, id, (LogicUnitType)s.unitRenderType );
        }
        else
        {
            // friendly force
            if ( s.unitRenderType == UnitRenderType.Soldier )
            {
                logicWorld.SelectedUnit( mark, id, replaceGroup );
            }
            else
            {
				if( s.unitRenderType == UnitRenderType.Tower )
				{
					logicWorld.TapTower( mark, id );
				}
				else if( s.unitRenderType == UnitRenderType.Institute )
				{
					logicWorld.TapInstitute( mark, id );
				}
            }
        }
    }

    public void ChangeUnitTarget( long currentDragUnit, GameObject target )
    {
        UnitRender targetRender = target.GetComponent<UnitRender>();
        ForceMark mark = DataManager.GetInstance().GetForceMark();
        logicWorld.ChangeTarget( mark, currentDragUnit, targetRender.id, (LogicUnitType)targetRender.unitRenderType );
    }

    // be used to UI selected.
    public void SelectedUnit( long id, bool replaceGroup )
    {
        ForceMark mark = DataManager.GetInstance().GetForceMark();

        logicWorld.SelectedUnit( mark, id, replaceGroup );
    }

    // double tap a enemy unit, there will be change all target to enemy
    // double tap a friendly unit, there will be just select all unit.
    public void DoubleTapOperationOnUnit( GameObject g )
    {
        ForceMark playerForceMark = DataManager.GetInstance().GetForceMark();
        UnitRender s = g.GetComponent<UnitRender>();
        long id = s.id;

        logicWorld.SelectAllUnits( playerForceMark, false );

        if ( s.mark != playerForceMark )
        {
            // enemy force
            // TODO: change the position to logic unit
            DebugUtils.Log( DebugUtils.Type.Battle, string.Format( " Set {0} {1} {2} as target, will change the unit target to this unit", s.mark, s.unitRenderType, id ) );

            logicWorld.ChangeTarget( playerForceMark, id, (LogicUnitType)s.unitRenderType );
        }
        else
        {
            // friendly force 
            // do nothing
        }
    }

    // Used by UpdateMessage.ChangedTarget
    public void ChangeForceTarget( long playerId, long unitId, long targetId )
    {
        logicWorld.ChangeForceTarget( playerId, unitId, targetId );
    }

    // Used by UpdateMessage.ChangedDestination
    public void ChangeForceDestination( long playerId, long unitId, Vector3 destination )
    {
        logicWorld.ChangeForceDestination( playerId, unitId, destination );
    }

    // Used by Training mode
    public void ChangeSingleSoldierTarget( ForceMark mark, long id, Vector3 postion, int pathMask )
    {
        logicWorld.ChangeSingleSoldierTarget( mark, id, postion, pathMask );
    }

    // if soldier group not empty and not the block, will move the group to there
    public void TapGround( Vector3 pos, PathType pathType )
    {
        ForceMark mark = DataManager.GetInstance().GetForceMark();

        DebugUtils.Log( DebugUtils.Type.Battle, string.Format( " Player tap ground, will change the soldier group to target position = {0} , pathType = {1}", pos, pathType ) );
        logicWorld.ChangeDestination( mark, pos, pathType );
    }

	#region TapBuildingBase

	public void TapInstituteBase( Vector3 pos )
	{
		if( logicWorld.forces[this.mark].Alive() )
		{
			if( battleType == BattleType.BattleP2vsP2 )
			{
				if( mapData2V2.IsSelfInstituteBase( this.mark, pos ) )
				{
					MessageDispatcher.PostMessage( Constants.MessageType.OpenBuildInstiutePopUp, pos );
				}
			}
			else if( battleType == BattleType.BattleP1vsP1 || ( battleType == BattleType.Tutorial && tutorialModeManager.GetTutorialStage() == TutorialModeManager.TutorialModeStage.BuildingControlOperation_Stage ) )
			{
				if( mapData1V1.IsSelfInstituteBase( this.mark, pos ) )
				{
					MessageDispatcher.PostMessage( Constants.MessageType.OpenBuildInstiutePopUp, pos );
				}
			}
			else
			{
				//We just have 2 type map, if have more map, need add logic.Dwayne.
			}
		}
		else
		{
			//If you defeat the game can't build institute.
		}

	}

	public void TapTowerBase( Vector3 pos )
	{
		if( !logicWorld.forces[this.mark].Alive() )
		{
			//If you defeat the game can't build tower.
			return;
		}

		if( battleType == BattleType.BattleP2vsP2 )
		{
			if( mapData2V2.IsSelfTowerBase( this.mark, pos ) )
			{
				bool canPop = true;

				foreach( TowerRender item in renderWorld.towerRenders.Values )
				{
					if( item.GetPosition() == pos )
					{
						canPop = false;
					}
				}

				if( canPop )
				{
					MessageDispatcher.PostMessage( Constants.MessageType.OpenTowerPopUp, pos );
				}
			}
		}
		else if( battleType == BattleType.BattleP1vsP1 )
		{
			if( mapData1V1.IsSelfTowerBase( this.mark, pos ) )
			{
				bool canPop = true;

				foreach( TowerRender item in renderWorld.towerRenders.Values )
				{
					if( item.GetPosition() == pos )
					{
						canPop = false;
					}
				}

				if( canPop )
				{
					MessageDispatcher.PostMessage( Constants.MessageType.OpenTowerPopUp, pos );
				}
			}
		}
		else
		{
			//We just have 2 type map, if have more map, need add logic.Dwayne.
		}

	}

	#endregion

    // just draw PathPoint, will not move unit
    public void DrawUnitsPathPoint( long unitId, Vector3 pos, PathType pathType )
    {
        logicWorld.DrawUnitsPathPoint( mark, unitId, pos, pathType );
    }

    // will make unit move
    public void ExcutePathPoint( List<Vector3> positionList, long unitId, PathType pathType, Boolean isLast )
    {
        logicWorld.ChangeTargetByResetPath( mark, unitId, positionList, pathType, isLast );
    }

    public void SelectAllUnits()
    {
        logicWorld.SelectAllUnits( mark, true );
    }

    public void RandomSetCameraTarget( ForceMark mark )
    {
        logicWorld.SetCameraFollowUnit( mark );
    }
}

public class RenderMessage
{
    public enum Type
    {
        TimeChanged,
        CoinChanged,
        SyncPosition,
        SyncHP,
        SyncUnitPathPoint,
        DrawPathPoint,
        ClearPath,

        SpawnTown,
        SpawnProjectile,
        SpawnSoldier,
        SpawnTower,
        SpawnCrystal,
        SpawnCrystalCar,
        SpawnDemolisher,
        SpawnPowerUp,
		SpawnInstitute,
        SpawnNPC,
        SpawnSummonedUnit,
        SpawnTrap,
        SpawnIdol,
        SpawnIdolGuard,

        SoldierBornComplete,
        SoldierWalk,
        SoldierAttack,
        SoldierSpawnProjectile,
        SoldierIdle,
        SoldierDeath,
        SoldierHurt,
        SoldierHeal,
        SoldierSelected,
        SoldierPickUpPowerUp,
        SoldierVisibleChange,
        SoldierStuned,
        SoldierReleaseSkill,
        SoldierHitTarget,
        SoldierDash,
        SoldierUseBattery,
        SoldierBatteryFire,

        CritHurtUnit,

        TownHurt,
        TownDestroy,
		TapTower,

        TowerHurt,
        TowerDestroy,
		RecylingTower,

		InstituteHurt,
		InstituteDestroy,
		TapInstitute,

		DeployBuildingAreaOpen,
		DeployBuildingAreaClose,

        ProjectileHit,
        ProjectileTimeOut,

        CrystalMined,
        CrystalDestroy,
        CrystalRecover,

        // Crystal
        CrystalCarIdle,
        CrystalCarWalk,
        CrystalCarMining,
        CrystalCarHurt,
        CrystalCarDestroy,

        // Demolisher
        DemolisherIdle,
        DemolisherWalk,
        DemolisherFight,
        DemolisherHurt,
        DemolisherDestroy,

        // NPC
        NpcWalk,
        NpcIdle,
        NpcFight,
        NpcFightHit,
        NpcHurt,
        NpcDeath,
        NpcReborn,

        // Idol
        IdolOut,
        IdolHurt,
        IdolDeath,

        // Guard
        IdolGuardDeath,
        IdolGuardHurt,

        // Summon
        SummonedUnitIdle,
        SummonedUnitAttack,
        SummonedUnitDeath,

		// Trap
        TrapDestroy,

        PowerUpDestoried,
        PowerUpPickedUp,

        SkillReleased,
		InstituteLevelUpStart,
		InstituteLevelUpComplete,
		InstituteSkillLevelUp,

        ShowDeployAreas,
        CloseDeployAreas,
        ShowDeployMouseEffect,
        ShowFocusTargetEffect,

        SetCameraFollow,

        // buff
        AttachAttributeEffect,
        DetachAttributeEffect,

        //Skill
        SpawnSkill,
        SkillRelease,
        SkillHit,

        // UI Notice
        InitUIUnitCard,
        FillUnitUICard,
        NoticeKillUnit,
        NoticeKillIdol,

        // PVE
        SimUnitEnterIdleState,
    }

    public Type type;
    public long ownerId;
    public Vector3 position;
    public Vector3 direction;

    public Dictionary<string, object> arguments;

    public RenderMessage()
    {
        arguments = new Dictionary<string, object>();
    }
}
