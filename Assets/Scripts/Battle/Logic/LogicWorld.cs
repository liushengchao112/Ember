/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: LogicWorld.cs
// description: 
// 
// created time：09/28/2016
//
//----------------------------------------------------------------*/

using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

using Data;
using BattleAgent;
using Utils;
using Constants;

using SkillProto = Data.UnitSkillsProto.UnitSkill;
using TowerProto = Data.TowerProto.Tower;
using AttributeEffect = Data.AttributeEffectProto.AttributeEffect;
using BattlerUnit = Data.Battler.BattleUnit;

namespace Logic
{
    public delegate void RenderMessageHandler( RenderMessage message );

    public delegate Soldier FindSoldierMethod( FixVector3 center, long radius );
    public delegate List<Soldier> FindOwnForceSoldiersMethod( ForceMark mark, FixVector3 center, long radius );
    public delegate List<Soldier> FindFriendlySoldiersMethod( ForceMark mark, FixVector3 center, long radius );
    public delegate Soldier FindOpponentSoldierMethod( ForceMark mark, FixVector3 center, long radius );
    public delegate List<Soldier> FindOpponentSoldiersMethod( ForceMark mark, FixVector3 center, Predicate<Soldier> match );
    public delegate Crystal FindOpponentCrystalMethod( FixVector3 center, long radius );
    public delegate CrystalCar FindOpponentCrystalCarMethod( ForceMark mark, FixVector3 center, long radius );
    public delegate List<CrystalCar> FindOpponentCrystalCarsMethod( ForceMark mark, FixVector3 center, Predicate<CrystalCar> match );
    public delegate Demolisher FindOpponentDemolisherMethod( ForceMark mark, FixVector3 center, long radius );
    public delegate List<Demolisher>  FindOpponentDemolishersMethod( ForceMark mark, FixVector3 center, Predicate<Demolisher> match );
    public delegate FixVector3 CalculateAvoidanceMethod( Soldier looker, FixVector3 ahead );

    public delegate LogicUnit FindNeutralUnitMethod( FixVector3 center, long radius );
    public delegate List<LogicUnit> FindNeutralUnitsMethod( FixVector3 center, Predicate<LogicUnit> match );

    public delegate LogicUnit FindOpponentBuildingMethod( ForceMark mark, FixVector3 center, long radius );
    public delegate List<LogicUnit> FindOpponentBuildingsMethod( ForceMark mark, FixVector3 center, long radius );

    public delegate PowerUp FindPowerUpMethod( FixVector3 center, long radius );

    // public delegate Debuff DebuffGenerator( int metaId );
    public delegate AttributeEffect AttributeEffectGenerator( int metaId );
    public delegate Skill SkillGenerator( Soldier owner, SkillProto skillProto, int index );
    public delegate Soldier SoldierGenerator( Town town, int metaId, FixVector3 pos, BattlerUnit unitInfo, bool isNpc, Action<Soldier> onCompleteUnitBehavior );
	public delegate Tower TowerGenerator( Town town, int type, FixVector3 pos );
	public delegate Institute InstituteGenerator( Town town, FixVector3 pos );
    public delegate Projectile ProjectileGenerator( LogicUnit owner, int metaId, FixVector3 position, LogicUnit target );
    public delegate CrystalCar CrystalCarGenerator( Town town, FixVector3 pos, bool setAsNpc );
    public delegate Demolisher DemolisherGenerator( Town town, FixVector3 pos, bool setAsNpc );
    public delegate SummonedUnit SummonGenerator( Skill owner, int metaId, FixVector3 pos, FixVector3 rotation );
    public delegate Trap TrapGenerator( LogicUnit owner , int metaId , FixVector3 position , FixVector3 rotation );

    public delegate void DestroyHandler( LogicUnit unit );
    public delegate int GetRandomNumberMethod( int range = 1000 );
    public delegate List<Soldier> GetGroupSoldiersMethod();
    public delegate void BattleMessageHandler( MsgCode protocolCode, object obj );

    public delegate bool WithinFrontRectAreaPredicate( FixVector3 orgin, Quaternion r, long width, long height, FixVector3 position );
    public delegate bool WithinCircleAreaPredicate( FixVector3 orgin, long radius, FixVector3 position );
    public delegate bool WithinSectorAreaPredicate( FixVector3 orgin, FixVector3 direction, long radius, FixVector3 position );

    public class LogicWorld
    {
        const string ATTRIBUTE_EFFECT = "ATTRIBUTE_EFFECT";
        const string MOVEABLE = "Moveable";
        const string PROJECTILE_ID = "PROJECTILE";
        const string POWERUP_ID = "POWERUP";
        const string SKILL_ID = "SKILL";
        const string TARGET_UNIT = "TARGET_UNIT";
        const string SUMMON = "SUMMON";
		const string TRAP_ID = "TRAP";

        //private const float BATTLE_TIME = 0;

        private int timer;

        private System.Random random;

        private PathAgentType pathMethodType = PathAgentType.NavMesh;

        private RenderMessageHandler PostRenderMessage;
        private BattleMessageHandler PostAgentMessage;

        // forces
        public Dictionary<ForceMark, Town> forces;

        private Dictionary<long, Town> towns;
        public Dictionary<MatchSide, List<Town>> sideTowns;

        private Dictionary<long, Tower> towers;
		private Dictionary<int, int> towersBuildingArea;
		public Dictionary<MatchSide, List<Tower>> sideTowers;

		private Dictionary<long, Institute>institutes;
		private Dictionary<MatchSide,List<Institute>> sideInstitutes;

        public Dictionary<long, Soldier> soldiers;//fix me
        private Dictionary<MatchSide, List<Soldier>> sideSoldiers;

        private Dictionary<long, Demolisher> demolishers;
        private Dictionary<MatchSide, List<Demolisher>> sideDemolishers;

        private Dictionary<long, CrystalCar> crystalCars;
        private Dictionary<MatchSide, List<CrystalCar>> sideCrystalCars;

        private Dictionary<long, Projectile> projectiles;
        private Dictionary<long, SummonedUnit> summons;

        private Dictionary<long, LogicUnit> buildings;
        private Dictionary<MatchSide, List<LogicUnit>> sideBuildings;

        private Dictionary<long, Crystal> crystals;

        private Dictionary<long, Npc> npcs;
        private Dictionary<long, IdolGuard> idolGuards;
        private Dictionary<long, Idol> idols;
        private Dictionary<long, LogicUnit> neutralUnits;

        private Dictionary<long, PowerUp> powerUps;

        private Dictionary<long, Skill> skills;

        private Dictionary<long , Trap> traps;

        private List<LogicUnit> garbges;


        private BattlePoolGroop skillPoolGroup;
        private BattlePool soldierPool;
        private BattlePoolGroop projectilePoolGroup;
        private BattlePool crystalCarPool;
        private BattlePool crystalPool;
        private BattlePool demolisherPool;
        private BattlePool npcPool;
        private BattlePool institutePool;
        private BattlePool towerPool;
        private BattlePool summonedUnitPool;
        private BattlePoolGroop trapPoolGroup;

        private List<Idol> idolPool;
        private List<IdolGuard> idolGuardPool;


        public Vector3 powerUpsBornPos;

        public LogicWorld()
        {
            IdGenerator.ResetAll();

            //timer = BATTLE_TIME;

            random = new System.Random( (int)DataManager.GetInstance().GetSeed() );

            forces = new Dictionary<ForceMark, Town>();

            towns = new Dictionary<long, Town>();
            sideTowns = new Dictionary<MatchSide, List<Town>>();
            sideTowns[MatchSide.Red] = new List<Town>();
            sideTowns[MatchSide.Blue] = new List<Town>();

            towers = new Dictionary<long, Tower>();
            sideTowers = new Dictionary<MatchSide, List<Tower>>();
            sideTowers[MatchSide.Red] = new List<Tower>();
            sideTowers[MatchSide.Blue] = new List<Tower>();

			institutes = new Dictionary<long, Institute>();
			sideInstitutes = new Dictionary<MatchSide, List<Institute>>();
			sideInstitutes[ MatchSide.Red ] = new List<Institute>();
			sideInstitutes[ MatchSide.Blue ] = new List<Institute>();

            soldiers = new Dictionary<long, Soldier>();
            sideSoldiers = new Dictionary<MatchSide, List<Soldier>>();
            sideSoldiers[MatchSide.Red] = new List<Soldier>();
            sideSoldiers[MatchSide.Blue] = new List<Soldier>();

            crystals = new Dictionary<long, Crystal>();

            projectiles = new Dictionary<long, Projectile>();
            projectilePoolGroup = new BattlePoolGroop();

            demolishers = new Dictionary<long, Demolisher>();
            sideDemolishers = new Dictionary<MatchSide, List<Demolisher>>();
            sideDemolishers[MatchSide.Red] = new List<Demolisher>();
            sideDemolishers[MatchSide.Blue] = new List<Demolisher>();
            demolisherPool = new BattlePool();

            crystalCars = new Dictionary<long, CrystalCar>();
            sideCrystalCars = new Dictionary<MatchSide, List<CrystalCar>>();
            sideCrystalCars[MatchSide.Red] = new List<CrystalCar>();
            sideCrystalCars[MatchSide.Blue] = new List<CrystalCar>();
            crystalCarPool = new BattlePool();

            buildings = new Dictionary<long, LogicUnit>();
            sideBuildings = new Dictionary<MatchSide, List<LogicUnit>>();
            sideBuildings[MatchSide.Red] = new List<LogicUnit>();
            sideBuildings[MatchSide.Blue] = new List<LogicUnit>();

            powerUps = new Dictionary<long, PowerUp>();
            skills = new Dictionary<long, Skill>();
            npcs = new Dictionary<long, Npc>();
            idols = new Dictionary<long, Idol>();
            idolGuards = new Dictionary<long, IdolGuard>();
            neutralUnits = new Dictionary<long, LogicUnit>();
            summons = new Dictionary<long, SummonedUnit>();
            summonedUnitPool = new BattlePool();

            soldierPool = new BattlePool();
            skillPoolGroup = new BattlePoolGroop();
            crystalPool = new BattlePool();
            npcPool = new BattlePool();

            idolPool = new List<Idol>();
            idolGuardPool = new List<IdolGuard>();

            garbges = new List<LogicUnit>();

            towerPool = new BattlePool();
            institutePool = new BattlePool();

            traps = new Dictionary<long , Trap>();
            trapPoolGroup = new BattlePoolGroop();
        }

