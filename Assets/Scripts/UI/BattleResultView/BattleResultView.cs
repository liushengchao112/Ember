using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

using Resource;
using Utils;
using Data;

namespace UI
{
    public class BattleResultView : ViewBase
    {

        #region Component Name

        //BattleResultPanel
        private Transform battleResultUI;
        //private GameObject expBarObj;
        //private Image expBarMaskImage;
        private Text expBarText, titleText_R, battleModeText, playerLevelText, xpText, moneyText;
        private Button continueButton;
        private GameObject levelUpImage;

        //BattleSettlementPanel
        private Transform battleSettlementUI;
        private GridLayoutGroup victoryGroup, failGroup;
        private Button backButton, saveVideoButton, againButton, shareButton;
        private Text shareText, dateText, battleTimeText, waveCountText, headerText;
        private Text blueKillCount, redKillCount;
        private Button addFriendButton, informButton;
        private Text backBtnText;

        #endregion

        #region Path
        private const string BATTLE_RESULT__ITEM_PATH = "BattleResultItem";
        #endregion

        private BattleResultController controller;

        private List<BattleResultItem> victoryBattleResultItems = new List<BattleResultItem>();
        private List<BattleResultItem> failBattleResultItems = new List<BattleResultItem>();

        private List<BattleInfo> mainUIBattleInfoList = new List<BattleInfo>();

        private bool isMainUIOpen = false;

        public override void OnInit()
        {
            base.OnInit();

            controller = new BattleResultController( this );
            _controller = controller;

            #region BattleResultPanel
            battleResultUI = transform.Find( "BattleResultPanel" );
            //expBarObj = battleResultUI.Find( "ExpBarObj" ).gameObject;
            //expBarMaskImage = expBarObj.transform.Find( "ExpBarMaskImage" ).GetComponent<Image>();
            //expBarText = expBarObj.transform.Find( "ExpBarText" ).GetComponent<Text>();
            titleText_R = battleResultUI.Find( "TitleText" ).GetComponent<Text>();
            battleModeText = battleResultUI.Find( "BattleModeText" ).GetComponent<Text>();
            playerLevelText = battleResultUI.Find( "PlayerLevelText" ).GetComponent<Text>();
            xpText = battleResultUI.Find( "XPText" ).GetComponent<Text>();
            moneyText = battleResultUI.Find( "MoneyText" ).GetComponent<Text>();
            continueButton = battleResultUI.Find( "ContinueButton" ).GetComponent<Button>();
            levelUpImage = battleResultUI.Find( "LevelImageBg/LevelUpImage" ).gameObject;

            continueButton.AddListener( OnClickContinueButton );
            #endregion
            #region BattleSettlementPanel
            battleSettlementUI = transform.Find( "BattleSettlementPanel" );
            battleSettlementUI.gameObject.SetActive( false );
            victoryGroup = battleSettlementUI.Find( "VictoryGroup" ).GetComponent<GridLayoutGroup>();
            failGroup = battleSettlementUI.Find( "FailGroup" ).GetComponent<GridLayoutGroup>();
            shareText = battleSettlementUI.Find( "ShareText" ).GetComponent<Text>();
            dateText = battleSettlementUI.Find( "DateText" ).GetComponent<Text>();
            battleTimeText = battleSettlementUI.Find( "BattleTimeText" ).GetComponent<Text>();
            waveCountText = battleSettlementUI.Find( "WaveCountText" ).GetComponent<Text>();
            headerText = battleSettlementUI.Find( "FriendHeaderText" ).GetComponent<Text>();
            backButton = battleSettlementUI.Find( "BackButton" ).GetComponent<Button>();            
            backBtnText = backButton.transform.Find( "Text" ).GetComponent<Text>();
            saveVideoButton = battleSettlementUI.Find( "SaveVideoButton" ).GetComponent<Button>();
            againButton = battleSettlementUI.Find( "AgainButton" ).GetComponent<Button>();
            shareButton = battleSettlementUI.Find( "ShareButton" ).GetComponent<Button>();
            blueKillCount = battleSettlementUI.Find( "BlueKillCount" ).GetComponent<Text>();
            redKillCount = battleSettlementUI.Find( "RedKillCount" ).GetComponent<Text>();
            addFriendButton = battleSettlementUI.Find( "AddFriendButton" ).GetComponent<Button>();
            informButton = battleSettlementUI.Find( "InformButton" ).GetComponent<Button>();

            backButton.AddListener( OnClickBackButton );
            saveVideoButton.AddListener( OnClickSaveVideoButton );
            againButton.AddListener( OnClickAgainButton );
            shareButton.AddListener( OnClickShareButton );
            addFriendButton.AddListener( OnClickAddFriendButton );
            informButton.AddListener( OnClickInformButton );

            if ( DataManager.GetInstance().recordScreenChoose == 2 || DataManager.GetInstance().CurBattleIsPVE() )
            {
                saveVideoButton.interactable = false;
            }
            #endregion
        }

