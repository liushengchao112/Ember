using UnityEngine;
using System.Collections.Generic;
using System;

using Data;
using Utils;
using Network;

namespace UI
{
    public enum BlackMarketType
    {
        Buy = 1,
        Sell = 2,
        ExChange = 3
    }
    public class BlackMarketController : ControllerBase
    {
        private BlackMarketView view;
        //private GearProto gearProto;
        //private ComponentsProto componentProto;
        private List<LocalizationProto.Localization> localizationProto;

        public List<ItemInfo> exChangeItemList = new List<ItemInfo>() { new ItemInfo(), new ItemInfo(), new ItemInfo(), new ItemInfo(), new ItemInfo(), new ItemInfo() };

        public BlackMarketController( BlackMarketView v )
        {
            viewBase = v;
            view = v;

            //blackMarketPriceProto = DataManager.GetInstance().blackMarketPriceProtoData;
            //blackMarketTradingProto = DataManager.GetInstance().blackMarketTradingProtoData;
            //gearProto = DataManager.GetInstance().gearProtoData;
            //componentProto = DataManager.GetInstance().componentsProtoData;
            localizationProto = DataManager.GetInstance().localizationProtoData;
        }

        public override void OnResume()
        {
            base.OnResume();
            NetworkManager.RegisterServerMessageHandler( MsgCode.BlackMarketBuyMessage, HandleBuyGoodsFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.BlackMarketSellMessage, HandleSellGoodsFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.BlackMarketRefreshMessage, HandleRefreshGoodsFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.BlackMarketTradingMessage, HandleExChangeGoodsFeedback );
        }

        public override void OnPause()
        {
            base.OnPause();
            NetworkManager.RemoveServerMessageHandler( MsgCode.BlackMarketBuyMessage, HandleBuyGoodsFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.BlackMarketSellMessage, HandleSellGoodsFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.BlackMarketRefreshMessage, HandleRefreshGoodsFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.BlackMarketTradingMessage, HandleExChangeGoodsFeedback );
        }

        #region Data UI

        public bool HaveEnoughCurrencyBuyGoods( int goodsIndex )
        {
            bool haveEnough = false;

            int blackMarketId = DataManager.GetInstance().GetPlayerBlackMarketInfo().slots[goodsIndex].blackMarketPriceId;

            //CurrencyType currencyType = (CurrencyType)blackMarketPriceProto.blackMarketPrices.Find( p => p.ID == blackMarketId ).CurrencyType;

            //switch ( currencyType )
            //{
            //    case CurrencyType.GOLD:
            //        haveEnough = ( blackMarketPriceProto.blackMarketPrices.Find( p => p.ID == blackMarketId ).CurrencyValue ) <= ( DataManager.GetInstance().GetPlayerGold() );
            //        break;
            //    case CurrencyType.EMBER:
            //        haveEnough = ( blackMarketPriceProto.blackMarketPrices.Find( p => p.ID == blackMarketId ).CurrencyValue ) <= ( DataManager.GetInstance().GetPlayerEmber() );
            //        break;
            //    case CurrencyType.DIAMOND:

            //        break;
            //}
            if ( !haveEnough )
            {
                string notGold = "The Currency is not enough";
                string titleText = "Prompt";
                MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, AlertType.ConfirmAlone, notGold, titleText );
            }
            return haveEnough;
        }

        public bool HaveEnoughCurrencyRefresh()
        {
            bool haveEnough = false;
            haveEnough = ( DataManager.GetInstance().GetPlayerBlackMarketInfo().consumeCurrency ) <= ( DataManager.GetInstance().GetPlayerGold() );

            if ( !haveEnough )
            {
                string notGold = "The Gold is not enough";
                string titleText = "Prompt";
                MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, AlertType.ConfirmAlone, notGold, titleText );
            }