        public void Release()
        {
            soldierPool.Release();
            skillPoolGroup.Release();
            crystalPool.Release();
            npcPool.Release();
            towerPool.Release();
            institutePool.Release();
            summonedUnitPool.Release();
        }

		public void SetupForce( Battler battler, Vector3 position, List<Vector3> unitFormation )
        {
            List<FixVector3> formations = ConvertUtils.ToFixVector3List( unitFormation );
            FixVector3 forcePostion = (FixVector3)position;

            long id = battler.playerId;
            Town town = LogicUnit.Create<Town>( id.ToString() );
            MatchSide side = battler.side;
            ForceMark mark = battler.forceMark;

            town.position = forcePostion;
            town.transform.position = forcePostion.vector3;

            town.RegisterRenderMessageHandler( PostRenderMessage );
            town.RegisterBattleAgentMessageHandler( PostAgentMessage );
            town.RegisterDestroyHandler( PrepareToDestroy );

            town.RegisterAttributeEffectGenerator( GenerateAttributeEffect );
            town.RegisterSkillGenerator( GenerateSoldierSkill );
            town.RegisterProjectileGenerator( GenerateProjectile );
            town.RegisterSoldierGenerator( GenerateSoldier );
			town.RegisterTowerGenerator( GenerateTower );
			town.RegisterInstituteGenerator( GenerateInstitute );
			town.RegisterCrystalCarGenerator( GenerateCrystalCar );
            town.RegisterDemolisherGenerator( GenerateDemolisher );
            town.RegisterSummonGenerator( GenerateSummon );
            town.RegisterTrapGenerator( GenerateTrap );

            town.RegisterFindOwnForceSoldiers( FindOwnForceSoldiers );
            town.RegisterFindFriendlySoldiers( FindFriendlySoldiers );
            town.RegisterFindOpponentSoldier( FindOpponentSoldier );
            town.RegisterFindOpponentSoldiers( FindOpponentSoldiers );
            town.RegisterFindOpponentBuilding( FindOpponentBuilding );
            town.RegisterFindOpponentBuildings( FindOpponentBuildings );

            town.RegisterFindOpponentCrystal( FindOpponentCrystal );
            town.RegisterFindOpponentCrystalCar( FindOpponentCrystalCar );
            town.RegisterFindOpponentCrystalCars( FindOpponentCrystalCars );
            town.RegisterFindOpponentDemolisher( FindOpponentDemolisher );
            town.RegisterFindOpponentDemolishers( FindOpponentDemolishers );
            town.RegisterFindNeutralUnit( FindNeutralUnit );
            town.RegisterFindNeutralUnits( FindNeutralUnits );

            town.RegisterFindPowerUp( FindPowerUp );

            town.RegisterCalculateAvoidance( CalculateAvoidance );
            town.RegisterRandomMethod( GetRandomNumber );

            town.RegisterWithinCircleAreaPredicate( AreaDecisionUtils.WithInCircleArea );
            town.RegisterWithinFrontRectAreaPredicate( AreaDecisionUtils.WithInFrontRectArea );
            town.RegisterWithinFrontSectorAreaPredicate( AreaDecisionUtils.WithInFrontSectorArea );

			town.Initialize( id, mark, battler.battleUnits, formations );

            forces.Add( mark, town );
            towns.Add( id, town );
            sideTowns[side].Add( town );
            sideBuildings[side].Add( town );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnTown;
            rm.ownerId = town.id;
            rm.position = town.position.vector3;
            rm.arguments.Add( "mark", mark );
            rm.arguments.Add( "hp", town.maxHp );
            rm.arguments.Add( "model", town.modelId );
            PostRenderMessage( rm );
        }

		#region BuildingMode functions

		//Player use
		public void SetupTower( long townId, FixVector3 pos, int buildID )
		{
			Tower tower = towns[townId].SetupTower( pos, buildID, TowerLocateType.None );

			DebugUtils.Log( DebugUtils.Type.Building, towns[townId].mark + " town " + townId + " spawn tower " + tower.id );

			MatchSide side = GetSideFromMark( towns[townId].mark );
			towers.Add( tower.id, tower );
			sideBuildings[side].Add( tower );
			sideTowers[side].Add( tower );
		}

		//PvE AI use
		//Now AI not have build ability.
		/*public void SetupTower( ForceMark mark, long id, Vector3 pos, TowerLocateType locate = TowerLocateType.None, int buildTowerID = 0 )
		{
			Tower tower = LogicUnit.Create<Tower>( id.ToString() );
			tower.Initialize( id, forces[mark], pos, buildTowerID, locate );
			tower.RegisterRenderMessageHandler( PostRenderMessage );
			tower.RegisterProjectileGenerator( GenerateProjectile );
			tower.RegisterDestroyHandler( PrepareToDestroy );
			tower.RegisterFindOpponentSoldier( FindOpponentSoldier );
			tower.RegisterFindOpponentSoldiers( FindOpponentSoldiers );
			tower.RegisterWithinCircleAreaPredicate( AreaDecisionUtils.WithInCircleArea );
			tower.RegisterRandomMethod( GetRandomNumber );

			MatchSide side = GetSideFromMark(mark);

			towers.Add( id, tower );
			sideBuildings[side].Add( tower );
			sideTowers[side].Add( tower );

			RenderMessage rm = new RenderMessage();
			rm.type = RenderMessage.Type.SpawnTower;
			rm.ownerId = tower.id;
			rm.position = pos;
			//TODO:Need add build time.
			rm.arguments.Add( "mark", tower.mark );
			rm.arguments.Add( "model", tower.modelId );
			rm.arguments.Add( "hp", tower.maxHp );
			rm.arguments.Add( "towerTypeID", tower.towerTypeID );//TODO:When guocheng finished table change this to TowerType.
			PostRenderMessage( rm );
		}*/
			
		public void SetupInstitute( long townId, FixVector3 pos )
		{
			Institute institute = towns[townId].SetUpInstitute( pos );
			MatchSide side = GetSideFromMark( towns[townId].mark );
			institutes.Add( institute.id, institute );
			sideBuildings[side].Add( institute );
			sideInstitutes[side].Add( institute );
		}

		public void InstituteLevelUp( long playerId )
		{
			towns[playerId].InstituteLevelUp();
			DebugUtils.Log( DebugUtils.Type.BuildingLevelUp, "The player ID is :" + playerId + " instituteLevelUp " );
		}

		public void InstituteSkillLevelUp( long playerId, int skillID, int upgradeNum )
		{
			towns[playerId].InstituteSkillLevelUp( skillID, upgradeNum );
			DebugUtils.Log( DebugUtils.Type.InstitutesSkill, string.Format( "LogicWorld receive the InstituteSkillLevelUp message, PlayerID is : {0}, SkillID is : {1}, Upgrade number ot time : {2}.", playerId, skillID, upgradeNum ));
		}

		public bool IsCanDeployBuildingFromTower( ForceMark mark, FixVector3 pos )
		{
			List<Tower> towers;

            MatchSide side = GetSideFromMark( mark );
			sideTowers.TryGetValue( side, out towers );

			if( towers != null )
			{
				if( towersBuildingArea == null )
				{
					towersBuildingArea = new Dictionary<int, int>();
					List<TowerProto> temp = DataManager.GetInstance().towerProtoData;

					for( int i = 0; i < temp.Count; i++ )
					{
                        towersBuildingArea.Add( temp[i].ID, ConvertUtils.ToLogicInt( temp[i].BuildingArea ) );
                    }
                }
				
				for( int i = 0; i < towers.Count; i++ )
				{
					int buildingArea;
					towersBuildingArea.TryGetValue( towers[i].towerTypeID, out buildingArea );

					if( ( towers[i].position - pos ).magnitude <= buildingArea )
					{
						return true;
					}
				}

				return false;
			}
			else
			{
				DebugUtils.Log( DebugUtils.Type.Building, "Player not have tower to support build building！" );
				return false;
			}
		}

		public void ShowDeployBuildingArea( ForceMark mark )
		{
			Town town;
			forces.TryGetValue( mark, out town );

			if( town != null )
			{
				town.DeployBuildingAreaOpen( mark );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Building, "This mark :" + mark + "can't find town!" );
			}
		}

