using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using Constants;
using Utils;

using BattleUnit = Data.Battler.BattleUnit;
using Resource;
using SignInfo = Data.DailyLoginS2C.SignInfo;
using TutorialStage = PVE.TutorialModeManager.TutorialModeStage;


namespace Data
{
    public class DataManager
    {
        #region properties   

        private string loginServerIp;
        private int loginServerPort;
        private string gameServerIp;
        private int gameServerPort;
        private string battleServerIp;
        private int battleServerPort;
        private string lobbyServerIp;
        private int lobbyServerPort;

        private Player player;
        private Account account;
        private Battle battle;
        private CacheResource cacheResource;

        // Proto data
        public List<LocalizationProto.Localization> localizationProtoData;
        public List<UnitsProto.Unit> unitsProtoData;
        public List<UnitSkillsProto.UnitSkill> unitSkillsProtoData;
        public List<ExpValuesProto.ExpValue> expValuesProtoData;
        public List<ProducePackProto.ProducePack> producePackProto;
        public List<TrainingBehaviorProto.TrainingBehavior> trainingBehaviorProtoData;
        public List<SlotProto.Slot> slotProtoData;
        public List<StructuresProto.Structure> structuresProtoData;
        public List<ItemBaseProto.ItemBase> itemsProtoData;
        public List<StoreProto.Store> storeProtoData;
        public List<RuneProto.Rune> runeProtoData;
        public List<ExchangeProto.Exchange> exchangeProtoData;
        public List<AttributeEffectProto.AttributeEffect> attributeEffectProtoData;
        public List<ResourcesProto.Resources> resourcesProtoData;
        public List<BaseValueProto.BaseValue> baseValueProtoData;
        public List<SummonProto.Summon> summonProtoData;
        public List<NpcProto.Npc> npcProtoData;
        public List<AvatarsProto.Avatars> avatarsProtoData;
        public List<PowerUpsProto.PowerUps> powerUpsProtoData;
        public List<TrainingWavesProto.TrainingWaves> trainingWavesProtoData;
        public List<EndlessWavesProto.EndlessWaves> endlessWavesProtoData;
        public List<EndlessBehaviorProto.EndlessBehavior> endlessBehaviorProtoData;
        public List<ProjectileProto.Projectile> projectileProtoData;
        public List<TowerProto.Tower> towerProtoData;
        public List<InstituteProto.Institute> instituteProtoData;
        public List<InstituteSkillProto.InstituteSkill> instituteSkillProtoData;
        public List<RandomNameProto.RandomName> randomNameProtoData;
        public List<TrapProto.Trap> trapProtoData;

        string protoPath;//tmp

        #endregion

		#region TutorialStage

		private TutorialStage tutorialStage = TutorialStage.Null;

		#endregion

        private static DataManager instance;

        public static DataManager GetInstance()
        {
            if ( instance == null )
            {
                instance = new DataManager();
            }
            return instance;
        }

        private DataManager()
        {
            protoPath = string.Format( "{0}/{1}{2}", UnityEngine.Application.persistentDataPath, DownloadResource.SERVER_URL_BRANCK, "/GameResources/bytes" );
            account = new Account();
            player = new Player();
            battle = new Battle();
            cacheResource = new CacheResource();
        }

        public void Dispose()
        {
            cacheResource.Clear();
            cacheResource = null;
        }

        #region Load proto files

