using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using Utils;
using Constants;
using Data;

namespace UI
{
    public enum CameraViewType
    {
        High = 1,
        Middle = 2,
        Low = 3,
    }
    public enum QualitySettingType
    {
        High = 1,
        Middle = 2,
        Low = 3,
    }

    public class SettingView : ViewBase
    {
        //Setting UI
        private Transform settingTran;
        private Toggle baseSettingToggle, soundSettingToggle, operationSettingToggle;
        private Button closeButton;
        private Text titleText, baseSettingToggleText, soundSettingToggleText, operationSettingToggleText;

        //Base Setting UI
        private Transform baseSettingTran;
        private Toggle lowViewTg, middleViewTg, highViewTg, lowQualityTg, middleQualityTg, highQualityTg, onRecordScreenTg, offRecordScreenTg, onDamageNumTg, offDamageNumTg, onDisplayBarTg, offDisplayBarTg;
        private Text viewAngleText, quailtyText, recordScreenText, damageNumText, displayBarText, viewLowText, viewMiddleText, viewHightText, qualityLowText, qualityMiddleText, qualityHighText, onRecordScreenText, offRecordScreenText, onDamageNumText, offDamageNumText, onDisplayBarText, offDisplayBarText;

        //Sound Setting UI
        private Transform soundSettingTran;
        private Toggle onMusicToggle, offMusicToggle, onSoundToggle, offSoundToggle;
        private Text musicText, soundText, onMusicTgText, offMusicTgText, onSoundTgText, offSoundTgText;

        //Operation Setting UI
        private Transform operationSettingTran;
        private Toggle operationOneTg, operationTwoTg, operationThreeTg;
        private Image operationOneSelectImage, operationTwoSelectImage, operationThreeSelectImage;
        private Text operationOneTgText, operationTwoTgText, operationThreeTgText;
        private Text operationOneText, operationTwoText, operationThreeText;

        private SettingController controller;

        private Color myGray = new Color( 150 / (float)255, 150 / (float)255, 150 / (float)255, 1 );
        private Color textGray = new Color( 131 / (float)255, 138 / (float)255, 154 / (float)255 );
        private Color operationTextGray = new Color( 61 / (float)255, 126 / (float)255, 193 / (float)255 );