		public void CloseDeployBuildingArea( ForceMark mark )
		{
			Town town;
			forces.TryGetValue( mark, out town );

			if( town != null )
			{
				town.DeployBuildingAreaClose( mark );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Building, "This mark :" + mark + "can't find town!" );
			}
		}

		public void TapTower( ForceMark mark, long id )
		{
			Town town;
			forces.TryGetValue( mark, out town );
			Tower tower = town.GetForceTower( id );

			if( tower != null )
			{
				RenderMessage rm = new RenderMessage();
				rm.type = RenderMessage.Type.TapTower;
				rm.position = tower.position.vector3;
				rm.ownerId = id;

				int recylingMoney = ( int )( tower.GetBuildCost() * tower.GetRecycleValue() );

				rm.arguments.Add( "RecylingMoney", recylingMoney );
				PostRenderMessage( rm );

				DebugUtils.Log( DebugUtils.Type.Building, "Taped tower pos is :" + tower.position );
			}
			else
			{
				DebugUtils.Log( DebugUtils.Type.Building, "Taped tower is die." );
			}
		}

		public void RecylingTower( long playerID, long towerID )
		{
			towns[ playerID ].RecylingTower( towerID );
		}

		public void TapInstitute( ForceMark mark, long id )
		{
			Town town;
			forces.TryGetValue( mark, out town );
			Institute institute = town.GetInstitute();

			RenderMessage rm = new RenderMessage();
			rm.type = RenderMessage.Type.TapInstitute;
			rm.position = institute.position.vector3;
			rm.ownerId = id;

			PostRenderMessage( rm );
		}
			
		#endregion

        public void SetupCrystal( Vector3 pos, Quaternion angle, bool isPlusCrystal )
        {
            int id = IdGenerator.GenerateIntId( TARGET_UNIT );

            Crystal crystal = LogicUnit.Create<Crystal>( id.ToString() );
            crystal.Initialize( id, (FixVector3)pos, (FixVector3)angle.eulerAngles, isPlusCrystal );
            crystal.RegisterRenderMessageHandler( PostRenderMessage );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnCrystal;
            rm.ownerId = id;
            rm.position = pos;
            rm.direction = crystal.transform.eulerAngles;
            rm.arguments.Add( "hp", crystal.maxHp );
            rm.arguments.Add( "modelId", crystal.modelId );
            rm.arguments.Add( "plus", crystal.crystalPlus );
            PostRenderMessage( rm );

            crystalPool.AddUsedUnit( crystal );
            crystals.Add( id, crystal );
        }

        public void SpawnPowerup( PowerUpType type, FixVector3 pos )
        {
            if ( powerUps != null )
            {
                foreach ( KeyValuePair<long, PowerUp> p in powerUps )
                {
                    p.Value.PickedUp();
                }
                powerUps.Clear();
            }

            int id = IdGenerator.GenerateIntId( POWERUP_ID );

            PowerUp powerup = LogicUnit.Create<PowerUp>( id.ToString() );
            powerup.type = LogicUnitType.PowerUp;
            powerup.Initialize( id, type, pos );
            powerup.RegisterRenderMessageHandler( PostRenderMessage );
            powerup.RegisterDestroyHandler( PrepareToDestroy );
            DebugUtils.Log( DebugUtils.Type.Battle, string.Format( "Spawned a {0} Powerup, position = {1}", type, powerup.position ) );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnPowerUp;
            rm.ownerId = powerup.id;
            rm.arguments.Add( "modelId", powerup.modelId );
            rm.arguments.Add( "powerUpType", (int)powerup.PowerUpType );
            rm.arguments.Add( "mark", (int)powerup.mark );
            rm.arguments.Add( "x", powerup.position.x );
            rm.arguments.Add( "y", powerup.position.y );
            rm.arguments.Add( "z", powerup.position.z );
            PostRenderMessage( rm );

            powerUps.Add( id, powerup );
        }

        public void SetupWildMonster( Vector3 position, Quaternion rotation )
        {
            NpcProto.Npc npcProto = DataManager.GetInstance().npcProtoData.Find( p => p.ID == GameConstants.WILDMONSTER_METAID );
            DebugUtils.Assert( npcProto != null, string.Format( "Cant't find npc proto data, metaId = {0}", GameConstants.WILDMONSTER_METAID ) );

            int id = IdGenerator.GenerateIntId( TARGET_UNIT );

            NpcType npcType = (NpcType)npcProto.NPCType;
            Npc npc = null;

            npc = (Npc)npcPool.GetUnit();
            if ( npc != null )
            {
                npc.Reset();
            }
            else
            {
                npc = LogicUnit.Create<WildMonster>( id.ToString() );
                npc.Initialize( id, npcProto, new FixVector3(position), new FixVector3( rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z ) );
                npc.RegisterRenderMessageHandler( PostRenderMessage );
                npc.RegisterDestroyHandler( PrepareToDestroy );
                npc.RegisterFindOpponentSoldier( FindSoldier );
                npc.RegisterProjectileGenerator( GenerateProjectile );

                npcPool.AddUsedUnit( npc );
            }

            npcs.Add( id, npc );
            neutralUnits.Add( id, npc );

            DebugUtils.Log( DebugUtils.Type.AI_Npc, string.Format( " Set up a {0} on map {1}", npcType, position ) );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnNPC;
            rm.position = npc.position.vector3;
            rm.direction = rotation.eulerAngles;
            rm.ownerId = npc.id;
            rm.arguments.Add( "hp", npc.maxHp );
            rm.arguments.Add( "mark", npc.mark );
            rm.arguments.Add( "modelId", npc.modelId );
            PostRenderMessage( rm );
        }

        public void SetupIdolGuard( Vector3 position, Quaternion rotation )
        {
            int metaId = GameConstants.IDOLGUARD_METAID;
            NpcProto.Npc proto = DataManager.GetInstance().npcProtoData.Find( p => p.ID == metaId );
            DebugUtils.Assert( proto != null, string.Format( "Cant't find npc proto data, metaId = {0}", metaId ) );

            int id = IdGenerator.GenerateIntId( TARGET_UNIT );

            IdolGuard idolGuard = null;
            for ( int i = 0; i < idolGuardPool.Count; i++ )
            {
                if ( idolGuardPool[i].recycled )
                {
                    idolGuard = idolGuardPool[i];
                    idolGuard.Reset();
                }
            }

            if ( idolGuard == null )
            {
                idolGuard = LogicUnit.Create<IdolGuard>( id.ToString() );
                idolGuardPool.Add( idolGuard );
            }
            
            idolGuard.Initialize( id, proto, new FixVector3(position), new FixVector3( rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z ) );
            idolGuard.RegisterRenderMessageHandler( PostRenderMessage );
            idolGuard.RegisterDestroyHandler( PrepareToDestroy );
            idolGuard.RegisterProjectileGenerator( GenerateProjectile );
            idolGuard.RegisterFindSoldier( FindSoldier );

            idolGuards.Add(id, idolGuard);
            neutralUnits.Add( id, idolGuard );

            DebugUtils.Log( DebugUtils.Type.AI_Npc, string.Format( " Set up a IdolGuard on map {0}", position ) );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnIdolGuard;
            rm.ownerId = idolGuard.id;

            rm.position = idolGuard.position.vector3;
            rm.direction = idolGuard.brithDirection.vector3;
            rm.arguments.Add( "maxHp", idolGuard.maxHp );
            rm.arguments.Add( "mark", idolGuard.mark );
            rm.arguments.Add( "modelId", idolGuard.modelId );
            PostRenderMessage( rm );
        }

        public void SetupIdol( Vector3 position, Quaternion rotation )
        {
            int metaId = GameConstants.IDOL_METAID;
            NpcProto.Npc proto = DataManager.GetInstance().npcProtoData.Find( p => p.ID == metaId );
            DebugUtils.Assert( proto != null, string.Format( "Cant't find npc proto data, metaId = {0}", metaId ) );

            int id = IdGenerator.GenerateIntId( TARGET_UNIT );

            Idol idol = null;
            for ( int i = 0; i < idolPool.Count; i++ )
            {
                if ( idolPool[i].recycled )
                {
                    idol = idolPool[i];
                    idol.Reset();
                }
            }

            if ( idol == null )
            {
                idol = LogicUnit.Create<Idol>( id.ToString() );
                idolPool.Add( idol );
            }

            idol.RegisterRenderMessageHandler( PostRenderMessage );
            idol.RegisterDestroyHandler( PrepareToDestroy );
            idol.Initialize( id, proto, new FixVector3( position ), new FixVector3( rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z ) );

            idols.Add( id, idol );
            neutralUnits.Add( id, idol );

            DebugUtils.Log( DebugUtils.Type.AI_Npc, string.Format( " Set up a Idol on map {0}", position ) );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnIdol;
            rm.ownerId = idol.id;
            rm.position = idol.position.vector3;
            rm.direction = rotation.eulerAngles;

            rm.arguments.Add( "maxHp", idol.maxHp );
            rm.arguments.Add( "mark", idol.mark );
            rm.arguments.Add( "modelId", idol.modelId );
            PostRenderMessage( rm );

            RenderMessage rm1 = new RenderMessage();
            rm1.ownerId = idol.id;
            rm1.type = RenderMessage.Type.IdolOut;
            PostRenderMessage( rm1 );
        }

		public void RequestSpawnUnit( long townId, int unitMetaId, int buttonIndex )
        {
            if ( towns.ContainsKey( townId ) )
            {
				towns[townId].RequestSpawnSoldier( unitMetaId, buttonIndex );
            }
        }

        public long SpawnUnit( long townId, int unitMetaId, Vector3 pos, bool setAsNpc, BattlerUnit unitInfo = null )
        {
            FixVector3 fixPos = (FixVector3)pos;

            UnitsProto.Unit unitProto = DataManager.GetInstance().unitsProtoData.Find( p => p.ID == unitMetaId );
            if ( unitProto == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( "Can't find metaId = {0} in unitProto, when generate unit" ) );
                return -1;
            }

            long id = -1;
            UnitType unitType = (UnitType)unitProto.UnitType;
            if ( unitType == UnitType.Demolisher )
            {
                Demolisher demolisher = towns[townId].SetupDemolisher( fixPos, setAsNpc );

                demolishers.Add( demolisher.id, demolisher );
                sideDemolishers[GetSideFromMark( towns[townId].mark )].Add( demolisher );
                id = demolisher.id;

                DebugUtils.Log( DebugUtils.Type.Battle, towns[townId].mark + " town " + townId + " spawn demolisher " + demolisher.id );
            }
            else if ( unitType == UnitType.Tramcar )
            {
                CrystalCar crystalCar = towns[townId].SetupCrystalCar( fixPos, setAsNpc );

                crystalCars.Add( crystalCar.id, crystalCar );
                sideCrystalCars[GetSideFromMark( towns[townId].mark )].Add( crystalCar );
                id = crystalCar.id;

                DebugUtils.Log( DebugUtils.Type.Battle, towns[townId].mark + " town " + townId + " spawn CrystalCar " + crystalCar.id );
            }
            else 
            {
                Soldier soldier;
                if ( setAsNpc )
                {
                    soldier = towns[townId].SpawnSoldier( unitMetaId, fixPos, setAsNpc, unitInfo );
					soldiers.Add( soldier.id, soldier );
					sideSoldiers[GetSideFromMark( towns[townId].mark )].Add( soldier );
					id = soldier.id;
					DebugUtils.Log( DebugUtils.Type.Battle, towns[townId].mark + " town " + townId + " spawn soldier " + soldier.id );
                }
				//Drag deployment logic locked.Dwayne 2017.9
                //else
                //{
                    //soldier = towns[townId].SpawnSoldier( unitMetaId, pos );
                //}

                //soldiers.Add( soldier.id, soldier );
                //sideSoldiers[GetSideFromMark( towns[townId].mark )].Add( soldier );
                //id = soldier.id;

                //DebugUtils.Log( DebugUtils.Type.Battle, towns[townId].mark + " town " + townId + " spawn soldier " + soldier.id );
            }

            return id;
        }

		public void SpawnUnit( long townId, int unitMetaId )
		{
			Soldier soldier = towns[townId].SpawnSoldier( unitMetaId );
			soldiers.Add( soldier.id, soldier );
			sideSoldiers[GetSideFromMark( towns[townId].mark )].Add( soldier );
		}

        // Be used to PVE mode
        public long SpawnUnit( long townId, int unitMetaId, Vector3 pos, BattlerUnit unitInfo )
        {
            return SpawnUnit( townId, unitMetaId, pos, true, unitInfo );
        }
			
        public void SelectedUnit( ForceMark mark, long ownerId, bool replaceGroup )
        {
            if ( forces.ContainsKey( mark ) )
            {
                Town town = forces[mark];
                town.SelectUnit( ownerId , replaceGroup );
            }
        }

        public void ChangeForceTarget( long playerId, long unitId, long targetId )
        {
            if ( towns.ContainsKey( playerId ) )
            {
                Town town = towns[playerId];

                LogicUnit target = FindOpponentLogicUnit( town.mark, targetId );

                if ( target == null )
                {
                    target = FindNeutralUnit( targetId );
                }

                if ( target != null )
                {
                    town.ChangeForceTarget( unitId, target );
                }
                else
                {
                    // Do nothing
                    // DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( "{0} town {1} can't find target by targetId = {2} ", town.side, town.id ,targetId ) );
                }
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( "Logic world can't find town by player id = {0} ", playerId ) );
            }
        }
            
        public void ChangeForceDestination( long playerId, long unitId, Vector3 destination )
        {
            if ( towns.ContainsKey( playerId ) )
            {
                FixVector3 fixVector3 = (FixVector3)destination;
                Town town = towns[playerId];
                town.ChangeForceDestination( unitId, fixVector3 );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( " Can't find town by player id = {0} ", playerId ) );
            }
        }

        public void ChangeDestination( ForceMark mark, Vector3 pos, PathType pathType )
        {
            if ( forces.ContainsKey( mark ) )
            {
                Town town = forces[mark];
                town.ChangeForceDestination( pos, pathType );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( " Try to find a unexsited town, mark = {0} ", mark ) );
            }
        }

        public void ChangeTarget( ForceMark mark, long targetId, LogicUnitType logicUnitType )
        {
            if ( IsUnitAlive( logicUnitType, targetId ) && IsUnitVisible( logicUnitType, targetId ) )
            {
                if ( forces.ContainsKey( mark ) )
                {
                    Town town = forces[mark];
                    town.ChangeForceTarget( targetId, logicUnitType );
                }
                else
                {
                    DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( " Try to find a unexsited town, mark = {0} ", mark ) );
                }
            }
            else
            {
                // target unit already death...ignore this command
            }
        }

        public void ChangeTarget( ForceMark mark, long unitId, long targetId, LogicUnitType targetType )
        {
            if ( IsUnitAlive( targetType, targetId ) && IsUnitVisible( targetType, targetId ) )
            {
                if ( forces.ContainsKey( mark ) )
                {
                    Town town = forces[mark];
                    town.ChangeForceTarget( unitId, targetId, targetType );
                }
                else
                {
                    DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( " Try to find a unexsited town, mark = {0} ", mark ) );
                }
            }
            else
            {
                // target unit already death...ignore this command
            }
        }

        // Used by Training mode
        public void ChangeSingleSoldierTarget( ForceMark mark, long id, Vector3 pos, int pathMask )
        {
            FixVector3 fixPos = (FixVector3)pos;

            if ( forces.ContainsKey( mark ) )
            {
                Town town = forces[mark];
                town.ChangeSingleSoldierTarget( id, fixPos, pathMask );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( " Try to find a unexsited town, mark = {0} ", mark ) );
            }
        }

        public void SetSimUnitSkillEnable( long townId, bool enable )
        {
            if ( towns.ContainsKey( townId ) )
            {
                Town town = towns[townId];
                town.SetSimUnitSkillEnable( enable );
            }
        }

        public void SyncMessageHandler( long unitId, long frame, Sync sync, List<Position> positions )
        {
            if ( soldiers.ContainsKey( unitId ) )
            {
                soldiers[unitId].SyncMessageHandler( frame, sync, positions );
            }
            else if ( crystalCars.ContainsKey( unitId ) )
            {
                crystalCars[unitId].SyncMessageHandler( frame, sync, positions );
            }
            else if ( demolishers.ContainsKey( unitId ) )
            {
                demolishers[unitId].SyncMessageHandler( frame, sync, positions );
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, "there isn't such soldier " + unitId );
            }
        }

        public void DrawUnitsPathPoint( ForceMark mark, long unitId, Vector3 pos, PathType pathType )
        {
            if ( forces.ContainsKey( mark ) )
            {
                Town town = forces[mark];
                town.DrawUnitsPathPoint( unitId, pos, pathType );
            }
        }

        public void ChangeTargetByResetPath(ForceMark mark, long unitId, List<Vector3> path, PathType pathType, Boolean isLast )
        {
            if ( forces.ContainsKey( mark ) )
            {
                forces[mark].ChangeForceTargetByPath( GetFixVector3FromVector3( path ), unitId, pathType, isLast );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( " Try to find a unexsited town, mark = {0} ", mark ) );
            }
        }

        public void MapAreaCollision( long playerID, long unitId, int collisionType, int collisionState )
        {
            if ( towns.ContainsKey( playerID ) )
            {
                towns[playerID].MapAreaCollision( unitId, collisionType, collisionState );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( " Try to find a unexsited town, playerID = {0} ", playerID ) );
            }
        }

        public void SyncSkillTargetPosition( long skillId, Vector3 position, long ownerId, int metaId )
        {
            if ( skills.ContainsKey( skillId ) )
            {
                skills[skillId].SyncMessageHandler( (FixVector3)position );
            }
            else
            {
                DebugUtils.LogWarning( DebugUtils.Type.AI_Skill, string.Format( " Try to find a unexsited skill, SkillID = {0} ,ownerid = {1}, metaId = {2} ", skillId, ownerId, metaId ) );
            }
        }

        public void ClearFreePath( ForceMark mark/*long townId,*/ )
        {
            // for 3v3
            //if ( towns.ContainsKey( townId ) )
            //{
            //    towns[townId].ClearFreePath( soldierId );

            //}

            if ( forces.ContainsKey( mark ) )
            {
                forces[mark].ClearFreePath();
            }
        }

        public void SelectAllUnits( ForceMark mark, bool invert )
        {
            if ( forces.ContainsKey( mark ) )
            {
                forces[mark].SelectAllActiveUnits( invert );
            }
        }

        // Save the power up code 
        //private float generateTimer = 0f;
        //private float generateGapTime = 30f;

        //private void PowerUpTimer( float deltaTime )
        //{
        //    if ( powerUps.Count == 0 )
        //    {
        //        if ( generateTimer < generateGapTime )
        //        {
        //            generateTimer += deltaTime;
        //        }
        //        else
        //        {
        //            SpawnPowerup( GetPowerUpType(), powerUpsBornPos );

        //            generateTimer = 0f;
        //        }
        //    }
        //}

        public MatchSide GetSideFromMark( ForceMark mark )
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
                DebugUtils.LogError( DebugUtils.Type.AI_LogicWorld, "there is no side for " + mark );
                return MatchSide.NoSide;
            }
        }

        public void SetCameraFollowUnit( ForceMark mark )
        {
            if ( forces.ContainsKey( mark ) )
            {
                forces[mark].SetCameraFollowUnit();
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( "Can't find force {0}", mark ) );
            }
        }

        public bool IsUnitVisible( LogicUnitType type, long id )
        {
            switch (type)
            {
                case LogicUnitType.Soldier:
                {
                    if ( soldiers.ContainsKey( id ) )
                    {
                        return !soldiers[id].GetFlag( SoldierAttributeFlags.Cloaking );
                    }
                    break;
                }
                default:
                {
                    return true;
                }
            }

            return true;
        }

        public bool IsUnitAlive( LogicUnitType type, long id )
        {
            switch (type)
            {
                case LogicUnitType.Soldier:
                {
                    if ( soldiers.ContainsKey( id ) )
                    {
                        return soldiers[id].Alive();
                    }
                    break;
                }
                case LogicUnitType.Town:
                {
                    if ( towns.ContainsKey( id ) )
                    {
                        return towns[id].Alive();
                    }
                    break;
                }
                case LogicUnitType.Tower:
                {
                    if ( towers.ContainsKey( id ) )
                    {
                        return towers[id].Alive();
                    }
                    break;
                }
                case LogicUnitType.CrystalCar:
                {
                    if ( crystalCars.ContainsKey( id ) )
                    {
                        return crystalCars[id].Alive();
                    }
                    break;
                }
                case LogicUnitType.institute:
                {
                    if ( institutes.ContainsKey( id ) )
                    {
                        return institutes[id].Alive();
                    }
                    break;
                }
                case LogicUnitType.Demolisher:
                {
                    if ( demolishers.ContainsKey( id ) )
                    {
                        return demolishers[id].Alive();
                    }
                    break;
                }
                case LogicUnitType.NPC:
                {
                    if ( npcs.ContainsKey( id ) )
                    {
                        return npcs[id].Alive();
                    }
                    break;
                }
                case LogicUnitType.Idol:
                {
                    if ( idols.ContainsKey( id ) )
                    {
                        return idols[id].Alive();
                    }
                    break;
                }
                case LogicUnitType.IdolGuard:
                {
                    if ( idolGuards.ContainsKey( id ) )
                    {
                        return idolGuards[id].Alive();
                    }
                    break;
                }
                default:
                {
                    DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( "Can't check {0} alive state", type ) );
                    break;
                }
            }

            return false;
        }

        public List<FixVector3> GetFixVector3FromVector3( List<Vector3> path )
        {
            List<FixVector3> fixPath = new List<FixVector3>();

            for ( int i = 0; i < path.Count; i++ )
            {
                fixPath.Add( (FixVector3)path[i] );
            }

            return fixPath;
        }

        public PowerUpType GetPowerUpType()
        {
             return (PowerUpType)( GetRandomNumber() % 4 + 1 );
        }

        public void SetupPool()
        {
            DebugUtils.Log( DebugUtils.Type.AI_LogicWorld, "Logic world prepares the pools!" );
        }

        bool isEnd = false;
        public void Update( int deltaTime )
        {
            timer += deltaTime;

            // TODO: Temp time control, will be controled by server in future
            if ( timer > 0 )
            {
                RenderMessage rm = new RenderMessage();
                rm.type = RenderMessage.Type.TimeChanged;
                rm.arguments.Add( "value", timer * 0.001f );
                PostRenderMessage( rm );
            }
			//TODO： Maybe after need automatic send battle end
            /*else
            {
                if ( !isEnd )
                {
                    BattleResultInfo info = new BattleResultInfo();
                    info.chestType = UI.ChestItemType.Item;
                    info.playerExp = 1000;
                    info.playerGold = 100;
                    info.soldiersRewardExp = 100;

                    //MessageDispatcher.PostMessage( MessageType.BattleResult, BattleResultType.Draw, info );
                    isEnd = true;
                }
            }*/

            foreach ( var town in towns )
            {
                town.Value.LogicUpdate( deltaTime );
            }

            for ( int i = 0; i < soldiers.Count; i++ )
            {
                var soldier = soldiers.ElementAt( i );
                soldier.Value.LogicUpdate( deltaTime );
            }

            foreach ( var tower in towers )
            {
                tower.Value.LogicUpdate( deltaTime );
            }

			foreach ( var institute in institutes )
			{
				institute.Value.LogicUpdate( deltaTime );
			}

            foreach ( var crystal in crystals )
            {
                crystal.Value.LogicUpdate( deltaTime );
            }

            for ( int i = 0; i < demolishers.Count; i++ )
            {
                var demolisher = demolishers.ElementAt( i );
                demolisher.Value.LogicUpdate( deltaTime );
            }

            for ( int i = 0; i < crystalCars.Count; i++ )
            {
                var crystalCar = crystalCars.ElementAt( i );
                crystalCar.Value.LogicUpdate( deltaTime );
            }

            foreach ( var powerUp in powerUps )
            {
                powerUp.Value.LogicUpdate( deltaTime );
            }

            for ( int i = 0; i < skills.Count; i++ )
            {
                var skill = skills.ElementAt( i );
                skill.Value.LogicUpdate( deltaTime );
            }
 
            foreach ( var npc in npcs )
            {
                npc.Value.LogicUpdate( deltaTime );
            }

            foreach ( var idolGuard in idolGuards )
            {
                idolGuard.Value.LogicUpdate( deltaTime );
            }

            //projectile should be updated after tower.
            foreach ( var projectile in projectiles )
            {
                projectile.Value.LogicUpdate( deltaTime );
            }
            //PowerUpTimer( deltaTime );

            foreach ( var trap in traps )
            {
                trap.Value.LogicUpdate( deltaTime );
            }


            foreach ( var summon in summons )
            {
                summon.Value.LogicUpdate( deltaTime );
            }

            Destroy();
        }

        #region the delegate operation methods

        public Soldier FindSoldier( FixVector3 center, long radius )
        {
            long distance = radius;
            long minDistance = distance;
            Soldier r = null;

            foreach ( KeyValuePair<long, Soldier> s in soldiers )
            {
                if ( s.Value.Alive() )
                {
                    long d = FixVector3.SqrDistance( s.Value.position, center );
                    if ( d <= minDistance )
                    {
                        r = s.Value;
                        minDistance = d;
                    }
                }
            }

            return r;
        }

        public List<Soldier> FindOwnForceSoldiers( ForceMark mark, FixVector3 center, long radius )
        {
            List<Soldier> s = forces[mark].GetAliveSoldiers();
            List<Soldier> r = new List<Soldier>();

            foreach ( Soldier soldier in s )
            {
                if ( soldier.Alive() )
                {
                    if ( FixVector3.SqrDistance( soldier.position, center ) <= radius )
                    {
                        r.Add( soldier );
                    }
                }
            }

            return r;
        }

        public List<Soldier> FindFriendlySoldiers( ForceMark mark, FixVector3 center, long radius )
        {
            MatchSide side = GetSideFromMark( mark );

            List<Soldier> s = sideSoldiers[side];
            List<Soldier> r = new List<Soldier>();

            foreach ( Soldier soldier in s )
            {
                if ( soldier.Alive() )
                {
                    if ( FixVector3.SqrDistance( soldier.position, center ) <= radius )
                    {
                        r.Add( soldier );
                    }
                }
            }

            return r;
        }

        public List<Soldier> FindOpponentSoldiers( ForceMark mark, FixVector3 center, Predicate<Soldier> match )
        {
            MatchSide side = GetSideFromMark( mark );
            //float distance = radius * radius;

            MatchSide opponentSide = GetOpponentSide( side );

            List<Soldier> s = sideSoldiers[opponentSide];
            List<Soldier> r = new List<Soldier>();

            foreach ( Soldier soldier in s )
            {
                if ( soldier.Alive() && !soldier.GetFlag( SoldierAttributeFlags.Cloaking ) && match( soldier ) )
                {
                    r.Add( soldier );
                }
            }

            return r;
        }

        public LogicUnit FindOpponentBuilding( ForceMark mark, FixVector3 center, long radius )
        {
            MatchSide side = GetSideFromMark( mark );

            long minDistance = radius;
            MatchSide opponentSide = GetOpponentSide( side );
            List<LogicUnit> units = sideBuildings[opponentSide];
            LogicUnit r = null;

            foreach ( LogicUnit unit in units )
            {
                if ( unit.Alive() )
                {
                    long d = FixVector3.SqrDistance( unit.position, center );

                    //DebugUtils.Log( DebugUtils.Type.AI_logicworld, "mark " + mark + " and building id " + unit.id + "'s distance = " + d );
                    if ( d <= minDistance )
					{
						if( DataManager.GetInstance().GetBattleType() == BattleType.Survival && opponentSide == MatchSide.Red )
						{
							//Because the survivalMode player can not attack AI building.
						}
						else
						{
							r = unit;
							minDistance = d;
						}
					}
                }
            }

            return r;
        }

        public Soldier FindOpponentSoldier( ForceMark mark, FixVector3 center, long radius )
        {
            MatchSide side = GetSideFromMark( mark );

            long minDistance = radius;
            MatchSide opponentSide = GetOpponentSide( side );
            List<Soldier> s = sideSoldiers[opponentSide];
            Soldier r = null;

            foreach ( Soldier soldier in s )
            {
                if ( soldier.Alive() && !soldier.GetFlag( SoldierAttributeFlags.Cloaking ) )
                {
                    long distance = FixVector3.SqrDistance( soldier.position, center );
                    if ( distance <= minDistance )
                    {
                        r = soldier;
                        minDistance = distance;
                    }
                }
            }

            return r;
        }

        public List<LogicUnit> FindOpponentBuildings( ForceMark mark, FixVector3 center, long radius )
        {
            MatchSide side = GetSideFromMark( mark );

            long minDistance = radius;
            MatchSide opponentSide = GetOpponentSide( side );
            List<LogicUnit> units = sideBuildings[opponentSide];
            List<LogicUnit> targets = new List<LogicUnit>();

            foreach ( LogicUnit unit in units )
            {
                if ( unit.Alive() )
                {
                    long d = FixVector3.SqrDistance( unit.position, center );

                    //DebugUtils.Log( DebugUtils.Type.AI_logicworld, "mark " + mark + " and building id " + unit.id + "'s distance = " + d );
                    if ( d <= minDistance )
                    {
                        if ( DataManager.GetInstance().GetBattleType() == BattleType.Survival && opponentSide == MatchSide.Red )
                        {
                            //Because the survivalMode player can not attack AI building.
                        }
                        else
                        {
                            targets.Add( unit );
                        }
                    }
                }
            }

            return targets;
        }

        public Crystal FindOpponentCrystal( FixVector3 center, long radius )
        {
            long minDistance = radius;
            Crystal r = null;

            foreach ( var crystal in crystals )
            {
                if ( crystal.Value.Alive() && crystal.Value.OwnerLess() )
                {
                    long distance = FixVector3.SqrDistance( crystal.Value.position, center );
                    if ( distance <= minDistance )
                    {
                        r = crystal.Value;
                        minDistance = distance;
                    }
                }
            }

            return r;
        }

        public CrystalCar FindOpponentCrystalCar( ForceMark mark, FixVector3 center, long radius )
        {
            long minDistance = radius;
            MatchSide opponentSide = GetOpponentSide( GetSideFromMark( mark ) );
            List<CrystalCar> s = sideCrystalCars[opponentSide];
            CrystalCar r = null;

            foreach ( CrystalCar crystalCar in s )
            {
                if ( crystalCar.Alive() )
                {
                    long distance = FixVector3.SqrDistance( crystalCar.position, center );
                    if ( distance <= minDistance )
                    {
                        r = crystalCar;
                        minDistance = distance;
                    }
                }
            }

            return r;
        }

        public List<CrystalCar> FindOpponentCrystalCars( ForceMark mark, FixVector3 center, Predicate<CrystalCar> match )
        {
            MatchSide opponentSide = GetOpponentSide( GetSideFromMark( mark ) );
            List<CrystalCar> s = sideCrystalCars[opponentSide];
            List<CrystalCar> r = new List<CrystalCar>();

            foreach ( CrystalCar crystalCar in s )
            {
                if ( crystalCar.Alive() && match( crystalCar ) )
                {
                    r.Add( crystalCar );
                }
            }

            return r;
        }

        public Demolisher FindOpponentDemolisher( ForceMark mark, FixVector3 center, long radius )
        {
            long minDistance = radius;
            MatchSide opponentSide = GetOpponentSide( GetSideFromMark( mark ) );
            List<Demolisher> s = sideDemolishers[opponentSide];
            Demolisher r = null;

            foreach ( Demolisher d in s )
            {
                if ( d.Alive() )
                {
                    long distance = FixVector3.SqrDistance( d.position, center );
                    if ( distance <= minDistance )
                    {
                        r = d;
                        minDistance = distance;
                    }
                }
            }

            return r;
        }

        public PowerUp FindPowerUp( FixVector3 center, long radius )
        {
            long minDistance = radius;
            PowerUp p = null;

            foreach ( KeyValuePair<long, PowerUp> k in powerUps )
            {
                if( k.Value )
                {
                    long distance = FixVector3.SqrDistance( k.Value.position, center );
                    if ( distance <= minDistance )
                    {
                        p = k.Value;
                        minDistance = distance;
                    }
                }
            }

            return p;
        }

        public List<Demolisher> FindOpponentDemolishers( ForceMark mark, FixVector3 center, Predicate<Demolisher> match )
        {
            MatchSide opponentSide = GetOpponentSide( GetSideFromMark( mark ) );
            List<Demolisher> s = sideDemolishers[opponentSide];
            List<Demolisher> r = new List<Demolisher>();

            foreach( Demolisher d in s )
            {
                if ( d.Alive() && match( d ) )
                {
                    r.Add( d );
                }
            }

            return r;
        }

        public LogicUnit FindOpponentLogicUnit( ForceMark mark, long unitId )
        {
            MatchSide opponentSide = GetOpponentSide( GetSideFromMark( mark ) );

            LogicUnit unit = sideSoldiers[opponentSide].Find( p => p.id == unitId );

            if ( unit == null )
            {
                unit = sideBuildings[opponentSide].Find( p => p.id == unitId );
            }

            if ( unit == null )
            {
                unit = sideCrystalCars[opponentSide].Find( p => p.id == unitId );
            }

            if ( unit == null )
            {
                unit = sideDemolishers[opponentSide].Find( p => p.id == unitId );
            }

            // Units that can be found must still alive
            if ( unit != null && !unit.Alive() )
            {
                unit = null;
            }

            return unit;
        }

        public LogicUnit FindNeutralUnit( FixVector3 center, long radius )
        {
            long minDistance = radius;
            LogicUnit n = null;

            foreach ( KeyValuePair<long, Npc> d in npcs )
            {
                if ( d.Value.Alive() )
                {
                    long distance = FixVector3.SqrDistance( d.Value.position, center );
                    if ( distance <= minDistance )
                    {
                        n = d.Value;
                        minDistance = distance;
                    }
                }
            }

            foreach ( KeyValuePair<long, IdolGuard> d in idolGuards )
            {
                if ( d.Value.Alive() )
                {
                    long distance = FixVector3.SqrDistance( d.Value.position, center );
                    if ( distance <= minDistance )
                    {
                        n = d.Value;
                        minDistance = distance;
                    }
                }
            }

            foreach ( KeyValuePair<long, Idol> d in idols )
            {
                if ( d.Value.Alive() )
                {
                    long distance = FixVector3.SqrDistance( d.Value.position, center );
                    if ( distance <= minDistance )
                    {
                        n = d.Value;
                        minDistance = distance;
                    }
                }
            }

            return n;
        }

        public List<LogicUnit> FindNeutralUnits( FixVector3 center, Predicate<LogicUnit> match )
        {
            List<LogicUnit> npc = new List<LogicUnit>();

            foreach ( KeyValuePair<long, Npc> d in npcs )
            {
                if ( d.Value.Alive() && match( d.Value ) )
                {
                    npc.Add( d.Value );
                }
            }

            foreach ( KeyValuePair<long, IdolGuard> d in idolGuards )
            {
                if ( d.Value.Alive() && match( d.Value ) )
                {
                    npc.Add( d.Value );
                }
            }

            foreach ( KeyValuePair<long, Idol> d in idols )
            {
                if ( d.Value.Alive() && match( d.Value ) )
                {
                    npc.Add( d.Value );
                }
            }

            return npc;
        }

        public LogicUnit FindNeutralUnit( long unitId )
        {
            LogicUnit unit = null;

            if ( neutralUnits.ContainsKey( unitId ) )
            {
                unit = neutralUnits[unitId];
            }

            // Units that can be found must still alive
            if ( unit != null && !unit.Alive() )
            {
                unit = null;
            }

            return unit;
        }

        public MatchSide GetOpponentSide( MatchSide side )
        {
            DebugUtils.Assert( side != MatchSide.NoSide, "there is no side for " + side );

            MatchSide opponentSide = MatchSide.NoSide;

            if( side == MatchSide.Red )
            {
                opponentSide = MatchSide.Blue;
            }
            else if( side == MatchSide.Blue )
            {
                opponentSide = MatchSide.Red;
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AI_LogicWorld, "there is no side for " + side );
                opponentSide = MatchSide.NoSide;
            }

            return opponentSide;
        }

        public FixVector3 CalculateAvoidance( Soldier looker, FixVector3 ahead )
        {
            ForceMark mark = looker.mark;
            MatchSide side = GetSideFromMark( mark );
            List<Soldier> s = sideSoldiers[side];
            FixVector3 avoidance = FixVector3.zero;

            foreach( Soldier soldier in s )
            {
                if( soldier.Alive() && soldier.id != looker.id )
                {
                    long r = looker.modelRadius + soldier.modelRadius;
                    FixVector3 p = ahead - soldier.position;
                    long d = p.magnitude;
                    if ( d < r )
                    {
                        //avoidance += p.normalized * 4;
                        avoidance = p.normalized * r;
                        DebugUtils.Log( DebugUtils.Type.Avoidance, string.Format( "soldier {0} has an avoidance ({1}, {2}, {3}) from soldier {4}!", looker.id, avoidance.x, avoidance.y, avoidance.z, soldier.id ) );
                        return avoidance;
                    }
                }
            }

            return avoidance;
        }
        #endregion

        #region the pool methods

		private Soldier GenerateSoldier( Town town, int metaId, FixVector3 pos, BattlerUnit unitInfo, bool isNpc, Action<Soldier> onCompleteUnitBehavior )
		{
			int id = IdGenerator.GenerateIntId( TARGET_UNIT );
			Soldier soldier = null;

            UnitsProto.Unit unitProto = DataManager.GetInstance().unitsProtoData.Find( p => p.ID == metaId );
            if ( unitProto == null )
            {
                DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( "Can't find metaId = {0} in unitProto, when generate Soldier" ) );
                return null;
            }

            soldier = (Soldier)soldierPool.GetUnit();
            if ( soldier != null )
            {
                soldier.Reset();
            }

            if( soldier == null )
            {
                soldier = LogicUnit.Create<Soldier>( "0" );
                soldier.InitializePathAgent();
                soldier.gameObject.SetActive( false );
                soldierPool.AddUsedUnit( soldier );
            }

			soldier.id = id;