        public void LoadGameData()
        {
            localizationProtoData = LoadProtoData<LocalizationProto>( @"1_Localization.bytes" ).localizations;
            unitsProtoData = LoadProtoData<UnitsProto>( @"2_Units.bytes" ).units;
            unitSkillsProtoData = LoadProtoData<UnitSkillsProto>( @"4_UnitSkills.bytes" ).unitSkills;
            expValuesProtoData = LoadProtoData<ExpValuesProto>( @"5_ExpValues.bytes" ).expValues;
            producePackProto = LoadProtoData<ProducePackProto>( @"6_ProducePack.bytes" ).producePacks;
            trainingBehaviorProtoData = LoadProtoData<TrainingBehaviorProto>( @"7_TrainingBehavior.bytes" ).trainingBehaviors;
            slotProtoData = LoadProtoData<SlotProto>( @"8_Slot.bytes" ).slots;
            structuresProtoData = LoadProtoData<StructuresProto>( @"9_Structures.bytes" ).structures;
            itemsProtoData = LoadProtoData<ItemBaseProto>( @"10_Item.bytes" ).itemBases;
            storeProtoData = LoadProtoData<StoreProto>( @"11_Store.bytes" ).stores;
            runeProtoData = LoadProtoData<RuneProto>( @"12_Rune.bytes" ).runes;
            attributeEffectProtoData = LoadProtoData<AttributeEffectProto>( "13_AttributeEffect.bytes" ).attributeEffects;
            exchangeProtoData = LoadProtoData<ExchangeProto>( "14_Exchange.bytes" ).exchanges;
            resourcesProtoData = LoadProtoData<ResourcesProto>( "15_Resources.bytes" ).resources;
            baseValueProtoData = LoadProtoData<BaseValueProto>( "16_BaseValue.bytes" ).baseValues;
            summonProtoData = LoadProtoData<SummonProto>( "19_Summon.bytes" ).summon;
            npcProtoData = LoadProtoData<NpcProto>( "21_Npc.bytes" ).npcs;
            avatarsProtoData = LoadProtoData<AvatarsProto>( "22_Avatars.bytes" ).avatarss;
            powerUpsProtoData = LoadProtoData<PowerUpsProto>( "23_PowerUps.bytes" ).powerUpss;
            trainingWavesProtoData = LoadProtoData<TrainingWavesProto>( "26_TrainingWaves.bytes" ).trainingWavess;
            endlessWavesProtoData = LoadProtoData<EndlessWavesProto>( "34_EndlessWaves.bytes" ).endlessWavess;
            endlessBehaviorProtoData = LoadProtoData<EndlessBehaviorProto>( "35_EndlessBehavior.bytes" ).endlessBehaviors;
            projectileProtoData = LoadProtoData<ProjectileProto>( "36_Projectile.bytes" ).projectiles;
            towerProtoData = LoadProtoData<TowerProto>( "37_Tower.bytes" ).towers;
            instituteProtoData = LoadProtoData<InstituteProto>( "38_Institute.bytes" ).institutes;
            instituteSkillProtoData = LoadProtoData<InstituteSkillProto>( "39_InstituteSkill.bytes" ).instituteSkills;
            randomNameProtoData = LoadProtoData<RandomNameProto>( "25_RandomName.bytes" ).randomNames;
            trapProtoData = LoadProtoData<TrapProto>( "40_Trap.bytes" ).traps;
        }

        private T LoadProtoData<T>( string name )
        {
            T t = default(T);
            try
            {
                string fileName = CommonUtil.EncodingToMd5( name );
                byte[] data = File.ReadAllBytes( string.Format( @"{0}/{1}", protoPath, fileName ) );
                t = ProtobufUtils.Deserialize<T>( data );
            }
            catch ( Exception e )
            {
                DebugUtils.LogError( DebugUtils.Type.Data, "load protos " + name + " failed! Because " + e.Message );
                DebugUtils.LogError( DebugUtils.Type.Data, e.ToString() );

                throw;
            }
            return t;
        }

        #endregion

        public void RegisterDataServerMessageHandler()
        {
            player.RegisterPlayerServerMessageHandler();
        }

        public void RegisterDataSocialServerMessageHandler()
        {
            player.RegisterPlayerSocialServerMessageHandler();
        }

        public void ResetBattleData()
        {
            battle.Reset();
        }

        #region Matcher Data

        private List<Matcher> matcherData;

        public void ResetMatchers()
        {
            if ( matcherData != null )
            {
                matcherData.Clear();
            }
        }

        public List<Matcher> GetMatchers()
        {
            return matcherData;
        }

        public void SetMatchers( List<Matcher> matchers )
        {
            matcherData = matchers;
        }

        public void SetMatcher( Matcher matcher )
        {
            if ( matcherData == null )
            {
                matcherData = new List<Matcher>();
            }

            matcherData.Add( matcher );
        }

        public Matcher GetMatcherDataByID( long id )
        {
            Matcher data = null;
            int length = matcherData == null ? 0 : matcherData.Count;
            for ( int i = 0; i < length; i++ )
            {
                if ( matcherData[i].playerId == id )
                {
                    data = matcherData[i];
                }
            }
            return data;
        }

        #endregion

        #region Public Get Informantion

