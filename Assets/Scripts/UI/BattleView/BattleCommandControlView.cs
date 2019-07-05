using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Utils;

namespace UI
{
    public class BattleCommandControlView : ViewBase
    {
        private BattleCommandControler controler;

        private bool startOrderNoticeCD = false;
        private float orderNoticeShowTime = 0;

        private Transform commandRoot;
        private Image commandIcon;
        private GameObject commandSelectObj;
        private Transform buttonGroup;
        private Image retreatButton, defenseButton, attackButton;
        private Image retreatButtonCD, defenseButtonCD, attackButtonCD;
        private GameObject retreatSelect, defenseSelect, attackSelect;
        private Transform sneerRoot;
        private Transform concentratedFireRoot;
        private Transform backCityRoot;
        private Button sneerIconBtn, concentratedFireBtn, backCityBtn;//sneer->greeting
        private Transform greetingRoot, greetingContent;
        private Button zhiJiaoBtn, niceWorkBtn, FaultBtn, goodLuckBtn, goodGameBtn;
        private Image greetingCDImage;

        private bool isShowGreetingPanel = false;
        private bool startGreetingCD = false;
        private float greetingPanelShowTime = 0;

        public override void OnInit()
        {
            _controller = new BattleCommandControler( this );
            controler = _controller as BattleCommandControler;

            commandRoot = transform.Find( "ControlAni/Command" );
            commandIcon = commandRoot.Find( "IconImage" ).GetComponent<Image>();
            ClickHandler.Get( commandIcon.gameObject ).onClickDown = OnClickDownCommandIcon;
            ClickHandler.Get( commandIcon.gameObject ).onClickUp = OnClickUpCommandIcon;
            commandSelectObj = commandRoot.Find( "SelectedImage" ).gameObject;
            buttonGroup = transform.Find( "ControlAni/ButtonGroup" );
            retreatButton = buttonGroup.Find( "RetreatButton" ).GetComponent<Image>();
            ClickHandler.Get( retreatButton.gameObject ).onEnter = OnEnterRetreatButton;
            ClickHandler.Get( retreatButton.gameObject ).onExit = OnExitRetreatButton;
            defenseButton = buttonGroup.Find( "DefenseButton" ).GetComponent<Image>();
            ClickHandler.Get( defenseButton.gameObject ).onEnter = OnEnterDefenseButton;
            ClickHandler.Get( defenseButton.gameObject ).onExit = OnExitDefenseButton;
            attackButton = buttonGroup.Find( "AttackButton" ).GetComponent<Image>();
            ClickHandler.Get( attackButton.gameObject ).onEnter = OnEnterAttackButton;
            ClickHandler.Get( attackButton.gameObject ).onExit = OnExitAttackButton;
            retreatButtonCD = buttonGroup.Find( "RetreatButtonCD" ).GetComponent<Image>();
            defenseButtonCD = buttonGroup.Find( "DefenseButtonCD" ).GetComponent<Image>();
            attackButtonCD = buttonGroup.Find( "AttackButtonCD" ).GetComponent<Image>();
            retreatSelect = buttonGroup.Find( "RetreatSelect" ).gameObject;
            defenseSelect = buttonGroup.Find( "DefenseSelect" ).gameObject;
            attackSelect = buttonGroup.Find( "AttackSelect" ).gameObject;
            sneerRoot = transform.Find( "ControlAni/Sneer" );
            sneerIconBtn = sneerRoot.Find( "IconImage" ).GetComponent<Button>();
            sneerIconBtn.AddListener( OnClickSneerIconBtn );
            greetingCDImage = sneerRoot.Find( "IconImageCD" ).GetComponent<Image>();
            greetingRoot = sneerRoot.Find( "GreetingRoot" );
            greetingContent = greetingRoot.Find( "ScrollView/Viewport/Content" );
            zhiJiaoBtn = greetingContent.Find( "ZhiJiao" ).GetComponent<Button>();
            zhiJiaoBtn.AddListener( OnClickZhiJiaoBtn );
            niceWorkBtn = greetingContent.Find( "NiceWork" ).GetComponent<Button>();
            niceWorkBtn.AddListener( OnClickNiceWorkBtn );
            FaultBtn = greetingContent.Find( "Fault" ).GetComponent<Button>();
            FaultBtn.AddListener( OnClickFaultBtn );
            goodLuckBtn = greetingContent.Find( "GoodLuck" ).GetComponent<Button>();
            goodLuckBtn.AddListener( OnClickGoodLuckBtn );
            goodGameBtn = greetingContent.Find( "GoodGame" ).GetComponent<Button>();
            goodGameBtn.AddListener( OnClickGoodGameBtn );
            ShowGreetingRoot( false );
            concentratedFireRoot = transform.Find( "ControlAni/ConcentratedFire" );
            concentratedFireBtn = concentratedFireRoot.Find( "IconImage" ).GetComponent<Button>();
            concentratedFireBtn.AddListener( OnClickConcentratedFireBtn );
            backCityRoot = transform.Find( "ControlAni/BackCity" );
            backCityBtn = backCityRoot.Find( "IconImage" ).GetComponent<Button>();
            backCityBtn.AddListener( OnClickBackCityBtn );
            ShowButtonGroup( false );
        }

