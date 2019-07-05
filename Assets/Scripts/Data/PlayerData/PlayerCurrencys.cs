using System.Collections;
using System.Collections.Generic;

using Network;
using Utils;

namespace Data
{
    public class PlayerCurrencies
    {
        public List<PlayerCurrencyInfo> currencys;

        public PlayerCurrencies()
        {
            currencys = new List<Data.PlayerCurrencyInfo>();

            currencys.Add( new Data.PlayerCurrencyInfo( CurrencyType.DIAMOND, 0 ) );
            currencys.Add( new Data.PlayerCurrencyInfo( CurrencyType.GOLD, 0 ) );
            currencys.Add( new Data.PlayerCurrencyInfo( CurrencyType.EMBER, 0 ) );
        }

        public PlayerCurrencyInfo GetCurrency( CurrencyType type )
        {
            return currencys.Find( p => p.type == type );
        }

        public void RegisterPlayerCurrencyServerMessageHandler()
        {
            NetworkManager.RegisterServerMessageHandler( MsgCode.GainMessage, HandleGainCurrencyFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.RefreshCurrencyMessage, HandleRefreshCurrencyFeedback );
        }

        public void RegisterPlayerCurrencySocialServerMessageHandler()
        {
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.GainMessage, HandleGainCurrencyFeedback );
        }

        #region Handle Currency data feedback

        private void HandleGainCurrencyFeedback( byte[] data )
        {
            GainS2C feedback = ProtobufUtils.Deserialize<GainS2C>( data );

            if ( feedback.result )
            {
                List<Currency> currenciesList = feedback.currencies;
                for ( int i = 0; i < currenciesList.Count; i++ )
                {
                    CurrencyType type = currenciesList[i].currencyType;

                    PlayerCurrencyInfo currency = GetCurrency( type );
                    currency.currencyValue += currenciesList[i].currencyValue;
                }

                MessageDispatcher.PostMessage( Constants.MessageType.RefreshCurrency );
            }
        }

        private void HandleRefreshCurrencyFeedback( byte[] data )
        {
            RefreshCurrencyS2C feedback = ProtobufUtils.Deserialize<RefreshCurrencyS2C>( data );

            if ( feedback.result )
            {
                List<Currency> currenciesList = feedback.currencies;
                for ( int i = 0; i < currenciesList.Count; i++ )
                {
                    CurrencyType type = currenciesList[i].currencyType;

                    PlayerCurrencyInfo currency = GetCurrency( type );
                    currency.currencyValue = currenciesList[i].currencyValue;
                }

                MessageDispatcher.PostMessage( Constants.MessageType.RefreshCurrency );
            }
        }

        #endregion

    }

    public class PlayerCurrencyInfo
    {
        public CurrencyType type;
        public int currencyValue;

        public PlayerCurrencyInfo( CurrencyType type, int currencyValue )
        {
            this.type = type;
            this.currencyValue = currencyValue;
        }
    }
}
