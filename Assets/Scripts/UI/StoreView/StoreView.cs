using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Resource;
using Utils;

namespace UI
{
    public class StoreView : ViewBase
    {
        private const float dragTime = 1f;
        private const float carouselMoveTime = 5f;
        private const int carouseImageCount = 3;

        #region Component
        //Store UI
        private Transform storeTran;
        private Toggle recommendToggle, unitToggle, skinToggle, boxToggle, runeToggle, debrisToggle;
        private Text recommendText, unitText, skinText, boxText, runeText, debrisText;

        private ScrollRect dragStoreItemPanel;
        private GridLayoutGroup storeItemGroup;

        //Box UI
        private Transform boxTran;
        private ScrollRect dragBoxPanel;
        private GridLayoutGroup boxItemGroup;

        //Recommend UI
        private Transform recommendTran;
        private Transform posLeft, posMiddle, posRight;
        private Image[] caroudeImages = new Image[carouseImageCount];
        private Image[] textureIndexImages = new Image[carouseImageCount];
        private GridLayoutGroup goodsGroup;
        private GridLayoutGroup sellingsGroup;
        private Text newGoodsText, sellingText;

        //Unit UI
        private Transform unitTran;
        private ScrollRect dragUnitPanel;
        private GridLayoutGroup unitGroup;
        private Toggle warriorTg_u, assassinTg_u, shooterTg_u, masterTg_u, auxiliaryTg_u;
        private Text warriorTgText_u, assassinTgText_u, shooterTgText_u, masterTgText_u, auxiliaryTgText_u;

        //Skin UI
        private Transform skinTran;
        private ScrollRect dragSkinPanel;
        private GridLayoutGroup skinGroup;
        private Toggle warriorTg_s, assassinTg_s, shooterTg_s, masterTg_s, auxiliaryTg_s;
        private Text warriorTgText_s, assassinTgText_s, shooterTgText_s, masterTgText_s, auxiliaryTgText_s;

        //Rune UI 
        private Transform runeTran;
        private ScrollRect dragRunePanel;
        private GridLayoutGroup runeGroup;
        private Toggle physicalTg_r, magicTg_r, critTg_r, atkSpeedTg_r, moveSpeedTg_r, lv1Tg_r, lv2Tg_r, lv3Tg_r, lv4Tg_r, lv5Tg_r, lv6Tg_r;
        private Text physicalTgText_r, magicTgText_r, critTgText_r, atkSpeedTgText_r, moveSpeedTgText_r, lv1TgText, lv2TgText, lv3TgText, lv4TgText, lv5TgText, lv6TgText;

        //Debris UI
        private Transform debrisTran;
        private ScrollRect dragDebrisPanel;
        private GridLayoutGroup debrisGroup;
        private Image debrisSkinIcon, debrisUnitIcon, debrisRuneIcon;
        private Toggle unitTg_d, skinTg_d, runeTg_d;
        private Text unitTgText_d, skinTgText_d, runeTgText_d, debrisText_d;

        #endregion

        private StoreController controller;

        private StoreCostCurrecyType currentCostType = StoreCostCurrecyType.None;
        public int currentItemType;
        private int currentCarouselIndex = 0;
		private long currentFriendId;

        private GameObject currentUI;

        private List<StoreItem> store_Items = new List<StoreItem>();
        private List<StoreRuneItem> rune_Items = new List<StoreRuneItem>();
        private List<SellingItem> selling_Items = new List<SellingItem>();

        private DebrisItemScrollView debrisItemScrollView;

        private bool isFristOpen = true;
        private Color myGray = new Color( 150 / (float)255, 150 / (float)255, 150 / (float)255, 1 );

        #region Path
        //private const string STORE_ITEM_PATH = "Prefabs/UI/StoreScreenItem/Store_Item";
        //private const string STORE_RUNE_ITEM_PATH = "Prefabs/UI/StoreScreenItem/Rune_Item";
        //private const string STORE_SELLING_ITEM_PATH = "Prefabs/UI/StoreScreenItem/Selling_Item";
        #endregion