        //////////////////get set methods////////////////////

        public int GetCurrentGuideIndex()
        {
            return player.PlayerGuide.GetCurrentGuideIndex();
        }

        public string GetLoginServerIp()
        {
            return loginServerIp;
        }

        public int GetLoginServerPort()
        {
            return loginServerPort;
        }

        public void SetLoginServerIp( string ip )
        {
            loginServerIp = ip;
        }

        public void SetLoginServerPort( int port )
        {
            loginServerPort = port;
        }

        public string GetGameServerIp()
        {
            return gameServerIp;
        }

        public int GetGameServerPort()
        {
            return gameServerPort;
        }

        public void SetGameServerIp( string ip )
        {
            gameServerIp = ip;
        }

        public void SetGameServerPort( int port )
        {
            gameServerPort = port;
        }

        public string GetLobbyServerIp()
        {
            return lobbyServerIp;
        }

        public int GetLobbyServerPort()
        {
            return lobbyServerPort;
        }

        public void SetLobbyServerIp( string ip )
        {
            lobbyServerIp = ip;
        }

        public void SetLobbyServerPort( int port )
        {
            lobbyServerPort = port;
        }

        public string GetBattleServerIp()
        {
            return battleServerIp;
        }

        public int GetBattleServerPort()
        {
            return battleServerPort;
        }

        public void SetBattleServerIp( string ip )
        {
            battleServerIp = ip;
        }

        public void SetBattleServerPort( int port )
        {
            battleServerPort = port;
        }

        public void SetPlayerId( long id )
        {
            player.PlayerId = id;
        }

        public long GetPlayerId()
        {
            return player.PlayerId;
        }

        public void SetPlayerNickName( string name )
        {
            player.playerNickName = name;
        }

        public string GetPlayerNickName()
        {
            return player.playerNickName;
        }

        public void SetPlayerIsInBattle( bool isInBattle )
        {
            player.isInBattle = isInBattle;
        }

        public bool GetPlayerIsInBattle()
        {
            return player.isInBattle;
        }

        public void SetPlayerHeadIcon( string name )
        {
            player.headIconName = name;
        }

        public string GetPlayerHeadIcon()
        {
            return player.headIconName;
        }

        public PlayerBagInfo GetPlayerBag( BagType bagType )
        {
            return player.bagGroup.GetBag( bagType );
        }

        public bool GetIsFirstLogin()
        {
            return player.playerDailySign.GetIsFirstLogin();
        }

        public void SetFirstLoginOver()
        {
            player.playerDailySign.SetFirstLoginOver();
        }

        public List<SignInfo> GetSignInfo()
        {
            return player.playerDailySign.GetSignInfo();
        }

        public bool GetIsSignToday()
        {
            return player.playerDailySign.GetIsDailySign();
        }

        public PlayerUnits GetPlayerUnits()
        {
            return player.army;
        }

        public void SetPlayerBlackMarketInfo( BlackMarketInfo info )
        {
            player.blackMarketInfo = info;
        }

        public BlackMarketInfo GetPlayerBlackMarketInfo()
        {
            return player.blackMarketInfo;
        }

        public List<ArmyInfo> GetBattleArmyList()
        {
            return player.battleArmyList.ArmyList;
        }

        public List<RunePageInfo> GetRunePageList()
        {
            return player.runePageList;
        }

        public void SetBattleArmyList( List<ArmyInfo> armyList )
        {
            player.battleArmyList.ArmyList = armyList;
        }

        public int GetPlayerExp()
        {
            return player.Exp;
        }

        public void SetPlayerExp( int gainExp )
        {
            player.Exp = gainExp;
        }

        public int GetPlayerGold()
        {
            return player.currencies.GetCurrency( CurrencyType.GOLD ).currencyValue;
        }

        public void SetPlayerGold( int gold )
        {
            player.currencies.GetCurrency( CurrencyType.GOLD ).currencyValue = gold;
        }

        public int GetPlayerEmber()
        {
            return player.currencies.GetCurrency( CurrencyType.EMBER ).currencyValue;
        }

        public void SetPlayerDiamond( int diamond )
        {
            player.currencies.GetCurrency( CurrencyType.DIAMOND ).currencyValue = diamond;
        }

        public int GetPlayerDiamond()
        {
            return player.currencies.GetCurrency( CurrencyType.DIAMOND ).currencyValue;
        }

