using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

using Resource;
using Utils;
using DG.Tweening;

namespace UI
{
    public class ArmyManagementView : ViewBase
    {
        #region Component

        private Toggle unitToggle, instituteToggle;
        private Text unitText, instituteText;

        private Transform armyTran;
        //ArmyManagementUI Component
        private Transform armyManTran;
        private GridLayoutGroup groupArmyCards;
        private ToggleGroup toggleGroup_army;
        private ScrollRect dragArmyPanel;
        private Text armyText;

        //CombatDeckUI Component
        private Transform combatDeckTran;
        private GridLayoutGroup groupCombatCards;
        private Toggle toggle_1, toggle_2, toggle_3;
        private Text armyTgText1, armyTgText2, armyTgText3;
        private Text combatDeckText;

        //InstituteManagementUI Component
        private Transform instituteManTran;
        private GridLayoutGroup groupSkillCards;
        private ToggleGroup toggleGroup_insti;
        private ScrollRect dragInstitutePanel;
        private Text instituteSkillText, skillPropText;

        //Institute Skill Deck Component
        private Transform skillDeckTran;
        private GridLayoutGroup skillDeckGroup;
        private Toggle toggle1, toggle2, toggle3;
        private Button unlockButton2, unlockButton3;
        private Image toggleImage2, toggleImage3, unlockImage2, unlockImage3;
        private Text skillToggleText1, skillToggleText2, skillToggleText3, skillDeckText, skillDeckPropText, unlockPriceText2, unlockPriceText3;

        #endregion

        private List<ArmyCardItem> armyCard_Items = new List<ArmyCardItem>();
        private List<CombatDeckCardItem> combatDeckCard_Items = new List<CombatDeckCardItem>();
        private List<InstituteSkillCardItem> instituteSkill_Items = new List<InstituteSkillCardItem>();
        private List<SkillDeckCardItem> skillDeckCard_Items = new List<SkillDeckCardItem>();

        private ArmyCardItem currentClickArmyItem;
        private InstituteSkillCardItem currentClickSkillItem;

        private ArmyManagementController controller;
        private int currentListId;

        private Color myGray = new Color( 150 / (float)255, 150 / (float)255, 150 / (float)255 );

        #region Path
        private const string ARMY_CARD_ITEM_PATH = "ArmyCard_Item";
        private const string COMBATDECK_CARD_ITEM_PATH = "CombatDeckCard_Item";
        private const string INSTITUTE_SKILL_ITEM_PATH = "InstituteSkill_Item";
        private const string INSTITUTE_SKILL_DECK_ITEM_PATH = "SkillCardDeck_Item";
        #endregion