        public override void OnInit()
        {
            base.OnInit();
            controller = new StoreController( this );
            _controller = controller;

            #region Store UI
            storeTran = transform.Find( "StoreUI" );
            recommendText = storeTran.Find( "TypeTextGroup/ReCommendText" ).GetComponent<Text>();
            unitText = storeTran.Find( "TypeTextGroup/UnitText" ).GetComponent<Text>();
            skinText = storeTran.Find( "TypeTextGroup/SkinText" ).GetComponent<Text>();
            boxText = storeTran.Find( "TypeTextGroup/BoxText" ).GetComponent<Text>();
            runeText = storeTran.Find( "TypeTextGroup/RuneText" ).GetComponent<Text>();
            debrisText = storeTran.Find( "TypeTextGroup/DebrisText" ).GetComponent<Text>();

            recommendToggle = storeTran.Find( "GoodsTypeToggleGroup/ReCommendToggle" ).GetComponent<Toggle>();
            unitToggle = storeTran.Find( "GoodsTypeToggleGroup/UnitToggle" ).GetComponent<Toggle>();
            skinToggle = storeTran.Find( "GoodsTypeToggleGroup/SkinToggle" ).GetComponent<Toggle>();
            boxToggle = storeTran.Find( "GoodsTypeToggleGroup/BoxToggle" ).GetComponent<Toggle>();
            runeToggle = storeTran.Find( "GoodsTypeToggleGroup/RuneToggle" ).GetComponent<Toggle>();
            debrisToggle = storeTran.Find( "GoodsTypeToggleGroup/DebrisToggle" ).GetComponent<Toggle>();

            recommendToggle.AddListener( OnClickReCommendTg );
            unitToggle.AddListener( OnClickUnitTg );
            skinToggle.AddListener( OnClickSkinTg );
            boxToggle.AddListener( OnClickBoxTg );
            runeToggle.AddListener( OnClickRuneTg );
            debrisToggle.AddListener( OnClickDebrisTg );
            #endregion

            #region Box UI
            boxTran = transform.Find( "BoxUI" );
            dragBoxPanel = boxTran.Find( "DragBoxPanel" ).GetComponent<ScrollRect>();
            boxItemGroup = boxTran.Find( "DragBoxPanel/ItemGroup" ).GetComponent<GridLayoutGroup>();
            #endregion

            #region Recommend UI
            recommendTran = transform.Find( "RecommendUI" );
            posLeft = recommendTran.Find( "CarouselTexture/PosLeft" );
            posMiddle = recommendTran.Find( "CarouselTexture/PosMiddle" );
            posRight = recommendTran.Find( "CarouselTexture/PosRight" );
            goodsGroup = recommendTran.Find( "DragNewGoodsPanel/GoodsItemGroup" ).GetComponent<GridLayoutGroup>();
            sellingsGroup = recommendTran.Find( "DragSellingPanel/SellingItemGroup" ).GetComponent<GridLayoutGroup>();
            caroudeImages[0] = recommendTran.Find( "CarouselTexture/Image1" ).GetComponent<Image>();
            caroudeImages[1] = recommendTran.Find( "CarouselTexture/Image2" ).GetComponent<Image>();
            caroudeImages[2] = recommendTran.Find( "CarouselTexture/Image3" ).GetComponent<Image>();
            textureIndexImages[0] = recommendTran.Find( "TextureIndexObjGroup/IndexObj1/SelectImage" ).GetComponent<Image>();
            textureIndexImages[1] = recommendTran.Find( "TextureIndexObjGroup/IndexObj2/SelectImage" ).GetComponent<Image>();
            textureIndexImages[2] = recommendTran.Find( "TextureIndexObjGroup/IndexObj3/SelectImage" ).GetComponent<Image>();
            newGoodsText = recommendTran.Find( "NewGoodsText" ).GetComponent<Text>();
            sellingText = recommendTran.Find( "SellingText" ).GetComponent<Text>();

            ClickHandler.Get( caroudeImages[0].gameObject ).onDrag = DragImage;
            ClickHandler.Get( caroudeImages[1].gameObject ).onDrag = DragImage;
            ClickHandler.Get( caroudeImages[2].gameObject ).onDrag = DragImage;
            #endregion

            #region Unit UI
            unitTran = transform.Find( "UnitUI" );
            dragUnitPanel = unitTran.Find( "DragUnitPanel" ).GetComponent<ScrollRect>();
            unitGroup = unitTran.Find( "DragUnitPanel/UnitItemGroup" ).GetComponent<GridLayoutGroup>();
            Transform unitTg = unitTran.Find( "ToggleGroup" );
            warriorTg_u = unitTg.Find( "WarriorToggle" ).GetComponent<Toggle>();
            assassinTg_u = unitTg.Find( "AssassinToggle" ).GetComponent<Toggle>();
            shooterTg_u = unitTg.Find( "ShooterToggle" ).GetComponent<Toggle>();
            masterTg_u = unitTg.Find( "MasterToggle" ).GetComponent<Toggle>();
            auxiliaryTg_u = unitTg.Find( "AuxiliaryToggle" ).GetComponent<Toggle>();
            warriorTgText_u = unitTg.Find( "WarriorToggle/TypeText" ).GetComponent<Text>();
            assassinTgText_u = unitTg.Find( "AssassinToggle/TypeText" ).GetComponent<Text>();
            shooterTgText_u = unitTg.Find( "ShooterToggle/TypeText" ).GetComponent<Text>();
            masterTgText_u = unitTg.Find( "MasterToggle/TypeText" ).GetComponent<Text>();
            auxiliaryTgText_u = unitTg.Find( "AuxiliaryToggle/TypeText" ).GetComponent<Text>();

            warriorTg_u.AddListener( OnClickWarriorTg_U );
            assassinTg_u.AddListener( OnClickAssassinTg_U );
            shooterTg_u.AddListener( OnClickShooterTg_U );
            masterTg_u.AddListener( OnClickMasterTg_U );
            auxiliaryTg_u.AddListener( OnClickAuxiliaryTg_U );
            #endregion

            #region Skin UI
            skinTran = transform.Find( "SkinUI" );
            dragSkinPanel = skinTran.Find( "DragSkinPanel" ).GetComponent<ScrollRect>();
            skinGroup = skinTran.Find( "DragSkinPanel/SkinItemGroup" ).GetComponent<GridLayoutGroup>();
            Transform tgSTran = skinTran.Find( "ToggleGroup" );
            warriorTg_s = tgSTran.Find( "WarriorToggle" ).GetComponent<Toggle>();
            assassinTg_s = tgSTran.Find( "AssassinToggle" ).GetComponent<Toggle>();
            shooterTg_s = tgSTran.Find( "ShooterToggle" ).GetComponent<Toggle>();
            masterTg_s = tgSTran.Find( "MasterToggle" ).GetComponent<Toggle>();
            auxiliaryTg_s = tgSTran.Find( "AuxiliaryToggle" ).GetComponent<Toggle>();
            warriorTgText_s = tgSTran.Find( "WarriorToggle/TypeText" ).GetComponent<Text>();
            assassinTgText_s = tgSTran.Find( "AssassinToggle/TypeText" ).GetComponent<Text>();
            shooterTgText_s = tgSTran.Find( "ShooterToggle/TypeText" ).GetComponent<Text>();
            masterTgText_s = tgSTran.Find( "MasterToggle/TypeText" ).GetComponent<Text>();
            auxiliaryTgText_s = tgSTran.Find( "AuxiliaryToggle/TypeText" ).GetComponent<Text>();

            warriorTg_s.AddListener( OnClickWarriorTg_S );
            assassinTg_s.AddListener( OnClickAssassinTg_S );
            shooterTg_s.AddListener( OnClickShooterTg_S );
            masterTg_s.AddListener( OnClickMasterTg_S );
            auxiliaryTg_s.AddListener( OnClickAuxiliaryTg_S );
            #endregion

            #region Rune UI
            runeTran = transform.Find( "RuneUI" );
            dragRunePanel = runeTran.Find( "DragRunePanel" ).GetComponent<ScrollRect>();
            runeGroup = runeTran.Find( "DragRunePanel/RuneItemGroup" ).GetComponent<GridLayoutGroup>();

            Transform tgRTran = runeTran.Find( "TypeToggleGroup" );
            physicalTg_r = tgRTran.Find( "PhysicalToggle" ).GetComponent<Toggle>();
            magicTg_r = tgRTran.Find( "MagicToggle" ).GetComponent<Toggle>();
            critTg_r = tgRTran.Find( "CritToggle" ).GetComponent<Toggle>();
            atkSpeedTg_r = tgRTran.Find( "AtkSpeedToggle" ).GetComponent<Toggle>();
            moveSpeedTg_r = tgRTran.Find( "MoveSpeedToggle" ).GetComponent<Toggle>();
            physicalTgText_r = tgRTran.Find( "PhysicalToggle/TypeText" ).GetComponent<Text>();
            magicTgText_r = tgRTran.Find( "MagicToggle/TypeText" ).GetComponent<Text>();
            critTgText_r = tgRTran.Find( "CritToggle/TypeText" ).GetComponent<Text>();
            atkSpeedTgText_r = tgRTran.Find( "AtkSpeedToggle/TypeText" ).GetComponent<Text>();
            moveSpeedTgText_r = tgRTran.Find( "MoveSpeedToggle/TypeText" ).GetComponent<Text>();

            lv1Tg_r = runeTran.Find( "ToggleGroup/Level1Toggle" ).GetComponent<Toggle>();
            lv2Tg_r = runeTran.Find( "ToggleGroup/Level2Toggle" ).GetComponent<Toggle>();
            lv3Tg_r = runeTran.Find( "ToggleGroup/Level3Toggle" ).GetComponent<Toggle>();
            lv4Tg_r = runeTran.Find( "ToggleGroup/Level4Toggle" ).GetComponent<Toggle>();
            lv5Tg_r = runeTran.Find( "ToggleGroup/Level5Toggle" ).GetComponent<Toggle>();
            lv6Tg_r = runeTran.Find( "ToggleGroup/Level6Toggle" ).GetComponent<Toggle>();

            lv1TgText = runeTran.Find( "ToggleGroup/Level1Toggle/ToggleText" ).GetComponent<Text>();
            lv2TgText = runeTran.Find( "ToggleGroup/Level2Toggle/ToggleText" ).GetComponent<Text>();
            lv3TgText = runeTran.Find( "ToggleGroup/Level3Toggle/ToggleText" ).GetComponent<Text>();
            lv4TgText = runeTran.Find( "ToggleGroup/Level4Toggle/ToggleText" ).GetComponent<Text>();
            lv5TgText = runeTran.Find( "ToggleGroup/Level5Toggle/ToggleText" ).GetComponent<Text>();
            lv6TgText = runeTran.Find( "ToggleGroup/Level6Toggle/ToggleText" ).GetComponent<Text>();

            physicalTg_r.AddListener( OnClickPhysicalTg_R );
            magicTg_r.AddListener( OnClickMagicTg_R );
            critTg_r.AddListener( OnClickCritTg_R );
            atkSpeedTg_r.AddListener( OnClickAtkSpeedTg_R );
            moveSpeedTg_r.AddListener( OnClickMoveSpeedTg_R );
            lv1Tg_r.AddListener( OnClickLV1Tg_R );
            lv2Tg_r.AddListener( OnClickLV2Tg_R );
            lv3Tg_r.AddListener( OnClickLV3Tg_R );
            lv4Tg_r.AddListener( OnClickLV4Tg_R );
            lv5Tg_r.AddListener( OnClickLV5Tg_R );
            lv6Tg_r.AddListener( OnClickLV6Tg_R );
            #endregion

            #region Debris UI
            debrisTran = transform.Find( "DebrisUI" );
            dragDebrisPanel = debrisTran.Find( "DragDebrisPanel" ).GetComponent<ScrollRect>();
            debrisGroup = debrisTran.Find( "DragDebrisPanel/DebrisItemGroup" ).GetComponent<GridLayoutGroup>();
            debrisSkinIcon = debrisTran.Find( "DebrisSkinIcon" ).GetComponent<Image>();
            debrisUnitIcon = debrisTran.Find( "DebrisUnitIcon" ).GetComponent<Image>();
            debrisRuneIcon = debrisTran.Find( "DebrisRuneIcon" ).GetComponent<Image>();
            debrisRuneIcon.gameObject.SetActive( false );
            debrisSkinIcon.gameObject.SetActive( false );
            Transform tgDTran = debrisTran.Find( "ToggleGroup" );
            unitTg_d = tgDTran.Find( "UnitToggle" ).GetComponent<Toggle>();
            skinTg_d = tgDTran.Find( "SkinToggle" ).GetComponent<Toggle>();
            runeTg_d = tgDTran.Find( "RuneToggle" ).GetComponent<Toggle>();
            unitTgText_d = tgDTran.Find( "UnitToggle/TypeText" ).GetComponent<Text>();
            skinTgText_d = tgDTran.Find( "SkinToggle/TypeText" ).GetComponent<Text>();
            runeTgText_d = tgDTran.Find( "RuneToggle/TypeText" ).GetComponent<Text>();
            debrisText_d = debrisTran.Find( "DebrisText" ).GetComponent<Text>();

            if ( dragDebrisPanel.GetComponent<DebrisItemScrollView>() == null )
                debrisItemScrollView = dragDebrisPanel.gameObject.AddComponent<DebrisItemScrollView>();
            else
                debrisItemScrollView = dragDebrisPanel.GetComponent<DebrisItemScrollView>();
            debrisItemScrollView.onCreateItemHandler = OnCreateDebrisItem;

            unitTg_d.AddListener( OnClickUnitTg_D );
            skinTg_d.AddListener( OnClickSkinTg_D );
            runeTg_d.AddListener( OnClickRuneTg_D );
            #endregion

            currentUI = recommendTran.gameObject;

            InitDebrisItem();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if ( isFristOpen )
            {
                recommendToggle.isOn = true;
                isFristOpen = false;
            }

			currentFriendId = 0;
        }

