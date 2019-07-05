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
	//Use for control TrainingMode and tutorial AI soldiers generate and soldiers battle srategy. Dwayne.
    public class TrainingModeManager
    {
		private enum TrainingGenerateType
		{
			Normaly,
			Tutorial
		}

        InputHandler inputHandler;

		private Vector3 aiUnitBornPosition = Vector3.zero;
		private Vector3 playerTownPosition = Vector3.zero;

		private List<long> aiSoldierIDList;

		private bool isTrainingModeBattleStop = false;

		#region AI use value

		private float aiActiveTimer;
		private float aiActiveCycle = 20f;

		private int sigeCarID = GameConstants.UNIT_DEMOLISHER_METAID;
		private long holdSigeCarID;
		private bool isHaveSigeCar;

		private LogicWorld logicWorld;
		private DataManager datamanager;

		#endregion

		#region TrainingModeDataCache

		private MapDataPVE mapData;

		private List<int>trainingModeBehaviorIDlist = new List<int>();
		private List<int>trainingModeWavesIDlist = new List<int>();
		private Dictionary<int,TrainingBehaviorProto.TrainingBehavior>trainingModeBehaviorData = new Dictionary<int, TrainingBehaviorProto.TrainingBehavior>();
		private Dictionary<int,TrainingWavesProto.TrainingWaves>trainingModeWavesData = new Dictionary<int, TrainingWavesProto.TrainingWaves>();
		private Dictionary<int,BattleUnit> trainingModeUnitData = new Dictionary<int, BattleUnit>();

		private int currentWaveId = 0;
		private int currentWaveQuantity = 0;

		private bool isWaitingForComplete = false;

		BattleUnit unit;

		#endregion

		#region TrainingMode Timer 

		private float trainingModeTimer = 0;
		private float currentWaveDelayTime = 0;

		#endregion 

		#region soldierCache

		private List<ConscriptSoldier> conscriptSoldierCache;

		#endregion

		#region tutorial values

		private bool isTutorial;

		#endregion

		public void InitTrainingMode()
        {
			datamanager = DataManager.GetInstance();
			conscriptSoldierCache = new List<ConscriptSoldier>();

			for( int i = 0; i < 8; i++ )
			{
				ConscriptSoldier temp = new ConscriptSoldier();
				temp.Reset();
				temp.isExpired = true;
				conscriptSoldierCache.Add( temp );
			}

            inputHandler = InputHandler.Instance;
            aiSoldierIDList = new List<long>();

			MessageDispatcher.AddObserver( DestorySoldier, MessageType.SoldierDeath );
			MessageDispatcher.AddObserver( BattleStop, MessageType.BattleEnd );
			MessageDispatcher.AddObserver( PassiveDetectionAI, MessageType.BuildTower );
			MessageDispatcher.AddObserver( SoldierCommand, MessageType.SimUnitEnterIdleState );

			//MessageDispatcher.AddObserver( BattlePause, MessageType.BattlePause );

			if( currentWaveId == 0 )
			{
				InitTrainingModeData();
			}

			//Tell ui trainingMode ready.
			MessageDispatcher.PostMessage( MessageType.TrainingModeReady );
        }
			
		public void OnDestroy()
		{
			MessageDispatcher.RemoveObserver( DestorySoldier, MessageType.SoldierDeath );
			MessageDispatcher.RemoveObserver( BattleStop, MessageType.BattleEnd );
			MessageDispatcher.RemoveObserver( PassiveDetectionAI, MessageType.BuildTower );
			MessageDispatcher.RemoveObserver( SoldierCommand, MessageType.SimUnitEnterIdleState );

			//MessageDispatcher.RemoveObserver( BattlePause, MessageType.BattlePause );
		}

		public void TrainingModeUpdate()
		{
			if( currentWaveId != 0 && isTrainingModeBattleStop == false )
			{
				TrainingModeTiming( GameConstants.LOGIC_FRAME_TIME );

				if( isWaitingForComplete && aiSoldierIDList.Count == 0 )
				{
					isWaitingForComplete = false;
				}
			}
		}

		public void SetMapData( MapDataPVE mapData )
		{
			this.mapData = mapData;
			aiUnitBornPosition = new Vector3( this.mapData.redBaseTransform.transform.position.x - 2, this.mapData.redBaseTransform.transform.position.y, this.mapData.redBaseTransform.transform.position.z );
			playerTownPosition = this.mapData.blueBaseTransform.position;
		}

		public void SetLogicWorld( LogicWorld logic )
		{
			logicWorld = logic;
		}

		#region InitTrainingModeData

		private void InitTrainingModeData()
		{
			//There are TrainingMode Waves data
			List<TrainingBehaviorProto.TrainingBehavior> tempDatasTrainingModeBehavior = DataManager.GetInstance().trainingBehaviorProtoData;

			for( int i = 0; i < tempDatasTrainingModeBehavior.Count; i++ )
			{
				int id = tempDatasTrainingModeBehavior[ i ].ID;
				trainingModeBehaviorIDlist.Add( id );
				trainingModeBehaviorData.Add( id, tempDatasTrainingModeBehavior[ i ] );
			}

			List<TrainingWavesProto.TrainingWaves> tempDatasTrainingModeWaves = DataManager.GetInstance().trainingWavesProtoData;

			for( int i = 0; i < tempDatasTrainingModeWaves.Count; i++ )
			{
				int id = tempDatasTrainingModeWaves[ i ].ID;
				trainingModeWavesIDlist.Add( id );
				trainingModeWavesData.Add( id, tempDatasTrainingModeWaves[i] );
			}

			//There is TrainingMode unit value data,Have 2 type.Need server support.
			//TODO:Here will add check player is tutorial.
			InitTrainingModeUnitData( tempDatasTrainingModeWaves[ 0 ], TrainingGenerateType.Normaly );

			TrainingModeBehaviorLogic();
		}

		private void InitTrainingModeUnitData( object sampleData, TrainingGenerateType dataType )
		{
			List<int> trainingModeUnitUIDList = new List<int>();
			List<UnitsProto.Unit> unitsTempDatas = DataManager.GetInstance().unitsProtoData;

			if( dataType == TrainingGenerateType.Normaly )
			{
				TrainingWavesProto.TrainingWaves data =	( TrainingWavesProto.TrainingWaves )sampleData;

				trainingModeUnitUIDList.Add( data.UnitID_1 );
				trainingModeUnitUIDList.Add( data.UnitID_2 );
				trainingModeUnitUIDList.Add( data.UnitID_3 );
				trainingModeUnitUIDList.Add( data.UnitID_4 );
				trainingModeUnitUIDList.Add( data.UnitID_5 );
				trainingModeUnitUIDList.Add( data.UnitID_6 );
				trainingModeUnitUIDList.Add( data.UnitID_7 );
				trainingModeUnitUIDList.Add( data.UnitID_8 );
				trainingModeUnitUIDList.Add( data.UnitID_9 );
				trainingModeUnitUIDList.Add( data.UnitID_10 );
			}
			else if( dataType == TrainingGenerateType.Tutorial )
			{
				//TODO: Need add tutorial table data.
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Training, string.Format( "Training can't know this type {0}", dataType ) );
			}

            for ( int i = 0; i < trainingModeUnitUIDList.Count; i++ )
            {
				if( trainingModeUnitUIDList[i] == 0 || trainingModeUnitUIDList[i] == -1 )
				{
					continue;
				}

                BattleUnit model = new BattleUnit();
				UnitsProto.Unit data = DataManager.GetInstance().unitsProtoData.Find( p => p.ID == trainingModeUnitUIDList[i] );

				if( data == null )
				{
					DebugUtils.LogError( DebugUtils.Type.Training, string.Format( "The Training mode unit id error, can't find this unit id {0} unit, please check this.", trainingModeUnitUIDList[i] ));
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

                PropertyInfo healthRegen = new PropertyInfo();
                healthRegen.propertyType = PropertyType.HealthRecover;
                healthRegen.propertyValue = data.HealthRegen;
                model.props.Add( healthRegen );
			
				trainingModeUnitData.Add( trainingModeUnitUIDList[i], model );
            }
        }

		#endregion
			
		#region TrainingModeTiming and Behaviorlogic WaveLogic

		private void TrainingModeTiming( float deltatime )
		{
			TrainingModeWaveLogic( deltatime );
			ConscriptTeamReadyBattle( deltatime );
			TrainingActiveDetectionAI( deltatime );
		}
			
		private void TrainingModeBehaviorLogic()
		{
			if( trainingModeBehaviorIDlist.Count == 1 )
			{
				TrainingBehaviorProto.TrainingBehavior data;
				trainingModeBehaviorData.TryGetValue( trainingModeBehaviorIDlist[0], out data );

				if( data != null )
				{
					currentWaveId = data.WaveID1;
					currentWaveDelayTime = data.WaveDelay1;
					currentWaveQuantity = data.WaveQuantity1;
				}
				else
				{
					DebugUtils.LogError( DebugUtils.Type.Training, "Can not find TrainingModeBehavior data" );
				}
			}
			else
			{
				//TODO: maybe have other initial strategy,Now not need
				DebugUtils.Log( DebugUtils.Type.Training, "Now not need do anything" );
			}
		}

		private void TrainingModeWaveLogic( float deltatime )
		{
			trainingModeTimer += deltatime;

			if( trainingModeTimer >= currentWaveDelayTime && isWaitingForComplete == false )
			{
				TrainingWavesProto.TrainingWaves currentWave;
				trainingModeWavesData.TryGetValue( currentWaveId, out currentWave );

				if( currentWave != null )
				{
					//TODO: there will change about designer, maybe need waiting a short time spacing to spwan new soldier
					for( int i = 0; i < currentWaveQuantity; i++ )
					{
						int rangeInt = Random.Range( 1, 101 );
						long id = 0;
						int currenWavetUnitID;

						currenWavetUnitID = GetWavesUnitID( rangeInt, currentWave );
						trainingModeUnitData.TryGetValue( currenWavetUnitID, out unit );
							
						if( unit != null && currenWavetUnitID != -1 )
						{
							unit = UnitLevelPropertyRatio( unit, currentWave );
							id = CreateSoldiers( currenWavetUnitID, unit );

							DebugUtils.Assert( id != 0, "Training Mode: Soldier ID is 0!" );

							aiSoldierIDList.Add( id );
						}
						else
						{
							DebugUtils.LogError( DebugUtils.Type.Training, "Can not find this ID : " + currenWavetUnitID + "unit in TrainingModeUnitData" );
						}
							
                        int pathMask = NavMeshAreaType.WALKABLE;//walkable

                        if( rangeInt >= currentWave.Deploy_Lane )
                        {
                            pathMask = NavMeshAreaType.BOTTOMPATH;//BottomPath
                        }

						DebugUtils.Log( DebugUtils.Type.Training, "rangeInt is " + rangeInt + "pathMask is " + pathMask );

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

                // For the PVE statistics
				datamanager.AddPveWaveNumber();

				currentWaveId = currentWave.NextWave;
				currentWaveDelayTime = currentWave.NextDelay;
				currentWaveQuantity = currentWave.NextQuantity;

				if( currentWave.WaitForComplete == true )
				{
					isWaitingForComplete = true;
				}

				trainingModeTimer = 0;
			}
			else
			{
				DebugUtils.Log( DebugUtils.Type.Training, "TrainingModeTiming... Not need to do anything" );
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

		#region TrainingModeAI

		//When aiActiveCycle time, activation some logic to check player data and punish them! But now we just have Anti-Tower....
		private void TrainingActiveDetectionAI( float deltatime )
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
			holdSigeCarID = CreateSoldiers( sigeCarID );
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

		private int GetWavesUnitID( int randomNum, TrainingWavesProto.TrainingWaves currentWave )
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
				DebugUtils.LogError( DebugUtils.Type.Training, "TrainingMode soldier spwan all chance is not happend, Check the design!" );
				return -1;
			}
		}

		private BattleUnit UnitLevelPropertyRatio( BattleUnit unitData, TrainingWavesProto.TrainingWaves currentWave )
		{
			//TODO:If need change init unit value.Need modify waveTable like endlessWaveTable

			int waveLV = 0;

			for( int i = 0; i < unitData.props.Count; i++ )
			{
				if( unitData.props[ i ].propertyType == PropertyType.MaxHealth )
				{
					unitData.props[ i ].propertyValue =  unitData.props[ i ].propertyValue * currentWave.UnitLevel_PropertyRatio * waveLV;
				}
				else if( unitData.props[ i ].propertyType == PropertyType.HealthRecover )
				{
					unitData.props[ i ].propertyValue = unitData.props[ i ].propertyValue * currentWave.UnitLevel_PropertyRatio * waveLV;
				}
				else if( unitData.props[ i ].propertyType == PropertyType.PhysicalAttack )
				{
					unitData.props[ i ].propertyValue = unitData.props[ i ].propertyValue * currentWave.UnitLevel_PropertyRatio * waveLV;
				}
				//else if( unitData.props[ i ].propertyType == PropertyType.RangedDmgBase )
				//{	
				//	unitData.props[ i ].propertyValue += unitData.props[ i ].propertyValue * currentWave.UnitLevel_PropertyRatio * waveLV;
				//}
				else if( unitData.props[ i ].propertyType == PropertyType.AttackRange )
				{
					unitData.props[ i ].propertyValue = unitData.props[ i ].propertyValue * currentWave.UnitLevel_RangeRatio * waveLV;
				}
				//else if( unitData.props[ i ].propertyType == PropertyType.AttackRadius )
				//{
				//	unitData.props[ i ].propertyValue += unitData.props[ i ].propertyValue * currentWave.UnitLevel_DamageRadiusRatio * waveLV;
				//}
				else if( unitData.props[ i ].propertyType == PropertyType.Speed )
				{
					unitData.props[ i ].propertyValue = unitData.props[ i ].propertyValue * currentWave.UnitLevel_SpeedRatio * waveLV;
				}
				else
				{
					DebugUtils.Log( DebugUtils.Type.Training, "This prop not need modify value about TrainingWaves table" );
					continue;
				}
			}

			return unitData;
		}

		private long CreateSoldiers( int soldierType, BattleUnit unitInfo = null )
        {
			long id = inputHandler.SpawnSoldierInMap( ForceMark.TopRedForce, soldierType, aiUnitBornPosition, unitInfo );

            return id;
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
				DebugUtils.Log( DebugUtils.Type.Training, "This soldier is died, Not need pathFinding." );
			}
        }

        void DestorySoldier( object side, object id )
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

        private void BattleStop( object type )
		{
			isTrainingModeBattleStop = true;
		}

//		private void BattlePause( object  isPause )
//		{
//			bool pause = ( bool )isPause;
//			//TODO: if designer want when game pause do some things Add there.
//		}
    }

	public class ConscriptSoldier
	{
		public float readyFightTimer;
		public int navArea;
		public long soldierID;
		public bool isExpired;

		public void Reset()
		{
			readyFightTimer = 0;
			navArea = -1;
			soldierID = -1;
			isExpired = false;
		}
	}
}
