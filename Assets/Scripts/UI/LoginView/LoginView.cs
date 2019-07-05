using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using Data;
using Network;
using Resource;
using Utils;

namespace UI
{
    public class LoginView : ViewBase
    {
        private string INVALID_USERID;
        private string INVALID_PASSWORD;
        private string PASSWORDS_DONT_MATCH;
        private string FAILED_LOGIN;

        private GameObject loginWindow, playerChoiceWindow, tutorialWindow;

        #region Login Component
        private GameObject verificationObject, confirmObject, cancelObject, registerObject, loginObject;

        private Text useridText, passwordText, confirmText, cancelText, verificationText, verificationPlaceholder, verificationHeaderText;
        private Text useridPlaceholder, passwordPlaceholder, loginButtonText, registerButtonText, useridHeaderText, passwordHeaderText;
        private Button loginButton, registerButton, cancelButton, confirmButton;

        private InputField userIDInput, passwordInput, verificationInput;

        private bool register;

        private const string LOGIN_PATH = "LoginUI/LoginWindow/";

        public static LoginViewController controller;
        #endregion Login

        #region PlayerChoice Component
        private const int nrOfAvatars = 12;
        private int currChosenAvatar;

        private Text namePlaceholderText, nameText, createButtonText, noTutorialText, yesTutorialText, greyCreateButtonText;
        private InputField inputText;
        private Image[] avatarImages = new Image[nrOfAvatars];
        private Image avatarIcon;
        private Toggle[] avatarToggles = new Toggle[nrOfAvatars];
        private Button randomNameBt, createButton, noTutorialButton, yesTutorialButton;
        private GameObject greyCreateButton;

        private const string PLAYER_CHOICE_PATH = "LoginUI/PlayerChoiceWindow";
        private const string TUTORIAL_PATH = "LoginUI/TutorialWindow";
        private const string FEEDBACK_PATH = "LoginUI/PlayerFeedbackWindow";

        private string[] avatars;
        #endregion PlayerChoice

        string ActiveAvatar
        {
            get
            {
                if ( currChosenAvatar < 0 || currChosenAvatar >= avatars.Length )
                    return string.Empty;
                return avatars[currChosenAvatar];
            }
        }

        private void Start()
        {
            controller.Start();
        }

        public override void OnEnter()
        {
            base.OnEnter();

            //Change Sound Value
			if( controller.datamanager == null )
			{
				controller.datamanager = DataManager.GetInstance();
			}

			controller.datamanager.ReadSettingDataFromPlayerPrefs();

            inputText.text = "";
            /*
            MsgCode serverChannel = MsgCode.LoginMessage;
            LoginC2S loginData = null;
            loginData = new LoginC2S();
            loginData.loginName = "jiawen1";
            loginData.password = "123456";
            loginData.UDID = DeviceUtil.Instance.GetDeviceUniqueIdentifier();
            loginData.MAC = DeviceUtil.Instance.GetDeviceUniqueIdentifier();
            loginData.ip = DeviceUtil.Instance.GetDeviceIP();
            byte[] stream = ProtobufUtils.Serialize( loginData );
            NetworkManager.SendRequest( MsgCode.LoginMessage, stream, () => {
                LoginC2S loginData1 = new LoginC2S();
                loginData1.loginName = "jiawen1";
                loginData1.password = "123456";
                loginData1.UDID = DeviceUtil.Instance.GetDeviceUniqueIdentifier();
                loginData1.MAC = DeviceUtil.Instance.GetDeviceUniqueIdentifier();
                loginData1.ip = DeviceUtil.Instance.GetDeviceIP();
                byte[] stream1 = ProtobufUtils.Serialize( loginData1 );
                NetworkManager.SendRequest( MsgCode.LoginMessage, stream1 );
            } );
            */

        }

