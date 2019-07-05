using Network;
using System.Collections.Generic;

using Utils;

namespace Data
{
    public class Player
    {
        public long PlayerId { get { return ClientTcpMessage.playerId; } set { ClientTcpMessage.playerId = value; } }

        public string playerNickName;
        public bool isInBattle = false;

        private int playerLevel = 1;
        public int PlayerLevel
        {
            get { return playerLevel; }
            set
            {
                playerLevel = value;
                MessageDispatcher.PostMessage( Constants.MessageType.RefreshPlayerUpLevelData );
            }
        }

        private int exp;
        public int Exp
        {
            get { return exp; }
            set
            {
                exp = value;

                MessageDispatcher.PostMessage( Constants.MessageType.RefreshPlayerUpLevelData );
            }
        }

        public int starNum;

        public string headIconName;

        public PlayerBags bagGroup;

        public PlayerUnits army;

        public PlayerCurrencies currencies;

        public PlayerBattleList battleArmyList;

		public List<RunePageInfo> runePageList;
       
        public BattleTypeConfigInfo battleTypeConfig;

        public RedCaptions redcaptions;

		public PlayerChatMessages chatMessages;

        public BlackMarketInfo blackMarketInfo;

        public PlayerDailySign playerDailySign;

        public PlayerGuide PlayerGuide;

        public PlayerNoviceGuidanceData playerNoviceGuidanceData;

        #region InstituteSkills value
        public List<int> firstSetedInstituteSkillIDList;
		public List<int> secondSetedInstituteSkillIDList;
		public List<int> thirdSetedInstituteSkillIDlist;

		#endregion

        public Player()
        {
            bagGroup = new PlayerBags();

            army = new PlayerUnits();

            currencies = new PlayerCurrencies();

            battleArmyList = new Data.PlayerBattleList();

            redcaptions = new Data.RedCaptions();

			chatMessages = new PlayerChatMessages();

            playerDailySign = new PlayerDailySign();

            PlayerGuide = new PlayerGuide();

            playerNoviceGuidanceData = new Data.PlayerNoviceGuidanceData();
        }

        public void RegisterPlayerServerMessageHandler()
        {
            NetworkManager.RegisterServerMessageHandler( MsgCode.RefreshPlayerBaseMessage, HandleRefreshPlayerBaseDataFeedback );

            bagGroup.RegisterPlayerBagServerMessageHandler();
            currencies.RegisterPlayerCurrencyServerMessageHandler();
            army.RegisterPlayerUnitsServerMessageHandler();
            battleArmyList.RegisterPlayerBattleListServerMessageHandler();
            redcaptions.RegisterRedCaptionMessageHandler();
            playerDailySign.RegisterPlayerDailySign();
        }

        public void RegisterPlayerSocialServerMessageHandler()
        {
            bagGroup.RegisterPlayerBagSocialServerMessageHandler();
            currencies.RegisterPlayerCurrencySocialServerMessageHandler();
            army.RegisterPlayerUnitsSocialServerMessageHandler();
            redcaptions.RegisterRedCaptionSocialMessageHandler();
			chatMessages.RegisterForwardChatSocialServerMessageHandler();
        }

        #region ReponseHandling
       
        private void HandleRefreshPlayerBaseDataFeedback( byte[] data )
        {
            RefreshPlayerBaseS2C feedback = ProtobufUtils.Deserialize<RefreshPlayerBaseS2C>( data );
            if ( feedback == null )
                return;

            if ( feedback.result )
            {
                SetPlayerInfo( feedback.playerInfo );
            }
        }

        #endregion

        #region Prepare Player Data

        public void ReceiveLoginFeedback( LoginGameServerS2C feedback )
        {
            if ( feedback == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Protocol, "ReceiveLoginFeedback~~~~Feedback is null" );
                return;
            }

            PlayerInfo playerInfo = feedback.playerInfo;

            SetPlayerInfo( playerInfo );

            SetPlayerBagsInfo( feedback.bags );

            SetPlayerCurrenciesInfo( feedback.playerInfo.currencies );

            SetPlayerArmyInfo( feedback.soldiers );

            SetPlayerBattleListInfo( feedback.armys );

            //SetBuildingInfo( feedback.buindings );

            SetInstituteSkillAttributeList( feedback.scienceInfo );

            SetPlayerRunePageInfos( feedback.runePageInfos );
           
            //SetBlackMarketInfo( feedback.blackMarketInfo );
        }

        public void ReconnectGameServerFeedback( ReconnectGameS2C feedback )
        {
            if ( feedback == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Protocol, "ReconnectGameServerFeedback~~~~Feedback is null" );
                return;
            }

            PlayerInfo playerInfo = feedback.playerInfo;

            SetPlayerInfo( playerInfo );

            SetPlayerBagsInfo( feedback.bags );

            SetPlayerCurrenciesInfo( feedback.playerInfo.currencies );

            SetPlayerArmyInfo( feedback.soldiers );

            SetPlayerBattleListInfo( feedback.armys );
            