        public int GetPlayerStarNum()
        {
            return player.starNum;
        }

        public List<SoldierInfo> GetPlayerSoldiers()
        {
            return player.army.soldiers;
        }

        public int GetPlayerLevel()
        {
            return player.PlayerLevel;
        }

        public void SetPlayerLevel( int playerLevel )
        {
            player.PlayerLevel = playerLevel;
        }

        public int GetPlayerCurrentLevelMaxXP()
        {
            return expValuesProtoData.Find( p => p.ID == ( GetPlayerLevel() + 1 ) ).XPtoLevel;
        }

        //public int GetPlayerCurrentLevelMaxNEM()
        //{
        //    if ( player.starNum >= 3 )
        //        return 0;
        //    int currentMaxNEM = expValuesProtoData.expValues.Find( p => p.StarRank == ( player.starNum + 1 ) ).NEMs;

        //    return currentMaxNEM;
        //}

        /// <summary>
        /// If you want to use the index to get army, The return value needs to be reduced by 1
        /// </summary>
        public int GetArmyIndexByBattleType( BattleType type )
        {
            return player.GetArmyIndexByBattleType( type );
        }

        public int GetRuneIndexByBattleType( BattleType type )
        {
            return player.GetRuneIndexByBattleType( type );
        }

        public int GetInstituteSkillIndexByBattleType( BattleType type )
        {
            return player.GetInstituteSkillIndexByBattleType( type );
        }

		public Dictionary<int,float[]> GetRunePageAttribute( int runePageId )
		{
			return player.GetRunePageAttribute ( runePageId );
		}

        public void ReceiveLoginFeedbackData( LoginGameServerS2C data )
        {
            player.ReceiveLoginFeedback( data );
        }
			
        public void ReconnectGameServerFeedbackData( ReconnectGameS2C data )
        {
            player.ReconnectGameServerFeedback( data );
        }

        // Account info
        public Account GetAccount()
        {
            return account;
        }

        public string GetAccountUserName()
        {
            return account.username;
        }

        public void SetAccountId( long id )
        {
            account.accountId = id;
        }

        public long GetPlayerAccountId()
        {
            return account.accountId;
        }

        //Battle Type ConfigInfo
        public void SetBattleConfigArmyIndex( BattleType type, int index )
        {
            player.battleTypeConfig.armyConfigs.Find( p => p.battleType == type ).labelPage = index;
        }

        public int GetBattleConfigArmyIndex( BattleType type )
        {
            return player.GetArmyIndexByBattleType( type );
        }

        public void SetBattleConfigRuneIndex( BattleType type, int index )
        {
            player.battleTypeConfig.runePageConfigs.Find( p => p.battleType == type ).labelPage = index;
        }

        public int GetBattleConfigRuneIndex( BattleType type )
        {
            return player.GetRuneIndexByBattleType( type );
        }

        public void SetBattleConfigInstituteSkillIndex( BattleType type, int index )
        {
            player.battleTypeConfig.instituteConfigs.Find( p => p.battleType == type ).labelPage = index;
        }

        public int GetBattleConfigInsituteSkillIndex( BattleType type )
        {
			if( type == BattleType.Tutorial )
			{
				return 0;
			}

            return player.GetInstituteSkillIndexByBattleType( type );
        }

