/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: GameConstants.cs
// description:
// 
// created time： 09/26/2016
//
//----------------------------------------------------------------*/

using UnityEngine;

namespace Constants
{
    public class NetworkConstants
	{
        public const string EXTERNAL_SERVER_NAME1 = "King Ember Cloud";
		public const string EXTERNAL_SERVER_URL1 = "120.92.220.119";
        public const int EXTERNAL_SERVER_PORT = 2000;
        
        public const string EXTERNAL_SERVER_NAME2 = "SunMAC's Server";
        public const string EXTERNAL_SERVER_URL2 = "10.199.31.42";
        public const string INTERNAL_SERVER_NAME1 = "Jiawen's Server";
        public const string INTERNAL_SERVER_URL1 = "10.199.31.209";
        public const string INTERNAL_SERVER_NAME2 = "Ancher's Server";
        public const string INTERNAL_SERVER_URL2 = "10.199.21.247";
        public const string INTERNAL_SERVER_NAME3 = "FangJS's Server";
        public const string INTERNAL_SERVER_URL3 = "10.199.21.15";
        public const int INTERNAL_SERVER_PORT = 8473;

        public const string LOGIN_SERVER_NAME = "LoginServer";
        public const int LOGIN_SERVER_PORT = 8473;

        public const string GAME_SERVER_NAME = "GameServer ";
        public const int GAME_SERVER_PORT = 8474;

        public const string LOBBY_SERVER_NAME = "LobbyServer ";
        public const int LOBBY_SERVER_PORT = 8475;

        public const string BATTLE_SERVER_NAME = "BattleServer";

        public const int MAX_PACKET_SIZE = 4096;
        public const int MAX_SEGMENT_SIZE = 1464; // ADSL
        public const int REQUEST_TIMEOUT = 5000;
	}

    public class PlayerPrefKey
    {
        public const string LOGIN_USER_NAME = "1";
        public const string LOGIN_PASSWORD = "2";

        public const string CHAT_ENTER = "3";
		public const string CHAT_LOCK = "4";
    }

    public class GameConstants
    {
        public const float SOLDIER_BORN_INTERVAL = 10;
        public const int SOLDIER_NUM = 100;
        public const int RANGED_SOLDIER_NUM = 200;
        public const int MELEE_SOLDIER_NUM = 200;
        public const int PROJECTILE_NUM = 200;

        public const int NPC_NUM = 200;
        public const int ATTACKRANG_THRESHOLD = 5;

        public const float LOGIC_FRAME_TIME = 0.060f;
        public const float LOGIC_FIXPOINT_PRECISION = 1000f;
        public const float LOGIC_FIXPOINT_PRECISION_FACTOR = 0.001f;

        public const int EQUAL_DISTANCE = 200;

        public const int HIT_DISTANCE = 500;

        public const int BEGINCHASE_DISTANCE = 5000;// 5f is a testing distance

        public readonly static Color BackgroundColor = new Color( 0.67f, 0.67f, 0.67f, 1 );

        public const float DOUBLE_TAP_THRESHOLD = 0.2f;

        public const float BATTLE_NOTIFICATION_TIME = 6f;

        public const float PING_TIME = 5f;//HeartBeatTime

        public const float CAMARE_HIGH_VIEW_VALUE = 40f;//High 
        public const float CAMARE_MIDDLE_VIEW_VALUE = 35f;//Middle
        public const float CAMARE_LOW_VIEW_VALUE = 30f;//Low

        public const int QUALITY_HIGH_VALUE = 5;//Fantastic
        public const int QUALITY_MIDDLE_VALUE = 1;//Fast
        public const int QUALITY_LOW_VALUE = 0;//Fastest

        public const int INSTITUTE_ID = 1001;

        public const int BATTLE_CAN_SURRENDER_TIME = 120;  //Surrender button activation time

        public const int START_COINS = 900000;//Temp modify coins for designer test.

        public const int HP_RECOVERY_INTERVAL_MILLISECOND = 1000; // ms

        // Town
        public const int TOWN_BLUEBASEID = 1;
		public const int TOWN_REDBASEID = 2;
        public const int TOWN_KILLREWARD = 300;

        public const int TOWN_COINSRECOVER_INTERVAL_MILLISECOND = 1000; // ms
        public const int TOWN_COINSRECOVER_COUNT = 5;

		//Deployment Unit limit
		public const int INSTITUTE_DEPLOYMENT_LIMIT = 1;//every player just have one institue.
		public const int TOWER_DEPLOYMENT_LIMIT = 3;//every player can hold five towers.

