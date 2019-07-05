using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Network;
using Utils;
using TutorialStage = PVE.TutorialModeManager.TutorialModeStage;
using UnityEngine.SceneManagement;

namespace UI
{
    public class TutorialModeController : ControllerBase
    {
        public TutorialModeView view;
		public DataManager clientData;
        public TutorialModeController( TutorialModeView v )
        {
            viewBase = v;
            view = v;
        }

		public void RegisterServerMessageHandler()
		{
			NetworkManager.RegisterServerMessageHandler( ServerType.GameServer, MsgCode.CheckNoviceGuidanceMessage, ServerFeedBack );
		}

		public override void OnPause()
		{
			base.OnPause();

			NetworkManager.RemoveServerMessageHandler( ServerType.GameServer, MsgCode.CheckNoviceGuidanceMessage, ServerFeedBack );
		}

		/// <summary>
		/// Build tutorial battle data.
		/// </summary>
		public void SetTutorialBattleData()
		{
			if( clientData == null )
			{
				clientData = DataManager.GetInstance();
			}

			clientData.ResetMatchers();

			Matcher robot = new Matcher();
			robot.playerId = 0;     //AI's ID must be consistent with entering the battle
			robot.name = "怠惰的教官";
			robot.side = MatchSide.Red;
			robot.portrait = "EmberAvatar_10";

			Matcher myself = new Matcher();
			myself.playerId = clientData.GetPlayerId();
			myself.name = clientData.GetPlayerNickName();
			myself.side = MatchSide.Blue;
			myself.portrait = clientData.GetPlayerHeadIcon();

			clientData.SetMatcher( robot );
			clientData.SetMatcher( myself );
		}

		public void SendCheckTutorialStage()
		{
			CheckNoviceGuidanceC2S message = new CheckNoviceGuidanceC2S();
			byte[] stream = ProtobufUtils.Serialize( message );
			NetworkManager.SendRequest( ServerType.GameServer, MsgCode.CheckNoviceGuidanceMessage, stream );
		}

		private void ServerFeedBack( byte[] feedBack )
		{
			CheckNoviceGuidanceS2C data = ProtobufUtils.Deserialize<CheckNoviceGuidanceS2C>( feedBack );

			if( data.npcTraining == 1 )
			{
				//TODO:If have more tutorial mode add here.
			}
			else
			{
				//TODO:If have more tutorial mode add here.
			}

			if( data.skillTraining == 1 )
			{
				view.OpenJungleTrainingButton();
			}
			else
			{
				view.CloseJungleTrainingButton();
			}

			if( data.buildTraining == 1 )
			{
				view.OpenSkillTrainingButton();
			}
			else
			{
				view.CloseSkillTrainingButton();
			}

			if( data.basicOperation == 1 )
			{
				view.OpenBuildingTrainingButton();
			}
			else
			{
				view.CloseBuildingTrainingButton();
			}
		}

		public void IntoTutorialFightMatch( BattleType type, TutorialStage stage )
		{
			DataManager dataManager = DataManager.GetInstance();
			dataManager.SetBattleType( type, false );
			dataManager.SetTutorialStage( stage );
			dataManager.SimulatePVEData( type );
			SetTutorialBattleData();
			SceneManager.LoadSceneAsync( "Loading" );
		}
    }
}