        public void GetPlayerBattleUnitByType( BattleType type, List<BattleUnit> battleUnits )
        {
            int armyIndex = GetArmyIndexByBattleType( type );
            ArmyInfo army = player.battleArmyList.ArmyList.Find( p => p.listId == ( armyIndex ) );

			Dictionary<int, float[]>runeDic = GetRunePageAttribute( ( GetRuneIndexByBattleType( type ) ));

            for ( int i = 0; i < army.unitIds.Count; i++ )
            {
                BattleUnit unit = battleUnits.Find( p => p.metaId == army.unitIds[i] );

                SoldierInfo s = player.army.soldiers.Find( p => p.metaId == army.unitIds[i] );

				UnitsProto.Unit soldierProto = unitsProtoData.Find( p => p.ID == s.metaId );

                unit = new BattleUnit();
                unit.metaId = army.unitIds[i];
                unit.count = 1;

				foreach( KeyValuePair<int, float[]> item in runeDic )
				{
					PropertyInfo temp = new PropertyInfo();
					temp.propertyType = ( PropertyType )item.Key;

					if( temp.propertyType == PropertyType.ArmorPro )
					{
						temp.propertyValue = soldierProto.Armor * ( 1 + item.Value[1] ) + item.Value[0];
					}
					else if( temp.propertyType == PropertyType.AttackRange )
					{
						temp.propertyValue = soldierProto.AttackRange * ( 1 + item.Value[1] ) + item.Value[0];
					}
					else if( temp.propertyType == PropertyType.AttackSpeed )
					{
						temp.propertyValue = soldierProto.AttackInterval * ( 1 + item.Value[1] ) + item.Value[0];
					}
					else if( temp.propertyType == PropertyType.CriticalChance )
					{
						temp.propertyValue = soldierProto.CriticalChance * ( 1 + item.Value[1] ) + item.Value[0];
					}
					else if( temp.propertyType == PropertyType.CriticalDamage )
					{
						temp.propertyValue = soldierProto.CriticalDamage * ( 1 + item.Value[1] ) + item.Value[0];
					}
					else if( temp.propertyType == PropertyType.HealthRecover )
					{
						temp.propertyValue = soldierProto.HealthRegen * ( 1 + item.Value[1] ) + item.Value[0];
					}
					else if( temp.propertyType == PropertyType.MagicAttack )
					{
						temp.propertyValue = soldierProto.MagicAttack * ( 1 + item.Value[1] ) + item.Value[0];
					}
					else if( temp.propertyType == PropertyType.MagicResist )
					{
						temp.propertyValue = soldierProto.MagicResist * ( 1 + item.Value[1] ) + item.Value[0];
					}
					else if( temp.propertyType == PropertyType.MaxHealth )
					{
						temp.propertyValue = soldierProto.Health * ( 1 + item.Value[1] ) + item.Value[0];
					}
					else if( temp.propertyType == PropertyType.PhysicalAttack )
					{
						temp.propertyValue = soldierProto.PhysicalAttack * ( 1 + item.Value[1] ) + item.Value[0];
					}
					else if( temp.propertyType == PropertyType.Speed )
					{
						temp.propertyValue = soldierProto.MoveSpeed * ( 1 + item.Value[1] ) + item.Value[0];
					}
					else
					{
						DebugUtils.LogError( DebugUtils.Type.Battle, "Can't find this proptyType" + temp.propertyType );
					}    
				
					unit.props.Add( temp );
				}
					
                // fill unit's gear data
                //for ( int j = 0; j < s.items.Count; j++ )
                //{
                //    // itemType == 1 means this item is a GearItem
                //    if ( s.items[j].itemType == 1 )
                //    {
                //        unit.gearMetaId.Add( (int)s.items[j].metaId );
                //    }
                //}

                //fill unit's propertyInfo
                //for ( int j = 0; j < s..Count; j++ )
                //{
                //    PropertyInfo p = new PropertyInfo();
                //    p.propertyType = s.props[j].propertyType;
                //    p.propertyValue = s.props[j].propertyValue;
                //}

                //unit.level = s.level;
                //unit.star = s.star;
                battleUnits.Add( unit );
            }
        }

		#region InstituteTutorial Unit

		public void SetTutorialPlayerUnits( List<BattleUnit> battleUnits )
		{
			List<int> unitIds = new List<int>(){ 30001, 30005, 30006 };

			for ( int j = 0; j < 3; j++ )
			{
				for( int i = 0; i < unitIds.Count; i++ )
				{
					BattleUnit unit = new BattleUnit();
					unit.metaId = unitIds[i];
					unit.count = 1;

					battleUnits.Add( unit );
				}
			}
		}
			
		#endregion

        #region InstituteInfo

        public List<int> GetPlayerSetedPackageInstituteSkills( int instituteSkillIndex )
        {
            if ( instituteSkillIndex == 0 )
            {
                return player.firstSetedInstituteSkillIDList;
            }
            else if ( instituteSkillIndex == 1 )
            {
                return player.secondSetedInstituteSkillIDList;
            }
            else if ( instituteSkillIndex == 2 )
            {
                return player.thirdSetedInstituteSkillIDlist;
            }
            else
            {
                return null;
                DebugUtils.LogError( DebugUtils.Type.InstitutesSkill, "Can't know this type player instituteSkill package." );
            }
        }