        public override void OnInit()
        {
            base.OnInit();

            controller = new SettingController( this );
            _controller = controller;

            #region Setting UI
            settingTran = transform.Find( "SettingUI" );
            baseSettingToggleText = settingTran.Find( "BaseSettingToggleText" ).GetComponent<Text>();
            soundSettingToggleText = settingTran.Find( "SoundSettingToggleText" ).GetComponent<Text>();
            operationSettingToggleText = settingTran.Find( "OperationSettingToggleText" ).GetComponent<Text>();

            baseSettingToggle = settingTran.Find( "ToggleGroup/BaseSettingToggle" ).GetComponent<Toggle>();
            soundSettingToggle = settingTran.Find( "ToggleGroup/SoundSettingToggle" ).GetComponent<Toggle>();
            operationSettingToggle = settingTran.Find( "ToggleGroup/OperationSettingToggle" ).GetComponent<Toggle>();
            closeButton = settingTran.Find( "CloseButton" ).GetComponent<Button>();

            baseSettingToggle.AddListener( OnClickBaseSettingTg );
            soundSettingToggle.AddListener( OnClickSoundSettingTg );
            operationSettingToggle.AddListener( OnClickOperationSettingTg );
            closeButton.AddListener( OnClickCloseButton );
            #endregion

            #region Base Setting UI
            baseSettingTran = transform.Find( "BaseSettingUI" );

            viewAngleText = baseSettingTran.Find( "ViewAngleText" ).GetComponent<Text>();
            quailtyText = baseSettingTran.Find( "QualityText" ).GetComponent<Text>();
            recordScreenText = baseSettingTran.Find( "RecordScreenText" ).GetComponent<Text>();
            damageNumText = baseSettingTran.Find( "DamageNumText" ).GetComponent<Text>();
            displayBarText = baseSettingTran.Find( "DisplayBarText" ).GetComponent<Text>();
            viewLowText = baseSettingTran.Find( "ViewLowText" ).GetComponent<Text>();
            viewMiddleText = baseSettingTran.Find( "ViewMiddleText" ).GetComponent<Text>();
            viewHightText = baseSettingTran.Find( "ViewHighText" ).GetComponent<Text>();
            qualityLowText = baseSettingTran.Find( "QualityLowText" ).GetComponent<Text>();
            qualityMiddleText = baseSettingTran.Find( "QualityMiddleText" ).GetComponent<Text>();
            qualityHighText = baseSettingTran.Find( "QualityHighText" ).GetComponent<Text>();
            onRecordScreenText = baseSettingTran.Find( "RecordScreenOnText" ).GetComponent<Text>();
            offRecordScreenText = baseSettingTran.Find( "RecordScreenOffText" ).GetComponent<Text>();
            onDamageNumText = baseSettingTran.Find( "DamageNumOnText" ).GetComponent<Text>();
            offDamageNumText = baseSettingTran.Find( "DamageNumOffText" ).GetComponent<Text>();
            onDisplayBarText = baseSettingTran.Find( "DisplayBarOnText" ).GetComponent<Text>();
            offDisplayBarText = baseSettingTran.Find( "DisplayBarOffText" ).GetComponent<Text>();

            lowViewTg = baseSettingTran.Find( "ViewAngleToggleGroup/LowToggle" ).GetComponent<Toggle>();
            middleViewTg = baseSettingTran.Find( "ViewAngleToggleGroup/MiddleToggle" ).GetComponent<Toggle>();
            highViewTg = baseSettingTran.Find( "ViewAngleToggleGroup/HighToggle" ).GetComponent<Toggle>();
            lowQualityTg = baseSettingTran.Find( "QualityToggleGroup/LowToggle" ).GetComponent<Toggle>();
            middleQualityTg = baseSettingTran.Find( "QualityToggleGroup/MiddleToggle" ).GetComponent<Toggle>();
            highQualityTg = baseSettingTran.Find( "QualityToggleGroup/HighToggle" ).GetComponent<Toggle>();
            onRecordScreenTg = baseSettingTran.Find( "RecordScreenToggleGroup/OnToggle" ).GetComponent<Toggle>();
            offRecordScreenTg = baseSettingTran.Find( "RecordScreenToggleGroup/OffToggle" ).GetComponent<Toggle>();
            onDamageNumTg = baseSettingTran.Find( "DamageNumToggleGroup/OnToggle" ).GetComponent<Toggle>();
            offDamageNumTg = baseSettingTran.Find( "DamageNumToggleGroup/OffToggle" ).GetComponent<Toggle>();
            onDisplayBarTg = baseSettingTran.Find( "DisplayBarToggleGroup/OnToggle" ).GetComponent<Toggle>();
            offDisplayBarTg = baseSettingTran.Find( "DisplayBarToggleGroup/OffToggle" ).GetComponent<Toggle>();

            lowViewTg.AddListener( OnClickLowViewTg );
            middleViewTg.AddListener( OnClickMiddleViewTg );
            highViewTg.AddListener( OnClickHighViewTg );
            lowQualityTg.AddListener( OnClickLowQualityTg );
            middleQualityTg.AddListener( OnClickMiddleQualityTg );
            highQualityTg.AddListener( OnClickHighQualityTg );
            onRecordScreenTg.AddListener( OnClickOnRecordScreenTg );
            offRecordScreenTg.AddListener( OnClickOffRecordScreenTg );
            onDamageNumTg.AddListener( OnClickOnDamageNumTg );
            offDamageNumTg.AddListener( OnClickOffDamageNumTg );
            onDisplayBarTg.AddListener( OnClickOnDisplayBarTg );
            offDisplayBarTg.AddListener( OnClickOffDisplaybarTg );
            #endregion

            #region Sound Setting UI
            soundSettingTran = transform.Find( "SoundSettingUI" );
            soundSettingTran.gameObject.SetActive( false );

            musicText = soundSettingTran.Find( "MusicText" ).GetComponent<Text>();
            soundText = soundSettingTran.Find( "SoundText" ).GetComponent<Text>();
            onMusicTgText = soundSettingTran.Find( "MusicOnText" ).GetComponent<Text>();
            offMusicTgText = soundSettingTran.Find( "MusicOffText" ).GetComponent<Text>();
            onSoundTgText = soundSettingTran.Find( "SoundOnText" ).GetComponent<Text>();
            offSoundTgText = soundSettingTran.Find( "SoundOffText" ).GetComponent<Text>();

            onMusicToggle = soundSettingTran.Find( "MusicToggleGroup/OnToggle" ).GetComponent<Toggle>();
            offMusicToggle = soundSettingTran.Find( "MusicToggleGroup/OffToggle" ).GetComponent<Toggle>();
            onSoundToggle = soundSettingTran.Find( "SoundToggleGroup/OnToggle" ).GetComponent<Toggle>();
            offSoundToggle = soundSettingTran.Find( "SoundToggleGroup/OffToggle" ).GetComponent<Toggle>();

            onMusicToggle.AddListener( OnClickOnMusicTg );
            offMusicToggle.AddListener( OnClickOffMusicTg );
            onSoundToggle.AddListener( OnClickOnSoundTg );
            offSoundToggle.AddListener( OnClickOffSoundTg );
            #endregion

            #region Operation Setting UI
            operationSettingTran = transform.Find( "OperationSettingUI" );
            operationOneSelectImage = operationSettingTran.Find( "OneSelectImage" ).GetComponent<Image>();
            operationTwoSelectImage = operationSettingTran.Find( "TwoSelectImage" ).GetComponent<Image>();
            operationThreeSelectImage = operationSettingTran.Find( "ThreeSelectImage" ).GetComponent<Image>();
            operationOneSelectImage.gameObject.SetActive( false );
            operationTwoSelectImage.gameObject.SetActive( false );
            operationThreeSelectImage.gameObject.SetActive( false );

            operationOneTgText = operationSettingTran.Find( "OneTgText" ).GetComponent<Text>();
            operationTwoTgText = operationSettingTran.Find( "TwoTgText" ).GetComponent<Text>();
            operationThreeTgText = operationSettingTran.Find( "ThreeTgText" ).GetComponent<Text>();
            operationOneText = operationSettingTran.Find( "OperationOneText" ).GetComponent<Text>();
            operationTwoText = operationSettingTran.Find( "OperationTwoText" ).GetComponent<Text>();
            operationThreeText = operationSettingTran.Find( "OperationThreeText" ).GetComponent<Text>();

            operationOneTg = operationSettingTran.Find( "OperationToggleGroup/OneToggle" ).GetComponent<Toggle>();
            operationTwoTg = operationSettingTran.Find( "OperationToggleGroup/TwoToggle" ).GetComponent<Toggle>();
            operationThreeTg = operationSettingTran.Find( "OperationToggleGroup/ThreeToggle" ).GetComponent<Toggle>();

            operationOneTg.AddListener( OnClickOperationOneTg );
            operationTwoTg.AddListener( OnClickOperationTwoTg );
            operationThreeTg.AddListener( OnClickOperationThreeTg );
            #endregion
        }