        private void Update()
        {
            if ( startOrderNoticeCD )
            {
                orderNoticeShowTime += Time.deltaTime;
                if ( orderNoticeShowTime >= 5 )
                {
                    startOrderNoticeCD = false;
                    orderNoticeShowTime = 0;
                    HideOrderButtonCd();
                }
                else
                {
                    retreatButtonCD.fillAmount -= Time.deltaTime / 5;
                    defenseButtonCD.fillAmount -= Time.deltaTime / 5;
                    attackButtonCD.fillAmount -= Time.deltaTime / 5;
                }
            }

            if ( startGreetingCD )
            {
                greetingPanelShowTime += Time.deltaTime;
                if ( greetingPanelShowTime >= 5 )
                {
                    startGreetingCD = false;
                    greetingPanelShowTime = 0;
                    greetingCDImage.gameObject.SetActive( false );
                }
                else
                {
                    greetingCDImage.fillAmount -= Time.deltaTime / 5;
                }
            }
        }

        private void OnClickDownCommandIcon( GameObject go )
        {
            ShowButtonGroup( true );
        }

        private void OnClickUpCommandIcon( GameObject go )
        {
            ShowButtonGroup( false );
        }

        private void ShowButtonGroup( bool isShow )
        {
            buttonGroup.gameObject.SetActive( isShow );
        }

        private void HideOrderButtonCd()
        {
            retreatButtonCD.gameObject.SetActive( false );
            defenseButtonCD.gameObject.SetActive( false );
            attackButtonCD.gameObject.SetActive( false );
        }

        private void OnEnterRetreatButton( GameObject go )
        {
            retreatSelect.SetActive( true );
            OnClickRetreatButton();
        }

        private void OnExitRetreatButton( GameObject go )
        {
            retreatSelect.SetActive( false );
        }

        private void OnClickRetreatButton()
        {
            SendOrderNotice( OrderType.Retreat );
        }

        private void OnEnterDefenseButton( GameObject go )
        {
            defenseSelect.SetActive( true );
            OnClickDefenseButton();
        }

        private void OnExitDefenseButton( GameObject go )
        {
            defenseSelect.SetActive( false );
        }

        private void OnClickDefenseButton()
        {
            SendOrderNotice( OrderType.Defense );
        }

        private void OnEnterAttackButton( GameObject go )
        {
            attackSelect.SetActive( true );
            OnClickAttackButton();
        }

        private void OnExitAttackButton( GameObject go )
        {
            attackSelect.SetActive( false );
        }

        private void OnClickAttackButton()
        {
            SendOrderNotice( OrderType.Attack );
        }

        private void SendOrderNotice( OrderType type )
        {
            controler.SendOrderNotice( type );
            StartOrderNoticeCD();
        }

        private void StartOrderNoticeCD()
        {
            retreatButtonCD.gameObject.SetActive( true );
            retreatButtonCD.fillAmount = 1;
            defenseButtonCD.gameObject.SetActive( true );
            defenseButtonCD.fillAmount = 1;
            attackButtonCD.gameObject.SetActive( true );
            attackButtonCD.fillAmount = 1;
            startOrderNoticeCD = true;
        }

        private void StartGreetingCD()
        {
            greetingCDImage.gameObject.SetActive( true );
            greetingCDImage.fillAmount = 1;
            startGreetingCD = true;
        }

        private void OnClickSneerIconBtn()
        {
            //temp code
            return;
            isShowGreetingPanel = !isShowGreetingPanel;

            ShowGreetingRoot( isShowGreetingPanel );
        }

        private void ShowGreetingRoot( bool isShow )
        {
            greetingRoot.gameObject.SetActive( isShow );
            isShowGreetingPanel = isShow;
        }

        private void SendGreeting( GreetingType type )
        {
            controler.SendGreeting( type );
            StartGreetingCD();
            ShowGreetingRoot( false );
        }

        private void OnClickZhiJiaoBtn()
        {
            SendGreeting( GreetingType.ZhiJiao );
        }

        private void OnClickNiceWorkBtn()
        {
            SendGreeting( GreetingType.NiceWork );
        }

        private void OnClickFaultBtn()
        {
            SendGreeting( GreetingType.Fault );
        }

        private void OnClickGoodLuckBtn()
        {
            SendGreeting( GreetingType.GoodLuck );
        }

        private void OnClickGoodGameBtn()
        {
            SendGreeting( GreetingType.GoodGame );
        }

        private void OnClickConcentratedFireBtn()
        {
            controler.SelectAllUnit();
        }

        private void OnClickBackCityBtn()
        {            
            MessageDispatcher.PostMessage( Constants.MessageType.BattleUIBackToCity );
        }
    }
}