        public void SetPlayerSetedPackageInstituteSkills( int instituteSkillIndex, List<int> playerSetedInstituteSkills )
        {
            if ( instituteSkillIndex == 0 )
            {
                player.firstSetedInstituteSkillIDList = playerSetedInstituteSkills;
            }
            else if ( instituteSkillIndex == 1 )
            {
                player.secondSetedInstituteSkillIDList = playerSetedInstituteSkills;
            }
            else if ( instituteSkillIndex == 2 )
            {
                player.thirdSetedInstituteSkillIDlist = playerSetedInstituteSkills;
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.InstitutesSkill, "Can't know this type player instituteSkill package." );
            }
        }

        public void SetInstituteLV( int lv )
        {
            battle.institeLv = lv;
        }

        public int GetInstituteLV()
        {
            return battle.institeLv;
        }

        #endregion

        // Match info
        public MatchSide GetMatchSide()
        {
            return battle.side;
        }

        public void SetMatchSide( MatchSide side )
        {
            battle.side = side;
        }

        public ForceMark GetForceMark()
        {
            return battle.forceMark;
        }

        public void SetForceMark( ForceMark mark )
        {
            battle.forceMark = mark;
        }

        // Battle info
        public BattleType GetBattleType()
        {
            return battle.battleType;
        }

        public void SetSimulateBattleData( List<Frame> frames )
        {
            battle.frames = frames;
        }

        public List<Frame> GetSimulateBattleData()
        {
            return battle.frames;
        }

        public bool GetBattleSimluateState()
        {
            return battle.simulateBattle;
        }

        public void SetBattleType( BattleType type, bool simulate )
        {
            battle.battleType = type;
            battle.simulateBattle = simulate;
        }

        public bool CurBattleIsPVE()
        {
            return GetBattleType() == BattleType.Survival || GetBattleType() == BattleType.Tranining;
        }

        public long GetBattleId()
        {
            return battle.id;
        }

        public void SetBattleId( long id )
        {
            battle.id = id;
        }

        public List<Battler> GetBattlers()
        {
            return battle.battlers;
        }

        public void SetBattlers( List<Battler> list )
        {
            Battler battler = list.Find( b => b.playerId == player.PlayerId );
            battle.side = battler.side;
            battle.forceMark = battler.forceMark;
            battle.battlers = list;
        }

        public long GetFrame()
        {
            return battle.frame;
        }

        public void SetFrame( long frame )
        {
            battle.frame = frame;
        }

        public void SetCameraHeight( float height )
        {
            battle.cameraHeight = height;
        }

        public float GetCameraHeight()
        {
            return battle.cameraHeight;
        }

        public void AddPlayerKillCount( ForceMark mark )
        {
            battle.AddUnitKillCount( mark );
        }

        public int GetPlayerKillCount( ForceMark mark )
        {
            return battle.GetUnitKillCount( mark );
        }

        public void AddPlayerFatality( ForceMark mark )
        {
            battle.AddUnitFatality( mark );
        }

        public int GetPlayerFatality( ForceMark mark )
        {
            return battle.GetUnitFatality( mark );
        }

        public void AddPlayerTotalResources( ForceMark mark , int emberCount )
        {
            battle.AddUnitResources( mark , emberCount );
        }

        public int GetPlayerTotalResources( ForceMark mark )
        {
            return battle.GetUnitResources( mark );
        }

        public void AddPveWaveNumber()
        {
            battle.pveWaveNumber++;
        }

        public int GetPveWaveNumber()
        {
            return battle.pveWaveNumber + 1;
        }

        public long GetSimBattleDuration()
        {
            return battle.battleDuration;
        }

        public void SetSimBattleDuration( long duration )
        {
            battle.battleDuration = duration;
        }

        //RedCaption
        public int GetRedBubbleNum( CaptionType type )
        {
            return player.redcaptions.GetRedCaption( type ).captionNum;
        }

        public void SetRedBubbleNum( CaptionType type, int num )
        {
            player.redcaptions.GetRedCaption( type ).captionNum = num;
        }

        //chat Data info
        public List<ChatDataStruct> GetWorldChatDataList()
		{
			return player.chatMessages.worldChatDataList;
		}

		public Dictionary<long , List<ChatDataStruct>> GetPrivateChatDataList()
		{
			return player.chatMessages.privateChatDataDic;
		}

