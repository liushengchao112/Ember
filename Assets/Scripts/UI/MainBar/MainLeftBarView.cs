using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace UI
{
    public class MainLeftBarView : ViewBase
    {
        private ToggleGroup toggleGroup;
        private Image unitIcon, runeIcon, armyManageIcon, playerBagIcon, mainMenuIcon, rankIcon, storeIcon, unitGrayIcon, runeGrayIcon, armyManageGrayIcon, playerBagGrayIcon, mainMenuGrayIcon, rankGrayIcon, storeGrayIcon, unitRedBubble, runeRedBubble, armyManageRedBubble, playerBagRedBubble, mainMenuRedBubble, rankRedBubble, storeRedBubble;
        private Toggle unitToggle, runeToggle, armyManageToggle, playerBagToggle, mainMenuToggle, rankToggle, storeToggle;

        private Color myGray = new Color( 150 / (float)255, 150 / (float)255, 150 / (float)255, 1 );

        public override void OnInit()
        {
            base.OnInit();

            _controller = new MainLeftBarController( this );
            _controller.OnCreate();

            Transform tran = transform.Find( "ToggleGroup" );
            toggleGroup = tran.GetComponent<ToggleGroup>();

            unitIcon = tran.Find( "UnitToggle/UnitImage" ).GetComponent<Image>();
            runeIcon = tran.Find( "RuneToggle/RuneImage" ).GetComponent<Image>();
            armyManageIcon = tran.Find( "ArmyManageToggle/ArmyManageImage" ).GetComponent<Image>();
            playerBagIcon = tran.Find( "PlayerBagToggle/PlayerBagImage" ).GetComponent<Image>();
            mainMenuIcon = tran.Find( "MainMenuToggle/MainMenuImage" ).GetComponent<Image>();
            rankIcon = tran.Find( "RankToggle/RankImage" ).GetComponent<Image>();
            storeIcon = tran.Find( "StoreToggle/StoreImage" ).GetComponent<Image>();
            unitGrayIcon = tran.Find( "UnitToggle/UnitGrayImage" ).GetComponent<Image>();
            runeGrayIcon = tran.Find( "RuneToggle/RuneGrayImage" ).GetComponent<Image>();
            armyManageGrayIcon = tran.Find( "ArmyManageToggle/ArmyManageGrayImage" ).GetComponent<Image>();
            playerBagGrayIcon = tran.Find( "PlayerBagToggle/PlayerBagGrayImage" ).GetComponent<Image>();
            mainMenuGrayIcon = tran.Find( "MainMenuToggle/MainMenuGrayImage" ).GetComponent<Image>();
            rankGrayIcon = tran.Find( "RankToggle/RankGrayImage" ).GetComponent<Image>();
            storeGrayIcon = tran.Find( "StoreToggle/StoreGrayImage" ).GetComponent<Image>();
            unitRedBubble = tran.Find( "UnitToggle/RedBubble" ).GetComponent<Image>();
            runeRedBubble = tran.Find( "RuneToggle/RedBubble" ).GetComponent<Image>();
            armyManageRedBubble = tran.Find( "ArmyManageToggle/RedBubble" ).GetComponent<Image>();
            playerBagRedBubble = tran.Find( "PlayerBagToggle/RedBubble" ).GetComponent<Image>();
            mainMenuRedBubble = tran.Find( "MainMenuToggle/RedBubble" ).GetComponent<Image>();
            rankRedBubble = tran.Find( "RankToggle/RedBubble" ).GetComponent<Image>();
            storeRedBubble = tran.Find( "StoreToggle/RedBubble" ).GetComponent<Image>();

            unitToggle = tran.Find( "UnitToggle" ).GetComponent<Toggle>();
            runeToggle = tran.Find( "RuneToggle" ).GetComponent<Toggle>();
            armyManageToggle = tran.Find( "ArmyManageToggle" ).GetComponent<Toggle>();
            playerBagToggle = tran.Find( "PlayerBagToggle" ).GetComponent<Toggle>();
            mainMenuToggle = tran.Find( "MainMenuToggle" ).GetComponent<Toggle>();
            rankToggle = tran.Find( "RankToggle" ).GetComponent<Toggle>();
            storeToggle = tran.Find( "StoreToggle" ).GetComponent<Toggle>();

            unitToggle.AddListener( OnClickUnitToggle );
            runeToggle.AddListener( OnClickRuneToggle );
            armyManageToggle.AddListener( OnClickArmyManageToggle );
            playerBagToggle.AddListener( OnClickPlayerBagToggle );
            mainMenuToggle.AddListener( OnClickMainMenu );
            rankToggle.AddListener( OnClickRank );
            storeToggle.AddListener( OnClickStore );
        }

        #region ButtonEvent

        private void OnClickUnitToggle( bool isOn )
        {
            unitIcon.gameObject.SetActive( isOn );
            unitGrayIcon.gameObject.SetActive( !isOn );

            unitToggle.interactable = !isOn;
            if ( !isOn )
                return;

            UIManager.Instance.GetUIByType( UIType.UnitMainUI, EnterUI );
        }

        private void OnClickRuneToggle( bool isOn )
        {
            runeIcon.gameObject.SetActive( isOn );
            runeGrayIcon.gameObject.SetActive( !isOn );

            runeToggle.interactable = !isOn;
            if ( !isOn )
                return;

            UIManager.Instance.GetUIByType( UIType.RuneMainUI, EnterUI );
        }

        private void OnClickArmyManageToggle( bool isOn )
        {
            armyManageIcon.gameObject.SetActive( isOn );
            armyManageGrayIcon.gameObject.SetActive( !isOn );

            armyManageToggle.interactable = !isOn;
            if ( !isOn )
                return;

            UIManager.Instance.GetUIByType( UIType.ArmyManagementScreen, EnterUI );
        }

        private void OnClickPlayerBagToggle( bool isOn )
        {
            playerBagIcon.gameObject.SetActive( isOn );
            playerBagGrayIcon.gameObject.SetActive( !isOn );

            playerBagToggle.interactable = !isOn;
            if ( !isOn )
                return;

            UIManager.Instance.GetUIByType( UIType.PlayerBagView, EnterUI );
        }

        private void OnClickMainMenu( bool isOn )
        {
            mainMenuIcon.gameObject.SetActive( isOn );
            mainMenuGrayIcon.gameObject.SetActive( !isOn );

            mainMenuToggle.interactable = !isOn;
            if ( !isOn )
                return;

            UIManager.Instance.GetUIByType( UIType.MainMenu, EnterUI );
        }

        private void OnClickRank( bool isOn )
        {
            rankIcon.gameObject.SetActive( isOn );
            rankGrayIcon.gameObject.SetActive( !isOn );

            rankToggle.interactable = !isOn;
            if ( !isOn )
                return;

            UIManager.Instance.GetUIByType( UIType.RankView, EnterUI );
        }

        private void OnClickStore( bool isOn )
        {
            storeIcon.gameObject.SetActive( isOn );
            storeGrayIcon.gameObject.SetActive( !isOn );

            storeToggle.interactable = !isOn;
            if ( !isOn )
                return;

            UIManager.Instance.GetUIByType( UIType.StoreScreen, EnterUI );
        }

        private void EnterUI( ViewBase ui, System.Object param )
        {
            if ( ui != null )
            {
                if ( !ui.openState )
                {
                    ui.OnEnter();
                }
            }
        }

        #endregion

        public void SetLeftBarNoClick()
        {
            toggleGroup.allowSwitchOff = true;
            unitToggle.interactable = runeToggle.interactable = armyManageToggle.interactable = playerBagToggle.interactable = mainMenuToggle.interactable = rankToggle.interactable = storeToggle.interactable = true;
            unitToggle.isOn = runeToggle.isOn = armyManageToggle.isOn = mainMenuToggle.isOn = playerBagToggle.isOn = rankToggle.isOn = storeToggle.isOn = false;
            toggleGroup.allowSwitchOff = false;

            unitGrayIcon.gameObject.SetActive( true );
            runeGrayIcon.gameObject.SetActive( true );
            armyManageGrayIcon.gameObject.SetActive( true );
            mainMenuGrayIcon.gameObject.SetActive( true );
            playerBagGrayIcon.gameObject.SetActive( true );
            rankGrayIcon.gameObject.SetActive( true );
            storeGrayIcon.gameObject.SetActive( true );

            unitIcon.gameObject.SetActive( false );
            runeIcon.gameObject.SetActive( false );
            armyManageIcon.gameObject.SetActive( false );
            mainMenuIcon.gameObject.SetActive( false );
            playerBagIcon.gameObject.SetActive( false );
            rankIcon.gameObject.SetActive( false );
            storeIcon.gameObject.SetActive( false );
        }

        public void SetRuneClick()
        {
            runeToggle.isOn = true;
        }

        public void SetStoreClick()
        {
            storeToggle.isOn = true;
        }
    }
}
