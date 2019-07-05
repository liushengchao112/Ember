using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Data;
using Utils;
using TutorialStage = PVE.TutorialModeManager.TutorialModeStage;

namespace UI
{
    public class TutorialModeView : ViewBase
    {
        private Transform modeTran1, modeTran2, modeTran3, modeTran4;
        private Button modeButton1, modeButton2, modeButton3, modeButton4;
        private Image modeIcon1, modeTextImage1, modeIcon2, modeTextImage2, modeIcon3, modeTextImage3, modeIcon4, modeTextImage4;
        private Image grayModeIcon1, grayModeTextImage1, grayModeIcon2, grayModeTextImage2, grayModeIcon3, grayModeTextImage3, grayModeIcon4, grayModeTextImage4;
        private Image modeBg1, grayModeBg1, modeBg2, grayModeBg2, modeBg3, grayModeBg3, modeBg4, grayModeBg4;
		private Image mode2Image;
		private Image mode3Image;
		private Image mode4Image;

        public TutorialModeController controller;

        public override void OnInit()
        {
            base.OnInit();
			controller = new TutorialModeController( this );
			_controller = controller;

            modeTran1 = transform.Find( "ModeGroup/ModeObj1" );
            modeIcon1 = modeTran1.Find( "FrameImage/ModeIcon" ).GetComponent<Image>();
            modeTextImage1 = modeTran1.Find( "FrameImage/ModeTextImage" ).GetComponent<Image>();
            grayModeIcon1 = modeTran1.Find( "FrameImage/GrayModeIcon" ).GetComponent<Image>();
            grayModeTextImage1 = modeTran1.Find( "FrameImage/GrayModeTextImage" ).GetComponent<Image>();
            modeBg1 = modeTran1.Find( "FrameImage/ModeBackground" ).GetComponent<Image>();
            grayModeBg1 = modeTran1.Find( "FrameImage/GrayModeBackground" ).GetComponent<Image>();
            modeButton1 = modeTran1.Find( "ModeButton" ).GetComponent<Button>();

            modeTran2 = transform.Find( "ModeGroup/ModeObj2" );
            modeIcon2 = modeTran2.Find( "FrameImage/ModeIcon" ).GetComponent<Image>();
            modeTextImage2 = modeTran2.Find( "FrameImage/ModeTextImage" ).GetComponent<Image>();
            grayModeIcon2 = modeTran2.Find( "FrameImage/GrayModeIcon" ).GetComponent<Image>();
            grayModeTextImage2 = modeTran2.Find( "FrameImage/GrayModeTextImage" ).GetComponent<Image>();
            modeBg2 = modeTran2.Find( "FrameImage/ModeBackground" ).GetComponent<Image>();
            grayModeBg2 = modeTran2.Find( "FrameImage/GrayModeBackground" ).GetComponent<Image>();
            modeButton2 = modeTran2.Find( "ModeButton" ).GetComponent<Button>();
			mode2Image = modeButton2.GetComponent<Image>();

            modeTran3 = transform.Find( "ModeGroup/ModeObj3" );
            modeIcon3 = modeTran3.Find( "FrameImage/ModeIcon" ).GetComponent<Image>();
            modeTextImage3 = modeTran3.Find( "FrameImage/ModeTextImage" ).GetComponent<Image>();
            grayModeIcon3 = modeTran3.Find( "FrameImage/GrayModeIcon" ).GetComponent<Image>();
            grayModeTextImage3 = modeTran3.Find( "FrameImage/GrayModeTextImage" ).GetComponent<Image>();
            modeBg3 = modeTran3.Find( "FrameImage/ModeBackground" ).GetComponent<Image>();
            grayModeBg3 = modeTran3.Find( "FrameImage/GrayModeBackground" ).GetComponent<Image>();
            modeButton3 = modeTran3.Find( "ModeButton" ).GetComponent<Button>();
			mode3Image = modeButton3.GetComponent<Image>();

            modeTran4 = transform.Find( "ModeGroup/ModeObj4" );
            modeIcon4 = modeTran4.Find( "FrameImage/ModeIcon" ).GetComponent<Image>();
            modeTextImage4 = modeTran4.Find( "FrameImage/ModeTextImage" ).GetComponent<Image>();
            grayModeIcon4 = modeTran4.Find( "FrameImage/GrayModeIcon" ).GetComponent<Image>();
            grayModeTextImage4 = modeTran4.Find( "FrameImage/GrayModeTextImage" ).GetComponent<Image>();
            modeBg4 = modeTran4.Find( "FrameImage/ModeBackground" ).GetComponent<Image>();
            grayModeBg4 = modeTran4.Find( "FrameImage/GrayModeBackground" ).GetComponent<Image>();
            modeButton4 = modeTran4.Find( "ModeButton" ).GetComponent<Button>();
			mode4Image = modeButton4.GetComponent<Image>();

            SetGrayMode1( true );
            ClickHandler.Get( modeButton1.gameObject ).onClickDown = OnClickDownModeBt1;
            ClickHandler.Get( modeButton1.gameObject ).onClickUp = OnClickUpModeBt1;
            modeButton1.AddListener( OnClickModeBt1 );

			CloseBuildingTrainingButton();
			CloseSkillTrainingButton();
			CloseJungleTrainingButton();
        }

		public override void OnEnter()
		{
			base.OnEnter();

			if( controller != null )
			{
				controller.RegisterServerMessageHandler();
				controller.SendCheckTutorialStage();
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Tutorial, string.Format( "Tutorial mode select panle controller is null, Check this." ) );
			}

		}