		public void OpenStore( long friendId )
		{
			currentFriendId = friendId;
		}

        #region Button & Toggle Event

        #region Store UI

        private void OnClickReCommendTg( bool isOn )
        {
            recommendToggle.interactable = !isOn;
            recommendText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            currentItemType = 1;

            currentUI.gameObject.SetActive( false );
            recommendTran.gameObject.SetActive( true );
            currentUI = recommendTran.gameObject;
            storeItemGroup = goodsGroup;

            controller.SendStoreC2S( Data.StoreType.Recommend );
        }

        private void OnClickUnitTg( bool isOn )
        {
            unitToggle.interactable = !isOn;
            unitText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            currentItemType = 2;

            currentUI.gameObject.SetActive( false );
            unitTran.gameObject.SetActive( true );
            currentUI = unitTran.gameObject;
            dragStoreItemPanel = dragUnitPanel;
            storeItemGroup = unitGroup;

            controller.SendStoreC2S( Data.StoreType.Hero );
        }

        private void OnClickSkinTg( bool isOn )
        {
            skinToggle.interactable = !isOn;
            skinText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            currentItemType = 3;

            currentUI.gameObject.SetActive( false );
            skinTran.gameObject.SetActive( true );
            currentUI = skinTran.gameObject;
            dragStoreItemPanel = dragSkinPanel;
            storeItemGroup = skinGroup;

            controller.SendStoreC2S( Data.StoreType.Skin );
        }