        public override void OnEnter()
        {
            base.OnEnter();

            battleResultUI.gameObject.SetActive( true );
            battleSettlementUI.gameObject.SetActive( false );
        }

        #region Button Event

        private void OnClickContinueButton()
        {
            battleResultUI.gameObject.SetActive( false );
            battleSettlementUI.gameObject.SetActive( true );
        }

        private void OnClickBackButton()
        {
            if ( isMainUIOpen )
            {
                UIManager.Instance.GetUIByType( UIType.PlayerInfoUI, ( ViewBase ui, System.Object param ) => { ( ui as PlayerInfoView ).openDetailedRecord = true; } );
                OnExit( true );
            }
            else
            {
                controller.EnterMainMenu();
            }            
        }

        private void OnClickAgainButton()
        {
            controller.EnterMainMenu();
        }

        private void OnClickSaveVideoButton()
        {
            controller.RequsetPlayBackData();
        }

        private void OnClickShareButton()
        {

        }

        private void OnClickAddFriendButton()
        {
            for ( int i = 0; i < victoryBattleResultItems.Count; i++ )
            {
                if ( victoryBattleResultItems[i].gameObject.activeInHierarchy )
                {
                    victoryBattleResultItems[i].ShowAddFriendButton();
                }
            }
            for ( int i = 0; i < failBattleResultItems.Count; i++ )
            {
                if ( failBattleResultItems[i].gameObject.activeInHierarchy )
                {
                    failBattleResultItems[i].ShowAddFriendButton();
                }
            }
        }

        private void OnClickInformButton()
        {
            for ( int i = 0; i < victoryBattleResultItems.Count; i++ )
            {
                if ( victoryBattleResultItems[i].gameObject.activeInHierarchy )
                {
                    victoryBattleResultItems[i].ShowInformButton();
                }
            }
            for ( int i = 0; i < failBattleResultItems.Count; i++ )
            {
                if ( failBattleResultItems[i].gameObject.activeInHierarchy )
                {
                    failBattleResultItems[i].ShowInformButton();
                }
            }
        }

        #endregion

        #region Init Battle Result Item

        private int victoryCount;
        private int failCount;

        public void InitBattleResultItem()
        {
            victoryCount = controller.GetVictoryCount();
            failCount = controller.GetFailCount();

            GameResourceLoadManager.GetInstance().LoadResource( BATTLE_RESULT__ITEM_PATH, OnLoadVictoryItem, true );
            GameResourceLoadManager.GetInstance().LoadResource( BATTLE_RESULT__ITEM_PATH, OnLoadFailItem, true );
        }

        private void OnLoadVictoryItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<BattleResultItem>( victoryBattleResultItems );