        public override void OnEnter()
        {
            base.OnEnter();

            SetToggleUI();

            if ( DataManager.GetInstance().GetPlayerIsInBattle() )
            {
                SetButtonEnableMark();
            }
        }

        public void SetToggleUI()
        {
            Data.DataManager dataManager = Data.DataManager.GetInstance();

            #region Set Camera View Toggle
            switch ( (CameraViewType)dataManager.camareViewChoose )
            {
                case CameraViewType.High:
                    highViewTg.isOn = true;
                    break;
                case CameraViewType.Middle:
                    middleViewTg.isOn = true;
                    break;
                case CameraViewType.Low:
                    lowViewTg.isOn = true;
                    break;
            }
            #endregion

            //Set Game Quality Toggle
            int qualityValue = dataManager.qualitySettingChoose;
            currentQualityType = qualityValue == 0 ? QualitySettingType.Middle : (QualitySettingType)qualityValue;
            ChangeQualityEvent();

            //Set Music Toggle
            float musicValue = dataManager.musicChoose;
            onMusicToggle.isOn = ( musicValue == 1 );
            offMusicToggle.isOn = ( musicValue == 2 );

            //Set Sound Toggle
            float soundValue = dataManager.soundChoose;
            onSoundToggle.isOn = ( soundValue == 1 );
            offSoundToggle.isOn = ( soundValue == 2 );

            //Set RecordScreen Toggle
            int recordScreenChoose = dataManager.recordScreenChoose;
            onRecordScreenTg.isOn = ( recordScreenChoose == 1 );
            offRecordScreenTg.isOn = ( recordScreenChoose == 2 );

            //Set Damage Toggle
            int damageNumChoose = dataManager.damageNumChoose;
            onDamageNumTg.isOn = ( damageNumChoose == 1 );
            offDamageNumTg.isOn = ( damageNumChoose == 2 );

            //Set Display Bar Toggle
            int displayBarChoose = dataManager.displayBarChoose;
            onDisplayBarTg.isOn = ( displayBarChoose == 1 );
            offDisplayBarTg.isOn = ( displayBarChoose == 2 );

            //Set Battle Mode Toggle
            int unitOperationChoose = dataManager.unitOperationChoose;
            operationOneTg.isOn = ( unitOperationChoose == 1 );
            operationTwoTg.isOn = ( unitOperationChoose == 2 );
            operationThreeTg.isOn = ( unitOperationChoose == 3 );
        }