        private void OnClickBoxTg( bool isOn )
        {
            boxToggle.interactable = !isOn;
            boxText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            currentItemType = 4;

            currentUI.gameObject.SetActive( false );
            boxTran.gameObject.SetActive( true );
            currentUI = boxTran.gameObject;
            dragStoreItemPanel = dragBoxPanel;
            storeItemGroup = boxItemGroup;

            controller.SendStoreC2S( Data.StoreType.Box );
        }

        private void OnClickRuneTg( bool isOn )
        {
            runeToggle.interactable = !isOn;
            runeText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            currentItemType = 5;

            currentUI.gameObject.SetActive( false );
            runeTran.gameObject.SetActive( true );
            currentUI = runeTran.gameObject;

            controller.SendStoreC2S( Data.StoreType.Rune );
        }

        private void OnClickDebrisTg( bool isOn )
        {
            debrisToggle.interactable = !isOn;
            debrisText.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;

            currentItemType = 6;

            currentUI.gameObject.SetActive( false );
            debrisTran.gameObject.SetActive( true );
            currentUI = debrisTran.gameObject;

            controller.SendStoreC2S( Data.StoreType.Conversion );
            RefreshDebrisItem();

            debrisText_d.text = controller.GetExchangeItemPrice().ToString();
        }

        #endregion