            for ( int i = 0; i < victoryCount; i++ )
            {
                BattleResultItem resultItem;
                if ( victoryBattleResultItems.Count < victoryCount )
                {
                    resultItem = CommonUtil.CreateItem<BattleResultItem>( obj, victoryGroup.transform );

                    victoryBattleResultItems.Add( resultItem );
                }
                resultItem = victoryBattleResultItems[i];
                resultItem.gameObject.SetActive( true );

                resultItem.isMVP = controller.IsMVP_Victory( i );
                resultItem.mvpValue = controller.GetVictoryMvpValue( i );
                resultItem.playerName = controller.GetVictoryName( i );
                resultItem.killCount = controller.GetVictoryKillCount( i );
                resultItem.killedCount = controller.GetVictoryKilledCount( i );
                resultItem.resourcesCount = controller.GetVictoryResources( i );
                resultItem.icon = controller.GetVictoryIcon( i );
                resultItem.side = controller.GetVictorySide( i );
                resultItem.isVictory = true;
                resultItem.playerID = controller.GetVictoryBattleSituationsList()[i].playerId;

                resultItem.SetAddFriendButton( !DataManager.GetInstance().CurBattleIsPVE() );
                resultItem.ShowAddFriendButton();
                resultItem.RefreshBattleResultItem();
            }
        }

        private void OnLoadFailItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<BattleResultItem>( failBattleResultItems );

