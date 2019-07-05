using UnityEngine;
using System.Collections.Generic;
using System;

using Data;
using Constants;
using Utils;
using BattleAgent;

using BattlerUnit = Data.Battler.BattleUnit;
using UnitProto = Data.UnitsProto.Unit;

namespace Logic
{
    public enum BuildingState
    {
        NONE,
		BUILD,
        IDLE,
        ATTACK,
        DESTROY,
    }

    public class Town : LogicUnit
    {
        private FindFriendlySoldiersMethod FindFriendlySoldiers;
        private FindOwnForceSoldiersMethod FindOwnForceSoldiers;
        private FindOpponentSoldierMethod FindOpponentSoldier;
        private FindOpponentSoldiersMethod FindOpponentSoldiers;
        private FindOpponentBuildingMethod FindOpponentBuilding;
        private FindOpponentBuildingsMethod FindOpponentBuildings;
        private CalculateAvoidanceMethod CalculateAvoidance;
        private FindOpponentCrystalMethod FindOpponentCrystal;
        private FindOpponentCrystalCarMethod FindOpponentCrystalCar;
        private FindOpponentCrystalCarsMethod FindOpponentCrystalCars;
        private FindOpponentDemolisherMethod FindOpponentDemolisher;
        private FindOpponentDemolishersMethod FindOpponentDemolishers;
        private FindNeutralUnitMethod FindNeutralUnit;
        private FindNeutralUnitsMethod FindNeutralUnits;
        private FindPowerUpMethod FindPowerUpMethod;
        private WithinCircleAreaPredicate WithinCircleAreaPredicate;
        private WithinFrontRectAreaPredicate WithinFrontRectAreaPredicate;
        private WithinSectorAreaPredicate WithinSectorAreaPredicate;

        private AttributeEffectGenerator GenerateAttributeEffectGenerator;
        private SkillGenerator GenerateSkill;
        private SoldierGenerator GenerateSoldier;
		private TowerGenerator GenerateTower;
		private InstituteGenerator GenerateInstitute;
        private ProjectileGenerator GenerateProjectile;
        private CrystalCarGenerator GenerateCrystalCar;
        private DemolisherGenerator GenerateDemolisher;
        private SummonGenerator GenerateSummon;
		private TrapGenerator GenerateTrap;

        public int coins;

        private List<Soldier> soldiers;
		private List<Tower> towers;
		private Institute institute;
        private Dictionary<long, Soldier> soldierGroup;

        private List<Demolisher> demolishers;
        private List<CrystalCar> crystalCars;

        private BuildingState state;

        public LogicUnit target;
        public long targetId;
        private int fightTimer;
        private int fightTime;
        private int attackArea;
        private int rangeDmgBase;
        private int rangeDmgVar;
		private int physicalResistance;
		private int magicResistance;
        private int healthRecover;

        private int coinRecoverInterval;
        private int addCoinTimer;
        private int increaseCoinNumber;

        private int healthRecoverInterval;
        private int healthRecoverTimer;

        private bool allUnitsSelected = false;

        private int projectileId;

        public List<BattlerUnit> battlerUnitDeck;
        private List<SquadData> unitCardPool;
        private SquadData lastWaitingCard;

        private bool owner;

        public List<int> UNIT_METAID_FLITER = new List<int> { 30000, 30001, 30002, 30003, 30004, 30005, 30006, 30007 };

        #region FormationValues
        private List<FixVector3> unitFormations;
        #endregion

		public void Initialize( long id, ForceMark mark, List<BattlerUnit> units, List<FixVector3> unitFormation )
        {
            type = LogicUnitType.Town;

            this.id = id;
            this.mark = mark;
            this.battlerUnitDeck = units;
            owner = DataManager.GetInstance().GetForceMark() == mark;

			unitFormations = unitFormation;

            InitializedData();

            if ( owner )
            {
                InitDeckPool();
            }

            soldiers = new List<Soldier>();
			towers = new List<Tower>();
            demolishers = new List<Demolisher>();
            crystalCars = new List<CrystalCar>();

            soldierGroup = new Dictionary<long, Soldier>();
        }

        private void InitDeckPool()
        {
            unitCardPool = new List<SquadData>();
            for ( int i = 0; i < battlerUnitDeck.Count; i++ )
            {
                for ( int j = 0; j < battlerUnitDeck[i].count; j++ )
                {
                    AddUnitCardToDeckPool( battlerUnitDeck[i].metaId );
                }
            }

            List<SquadData> cards = new List<SquadData>();
            for ( int i = 0; i < 3; i++ )
            {
                cards.Add( GetUnitCardFromDeckPool() );
            }

            lastWaitingCard = GetUnitCardFromDeckPool();
            RenderMessage rm = new RenderMessage();
            rm.ownerId = id;
            rm.type = RenderMessage.Type.InitUIUnitCard;
            rm.arguments.Add( "Cards", cards );
            rm.arguments.Add( "waitingCard", lastWaitingCard );
            PostRenderMessage( rm );
        }

        private SquadData GetUnitCardFromDeckPool()
        {
            if ( unitCardPool.Count > 0 )
            {
                int index = 0;/*GetRandomNumber() % unitCardPool.Count;*/
                SquadData card = unitCardPool[index];
                unitCardPool.RemoveAt( index );
                return card;
            }
            else
            {
                return null;
            }
        }

        private void AddUnitCardToDeckPool( int metaId )
        {
            UnitsProto.Unit unitProto = DataManager.GetInstance().unitsProtoData.Find( p => p.ID == metaId );
            unitCardPool.Add( new SquadData( unitProto, unitCardPool.Count ) );
        }

        private void InitializedData()
        {
			int townID = 0;
			if( mark == ForceMark.TopRedForce || mark == ForceMark.BottomRedForce )
			{
				townID = GameConstants.TOWN_REDBASEID;
			}
			else if( mark == ForceMark.TopBlueForce || mark == ForceMark.BottomBlueForce )
			{
				townID = GameConstants.TOWN_BLUEBASEID;
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Map, string.Format( "Town can't find this mark {0}", mark ) );
			}

			StructuresProto.Structure proto = DataManager.GetInstance().structuresProtoData.Find( p => p.ID == townID );
            rangeDmgBase = (int)proto.PhysicalAttack;
            physicalResistance = (int)proto.PhysicalResistance;
            attackArea = ConvertUtils.ToLogicInt( proto.AttackRange );
            fightTime = ConvertUtils.ToLogicInt( proto.AttackInterval );
            maxHp = proto.Health;
            healthRecover = 1;// Temp data;
            projectileId = proto.ProjectileType;
            iconId = -1;
            rangeDmgVar = (int)proto.PhysicalDmgVar;
            magicResistance = (int)proto.MagicResistance;

            hp = maxHp;

            state = BuildingState.IDLE;