#if DEBUG
            const string prefix = "soldier";
            soldier.name = prefix + id.ToString();
            soldier.gameObject.name = soldier.name;
            soldier.ChangeRoot();
#endif

            onCompleteUnitBehavior( soldier );
            soldier.Initialize( town, unitProto, pos, unitInfo, isNpc );
 
            return soldier;
        }

        public CrystalCar GenerateCrystalCar( Town town, FixVector3 pos, bool simulate )
        {
            int id = IdGenerator.GenerateIntId( TARGET_UNIT );

            CrystalCar crystalCar = null;

            crystalCar = (CrystalCar)crystalCarPool.GetUnit();
            if ( crystalCar != null )
            {
                crystalCar.Reset();
            }

            if ( crystalCar == null )
            {
                crystalCar = LogicUnit.Create<CrystalCar>( "0" );
                crystalCar.pathAgent = new NavAgent( crystalCar );
                crystalCarPool.AddUsedUnit( crystalCar );
            }

            crystalCar.gameObject.SetActive( true );
            crystalCar.gameObject.name = string.Format( "CrystalCar_{0}", crystalCar.id );
            crystalCar.Initialize( town, id, pos, simulate );

            return crystalCar;
        }

        public Demolisher GenerateDemolisher( Town town, FixVector3 pos, bool simulate )
        {
            int id = IdGenerator.GenerateIntId( TARGET_UNIT );

            Demolisher d = null;

            d = (Demolisher)demolisherPool.GetUnit();
            if ( d != null )
            {
                d.Reset();
            }

            if ( d == null )
            {
                d = LogicUnit.Create<Demolisher>( "0" );
                d.pathAgent = new NavAgent( d );
                demolisherPool.AddUsedUnit( d );
            }

            d.gameObject.SetActive( true );
            d.gameObject.name = string.Format( "Demolisher_{0}", d.id );
            d.Initialize( town, id, pos, simulate );

            return d;
        }

        //TODO:refactor projectile pool
        public Projectile GenerateProjectile( LogicUnit owner, int metaId, FixVector3 position, LogicUnit target )
        {
            int id = IdGenerator.GenerateIntId( PROJECTILE_ID );

            ProjectileProto.Projectile proto = DataManager.GetInstance().projectileProtoData.Find( p=>p.ID == metaId );
            DebugUtils.Assert( proto != null, string.Format( "Can't find metaId = {0} in projectile table", metaId ) );

            Projectile projectile = null;
            ProjectileType type = (ProjectileType)proto.ProjectileType;

            projectile = (Projectile)projectilePoolGroup.GetUnit( proto.ProjectileType );
            if ( projectile != null )
            {
                projectile.Reset();
            }

            if ( projectile == null )
            {
                if ( type == ProjectileType.NormalProjectile )
                {
                    projectile = LogicUnit.Create<NormalProjectile>( "0" );
                }
                else if ( type == ProjectileType.MagicExplodeSkill )
                {
                    projectile = LogicUnit.Create<MoFaBaoLieDanArrow>( "0" );
                }
                else if ( type == ProjectileType.TrapProjectile )
                {
                    projectile = LogicUnit.Create<TrapArrow>( "0" );
                }
                else
                {
                    DebugUtils.LogError( DebugUtils.Type.AI_Projectile, "Can't handle this projectileType = " + type );
                }

                projectilePoolGroup.AddUsedUnit( proto.ProjectileType , projectile );
            }

            projectile.gameObject.name = id.ToString();
            projectiles.Add( id, projectile );
            projectile.Initialize( owner, proto, id, position, target );

            return projectile;
        }

        public Skill GenerateSoldierSkill( Soldier owner, SkillProto skillProto, int skillIndex )
        {
            int id = IdGenerator.GenerateIntId( SKILL_ID );
            Skill skill = null;

            skill = (Skill)skillPoolGroup.GetUnit( skillProto.ID );
            if ( skill != null )
            {
                skill.Reset();
            }
            else
            {
                if ( skillProto.ID == 1 )
                {
                    skill = LogicUnit.Create<YueGuangChongCiSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 2 )
                {
                    skill = LogicUnit.Create<YueXingDaJiSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 3 )
                {
                    skill = LogicUnit.Create<ZhiMingYiJiSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 4 )
                {
                    skill = LogicUnit.Create<BaiRenZhanSkill>( id.ToString() );
                }
                else if( skillProto.ID == 5 )
                {
                    skill = LogicUnit.Create<MolaPaoTaiSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 6 )
                {
                    skill = LogicUnit.Create<JiShuGengXinSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 7 )
                {
                    skill = LogicUnit.Create<MoFaBaoLieDanSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 8 )
                {
                    skill = LogicUnit.Create<MoFaXianJingSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 9 )
                {
                    skill = LogicUnit.Create<ChaoFengSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 10 )
                {
                    skill = LogicUnit.Create<ShengSiYiZhanSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 11 )
                {
                    skill = LogicUnit.Create<XieEChouHenSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 12 )
                {
                    skill = LogicUnit.Create<JingJiXianJingSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 13 )
                {
                    skill = LogicUnit.Create<MoFaYuanSuSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 14 )
                {
                    skill = LogicUnit.Create<MoFaShiKongSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 15 )
                {
                    skill = LogicUnit.Create<CiKeZongJiSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 16 )
                {
                    skill = LogicUnit.Create<SiWangXianJingSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 17 )
                {
                    skill = LogicUnit.Create<ZhanDouYinYueSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 18 )
                {
                    skill = LogicUnit.Create<ShenZhiZhuFuSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 19 )
                {
                    skill = LogicUnit.Create<BinSiJiuShuSkill>( id.ToString() );
                }
                else if ( skillProto.ID == 20 )
                {
                    skill = LogicUnit.Create<BinSiJiuShuSkill>( id.ToString() );
                }
                else
                {
                    DebugUtils.LogError( DebugUtils.Type.AI_Skill, string.Format( "Can't handle this skill id now, id = {0}", skillProto.ID ) );
                    return null;
                }

                skillPoolGroup.AddUsedUnit( skillProto.ID, skill );
            }

            skill.Initialize( id, owner, skillProto, skillIndex );
            skills.Add( skill.id, skill );
#if DEBUG
            const string prefix = "Skill_";
            skill.name = prefix + id.ToString();
            skill.gameObject.name = skill.name;
            skill.ChangeRoot();
#endif
            return skill;
        }
			
		private Tower GenerateTower( Town town, int towerTypeID, FixVector3 pos )
		{
			int id = IdGenerator.GenerateIntId( TARGET_UNIT );

			Tower tower = null;

            tower = (Tower)towerPool.GetUnit();

			//TODO: Need add check mode id code to check.( Now we can build enemy tower if have enemy tower destroyed, because pool not check mode id )

            if ( tower != null )
            {
                tower.Reset();
            }

            if ( tower == null )
			{
				tower = LogicUnit.Create<Tower>( id.ToString() );
				tower.gameObject.SetActive( false );
                towerPool.AddUsedUnit( tower );
			}

			tower.id = id;
			#if DEBUG
			const string prefix = "Tower";
			tower.name = prefix + id.ToString();
			tower.gameObject.name = tower.name;
			tower.ChangeRoot();
			#endif

			tower.Initialize( id, town, pos, towerTypeID );
			return tower;
		}

        public SummonedUnit GenerateSummon( Skill owner, int metaId, FixVector3 position, FixVector3 rotation )
        {
            SummonProto.Summon proto = DataManager.GetInstance().summonProtoData.Find( p => p.ID == metaId );
            DebugUtils.Assert( proto != null, string.Format( "Can't find metaId = {0} in summon table", metaId ) );

            long id = IdGenerator.GenerateIntId( SUMMON );


            SummonedUnit summon = (SummonedUnit)summonedUnitPool.GetUnit();

            if ( summon == null )
            {
                summon = LogicUnit.Create<SummonedUnit>( id.ToString() );
                summonedUnitPool.AddUsedUnit( summon );
            }
            else
            {
                summon.Reset();
            }

            summon.Initialize( owner, id, proto, position, rotation );
            summons.Add( id, summon );

            return summon;
        }

        private Institute GenerateInstitute( Town town, FixVector3 pos )
		{
			int id = IdGenerator.GenerateIntId( TARGET_UNIT );

			Institute institute = null;

            institute = (Institute)institutePool.GetUnit();
            if ( institute != null )
            {
                institute.Reset();
            }

            if ( institute == null )
			{
				institute = LogicUnit.Create<Institute>( id.ToString() );
                institutePool.AddUsedUnit( institute );

            }

			institute.id = id;
			#if DEBUG
			const string prefix = "Institute";
			institute.name = prefix + id.ToString();
			institute.gameObject.name = institute.name;
			institute.ChangeRoot();
			#endif

			institute.Initialize( id, town, pos );
			return institute;
		}

        public AttributeEffect GenerateAttributeEffect( int metaId )
        {
            AttributeEffectProto.AttributeEffect proto = DataManager.GetInstance().attributeEffectProtoData.Find( p => p.ID == metaId );
            if ( proto == null )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect, string.Format( "Unexist AttributeEffect id {0}", metaId ) );
            }

            int id = IdGenerator.GenerateIntId( ATTRIBUTE_EFFECT );
            AttributeEffect attributEffect = null;
            AttributeAffectType affectType = (AttributeAffectType)proto.AffectedType;

            if ( affectType == AttributeAffectType.Buff )
            {
                BuffType type = (BuffType)proto.AttributeType;
                if ( type == BuffType.PhysicalAttack )
                {
                    attributEffect = new PhysicalAttackBuff();
                }
                else if ( type == BuffType.MagicAttack )
                {
                    attributEffect = new MagicAttackBuff();
                }
                else if ( type == BuffType.Armor )
                {
                    attributEffect = new ArmorBuff();
                }
                else if ( type == BuffType.MagicResist )
                {
                    attributEffect = new MagicResistBuff();
                }
                else if ( type == BuffType.CriticalChance )
                {
                    attributEffect = new CriticalChanceBuff();
                }
                else if ( type == BuffType.CriticalDamage )
                {
                    attributEffect = new CriticalDamageBuff();
                }
                else if ( type == BuffType.Speed )
                {
                    attributEffect = new MoveSpeedBuff();
                }
                else if ( type == BuffType.AttackSpeed )
                {
                    attributEffect = new AttackSpeedBuff();
                }
                else if ( type == BuffType.MaxHealth )
                {
                    attributEffect = new MaxHealthBuff();
                }
                else if ( type == BuffType.HealthRecover )
                {
                    attributEffect = new HealthRecoverBuff();
                }
                else if ( type == BuffType.Heal )
                {
                    attributEffect = new HealBuff();
                }
                else if ( type == BuffType.Cloaking )
                {
                    attributEffect = new CloakingBuff();
                }
                else
                {
                    DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect, "Can't handle this buff type now! type = " + type );
                }
            }
            else if ( affectType == AttributeAffectType.Debuff )
            {
                DebuffType type = (DebuffType)proto.AttributeType;
                if ( type == DebuffType.PhysicalDamage )
                {
                    attributEffect = new PhysicalDamageDebuff();
                }
                else if ( type == DebuffType.MagicDamage )
                {
                    attributEffect = new MagicDamageDebuff();
                }
                else if ( type == DebuffType.PhysicalAttack )
                {
                    attributEffect = new PhysicalAttackDebuff();
                }
                else if ( type == DebuffType.MagicAttack )
                {
                    attributEffect = new MagicAttackDebuff();
                }
                else if ( type == DebuffType.Armor )
                {
                    attributEffect = new ArmorDebuff();
                }
                else if ( type == DebuffType.MagicResist )
                {
                    attributEffect = new MagicResistDebuff();
                }
                else if ( type == DebuffType.CriticalChance )
                {
                    attributEffect = new CriticalChanceDebuff();
                }
                else if ( type == DebuffType.CriticalDamage )
                {
                    attributEffect = new CriticalDamageDebuff();
                }
                else if ( type == DebuffType.Speed )
                {
                    attributEffect = new MoveSpeedDebuff();
                }
                else if ( type == DebuffType.AttackSpeed )
                {
                    attributEffect = new AttackSpeedDebuff();
                }
                else if ( type == DebuffType.MaxHealth )
                {
                    attributEffect = new MaxHealthDebuff();
                }
                else if ( type == DebuffType.HealthRecover )
                {
                    attributEffect = new HealthRecoverDebuff();
                }
                else if ( type == DebuffType.ForbiddenMoves )
                {
                    attributEffect = new ForbiddenMovesDebuff();
                }
                else if ( type == DebuffType.Slience )
                {
                    attributEffect = new SlienceDebuff();
                }
                else if ( type == DebuffType.ForbiddenAttacks )
                {
                    attributEffect = new ForbiddenAttacksDebuff();
                }
                else if ( type == DebuffType.Stun )
                {
                    attributEffect = new StunDebuff();
                }
                else if ( type == DebuffType.Sneer )
                {
                    attributEffect = new SneerDebuff();
                }
                else
                {
                    DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect, "Can't handle this debuff type now! type = " + type );
                }
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect, string.Format( "AttributeEffect {0}'s affectedType is incorrected", proto.ID ) );
            }

            attributEffect.Init( id, proto.AttributeType, metaId, proto.Duration, proto.MainValue, proto.CalculateType );
            attributEffect.RegisterRenderMessageHandler( PostRenderMessage );

            return attributEffect;
        }

        public Trap GenerateTrap( LogicUnit owner , int metaId , FixVector3 position , FixVector3 rotation )
        {
            //--TODO
            int id = IdGenerator.GenerateIntId( TRAP_ID );

            TrapProto.Trap proto = DataManager.GetInstance().trapProtoData.Find( p => p.ID == metaId );
            DebugUtils.Assert( proto != null , string.Format( "Can't find metaId = {0} in Trap table" , metaId ) );

            Trap trap = null;
            TrapType trapType = (TrapType)proto.TrapType;

            trap = (Trap)trapPoolGroup.GetUnit( proto.TrapType );

            if ( trap != null )
            {
                trap.Reset();
            }
            else
            {

                trap = LogicUnit.Create<Trap>( "0" );
                trapPoolGroup.AddUsedUnit( proto.TrapType, trap );
            }

            trap.gameObject.name = id.ToString();
            traps.Add( id , trap );
            trap.Initialize( owner ,proto, id , position , rotation );

            return trap;
        }

        #endregion

        public void RegisterRenderMessageHandler( RenderMessageHandler handler )
        {
            PostRenderMessage = handler;
        }

        public void RegisterAgentMessageHandler( BattleMessageHandler handler )
        {
            PostAgentMessage = handler;
        }

        public int GetRandomNumber( int range = 100 )
        {
            return random.Next( range );
        }

        public void PrepareToDestroy( LogicUnit unit )
        {
            if ( !garbges.Contains( unit ) )
            {
                garbges.Add( unit );
            }
            else
            {
                DebugUtils.Assert( false, "one unit can't be destroy twice " + unit.ToString() );
            }
        }

        public void Destroy()
        {
            foreach( LogicUnit unit in garbges )
            {
                MatchSide side = GetSideFromMark( unit.mark );
                //if( unit.type != LogicUnitType.Npc )
                //{
                //    side = GetSideFromMark( unit.mark );
                //}

                if ( unit.type == LogicUnitType.Town )
                {
                    forces.Remove( unit.mark );
                    towns.Remove( unit.id );
                    sideTowns[side].Remove( (Town)unit );
                }
                else if( unit.type == LogicUnitType.Tower )
                {
                    towers.Remove( unit.id );
                    sideTowers[side].Remove( (Tower)unit );
                }
				else if( unit.type == LogicUnitType.institute )
				{
					institutes.Remove( unit.id );
					sideInstitutes[side].Remove( ( Institute )unit );
					forces[ unit.mark ].RemoveInstitute();
				}
                else if( unit.type == LogicUnitType.Soldier )
                {
                    soldiers.Remove( unit.id );
                    sideSoldiers[side].Remove( (Soldier)unit );
                }
                else if( unit.type == LogicUnitType.Demolisher )
                {
                    demolishers.Remove( unit.id );
                    sideDemolishers[side].Remove( (Demolisher)unit );
                }
                else if( unit.type == LogicUnitType.CrystalCar )
                {
                    crystalCars.Remove( unit.id );
                    sideCrystalCars[side].Remove( (CrystalCar)unit );
                }
                else if( unit.type == LogicUnitType.Projectile )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Projectile, "remove projectile " + unit.id );
                    projectiles.Remove( unit.id );
                }
                else if ( unit.type == LogicUnitType.PowerUp )
                {
                    powerUps.Remove( unit.id );
                }
                else if ( unit.type == LogicUnitType.Skill )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Skill, "remove skill " + unit.id );
                    skills.Remove( unit.id );
                }
                else if ( unit.type == LogicUnitType.NPC )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Npc, "remove NPC " + unit.id );
                    npcs.Remove( unit.id );
                }
                else if ( unit.type == LogicUnitType.Idol )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Npc, "remove Idol " + unit.id );
                    idols.Remove( unit.id );
                }
                else if ( unit.type == LogicUnitType.IdolGuard )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Npc, "remove IdolGuard " + unit.id );
                    idolGuards.Remove( unit.id );
                }
                else if ( unit.type == LogicUnitType.Summon )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Summon, "remove summon " + unit.id );
                    summons.Remove( unit.id );
                }
			    else if ( unit.type == LogicUnitType.Trap )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Trap , "remove Trap " + unit.id );
                    traps.Remove( unit.id );
                }
                else
                {
                    DebugUtils.LogError( DebugUtils.Type.AI_LogicWorld, "the game can't destroy this one : " + unit.type );
                }

                if ( unit.pool != null )
                {
                    unit.Recycle();
                }
            }

            garbges.Clear();
        }

    }
}