        #region Button & Toggle Event

        #region Setting UI
        private void OnClickBaseSettingTg( bool isOn )
        {
            baseSettingToggle.interactable = !isOn;
            baseSettingToggleText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            baseSettingTran.gameObject.SetActive( true );
            soundSettingTran.gameObject.SetActive( false );
            operationSettingTran.gameObject.SetActive( false );
        }

        private void OnClickSoundSettingTg( bool isOn )
        {
            soundSettingToggle.interactable = !isOn;
            soundSettingToggleText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            baseSettingTran.gameObject.SetActive( false );
            soundSettingTran.gameObject.SetActive( true );
            operationSettingTran.gameObject.SetActive( false );
        }

        private void OnClickOperationSettingTg( bool isOn )
        {
            if ( DataManager.GetInstance().GetPlayerIsInBattle() )
            {
                return;
            }
            operationSettingToggle.interactable = !isOn;
            operationSettingToggleText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            baseSettingTran.gameObject.SetActive( false );
            soundSettingTran.gameObject.SetActive( false );
            operationSettingTran.gameObject.SetActive( true );
        }

        private void OnClickCloseButton()
        {
            OnExit( false );
        }
        #endregion

        #region BaseSetting UI
        private void OnClickLowViewTg( bool isOn )
        {
            lowViewTg.interactable = !isOn;
            viewLowText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            OnClickViewAngleToggleEvent( CameraViewType.Low );
        }

        private void OnClickMiddleViewTg( bool isOn )
        {
            middleViewTg.interactable = !isOn;
            viewMiddleText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            OnClickViewAngleToggleEvent( CameraViewType.Middle );
        }

        private void OnClickHighViewTg( bool isOn )
        {
            highViewTg.interactable = !isOn;
            viewHightText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            OnClickViewAngleToggleEvent( CameraViewType.High );
        }

        private void OnClickLowQualityTg( bool isOn )
        {
            if ( DataManager.GetInstance().GetPlayerIsInBattle() )
            {
                return;
            }
            if ( !isOn )
                return;

            OnClickSetQualityToggleEvent( QualitySettingType.Low );
        }

