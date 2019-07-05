using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using Resource;

namespace UI
{
    public class MainTopBarView : ViewBase
    {
        private Button buyButton, playerInfoButton;
        private Image playerImage, wifiImage, batteryValueImage;
        private Text emberText, goldText, diamondText, playerNameText, playerLevelText, xPText;
        private Slider xPSlider;

        #region Player HeadIcon Image Path
        //private const string PLAYER_ICON_PATH = "UITexture/Avatar_icon/";
        #endregion

        private MainTopBarController controller;

        public override void OnInit()
        {
            base.OnInit();

            controller = new MainTopBarController( this );
            _controller = controller;

            playerImage = transform.Find( "PlayerImage" ).GetComponent<Image>();
            wifiImage = transform.Find( "WIFIImage" ).GetComponent<Image>();
            batteryValueImage = transform.Find( "BatteryObj/BatteryValueImage" ).GetComponent<Image>();
            emberText = transform.Find( "EmberText" ).GetComponent<Text>();
            diamondText = transform.Find( "DiamondText" ).GetComponent<Text>();
            goldText = transform.Find( "GoldText" ).GetComponent<Text>();
            playerNameText = transform.Find( "PlayerNameText" ).GetComponent<Text>();
            playerLevelText = transform.Find( "PlayerLevelText" ).GetComponent<Text>();
            xPText = transform.Find( "XPText" ).GetComponent<Text>();

            buyButton = transform.Find( "BuyEmberButton" ).GetComponent<Button>();
            playerInfoButton = transform.Find( "PlayerInfoButton" ).GetComponent<Button>();
            xPSlider = transform.Find( "LevelUpBar/SliderMask/LevelUpBarSlider" ).GetComponent<Slider>();

            buyButton.AddListener( OnClickBuyButto );
            playerInfoButton.AddListener( OnClickPlayerInfoButton );
        }

        public override void OnEnter()
        {
            base.OnEnter();

            RefreshMainTopBar();

            SoundManager.Instance.PlayMusic( 60002 );

            //TODO:  It is now only Android side execution
            //Refresh Battery 
#if UNITY_ANDROID
            StartCoroutine( "RefreshBatteryLevel" );
#endif
        }

        #region Button Event

        private void OnClickBuyButto()
        {

        }

        private void OnClickPlayerInfoButton()
        {
            UIManager.Instance.GetUIByType( UIType.PlayerInfoUI, ( ViewBase ui, System.Object param ) => {
                ui.OnEnter();
                ( ui as PlayerInfoView ).openBattleResult = false;
            } );
        }

        #endregion

        public override void OnDestroy()
        {
            // because top bar view will destroyed with MainScene
            base.OnDestroy();
        }

        #region Refresh UI

        IEnumerator RefreshBatteryLevel()
        {
            while ( true )
            {
                batteryValueImage.fillAmount = controller.GetAndroidBatteryValue();

                yield return new WaitForSeconds( 10f );
            }
        }

        public void RefreshMainTopBar()
        {
            RefreshPlayerName( controller.GetPlayerName() );
            RefreshPlayerXP();
            RefreshEmber();
            RefreshGold();
            RefreshDiamond();

            RefreshPlayerHeadIcon();
        }

        public void RefreshPlayerName( string name )
        {
            playerNameText.text = name;
        }

        public void RefreshPlayerXP()
        {
            playerLevelText.text = "LV." + controller.GetPlayerLevel();
            xPText.text = controller.GetXPText();
            xPSlider.value = controller.GetXPBarValue();
        }

        public void RefreshGold()
        {
            goldText.text = controller.GetGoldCount();
        }

        public void RefreshEmber()
        {
            emberText.text = controller.GetEmberCount();
        }

        public void RefreshDiamond()
        {
            diamondText.text = controller.GetDiamondCount();
        }

        public void RefreshPlayerHeadIcon()
        {
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( controller.GetPlayerHeadIcon(), delegate ( string name, AtlasSprite atlasSprite, System.Object param )
            {
                playerImage.SetSprite( atlasSprite );
            }, true );
        }

        #endregion

    }
}
