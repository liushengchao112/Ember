using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using Data;
using Network;
using Utils;
using Constants;

namespace UI
{
    public class PlayerInfoController : ControllerBase
    {
        private PlayerInfoView view;

        private List<UnitsProto.Unit> unitsProtoList;
        private List<ExpValuesProto.ExpValue> expValuesData;

        private AccountInformationS2C playerInfoData;
        private BattleRecordS2C battleRecordData;

        private string changeName;
        private int changePortraitIndex = -1;

        #region Chinese
        private readonly string popUpTitle = "提示";
        private readonly string inputInvalid = "输入无效，请重新输入";
        private readonly string sameName = "您输入的名字和现在的名字相同，请重新输入";
        private readonly string notEnough = "货币不足，请充值";
        private readonly string nameExist = "您输入的名字已被占用，请重新输入";
        #endregion

        public PlayerInfoController( PlayerInfoView v )
        {
            viewBase = v;
            view = v;

            unitsProtoList = DataManager.GetInstance().unitsProtoData;
            expValuesData = DataManager.GetInstance().expValuesProtoData;
        }

        public override void OnResume()
        {
            base.OnResume();
            NetworkManager.RegisterServerMessageHandler( MsgCode.AccountInfomationMessage, HandleAccountInfomationFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.BattleRecordMessage, HandleBattleRecordFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.ChangeNickNameMessage, HandleChangeNickNameFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.ChangeSignatureNameMessage, HandleChangeSignFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.ChangePortraitMessage, HandleChangePortraitFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.BattleDetailRecordMessage, HandleBattleDetailRecordFeedback );
        }

        public override void OnPause()
        {
            base.OnPause();
            NetworkManager.RemoveServerMessageHandler( MsgCode.AccountInfomationMessage, HandleAccountInfomationFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.BattleRecordMessage, HandleBattleRecordFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.ChangeNickNameMessage, HandleChangeNickNameFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.ChangeSignatureNameMessage, HandleChangeSignFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.ChangePortraitMessage, HandleChangePortraitFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.BattleDetailRecordMessage, HandleBattleDetailRecordFeedback );
        }

        #region Data UI  

        #region PlayerInfo Data

        public string GetPlayerName()
        {
            return playerInfoData.playerName;
        }

        public string GetPlayerLv()
        {
            return "LV." + playerInfoData.level;
        }

        public string GetPlayerXP()
        {
            if ( expValuesData.Find( p => p.ID == ( playerInfoData.level + 1 ) ) == null )
                return "0/0";
            return string.Format( "{0}/{1}", playerInfoData.exps, expValuesData.Find( p => p.ID == ( playerInfoData.level + 1 ) ).XPtoLevel );
        }

        public float GetXPValue()
        {
            if ( expValuesData.Find( p => p.ID == ( playerInfoData.level + 1 ) ) == null )
                return 1;
            return 1 - ( playerInfoData.exps / (float)expValuesData.Find( p => p.ID == ( playerInfoData.level + 1 ) ).XPtoLevel );
        }

        public string GetPlayerSign()
        {
            return playerInfoData.signature;
        }

        public string GetPlayerIcon()
        {
            return playerInfoData.portrait;
        }

        public string GetPlayerId()
        {
            return "ID:" + playerInfoData.playerId;
        }

        public string GetPlayerRank()
        {
            return playerInfoData.currentRank;
        }

        public string GetPlayerSegmentGroup()
        {
            return playerInfoData.segmentGroup;
        }

        public string GetPlayerUnitCount()
        {
            return string.Format( "英雄数量: <color=#acacac>{0}</color>", playerInfoData.heroQuantity );
        }

        public string GetPlayerSkinCount()
        {
            return string.Format( "皮肤数量: <color=#acacac>{0}</color>", playerInfoData.skinQuantity );
        }

        public string GetPlayerFightCount()
        {
            return string.Format( "对战次数: <color=#acacac>{0}</color> 场", playerInfoData.battleQuantity );
        }

        public string GetPlayer1v1WinRate()
        {
            return string.Format( "{0:P1}", playerInfoData.winRateOneToOne );
        }

        public string GetPlayer2v2WinRate()
        {
            return string.Format( "{0:P1}", playerInfoData.winRateTwoToTwo );
        }

        public bool IsMyself()
        {
            return ( playerInfoData.playerId == DataManager.GetInstance().GetPlayerId() );
        }

        #region Fight Record Data

        public List<BattleInfo> GetFightRecordDataList()
        {
            return playerInfoData.battleInfo;
        }

        #endregion

        #endregion

        #region Battle Record Data

        public List<BattleInfo> GetDetailedRecordDataList()
        {
            return battleRecordData.battleInfo;
        }

        #endregion

        #region ChangePlayerPortrait Data