            return haveEnough;
        }

        //Goods Data
        public List<ItemInfo> GetBagGoodsList( PlayerBagItemType bagType )
        {
            List<ItemInfo> itemList = new List<ItemInfo>();
            //PlayerBagInfo gearBag = DataManager.GetInstance().GetPlayerBag( BagType.GearBag );
            //PlayerBagInfo itemBag = DataManager.GetInstance().GetPlayerBag( BagType.ItemBag );

            //switch ( bagType )
            //{
            //    case PlayerBagItemType.Component:
            //    case PlayerBagItemType.ExpItem:
            //    case PlayerBagItemType.Nem:
            //    case PlayerBagItemType.BluePrintItem:
            //        itemList = itemBag.itemList.FindAll( p => p.itemType == (int)bagType );
            //        break;
            //    case PlayerBagItemType.GearItem:
            //        itemList = gearBag.itemList;
            //        break;
            //}

            return itemList;
        }

        public int GetGoodsCount( BlackMarketType type, PlayerBagItemType bagType )
        {
            int count = 0;
            switch ( type )
            {
                case BlackMarketType.Buy:
                    count = DataManager.GetInstance().GetPlayerBlackMarketInfo().slots.Count;
                    break;
                case BlackMarketType.Sell:
                case BlackMarketType.ExChange:
                    count = GetBagGoodsList( bagType ).Count;
                    break;
            }
            return count;
        }

        public int GetGoodsItemIcon( BlackMarketType type, PlayerBagItemType bagType, int goodsIndex )
        {
            int icon = 0;
            int itemId;
            int goodsType;

            if ( type == BlackMarketType.Buy )
            {
                int blackMarketId = DataManager.GetInstance().GetPlayerBlackMarketInfo().slots[goodsIndex].blackMarketPriceId;
                //itemId = blackMarketPriceProto.blackMarketPrices.Find( p => p.ID == blackMarketId ).ItemID;
                //goodsType = blackMarketPriceProto.blackMarketPrices.Find( p => p.ID == blackMarketId ).Type;
            }
            else
            {
                itemId = GetBagGoodsList( bagType )[goodsIndex].metaId;
                //goodsType = blackMarketPriceProto.blackMarketPrices.Find( p => p.ItemID == itemId ).Type;
            }

            //switch ( (PlayerBagItemType)goodsType )
            //{
            //    case PlayerBagItemType.ItemNone:
            //        break;
            //    case PlayerBagItemType.Component:
            //    case PlayerBagItemType.ExpItem:
            //    case PlayerBagItemType.Nem:
            //    case PlayerBagItemType.BluePrintItem:
            //        icon = componentProto.components.Find( p => p.ID == itemId ).Icon;
            //        break;
            //    case PlayerBagItemType.GearItem:
            //        icon = gearProto.gears.Find( p => p.ID == itemId ).Icon;
            //        break;
            //    default:
            //        DebugUtils.Log( DebugUtils.Type.UI, "the goods type is wrong~~" );
            //        break;
            //}

            if ( icon == 0 ) DebugUtils.Log( DebugUtils.Type.UI, "Read Excel is wrong or the Excel is wrong~~~" );

            return icon;
        }

        public int GetGoodsItemQuantity( BlackMarketType type, PlayerBagItemType bagType, int goodsIndex )
        {
            if ( type == BlackMarketType.Buy )
                return 1;

            int itemId = GetBagGoodsList( bagType )[goodsIndex].metaId;
            //int goodsType = blackMarketPriceProto.blackMarketPrices.Find( p => p.ItemID == itemId ).Type;

            //PlayerBagInfo gearBag = DataManager.GetInstance().GetPlayerBag( BagType.GearBag );
            //PlayerBagInfo itemBag = DataManager.GetInstance().GetPlayerBag( BagType.ItemBag );

            int count = 0;

            //switch ( (PlayerBagItemType)goodsType )
            //{
            //    case PlayerBagItemType.ItemNone:
            //        break;
            //    case PlayerBagItemType.Component:
            //    case PlayerBagItemType.ExpItem:
            //    case PlayerBagItemType.Nem:
            //    case PlayerBagItemType.BluePrintItem:
            //        count = itemBag.itemList.Find( p => p.metaId == itemId ).count;

            //        if ( type == BlackMarketType.ExChange )
            //        {
            //            foreach ( ItemInfo item in exChangeItemList )
            //            {
            //                if ( item.metaId == GetBagGoodsList( bagType )[goodsIndex].metaId )
            //                    count--;
            //            }
            //        }
            //        break;
            //    case PlayerBagItemType.GearItem:
            //        count = gearBag.itemList.Find( p => p.metaId == itemId ).count;

            //        if ( type == BlackMarketType.ExChange )
            //        {
            //            foreach ( ItemInfo item in exChangeItemList )
            //            {
            //                if ( item.itemId == GetBagGoodsList( bagType )[goodsIndex].itemId )
            //                    count--;
            //            }
            //        }
            //        break;

            //    default:
            //        DebugUtils.Log( DebugUtils.Type.UI, "the goods type is wrong~~" );
            //        break;
            //}
            if ( count < 0 )
                return 0;

            return count;
        }

        public CurrencyType GetGoodsItemCurrencyType( BlackMarketType type, PlayerBagItemType bagType, int goodsIndex )
        {
            if ( type == BlackMarketType.Buy )
            {
                int blackMarketId = DataManager.GetInstance().GetPlayerBlackMarketInfo().slots[goodsIndex].blackMarketPriceId;
                //return (CurrencyType)( blackMarketPriceProto.blackMarketPrices.Find( p => p.ID == blackMarketId ).CurrencyType );
            }

            int itemId = GetBagGoodsList( bagType )[goodsIndex].metaId;
            int costType = 1;
            //costType = blackMarketPriceProto.blackMarketPrices.Find( p => p.ItemID == itemId ).CurrencyType;

            return (CurrencyType)costType;
        }

        public int GetGoodsItemCost( BlackMarketType type, PlayerBagItemType bagType, int goodsIndex )
        {
            if ( type == BlackMarketType.Buy )
            {
                int blackMarketId = DataManager.GetInstance().GetPlayerBlackMarketInfo().slots[goodsIndex].blackMarketPriceId;
               // return blackMarketPriceProto.blackMarketPrices.Find( p => p.ID == blackMarketId ).CurrencyValue;
            }

            int itemId = GetBagGoodsList( bagType )[goodsIndex].metaId;
            int goodsType = GetBagGoodsList( bagType )[goodsIndex].itemType;

            //int cost = ( blackMarketPriceProto.blackMarketPrices.FindAll( p => p.Type == goodsType ).Find( p => p.ItemID == itemId ).CurrencyValue ) / 3;

            return 1;
        }

        public string GetGoodsItemName( BlackMarketType type, PlayerBagItemType bagType, int goodsIndex )
        {
            if ( type == BlackMarketType.Buy )
            {
                int blackMarketId = DataManager.GetInstance().GetPlayerBlackMarketInfo().slots[goodsIndex].blackMarketPriceId;
                //return blackMarketPriceProto.blackMarketPrices.Find( p => p.ID == blackMarketId ).Name;
            }

            int itemId = GetBagGoodsList( bagType )[goodsIndex].metaId;
            //int goodsType = blackMarketPriceProto.blackMarketPrices.Find( p => p.ItemID == itemId ).Type;

            string name = "";

            //switch ( (PlayerBagItemType)goodsType )
            //{
            //    case PlayerBagItemType.ItemNone:
            //        break;
            //    case PlayerBagItemType.Component:
            //    case PlayerBagItemType.ExpItem:
            //    case PlayerBagItemType.Nem:
            //    case PlayerBagItemType.BluePrintItem:
            //        name = componentProto.components.Find( p => p.ID == itemId ).Name;
            //        break;
            //    case PlayerBagItemType.GearItem:
            //        name = gearProto.gears.Find( p => p.ID == itemId ).Name;
            //        break;

            //    default:
            //        DebugUtils.Log( DebugUtils.Type.UI, "the goods type is wrong~~" );
            //        break;
            //}
            return name;
        }

        public string GetGoodsItemType( BlackMarketType type, PlayerBagItemType bagType, int goodsIndex )
        {
            string typeString = "";
            if ( type == BlackMarketType.Buy )
            {
                int blackMarketId = DataManager.GetInstance().GetPlayerBlackMarketInfo().slots[goodsIndex].blackMarketPriceId;
                //int itemId = blackMarketPriceProto.blackMarketPrices.Find( p => p.ID == blackMarketId ).ItemID;
                //int goodsType = blackMarketPriceProto.blackMarketPrices.Find( p => p.ID == blackMarketId ).Type;
                //typeString = ( (PlayerBagItemType)goodsType ).ToString();
            }
            else
            {
                typeString = bagType.ToString();
            }
            return typeString;
        }

        public string GetGoodsItemDescription( BlackMarketType type, PlayerBagItemType bagType, int goodsIndex )
        {
            int itemId;
            int goodsType;
            int descriptionId = 0;

            if ( type == BlackMarketType.Buy )
            {
                int blackMarketId = DataManager.GetInstance().GetPlayerBlackMarketInfo().slots[goodsIndex].blackMarketPriceId;
                ///itemId = blackMarketPriceProto.blackMarketPrices.Find( p => p.ID == blackMarketId ).ItemID;
                //goodsType = blackMarketPriceProto.blackMarketPrices.Find( p => p.ID == blackMarketId ).Type;
            }
            else
            {
                itemId = GetBagGoodsList( bagType )[goodsIndex].metaId;
                //goodsType = blackMarketPriceProto.blackMarketPrices.Find( p => p.ItemID == itemId ).Type;
            }
            //switch ( (PlayerBagItemType)goodsType )
            //{
            //    case PlayerBagItemType.ItemNone:
            //        break;
            //    case PlayerBagItemType.Component:
            //    case PlayerBagItemType.ExpItem:
            //    case PlayerBagItemType.Nem:
            //    case PlayerBagItemType.BluePrintItem:
            //        return componentProto.components.Find( p => p.ID == itemId ).Name;
            //    case PlayerBagItemType.GearItem:
            //        descriptionId = gearProto.gears.Find( p => p.ID == itemId ).Description;
            //        break;
            //    default:
            //        break;
            //}
            return localizationProto.Find( p => p.ID == descriptionId ).Chinese;
        }

        public bool GetGoodsIsSoldOut( BlackMarketType type, int goodsIndex )
        {
            bool isSellOut = false;
            switch ( type )
            {
                case BlackMarketType.Buy:
                    isSellOut = DataManager.GetInstance().GetPlayerBlackMarketInfo().slots[goodsIndex].isSellOut;
                    break;
                case BlackMarketType.Sell:
                case BlackMarketType.ExChange:
                    isSellOut = false;
                    break;
            }
            return isSellOut;
        }

        //Buy Bottom Data
        public int GetRefreshGoodsItemCount()
        {
            return DataManager.GetInstance().GetPlayerBlackMarketInfo().consumeCurrency;
        }

        public long GetTimeStamp()
        {
            TimeSpan times = DateTime.UtcNow - new DateTime( 1970, 1, 1, 0, 0, 0 );

            return Convert.ToInt64( times.TotalSeconds );
        }

        bool canSendRefresh = true;
        public string GetTimeString()
        {
            long nextTime = DataManager.GetInstance().GetPlayerBlackMarketInfo().nextRefreshTime;
            int time = (int)( nextTime - GetTimeStamp() );

            if ( time <= 0 && canSendRefresh )
            {
                SendRefreshC2S();
                canSendRefresh = false;
            }

            if ( time <= 0 ) return "Refresh in: 00:00:00";
            return string.Format( "Refresh in:" + "{0:D2}", time / 3600 ) + ":" + string.Format( "{0:D2}", ( time % 3600 ) / 60 ) + ":" + string.Format( "{0:D2}", time % 60 );
        }

        //ExChange List
        public bool CanOpenChest()
        {
            foreach ( ItemInfo item in exChangeItemList )
            {
                if ( item.itemId == 0 ) return false;
            }
            return true;
        }

        public float[] GetExChangeRate( List<ItemInfo> itemList )
        {
            float[] rates = new float[4] { 0f, 0f, 0f, 0f };

            foreach ( ItemInfo item in itemList )
            {
                if ( item.metaId == 0 ) continue;
                //int quality = gearProto.gears.Find( p => p.ID == item.metaId ).Quality;

                //float comValue = blackMarketTradingProto.blackMarketTradings.Find( p => p.Quality == quality ).ComValue;
                //float rareValue = blackMarketTradingProto.blackMarketTradings.Find( p => p.Quality == quality ).RareValue;
                //float epicValue = blackMarketTradingProto.blackMarketTradings.Find( p => p.Quality == quality ).EpicValue;
                //float mythicValue = blackMarketTradingProto.blackMarketTradings.Find( p => p.Quality == quality ).MythicValue;

                //rates[0] += comValue;
                //rates[1] += rareValue;
                //rates[2] += epicValue;
                //rates[3] += mythicValue;
            }
            return rates;
        }

        #endregion

        #region Send

        public void SendSellC2S( int goodsIndex, PlayerBagItemType currentGoodsType, PlayerBagItemType bagType, int sellCount )
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );
            long itemId = 0L;

            //switch ( currentGoodsType )
            //{
            //    case PlayerBagItemType.Nem:
            //    case PlayerBagItemType.Component:
            //    case PlayerBagItemType.BluePrintItem:
            //        itemId = GetBagGoodsList( bagType )[goodsIndex].metaId;
            //        break;
            //    case PlayerBagItemType.GearItem:
            //        itemId = GetBagGoodsList( bagType )[goodsIndex].itemId;
            //        break;
            //    default:
            //        DebugUtils.Log( DebugUtils.Type.UI, "Goods Type is wrong~~~" );
            //        break;
            //}

            //BlackMarketSellC2S blackMarketSellData = new BlackMarketSellC2S();

           // blackMarketSellData.itemId = itemId;
            //blackMarketSellData.itemType = currentGoodsType;
           // blackMarketSellData.sellCount = sellCount;

           // byte[] data = ProtobufUtils.Serialize( blackMarketSellData );

            //NetworkManager.SendRequest( MsgCode.BlackMarketSellMessage, data );
        }

        public void SendBuyC2S( int goodsIndex )
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

            int blackMarketId = 0;

            blackMarketId = DataManager.GetInstance().GetPlayerBlackMarketInfo().slots[goodsIndex].blackMarketPriceId;

           // BlackMarketBuyC2S blackMarketBuyData = new BlackMarketBuyC2S();

            //blackMarketBuyData.blackMarketPriceId = blackMarketId;
           // blackMarketBuyData.slotId = goodsIndex + 1;

           // byte[] data = ProtobufUtils.Serialize( blackMarketBuyData );

           // NetworkManager.SendRequest( MsgCode.BlackMarketBuyMessage, data );
        }

        public void SendOpenChestC2S()
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

            List<long> itemId = new List<long>() { 0L, 0L, 0L, 0L, 0L, 0L };

            for ( int i = 0; i < exChangeItemList.Count; i++ )
            {
                itemId[i] = exChangeItemList[i].itemId;
            }

           // BlackMarketTradingC2S blackMarketTradingData = new BlackMarketTradingC2S();

           // blackMarketTradingData.gearIds.AddRange( itemId );

          //  byte[] data = ProtobufUtils.Serialize( blackMarketTradingData );

          //  NetworkManager.SendRequest( MsgCode.BlackMarketTradingMessage, data );
        }

        public void SendRefreshC2S()
        {
          //  BlackMarketRefreshC2S blackMarketRefreshData = new BlackMarketRefreshC2S();

           // byte[] data = ProtobufUtils.Serialize( blackMarketRefreshData );
           // NetworkManager.SendRequest( MsgCode.BlackMarketRefreshMessage, data );
        }

        #endregion

        #region ReponseHandling

        private void HandleBuyGoodsFeedback( byte[] data )
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.Normal );

            //BlackMarketBuyS2C feedback = ProtobufUtils.Deserialize<BlackMarketBuyS2C>( data );
           // if ( feedback.result )
            {
                //DataManager.GetInstance().GetPlayerBlackMarketInfo().slots[feedback.slotId - 1].isSellOut = true;

                view.SetDescriptionUI( false );
                view.SetButton( false );
                view.RefreshGoodsItem();
            }
        }

        private void HandleSellGoodsFeedback( byte[] data )
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.Normal );

           // BlackMarketSellS2C feedback = ProtobufUtils.Deserialize<BlackMarketSellS2C>( data );
           // if ( feedback.result )
            {
               // int count = feedback.sellCount;
               // long itemId = feedback.itemId;

                //PlayerBagInfo itemBag = DataManager.GetInstance().GetPlayerBag( BagType.ItemBag );
                //PlayerBagInfo gearBag = DataManager.GetInstance().GetPlayerBag( BagType.GearBag );

                //switch ( feedback.itemType )
                //{
                //    case PlayerBagItemType.Component:
                //    case PlayerBagItemType.ExpItem:
                //    case PlayerBagItemType.Nem:
                //    case PlayerBagItemType.BluePrintItem:
                //        itemBag.itemList.Find( p => p.metaId == (int)itemId ).count -= count;
                //        break;
                //    case PlayerBagItemType.GearItem:
                //        gearBag.itemList.Remove( gearBag.itemList.Find( p => p.itemId == itemId ) );
                //        break;

                //    default:
                //        DebugUtils.Log( DebugUtils.Type.Data, "Sell Goods Type is wrong~~~" );
                //        break;
                //}

                view.RefreshGoodsItem();
            }
        }

        public void HandleRefreshGoodsFeedback( byte[] data )
        {
            canSendRefresh = true;
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.Normal );

           // BlackMarketRefreshS2C feedback = ProtobufUtils.Deserialize<BlackMarketRefreshS2C>( data );
           // if ( feedback.result )
            {
               // DataManager.GetInstance().SetPlayerBlackMarketInfo( feedback.blackMarketInfo );

                view.RefreshGoodsItem();
            }
        }

        public void HandleExChangeGoodsFeedback( byte[] data )
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.Normal );

           // BlackMarketTradingS2C feedback = ProtobufUtils.Deserialize<BlackMarketTradingS2C>( data );
           // if ( feedback.result )
            {
                List<ItemInfo> newItemList = new List<ItemInfo>();
                //PlayerBagInfo gearBag = DataManager.GetInstance().GetPlayerBag( BagType.GearBag );
                //newItemList.Add( gearBag.itemList.Find( p => p.itemId == feedback.gearId ) );

                //MessageDispatcher.PostMessage( Constants.MessageType.OpenChestWindow, newItemList, new List<int>() { 1 }, ChestItemType.Item );

                view.ClearExChangeList();
                view.RefreshGoodsItem();
                view.RefreshExChangeGoods();
            }
        }

        #endregion

    }
}