        public override void OnInit()
        {
            base.OnInit();

            controller = new LoginViewController( this );
            _controller = controller;

            SoundManager.Instance.PlayMusic( 60001 );

            //#if UNITY_EDITOR
            //DestroyImmediate(transform.Find( "LoginUI" ).gameObject);
            //AttachAssetBundlePrefab( transform, "loginscreen", "LoginUI" );
            //#endif

            avatars = new string[] { "EmberAvatar_40", "EmberAvatar_7", "EmberAvatar_9", "EmberAvatar_10", "EmberAvatar_20", "EmberAvatar_21", "EmberAvatar_24", "EmberAvatar_38", "EmberAvatar_39", "EmberAvatar_6", "EmberAvatar_41", "EmberAvatar_42" };

            Transform t = transform;

            playerChoiceWindow = t.Find( PLAYER_CHOICE_PATH ).gameObject;
            tutorialWindow = t.Find( TUTORIAL_PATH ).gameObject;
            loginWindow = t.Find( LOGIN_PATH ).gameObject;

            #region Components Login
            Transform loginWindowTran = t.Find( LOGIN_PATH + "LoginInfoWindow" );

            verificationObject = loginWindowTran.Find( "InputGroup/Verification" ).gameObject;
            confirmObject = loginWindowTran.Find( "ConfirmButton" ).gameObject;
            cancelObject = loginWindowTran.Find( "CancelButton" ).gameObject;
            registerObject = loginWindowTran.Find( "RegisterButton" ).gameObject;
            loginObject = loginWindowTran.Find( "LoginButton" ).gameObject;

            userIDInput = loginWindowTran.Find( "InputGroup/UserID/UserIDField" ).GetComponent<InputField>();
            passwordInput = loginWindowTran.Find( "InputGroup/Password/PasswordField" ).GetComponent<InputField>();
            verificationInput = loginWindowTran.Find( "InputGroup/Verification/VerificationField" ).GetComponent<InputField>();

            useridText = loginWindowTran.Find( "InputGroup/UserID/UserIDField/UserIDText" ).GetComponent<Text>();
            passwordText = loginWindowTran.Find( "InputGroup/Password/PasswordField/PasswordText" ).GetComponent<Text>();
            verificationText = loginWindowTran.Find( "InputGroup/Verification/VerificationField/VerificationText" ).GetComponent<Text>();

            cancelText = loginWindowTran.Find( "CancelButton/CancelText" ).GetComponent<Text>();
            confirmText = loginWindowTran.Find( "ConfirmButton/ConfirmText" ).GetComponent<Text>();
            verificationPlaceholder = loginWindowTran.Find( "InputGroup/Verification/VerificationField/VerificationPlaceholder" ).GetComponent<Text>();
            verificationHeaderText = loginWindowTran.Find( "InputGroup/Verification/VerificationHeader" ).GetComponent<Text>();

            useridPlaceholder = loginWindowTran.Find( "InputGroup/UserID/UserIDField/UserIDPlaceholder" ).GetComponent<Text>();
            passwordPlaceholder = loginWindowTran.Find( "InputGroup/Password/PasswordField/PasswordPlaceholder" ).GetComponent<Text>();
            loginButtonText = loginWindowTran.Find( "LoginButton/LoginText" ).GetComponent<Text>();
            registerButtonText = loginWindowTran.Find( "RegisterButton/RegisterText" ).GetComponent<Text>();
            useridHeaderText = loginWindowTran.Find( "InputGroup/UserID/UserIDHeader" ).GetComponent<Text>();
            passwordHeaderText = loginWindowTran.Find( "InputGroup/Password/PasswordHeader" ).GetComponent<Text>();

            loginButton = loginWindowTran.Find( "LoginButton" ).GetComponent<Button>();
            registerButton = loginWindowTran.Find( "RegisterButton" ).GetComponent<Button>();

            confirmButton = loginWindowTran.Find( "ConfirmButton" ).GetComponent<Button>();
            cancelButton = loginWindowTran.Find( "CancelButton" ).GetComponent<Button>();
            #endregion Components Login

            SetLoginFromPlayerPrefs();

            #region Components PlayerChoice
            noTutorialText = t.Find( TUTORIAL_PATH + "/NoTutorial/NoTutorialButton/NoTutorialHeader/NoTutorialText" ).GetComponent<Text>();
            yesTutorialText = t.Find( TUTORIAL_PATH + "/YesTutorial/YesTutorialButton/YesTutorialHeader/YesTutorialText" ).GetComponent<Text>();

            noTutorialButton = t.Find( TUTORIAL_PATH + "/NoTutorial/NoTutorialButton" ).GetComponent<Button>();
            yesTutorialButton = t.Find( TUTORIAL_PATH + "/YesTutorial/YesTutorialButton" ).GetComponent<Button>();

            inputText = t.Find( PLAYER_CHOICE_PATH + "/NameWindow/NameInputField" ).GetComponent<InputField>();
            namePlaceholderText = t.Find( PLAYER_CHOICE_PATH + "/NameWindow/NameInputField/NamePlaceholder" ).GetComponent<Text>();
            nameText = t.Find( PLAYER_CHOICE_PATH + "/NameWindow/NameInputField/NameText" ).GetComponent<Text>();
            createButtonText = t.Find( PLAYER_CHOICE_PATH + "/NameWindow/CreateButton/CreateButtonText" ).GetComponent<Text>();
            greyCreateButtonText = t.Find( PLAYER_CHOICE_PATH + "/NameWindow/GreyCreateButton/GreyCreateButtonText" ).GetComponent<Text>();
            avatarIcon = t.Find( PLAYER_CHOICE_PATH + "/NameWindow/AvatarIcon" ).GetComponent<Image>();

            randomNameBt = t.Find( PLAYER_CHOICE_PATH + "/NameWindow/RandomNameButton" ).GetComponent<Button>();

            string s;

            for ( int i = 0; i < nrOfAvatars; i++ )
            {
                s = PLAYER_CHOICE_PATH + "/AvatarWindow/AvatarObj" + ( i + 1 );
                avatarImages[i] = t.Find( s + "/AvatarIcon" ).GetComponent<Image>();
                avatarToggles[i] = t.Find( s ).GetComponent<Toggle>();

                Image playerIcon = avatarImages[i];
                Toggle avatarToggle = avatarToggles[i];
                int index = i;
                avatarToggles[index].AddListener( ( bool isOn ) => { OnClickAvatarToggle( index, playerIcon.sprite, avatarToggle, isOn ); } );
            }

            createButton = t.Find( PLAYER_CHOICE_PATH + "/NameWindow/CreateButton" ).GetComponent<Button>();
            greyCreateButton = t.Find( PLAYER_CHOICE_PATH + "/NameWindow/GreyCreateButton" ).gameObject;

            #endregion Components PlayerChoice

            #region AddListeners
            inputText.onValueChanged.AddListener( NameInputValueChange );

            loginButton.AddListener( OnClickLoginButton, UIEventGroup.Middle, UIEventGroup.Middle );
            registerButton.AddListener( OnClickRegisterButton );
            confirmButton.AddListener( OnClickConfirmButton, UIEventGroup.Middle, UIEventGroup.Middle );
            cancelButton.AddListener( OnClickCancelButton );

            randomNameBt.AddListener( OnClickRandomNameButton );
            createButton.AddListener( OnClickCreateButton );
            noTutorialButton.AddListener( OnClickNoTutorial );
            yesTutorialButton.AddListener( OnClickYesTutorial );
            //_feedbackButton.AddListener( OnClickFeedbackButton );
            #endregion AddListeners

            for ( int i = 0; i < nrOfAvatars; i++ )
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( avatars[i], delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    avatarImages[(int)param].SetSprite( atlasSprite );
                }, i );
            }

