using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Utils;
using Data;

namespace UI
{
    public class MainMenuView : ViewBase
    {
        private Text noticeText;
        private ScrollRect dragModePanel;
        private GridLayoutGroup modeItemGroup;

        #region PVE
        private Transform pveTran;
        private Toggle pveToggle;
        private Image pveModeIcon,pveModeTextImage;
        private Transform pvePopUpUI;
        private Button pveModeBt1, pveModeBt2, pveModeBt3;
        #endregion
        #region PVP
        private Transform pvpTran;
        private Toggle pvpToggle;
        private Image pvpModeIcon,pvpModeTextImage;
        private Transform pvpPopUpUI;
        private Button pvpModeBt1, pvpModeBt2;
        #endregion
        #region HorseRaceLamp
        private HorseRaceLamp horseRaceLamp;
        #endregion
        #region Other
        private Transform otherTran;
        private Image otherModeIcon,otherModeTextImage;
        #endregion

        public MainMenuController controller;

        public override void OnInit()
        {
            base.OnInit();

            controller = new MainMenuController( this );
            _controller = controller;

            noticeText = transform.Find( "MaskImage/NoticeText" ).GetComponent<Text>();
            dragModePanel = transform.Find( "DragModePanel" ).GetComponent<ScrollRect>();
            modeItemGroup = transform.Find( "DragModePanel/ModeItemGroup" ).GetComponent<GridLayoutGroup>();
            #region PVE
            pveTran = transform.Find( "DragModePanel/ModeItemGroup/PVEMode" );
            pveToggle = pveTran.Find( "ClickToggle" ).GetComponent<Toggle>();
            pveModeIcon = pveTran.Find( "ModeIcon" ).GetComponent<Image>();
            pveModeTextImage = pveTran.Find( "ModeTextImage" ).GetComponent<Image>();
            pvePopUpUI = pveTran.Find( "PopUpUI" );
            pveModeBt1 = pvePopUpUI.Find( "ModeBtGroup/ModeOneBt" ).GetComponent<Button>();
            pveModeBt2 = pvePopUpUI.Find( "ModeBtGroup/ModeTwoBt" ).GetComponent<Button>();
            pveModeBt3 = pvePopUpUI.Find( "ModeBtGroup/ModeThreeBt" ).GetComponent<Button>();

            pveModeIcon.SetGray( true );
            pveModeIcon.color = myGray;
            pveModeTextImage.SetGray( true );
            pveModeTextImage.color = myGray;
            #endregion
            #region PVP
            pvpTran = transform.Find( "DragModePanel/ModeItemGroup/PVPMode" );
            pvpToggle = pvpTran.Find( "ClickToggle" ).GetComponent<Toggle>();
            pvpModeIcon = pvpTran.Find( "ModeIcon" ).GetComponent<Image>();
            pvpModeTextImage = pvpTran.Find( "ModeTextImage" ).GetComponent<Image>();
            pvpPopUpUI = pvpTran.Find( "PopUpUI" );
            pvpModeBt1 = pvpPopUpUI.Find( "ModeBtGroup/ModeOneBt" ).GetComponent<Button>();
            pvpModeBt2 = pvpPopUpUI.Find( "ModeBtGroup/ModeTwoBt" ).GetComponent<Button>();
            #endregion
            #region HorseRaceLamp
            horseRaceLamp = transform.Find("MaskImage").gameObject.AddComponent<HorseRaceLamp>();
            #endregion

            #region Other
            otherTran = transform.Find( "DragModePanel/ModeItemGroup/OtherMode" );
            otherModeIcon = otherTran.Find( "ModeIcon" ).GetComponent<Image>();
            otherModeTextImage = otherTran.Find( "ModeTextImage" ).GetComponent<Image>();

            otherModeIcon.SetGray( true );
            otherModeIcon.color = myGray;
            otherModeTextImage.SetGray( true );
            otherModeTextImage.color = myGray;
            #endregion

            pveToggle.AddListener( OnClickPVETg );
            pvpToggle.AddListener( OnClickPVPTg );

            pveModeBt1.AddListener( OnClickPVEModeBt1 );
            pveModeBt2.AddListener( OnClickPVEModeBt2 );
            pveModeBt3.AddListener( OnClickPVEModeBt3 );

            pvpModeBt1.AddListener( OnClickPVPModeBt1 );
            pvpModeBt2.AddListener( OnClickPVPModeBt2 );

            controller.RegisterHorseRaceLampMessage();
        }
			
