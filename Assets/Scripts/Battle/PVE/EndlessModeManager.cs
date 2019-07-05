using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

using Data;
using Utils;
using Constants;
using Map;

using LogicWorld = Logic.LogicWorld;
using BattleUnit = Data.Battler.BattleUnit;

namespace PVE
{
	//Use for control EndlessMode AI soldiers generate and soldiers battle srategy. Dwayne.
    public class EndlessModeManager
    {
        InputHandler inputHandler;
	
		private Vector3 aiUnitBornPosition = Vector3.zero;
		private Vector3 playerTownPosition = Vector3.zero;

		private List<long> aiSoldierIDList;

		private bool isEndlessModeStop = false;

		#region AI use value

		private float aiActiveTimer;
		private float aiActiveCycle = 10f;

		private int sigeCarID = GameConstants.UNIT_DEMOLISHER_METAID;
		private long holdSigeCarID;
		private bool isHaveSigeCar;

		private LogicWorld logicWorld;
		private DataManager datamanager;

		#endregion

		#region EndLessModeDataCache

		private MapDataPVE mapData;

		private List<int> endLessModeBehaviorIDlist = new List<int>();
		private List<int> endLessModeWavesIDlist = new List<int>();
		private Dictionary<int,EndlessBehaviorProto.EndlessBehavior> endlessModeBehaviorData = new Dictionary<int, EndlessBehaviorProto.EndlessBehavior>();
		private Dictionary<int,EndlessWavesProto.EndlessWaves> endlessModeWavesData = new Dictionary<int, EndlessWavesProto.EndlessWaves>();
		private Dictionary<int,BattleUnit> endlessModeUnitData = new Dictionary<int, BattleUnit>();

		private int currentWaveId = 0;
		private int currentWaveQuantity = 0;

		private bool isWaitingForComplete = false;

		#endregion

		#region EndLessModeTimer

		private float endLessModeTimer = 0;
		private float currentWaveDelayTime = 0;

		private BattleUnit unit;

		#endregion

		#region soldierCache

		private List<ConscriptSoldier> conscriptSoldierCache;

		#endregion

		public void InitEndlessMode()
		{
			datamanager = DataManager.GetInstance();
			conscriptSoldierCache = new List<ConscriptSoldier>();

			for( int i = 0; i < 15; i++ )
			{
				ConscriptSoldier temp = new ConscriptSoldier();
				temp.Reset();
				temp.isExpired = true;
				conscriptSoldierCache.Add( temp );
			}

            InitData();
			MessageDispatcher.AddObserver( DestorySoldier, MessageType.SoldierDeath );
			MessageDispatcher.AddObserver( BattleStop, MessageType.BattleEnd );
			MessageDispatcher.AddObserver( PassiveDetectionAI, MessageType.BuildTower );
			MessageDispatcher.AddObserver( SoldierCommand, MessageType.SimUnitEnterIdleState );
			MessageDispatcher.AddObserver( DestroyDemolisher, MessageType.DemolisherDestroyed );
            //MessageDispatcher.AddObserver( BattlePause, MessageType.BattlePause );
        }

		public void OnDestroy()
		{
			MessageDispatcher.RemoveObserver( DestorySoldier, MessageType.SoldierDeath );
			MessageDispatcher.RemoveObserver( BattleStop, MessageType.BattleEnd );
			MessageDispatcher.RemoveObserver( PassiveDetectionAI, MessageType.BuildTower );
			MessageDispatcher.RemoveObserver( SoldierCommand, MessageType.SimUnitEnterIdleState );
			MessageDispatcher.RemoveObserver( DestroyDemolisher, MessageType.DemolisherDestroyed );
			//MessageDispatcher.RemoveObserver( BattlePause, MessageType.BattlePause );
		}

		public void EndlessModeUpdate()
		{
			if( currentWaveId != 0 && isEndlessModeStop == false )
			{
				EndlessModeTiming( GameConstants.LOGIC_FRAME_TIME );

				if( isWaitingForComplete && aiSoldierIDList.Count == 0 )
				{
					isWaitingForComplete = false;
				}
			}
		}

		//Now pve just use 1v1 map,if later can select more map need add here.
		public void SetMapData( MapDataPVE mapData )
		{
			this.mapData = mapData;
			aiUnitBornPosition = this.mapData.redBaseTransform.position;
			playerTownPosition = this.mapData.blueBaseTransform.position;
		}