        #region Unit UI

        private void OnClickWarriorTg_U( bool isOn )
        {
            warriorTg_u.interactable = !isOn;
            warriorTgText_u.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentUnitType = Data.ProfessionType.FighterType;
            RefreshStoreItem();
        }

        private void OnClickAssassinTg_U( bool isOn )
        {
            assassinTg_u.interactable = !isOn;
            assassinTgText_u.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentUnitType = Data.ProfessionType.AssassinType;
            RefreshStoreItem();
        }

        private void OnClickShooterTg_U( bool isOn )
        {
            shooterTg_u.interactable = !isOn;
            shooterTgText_u.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentUnitType = Data.ProfessionType.ShooterType;
            RefreshStoreItem();
        }

        private void OnClickMasterTg_U( bool isOn )
        {
            masterTg_u.interactable = !isOn;
            masterTgText_u.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentUnitType = Data.ProfessionType.WizardType;
            RefreshStoreItem();
        }

        private void OnClickAuxiliaryTg_U( bool isOn )
        {
            auxiliaryTg_u.interactable = !isOn;
            auxiliaryTgText_u.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentUnitType = Data.ProfessionType.AssistantType;
            RefreshStoreItem();
        }

        #endregion

        #region Skin UI

        private void OnClickWarriorTg_S( bool isOn )
        {
            warriorTg_s.interactable = !isOn;
            warriorTgText_s.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentSkinType = Data.ProfessionType.FighterType;
            RefreshStoreItem();
        }

        private void OnClickAssassinTg_S( bool isOn )
        {
            assassinTg_s.interactable = !isOn;
            assassinTgText_s.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentSkinType = Data.ProfessionType.AssassinType;
            RefreshStoreItem();
        }

        private void OnClickShooterTg_S( bool isOn )
        {
            shooterTg_s.interactable = !isOn;
            shooterTgText_s.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentSkinType = Data.ProfessionType.ShooterType;
            RefreshStoreItem();
        }

        private void OnClickMasterTg_S( bool isOn )
        {
            masterTg_s.interactable = !isOn;
            masterTgText_s.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentSkinType = Data.ProfessionType.WizardType;
            RefreshStoreItem();
        }

        private void OnClickAuxiliaryTg_S( bool isOn )
        {
            auxiliaryTg_s.interactable = !isOn;
            auxiliaryTgText_s.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentSkinType = Data.ProfessionType.AssistantType;
            RefreshStoreItem();
        }

