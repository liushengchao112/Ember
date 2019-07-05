using System;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Utils;
using Network;

namespace UI
{
    public class StorePopUpController : ControllerBase
    {
        private StorePopUpView view;
        private List<StoreProto.Store> storeProtosData;
        private List<ExchangeProto.Exchange> exchangeProtosData;
        private List<ItemBaseProto.ItemBase> itemProtosData;
        private List<RuneProto.Rune> runeProtosData;
        private List<FriendInfo> friendList;

        private const int runePropDvalue = 604;
        private int buyCount = 0;
        private int currentGoodId;

        public StorePopUpController( StorePopUpView v )
        {
            viewBase = v;
            view = v;

            storeProtosData = DataManager.GetInstance().storeProtoData;
            exchangeProtosData = DataManager.GetInstance().exchangeProtoData;
            itemProtosData = DataManager.GetInstance().itemsProtoData;
            runeProtosData = DataManager.GetInstance().runeProtoData;
            friendList = new List<FriendInfo>();
        }

        public override void OnResume()
        {
            base.OnResume();
            NetworkManager.RegisterServerMessageHandler( MsgCode.StoreBuyMessage, HandleStoreBuyS2CFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.StoreExchangeMessage, HandleStoreExchangeS2CFeedback );
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.GIFT_ITEMS, HandleGiftItemsFeedback );
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.RelationListMessage, HandleRelationListFeedback );
            MessageDispatcher.AddObserver( OpenGainItemWindow, Constants.MessageType.OpenGainItemWindow );
        }

        public override void OnPause()
        {
            base.OnPause();
            NetworkManager.RemoveServerMessageHandler( MsgCode.StoreBuyMessage, HandleStoreBuyS2CFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.StoreExchangeMessage, HandleStoreExchangeS2CFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.GIFT_ITEMS, HandleGiftItemsFeedback );
            NetworkManager.RemoveServerMessageHandler( ServerType.SocialServer, MsgCode.RelationListMessage, HandleRelationListFeedback );
            MessageDispatcher.RemoveObserver( OpenGainItemWindow, Constants.MessageType.OpenGainItemWindow );
        }

        #region Data UI

        public string GetStoreItemName( int id )
        {
            return storeProtosData.Find( p => p.ID == id ).Name;
        }

        public int GetStoreItemId( int id )
        {
            return storeProtosData.Find( p => p.ID == id ).ItemId;
        }

        public string GetStoreItemDescribe( int id )
        {
            int textId = storeProtosData.Find( p => p.ID == id ).Txt_ID;

            return textId.Localize();
        }

        public List<FriendInfo> GetfriendList()
        {
            return friendList;
        }

        public string GetStoreItemProp( int id )
        {
            string prop = "";

            if ( GetStoreItemType( id ) == 3 )
                prop = GetRuneAttributeStr( id );
            else
                prop = "";
            return prop;
        }

        private string GetRuneAttributeStr( int id )
        {
            string prop1 = "";
            string prop2 = "";
            string prop3 = "";
            string prop4 = "";

            int itemId = storeProtosData.Find( p => p.ID == id ).ItemId;

            RuneProto.Rune runeProto = runeProtosData.Find( p => p.ID == itemId );

            if ( runeProto.Property1 != 0 )
                prop1 = ( runeProto.Property1 + runePropDvalue ).Localize() + ( runeProto.Addition1 == 1 ? runeProto.Value1.ToString() : string.Format( "{0:P}", runeProto.Value1 ) );
            if ( runeProto.Property2 != 0 )
                prop2 = ( runeProto.Property2 + runePropDvalue ).Localize() + ( runeProto.Addition2 == 1 ? runeProto.Value2.ToString() : string.Format( "{0:P}", runeProto.Value2 ) );
            if ( runeProto.Property3 != 0 )
                prop3 = ( runeProto.Property3 + runePropDvalue ).Localize() + ( runeProto.Addition3 == 1 ? runeProto.Value3.ToString() : string.Format( "{0:P}", runeProto.Value3 ) );
            if ( runeProto.Property4 != 0 )
                prop4 = ( runeProto.Property4 + runePropDvalue ).Localize() + ( runeProto.Addition4 == 1 ? runeProto.Value4.ToString() : string.Format( "{0:P}", runeProto.Value4 ) );

            return prop1 + " " + prop2 + " " + prop3 + " " + prop4;
        }

        public int GetStoreItemType( int id )
        {
            return storeProtosData.Find( p => p.ID == id ).Type;
        }

        public int GetStoreItemLeftCostType( int id )
        {
            return storeProtosData.Find( p => p.ID == id ).CurrencyType1;
        }

        public int GetStoreItemRightCostType( int id )
        {
            return storeProtosData.Find( p => p.ID == id ).CurrencyType2;
        }

        public int GetStoreItemIcon( int id )
        {
            return storeProtosData.Find( p => p.ID == id ).Icon;
        }

        public int GetStoreItemCostLeft( int id )
        {
            return storeProtosData.Find( p => p.ID == id ).CurrencyValue1;
        }

        public int GetDiamondCost( int id )
        {
            StoreProto.Store storeData = storeProtosData.Find( p => p.ID == id );

            if ( storeData.CurrencyType1 == (int)CurrencyType.DIAMOND )
                return storeData.CurrencyValue1;
            if ( storeData.CurrencyType2 == (int)CurrencyType.DIAMOND )
                return storeData.CurrencyValue2;
            DebugUtils.Log( DebugUtils.Type.UI, "The Currency of gift is't Diamond , Please Check ... " );
            return 0;
        }

        public int GetStoreItemCostRight( int id )
        {
            StoreProto.Store store = storeProtosData.Find( p => p.ID == id );
            if ( store.CurrencyType2 == 0 )
                return store.CurrencyValue1;
            return store.CurrencyValue2;
        }

        public bool MoreThanLV10()
        {
            return DataManager.GetInstance().GetPlayerLevel() >= 10;
        }

        public int GetStoreItemLeftMaxCount( int id )
        {
            int currency = 0;

            switch ( (CurrencyType)GetStoreItemLeftCostType( id ) )
            {
                case CurrencyType.GOLD:
                    currency = DataManager.GetInstance().GetPlayerGold();
                    break;
                case CurrencyType.EMBER:
                    currency = DataManager.GetInstance().GetPlayerEmber();
                    break;
                case CurrencyType.DIAMOND:
                    currency = DataManager.GetInstance().GetPlayerDiamond();
                    break;
            }
            return currency / storeProtosData.Find( p => p.ID == id ).CurrencyValue1;
        }

        public int GetStoreItemRightMaxCount( int id )
        {
            int currencyType = storeProtosData.Find( p => p.ID == id ).CurrencyValue2;
            if ( currencyType == 0 )
                return 0;

            int currency = 0;

            switch ( (CurrencyType)GetStoreItemRightCostType( id ) )
            {
                case CurrencyType.GOLD:
                    currency = DataManager.GetInstance().GetPlayerGold();
                    break;
                case CurrencyType.EMBER:
                    currency = DataManager.GetInstance().GetPlayerEmber();
                    break;
                case CurrencyType.DIAMOND:
                    currency = DataManager.GetInstance().GetPlayerDiamond();
                    break;
            }

            return currency / currencyType;
        }

        private bool IsAloneBt( int id )
        {
            return !( storeProtosData.Find( p => p.ID == id ).Type == (int)StoreType.Box ) || ( storeProtosData.Find( p => p.ID == id ).Type == (int)StoreType.Skin );
        }

        //Debris Data
        public int GetExchangeId( int id )
        {
            return exchangeProtosData.Find( p => p.ItemId == id ).ID;
        }

        public string GetDebrisItemName( int id )
        {
            return exchangeProtosData.Find( p => p.ID == GetExchangeId( id ) ).Name;
        }

        public int GetDebrisItemIcon( int id )
        {
            return exchangeProtosData.Find( p => p.ID == GetExchangeId( id ) ).Icon;
        }

        public string GetDebrisItemDescribe( int id )
        {
            return exchangeProtosData.Find( p => p.ID == GetExchangeId( id ) ).Txt_ID.Localize();
        }

        public int GetDebrisItemPrice( int id )
        {
            return exchangeProtosData.Find( p => p.ID == GetExchangeId( id ) ).DebrisQuantity;
        }

        public int GetDebrisItemPriceType( int id )
        {
            return exchangeProtosData.Find( p => p.ID == GetExchangeId( id ) ).ExchangeType;
        }

        public int GetDebrisCostCount( int id )
        {
            int itemid = exchangeProtosData.Find( p => p.ID == GetExchangeId( id ) ).DebrisType;

            PlayerBagInfo debrisBag = DataManager.GetInstance().GetPlayerBag( BagType.DebrisBag );

            if ( debrisBag.itemList.Find( p => p.itemId == itemid ) == null )
                return 0;

            return debrisBag.itemList.Find( p => p.itemId == itemid ).count;
        }

        #endregion

        #region Send

        public void SendGiftItems( long playerId, int itemId )
        {
            currentGoodId = itemId;
            GiftItemsC2S message = new GiftItemsC2S();
            message.playerId = playerId;
            message.itemId = itemId;
            byte[] stream = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.GIFT_ITEMS, stream );
        }

        public void SendRelationList()
        {
            RelationListC2S message = new RelationListC2S();
            message.listType = 1;
            byte[] stream = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( ServerType.SocialServer, MsgCode.RelationListMessage, stream );
        }

        public void SendStoreBuyC2S( CurrencyType type, int id, int count )
        {
            StorePurchaseC2S message = new StorePurchaseC2S();

            buyCount = count;

            message.currencyType = type;
            message.goodId = id;
            message.count = count;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.StoreBuyMessage, data );
        }

        public void SendStoreExchangeC2S( int id )
        {
            StoreExchangeC2S message = new StoreExchangeC2S();
            message.exchangeId = id;
            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.StoreExchangeMessage, data );
        }

        #endregion

        #region ReponseHandling

        private void HandleStoreBuyS2CFeedback( byte[] data )
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.Normal );
            StorePurchaseS2C feedback = ProtobufUtils.Deserialize<StorePurchaseS2C>( data );

            if ( feedback.result )
            {
                UIManager.Instance.GetUIByType( UIType.StoreScreen, ( ViewBase ui, System.Object param ) => { ( ui as StoreView ).RefreshStoreUI(); } );
                view.OnExit( false );
            }
        }

        private void HandleStoreExchangeS2CFeedback( byte[] data )
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.Normal );
            StoreExchangeS2C feedback = ProtobufUtils.Deserialize<StoreExchangeS2C>( data );

            if ( feedback.result )
            {
                UIManager.Instance.GetUIByType( UIType.StoreScreen, ( ViewBase ui, System.Object param ) => { ( ui as StoreView ).RefreshStoreUI(); } );
                view.OnExit( false );
            }
        }

        private void HandleGiftItemsFeedback( byte[] data )
        {
            GiftItemsS2C feedback = ProtobufUtils.Deserialize<GiftItemsS2C>( data );
            if ( feedback.result )
            {
                int diamond = DataManager.GetInstance().GetPlayerDiamond();
                DataManager.GetInstance().SetPlayerDiamond( diamond - GetDiamondCost( currentGoodId ) );

                MessageDispatcher.PostMessage( Constants.MessageType.RefreshCurrency );

                Action exit = () => { view.OnExit( false ); };
                MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, exit, UI.AlertType.ConfirmAlone, "礼物赠送成功", "提示" );
            }
        }

        private void HandleRelationListFeedback( byte[] data )
        {
            RelationListS2C feedback = ProtobufUtils.Deserialize<RelationListS2C>( data );

            if ( feedback.result )
            {
                friendList = feedback.friends;
                friendList.Sort( SortFriendInfo );
                view.RefreshFriendList();
            }
        }

        #endregion

        private static int SortFriendInfo( FriendInfo x, FriendInfo y )
        {
            if ( y.isOnline.CompareTo( x.isOnline ) != 0 )
            {
                return y.isOnline.CompareTo( x.isOnline );

            }
            else if ( y.grade.CompareTo( x.grade ) != 0 )
            {
                return y.grade.CompareTo( x.grade );
            }
            else if ( y.level.CompareTo( x.level ) != 0 )
            {
                return y.level.CompareTo( x.level );
            }
            else if ( y.name.CompareTo( x.name ) != 0 )
            {
                return y.name.CompareTo( x.name );
            }
            return 1;
        }

        private void OpenGainItemWindow( object exps, object currencies, object items, object soldierInfos )
        {
            int exp = (int)exps;
            List<Currency> currency = currencies as List<Currency>;
            List<ItemInfo> item = items as List<ItemInfo>;
            List<SoldierInfo> soldier = soldierInfos as List<SoldierInfo>;

            UI.UIManager.Instance.GetUIByType( UI.UIType.GainItemView, ( UI.ViewBase ui, System.Object obj ) => { ( ui as GainItemView ).OpenGainItemView( exp, currency, item, soldier, IsAloneBt( view.currentId ) ); } );
        }
    }
}