		public void SetLogicWorld( LogicWorld logic )
		{
			this.logicWorld = logic;
		}

		#region InitEndlessModeData

        void InitData()
        {
            inputHandler = InputHandler.Instance;
            aiSoldierIDList = new List<long>();

			if( currentWaveId == 0 )
			{
				//There are EndlessMode waves data
				List<EndlessBehaviorProto.EndlessBehavior> tempDatasEndlessModeBehavior = DataManager.GetInstance().endlessBehaviorProtoData;
				
				for( int i = 0; i < tempDatasEndlessModeBehavior.Count; i++ )
				{
					int id = tempDatasEndlessModeBehavior[ i ].ID;
					endLessModeBehaviorIDlist.Add( id );
					endlessModeBehaviorData.Add( id, tempDatasEndlessModeBehavior[ i ] );
				}

				List<EndlessWavesProto.EndlessWaves> tempDatasEndlessModeWaves = DataManager.GetInstance().endlessWavesProtoData;

				for( int i = 0; i < tempDatasEndlessModeWaves.Count; i++ )
				{
					int id = tempDatasEndlessModeWaves[ i ].ID;
					endLessModeWavesIDlist.Add( id );
					endlessModeWavesData.Add( id, tempDatasEndlessModeWaves[ i ] );
				}

				//There is EndlessMode unit value data
				InitEndlessModeUnitData( tempDatasEndlessModeWaves[ 0 ] );
			}

			EndlessModeBehaviorLogic();
        }

		private void InitEndlessModeUnitData( EndlessWavesProto.EndlessWaves sampleData )
		{
			List<int> EndlessModeUnitUIDList = new List<int>();
			List<UnitsProto.Unit> unitsTempDatas = DataManager.GetInstance().unitsProtoData;

			EndlessModeUnitUIDList.Add( sampleData.UnitID_1 );
			EndlessModeUnitUIDList.Add( sampleData.UnitID_2 );
			EndlessModeUnitUIDList.Add( sampleData.UnitID_3 );
			EndlessModeUnitUIDList.Add( sampleData.UnitID_4 );
			EndlessModeUnitUIDList.Add( sampleData.UnitID_5 );
			EndlessModeUnitUIDList.Add( sampleData.UnitID_6 );
			EndlessModeUnitUIDList.Add( sampleData.UnitID_7 );
			EndlessModeUnitUIDList.Add( sampleData.UnitID_8 );
			EndlessModeUnitUIDList.Add( sampleData.UnitID_9 );
			EndlessModeUnitUIDList.Add( sampleData.UnitID_10 );

			for( int i = 0; i < EndlessModeUnitUIDList.Count; i++ )
			{
				if( EndlessModeUnitUIDList[i] > 0 )
				{
					BattleUnit model = new BattleUnit(); 
					UnitsProto.Unit data = DataManager.GetInstance().unitsProtoData.Find( p => p.ID == EndlessModeUnitUIDList[i] );

					if( data == null )
					{
						DebugUtils.LogError( DebugUtils.Type.Training, string.Format( "The Endless mode unit id error, can't find this unit id {0} unit, please check this.", EndlessModeUnitUIDList[i] ));
					}

                    PropertyInfo physicalAttack = new PropertyInfo();
                    physicalAttack.propertyType = PropertyType.PhysicalAttack;
                    physicalAttack.propertyValue = data.PhysicalAttack;
                    model.props.Add( physicalAttack );

                    PropertyInfo magicAttackValue = new PropertyInfo();
					magicAttackValue.propertyType = PropertyType.MagicAttack;
					magicAttackValue.propertyValue = data.MagicAttack;
					model.props.Add( magicAttackValue );

                    PropertyInfo armor = new PropertyInfo();
                    armor.propertyType = PropertyType.ArmorPro;
                    armor.propertyValue = data.Armor;
                    model.props.Add( armor );

                    PropertyInfo magicResist = new PropertyInfo();
                    magicResist.propertyType = PropertyType.MagicResist;
                    magicResist.propertyValue = data.MagicResist;
                    model.props.Add( magicResist );

                    PropertyInfo criticalChance = new PropertyInfo();
                    criticalChance.propertyType = PropertyType.CriticalChance;
                    criticalChance.propertyValue = data.CriticalChance;
                    model.props.Add( criticalChance );

                    PropertyInfo criticalDamage = new PropertyInfo();
                    criticalDamage.propertyType = PropertyType.CriticalDamage;
                    criticalDamage.propertyValue = data.CriticalDamage;
                    model.props.Add( criticalDamage );

                    PropertyInfo moveSpeed = new PropertyInfo();
                    moveSpeed.propertyType = PropertyType.Speed;
                    moveSpeed.propertyValue = data.MoveSpeed;
                    model.props.Add( moveSpeed );

                    PropertyInfo rangedMitigation = new PropertyInfo();
                    rangedMitigation.propertyType = PropertyType.AttackSpeed;
                    rangedMitigation.propertyValue = data.AttackInterval;
                    model.props.Add( rangedMitigation );

                    PropertyInfo attackRange = new PropertyInfo();
                    attackRange.propertyType = PropertyType.AttackRange;
                    attackRange.propertyValue = data.AttackRange;
                    model.props.Add( attackRange );

                    PropertyInfo health = new PropertyInfo();
                    health.propertyType = PropertyType.MaxHealth;
                    health.propertyValue = data.Health;
                    model.props.Add( health );

                    PropertyInfo healthRegen = new  PropertyInfo();
					healthRegen.propertyType = PropertyType.HealthRecover;
					healthRegen.propertyValue = data.HealthRegen;
					model.props.Add( healthRegen );

                    endlessModeUnitData.Add( EndlessModeUnitUIDList[i], model );
				}
			}
		}

