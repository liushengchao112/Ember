using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Logic;
using Data;
using Utils;
using Constants;
using Network;
using BattleUnit = Data.Battler.BattleUnit;

namespace PVE
{
	//This manager control all tutorial stage and index check and order UI logic. Dwayne.
	public class TutorialModeManager
	{
		public enum TutorialModeStage
		{
			Null = 0,
			NormallyControlOperation_Stage = 1,
			//BattleControlOperation_Stage,
			BuildingControlOperation_Stage,
			OptionalTrainingOperation_Stage,
			SkipTutorial = 6,
		}

		#region TutorialModeManager values

		private Dictionary<TutorialModeStage,int> stageIndexLimit; 
			
		private int tutorialIndex = -1;
		private TutorialModeStage tutorialStage = TutorialModeStage.Null;

		private MapData1V1 mapData;//This is temp, waiting tutorial mapdata need changed.
		private NavigateCamera mainCamera;

		//PathPoint Detected Soldernumbers,if need soldier data change this value.

		private int playerSoldiersOnPosNum = 0;

		//Tutorial mode ai used soldiersID
		private List<long> aiHoldSoldierID;

		private DataManager dataManager;
		private InputHandler inputHandler;
		private LogicWorld logicWorld;

		private bool isNeedCheckUnitNumEndTutorial = false;

		#endregion

		#region Default functions

		public void InitTutorialMode()
		{
			//TODO:Need add more logic.
			stageIndexLimit = new Dictionary<TutorialModeStage, int>();
			stageIndexLimit.Add( TutorialModeStage.NormallyControlOperation_Stage, 99 );//TODO:This 99 is temp number when finished tutorial the number must be equal index limit.
			stageIndexLimit.Add( TutorialModeStage.BuildingControlOperation_Stage, 99 );//TODO:This 99 is temp number when finished tutorial the number must be equal index limit.

			tutorialIndex = 0;
			DebugUtils.Log( DebugUtils.Type.Tutorial, string.Format( "InitTutorialMode and registerMessage, now tutorialIndex is {0}", tutorialIndex ));

			dataManager = DataManager.GetInstance();
			inputHandler = InputHandler.Instance;
			aiHoldSoldierID = new List<long>();

			mainCamera.SetCameraVector2Position( mapData.GetTutorialCameraPos( TutorialModeStage.NormallyControlOperation_Stage, 1 ) );

			MessageDispatcher.AddObserver( TutorialUpdate, MessageType.TutorialUpData );
			MessageDispatcher.AddObserver( TutorialModeManagerFeedBack, MessageType.InitNewbieGuideSucceed );
			MessageDispatcher.AddObserver( TutorialModePathPointFirstStage, MessageType.TutorialPathPointMessageSend);
			MessageDispatcher.AddObserver( DestorySoldier, MessageType.SoldierDeath );

			NetworkManager.RegisterServerMessageHandler( ServerType.GameServer, MsgCode.NoviceGuidanceMessage, ServerFeedBack );
			NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.NoviceGuidanceMessage, SocialServerFeedBack );
		}