        public string GetPortraitIcon( int index )
        {
            string[] iconArray = new string[] { "EmberAvatar_40", "EmberAvatar_7", "EmberAvatar_9", "EmberAvatar_10", "EmberAvatar_20", "EmberAvatar_21", "EmberAvatar_24", "EmberAvatar_38", "EmberAvatar_39", "EmberAvatar_6", "EmberAvatar_41", "EmberAvatar_42" };

            return iconArray[index];
        }

        #endregion

        #endregion

        #region Send

        public void SendAccountInfomationC2S()
        {
            AccountInformationC2S message = new AccountInformationC2S();

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.AccountInfomationMessage, data );
        }

        public void SendBattleRecordC2S()
        {
            BattleRecordC2S message = new BattleRecordC2S();

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.BattleRecordMessage, data );
        }

        public void SendChangeNameC2S( string name )
        {
            if ( name == DataManager.GetInstance().GetPlayerNickName() )
            {
                MessageDispatcher.PostMessage( MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, sameName, popUpTitle );
                view.CloseChangeNameUI();
                return;
            }
            if ( DataManager.GetInstance().GetPlayerDiamond() < 20 )
            {
                MessageDispatcher.PostMessage( MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, notEnough, popUpTitle );
                view.CloseChangeNameUI();
                return;
            }
            if ( !InputUtil.IsValidInput( name ) )
            {
                MessageDispatcher.PostMessage( MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, inputInvalid, popUpTitle );
                view.CloseChangeNameUI();
                return;
            }

            changeName = name;

            ChangeNickNameC2S message = new ChangeNickNameC2S();

            message.nickName = name;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.ChangeNickNameMessage, data );
        }

        public void SendSignC2S( string sign )
        {
            ChangeSignatureNameC2S message = new ChangeSignatureNameC2S();

            message.playerName = sign;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.ChangeSignatureNameMessage, data );
        }

        public void SendChangePortraitC2S( int iconIndex )
        {
            changePortraitIndex = iconIndex;

            string icon = GetPortraitIcon( iconIndex );
            DataManager dataManager = DataManager.GetInstance();
            if ( icon == dataManager.GetPlayerHeadIcon() )
                return;

            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );
            ChangePortraitC2S message = new ChangePortraitC2S();

            message.icon = icon;
            message.name = dataManager.GetPlayerNickName();

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.ChangePortraitMessage, data );
        }

        public void SendBattleDetailRecordC2S( long battleId )
        {
            BattleDetailRecordC2S message = new BattleDetailRecordC2S();

            message.battleId = battleId;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.BattleDetailRecordMessage, data );
        }

        #endregion

        #region ReponseHanding

        private void HandleAccountInfomationFeedback( byte[] data )
        {
            AccountInformationS2C feedback = ProtobufUtils.Deserialize<AccountInformationS2C>( data );

            if ( feedback.result )
            {
                playerInfoData = feedback;

                view.RefreshPlayerInfoView();
            }
        }

        private void HandleBattleRecordFeedback( byte[] data )
        {
            BattleRecordS2C feedback = ProtobufUtils.Deserialize<BattleRecordS2C>( data );

            if ( feedback.result )
            {
                battleRecordData = feedback;

                view.RefreshDetailedRecordListData();
            }
        }

        private void HandleChangeNickNameFeedback( byte[] data )
        {
            ChangeNickNameS2C feedback = ProtobufUtils.Deserialize<ChangeNickNameS2C>( data );

            if ( feedback.result )
            {
                view.CloseChangeNameUI();
                view.RefreshName( changeName );
                DataManager.GetInstance().SetPlayerNickName( changeName );

                MessageDispatcher.PostMessage( Constants.MessageType.RefreshPlayerName, changeName );
            }
            else
            {
                MessageDispatcher.PostMessage( MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, nameExist, popUpTitle );
                view.CloseChangeNameUI();
                return;
            }
        }

        private void HandleChangeSignFeedback( byte[] data )
        {
            ChangeSignatureNameS2C feedback = ProtobufUtils.Deserialize<ChangeSignatureNameS2C>( data );

            if ( feedback.result )
            {

            }
        }

        private void HandleChangePortraitFeedback( byte[] data )
        {
            ChangePortraitS2C feedback = ProtobufUtils.Deserialize<ChangePortraitS2C>( data );

            if ( feedback.result )
            {
                UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.Normal );

                string icon = GetPortraitIcon( changePortraitIndex );
                DataManager.GetInstance().SetPlayerHeadIcon( icon );
                playerInfoData.portrait = icon;

                view.RefreshPlayerInfoView();
                view.CloseChangePortraitUI();
            }
        }

        private void HandleBattleDetailRecordFeedback( byte[] data )
        {
            BattleDetailRecordS2C feedback = ProtobufUtils.Deserialize<BattleDetailRecordS2C>( data );

            if ( feedback.result )
            {
                view.OpenBattleResultView( feedback.battleInfo );
            }
        }

        #endregion
    }
}