        // NPC
        public const int WILDMONSTER_METAID = 6;
        public const int IDOL_METAID = 1;
        public const int IDOLGUARD_METAID = 2;
        public const int IDOL_OUT_EFFECT = 80108;
        public const string IDOL_OUT_EFFECT_BINDPOINT = "DamagePoint";
        public const int IDOL_DEATH_EFFECT = 80107;
        public const string IDOL_DEATH_EFFECT_BINDPOINT = "DamagePoint";

        // Unit
        public const int UNIT_TRAMCAR_METAID = 30010; // tramcar meta ID
        public const int UNIT_DEMOLISHER_METAID = 30011; // DEMOLISHER meta ID

        public const int GRASS_BUFF_ID = 20002; // the attribute effect id when unit enter grass area
        public const int SHALLOWWATER_DEBUFF_ID = 20001; // the attribute effect id when unit enter shallow water area

        // Crystal 
        public const int CRYSTAL_BIG_RESOURCEID = 40005;
        public const int CRYSTAL_BIG_RESERVES = 1000; // how many crystal that big crystal have
        public const int CRYSTAL_BIG_RECOVERTIME = 60000; // crystal recover time

        public const int CRYSTAL_SMALL_RESOURCEID = 40006;
        public const int CRYSTAL_SMALL_RESERVES = 700;  // how many crystal that small crystal have
        public const int CRYSTAL_SMALL_RECOVERTIME = 30000; // crystal recover time

        public const int CRYSTAL_REMOVEOWNER_INTERVAL = 5000;
        public const float CRYSTAL_MODELRADIUS = 0.5f;

        //PVE AI unit after generete waiting effects finish time
        public const float PVE_TRAININGMODE_SOLDIER_WAITINGTIME = 2.75f;
		public const float PVE_ENDLESSMODE_SOLDIER_WAITINGTIME = 2.75f;
        public const int PVE_SIMUNIT_STAYIDLESTATE_TIME = 3000;

        public const float PATHPOINT_GAPDISTANCE = 1;

        //UI
        public const int PLAYER_CANDEPLOY_UNIT_MAXCOUNT = 6;

		//MiniMap
		public const int MINIMAP_1V1ID = 17001;
		public const int MINIMAP_2V2ID = 17002;

		//Hero born time
		public const float HERO_BORNTIME = 0.1f;

        //Play back local cache time limit
        public const int PLAYBACK_LOCALCACHE_TIMELIMIT = 3; 

        public static string GAME_CONFIG_SECRET_KEY = "OXZpSnBTWGJ4VExlYlN3bw==";
        public const string BundleExtName = ".bundle";
        public const string BundleManifest = "assetbundle";
		public static bool LoadAssetByEditor;

        // Sound 
        public const int SOUND_VICTORY_ID = 60103;
        public const int SOUND_DEFEAT_ID = 60104;
        public const int SOUND_SPAWNUNIT_ID = 60132;
        public const int BGM_BATTLE_ID = 60130;

        // Skill
        public const int UNIT_DASHSTART_DURATION = 1250;
        public const int UNIT_DASH_DURATION = 420;
        public const int UNIT_DASHEND_DURATION = 500;

        public const int UNIT_INSTALLBATTRY_DURATION = 1330;
        //public const float UNIT_DASH_DURATION = 0.42f;
        public const int UNIT_PACKUPBATTRY_DURATION = 500;

        public const int PRELOADED_SHOW_UNIT_ID = 30001;

        public const int JINGJIXIANJINGID = 1;
    }

    public class LayerName
    {
        //Layer names
        public const string LAYER_FLYINGWALKABLE = "FlyingWalkable";
        public const string LAYER_GROUNDWALKABLE = "GroundWalkable";
        public const string LAYER_UNIT = "Unit";
        public const string LAYER_BORNAREA = "BornArea";
        public const string LAYER_OBSTACLE = "Obstacle";
        public const string LAYER_GROUND = "Ground";
		public const string LAYER_INSTITUTE_BASE = "InstituteBase";
		public const string LAYER_TOWER_BASE = "TowerBase";
    }

    public class TagName
    {
        //Tag names
        public const string TAG_BUILDINGSOBSTACLE = "BuildingsObstacle";
        public const string TAG_SHOALWATERS = "ShoalWaters";
        public const string TAG_GRASS = "Grass";
        public const string TAG_CAMERAMAINMENU_V2 = "CameraMainMenu_v2";
        public const string TAG_NPC = "NPC";
        public const string TAG_SMALLCRYSTAL = "SmallCrystal";
        public const string TAG_BIGCRYSTAL = "BigCrystal";
    }

    public class NavMeshAreaType
    {
        public const int ALLAREAS = -1;