		#endregion

		#region Timer, EndlessModeBehavior and Wave logic.

		private void EndlessModeTiming( float deltaTime )
		{
			EndLessModeWaveLogic( deltaTime );
			ConscriptTeamReadyBattle( deltaTime );
			EndlessActiveDetectionAI( deltaTime );
		}

		private void EndlessModeBehaviorLogic()
		{
			if( endLessModeBehaviorIDlist.Count == 1 )
			{
				EndlessBehaviorProto.EndlessBehavior data;
				endlessModeBehaviorData.TryGetValue( endLessModeBehaviorIDlist[ 0 ], out data );

				if( data != null )
				{
					currentWaveId = data.WaveID1;
					currentWaveDelayTime = data.WaveDelay1;
					currentWaveQuantity = data.WaveQuantity1;
				}
				else
				{
					DebugUtils.LogError( DebugUtils.Type.Endless, "Can not find EndlessModeBehavior data" );
				}
			}
			else
			{
				//TODO: maybe have other initial strategy, Now not need.
				DebugUtils.Log( DebugUtils.Type.Endless, "Now not need do anything" );
			}
		}

		private void EndLessModeWaveLogic( float deltaTime )
		{
			endLessModeTimer += deltaTime;

			if( endLessModeTimer >= currentWaveDelayTime && isWaitingForComplete == false )
			{
				EndlessWavesProto.EndlessWaves currentWave;
				endlessModeWavesData.TryGetValue( currentWaveId, out currentWave );

				if( currentWave != null )
				{
					for( int i = 0; i < currentWaveQuantity; i++ )
					{
						int rangeInt = Random.Range( 1, 101 );
						long id = 0;
						int currentWaveUnitID;

						currentWaveUnitID = GetWavesUnitID( rangeInt, currentWave );
						endlessModeUnitData.TryGetValue( currentWaveUnitID, out unit );

						if( unit != null && currentWaveUnitID != -1 )
						{
							unit = UnitLevelPropertyRatio( unit, currentWave );
							id = CreateSoldier( currentWaveUnitID, unit );

							DebugUtils.Assert( id != 0, "Endless Mode: Soldier ID is 0!" );

							aiSoldierIDList.Add( id );
						}
						else
						{
							DebugUtils.LogError( DebugUtils.Type.Endless, "Can not find this ID : " + currentWaveUnitID + "unit in EndlessModeUnitData" );
						}
							
						int pathMask = NavMeshAreaType.WALKABLE;//Walkable

                        if( rangeInt >= currentWave.Deploy_Lane )
                        {
							pathMask = NavMeshAreaType.BOTTOMPATH;//BottomPath
                        }

						DebugUtils.Log( DebugUtils.Type.Endless, "rangeInt is " + rangeInt + "pathMask is " + pathMask );

						ConscriptSoldier conscriptSoldier = null;

						for( int j = 0; j < conscriptSoldierCache.Count; j++ )
						{
							if( conscriptSoldierCache[j].isExpired )
							{
								conscriptSoldier = conscriptSoldierCache[j];
								conscriptSoldier.Reset();
								conscriptSoldier.soldierID = id;
								conscriptSoldier.navArea = pathMask;
								break;
							}
						}

						if( conscriptSoldier == null )
						{
							conscriptSoldier = new ConscriptSoldier();

							conscriptSoldier.soldierID = id;
							conscriptSoldier.navArea = pathMask;
							conscriptSoldier.readyFightTimer = 0f;

							conscriptSoldierCache.Add( conscriptSoldier );
						}
					}
				}
				else
				{
					DebugUtils.LogError( DebugUtils.Type.Endless, "Can not find EndlessMode AI unit data" );
				}

                // For the PVE statistics
				datamanager.AddPveWaveNumber();

                currentWaveId = currentWave.NextWave;
				currentWaveDelayTime = currentWave.NextDelay;
				currentWaveQuantity = currentWave.NextQuantity;

				if( currentWave.WaitForComplete == true )
				{
					isWaitingForComplete = true;
				}

				endLessModeTimer = 0;
			}
		}