        private void OnClickMiddleQualityTg( bool isOn )
        {
            if ( DataManager.GetInstance().GetPlayerIsInBattle() )
            {
                return;
            }
            if ( !isOn )
                return;
            OnClickSetQualityToggleEvent( QualitySettingType.Middle );
        }

        private void OnClickHighQualityTg( bool isOn )
        {
            if ( DataManager.GetInstance().GetPlayerIsInBattle() )
            {
                return;
            }
            if ( !isOn )
                return;
            OnClickSetQualityToggleEvent( QualitySettingType.High );
        }

        private void OnClickOnRecordScreenTg( bool isOn )
        {
            if ( DataManager.GetInstance().GetPlayerIsInBattle() )
            {
                return;
            }
            onRecordScreenTg.interactable = !isOn;
            onRecordScreenText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            OnClickRecordScreenToggleEvent( 1 );
        }

        private void OnClickOffRecordScreenTg( bool isOn )
        {
            if ( DataManager.GetInstance().GetPlayerIsInBattle() )
            {
                return;
            }
            offRecordScreenTg.interactable = !isOn;
            offRecordScreenText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            OnClickRecordScreenToggleEvent( 2 );
        }

        private void OnClickOnDamageNumTg( bool isOn )
        {
            onDamageNumTg.interactable = !isOn;
            onDamageNumText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            OnClickDamageNumToggleEvent( 1 );
        }

        private void OnClickOffDamageNumTg( bool isOn )
        {
            offDamageNumTg.interactable = !isOn;
            offDamageNumText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            OnClickDamageNumToggleEvent( 2 );
        }

        private void OnClickOnDisplayBarTg( bool isOn )
        {
            onDisplayBarTg.interactable = !isOn;
            onDisplayBarText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            OnClickDisplayBarToggleEvent( 1 );
        }

        private void OnClickOffDisplaybarTg( bool isOn )
        {
            offDisplayBarTg.interactable = !isOn;
            offDisplayBarText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            OnClickDisplayBarToggleEvent( 2 );
        }
        #endregion

        #region SoundSetting UI
        private void OnClickOnMusicTg( bool isOn )
        {
            onMusicToggle.interactable = !isOn;
            onMusicTgText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            OnClickMusicEvent( 1 );
        }

        private void OnClickOffMusicTg( bool isOn )
        {
            offMusicToggle.interactable = !isOn;
            offMusicTgText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            OnClickMusicEvent( 2 );
        }

        private void OnClickOnSoundTg( bool isOn )
        {
            onSoundToggle.interactable = !isOn;
            onSoundTgText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            OnClickSoundEvent( 1 );
        }

        private void OnClickOffSoundTg( bool isOn )
        {
            offSoundToggle.interactable = !isOn;
            offSoundTgText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            OnClickSoundEvent( 2 );
        }
        #endregion

        #region OperationSetting UI
        private void OnClickOperationOneTg( bool isOn )
        {
            operationOneTg.interactable = !isOn;
            operationOneTgText.color = isOn ? Color.white : textGray;
            operationOneSelectImage.gameObject.SetActive( isOn );
            operationOneText.color = isOn ? Color.white : operationTextGray;

            if ( isOn )
                OnClickOperationToggleEvent( 1 );
        }

        private void OnClickOperationTwoTg( bool isOn )
        {
            operationTwoTg.interactable = !isOn;
            operationTwoTgText.color = isOn ? Color.white : textGray;
            operationTwoSelectImage.gameObject.SetActive( isOn );
            operationTwoText.color = isOn ? Color.white : operationTextGray;

            if ( isOn )
                OnClickOperationToggleEvent( 2 );
        }

        private void OnClickOperationThreeTg( bool isOn )
        {
            operationThreeTg.interactable = !isOn;
            operationThreeTgText.color = isOn ? Color.white : textGray;
            operationThreeSelectImage.gameObject.SetActive( isOn );
            operationThreeText.color = isOn ? Color.white : operationTextGray;

            if ( isOn )
                OnClickOperationToggleEvent( 3 );
        }
        #endregion