        public const int WALKABLE = 1;

        // No use so far.
        //public const int NotWalkable = 2;
        //public const int Jump = 4;

        public const int BOTTOMPATH = 8;
    }

    public enum MessageType
    {
        ChangePlayerId,

        EnterBattle,
        BattleEnd,

		MapShowEffectStart,

		LocalBattlePause,
		LocalBattleContinue,
        ShowBattleResultView,
        BattleTimeChanged,

        TapNavigateCamera,
        GestureOnNavigateCamera,
		BuildTown,
        BuildBarrack,
        BuildTower,
		BuildInstitute,
		BuildTramCar,
		BuildDemolisher,
		BuildingLevelUP,
        ManaChanged,
		ManaCost,
        CoinChanged,
        ReleaseSpell,
        HideRallyPoint,
        GenerateSoldier,
		GenerateBuilding,
        GenerateNpc,
        GenerateBoss,
        GenerateHero,
        ChangeSoldierPosition,
        TownDestroy,
        TownDestroyed,
        BarrackDestroyed,
        TowerDestroyed,
		InstituteDestroyed,
		TramCarDestroyed,
		DemolisherDestroyed,
        NpcDeath,
        SoldierDeath,
        SoldierHurt,
        SoldierHeal,
        SoldierVisibleChange,
        SyncUnitHp,
        HeroDeath,
		TownHurt,
		BarrackHurt,
		TowerHurt,
		InstituteHurt,
        ShowRallyPoint,
        SpawnPowerUp,
        PowerUpDestroyed,

        ArmyUnitClick,

        RefreshCurrency,
        RefreshPlayerName,
        RefreshSoldier,
        RefreshSocialServerRedBubble,
        RefreshGameServerRedBubble,

        RefreshPlayerUnitsData,
        RefreshPlayerBagsData,
        RefreshPlayerUpLevelData,
        RefreshPlayerUpStarData,
        RefreshPlayerBattleListData,
		RefreshPlayerChatData,
		RefreshHornNotificationData,
        
        OpenCurrencyWindow,
        OpenAlertWindow,
        OpenFriendInvationAlert,
        OpenBuildInstiutePopUp,
        OpenInstitutePopUp,
        OpenTowerPopUp,
		OpenCheckBattleFieldObj,
        OpenGainItemWindow,
		CheckBattleFieldObjPositionChange,
		CloseCheckBattleFieldObj,
		CloseTowerPopUp,
        IntoFightMatch,

		ChangeUIGestureState,
        DragFingerToMoveCamera,

        ChangeCanDeployedArea,
        ShowDeployAreas,
        CloseDeployAreas,
		SelectObjectInBattle,
        SelectAll,
        DeploySquad,
		DeployBuildingAreaOpen,
		DeployBuildingAreaClose,
		TapTower,
		RecylingTower,

        InitBattleUISquads,
        FillBattleUISquads,
        BattleUIOperationFeedBack,
        BattleUIKillIdolNotice,
        BattleUIKillUnitNotice,
        BuildingLevelUpOperationFeedBack,
		InstituteSkillOperationFeedBack,
		AddBuildingBuffMessage,
		RemoveBuildingBuffMessage,
        QuitBattleRequest,
        BattleUIBackToCity,

        CreateMiniMapEffect,

        SoundVolume,
        MusicVolume,

		GamePause,

        SurrenderTipsHandler,
        LoadBattleComplete,
        InitMiniMap,
		MiniMapEffectDestroy,

        BattleSituationRequest,
        BattleSituationResponse,

		InstituteSkillLevelUp,

        // PVE
        SimUnitEnterIdleState,
        TrainingModeReady,
		TutorialModeReady,
		TutorialUpData,
		TutorialGoNext,
		TutorialStop,
		TutorialStageComplete,
		TutorialShowResult,
		TutorialPathPointMessageSend,

        // play back message
        SetPlaybackSpeed,
        SetPlaybackPlayingState,
        SetCameraFollowForce,
        SetCameraFollowTarget,

        ConnectGameServer_GM,

        ShowMainBackground,

        // daily sign
        RefreshSignView,

        //newbie guide
        OpenNewbieGuide,
        InitNewbieGuideSucceed,

        //LoudspeakerView
        OpenLoudspeakerView,

        //refresh bag
        RefreshBagView,
        
        DisposeBundleCache,
    }

    public enum DisplayMessageType
    {
        CriticalStrike,
        NormalAttack,
        Dodge,
        Heal,
    }

	public enum BattleUIOperationType
	{
		None = 0,
		DeployUnitResult,
		DeployBuildingResult,
        SelectedUnitResult,
	}
}