            Localization();
            SetPlayerChoiceWindow( false );
            SetTutorialWindow( false );
            SetLoginWindow( true );
            //_feedbackWindow.SetActive( false );
            currChosenAvatar = 0;
        }

		void OnDisable()
		{
			controller.CheckRemoveNoviceGuidanceServerMessageHandler();
		}

        #region Set UI

        private void SetCreateButton( bool state )
        {
            createButton.gameObject.SetActive( state );
            greyCreateButton.SetActive( !state );
        }

        private void SetLoginFromPlayerPrefs()
        {
            string userid = PlayerPrefs.GetString( "userID" );
            string password = PlayerPrefs.GetString( "userPW" );

            if ( !string.IsNullOrEmpty( userid ) && !string.IsNullOrEmpty( password ) )
            {
                userIDInput.text = userid;
                passwordInput.text = password;
            }
        }

        public void SetPlayerChoiceWindow( bool state )
        {
            playerChoiceWindow.SetActive( state );

            if ( state )
            {
                SetTutorialWindow( false );
                SetLoginWindow( false );
            }
        }

        public void SetTutorialWindow( bool state )
        {
            tutorialWindow.SetActive( state );

            if ( state )
            {
                SetPlayerChoiceWindow( false );
                SetLoginWindow( false );
            }
        }

        public void SetLoginWindow( bool state )
        {
            if ( state )
                EnableLogin();

            loginWindow.SetActive( state );

            if ( state )
            {
                SetPlayerChoiceWindow( false );
                SetTutorialWindow( false );
            }
        }

        public void OpenPlayerChoiceWindow()
        {
            avatarToggles[0].isOn = true;

           SetPlayerChoiceWindow( true );
           SetLoginWindow( false );
        }

        void EnableRegister()
        {
            verificationObject.SetActive( true );
            confirmObject.SetActive( true );
            cancelObject.SetActive( true );
            registerObject.SetActive( false );
            loginObject.SetActive( false );
            register = true;
        }

        void EnableLogin()
        {
            verificationObject.SetActive( false );
            confirmObject.SetActive( false );
            cancelObject.SetActive( false );
            registerObject.SetActive( true );
            loginObject.SetActive( true );
            register = false;
        }

        public void ToggleTutorialMode()
        {
            bool tutActive = tutorialWindow.activeInHierarchy;
            tutorialWindow.SetActive( playerChoiceWindow.activeInHierarchy );
            playerChoiceWindow.SetActive( tutActive );
        }

        public void ResetLoginButtons()
        {
            OnClickCancelButton();
        }

        #endregion

        void Localization()
        {
            //cancelText.text = "Cancel";
            //confirmText.text = "Confirm";
            //verificationPlaceholder.text = "Enter text...";
            //verificationHeaderText.text = "Verify password";

            //useridPlaceholder.text = "Enter text...";
            //passwordPlaceholder.text = "Enter text...";
            //loginButtonText.text = "Login";
            //registerButtonText.text = "Register";
            //useridHeaderText.text = "User ID";
            //passwordHeaderText.text = "Password";

            //namePlaceholderText.text = "Enter text...";
            //greyCreateButtonText.text = createButtonText.text = "Create";

            //yesTutorialText.text = "Start Tutorial";
            //noTutorialText.text = "Skip Tutorial";

            INVALID_USERID = "无效的用户名";
            INVALID_PASSWORD = "密码错误";
            PASSWORDS_DONT_MATCH = "密码不一致";
            FAILED_LOGIN = "登录失败";
        }

        #region Open Alert

        private void HandleInvalidUserID()
        {
            ActivateFeedback( INVALID_USERID );
        }

        private void HandleInvalidPassword( string error )
        {
            ActivateFeedback( error );
        }

        private void HandleNotMatchingPasswords()
        {
            ActivateFeedback( PASSWORDS_DONT_MATCH );
        }

        public void HandleLoginGameServerFailure( string error )
        {
            ActivateFeedback( INVALID_PASSWORD );
        }

        public void FailedLogin( Data.TipType error )
        {
            Utils.DebugUtils.Log( Utils.DebugUtils.Type.Login, FAILED_LOGIN );
            ActivateFeedback( error.ToString() );
            ResetLoginButtons();
        }

        public void FailedRegister( Data.TipType error )
        {
            Utils.DebugUtils.Log( Utils.DebugUtils.Type.Login, error.ToString() );
            ActivateFeedback( error.ToString() );
        }

        public void FailedRegisterChoice( Data.TipType error )
        {
            Utils.DebugUtils.Log( Utils.DebugUtils.Type.Login, error.ToString() );
            ActivateFeedback( error.ToString() );
        }

        private void ActivateFeedback( string feedback )
        {
            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, feedback, "提示" );
        }

        #endregion

        private bool CheckInput( string userID, string password, string verification = "" )
        {
            if ( !InputUtil.IsValidInput( userID ) )
            {
                HandleInvalidUserID();
                return false;
            }

            if ( register && ( string.IsNullOrEmpty( verification ) || !password.Equals( verification ) ) )
            {
                HandleNotMatchingPasswords();
                return false;
            }

            string passwordError;
            if ( !Utils.InputUtil.IsValidPassword( password, userID, out passwordError ) )
            {
                if ( string.IsNullOrEmpty( passwordError ) )
                    passwordError = "密码应大于6个字符";

                HandleInvalidPassword( passwordError );
                return false;
            }
            return true;
        }

        private void ClearInput()
        {
            userIDInput.Select();
            userIDInput.text = string.Empty;
            verificationInput.text = string.Empty;
            passwordInput.text = string.Empty;
        }

        #region InputFieldValueChange & ButtonEvent

        private void NameInputValueChange( string input )
        {
            bool nameApproved = InputUtil.IsValidInput( input );

            SetCreateButton( nameApproved );
        }

        private void OnClickLoginButton()
        {
            if ( !CheckInput( useridText.text, passwordInput.text ) )
                return;

            controller.SendCredentials( LoginType.Login, useridText.text, passwordInput.text );
        }

        private void OnClickRegisterButton()
        {
            if ( register )
                return;

            ClearInput();
            EnableRegister();

            avatarToggles[0].isOn = true;
        }

        private void OnClickConfirmButton()
        {
            if ( !CheckInput( useridText.text, passwordInput.text, verificationInput.text ) )
                return;

            controller.SendCredentials( LoginType.Register, useridText.text, passwordInput.text );
        }

        private void OnClickCancelButton()
        {
            ClearInput();
            SetLoginFromPlayerPrefs();
            EnableLogin();
        }

        private void OnClickRandomNameButton()
        {
            inputText.text = controller.GetRandomName();
        }

        private void OnClickCreateButton()
        {
            if ( !InputUtil.IsValidInput( nameText.text ) || currChosenAvatar < 0 )
                return;
            controller.SendRegisterChoice( nameText.text, ActiveAvatar );
        }

        private void OnClickAvatarToggle( int index, Sprite sprite, Toggle toggle, bool isOn )
        {
            toggle.interactable = !isOn;

            if ( isOn )
            {
                currChosenAvatar = index;

                avatarIcon.sprite = sprite;
            }
        }

        private void OnClickNoTutorial()
        {
			PlayerNoviceGuidanceData guideData = new PlayerNoviceGuidanceData();
			guideData.SetBasicOperation( 0 );
			guideData.SetBuildTraining( 0 );
			guideData.SetIsSkipGuide( 1 );
			guideData.SetNpcTraining( 0 );
			guideData.SetSkillTraining( 0 );
			guideData.SetTrainingMode( 0 );

			controller.datamanager.SetPlayerNoviceGuidanceData( guideData );
			controller.SendSkipTutorialMessage();
        }

        private void OnClickYesTutorial()
        {
			//This is old Training mode interface. Dwayne.
            //controller.datamanager.SetBattleType( BattleType.Tranining, false );

			#region there will set tutorial battle type

			controller.datamanager.SetBattleType( BattleType.Tutorial, false );

			#endregion

			controller.SwtichScene();
        }

        #endregion ButtonEvent
    }
}