            this.coins = GameConstants.START_COINS;
            coinRecoverInterval = GameConstants.TOWN_COINSRECOVER_INTERVAL_MILLISECOND;
            increaseCoinNumber = GameConstants.TOWN_COINSRECOVER_COUNT;

            healthRecoverInterval = GameConstants.HP_RECOVERY_INTERVAL_MILLISECOND;
            healthRecoverTimer = 0;

            killReward = GameConstants.TOWN_KILLREWARD;
        }

        public override void LogicUpdate( int deltaTime )
        {
            addCoinTimer += deltaTime;
            if( addCoinTimer > coinRecoverInterval )
            {
                addCoinTimer = 0;
                AddCoin( increaseCoinNumber );
            }

            if( state == BuildingState.IDLE )
            {
                LogicUnit target = FindOpponent();
                if ( target != null )
                {
                    Attack( target );
                }
            }
            else if( state == BuildingState.ATTACK )
            {
                if( target.Alive() && target.id == targetId )
                {
                    long distance = FixVector3.SqrDistance( target.position, position );
                    long attackDistance = GetAttackArea();

                    if( distance > attackDistance )
                    {
                        state = BuildingState.IDLE;
                    }
                    else
                    {
                        fightTimer += deltaTime;
                        if( fightTimer > fightTime )
                        {
                            fightTimer = 0;
                            Fire();
                        }
                    }
                }
                else
                {
                    Idle();
                }
            }
            else if( state == BuildingState.DESTROY )
            {

            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Tower, "the tower " + id + " is in strange state " + state );
            }
        }

        private long GetAttackArea()
        {
            long baseArea = attackArea;
            long baseModelRadius = modelRadius;

            if ( target != null )
            {
                long targetModelRadius = target.modelRadius;
                return baseArea + baseModelRadius + targetModelRadius;
            }

            return attackArea + modelRadius;
        }

		public void InstituteLevelUp()
		{
			int cost = this.institute.InstituteLevelUpStart();
			AddCoin( -cost );
		}

		public void InstituteSkillLevelUp( int skillID, int upgradeNum )
		{
			//TODO: check cost change player coin
			int cost = this.institute.InstituteSkillLevelUp( skillID, upgradeNum );
			AddCoin( -cost );
		}

        private void Idle()
        {
            state = BuildingState.IDLE;
        }

        private void Attack( LogicUnit t )
        {
            DebugUtils.Log( DebugUtils.Type.AI_Tower, "the tower " + id + " begins to attack unit " + t.id );

            targetId = t.id;
            target = t;
            fightTimer = 0;
            state = BuildingState.ATTACK;
        }

        private void Fire()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Tower, "the tower " + id + " begins to fire to target " + target.id );

            damage = GetDamageValue();

            Projectile projectile = GenerateProjectile( this, projectileId, position, target );
            projectile.RegisterRenderMessageHandler( PostRenderMessage );
            projectile.RegisterDestroyHandler( PostDestroy );
            projectile.RegisterRandomMethod( GetRandomNumber );
            projectile.RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate );
            projectile.RegisterWithinFrontRectangleAreaPredicate( WithinFrontRectAreaPredicate );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnProjectile;
            rm.ownerId = projectile.id;
            rm.position = projectile.position.vector3;
            rm.direction = projectile.speed.vector3;
            rm.arguments.Add( "mark", projectile.mark );
            rm.arguments.Add( "metaId", projectile.metaId );
            rm.arguments.Add( "holderType", (int)projectile.owner.type );
            rm.arguments.Add( "holderId", projectile.owner.id );
            PostRenderMessage( rm );
        }

        private void HealthAutoRecovery( int deltaTime )
        {
            if ( Alive() )
            {
                healthRecoverTimer += deltaTime;
				if ( healthRecoverTimer > healthRecoverInterval && hp < maxHp )
                {
                    hp += healthRecover;
                    healthRecoverTimer = 0;

                    RenderMessage rm = new RenderMessage();
                    rm.type = RenderMessage.Type.SyncHP;
                    rm.ownerId = id;
                    rm.arguments.Add( "type", (int)type );
                    rm.arguments.Add( "hp", hp );
                    rm.arguments.Add( "mark", mark );
                    rm.arguments.Add( "maxHp", maxHp );

                    PostRenderMessage( rm );
                }
            }
        }

        public override bool Alive()
        {
            return state != BuildingState.DESTROY;
        }

        public void Destroy()
        {
            state = BuildingState.DESTROY;

            RenderMessage renderMessage = new RenderMessage();
            renderMessage.type = RenderMessage.Type.TownDestroy;
            renderMessage.ownerId = id;
            renderMessage.arguments.Add( "mark", mark );

			if( towers.Count > 0 )
			{
				for( int i = 0; i < towers.Count; i++ )
				{
					towers[i].Destroy();
				}
			}

			if( institute != null )
			{
				institute.Destroy();
			}

            PostRenderMessage( renderMessage );
        }

        public override void Hurt( int hurtValue, AttackPropertyType type, bool isCrit, LogicUnit injurer )
        {
            if ( Alive() )
            {
                // town only can be affected by PhysicalAttack
                if ( type == AttackPropertyType.PhysicalAttack )
                {
                    hurtValue = GetTownActualHurtValue( type, hurtValue );
                    hp -= hurtValue;
                    RenderMessage message = new RenderMessage();
                    message.type = RenderMessage.Type.TownHurt;
                    message.ownerId = id;
                    message.position = position.vector3;
                    message.arguments.Add( "value", hurtValue );
                    PostRenderMessage( message );

                    if ( hp <= 0 )
                    {
                        injurer.OnKillEnemy( killReward, injurer, this );
                        Destroy();
                    }
                }
            }
        }

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            // Get Kill Reward
            AddCoin( emberReward );

            // Record Battle data 
            if ( dead.type == LogicUnitType.Soldier )
            {
                RenderMessage rm = new RenderMessage();
                rm.type = RenderMessage.Type.NoticeKillUnit;
                rm.arguments.Add( "killerMark", mark );
                rm.arguments.Add( "killerIconId", killer.iconId );
                rm.arguments.Add( "deadMark", dead.mark );
                rm.arguments.Add( "deadIconId", dead.iconId );
                PostRenderMessage( rm );

                DataManager clientData = DataManager.GetInstance();
                clientData.AddPlayerKillCount( mark );

                if ( clientData.GetForceMark() == mark )
                {
                    int d = clientData.GetPlayerFatality( clientData.GetForceMark() );
                    int k = clientData.GetPlayerKillCount( clientData.GetForceMark() );
                    int z = clientData.GetPlayerTotalResources( clientData.GetForceMark() );

                    // Send battle data to server
                    UploadSituationC2S result = new UploadSituationC2S();
                    result.type = 1;
                    result.battleSituation = new BattleSituation();
                    result.battleSituation.playerId = clientData.GetPlayerId();
                    result.battleSituation.fatality = d;
                    result.battleSituation.kills = k;
                    result.battleSituation.resources = z;
                    result.battleSituation.mvpValue = Formula.BattleMVPValue( z, k, d );

                    DebugUtils.LogWarning( DebugUtils.Type.Battle, string.Format( "{0} send battle result data , k = {1}, d = {2}, z ={3}, mvpValue = {4}", DataManager.GetInstance().GetPlayerNickName(), d, k, z, result.battleSituation.mvpValue ) );
                    PostBattleMessage( MsgCode.UploadSituationMessage, result );
                }
            }
            else if ( dead.type == LogicUnitType.Idol )
            {
                RenderMessage rm = new RenderMessage();
                rm.type = RenderMessage.Type.NoticeKillIdol;
                rm.arguments.Add( "killerMark", mark );
                rm.arguments.Add( "killerIconId", killer.iconId );
                PostRenderMessage( rm );
            }
        }

        public LogicUnit FindOpponent()
        {
            LogicUnit target = FindOpponentSoldier( mark, position, attackArea );
            if ( target == null )
            {
                target = FindOpponentDemolisher( mark, position, attackArea );
            }

            return target;
        }

		public void RequestSpawnSoldier( int unitMetaId, int buttonIndex )
        {
            BattlerUnit battlerUnit = battlerUnitDeck.Find( p => p.metaId == unitMetaId );

            if ( battlerUnit != null )
            {
                UnitProto proto = DataManager.GetInstance().unitsProtoData.Find( p => p.ID == unitMetaId );

                // check coins
                if ( coins >= proto.DeploymentCost )
                {
                    // check soldiers count
                    if ( soldiers.Count < GameConstants.PLAYER_CANDEPLOY_UNIT_MAXCOUNT )
                    {
                        DataManager clientData = DataManager.GetInstance();
                        // true
                        Operation operation = new Operation();
                        operation.playerId = clientData.GetPlayerId();
                        operation.opType = OperationType.PlaceUnit;
                        operation.unitMetaId = unitMetaId;

                        UpdateC2S deployUnitMessage = new UpdateC2S();
                        deployUnitMessage.timestamp = clientData.GetFrame();
                        deployUnitMessage.operation = operation;
                        PostBattleMessage( MsgCode.UpdateMessage, deployUnitMessage );

                        // currentWaitingCard can be null, when the pool has empty
                        SquadData currentWaitingCard = GetUnitCardFromDeckPool();

                        RenderMessage rm1 = new RenderMessage();
                        rm1.type = RenderMessage.Type.FillUnitUICard;
                        rm1.arguments.Add( "fillCard", lastWaitingCard );
                        rm1.arguments.Add( "waitingCard", currentWaitingCard );
						rm1.arguments.Add( "buttonIndex", buttonIndex );
                        PostRenderMessage( rm1 );

                        if ( currentWaitingCard != null )
                        {
                            DebugUtils.Log( DebugUtils.Type.AI_Town, string.Format( "Fill card meta id: {0}, index: {1}, waitingcard meta id: {2}, index: {3}", lastWaitingCard.protoData.ID, lastWaitingCard.index, currentWaitingCard.protoData.ID, currentWaitingCard.index ) );
                        }
                        else
                        {
                            DebugUtils.Log( DebugUtils.Type.AI_Town, string.Format( "Fill card meta id: {0}, index:{1}, waitingCard : null", lastWaitingCard.protoData.ID, lastWaitingCard.index ) );
                        }

                        lastWaitingCard = currentWaitingCard;
                    }
                    else
                    {
                        // can't get more unit
                        DebugUtils.Log( DebugUtils.Type.AI_Town, string.Format( "Fill card meta id: {0} failed, because town already has {1} units", lastWaitingCard.protoData.ID, GameConstants.PLAYER_CANDEPLOY_UNIT_MAXCOUNT ) );
                    }
                }
                else
                {
                    // coins was not enough
                    DebugUtils.Log( DebugUtils.Type.AI_Town, string.Format( "Fill card: {0} failed, because coins was not enough, current:{1}, need:{2}", lastWaitingCard.protoData.ID, lastWaitingCard.protoData.DeploymentCost, coins ) );
                }
            }
            else
            {
                // can't find this squad
                DebugUtils.LogError( DebugUtils.Type.AI_Town, string.Format( "Fill card: {0} failed, because can't find this squad card in pool", lastWaitingCard.protoData.ID ) );
            }
        }

        // Be used to PVP, get unit information from battlerList send from server
        // Be used to PVE, get unit information from battlerList send from server
        public Soldier SpawnSoldier( int unitMetaId, FixVector3 pos )
        {
            return SpawnSoldier( unitMetaId, pos, false, battlerUnitDeck.Find( p => p.metaId == unitMetaId ) );
        }
		// use for tap build mode PvP
		public Soldier SpawnSoldier( int unitMetaId )
		{
			return SpawnSoldier( unitMetaId, battlerUnitDeck.Find( p => p.metaId == unitMetaId ) );
		}

        public Soldier SpawnSoldier( int unitMetaId, FixVector3 pos, bool isNpC, BattlerUnit unit )
        {
            //TODO: Use the temp id to avoid error.

            if ( !UNIT_METAID_FLITER.Contains( unitMetaId ) )

            {
                unitMetaId = 30001;
            }

            //TODO: The code needs to be optimized
            Soldier soldier = GenerateSoldier( this, unitMetaId, pos, unit, isNpC, ( s ) =>
            {
                s.RegisterRenderMessageHandler( PostRenderMessage );
                s.RegisterBattleAgentMessageHandler( PostBattleMessage );
                s.RegisterDestroyHandler( PostDestroy );
                s.RegisterDestroyHandler( OnSoldierDestroy );
                s.RegisterRandomMethod( GetRandomNumber );

                s.RegisterGenerateAttributeEffectMethod( GenerateAttributeEffectGenerator );
                s.RegisterSkillGenerator( GenerateSkill );
                s.RegisterProjectileGenerator( GenerateProjectile );
                s.RegisterSummonGenerator( GenerateSummon );
                s.RegisterTrapGenerator( GenerateTrap );

                s.RegisterFindOwnForceSoldiers( FindOwnForceSoldiers );
                s.RegisterFindFriendlySoldiers( FindFriendlySoldiers );
                s.RegisterFindOpponentSoldier( FindOpponentSoldier );
                s.RegisterFindOpponentBuilding( FindOpponentBuilding );
                s.RegisterFindOpponentBuildings( FindOpponentBuildings );
                s.RegisterFindOpponentSoldiers( FindOpponentSoldiers );
                s.RegisterFindOpponentCrystalCar( FindOpponentCrystalCar );
                s.RegisterFindOpponentCrystalCars( FindOpponentCrystalCars );
                s.RegisterFindOpponentDemolisher( FindOpponentDemolisher );
                s.RegisterFindOpponentDemolishers( FindOpponentDemolishers );
                s.RegisterFindNeutralUnit( FindNeutralUnit );
                s.RegisterFindNeutralUnits( FindNeutralUnits );
                s.RegisterFindPowerUp( FindPowerUpMethod );

                s.RegisterGetGroupSoldiers( GetAliveSoldiers );
                s.RegisterCalculateAvoidance( CalculateAvoidance );

                s.RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate );
                s.RegisterWithinFrontRectangleAreaPredicate( WithinFrontRectAreaPredicate );
                s.RegisterWithinFrontSectorAreaPredicate( WithinSectorAreaPredicate );
            } );

            AddCoin( -soldier.spawnCost );

            soldiers.Add( soldier );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnSoldier;
            rm.ownerId = soldier.id;
            rm.position = soldier.position.vector3;
            rm.arguments.Add( "mark", soldier.mark );
            rm.arguments.Add( "type", soldier.unitType );
            rm.arguments.Add( "metaId", soldier.metaId );
            rm.arguments.Add( "hp", soldier.maxHp );

            PostRenderMessage( rm );

            if ( owner )
            {
                //// currentWaitingCard can be null, when the pool has empty
                //SquadData currentWaitingCard = GetUnitCardFromDeckPool();

                //RenderMessage rm1 = new RenderMessage();
                //rm1.type = RenderMessage.Type.FillUnitUICard;
                //rm1.ownerId = soldier.id;
                //rm1.position = soldier.position;
                //rm1.arguments.Add( "fillCard", lastWaitingCard ); 
                //rm1.arguments.Add( "waitingCard", currentWaitingCard );
                //PostRenderMessage( rm1 );

                //if ( currentWaitingCard != null )
                //{
                //    DebugUtils.Log( DebugUtils.Type.AI_Town, "Fill card:" + lastWaitingCard.protoData.ID + " index: " + lastWaitingCard.index + " waitingCard:" + currentWaitingCard.protoData.ID + " index: " + currentWaitingCard.index );
                //}
                //else
                //{
                //    DebugUtils.Log( DebugUtils.Type.AI_Town, "Fill card:" + lastWaitingCard.protoData.ID + " index: " + lastWaitingCard.index + " waitingCard : null" );
                //}

                //lastWaitingCard = currentWaitingCard;
            }

            if ( this.institute != null && this.institute.effectedSoldier != null )
			{
				this.institute.AddSoliderInEffectedSoldier( soldier );
			}

            return soldier;
        }

		//Tap deploy mode use. Dwayne
		public Soldier SpawnSoldier( int unitMetaId, BattlerUnit unit )
		{
            //TODO: Use the temp id to avoid error.
            if ( !UNIT_METAID_FLITER.Contains( unitMetaId ) )
            {
                unitMetaId = 30001;
            }

            DebugUtils.Log( DebugUtils.Type.AI_Town, string.Format( "{0} town spawn soldier {1}", mark, unitMetaId ) );

            FixVector3 pos = GetFormationPos( soldiers );

			//TODO: The code needs to be optimized
			Soldier soldier = GenerateSoldier( this, unitMetaId, pos, unit, false, ( s ) =>
			{
				s.RegisterRenderMessageHandler( PostRenderMessage );
				s.RegisterBattleAgentMessageHandler( PostBattleMessage );
				s.RegisterDestroyHandler( PostDestroy );
				s.RegisterDestroyHandler( OnSoldierDestroy );
				s.RegisterRandomMethod( GetRandomNumber );

				s.RegisterGenerateAttributeEffectMethod( GenerateAttributeEffectGenerator );
				s.RegisterSkillGenerator( GenerateSkill );
				s.RegisterProjectileGenerator( GenerateProjectile );
				s.RegisterSummonGenerator( GenerateSummon );
				s.RegisterTrapGenerator( GenerateTrap );

				s.RegisterFindOwnForceSoldiers( FindOwnForceSoldiers );
				s.RegisterFindFriendlySoldiers( FindFriendlySoldiers );
				s.RegisterFindOpponentSoldier( FindOpponentSoldier );
				s.RegisterFindOpponentBuilding( FindOpponentBuilding );
				s.RegisterFindOpponentBuildings( FindOpponentBuildings );
				s.RegisterFindOpponentSoldiers( FindOpponentSoldiers );
				s.RegisterFindOpponentCrystalCar( FindOpponentCrystalCar );
				s.RegisterFindOpponentCrystalCars( FindOpponentCrystalCars );
				s.RegisterFindOpponentDemolisher( FindOpponentDemolisher );
				s.RegisterFindOpponentDemolishers( FindOpponentDemolishers );
				s.RegisterFindNeutralUnit( FindNeutralUnit );
				s.RegisterFindNeutralUnits( FindNeutralUnits );
				s.RegisterFindPowerUp( FindPowerUpMethod );

				s.RegisterGetGroupSoldiers( GetAliveSoldiers );
				s.RegisterCalculateAvoidance( CalculateAvoidance );

				s.RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate );
				s.RegisterWithinFrontRectangleAreaPredicate( WithinFrontRectAreaPredicate );
				s.RegisterWithinFrontSectorAreaPredicate( WithinSectorAreaPredicate );
			} );

			AddCoin( -soldier.spawnCost );

			soldiers.Add( soldier );

			RenderMessage rm = new RenderMessage();
			rm.type = RenderMessage.Type.SpawnSoldier;
			rm.ownerId = soldier.id;
			rm.position = soldier.position.vector3;
			rm.arguments.Add( "mark", soldier.mark );
			rm.arguments.Add( "type", soldier.unitType );
			rm.arguments.Add( "metaId", soldier.metaId );
			rm.arguments.Add( "hp", soldier.maxHp );

			PostRenderMessage( rm );

			if ( owner )
			{
				//// currentWaitingCard can be null, when the pool has empty
				//SquadData currentWaitingCard = GetUnitCardFromDeckPool();

				//RenderMessage rm1 = new RenderMessage();
				//rm1.type = RenderMessage.Type.FillUnitUICard;
				//rm1.ownerId = soldier.id;
				//rm1.position = soldier.position;
				//rm1.arguments.Add( "fillCard", lastWaitingCard ); 
				//rm1.arguments.Add( "waitingCard", currentWaitingCard );
				//PostRenderMessage( rm1 );

				//if ( currentWaitingCard != null )
				//{
				//	DebugUtils.Log( DebugUtils.Type.AI_Town, "Fill card:" + lastWaitingCard.protoData.ID + " index: " + lastWaitingCard.index + " waitingCard:" + currentWaitingCard.protoData.ID + " index: " + currentWaitingCard.index );
				//}
				//else
				//{
				//	DebugUtils.Log( DebugUtils.Type.AI_Town, "Fill card:" + lastWaitingCard.protoData.ID + " index: " + lastWaitingCard.index + " waitingCard : null" );
				//}

				//lastWaitingCard = currentWaitingCard;
			}

			if ( this.institute != null && this.institute.effectedSoldier != null )
			{
				this.institute.AddSoliderInEffectedSoldier( soldier );
			}

			return soldier;
		}

		private FixVector3 GetFormationPos( List<Soldier> soldierList )
		{
			return unitFormations[ soldierList.Count ];
		}
	
		#region BuildingMode functions

		public Tower SetupTower( FixVector3 pos, int buildTowerID, TowerLocateType locate = TowerLocateType.None )
		{
			Tower tower = GenerateTower( this, buildTowerID, pos );
			tower.RegisterRenderMessageHandler( PostRenderMessage );
			tower.RegisterProjectileGenerator( GenerateProjectile );
			tower.RegisterDestroyHandler( PostDestroy );
			tower.RegisterFindOpponentSoldier( FindOpponentSoldier );
			tower.RegisterFindOpponentSoldiers( FindOpponentSoldiers );
            tower.RegisterFindOpponentCrystalCar( FindOpponentCrystalCar );
            tower.RegisterFindOpponentDemolisher( FindOpponentDemolisher );
            tower.RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate );
			tower.RegisterWithinFrontRectAreaPredicate( WithinFrontRectAreaPredicate );
			tower.RegisterRandomMethod( GetRandomNumber );

			towers.Add( tower );

			AddCoin( -tower.GetBuildCost() );

			RenderMessage rm = new RenderMessage();
			rm.type = RenderMessage.Type.SpawnTower;
			rm.ownerId = tower.id;
			rm.position = pos.vector3;
			rm.arguments.Add( "mark", tower.mark );
			rm.arguments.Add( "model", tower.modelId );
			rm.arguments.Add( "buildTime", ConvertUtils.ToRealFloat( tower.towerBuildTime ) );
            rm.arguments.Add( "hp", tower.maxHp );
            PostRenderMessage( rm );

			return tower;
		}

		public Institute SetUpInstitute( FixVector3 pos )
		{
			Institute institute = GenerateInstitute( this, pos );

			if ( institute.instituteHeadSkills == null ) 
			{
				institute.InitInstituteSkillsData();
			}
				
			institute.RegisterRenderMessageHandler( PostRenderMessage );
			institute.RegisterDestroyHandler( PostDestroy );
			institute.RegisterRandomMethod( GetRandomNumber );
			institute.RegisterGenerateAttributeEffectMethod( GenerateAttributeEffectGenerator );

			this.institute = institute;

			AddCoin( -institute.GetBuildCost() );

			RenderMessage rm = new RenderMessage();
			rm.type = RenderMessage.Type.SpawnInstitute;
			rm.ownerId = institute.id;
			rm.position = pos.vector3;
			rm.arguments.Add( "mark", mark );
			rm.arguments.Add( "hp", institute.maxHp );
			rm.arguments.Add( "buildTime", ConvertUtils.ToRealFloat( institute.buildTime ) );
            rm.arguments.Add( "instituteSkillsData", institute.instituteHeadSkills );
            rm.arguments.Add( "modelId", institute.modelId );
			PostRenderMessage( rm );

			return institute;
		}
			
		public void DeployBuildingAreaOpen( ForceMark mark )
		{
			if( towers != null && towers.Count > 0 )
			{
				for( int i = 0; i < towers.Count; i++ )
				{
					RenderMessage rm = new RenderMessage();
					rm.type = RenderMessage.Type.DeployBuildingAreaOpen;
					rm.arguments.Add( "mark", mark );
					rm.arguments.Add( "TowerID", towers[i].id );
					PostRenderMessage( rm );
				}
			}
		}

		public void DeployBuildingAreaClose( ForceMark mark )
		{
			if( towers != null && towers.Count > 0 )
			{
				for( int i = 0; i < towers.Count; i++ )
				{
					RenderMessage rm = new RenderMessage();
					rm.type = RenderMessage.Type.DeployBuildingAreaClose;
					rm.arguments.Add( "mark", mark );
					rm.arguments.Add( "TowerID", towers[i].id );
					PostRenderMessage( rm );
				}
			}
		}

		public Tower GetForceTower( long inputId )
		{
			for( int i = 0; i < towers.Count; i++ )
			{
				if( towers[i].id == inputId )
				{
					return towers[ i ];
				}
			}

			return null;
		}

		public void RecylingTower( long towerID )
		{
			for( int i = 0; i < towers.Count; i++ )
			{
				if( towers[ i ].id == towerID )
				{
					int recylingMoney = ( int )( towers[ i ].GetBuildCost() * towers[ i ].GetRecycleValue() );
					AddCoin( recylingMoney );
					towers[ i ].RecylingTower();
					towers.Remove( towers[ i ] );
				}
			}

			RenderMessage rm = new RenderMessage();
			rm.type = RenderMessage.Type.RecylingTower;
			rm.ownerId = towerID;
			rm.arguments.Add( "ForceMark", mark );
			PostRenderMessage( rm );
		}

		public bool IsCanDeployInstitute()
		{
			if( this.institute == null )
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public Institute GetInstitute()
		{
			if( this.institute != null )
			{
				return this.institute;
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Building, "Can't find institute!" );
				return null;
			}
		}

		public void RemoveInstitute()
		{
			this.institute = null;
		}

		#endregion 
			
        public Demolisher SetupDemolisher( FixVector3 pos, bool simulate )
        {
            Demolisher demolisher = GenerateDemolisher( this, pos, simulate );

            demolisher.RegisterBattleAgentMessageHandler( PostBattleMessage );
            demolisher.RegisterRenderMessageHandler( PostRenderMessage );
            demolisher.RegisterDestroyHandler( PostDestroy );
            demolisher.RegisterFindOpponentBuilding( FindOpponentBuilding );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnDemolisher;
            rm.ownerId = demolisher.id;
            rm.position = demolisher.position.vector3;
            rm.arguments.Add( "hp", demolisher.maxHp );
            rm.arguments.Add( "modelId", demolisher.modelId );
            rm.arguments.Add( "mark", demolisher.mark );
            PostRenderMessage( rm );

            demolishers.Add( demolisher );

            return demolisher;
        }

        public CrystalCar SetupCrystalCar( FixVector3 pos, bool simulate )
        {
            CrystalCar crystalCar = GenerateCrystalCar( this, pos, simulate );

            crystalCar.RegisterBattleAgentMessageHandler( PostBattleMessage );
            crystalCar.RegisterRenderMessageHandler( PostRenderMessage );
            crystalCar.RegisterFindOpponentCrystal( FindOpponentCrystal );
            crystalCar.RegisterFindOpponentSoldiers( FindOpponentSoldiers );
            crystalCar.RegisterDestroyHandler( PostDestroy );
            crystalCar.RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnCrystalCar;
            rm.ownerId = crystalCar.id;
            rm.position = crystalCar.position.vector3;
            rm.arguments.Add( "hp", crystalCar.maxHp );
            rm.arguments.Add( "mark", crystalCar.mark );
            rm.arguments.Add( "modelId", crystalCar.modelId );
            PostRenderMessage( rm );

            crystalCars.Add( crystalCar );

            return crystalCar;
        }

        public void PickUpPowerUp( PowerUp p )
        {
            for ( int i = 0; i < soldiers.Count; i++ )
            {
                soldiers[i].PickUpPowerUp( p );
            }
        }

        #region Unit Operation

        public void SetSimUnitSkillEnable( bool enable )
        {
            for ( int i = 0; i < soldiers.Count; i++ )
            {
                if ( soldiers[i].Alive() )
                {
                    soldiers[i].SetSkillHandlerEnable( enable );
                }
            }
        }

        // Used by Training mode
        public void ChangeSingleSoldierTarget( long id , FixVector3 d, int pathMask )
        {
            Soldier s = soldiers.Find( p => p.id == id );
            if ( s != null )
            {
                DebugUtils.Log( DebugUtils.Type.Battle, string.Format( " Unit {0} change target to postion, pos = {1} ", id, d ) );
                s.ChangeForceTarget( d, pathMask );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( " Can't find soldier, id = {0}", id ) );
            }
        }

        public void ChangeForceDestination( long unitId, FixVector3 d )
        {
            Soldier s = soldiers.Find( p => p.id == unitId );
            if ( s != null )
            {
                DebugUtils.Log( DebugUtils.Type.Battle, string.Format( "Unit {0} change target to {1} ", unitId, d ) );

                s.ChangeForceTarget( d );
            }
        }

        public void ChangeForceTarget( long unitId, LogicUnit target )
        {
            Soldier s = soldiers.Find( p => p.id == unitId );
            if ( s != null )
            {
                ClearFreePath();

                s.ChangeForceTarget( target );
            }

            DebugUtils.LogWarning( DebugUtils.Type.Battle, string.Format( "{0} force can't find unit {1} , when change unit's target", mark, unitId ) );
        }

        public void ChangeForceDestination( Vector3 d, PathType pathType )
        {
            long frame = DataManager.GetInstance().GetFrame();
            foreach ( KeyValuePair<long, Soldier> s in soldierGroup )
            {
                if ( s.Value.unitMoveMode == pathType )
                {
                    DebugUtils.Log( DebugUtils.Type.Battle, string.Format( "{0} unit change target to a position {1}  pathType = {2}", s.Value.id, d, pathType ) );

                    UpdateC2S message = new UpdateC2S();
                    message.timestamp = frame;

                    Operation operation = new Operation();
                    operation.playerId = id;
                    operation.opType = OperationType.ChangeDestination;
                    operation.unitId = s.Value.id;
                    operation.x = d.x;
                    operation.y = d.y;
                    operation.z = d.z;

                    message.operation = operation;
                    PostBattleMessage( MsgCode.UpdateMessage, message );
                }
            }
        }

        public void ChangeForceTarget( long targetId, LogicUnitType logicUnitType )
        {
            DebugUtils.Log( DebugUtils.Type.Battle, string.Format( "{0} unit change target to chase a target unit, id = {1} ", soldierGroup.Count, targetId ) );
            long frame = DataManager.GetInstance().GetFrame();
            foreach ( KeyValuePair<long, Soldier> s in soldierGroup )
            {
                UpdateC2S message = new UpdateC2S();
                message.timestamp = frame;

                Operation operation = new Operation();
                operation.playerId = id;
                operation.opType = OperationType.ChangeTarget;
                operation.unitId = s.Value.id;
                operation.targetId = targetId;

                message.operation = operation;
                PostBattleMessage( MsgCode.UpdateMessage, message );
            }

            if (soldierGroup.Count > 0)
            {
                RenderMessage rm = new RenderMessage();
                rm.type = RenderMessage.Type.ShowFocusTargetEffect;
                rm.ownerId = targetId;
                rm.arguments.Add( "targetType", logicUnitType );
                PostRenderMessage( rm );
            }
        }

        public void ChangeForceTarget( long unitId, long targetId, LogicUnitType logicUnitType )
        {
            DebugUtils.Log( DebugUtils.Type.Battle, string.Format( "{0} unit change target to chase a target unit, id = {1} ", soldierGroup.Count, targetId ) );

            Soldier soldier = soldiers.Find( s => s.id == unitId );
            if ( soldier != null && soldier.Alive() )
            {
                long frame = DataManager.GetInstance().GetFrame();

                UpdateC2S message = new UpdateC2S();
                message.timestamp = frame;

                Operation operation = new Operation();
                operation.playerId = id;
                operation.opType = OperationType.ChangeTarget;
                operation.unitId = soldier.id;
                operation.targetId = targetId;

                message.operation = operation;
                PostBattleMessage( MsgCode.UpdateMessage, message );

                RenderMessage rm = new RenderMessage();
                rm.type = RenderMessage.Type.ShowFocusTargetEffect;
                rm.ownerId = targetId;
                rm.arguments.Add( "targetType", logicUnitType );
                PostRenderMessage( rm );
            }
        }

        // soldierGroup's path changed by a group of waypoint
        public void ChangeForceTargetByPath( List<FixVector3> p, long unitId, PathType pathType, Boolean isLast )
        {
            // backups code, Drag unit group
            //foreach ( KeyValuePair<long, Soldier> s in soldierGroup )
            //{
            //    if ( s.Value.unitMoveMode == pathType )
            //    {
            //        DebugUtils.Log( DebugUtils.Type.Battle, string.Format( "{0} town {1}, unit {2}  will move by {3} path, Is last path? {4} ", mark, id, s.Value.id, pathType, isLast ) );

            //        s.Value.ChangeForceTarget( p, isLast );
            //    }
            //}

            Soldier s = null;
            if ( soldierGroup.ContainsKey( unitId ) )
            {
                s = soldierGroup[unitId];
            }
            else
            {
                s = soldiers.Find( u => u.id == unitId );
            }

            if ( s != null && s.unitMoveMode == pathType )
            {
                DebugUtils.Log( DebugUtils.Type.Battle, string.Format( "{0} town {1}, unit {2}  will move by {3} path, Is last path? {4} ", mark, id, s.id, pathType, isLast ) );

                s.ChangeForceTarget( p, isLast );
            }
        }

        public void MapAreaCollision( long unitId, int collisionType, int collisionState )
        {
            Soldier s = soldiers.Find( p => p.id == unitId );

            if ( s != null && s.Alive() )
            {
                if ( collisionState == 1 )
                {
                    s.MapAreaCollisionStart( collisionType );
                }
                else if ( collisionState == 2 )
                {
                    s.MapAreaCollisionEnd( collisionType );
                }
                else
                {
                    DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( "Can't handle this map area collision state {0} now!", collisionType ) );
                }
            }
        }

        public void ClearFreePath()
        {
            for ( int i = 0; i < soldiers.Count; i++ )
            {
                soldiers[i].ClearPathRender();
            }
        }
        #endregion

        public override void AddCoin( int coin )
        {
            coins = coins + coin;

            RenderMessage renderMessage = new RenderMessage();
            renderMessage.type = RenderMessage.Type.CoinChanged;
            renderMessage.ownerId = id;
            renderMessage.arguments.Add( "value", coins );
            PostRenderMessage( renderMessage );

            PveGetResourcesNumber( coin );
        }

        //TODO : The number of PVE access to resources statistics
        private void PveGetResourcesNumber(int num)
        {
            if (num <= 0) return;

            DataManager clientData = DataManager.GetInstance();

            clientData.AddPlayerTotalResources( mark , num );
        }

        public override void Reset()
        {
            
        }

        private int GetTownActualHurtValue( AttackPropertyType type, int hurtValue )
        {
            int value = 0;

            if ( type == AttackPropertyType.PhysicalAttack )
            {
                value = Formula.ActuallyPhysicalDamage( hurtValue, physicalResistance );
            }
            //else if ( type == HurtType.MagicAttack )
            //{
            //	value = (int)( hurtValue / ( magicResistance / 100 + 1 ) );
            //}
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Town, string.Format( "Can't handle this hurtType {0} now!", type ) );
            }

            return value;
        }

        public int GetDamageValue()
        {
            return Formula.GetAttackFloatingValue( rangeDmgBase, GetRandomNumber(), GetRandomNumber( rangeDmgVar ) );
        }

        public List<Soldier> GetAliveSoldiers()
        {
            return soldiers;
        }

        private void OnSoldierDestroy( LogicUnit unit )
        {
            if ( unit.type == LogicUnitType.Soldier )
            {
                Soldier soldier = (Soldier)unit;
                soldiers.Remove( soldier );
                soldierGroup.Remove( unit.id );

                if ( this.institute != null && this.institute.effectedSoldier != null )
                {
                    this.institute.RemoveSoliderInEffectedSoldier( soldier );
                }

                DataManager clientData = DataManager.GetInstance();
                clientData.AddPlayerFatality( mark );

                if ( clientData.GetForceMark() == mark )
                {
                    int d = clientData.GetPlayerFatality( clientData.GetForceMark() );
                    int k = clientData.GetPlayerKillCount( clientData.GetForceMark() );
                    int z = clientData.GetPlayerTotalResources( clientData.GetForceMark() );

                    // Send battle data to server
                    UploadSituationC2S result = new UploadSituationC2S();
                    result.type = 1;
                    result.battleSituation = new BattleSituation();
                    result.battleSituation.playerId = clientData.GetPlayerId();
                    result.battleSituation.fatality = d;
                    result.battleSituation.kills = k;
                    result.battleSituation.resources = z;
                    result.battleSituation.mvpValue = Formula.BattleMVPValue( z, k, d );

                    DebugUtils.LogWarning( DebugUtils.Type.Battle, string.Format( "{0} send battle result data , k = {1}, d = {2}, z ={3}, mvpValue = {4}", DataManager.GetInstance().GetPlayerNickName(), d, k, z, result.battleSituation.mvpValue ) );
                    PostBattleMessage( MsgCode.UploadSituationMessage, result );
                }

                if ( owner )
                {
                    if ( unitCardPool.Count == 0 && lastWaitingCard == null )
                    {
                        AddUnitCardToDeckPool( unit.metaId );

                        SquadData currentWaitingCard = GetUnitCardFromDeckPool();

                        // waitingCard also can be null, when unit death and need to fill the empty ui card
                        RenderMessage rm = new RenderMessage();
                        rm.type = RenderMessage.Type.FillUnitUICard;
                        rm.ownerId = soldier.id;
                        rm.position = soldier.position.vector3;
                        rm.arguments.Add( "fillCard", lastWaitingCard ); // null  
                        rm.arguments.Add( "waitingCard", currentWaitingCard ); // filled waiting card
						rm.arguments.Add( "buttonIndex", 3 );// waiting card num
                        PostRenderMessage( rm );

                        DebugUtils.Log( DebugUtils.Type.AI_Town, "Unit death, Fill waiting card:" + currentWaitingCard.protoData.ID + " index: " + currentWaitingCard.index );

                        lastWaitingCard = currentWaitingCard;
                    }
                    else
                    {
                        AddUnitCardToDeckPool( unit.metaId );

                        DebugUtils.Log( DebugUtils.Type.AI_Town, "Unit death, add unit go back to pool:" + unit.metaId );
                    }
                }
            }
        }

        public void SetCameraFollowUnit()
        {
            if ( soldiers.Count > 0 )
            {
                int rollIndex = GetRandomNumber() % soldiers.Count;
                Soldier cameraTarget = soldiers[rollIndex];
                
                RenderMessage rm = new RenderMessage();
                rm.ownerId = cameraTarget.id;
                rm.type = RenderMessage.Type.SetCameraFollow;
                rm.arguments.Add( "targetType", (int)cameraTarget.type );
                PostRenderMessage( rm );
            }
            else
            {
                RenderMessage rm = new RenderMessage();
                rm.ownerId = id;
                rm.type = RenderMessage.Type.SetCameraFollow;
                rm.arguments.Add( "targetType", type );
                PostRenderMessage( rm );
            }
        }

        #region Soldier Group Operation
        public bool IsSoldierInGroup( long ownerId )
        {
            return soldierGroup.ContainsKey( ownerId );
        }

        private void MakeANewGroup()
        {
            foreach ( KeyValuePair<long, Soldier> s in soldierGroup )
            {
                if ( s.Value != null )
                {
                    RenderMessage message = new RenderMessage();
                    message.type = RenderMessage.Type.SoldierSelected;
                    message.ownerId = s.Value.id;
                    message.arguments.Add( "value", false );
                    PostRenderMessage( message );
                }
            }

            soldierGroup = new Dictionary<long, Soldier>();
        }

        private void AddMemberInSelectionGroup( long ownerId )
        {
            if ( !soldierGroup.ContainsKey( ownerId ) )
            {
                Soldier soldier = soldiers.Find( p => p.id == ownerId );

                if ( soldier!= null && soldier.Alive() )
                {
                    soldierGroup.Add( ownerId, soldier );

                    RenderMessage message = new RenderMessage();
                    message.type = RenderMessage.Type.SoldierSelected;
                    message.ownerId = ownerId;
                    message.arguments.Add( "value", true );
                    PostRenderMessage( message );

                    DebugUtils.Log( DebugUtils.Type.Battle, string.Format( "{0} town set unit {1} as selected", mark, ownerId ) );
                }
            }
        }

        // Create a new group will unselect all unit in group.
        public void SelectUnit( long ownerId, bool replaceGroup )
        {
            if ( replaceGroup )
            {
                // Select a single unit will unselect all other unit, and make itself selected ( group.count > 1 ).
                if ( soldierGroup.Count > 1 )
                {
                    if ( soldierGroup.ContainsKey( ownerId ) )
                    {
                        // Invert Selection
                        MakeANewGroup();
                    }

                    AddMemberInSelectionGroup( ownerId );
                }
                else if ( soldierGroup.Count == 1 )
                {
                    if ( soldierGroup.ContainsKey( ownerId ) )
                    {
                        // Select a single unit will unselect itself ( group only contains this selected unit )
                        soldierGroup.Remove( ownerId );
                        RenderMessage message = new RenderMessage();
                        message.type = RenderMessage.Type.SoldierSelected;
                        message.ownerId = ownerId;
                        message.arguments.Add( "value", false );
                        PostRenderMessage( message );
                    }
                    else
                    {
                        AddMemberInSelectionGroup( ownerId );
                    }
                }
                else
                {
                    AddMemberInSelectionGroup( ownerId );
                }
            }
            else
            {
                AddMemberInSelectionGroup( ownerId );
            }
        }

        public int GetUnitGroupCount()
        {
            return soldierGroup.Count;
        }

        public void DrawUnitsPathPoint( long unitId, Vector3 pos, PathType pathType )
        {
            Soldier s = null;
            if ( soldierGroup.ContainsKey( unitId ) )
            {
                s = soldierGroup[unitId];
            }
            else
            {
                s = soldiers.Find( p => p.id == unitId );
            }

            if ( s != null && s.unitMoveMode == pathType )
            {
                RenderMessage rm = new RenderMessage();
                rm.ownerId = s.id;
                rm.type = RenderMessage.Type.DrawPathPoint;
                rm.position = pos;
                rm.arguments.Add( "type", 1 );
                PostRenderMessage( rm );
            }
        }

        public void SelectAllActiveUnits( bool invert )
        {
            if ( soldiers.Count == soldierGroup.Count )
            {
                if ( invert )
                {
                    // if all units be selected, then this op means cancel all selected
                    MakeANewGroup();
                }
                else
                {
                    // all unit already selected
                    // do nothing
                }
            }
            else
            {
                for ( int i = 0; i < soldiers.Count; i++ )
                {
                    if ( !soldierGroup.ContainsKey( soldiers[i].id ) )
                    {
                        soldierGroup.Add( soldiers[i].id, soldiers[i] );
                        RenderMessage message = new RenderMessage();
                        message.type = RenderMessage.Type.SoldierSelected;
                        message.ownerId = soldiers[i].id;
                        message.arguments.Add( "value", true );
                        PostRenderMessage( message );
                    }
                    else
                    {
                        // already selected!
                    }
                }
            }
        }
        #endregion

        #region Register Handler
        public void RegisterAttributeEffectGenerator( AttributeEffectGenerator method )
        {
            GenerateAttributeEffectGenerator = method;
        }

        public void RegisterSkillGenerator( SkillGenerator method )
        {
            GenerateSkill = method;
        }

        public void RegisterSoldierGenerator( SoldierGenerator generator )
        {
            GenerateSoldier = generator;
        }

		public void RegisterTowerGenerator( TowerGenerator generator )
		{
			GenerateTower = generator;
		}

		public void RegisterInstituteGenerator( InstituteGenerator generator )
		{
			GenerateInstitute = generator;
		}

        public void RegisterProjectileGenerator( ProjectileGenerator generator )
        {
            GenerateProjectile = generator;
        }
		
		public void RegisterCrystalCarGenerator( CrystalCarGenerator generator )
        {
            GenerateCrystalCar = generator;
        }

        public void RegisterDemolisherGenerator( DemolisherGenerator generator )
        {
            GenerateDemolisher = generator;
        }

        public void RegisterSummonGenerator( SummonGenerator generator )
        {
            GenerateSummon = generator;
        }

        public void RegisterFindOwnForceSoldiers( FindOwnForceSoldiersMethod method )
        {
            FindOwnForceSoldiers = method;
        }

        public void RegisterFindOpponentSoldier( FindOpponentSoldierMethod method )
        {
            FindOpponentSoldier = method;
        }

        public void RegisterFindOpponentSoldiers( FindOpponentSoldiersMethod method )
        {
            FindOpponentSoldiers = method;
        }

        public void RegisterFindFriendlySoldiers( FindFriendlySoldiersMethod method )
        {
            FindFriendlySoldiers = method;
        }

        public void RegisterFindOpponentBuilding( FindOpponentBuildingMethod method )
        {
            FindOpponentBuilding = method;
        }

        public void RegisterFindOpponentBuildings( FindOpponentBuildingsMethod method )
        {
            FindOpponentBuildings = method;
        }

        public void RegisterCalculateAvoidance( CalculateAvoidanceMethod method )
        {
            CalculateAvoidance = method;
        }

        public void RegisterFindOpponentCrystal( FindOpponentCrystalMethod method )
        {
            FindOpponentCrystal = method;
        }

        public void RegisterFindOpponentCrystalCar( FindOpponentCrystalCarMethod method )
        {
            FindOpponentCrystalCar = method;
        }

        public void RegisterFindOpponentCrystalCars( FindOpponentCrystalCarsMethod method )
        {
            FindOpponentCrystalCars = method;
        }

        public void RegisterFindOpponentDemolisher( FindOpponentDemolisherMethod method )
        {
            FindOpponentDemolisher = method;
        }

        public void RegisterFindOpponentDemolishers( FindOpponentDemolishersMethod method )
        {
            FindOpponentDemolishers = method;
        }

        public void RegisterFindNeutralUnit( FindNeutralUnitMethod method )
        {
            FindNeutralUnit = method;
        }

        public void RegisterFindNeutralUnits( FindNeutralUnitsMethod method )
        {
            FindNeutralUnits = method;
        }

        public void RegisterFindPowerUp( FindPowerUpMethod method )
        {
            FindPowerUpMethod = method;
        }

        public void RegisterWithinFrontRectAreaPredicate( WithinFrontRectAreaPredicate method )
        {
            WithinFrontRectAreaPredicate = method;
        }

        public void RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate method )
        {
            WithinCircleAreaPredicate = method;
        }

        public void RegisterWithinFrontSectorAreaPredicate( WithinSectorAreaPredicate method )
        {
            WithinSectorAreaPredicate = method;
        }

        public void RegisterTrapGenerator( TrapGenerator method )
        {
            GenerateTrap = method;
        }
        #endregion
    }
}