        // Other 
        public long GetSeed()
        {
            return battle.seed;
        }

        public void SetSeed( long v )
        {
            battle.seed = v;
        }

        public bool PveIsVictory
        {
            set
            {
                battle.pveIsVictory = value;
            }
            get
            {
                return battle.pveIsVictory;
            }
        }

        public void SetBattleStartTime()
        {
            battle.startTime = DateTime.Now;
        }

        public int GetBattleDuration()
        {
            TimeSpan start = new TimeSpan( battle.startTime.Ticks );
            TimeSpan end = new TimeSpan( DateTime.Now.Ticks );
            TimeSpan ts = end.Subtract( start ).Duration();

            return (int)ts.TotalSeconds;
        }

        #endregion

		#region PVE functions 

		private long pveEnemyID = -1;

        public void SimulatePVEData( BattleType type )
        {
            List<Battler> b = new List<Battler>();
            // Player
            Battler player = new Battler();
            player.playerId = DataManager.GetInstance().GetPlayerId();
            player.side = MatchSide.Blue;
			player.forceMark = ForceMark.TopBlueForce;

			if( type != BattleType.Tutorial )
			{
				GetPlayerBattleUnitByType( type, player.battleUnits );
			}
			else
			{
				SetTutorialPlayerUnits( player.battleUnits );
			}

            b.Add( player );

            // Opponent
            Battler opponent = new Battler();
            opponent.side = MatchSide.Red;
            opponent.forceMark = ForceMark.TopRedForce;

            if ( type == BattleType.Survival )
            {
                opponent.playerId = IdGenerator.GenerateLongId( "OpponentAIPlayer_Survival" );
            }
			else if( type == BattleType.Tranining )
			{
				opponent.playerId = IdGenerator.GenerateLongId( "OpponentAIPlayer_Training" );
			}
			else
            {
                opponent.playerId = IdGenerator.GenerateLongId( "OpponentAIPlayer_Tutorial" );
            }

			pveEnemyID = opponent.playerId;

            // hold on to add building and unit data
            b.Add( opponent );
            SetBattlers( b );
        }

		public long GetPVEAIID()
		{
			if( pveEnemyID == -1 )
			{
				DebugUtils.LogError( DebugUtils.Type.Data, "PVE AI ID can't be -1, check logic." );
				return -1;
			}

			return pveEnemyID;
		}

		public void SetTutorialStage( TutorialStage stage )
		{
			this.tutorialStage = stage;
		}

		public TutorialStage GetTutorialStage()
		{
			if( this.tutorialStage == TutorialStage.Null )
			{
				DebugUtils.LogError( DebugUtils.Type.Tutorial, "Warning you want get tutorial stage, but it is null.Check logic." );
			}

			return this.tutorialStage;
		}

		public void SetPlayerNoviceGuidanceData( PlayerNoviceGuidanceData data )
		{
			player.playerNoviceGuidanceData = data;
		}

		public PlayerNoviceGuidanceData GetPlayerNoviceGuidanceData()
		{
			return player.playerNoviceGuidanceData;
		}

		#endregion
        
        #region PlayerPrefs Data

        public int musicChoose = 0;//1 - On , 2 - Off
        public int soundChoose = 0;//1 - On , 2 - Off
        public int damageNumChoose = 0;//1 - On , 2 - Off
        public int displayBarChoose = 0;//1 - On , 2 - Off
        public int languageIndex = 0;//1 is Chinese , 2 is English
        public int camareViewChoose = 0;//1 is High , 2 is Middle , 3 is Low
        public int qualitySettingChoose = 0;//1 is High , 2 is Middle , 3 is Low
        public int recordScreenChoose = 0;//1 - On , 2 - Off
        public int unitOperationChoose = 0;//1 is modeOne , 2 is modeTwo , 3 is modeThree

        public void SaveSettingDataToPlayerPrefs()
        {
            SettingDataToPlayerPrefs settingData = new SettingDataToPlayerPrefs();
            settingData.soundChoose = soundChoose;
            settingData.musicChoose = musicChoose;
            settingData.damageNumChoose = damageNumChoose;
            settingData.displayBarChoose = displayBarChoose;
            settingData.languageIndex = languageIndex;
            settingData.camareViewChoose = camareViewChoose;
            settingData.qualitySettingChoose = qualitySettingChoose;
            settingData.recordScreenChoose = recordScreenChoose;
            settingData.unitOperationChoose = unitOperationChoose;

            PlayerPrefsUtils.SaveToPlayerPrefs( "SettingData" + GetPlayerId(), settingData );
        }