        public override void OnInit()
        {
            base.OnInit();

            controller = new ArmyManagementController( this );
            _controller = controller;

            armyTran = transform.Find( "ArmyTran" );

            unitText = armyTran.Find( "UnitText" ).GetComponent<Text>();
            instituteText = armyTran.Find( "InstituteText" ).GetComponent<Text>();
            unitToggle = armyTran.Find( "ToggleGroup/UnitToggle" ).GetComponent<Toggle>();
            instituteToggle = armyTran.Find( "ToggleGroup/InstituteToggle" ).GetComponent<Toggle>();

            unitToggle.AddListener( OnClickUnitToggle );
            instituteToggle.AddListener( OnClickInstituteToggle );

            #region ArmyManagementUI
            armyManTran = armyTran.Find( "ArmyManagementUI" );

            dragArmyPanel = armyManTran.Find( "DragArmyPanel" ).GetComponent<ScrollRect>();
            groupArmyCards = armyManTran.Find( "DragArmyPanel/ArmyItemGroup" ).GetComponent<GridLayoutGroup>();
            toggleGroup_army = armyManTran.Find( "DragArmyPanel/ArmyItemGroup" ).GetComponent<ToggleGroup>();
            armyText = armyManTran.Find( "ArmyText" ).GetComponent<Text>();
            #endregion

            #region CombatDeckUI
            combatDeckTran = armyTran.Find( "CombatDeckUI" );

            groupCombatCards = combatDeckTran.Find( "CombatItemGroup" ).GetComponent<GridLayoutGroup>();
            combatDeckText = combatDeckTran.Find( "CombatDeckText" ).GetComponent<Text>();
            armyTgText1 = combatDeckTran.Find( "ToggleText_1" ).GetComponent<Text>();
            armyTgText2 = combatDeckTran.Find( "ToggleText_2" ).GetComponent<Text>();
            armyTgText3 = combatDeckTran.Find( "ToggleText_3" ).GetComponent<Text>();

            toggle_1 = combatDeckTran.Find( "ToggleGroup/Toggle_1" ).GetComponent<Toggle>();
            toggle_2 = combatDeckTran.Find( "ToggleGroup/Toggle_2" ).GetComponent<Toggle>();
            toggle_3 = combatDeckTran.Find( "ToggleGroup/Toggle_3" ).GetComponent<Toggle>();

            toggle_1.AddListener( OnClickOneToggle );
            toggle_2.AddListener( OnClickTwoToggle );
            toggle_3.AddListener( OnClickThreeToggle );
            #endregion

            #region InstituteMangementUI
            instituteManTran = armyTran.Find( "InstituteManagementUI" );

            dragInstitutePanel = instituteManTran.Find( "DragInstitutePanel" ).GetComponent<ScrollRect>();
            groupSkillCards = instituteManTran.Find( "DragInstitutePanel/SkillItemGroup" ).GetComponent<GridLayoutGroup>();
            toggleGroup_insti = instituteManTran.Find( "DragInstitutePanel/SkillItemGroup" ).GetComponent<ToggleGroup>();
            instituteSkillText = instituteManTran.Find( "InstituteSkillText" ).GetComponent<Text>();
            skillPropText = instituteManTran.Find( "SkillPropText" ).GetComponent<Text>();
            #endregion

            #region Institute Skill Deck UI
            skillDeckTran = armyTran.Find( "InstituteSkillDeckUI" );

            skillDeckGroup = skillDeckTran.Find( "SkillGroup" ).GetComponent<GridLayoutGroup>();
            toggle1 = skillDeckTran.Find( "ToggleGroup/Toggle_1" ).GetComponent<Toggle>();
            toggle2 = skillDeckTran.Find( "ToggleGroup/Toggle_2" ).GetComponent<Toggle>();
            toggle3 = skillDeckTran.Find( "ToggleGroup/Toggle_3" ).GetComponent<Toggle>();
            unlockButton2 = skillDeckTran.Find( "UnlockButton2" ).GetComponent<Button>();
            unlockButton3 = skillDeckTran.Find( "UnlockButton3" ).GetComponent<Button>();
            toggleImage2 = skillDeckTran.Find( "ToggleImage2" ).GetComponent<Image>();
            toggleImage3 = skillDeckTran.Find( "ToggleImage3" ).GetComponent<Image>();
            unlockImage2 = skillDeckTran.Find( "UnlockImage2" ).GetComponent<Image>();
            unlockImage3 = skillDeckTran.Find( "UnlockImage3" ).GetComponent<Image>();
            skillToggleText1 = skillDeckTran.Find( "ToggleText_1" ).GetComponent<Text>();
            skillToggleText2 = skillDeckTran.Find( "ToggleText_2" ).GetComponent<Text>();
            skillToggleText3 = skillDeckTran.Find( "ToggleText_3" ).GetComponent<Text>();
            skillDeckText = skillDeckTran.Find( "SkillDeckText" ).GetComponent<Text>();
            skillDeckPropText = skillDeckTran.Find( "SkillPropText" ).GetComponent<Text>();
            unlockPriceText2 = skillDeckTran.Find( "UnlockPriceText2" ).GetComponent<Text>();
            unlockPriceText3 = skillDeckTran.Find( "UnlockPriceText3" ).GetComponent<Text>();

            unlockButton2.AddListener( OnClickUnlockButtonTwo, UIEventGroup.Middle, UIEventGroup.Middle );
            unlockButton3.AddListener( OnClickUnlockButtonThree, UIEventGroup.Middle, UIEventGroup.Middle );
            toggle1.AddListener( OnClickSkillToggleOne );
            toggle2.AddListener( OnClickSkillToggleTwo );
            toggle3.AddListener( OnClickSkillToggleThree );
            #endregion

        }

        public override void OnEnter()
        {
            base.OnEnter();

            currentListId = 0;
            toggle_2.isOn = toggle_3.isOn = false;
            toggle_2.interactable = toggle_3.interactable = true;
            toggle_1.isOn = true;
            toggle_1.interactable = false;

            //Display Skill Deck Prop
            SetSkillDeckProp();

            //Init Skill Deck State
            unlockButton2.interactable = controller.isEnoughUnlockSkillDeck2();
            unlockButton3.interactable = controller.isEnoughUnlockSkillDeck3();

            RefreshArmyItem();
        }

        #region ToggleEvent