		private void ConscriptTeamReadyBattle( float deltatime )
		{
			if( conscriptSoldierCache.Count > 0 )
			{
				for( int i = 0; i < conscriptSoldierCache.Count; i++ )
				{
					if( !conscriptSoldierCache[i].isExpired )
					{
						conscriptSoldierCache[i].readyFightTimer += deltatime;

						if( conscriptSoldierCache[i].readyFightTimer >= GameConstants.PVE_TRAININGMODE_SOLDIER_WAITINGTIME )
						{
							PathFinding( conscriptSoldierCache[i].soldierID, conscriptSoldierCache[i].navArea );
							conscriptSoldierCache[i].isExpired = true;
						}
					}
				}
			}
		}

		#endregion

		#region EndlessModeAI

		//When aiActiveCycle time, activation some logic to check player data and punish them! But now we just have Anti-Tower....
		private void EndlessActiveDetectionAI( float deltatime )
		{
			aiActiveTimer += deltatime;

			if( aiActiveTimer >= aiActiveCycle )
			{
				DetectionPlayerTowerData();
				aiActiveTimer = 0;
			}
		}

		private void DetectionPlayerTowerData()
		{
			if( isHaveSigeCar == false && logicWorld.sideTowers[MatchSide.Blue] != null )
			{
				if( logicWorld.sideTowers[MatchSide.Blue].Count > 0 )
				{
					DebugUtils.Log( DebugUtils.Type.Endless, string.Format( "The player side have Tower active AntiTower." ));
					AntiTower();
				}
			}
		}

		//Now just use for anti-Tower.If need more logic, need new message support.
		private void PassiveDetectionAI( object mark, object playerID, object v3Pos )
		{
			ForceMark playerMark = ( ForceMark )mark;

			if( playerMark == ForceMark.TopBlueForce && !isHaveSigeCar )
			{
				AntiTower();
			}
		}

		private void AntiTower()
		{
			holdSigeCarID = CreateSoldier( sigeCarID );
			aiSoldierIDList.Add( holdSigeCarID );
			isHaveSigeCar = true;
		}

		//Simple soldier command, order ai soldier attack player base.If need more logic, need soldier status support.
		private void SoldierCommand( object id )
		{
			long soldierID = ( long )id;

			PathFinding( soldierID, NavMeshAreaType.WALKABLE );//PathMask Walkable 

			DebugUtils.Log( DebugUtils.Type.Endless, string.Format( "SodierCommand active, soldier ID {0} go attack player base.", soldierID ));
		}

		#endregion

		private int GetWavesUnitID( int randomNum, EndlessWavesProto.EndlessWaves currentWave )
		{
			if( randomNum <= currentWave.Chance_1 )
			{
				return currentWave.UnitID_1;
			}
			else if( randomNum <= currentWave.Chance_2 )
			{
				return currentWave.UnitID_2;
			}
			else if( randomNum <= currentWave.Chance_3 )
			{
				return currentWave.UnitID_3;
			}
			else if( randomNum <= currentWave.Chance_4 )
			{
				return currentWave.UnitID_4;
			}
			else if( randomNum <= currentWave.Chance_5 )
			{
				return currentWave.UnitID_5;
			}
			else if( randomNum <= currentWave.Chance_6 )
			{
				return currentWave.UnitID_6;
			}
			else if( randomNum <= currentWave.Chance_7 )
			{
				return currentWave.UnitID_7;
			}
			else if( randomNum <= currentWave.Chance_8 )
			{
				return currentWave.UnitID_8;
			}
			else if( randomNum <= currentWave.Chance_9 )
			{
				return currentWave.UnitID_9;
			}
			else if( randomNum <= currentWave.Chance_10 )
			{
				return currentWave.UnitID_10;
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Endless, "EndlessMode soldier spwan all chance is not happend, Check the design!" );
				return -1;
			}
		}