        public override void OnDestroy()
        {
            base.OnDestroy();
            controller.RemoveHorseRaceLampMessage();
        }
       
        #region Button & Toggle Event
        Color myGray = new Color( 150 / (float)255, 150 / (float)255, 150 / (float)255, 1 );

        private void OnClickPVETg( bool isOn )
        {
            pveToggle.interactable = !isOn;
            pvePopUpUI.gameObject.SetActive( isOn );
            pveModeIcon.SetGray( !isOn );
            pveModeIcon.color = isOn ? Color.white : myGray;
            pveModeTextImage.SetGray( !isOn );
            pveModeTextImage.color = isOn ? Color.white : myGray;
        }

        private void OnClickPVPTg( bool isOn )
        {
            pvpToggle.interactable = !isOn;
            pvpPopUpUI.gameObject.SetActive( isOn );
            pvpModeIcon.SetGray( !isOn );
            pvpModeIcon.color = isOn ? Color.white : myGray;
            pvpModeTextImage.SetGray( !isOn );
            pvpModeTextImage.color = isOn ? Color.white : myGray;
        }

        private void OnClickPVEModeBt1()
        {
            IntoFightMatch( true, BattleType.Tranining );
        }

        private void OnClickPVEModeBt2()
        {
           IntoFightMatch( true, BattleType.Survival );
        }

        private void OnClickPVEModeBt3()
        {
            UIManager.Instance.GetUIByType( UIType.MainLeftBar, ( ViewBase ui, System.Object param ) => { ( ui as MainLeftBarView ).SetLeftBarNoClick(); } );
            UIManager.Instance.GetUIByType( UIType.TutorialModeUI, ( ViewBase ui, System.Object param ) => { ui.OnEnter(); } );
        }

        private void OnClickPVPModeBt1()
        {
             IntoFightMatch( false, BattleType.BattleP1vsP1 );
        }

        private void OnClickPVPModeBt2()
        {
            IntoFightMatch( false, BattleType.BattleP2vsP2 );
        }

        public void IntoFightMatch( bool isPVE, BattleType type )
        {
            DataManager dataManager = DataManager.GetInstance();
            dataManager.SetBattleType( type, false );

            if ( isPVE )
            {
                dataManager.SimulatePVEData( type );
            }

            UIManager.Instance.GetUIByType( UIType.FightMatchScreen, ( ViewBase ui, System.Object param ) => { ( ui as FightMatchView ).EnterFightUI( isPVE, type ); } );
        }

        #endregion

        #region HorseRaceLamp 

        public void ShowHorseRaceLamp(LampMessage lamp)
        {
            switch (horseRaceLamp.GetCurretType())
            {
                case HorseRaceLampType.None:
                    horseRaceLamp.gameObject.SetActive(true);
                    horseRaceLamp.Init(lamp);
                    break;
                case HorseRaceLampType.ApplictionMessage:
                    if (lamp.type== HorseRaceLampType.ApplictionMessage)
                    {
                        horseRaceLamp.gameObject.SetActive(true);
                        horseRaceLamp.Init(lamp);
                    }
                    break;
                case HorseRaceLampType.PlayerMessage:
                    horseRaceLamp.gameObject.SetActive(true);
                    horseRaceLamp.Init(lamp);
                    break;
                default:
                    break;
            }           
        }
        #endregion
    }
}
