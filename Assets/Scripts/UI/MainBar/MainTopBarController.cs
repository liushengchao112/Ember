using System.Collections;

using Data;
using Utils;
using Network;

namespace UI
{
    public class MainTopBarController : ControllerBase
    {
        private MainTopBarView view;

        public MainTopBarController( MainTopBarView v )
        {
            viewBase = v;
            view = v;

            OnCreate();
        }

        public override void OnCreate()
        {
            base.OnCreate();

        }

        public override void OnResume()
        {
            base.OnResume();

            // add event listeners
            MessageDispatcher.AddObserver( RefreshCurrency, Constants.MessageType.RefreshCurrency );
            MessageDispatcher.AddObserver( RefreshPlayerName, Constants.MessageType.RefreshPlayerName );
            MessageDispatcher.AddObserver( RefreshPlayerUpLevel, Constants.MessageType.RefreshPlayerUpLevelData );

            NetworkManager.RegisterServerMessageHandler(ServerType.GameServer, MsgCode.InvitationNoticeMessage, HandleInvationNoticeFeedback );
        }

        public override void OnPause()
        {
            base.OnPause();

            // remover event listeners
            MessageDispatcher.RemoveObserver( RefreshCurrency, Constants.MessageType.RefreshCurrency );
            MessageDispatcher.RemoveObserver( RefreshPlayerName, Constants.MessageType.RefreshPlayerName );
            MessageDispatcher.RemoveObserver( RefreshPlayerUpLevel, Constants.MessageType.RefreshPlayerUpLevelData );
            
            NetworkManager.RemoveServerMessageHandler(ServerType.GameServer, MsgCode.InvitationNoticeMessage, HandleInvationNoticeFeedback );
        }
        
        #region Data UI

        public int GetPlayerLevel()
        {
            return DataManager.GetInstance().GetPlayerLevel();
        }

        public string GetPlayerName()
        {
            return DataManager.GetInstance().GetPlayerNickName();
        }

        public string GetPlayerHeadIcon()
        {
            return DataManager.GetInstance().GetPlayerHeadIcon();
        }

        public string GetEmberCount()
        {
            return SetCurrencyQuantityLengh( DataManager.GetInstance().GetPlayerEmber() );
        }

        public string GetGoldCount()
        {
            return SetCurrencyQuantityLengh( DataManager.GetInstance().GetPlayerGold() );
        }

        public string GetDiamondCount()
        {
            return SetCurrencyQuantityLengh( DataManager.GetInstance().GetPlayerDiamond() );
        }

        public string GetXPText()
        {
            if ( DataManager.GetInstance().GetPlayerLevel() >= 40 )
                return "";
            return DataManager.GetInstance().GetPlayerExp() + "/" + GetCurrentMaxXP();
        }

        public float GetXPBarValue()
        {
            if ( ( DataManager.GetInstance().GetPlayerLevel() % 10 ) == 0 )
                return 0;
            if ( GetCurrentMaxXP() == 0 )
                return 1;

            return ( 1 - ( (float)DataManager.GetInstance().GetPlayerExp() / (float)GetCurrentMaxXP() ) );
        }

        public int GetCurrentMaxXP()
        {
            return DataManager.GetInstance().GetPlayerCurrentLevelMaxXP();
        }
        #endregion

        #region Updata Battery

        public float GetAndroidBatteryValue()
        {
            float batteryLevel = 1;

            //batteryLevel = DeviceUtil.GetBatteryValue() / (float)100;

            return batteryLevel;
        }

        #endregion

        private string SetCurrencyQuantityLengh( int quantity )
        {
            if ( quantity > 100000 )
                return quantity / 1000 + "k";
            return quantity.ToString();
        }

        #region Event Listeners

        private void RefreshCurrency()
        {
            view.RefreshGold();
            view.RefreshEmber();
            view.RefreshDiamond();
        }

        private void RefreshPlayerName( object name )
        {
            view.RefreshPlayerName( name.ToString() );
        }

        private void RefreshPlayerUpLevel()
        {
            view.RefreshPlayerXP();
        }
        #endregion

        #region Reponse Handle

        private void HandleInvationNoticeFeedback( byte[] data )
        {
            InvitationNoticeS2C feedback = ProtobufUtils.Deserialize<InvitationNoticeS2C>( data );

            if ( feedback.state == InvitationState.WaitingProcess )
            {
                MessageDispatcher.PostMessage( Constants.MessageType.OpenFriendInvationAlert, feedback.friendId, feedback.battleType, feedback.friendName, feedback.friendPortrait );
            }
        }

        #endregion
    }
}