		//TODO:If EndlessMode soldier value change roles confirm, modify this function.Dwayne.
		private BattleUnit UnitLevelPropertyRatio( BattleUnit unitData, EndlessWavesProto.EndlessWaves currentWave )
		{
			for( int i = 0; i < unitData.props.Count; i++ )
			{
				if( unitData.props[ i ].propertyType == PropertyType.MaxHealth )
				{
					unitData.props[ i ].propertyValue = unitData.props[ i ].propertyValue * currentWave.UnitLevel_PropertyRatio * currentWave.Wave_Level;
				}
				else if( unitData.props[ i ].propertyType == PropertyType.HealthRecover )
				{
					unitData.props[ i ].propertyValue = unitData.props[ i ].propertyValue * currentWave.UnitLevel_PropertyRatio * currentWave.Wave_Level;
				}
				else if( unitData.props[ i ].propertyType == PropertyType.PhysicalAttack )
				{
					unitData.props[ i ].propertyValue = unitData.props[ i ].propertyValue * currentWave.UnitLevel_PropertyRatio * currentWave.Wave_Level;
				}
				//else if( unitData.props[ i ].propertyType == PropertyType.RangedDmgBase )
				//{	
				//	unitData.props[ i ].propertyValue += unitData.props[ i ].propertyValue * currentWave.UnitLevel_PropertyRatio * currentWave.Wave_Level;
				//}
				else if( unitData.props[ i ].propertyType == PropertyType.AttackRange )
				{
					unitData.props[ i ].propertyValue = unitData.props[ i ].propertyValue * currentWave.UnitLevel_RangeRatio * currentWave.Wave_Level;
				}
				else if( unitData.props[ i ].propertyType == PropertyType.AttackRange )
				{
					unitData.props[ i ].propertyValue = unitData.props[ i ].propertyValue * currentWave.UnitLevel_DamageRadiusRatio * currentWave.Wave_Level;
				}
				else if( unitData.props[ i ].propertyType == PropertyType.Speed )
				{
					unitData.props[ i ].propertyValue = unitData.props[ i ].propertyValue * currentWave.UnitLevel_SpeedRatio * currentWave.Wave_Level;
				}
				else
				{
					DebugUtils.Log( DebugUtils.Type.Endless, "This prop not need modify value about TrainingWaves table" );
					continue;
				}
			}

			return unitData;
		}
			
		private long CreateSoldier( int soldierType, BattleUnit unitInfo = null )
        {
			long id = inputHandler.SpawnSoldierInMap( ForceMark.TopRedForce, soldierType, aiUnitBornPosition, unitInfo );

            return id;
        }

		//TODO: Now can not change soldier value, waiting soldier interface.
        void SetSoldierLevel( long id, int waveLevel )
        {

        }

		//If need more control, add target pos here.
		private void PathFinding( long id, int pathMask )
        {
			if( aiSoldierIDList.Contains( id ) )
			{
                inputHandler.ChangeSingleSoldierTarget( ForceMark.TopRedForce, id, playerTownPosition, pathMask );
			}
			else
			{
				DebugUtils.Log( DebugUtils.Type.Endless, "This soldier is died, Not need pathFinding." );
			}
        }

		private void DestorySoldier( object side, object id )
        {
            long soldierId = ( long )id;

			for( int i = 0; i < aiSoldierIDList.Count; i++ )
			{
				if( aiSoldierIDList[i] == soldierId )
				{
					if( holdSigeCarID == soldierId)
					{
						isHaveSigeCar = false;
						holdSigeCarID = -1;
					}

					aiSoldierIDList.Remove( aiSoldierIDList[i] );
				}
			}
        }

		private void DestroyDemolisher( object side, object id, object direction )
		{
			DestorySoldier( side, id );
		}

        private void BattleStop( object type )
		{
			isEndlessModeStop = true;
		}

//		private void BattlePause( object isPause )
//		{
//			bool pause = ( bool )isPause;
//			//TODO: if designer want when game pause do some things Add there.
//		}
    }
}