        private void OnClickUnitToggle( bool isOn )
        {
            unitToggle.interactable = !isOn;
            unitText.color = isOn ? Color.white : myGray;

            if ( !isOn )
                return;

            OpenUnitUI();
        }

        private void OnClickInstituteToggle( bool isOn )
        {
            instituteToggle.interactable = !isOn;
            instituteText.color = isOn ? Color.white : myGray;

            if ( !isOn )
                return;

            OpenInstituteUI();
        }

        #region Army

        private void OnClickOneToggle( bool isOn )
        {
            toggle_1.interactable = !isOn;
            armyTgText1.color = isOn ? Color.white : myGray;

            if ( !isOn )
                return;

            currentListId = 0;
            RefreshArmyItem();
        }

        private void OnClickTwoToggle( bool isOn )
        {
            toggle_2.interactable = !isOn;
            armyTgText2.color = isOn ? Color.white : myGray;

            if ( !isOn )
                return;

            currentListId = 1;
            RefreshArmyItem();
        }

        private void OnClickThreeToggle( bool isOn )
        {
            toggle_3.interactable = !isOn;
            armyTgText3.color = isOn ? Color.white : myGray;

            if ( !isOn )
                return;

            currentListId = 2;
            RefreshArmyItem();
        }

        #endregion

        #region Institute

        private void OnClickSkillToggleOne( bool isOn )
        {
            toggle1.interactable = !isOn;
            skillToggleText1.color = isOn ? Color.white : myGray;

            if ( !isOn )
                return;
            controller.currentSelectSkillDeckIndex = 0;

            RefreshInstituteItem();
            SetSkillDeckProp();
        }

        private void OnClickSkillToggleTwo( bool isOn )
        {
            toggle2.interactable = !isOn;
            skillToggleText2.color = isOn ? Color.white : myGray;

            if ( !isOn )
                return;
            controller.currentSelectSkillDeckIndex = 1;

            RefreshInstituteItem();
            SetSkillDeckProp();
        }

        private void OnClickSkillToggleThree( bool isOn )
        {
            toggle3.interactable = !isOn;
            skillToggleText3.color = isOn ? Color.white : myGray;

            if ( !isOn )
                return;
            controller.currentSelectSkillDeckIndex = 2;

            RefreshInstituteItem();
            SetSkillDeckProp();
        }

        private void OnClickUnlockButtonTwo()
        {
            string disconnectText = "请确认是否花费500金币解锁此标签页";
            string titleText = "解锁";
            System.Action buy = UnlockSecondSkillDeck;

            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, buy, UI.AlertType.ConfirmAndCancel, disconnectText, titleText );
        }

        private void OnClickUnlockButtonThree()
        {
            if ( unlockButton2.isActiveAndEnabled )
            {
                string content = "请先解锁技能页二";
                string title = "提示";

                MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, content, title );
                return;
            }

            string disconnectText = "请确认是否花费300钻石解锁此标签页";
            string titleText = "解锁";
            System.Action buy = UnlockThirdSkillDeck;

            MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, buy, UI.AlertType.ConfirmAndCancel, disconnectText, titleText );
        }

        #endregion

        #region Buy Event

        private void UnlockSecondSkillDeck()
        {
            controller.SendUnlockSkillDeck( 1 );
        }

        private void UnlockThirdSkillDeck()
        {
            controller.SendUnlockSkillDeck( 2 );
        }

        #endregion

        #endregion

        #region Init Army Item

        private int _armyCard_itemCount;
        private int _combatDeck_itemCount = 9;

        public void RefreshArmyItem()
        {
            _armyCard_itemCount = controller.GetUnitsCount();

            GameResourceLoadManager.GetInstance().LoadResource( ARMY_CARD_ITEM_PATH, OnLoadArmyCardItem, true );

            GameResourceLoadManager.GetInstance().LoadResource( COMBATDECK_CARD_ITEM_PATH, OnLoadCombatDeckCardItem, true );
        }


        private void OnLoadArmyCardItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<ArmyCardItem>( armyCard_Items );

            //DOTween.To( () => _dragArmyPanel.verticalNormalizedPosition, value => _dragArmyPanel.verticalNormalizedPosition = value, 1, 0.3f );

            for ( int i = 0; i < _armyCard_itemCount; i++ )
            {
                ArmyCardItem _armyCardItem;
                if ( armyCard_Items.Count < _armyCard_itemCount )
                {
                    _armyCardItem = CommonUtil.CreateItem<ArmyCardItem>( obj, groupArmyCards.transform );

                    armyCard_Items.Add( _armyCardItem );
                }

                _armyCardItem = armyCard_Items[i];
                _armyCardItem.gameObject.SetActive( true );

                _armyCardItem.onArmyItemClicked = OnArmyItemClickCallBack;
                _armyCardItem.armyNumber = controller.GetUnitCount( currentListId, i );
                int id = controller.GetUnitsList()[i].metaId;
                _armyCardItem.id = id;
                _armyCardItem.unitIconImage = controller.GetArmyIcon( id );

                _armyCardItem.RefreshItem();
            }
        }

        private void OnLoadCombatDeckCardItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<CombatDeckCardItem>( combatDeckCard_Items );

            for ( int i = 0; i < _combatDeck_itemCount; i++ )
            {
                CombatDeckCardItem _combatDeckCardItem;
                if ( combatDeckCard_Items.Count < _combatDeck_itemCount )
                {
                    _combatDeckCardItem = CommonUtil.CreateItem<CombatDeckCardItem>( obj, groupCombatCards.transform );

                    combatDeckCard_Items.Add( _combatDeckCardItem );
                }
                else
                {
                    _combatDeckCardItem = combatDeckCard_Items[i];
                    _combatDeckCardItem.gameObject.SetActive( true );
                }

                _combatDeckCardItem.index = i;
                _combatDeckCardItem.onCombatDeckItemClicked = OnCombatDeckItemClickCallBack;
                _combatDeckCardItem.unitIconImage = controller.GetDeckUnitIcon( currentListId, i );
                _combatDeckCardItem.id = controller.GetDeckUnitList( currentListId )[i].metaId;

                _combatDeckCardItem.RefreshItem();
            }
        }

        #endregion

        #region Init Institute Item

        private int instituteSkillCount;
        private int skillDeckCardCount = 4;

        public void RefreshInstituteItem()
        {
            instituteSkillCount = controller.GetInstituteSkillCount();

            GameResourceLoadManager.GetInstance().LoadResource( INSTITUTE_SKILL_ITEM_PATH, OnLoadInstituteSkillItem, true );
            GameResourceLoadManager.GetInstance().LoadResource( INSTITUTE_SKILL_DECK_ITEM_PATH, OnLoadSkillDecklItem, true );
        }

        private void OnLoadInstituteSkillItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<InstituteSkillCardItem>( instituteSkill_Items );

            //DOTween.To( () => dragInstitutePanel.verticalNormalizedPosition, value => dragInstitutePanel.verticalNormalizedPosition = value, 1, 0.3f );

            for ( int i = 0; i < instituteSkillCount; i++ )
            {
                InstituteSkillCardItem instituteItem;
                if ( instituteSkill_Items.Count < instituteSkillCount )
                {
                    instituteItem = CommonUtil.CreateItem<InstituteSkillCardItem>( obj, groupSkillCards.transform );

                    instituteSkill_Items.Add( instituteItem );
                }

                instituteItem = instituteSkill_Items[i];
                instituteItem.gameObject.SetActive( true );

                instituteItem.index = i;
                instituteItem.id = controller.GetInstituteSkillId( i );
                instituteItem.icon = controller.GetInstituteSkillIcon( i );
                instituteItem.unlockLevel = controller.GetInstituteSkillUnlockLevel( i );
                instituteItem.nameStr = controller.GetInstituteSkillName( i );
                instituteItem.isLock = controller.GetInstituteSkillIsLock( i );
                instituteItem.isEquip = controller.GetInstituteSkillIsEquip( i );
                instituteItem.onClickSkillCard = OnInstituteSkillCallBack;

                instituteItem.RefreshItem();
            }
        }

        private void OnLoadSkillDecklItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<SkillDeckCardItem>( skillDeckCard_Items );

            for ( int i = 0; i < skillDeckCardCount; i++ )
            {
                SkillDeckCardItem skillDeckItem;
                if ( skillDeckCard_Items.Count < skillDeckCardCount )
                {
                    skillDeckItem = CommonUtil.CreateItem<SkillDeckCardItem>( obj, skillDeckGroup.transform );

                    skillDeckCard_Items.Add( skillDeckItem );
                }

                skillDeckItem = skillDeckCard_Items[i];
                skillDeckItem.gameObject.SetActive( true );

                skillDeckItem.index = i;
                skillDeckItem.icon = controller.GetSkillDeckItemIcon( i );
                skillDeckItem.nameStr = controller.GetSkillDeckItemName( i );
                skillDeckItem.onClickSkillDeckCard = OnSkillDeckItemClickCallBack;

                skillDeckItem.RefreshItem();
            }
        }

        #endregion

        #region Change Combat Deck

        private void OnArmyItemClickCallBack( ArmyCardItem item )
        {
            currentClickArmyItem = null;
            foreach ( ArmyCardItem ite in armyCard_Items )
            {
                ite.glowImage.gameObject.SetActive( false );
            }

            if ( item.armyNumber <= 0 )
                return;

            item.glowImage.gameObject.SetActive( true );

            currentClickArmyItem = item;
        }

        private void OnCombatDeckItemClickCallBack( CombatDeckCardItem item )
        {
            if ( currentClickArmyItem == null )
                return;

            foreach ( ArmyCardItem ite in armyCard_Items )
            {
                ite.glowImage.gameObject.SetActive( false );
            }

            if ( currentClickArmyItem.id != item.id )
                controller.SaveJoinBattleList( currentListId, item.index, currentClickArmyItem );

            currentClickArmyItem = null;
        }

        #endregion

        #region Change Institute Skill

        private void OnInstituteSkillCallBack( InstituteSkillCardItem item )
        {
            currentClickSkillItem = null;
            foreach ( InstituteSkillCardItem ite in instituteSkill_Items )
            {
                ite.glowImage.gameObject.SetActive( false );
            }

            item.glowImage.gameObject.SetActive( true );

            currentClickSkillItem = item;
            SetClickSkillProp();

            if ( item.isEquip || item.isLock )
                currentClickSkillItem = null;

        }

        private void OnSkillDeckItemClickCallBack( SkillDeckCardItem item )
        {
            if ( currentClickSkillItem == null )
                return;

            foreach ( InstituteSkillCardItem ite in instituteSkill_Items )
            {
                ite.glowImage.gameObject.SetActive( false );
            }

            controller.SaveJoinSkillDeckList( item.index, currentClickSkillItem.id );

            currentClickSkillItem = null;
            SetClickSkillProp();

            SetSkillDeckProp();
        }

        #endregion

        #region Set UI

        public void OpenUnitUI()
        {
            armyManTran.gameObject.SetActive( true );
            combatDeckTran.gameObject.SetActive( true );
            instituteManTran.gameObject.SetActive( false );
            skillDeckTran.gameObject.SetActive( false );

            currentClickArmyItem = null;
            currentClickSkillItem = null;

            RefreshArmyItem();
        }

        private void OpenInstituteUI()
        {
            armyManTran.gameObject.SetActive( false );
            combatDeckTran.gameObject.SetActive( false );
            instituteManTran.gameObject.SetActive( true );
            skillDeckTran.gameObject.SetActive( true );

            currentClickArmyItem = null;
            currentClickSkillItem = null;
            SetClickSkillProp();

            SetSkillDeck();

            RefreshInstituteItem();
        }

        public void SetSkillDeck()
        {
            SetSecondSkillDeck( !controller.IsLockSkillDeck( 1 ) );
            SetThirdSkillDeck( !controller.IsLockSkillDeck( 2 ) );
        }

        public void SetSecondSkillDeck( bool unlock )
        {
            toggleImage2.gameObject.SetActive( unlock );
            skillToggleText2.gameObject.SetActive( unlock );
            unlockPriceText2.gameObject.SetActive( !unlock );
            unlockButton2.gameObject.SetActive( !unlock );
            unlockImage2.gameObject.SetActive( !unlock );
        }

        public void SetThirdSkillDeck( bool unlock )
        {
            toggleImage3.gameObject.SetActive( unlock );
            skillToggleText3.gameObject.SetActive( unlock );
            unlockPriceText3.gameObject.SetActive( !unlock );
            unlockButton3.gameObject.SetActive( !unlock );
            unlockImage3.gameObject.SetActive( !unlock );
        }

        #endregion

        #region Set UI Data

        private void SetClickSkillProp()
        {
            if ( currentClickSkillItem == null )
            {
                skillPropText.text = "";
                return;
            }
            string description = controller.GetSkillPropDescription( currentClickSkillItem.id );
            string buffValue1 = controller.GetSkillBuffValueStr( 0, currentClickSkillItem.id );
            string buffValue2 = controller.GetSkillBuffValueStr( 1, currentClickSkillItem.id );

            skillPropText.text = string.Format( description.Replace( "\\n", "\n" ).Replace( "\\t", "\t" ), buffValue1, buffValue2 );
        }

        private void SetSkillDeckProp()
        {
            skillDeckPropText.text = controller.GetSkillDeckPropsString();
        }

        #endregion
    }
}