        public void ReadSettingDataFromPlayerPrefs()
        {
            SettingDataToPlayerPrefs settingData = PlayerPrefsUtils.GetValueFromPlayerPrefs<SettingDataToPlayerPrefs>( "SettingData" + GetPlayerId() );

            musicChoose = settingData.musicChoose == 0 ? 1 : settingData.musicChoose;
            soundChoose = settingData.soundChoose == 0 ? 1 : settingData.soundChoose;
            damageNumChoose = settingData.damageNumChoose == 0 ? 1 : settingData.damageNumChoose;
            displayBarChoose = settingData.displayBarChoose == 0 ? 1 : settingData.displayBarChoose;
            languageIndex = settingData.languageIndex == 0 ? 1 : settingData.languageIndex;
            camareViewChoose = settingData.camareViewChoose == 0 ? 2 : settingData.camareViewChoose;
            qualitySettingChoose = settingData.qualitySettingChoose == 0 ? 2 : settingData.qualitySettingChoose;
            recordScreenChoose = settingData.recordScreenChoose == 0 ? 1 : settingData.recordScreenChoose;
            unitOperationChoose = settingData.unitOperationChoose == 0 ? 1 : settingData.unitOperationChoose;

            MessageDispatcher.PostMessage( MessageType.SoundVolume, soundChoose == 2 ? 0f : 1f );
            MessageDispatcher.PostMessage( MessageType.MusicVolume, musicChoose == 2 ? 0f : 1f );

            switch ( (UI.QualitySettingType)qualitySettingChoose )
            {
                case UI.QualitySettingType.High:
                    UnityEngine.QualitySettings.SetQualityLevel( GameConstants.QUALITY_HIGH_VALUE );
                    break;
                case UI.QualitySettingType.Middle:
                    UnityEngine.QualitySettings.SetQualityLevel( GameConstants.QUALITY_MIDDLE_VALUE );
                    break;
                case UI.QualitySettingType.Low:
                    UnityEngine.QualitySettings.SetQualityLevel( GameConstants.QUALITY_LOW_VALUE );
                    break;
            }
        }

        #endregion

        #region Cache resource

        public void AddMainMenuCacheObj(  string name, UnityEngine.Object obj )
        {
            cacheResource.mainMenuCacheObjDic[name] = obj;
        }

        public T GetMainMenuCacheObj<T>( string name ) where T : UnityEngine.Object
        {
            UnityEngine.Object obj = null;
            if( !cacheResource.mainMenuCacheObjDic.TryGetValue( name, out obj ) )
            {
                DebugUtils.LogError( DebugUtils.Type.Resource, "Not found cache obj! name:" + name );
                return null;
            }
            return ( T )obj;
        }

        public void AddMainMenuCacheName( string name )
        {
            if ( !cacheResource.mainMenuCacheNameList.Contains( name ) )
            {
                cacheResource.mainMenuCacheNameList.Add( name );
            }
        }

        public void AddMainMenuCacheId( int id )
        {
            if ( !cacheResource.mainMenuCacheIdList.Contains( id ) )
            {
                cacheResource.mainMenuCacheIdList.Add( id );
            }
        }

        #endregion

        public string SceneName()
        {
            return SceneManager.GetActiveScene().name;
        }
    }

    #region SettingData PlayerPrefs

    public class SettingDataToPlayerPrefs
    {
        public int soundChoose;//1 - On , 2 - Off
        public int musicChoose;//1 - On , 2 - Off
        public int damageNumChoose;//1 - On , 2 - Off
        public int displayBarChoose;//1 - On , 2 - Off
        public int languageIndex;//1 is Chinese , 2 is English
        public int camareViewChoose;//1 is High , 2 is Middle , 3 is Low
        public int qualitySettingChoose;//1 is High , 2 is Middle , 3 is Low
        public int recordScreenChoose = 0;//1 - On , 2 - Off
        public int unitOperationChoose;//1 is modeOne , 2 is modeTwo , 3 is modeThree
    }

    #endregion
}