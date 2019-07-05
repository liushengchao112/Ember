using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Utils;
using TutorialModeStage = PVE.TutorialModeManager.TutorialModeStage;

namespace UI
{
    public class NewbieGuideController
    {
        private NewbieGuideView view;
        private DataManager dataManager;

        public Dictionary<int, string> normallyGuidePathDic = new Dictionary<int, string>();
        public Dictionary<int, string> bulidingGuidePathDic = new Dictionary<int, string>();
        private int currentGuideIndex = 0;
        private TutorialModeStage currentTutorialMode;
		private List<Vector3> firstFormations;
		private List<Vector3> secondFormations;
        private BattleType battleType;

        public NewbieGuideController( NewbieGuideView v )
        {
            view = v;
            dataManager = DataManager.GetInstance();

            battleType = dataManager.GetBattleType();

            MessageDispatcher.AddObserver( TutorialModeReady, Constants.MessageType.TutorialModeReady );
            MessageDispatcher.AddObserver( view.GoNext, Constants.MessageType.TutorialGoNext );
            MessageDispatcher.AddObserver( view.TutorialStop, Constants.MessageType.TutorialStop );
            MessageDispatcher.AddObserver( view.TutorialFinish, Constants.MessageType.TutorialStageComplete );

            normallyGuidePathDic.Add( 1, "BattleScreen,BattleBottomPanel/SquadCardItemGroup/SquadCard_Item" );
            normallyGuidePathDic.Add( 2, "BattleScreen,BattleBottomPanel/SquadCardItemGroup/SquadCard_Item (1)" );
            normallyGuidePathDic.Add( 3, "BattleScreen,BattleBottomPanel/SquadCardItemGroup/SquadCard_Item (2)" );
            normallyGuidePathDic.Add( 4, "BattleScreen,BattleLeftPanel/DragUnitCardPanel/UnitCardItemGroup/UnitCard_Item" );
            normallyGuidePathDic.Add( 10, "BattleScreen,BattleRightPanel/ControlRoot/ControlAni/ConcentratedFire/IconImage" );

            bulidingGuidePathDic.Add( 3, "AlertUI(Clone),ConfirmButton" );
            bulidingGuidePathDic.Add( 5, "BattleScreen,BattleCenterPanel/InstitutePopUp/DragInstituteSkillPanel/InstituteSkillGroup/InstituteSkillItem_0" );
            bulidingGuidePathDic.Add( 6, "BattleScreen,BattleCenterPanel/InstitutePopUp/LevelUpButton" );
            bulidingGuidePathDic.Add( 7, "BattleScreen,BattleCenterPanel/InstitutePopUp/CloseButton" );
            bulidingGuidePathDic.Add( 9, "BattleScreen,BattleCenterPanel/TowerPopUp/DragTowerItemPanel/TowerItemGroup/TowerItem_0/ClickButton" );
        }

        public void OnDestroy()
        {
            MessageDispatcher.RemoveObserver( TutorialModeReady, Constants.MessageType.TutorialModeReady );
            MessageDispatcher.RemoveObserver( view.GoNext, Constants.MessageType.TutorialGoNext );
            MessageDispatcher.RemoveObserver( view.TutorialFinish, Constants.MessageType.TutorialStageComplete );
            MessageDispatcher.RemoveObserver( view.TutorialStop, Constants.MessageType.TutorialStop );
        }

        public BattleType GetBattleType()
        {
            return battleType;
        }

        private void TutorialModeReady( object mode, object firstFormations, object secondFormations )
        {
            currentTutorialMode = (TutorialModeStage)mode;
			this.firstFormations = ( List<Vector3> )firstFormations;
			this.secondFormations = (List<Vector3>)secondFormations;
        }

        public void OnShowNewbieUI( int index, int lastIndex )
        {
            view.SetGuideIndex( index, lastIndex );

            view.ShowNewbieUI();
        }

        public void ResetNewbieGuide( int index, int lastIndex )
        {
            currentGuideIndex = (int)index;
            view.SetGuideIndex( currentGuideIndex, lastIndex );
        }

        public string GetUIPrefabName()
        {
            Dictionary<int, string> guidePathDic = new Dictionary<int, string>();
            if ( currentTutorialMode == TutorialModeStage.NormallyControlOperation_Stage )
                guidePathDic = normallyGuidePathDic;
            else if ( currentTutorialMode == TutorialModeStage.BuildingControlOperation_Stage )
                guidePathDic = bulidingGuidePathDic;

            if ( guidePathDic.ContainsKey( currentGuideIndex ) )
            {
                return guidePathDic[currentGuideIndex].Split( ',' )[0];
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetUIPath()
        {
            Dictionary<int, string> guidePathDic = new Dictionary<int, string>();
            if ( currentTutorialMode == TutorialModeStage.NormallyControlOperation_Stage )
                guidePathDic = normallyGuidePathDic;
            else if ( currentTutorialMode == TutorialModeStage.BuildingControlOperation_Stage )
                guidePathDic = bulidingGuidePathDic;

            if ( guidePathDic.ContainsKey( currentGuideIndex ) )
            {
                return guidePathDic[currentGuideIndex].Split( ',' )[1];
            }
            else
            {
                return string.Empty;
            }
        }

        public int GetPlayerLv()
        {
            return dataManager.GetPlayerLevel();
        }

        public Vector3 GetUnitPostion( int unitIndex )
        {
            if ( firstFormations != null && firstFormations.Count > unitIndex )
            {
                return firstFormations[unitIndex];
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Map, " Error unitFormations" );
                return Vector3.zero;
            }
        }

        public Vector3 GetBoxPostion( int boxIndex )
        {
            if ( secondFormations != null && secondFormations.Count > boxIndex )
            {
                return secondFormations[boxIndex];
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Map, " Error unitFormations" );
                return Vector3.zero;
            }
        }

        public Vector3 GetInstitutePostion( int instituteIndex )
        {
            if ( firstFormations != null && firstFormations.Count > instituteIndex )
            {
                return firstFormations[instituteIndex];
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Map, " Error instituteFormations" );
                return Vector3.zero;
            }
        }

        public Vector3 GetTowerPostion( int towerIndex )
        {
            if ( secondFormations != null && secondFormations.Count > towerIndex )
            {
                return secondFormations[towerIndex];
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Map, " Error unitFormations" );
                return Vector3.zero;
            }
        }
    }
}