        private void OnClickSoundEvent( int f )//1 - On  2 - Off
        {
            Data.DataManager.GetInstance().soundChoose = f;
            controller.SavePlayerPrefsData();

            MessageDispatcher.PostMessage( MessageType.SoundVolume, f == 2 ? 0f : 1f );
        }

        private void OnClickMusicEvent( int f )//1 - On  2 - Off
        {
            Data.DataManager.GetInstance().musicChoose = f;
            controller.SavePlayerPrefsData();

            MessageDispatcher.PostMessage( MessageType.MusicVolume, f == 2 ? 0f : 1f );
        }

        private void OnLanguageChangeEvent( int type )
        {
            Data.DataManager.GetInstance().languageIndex = type;
            controller.SavePlayerPrefsData();
        }

        private void OnClickViewAngleToggleEvent( CameraViewType type )
        {
            Data.DataManager.GetInstance().camareViewChoose = (int)type;
            controller.SavePlayerPrefsData();
        }

        private void OnClickSetQualityToggleEvent( QualitySettingType type )
        {
            currentQualityType = type;

            string logOutText = "Are you sure change quality? ";
            string titleText = "Prompt";
            System.Action clickLogOut = ChangeQualityEvent;

            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, clickLogOut, AlertType.ConfirmAndCancel, logOutText, titleText );
        }

        private void OnClickRecordScreenToggleEvent( int value )//1 - On , 2 - Off
        {
            Data.DataManager.GetInstance().recordScreenChoose = value;
            controller.SavePlayerPrefsData();
        }

        private void OnClickDamageNumToggleEvent( int value )//1 - On  2 - Off
        {
            Data.DataManager.GetInstance().damageNumChoose = value;
            controller.SavePlayerPrefsData();
        }

        private void OnClickDisplayBarToggleEvent( int value )//1 - On  2 - Off
        {
            Data.DataManager.GetInstance().displayBarChoose = value;
            controller.SavePlayerPrefsData();
        }

        private void OnClickOperationToggleEvent( int value )//0 - OperationOne  1 - OperationTwo  2 - OperationThree
        {
            Data.DataManager.GetInstance().unitOperationChoose = value;
            controller.SavePlayerPrefsData();
        }

        private QualitySettingType currentQualityType;
        private void ChangeQualityEvent()
        {
            Data.DataManager.GetInstance().qualitySettingChoose = (int)currentQualityType;
            controller.SavePlayerPrefsData();

            switch ( currentQualityType )
            {
                case UI.QualitySettingType.High:
                    UnityEngine.QualitySettings.SetQualityLevel( GameConstants.QUALITY_HIGH_VALUE );
                    break;
                case UI.QualitySettingType.Middle:
                    UnityEngine.QualitySettings.SetQualityLevel( GameConstants.QUALITY_MIDDLE_VALUE );
                    break;
                case UI.QualitySettingType.Low:
                    UnityEngine.QualitySettings.SetQualityLevel( GameConstants.QUALITY_LOW_VALUE );
                    break;
            }

            highQualityTg.interactable = !( currentQualityType == QualitySettingType.High );
            middleQualityTg.interactable = !( currentQualityType == QualitySettingType.Middle );
            lowQualityTg.interactable = !( currentQualityType == QualitySettingType.Low );

            qualityHighText.color = ( currentQualityType == QualitySettingType.High ) ? Color.white : myGray;
            qualityMiddleText.color = ( currentQualityType == QualitySettingType.Middle ) ? Color.white : myGray;
            qualityLowText.color = ( currentQualityType == QualitySettingType.Low ) ? Color.white : myGray;
        }

        #endregion

        private void SetButtonEnableMark()
        {
            highQualityTg.interactable = true;
            middleQualityTg.interactable = true;
            lowQualityTg.interactable = true;
            onRecordScreenTg.interactable = true;
            offRecordScreenTg.interactable = true;
        }
    }
}