        #endregion

        #region Rune UI

        private void OnClickPhysicalTg_R( bool isOn )
        {
            physicalTg_r.interactable = !isOn;
            physicalTgText_r.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentRuneType = 1;
            RefreshRuneItem();
        }

        private void OnClickMagicTg_R( bool isOn )
        {
            magicTg_r.interactable = !isOn;
            magicTgText_r.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentRuneType = 2;
            RefreshRuneItem();
        }

        private void OnClickCritTg_R( bool isOn )
        {
            critTg_r.interactable = !isOn;
            critTgText_r.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentRuneType = 3;
            RefreshRuneItem();
        }

        private void OnClickAtkSpeedTg_R( bool isOn )
        {
            atkSpeedTg_r.interactable = !isOn;
            atkSpeedTgText_r.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentRuneType = 4;
            RefreshRuneItem();
        }

        private void OnClickMoveSpeedTg_R( bool isOn )
        {
            moveSpeedTg_r.interactable = !isOn;
            moveSpeedTgText_r.color = isOn ? Color.white : myGray;
            if ( !isOn )
                return;
            controller.currentRuneType = 5;
            RefreshRuneItem();
        }

        private void OnClickLV1Tg_R( bool isOn )
        {
            lv1Tg_r.interactable = !isOn;
            if ( !isOn )
                return;
            controller.currentRuneLV = 1;
            RefreshRuneItem();
        }

        private void OnClickLV2Tg_R( bool isOn )
        {
            lv2Tg_r.interactable = !isOn;
            if ( !isOn )
                return;
            controller.currentRuneLV = 2;
            RefreshRuneItem();
        }

        private void OnClickLV3Tg_R( bool isOn )
        {
            lv3Tg_r.interactable = !isOn;
            if ( !isOn )
                return;
            controller.currentRuneLV = 3;
            RefreshRuneItem();
        }

        private void OnClickLV4Tg_R( bool isOn )
        {
            lv4Tg_r.interactable = !isOn;
            if ( !isOn )
                return;
            controller.currentRuneLV = 4;
            RefreshRuneItem();
        }

        private void OnClickLV5Tg_R( bool isOn )
        {
            lv5Tg_r.interactable = !isOn;
            if ( !isOn )
                return;
            controller.currentRuneLV = 5;
            RefreshRuneItem();
        }

        private void OnClickLV6Tg_R( bool isOn )
        {
            lv6Tg_r.interactable = !isOn;
            if ( !isOn )
                return;
            controller.currentRuneLV = 6;
            RefreshRuneItem();
        }

        #endregion

        #region Debris UI

        private void OnClickUnitTg_D( bool isOn )
        {
            unitTg_d.interactable = !isOn;
            unitTgText_d.color = isOn ? Color.white : myGray;
            debrisUnitIcon.gameObject.SetActive( isOn );
            if ( !isOn )
                return;

            controller.currentDebrisType = 1;

            debrisText_d.text = controller.GetExchangeItemPrice().ToString();
            RefreshDebrisItem();
        }

        private void OnClickSkinTg_D( bool isOn )
        {
            skinTg_d.interactable = !isOn;
            skinTgText_d.color = isOn ? Color.white : myGray;
            debrisSkinIcon.gameObject.SetActive( isOn );
            if ( !isOn )
                return;

            controller.currentDebrisType = 2;

            debrisText_d.text = controller.GetExchangeItemPrice().ToString();
            RefreshDebrisItem();
        }

        private void OnClickRuneTg_D( bool isOn )
        {
            runeTg_d.interactable = !isOn;
            runeTgText_d.color = isOn ? Color.white : myGray;
            debrisRuneIcon.gameObject.SetActive( isOn );
            if ( !isOn )
                return;

            controller.currentDebrisType = 3;

            debrisText_d.text = controller.GetExchangeItemPrice().ToString();
            RefreshDebrisItem();
        }

        #endregion

        #endregion

        #region Init Debris Item
        
        private void OnCreateDebrisItem( DebrisItem item )
        {
            item.clickExChangeEvent = OnClickBuyButtonCallBack;
        }

        public void InitDebrisItem()
        {
            GameResourceLoadManager.GetInstance().LoadResource( "Debris_Item", OnLoadDebrisItem, true );
        }

        private void OnLoadDebrisItem( string name, UnityEngine.Object obj, System.Object param )
        {
            debrisItemScrollView.InitDataBase( dragDebrisPanel, obj, 5, 193.5f, 255, 10, new Vector3( 100, -138, 0 ) );
        }