            //SetBlackMarketInfo( feedback.blackMarketInfo );
        }

        private void SetPlayerInfo( PlayerInfo playerInfo )
        {
            this.PlayerId = playerInfo.playerId;
            this.playerNickName = playerInfo.name;
            this.PlayerLevel = playerInfo.level;
            this.headIconName = playerInfo.headIcon;
            this.Exp = playerInfo.exp;
            this.starNum = playerInfo.star;
            this.battleTypeConfig = playerInfo.battleTypeConfig;
        }

        private void SetPlayerBagsInfo( List<BagInfo> bagList )
        {
            for ( int i = 0; i < bagList.Count; i++ )
            {
                PlayerBagInfo bag = this.bagGroup.GetBag( (BagType)bagList[i].bagType );
                bag.itemList = bagList[i].items;
            }
        }

        private void SetPlayerCurrenciesInfo(List<Currency> currencies)
        {
            for ( int i = 0; i < currencies.Count; i++ )
            {
                PlayerCurrencyInfo currency = this.currencies.GetCurrency( currencies[i].currencyType );
                currency.currencyValue = currencies[i].currencyValue;
            }
        }

        private void SetPlayerArmyInfo( List<SoldierInfo> soldiersList )
        {
            this.army.soldiers = soldiersList;
        }

        private void SetPlayerBattleListInfo( List<ArmyInfo> armyList )
        {
            battleArmyList.ArmyList = armyList;
        }

		private void SetPlayerRunePageInfos( List<RunePageInfo> runePageList )
		{
			this.runePageList = runePageList;
		}

        private void SetInstituteSkillAttributeList( ScienceInfo info )
        {
            this.firstSetedInstituteSkillIDList = info.scienceSkillIds1;
            this.secondSetedInstituteSkillIDList = info.scienceSkillIds2;
            this.thirdSetedInstituteSkillIDlist = info.scienceSkillIds3;
        }

        private void SetBlackMarketInfo( BlackMarketInfo blackMarketInfo )
        {
            this.blackMarketInfo = blackMarketInfo;
        }

        public int GetArmyIndexByBattleType( BattleType type )
        {
            int v = battleTypeConfig.armyConfigs.Find( p => p.battleType == type ).labelPage;

            return v;
        }

        public int GetRuneIndexByBattleType( BattleType type )
        {
            int v = battleTypeConfig.runePageConfigs.Find( p => p.battleType == type ).labelPage;

            return v;
        }

        public int GetInstituteSkillIndexByBattleType( BattleType type )
        {
            int v = battleTypeConfig.instituteConfigs.Find( p => p.battleType == type ).labelPage;

            return v;
        }

		public Dictionary<int,float[]> GetRunePageAttribute(int runePageId)
		{
			List<RunePageInfo.RuneSlotInfo> slotList = runePageList.Find ( p => p.pageId == runePageId ).slots;
			List<int> runeIdList = new List<int> ();
			for ( int i = 0; i < slotList.Count; i++ )
			{
				RunePageInfo.RuneSlotInfo runeSlot = slotList[ i ];
				if( runeSlot.itemId != 0 )
				{
					runeIdList.Add ( runeSlot.itemId );
				}
			}

			Dictionary<int,float[]> attributes = new Dictionary<int, float[]> ();

			List<RuneProto.Rune> runes = DataManager.GetInstance ().runeProtoData;

			for ( int i = 0; i < runeIdList.Count; i++ )
			{
				int runeId = runeIdList[ i ];

				RuneProto.Rune rune = runes.Find ( p => p.ID == runeId );

				int property1 = rune.Property1;
				int addition1 = rune.Addition1;
				float value1 = rune.Value1;

				int property2 = rune.Property2;
				int addition2 = rune.Addition2;
				float value2 = rune.Value2;

				int property3 = rune.Property3;
				int addition3 = rune.Addition3;
				float value3 = rune.Value3;

				int property4 = rune.Property4;
				int addition4 = rune.Addition4;
				float value4 = rune.Value4;

				AddRunePageAttribute ( attributes , property1 , addition1 , value1 );
				AddRunePageAttribute ( attributes , property2 , addition2 , value2 );
				AddRunePageAttribute ( attributes , property3 , addition3 , value3 );
				AddRunePageAttribute ( attributes , property4 , addition4 , value4 );
			}

			return attributes;
		}

		private Dictionary<int,float[]> AddRunePageAttribute(	Dictionary<int,float[]> attributes, int property, int addition, float value )
		{
			if( attributes.ContainsKey ( property ) )
			{
				if( addition == 1 )
				{
					attributes[ property ][ 0 ] += value;
				}
				else if( addition == 2 )
				{
					attributes[ property ][ 1 ] += value;
				}
			}
			else if( property != 0 )
			{
				if( addition == 1 )
				{
					attributes.Add ( property , new float[]{ value, 0 } );
				}
				else if( addition == 2 )
				{
					attributes.Add ( property , new float[]{ 0, value } );
				}
			}
			return attributes;
		}

        #endregion
    }
}