		public void OnDestroy()
		{
			MessageDispatcher.RemoveObserver( TutorialUpdate, MessageType.TutorialUpData );
			MessageDispatcher.RemoveObserver( TutorialModeManagerFeedBack, MessageType.InitNewbieGuideSucceed );
			MessageDispatcher.RemoveObserver( TutorialModePathPointFirstStage, MessageType.TutorialPathPointMessageSend );
			MessageDispatcher.RemoveObserver( DestorySoldier, MessageType.SoldierDeath );

			NetworkManager.RemoveServerMessageHandler( ServerType.GameServer, MsgCode.NoviceGuidanceMessage, ServerFeedBack );
			NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.NoviceGuidanceMessage, SocialServerFeedBack );
		}

		//TutorialMode manager self check and feed back data.
		private void TutorialModeManagerFeedBack()
		{
			if( tutorialStage == TutorialModeStage.Null )
			{
				DebugUtils.LogError( DebugUtils.Type.Tutorial, "Error! The tutorialStage not set." );
				return;
			}

			if( tutorialIndex == -1 )
			{
				DebugUtils.LogError( DebugUtils.Type.Tutorial, "Error! The tutorialIndex not initialize." );
				return;
			}

			if( mapData == null )
			{
				DebugUtils.LogError( DebugUtils.Type.Tutorial, "Error! The tutorial mapdata not set." );
				return;
			}

			if( mainCamera == null )
			{
				DebugUtils.LogError( DebugUtils.Type.Tutorial, "Error! The battle use camera not set." );
				return;
			}

			//Tutorial mode manager all ready.
			if( tutorialStage == TutorialModeStage.NormallyControlOperation_Stage )
			{
				MessageDispatcher.PostMessage( MessageType.TutorialModeReady, tutorialStage, mapData.GetFormationPoint( ForceMark.BottomBlueForce ), mapData.GetTutorialPathPointPosition() );
			}
			else if( tutorialStage == TutorialModeStage.BuildingControlOperation_Stage )
			{
				List<Vector3> instituteBasePos;
				List<Vector3> towerBasePos;

				mapData.instituteBase.TryGetValue( ForceMark.TopBlueForce, out instituteBasePos );
				mapData.towerBase.TryGetValue( ForceMark.TopBlueForce, out towerBasePos );

				if( instituteBasePos == null | towerBasePos == null )
				{
					DebugUtils.LogError( DebugUtils.Type.Tutorial, string.Format( "The instituteBasePos or towerBasePos can't be null! Check mapData." ) );
				}

				MessageDispatcher.PostMessage( MessageType.TutorialModeReady, tutorialStage, instituteBasePos, towerBasePos );
			}
		}

		public void TutorialModeManagerUpdate()
		{
			if( isNeedCheckUnitNumEndTutorial )
			{
				CheckAllUnitDiedEndTheTutorial();
			}
		}

		#endregion

		#region StageControl functions

		public void SetTutorialStage( TutorialModeStage stage )
		{
			tutorialStage = stage;
		}

		public TutorialModeStage GetTutorialStage()
		{
			return tutorialStage;
		}

		#endregion

		#region IndexControl functions

		private void TutorialUpdate()
		{
			this.tutorialIndex++;

			if( tutorialStage == TutorialModeStage.NormallyControlOperation_Stage )
			{
				CheckNormallyControlOperation_StageKeyIndex();
			}
			else if( tutorialStage == TutorialModeStage.BuildingControlOperation_Stage )
			{
				//TODO: Need add buildingContorlOperation logic.
			}

			CheckTutorialIndex();
		}

		public void CheckTutorialIndex()
		{
			int index = -1;

			stageIndexLimit.TryGetValue( tutorialStage ,out index );

			if( index != -1 )
			{
				if( tutorialIndex == index )
				{
					MessageDispatcher.PostMessage( MessageType.TutorialStop );
				}
				else if( tutorialIndex < index )
				{
					MessageDispatcher.PostMessage( MessageType.TutorialGoNext, tutorialIndex );
				}
				else 
				{
					DebugUtils.LogError( DebugUtils.Type.Tutorial, "Out range tutorial index maximum, Check logic." );
				}
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Tutorial, string.Format( "Can't find this stage {0} index.Please check this.", tutorialStage ) );
			}
		}

		private void CheckNormallyControlOperation_StageKeyIndex()
		{
			if( tutorialIndex == 10 )//In NormallyControlOperation stage, 12 index need give player a taget unit to kill.
			{
				GenerateDollUnit();
			}
			else if( tutorialIndex == 12 )
			{
				MessageDispatcher.PostMessage( MessageType.TutorialStop );//There will show player all scene
				isNeedCheckUnitNumEndTutorial = true;//Open check ai unit is it dies, when unit died finished this stage.
			}
		}

		#endregion

		#region SetMapData

		public void SetMapData( MapData1V1 mapData )
		{
			this.mapData = mapData;
		}

		public void SetMainCamera( NavigateCamera camera )
		{
			this.mainCamera = camera;
		}

		public void SetLogicworld( LogicWorld logicworld )
		{
			this.logicWorld = logicworld;
		}

		#endregion

		#region TutorialPathPoint functions

		private void TutorialModePathPointFirstStage( object datas )
		{
			playerSoldiersOnPosNum = ( int )datas;

			if( playerSoldiersOnPosNum == 3 )
			{
				tutorialIndex++;
				MessageDispatcher.PostMessage( MessageType.TutorialGoNext, tutorialIndex );
				//SendSaveKeyStageMessage();
			}
		}

		#endregion

		#region TutorialSendServerMessage

		//When stage finished use this tell server.
		private void SendSaveKeyStageMessage()
		{
			NoviceGuidanceC2S message = new NoviceGuidanceC2S();
			message.guideStateType = ( GuideStateType ) tutorialStage;
			byte[] stream = ProtobufUtils.Serialize( message );

			NetworkManager.SendRequest( ServerType.GameServer, MsgCode.NoviceGuidanceMessage, stream );
			NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.NoviceGuidanceMessage, stream );
		}

		private void ServerFeedBack( byte[] feedback )
		{
			NoviceGuidanceS2C data = ProtobufUtils.Deserialize<NoviceGuidanceS2C>( feedback );

			if( data.result )
			{
				MessageDispatcher.PostMessage( MessageType.TutorialStageComplete, tutorialStage );
				MessageDispatcher.PostMessage( MessageType.TutorialShowResult );
				DebugUtils.Log( DebugUtils.Type.Tutorial, "Message saveKeyValue send complete." );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Tutorial, "Tutorial finish error, Server feedBack is false, please tell server check this NoviceGuidanceS2C data." );
			}
		}			

		private void SocialServerFeedBack( byte[] feedback )
		{
			NoviceGuidanceS2C data = ProtobufUtils.Deserialize<NoviceGuidanceS2C>( feedback );

			if( data.result )
			{
				DebugUtils.Log( DebugUtils.Type.Tutorial, "Message saveKeyValue send complete." );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Tutorial, "Tutorial finish error, Server feedBack is false, please tell server check this NoviceGuidanceS2C data." );
			}
		}

		#endregion

		#region TutorialUnit functions

		//Generate a week unit use for player kill.
		private void GenerateDollUnit()
		{
			Vector2 pos = mapData.GetTutorialCameraPos( TutorialModeStage.NormallyControlOperation_Stage, 2 );
			pos.x += 3;

			mainCamera.SetCameraVector2PositionLerp( pos, 0.75f );
			BattleUnit model = new BattleUnit(); 

			aiHoldSoldierID.Add( inputHandler.SpawnSoldierInMap( ForceMark.TopRedForce, 30001, mapData.GetTutorialPathPointPosition()[1], model ));
			Soldier weekUnit;
			logicWorld.soldiers.TryGetValue( aiHoldSoldierID[0], out weekUnit );

			if( weekUnit != null )
			{
                weekUnit.physicalAttack = Mathf.RoundToInt( weekUnit.physicalAttack * 0.5f );//This unit just have 50% physicalAttack.
            }

            //When TutorialModeStage.NormallyControlOperation_Stage all unit can't use skill
            if ( tutorialStage == TutorialModeStage.NormallyControlOperation_Stage )
			{
				ClosedAllUnitSkill();
			}
		}

		private void DestorySoldier( object side, object id )
		{
			long soldierID = ( long )id;
			
			if( aiHoldSoldierID.Contains( soldierID ) )
			{
				aiHoldSoldierID.Remove( soldierID );
			}
		}

		private void CheckAllUnitDiedEndTheTutorial()
		{
			if( aiHoldSoldierID.Count == 0 )
			{
				isNeedCheckUnitNumEndTutorial = false;
				//TODO: There need add victory and back main menu functions.
				DebugUtils.Log( DebugUtils.Type.Tutorial, "NormallyControlOperation_Stage Tutorial finished." );
				//NeedTell server clean frame.
				SendSaveKeyStageMessage();
			}
		}

		private void ClosedAllUnitSkill()
		{
			inputHandler.SetSimUnitSkillEnable( dataManager.GetPVEAIID(), false );
			inputHandler.SetSimUnitSkillEnable( dataManager.GetPlayerId(), false );
		}

		private void OpenAllUnitSkill()
		{
			inputHandler.SetSimUnitSkillEnable( dataManager.GetPVEAIID(), true );
			inputHandler.SetSimUnitSkillEnable( dataManager.GetPlayerId(), true );
		}

		#endregion
	}
}