        public void RefreshDebrisItem()
        {
            DG.Tweening.DOTween.To( () => dragDebrisPanel.verticalNormalizedPosition, value => dragDebrisPanel.verticalNormalizedPosition = value, 1, 0.3f );
            debrisItemScrollView.InitializeWithData( controller.GetStoreItemData_D() );
        }


        #endregion

        #region Init Store Item

        private int ItemCount = 1;
        public void RefreshStoreItem()
        {
            ItemCount = controller.GetItemCount();
            GameResourceLoadManager.GetInstance().LoadResource( "Store_Item", OnLoadStoreItem, true );
        }

        private void OnLoadStoreItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<StoreItem>( store_Items );

            DG.Tweening.DOTween.To( () => dragStoreItemPanel.horizontalNormalizedPosition, value => dragStoreItemPanel.horizontalNormalizedPosition = value, 0, 0.3f );

            for ( int i = 0; i < ItemCount; i++ )
            {
                StoreItem storeItem;
                if ( store_Items.Count < ItemCount )
                {
                    storeItem = CommonUtil.CreateItem<StoreItem>( obj, storeItemGroup.transform );
                    store_Items.Add( storeItem );
                }
                storeItem = store_Items[i];
                storeItem.transform.SetParent( storeItemGroup.transform );
                storeItem.gameObject.SetActive( true );

                storeItem.info = new UI.StoreController.StoreItemData( i, controller.GetItemIcon( i ), controller.GetItemCostLeft( i ), controller.GetItemCostRight( i ), currentItemType, controller.GetItemName( i ), controller.GetStoreItemBtType( currentItemType, i ), controller.GetItemCostCurrencyType( currentItemType, i ), controller.ItemHaveLimit( i ), controller.GetItemBoughtGoods( i ) );

                storeItem.ClickBuyButtonEvent = OnClickBuyButtonCallBack;
                storeItem.ClickGiveButtonEvent = OnClickGiveButtonCallBack;

                storeItem.RefreshStoreItem();
            }
        }

        #endregion

        #region Init Rune Item

        private int runeCount = 1;
        public void RefreshRuneItem()
        {
            runeCount = controller.GetItemCount();
            GameResourceLoadManager.GetInstance().LoadResource( "Rune_Item", OnLoadStoreRuneItem, true );
        }

        private void OnLoadStoreRuneItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<StoreRuneItem>( rune_Items );

            DG.Tweening.DOTween.To( () => dragRunePanel.verticalNormalizedPosition, value => dragRunePanel.verticalNormalizedPosition = value, 1, 0.3f );