            for ( int i = 0; i < failCount; i++ )
            {
                BattleResultItem resultItem;
                if ( failBattleResultItems.Count < failCount )
                {
                    resultItem = CommonUtil.CreateItem<BattleResultItem>( obj, failGroup.transform );

                    failBattleResultItems.Add( resultItem );
                }
                resultItem = failBattleResultItems[i];
                resultItem.gameObject.SetActive( true );

                resultItem.isMVP = controller.IsMVP_Fail( i );
                resultItem.mvpValue = controller.GetFailMvpValue( i );
                resultItem.playerName = controller.GetFailName( i );
                resultItem.killCount = controller.GetFailKillCount( i );
                resultItem.killedCount = controller.GetFailKilledCount( i );
                resultItem.resourcesCount = controller.GetFailResources( i );
                resultItem.icon = controller.GetFailIcon( i );
                resultItem.side = controller.GetFailSide( i );
                resultItem.isVictory = false;
                resultItem.playerID = controller.GetFailBattleSituationsList()[i].playerId;

                resultItem.SetAddFriendButton( !DataManager.GetInstance().CurBattleIsPVE() );
                resultItem.ShowAddFriendButton();
                resultItem.RefreshBattleResultItem();
            }
        }

        #endregion

        private void InitMianUIBattleResultItem()
        {
            victoryCount = controller.GetMainUIBattleWinDataList().Count;
            failCount = controller.GetMainUIBattleLoseDataList().Count;

            GameResourceLoadManager.GetInstance().LoadResource( BATTLE_RESULT__ITEM_PATH , OnLoadMianUIBattleVictoryItem , true );
            GameResourceLoadManager.GetInstance().LoadResource( BATTLE_RESULT__ITEM_PATH , OnLoadMianUIBattleLoseItem , true );
        }

        private void OnLoadMianUIBattleVictoryItem( string name , UnityEngine.Object obj , System.Object param )
        {
            CommonUtil.ClearItemList<BattleResultItem>( victoryBattleResultItems );

            for ( int i = 0; i < victoryCount; i++ )
            {
                BattleResultItem resultItem;
                if ( victoryBattleResultItems.Count < victoryCount )
                {
                    resultItem = CommonUtil.CreateItem<BattleResultItem>( obj , victoryGroup.transform );

                    victoryBattleResultItems.Add( resultItem );
                }
                resultItem = victoryBattleResultItems[i];
                resultItem.gameObject.SetActive( true );
                resultItem.HideInteractButton();
                resultItem.isMVP = controller.GetMainUIIsMvpWin( i );

                resultItem.UpdateBattleInfo( controller.GetMainUIBattleWinDataList()[i] );
            }
        }

        private void OnLoadMianUIBattleLoseItem( string name , UnityEngine.Object obj , System.Object param )
        {
            CommonUtil.ClearItemList<BattleResultItem>( failBattleResultItems );

            for ( int i = 0; i < failCount; i++ )
            {
                BattleResultItem resultItem;
                if ( failBattleResultItems.Count < failCount )
                {
                    resultItem = CommonUtil.CreateItem<BattleResultItem>( obj , failGroup.transform );

                    failBattleResultItems.Add( resultItem );
                }
                resultItem = failBattleResultItems[i];
                resultItem.gameObject.SetActive( true );
                resultItem.HideInteractButton();
                resultItem.isMVP = controller.GetMainUIIsMvpLose( i );

                resultItem.UpdateBattleInfo( controller.GetMainUIBattleLoseDataList()[i] );
            }
        }

        public void SetIsMainUIOpen(bool isTrue)
        {
            isMainUIOpen = isTrue;

            if ( isMainUIOpen )
            {
                OnClickContinueButton();
            }
        }

        public void SetBattleInfo( List<BattleInfo> infoList )
        {
            mainUIBattleInfoList.Clear();
            for ( int i = 0; i < infoList.Count; i++ )
            {
                mainUIBattleInfoList.Add( infoList[i] );
            }
            controller.SetBattleInfo( mainUIBattleInfoList );
        }

        private void HideInteractButton()
        {
            backButton.transform.localPosition = new Vector3( 0 , backButton.transform.localPosition.y , 0 );
            saveVideoButton.gameObject.SetActive( false );
            againButton.gameObject.SetActive( false );            
            addFriendButton.gameObject.SetActive( false );
            informButton.gameObject.SetActive( false );
            backBtnText.text = "返回";
        }

        public void SetBattleResultData( Data.NoticeType resType, BattleResultData resInfo, bool isPlayBack = false )
        {
            controller.SetBattleResultData( resType, resInfo, isPlayBack );
        }

        public void RefreshBattleResult()
        {
            if ( isMainUIOpen )
            {
                HideInteractButton();
                waveCountText.gameObject.SetActive( false );
                dateText.text = controller.GetMainUIBattleTimeString();
                battleTimeText.text = controller.GetMainUIBattleDuration();
                blueKillCount.text = controller.GetMainUIBattleBlueKillNum( true ).ToString();
                redKillCount.text = controller.GetMainUIBattleBlueKillNum( false ).ToString();

                InitMianUIBattleResultItem();
            }
            else
            {
                SetUI();

                //expBarMaskImage.fillAmount = 1 - controller.GetExpBarValue();
                //expBarText.text = string.Format( "EXP:{0}/{1}", controller.GetCurrentExp(), controller.GetUpLevelExt() );
                titleText_R.text = controller.GetBattleResultType();
                battleModeText.text = controller.GetBattleTypeString();
                playerLevelText.text = controller.GetPlayerLevel().ToString();
                xpText.text = "XP+" + controller.GetGainExp();
                moneyText.text = "+" + controller.GetGainGold();
                backBtnText.text = "返回大厅";
                backButton.transform.localPosition = new Vector3( -202 , backButton.transform.localPosition.y , 0 );

                waveCountText.text = "抵抗波数：" + controller.GetWaveCount();
                battleTimeText.text = controller.GetBattleDuration();
                dateText.text = controller.GetBattleDate();

                blueKillCount.text = controller.GetBlueSideKillCount().ToString();
                redKillCount.text = controller.GetRedSideKillCount().ToString();

                InitBattleResultItem();

                if ( controller.GetPlayerLevel() > DataManager.GetInstance().GetPlayerLevel() )
                {
                    levelUpImage.SetActive( true );
                }
                else
                {
                    levelUpImage.SetActive( false );
                }
            }
            if ( DataManager.GetInstance().GetBattleSimluateState() )
            {
                HideInteractButton();
            }
        }

        private void SetUI()
        {
            switch ( controller.currentBattleType )
            {
                case Data.BattleType.BattleP1vsP1:
                case Data.BattleType.BattleP2vsP2:
                    waveCountText.gameObject.SetActive( false );
                    break;
                case Data.BattleType.Survival:
                    waveCountText.gameObject.SetActive( true );
                    break;
                case Data.BattleType.Tranining:
                    waveCountText.gameObject.SetActive( false );
                    break;
            }
        }
    }
}