        #region Button Event

        private void OnClickDownModeBt1( GameObject obj )
        {
            SetGrayMode1( false );
        }

        private void OnClickDownModeBt2( GameObject obj )
        {
            SetGrayMode2( false );
        }

        private void OnClickDownModeBt3( GameObject obj )
        {
            SetGrayMode3( false );
        }

        private void OnClickDownModeBt4( GameObject obj )
        {
            SetGrayMode4( false );
        }

        private void OnClickUpModeBt1( GameObject obj )
        {
            SetGrayMode1( true );
        }

        private void OnClickUpModeBt2( GameObject obj )
        {
            SetGrayMode2( true );
        }

        private void OnClickUpModeBt3( GameObject obj )
        {
            SetGrayMode3( true );
        }

        private void OnClickUpModeBt4( GameObject obj )
        {
            SetGrayMode4( true );
        }

        private void OnClickModeBt1()
        {
            SetGrayMode1( true );
			//TODO: There need add check Tutorial data logic
			controller.IntoTutorialFightMatch( BattleType.Tutorial, TutorialStage.NormallyControlOperation_Stage );
        }

        private void OnClickModeBt2()
        {
            SetGrayMode2( true );
			//TODO: There need add check Tutorial data logic
			controller.IntoTutorialFightMatch( BattleType.Tutorial, TutorialStage.BuildingControlOperation_Stage );
        }

        private void OnClickModeBt3()
        {
            //SetGrayMode3( true );
        }

        private void OnClickModeBt4()
        {
            //SetGrayMode4( true );
        }

        #endregion

        #region Set Gray

        private void SetGrayMode1( bool state )
        {
            modeIcon1.gameObject.SetActive( !state );
            modeTextImage1.gameObject.SetActive( !state );
            modeBg1.gameObject.SetActive( !state );
            grayModeIcon1.gameObject.SetActive( state );
            grayModeTextImage1.gameObject.SetActive( state );
            grayModeBg1.gameObject.SetActive( state );
        }

        private void SetGrayMode2( bool state )
        {
            modeIcon2.gameObject.SetActive( !state );
            modeTextImage2.gameObject.SetActive( !state );
            modeBg2.gameObject.SetActive( !state );
            grayModeIcon2.gameObject.SetActive( state );
            grayModeTextImage2.gameObject.SetActive( state );
            grayModeBg2.gameObject.SetActive( state );
        }

        private void SetGrayMode3( bool state )
        {
            modeIcon3.gameObject.SetActive( !state );
            modeTextImage3.gameObject.SetActive( !state );
            modeBg3.gameObject.SetActive( !state );
            grayModeIcon3.gameObject.SetActive( state );
            grayModeTextImage3.gameObject.SetActive( state );
            grayModeBg3.gameObject.SetActive( state );
        }

        private void SetGrayMode4( bool state )
        {
            modeIcon4.gameObject.SetActive( !state );
            modeTextImage4.gameObject.SetActive( !state );
            modeBg4.gameObject.SetActive( !state );
            grayModeIcon4.gameObject.SetActive( state );
            grayModeTextImage4.gameObject.SetActive( state );
            grayModeBg4.gameObject.SetActive( state );
        }
			
        #endregion

		#region ButtonStatus switch

		public void OpenBuildingTrainingButton()
		{
			SetGrayMode2( true );
			ClickHandler.Get( modeButton2.gameObject ).onClickDown = OnClickDownModeBt2;
			ClickHandler.Get( modeButton2.gameObject ).onClickUp = OnClickUpModeBt2;
			modeButton2.AddListener( OnClickModeBt2 );
			mode2Image.SetGray( false );
		}

		public void OpenSkillTrainingButton()
		{
			SetGrayMode3( true );
			ClickHandler.Get( modeButton3.gameObject ).onClickDown = OnClickDownModeBt3;
			ClickHandler.Get( modeButton3.gameObject ).onClickUp = OnClickUpModeBt3;
			modeButton3.AddListener( OnClickModeBt3 );
			mode3Image.SetGray( false );
		}
		public void OpenJungleTrainingButton()
		{
			SetGrayMode4( true );
			ClickHandler.Get( modeButton4.gameObject ).onClickDown = OnClickDownModeBt4;
			ClickHandler.Get( modeButton4.gameObject ).onClickUp = OnClickUpModeBt4;
			modeButton4.AddListener( OnClickModeBt4 );
			mode4Image.SetGray( false );
		}

		public void CloseBuildingTrainingButton()
		{
			SetGrayMode2( true );
			ClickHandler.Get( modeButton2.gameObject ).onClickDown = null;
			ClickHandler.Get( modeButton2.gameObject ).onClickUp = null;
			modeButton2.onClick.RemoveListener( OnClickModeBt2 );
			mode2Image.SetGray( true );
		}

		public void CloseSkillTrainingButton()
		{
			SetGrayMode3( true );
			ClickHandler.Get( modeButton3.gameObject ).onClickDown = null;
			ClickHandler.Get( modeButton3.gameObject ).onClickUp = null;
			modeButton3.onClick.RemoveListener( OnClickModeBt3 );
			mode3Image.SetGray( true );
		}

		public void CloseJungleTrainingButton()
		{
			SetGrayMode4( true );
			ClickHandler.Get( modeButton4.gameObject ).onClickDown = null;
			ClickHandler.Get( modeButton4.gameObject ).onClickUp = null;
			modeButton4.onClick.RemoveListener( OnClickModeBt4 );
			mode4Image.SetGray( true );
		}
			
		#endregion
    }
}