            for ( int i = 0; i < runeCount; i++ )
            {
                StoreRuneItem runeItem;
                if ( rune_Items.Count < runeCount )
                {
                    runeItem = CommonUtil.CreateItem<StoreRuneItem>( obj, runeGroup.transform );
                    rune_Items.Add( runeItem );
                }
                runeItem = rune_Items[i];
                runeItem.gameObject.SetActive( true );

                runeItem.index = i;
                runeItem.currentItemType = currentItemType;
                runeItem.icon = controller.GetItemIcon( i );
                runeItem.RuneCost = controller.GetItemCostLeft( i );
                runeItem.nameStr = controller.GetRuneNameStr( i );
                runeItem.attributeStr = controller.GetRuneAttributeStr( i );
                runeItem.haveLimit = controller.ItemHaveLimit( i );
                runeItem.remainCount = controller.GetItemBoughtGoods( i );
                runeItem.currecyType = controller.GetItemCostCurrencyType( currentItemType, i );
                runeItem.ClickBuyButtonEvent = OnClickBuyButtonCallBack;

                runeItem.RefreshRuneItem();
            }
        }

        #endregion

        #region Init Selling Item

        private int sellCount = 4;
        public void RefreshSellingItem()
        {
            sellCount = controller.GetSellingItemCount();
            GameResourceLoadManager.GetInstance().LoadResource( "Selling_Item", OnLoadStoreSellingItem, true );
        }

        private void OnLoadStoreSellingItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<SellingItem>( selling_Items );

            for ( int i = 0; i < sellCount; i++ )
            {
                SellingItem sellingItem;
                if ( selling_Items.Count < sellCount )
                {
                    sellingItem = CommonUtil.CreateItem<SellingItem>( obj, sellingsGroup.transform );
                    selling_Items.Add( sellingItem );
                }
                sellingItem = selling_Items[i];
                sellingItem.gameObject.SetActive( true );

                sellingItem.index = i;
                sellingItem.icon = controller.GetSellingItemIcon( i );
                sellingItem.nameStr = controller.GetSellingItemName( i );
                sellingItem.ClickBuyButtonCallBack = OnClickBuyButtonCallBack;
                sellingItem.goodPrice = controller.GetSellingItemPrice( i );
                sellingItem.costType = controller.GetSellItemCostType( i );

                sellingItem.RefreshItem();
            }
        }

        #endregion

        #region Open Buy PopUp

        private void OnClickBuyButtonCallBack( int itemIndex )
        {
            int id = controller.GetItemId( itemIndex );
            currentCostType = controller.GetItemCostCurrencyType( currentItemType, itemIndex );

            UIManager.Instance.GetUIByType( UIType.StorePopUpUI, ( ViewBase ui, System.Object param ) => { ( ui as StorePopUpView ).EnterStoreBuyPopUpUI( id, currentCostType ); } );
        }

        #endregion

        #region Open Give PopUp

        private void OnClickGiveButtonCallBack( int itemIndex )
        {
            int id = controller.GetItemId( itemIndex );
            currentCostType = controller.GetItemCostCurrencyType( currentItemType, itemIndex );

			if(currentFriendId !=0)
				UIManager.Instance.GetUIByType( UIType.StorePopUpUI, ( ViewBase ui, System.Object param ) => { ( ui as StorePopUpView ).EnterStoreGivePopUpUI( id ,currentFriendId); } );
			else 
				UIManager.Instance.GetUIByType( UIType.StorePopUpUI, ( ViewBase ui, System.Object param ) => { ( ui as StorePopUpView ).EnterStoreGivePopUpUI( id ); } );
        }

        #endregion

        #region Drag Carousel
        private float time = carouselMoveTime;

        private void Update()
        {
            if ( recommendTran.gameObject.activeSelf )
            {
                if ( time > 0 )
                    time -= Time.deltaTime;
                else
                {
                    time = carouselMoveTime;
                    MoveImage( caroudeImages[currentCarouselIndex].gameObject );
                }
            }
        }

        private void DragImage( GameObject obj )
        {
            time = carouselMoveTime;

            if ( obj.transform.localPosition != Vector3.zero )
                return;

            int lastIndex = currentCarouselIndex - 1 >= 0 ? currentCarouselIndex - 1 : currentCarouselIndex - 1 + carouseImageCount;
            int nextIndex = currentCarouselIndex + 1 <= 2 ? currentCarouselIndex + 1 : currentCarouselIndex + 1 - carouseImageCount;

            if ( Input.GetAxis( "Mouse X" ) > 1 )
            {
                DG.Tweening.DOTween.To( () => posLeft.localPosition, value => caroudeImages[lastIndex].transform.localPosition = value, Vector3.zero, dragTime );
                DG.Tweening.DOTween.To( () => obj.transform.localPosition, value => obj.transform.localPosition = value, posRight.localPosition, dragTime );
                currentCarouselIndex = lastIndex;
                SetTextureIndexImage( currentCarouselIndex );
                return;
            }
            else if ( Input.GetAxis( "Mouse X" ) < -1 )
            {
                DG.Tweening.DOTween.To( () => posRight.localPosition, value => caroudeImages[nextIndex].transform.localPosition = value, Vector3.zero, dragTime );
                DG.Tweening.DOTween.To( () => obj.transform.localPosition, value => obj.transform.localPosition = value, posLeft.localPosition, dragTime );
                currentCarouselIndex = nextIndex;
                SetTextureIndexImage( currentCarouselIndex );
                return;
            }
        }

        private void MoveImage( GameObject obj )
        {
            if ( obj.transform.localPosition != Vector3.zero )
                return;

            int lastIndex = currentCarouselIndex - 1 >= 0 ? currentCarouselIndex - 1 : currentCarouselIndex - 1 + carouseImageCount;
            int nextIndex = currentCarouselIndex + 1 <= 2 ? currentCarouselIndex + 1 : currentCarouselIndex + 1 - carouseImageCount;

            DG.Tweening.DOTween.To( () => posRight.localPosition, value => caroudeImages[nextIndex].transform.localPosition = value, Vector3.zero, dragTime );
            DG.Tweening.DOTween.To( () => obj.transform.localPosition, value => obj.transform.localPosition = value, posLeft.localPosition, dragTime );

            currentCarouselIndex = nextIndex;
            SetTextureIndexImage( currentCarouselIndex );
        }

        private void SetTextureIndexImage( int index )
        {
            for ( int i = 0; i < textureIndexImages.Length; i++ )
            {
                textureIndexImages[i].gameObject.SetActive( false );
            }

            textureIndexImages[index].gameObject.SetActive( true );
        }

        #endregion

        #region Refresh Store

        public void RefreshStoreUI()
        {
            debrisText_d.text = controller.GetExchangeItemPrice().ToString();
            RefreshStoreItem();
        }
        #endregion
    }
}