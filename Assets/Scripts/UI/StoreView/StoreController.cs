using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Network;
using Utils;

namespace UI
{
    public enum StoreCostCurrecyType
    {
        None = 0,
        OnlyEmber = 1,
        OnlyDiamonds = 2,
        OnlyGold = 3,
        OnlySkinDebris = 4,
        GoldAndEmber = 5,
        GoldAndDiamonds = 6,
        OnlyUnitDebris = 7,
        OnlyRuneDebris = 8,
    }

    public class StoreController : ControllerBase
    {
        public class StoreItemData
        {
            public int index;
            public int icon;
            public int costLeft;
            public int costRight;
            public int currencyItemType;
            public string nameStr;
            public StoreItem.ButtonType btType;
            public StoreCostCurrecyType currencyType;
            public bool haveLimit;
            public int remainCount;

            public StoreItemData( int index, int icon, int costLeft, int costRight, int currencyItemType, string nameStr, StoreItem.ButtonType btType, StoreCostCurrecyType currencyType, bool haveLimit, int remainCount )
            {
                this.index = index;
                this.icon = icon;
                this.costLeft = costLeft;
                this.costRight = costRight;
                this.currencyItemType = currencyItemType;
                this.nameStr = nameStr;
                this.btType = btType;
                this.currencyType = currencyType;
                this.haveLimit = haveLimit;
                this.remainCount = remainCount;
            }
        }

        private StoreView view;

        private List<StoreProto.Store> storeProtosData;
        private List<RuneProto.Rune> runeProtosData;
        private List<ExchangeProto.Exchange> exchangeProtosData;
        private List<ItemBaseProto.ItemBase> itemBaseProtosData;

        private const int runePropDvalue = 604;
        private StoreInfo storeInfo;
        private StoreType currentStoreType;
        public ProfessionType currentUnitType = ProfessionType.FighterType;
        public ProfessionType currentSkinType = ProfessionType.FighterType;
        public int currentRuneType = 1;
        public int currentRuneLV = 1;
        public int currentDebrisType = 1;

        public StoreController( StoreView v )
        {
            viewBase = v;
            view = v;

            storeProtosData = DataManager.GetInstance().storeProtoData;
            runeProtosData = DataManager.GetInstance().runeProtoData;
            exchangeProtosData = DataManager.GetInstance().exchangeProtoData;
            itemBaseProtosData = DataManager.GetInstance().itemsProtoData;
        }

        public override void OnResume()
        {
            base.OnResume();
            NetworkManager.RegisterServerMessageHandler( MsgCode.StoreMessage, HandleStoreS2CFeedback );
        }

        public override void OnPause()
        {
            base.OnPause();
            NetworkManager.RemoveServerMessageHandler( MsgCode.StoreMessage, HandleStoreS2CFeedback );
        }

        #region Data UI

        private List<StoreProto.Store> GetItemIdList()
        {
            List<StoreProto.Store> storeList = new List<StoreProto.Store>();
            foreach ( StoreGoodInfo info in storeInfo.storeGoodInfos )
            {
                if ( currentStoreType == StoreType.Recommend )
                {
                    StoreProto.Store item = storeProtosData.Find( p => p.ID == info.id );

                    if ( item != null && item.Recommend == 1 )
                        storeList.Add( item );
                    else if ( item == null )
                        DebugUtils.LogError( DebugUtils.Type.UI, "Store Item is null , the store id is " + info.id );
                }
                else if ( currentStoreType == StoreType.Hero )
                {
                    StoreProto.Store item = storeProtosData.Find( p => p.ID == info.id );

                    if ( item != null && item.Profession == (int)currentUnitType )
                        storeList.Add( item );
                    else if ( item == null )
                        DebugUtils.LogError( DebugUtils.Type.UI, "Store Item is null , the store id is " + info.id );
                }
                else if ( currentStoreType == StoreType.Skin )
                {
                    StoreProto.Store item = storeProtosData.Find( p => p.ID == info.id );

                    if ( item != null && item.Profession == (int)currentSkinType )
                        storeList.Add( item );
                    else if ( item == null )
                        DebugUtils.LogError( DebugUtils.Type.UI, "Store Item is null , the store id is " + info.id );
                }
                else if ( currentStoreType == StoreType.Rune )
                {
                    StoreProto.Store item;
                    item = storeProtosData.Find( p => p.ID == info.id );

                    if ( item != null && item.RuneItemLeve == currentRuneLV && item.RuneType == currentRuneType )
                        storeList.Add( item );
                    else if ( item == null )
                        DebugUtils.LogError( DebugUtils.Type.UI, "Store Item is null , the store id is " + info.id );
                }
                else if ( currentStoreType == StoreType.Conversion )
                {
                    StoreProto.Store item;
                    item = storeProtosData.Find( p => p.ID == info.id );

                    if ( item != null && item.Type == currentDebrisType )
                        storeList.Add( item );
                    else if ( item == null )
                        DebugUtils.LogError( DebugUtils.Type.UI, "Store Item is null , the store id is " + info.id );
                }
                else
                {
                    StoreProto.Store item;
                    item = storeProtosData.Find( p => p.ID == info.id );

                    if ( item != null )
                        storeList.Add( item );
                    else if ( item == null )
                        DebugUtils.LogError( DebugUtils.Type.UI, "Store Item is null , the store id is " + info.id );
                }
            }

            return storeList;
        }

        public int GetItemCount()
        {
            if ( currentStoreType == StoreType.Conversion )
                return GetExchangeItemCount();
            return GetItemIdList().Count;
        }

        public int GetItemId( int itemIndex )
        {
            if ( currentStoreType == StoreType.Conversion )
                return GetExchangeItemId( itemIndex );
            return GetItemIdList()[itemIndex].ID;
        }

        public string GetItemName( int itemIndex )
        {
            if ( currentStoreType == StoreType.Conversion )
                return GetExchangeItemName( itemIndex );
            return GetItemIdList()[itemIndex].Name;
        }

        public string GetItemDescribe( int itemIndex )
        {
            if ( currentStoreType == StoreType.Conversion )
                return GetExchangeList()[itemIndex].Txt_ID.Localize();
            int id = GetItemIdList()[itemIndex].Txt_ID;

            return id.Localize();
        }

        public int GetItemIcon( int itemIndex )
        {
            if ( currentStoreType == StoreType.Conversion )
                return GetExchangeItemIcon( itemIndex );
            return GetItemIdList()[itemIndex].Icon;
        }

        public int GetItemCostLeft( int itemIndex )
        {
            if ( currentStoreType == StoreType.Conversion )
                return GetExchangeItemPrice( itemIndex );
            return GetItemIdList()[itemIndex].CurrencyValue1;
        }

        public int GetItemCostRight( int itemIndex )
        {
            if ( currentStoreType == StoreType.Conversion )
                return GetExchangeItemPrice( itemIndex );

            StoreProto.Store store = GetItemIdList()[itemIndex];
            if ( store.CurrencyValue2 == 0 )
                return store.CurrencyValue1;
            return store.CurrencyValue2;
        }

        private int GetItemQuantityLimit( int itemIndex )
        {
            if ( currentStoreType == StoreType.Conversion )
                return 0;
            return GetItemIdList()[itemIndex].QuantityLimit;
        }

        public bool ItemHaveLimit( int itemIndex )
        {
            if ( currentStoreType == StoreType.Conversion )
                return false;
            return ( GetItemQuantityLimit( itemIndex ) != 0 );
        }

        public int GetItemBoughtGoods( int itemIndex )
        {
            if ( currentStoreType == StoreType.Conversion )
                return 0;
            return GetItemQuantityLimit( itemIndex ) - storeInfo.storeGoodInfos[itemIndex].boughtGoods;
        }

        public StoreCostCurrecyType GetItemCostCurrencyType( int storeItemType, int itemIndex )
        {
            if ( storeItemType == 6 )
            {
                int exchangeType = exchangeProtosData.Find( p => p.ItemId == GetExchangeItemId( itemIndex ) ).ExchangeType;

                if ( exchangeType == 1 )
                    return StoreCostCurrecyType.OnlyUnitDebris;
                if ( exchangeType == 2 )
                    return StoreCostCurrecyType.OnlySkinDebris;
                if ( exchangeType == 3 )
                    return StoreCostCurrecyType.OnlyRuneDebris;
            }
            if ( GetItemIdList()[itemIndex].CurrencyType2 == 0 )
                switch ( (CurrencyType)GetItemIdList()[itemIndex].CurrencyType1 )
                {
                    case CurrencyType.GOLD:
                        return StoreCostCurrecyType.OnlyGold;
                    case CurrencyType.EMBER:
                        return StoreCostCurrecyType.OnlyEmber;
                    case CurrencyType.DIAMOND:
                        return StoreCostCurrecyType.OnlyDiamonds;
                }

            switch ( (CurrencyType)GetItemIdList()[itemIndex].CurrencyType2 )
            {
                case CurrencyType.EMBER:
                    return StoreCostCurrecyType.GoldAndEmber;
                case CurrencyType.DIAMOND:
                    return StoreCostCurrecyType.GoldAndDiamonds;
            }
            return StoreCostCurrecyType.None;
        }

        public CurrencyType GetSellItemCostType( int itemIndex )
        {
            return (CurrencyType)GetSellingItemList()[itemIndex].CurrencyType1;
        }

        #region Store Item

        public StoreItem.ButtonType GetStoreItemBtType( int storeItemType, int storeItemIndex )
        {
            if ( storeItemType == 6 )
                return StoreItem.ButtonType.OnlyExchange;

            if ( GetItemIdList()[storeItemIndex].IsGiving == 0 )
                return StoreItem.ButtonType.OnlyBuy;
            return StoreItem.ButtonType.BuyAndGive;
        }

        #endregion

        #region Rune Item

        public string GetRuneAttributeStr( int runeItemIndex )
        {
            string prop1 = "";
            string prop2 = "";
            string prop3 = "";
            string prop4 = "";

            int itemId = storeProtosData.Find( p => p.ID == GetItemId( runeItemIndex ) ).ItemId;

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

        public string GetRuneNameStr( int runeItemIndex )
        {
            int level = storeProtosData.Find( p => p.ID == GetItemId( runeItemIndex ) ).RuneItemLeve;
            return GetItemName( runeItemIndex ) + "<color=#7CFC00>LV:" + level + "</color>";
        }

        #endregion

        #region Selling Item

        private List<StoreProto.Store> GetSellingItemList()
        {
            List<StoreProto.Store> sellingList = new List<StoreProto.Store>();
            for ( int i = 0; i < storeInfo.storeGoodInfos.Count; i++ )
            {
                int storeId = storeInfo.storeGoodInfos[i].id;

                StoreProto.Store storeItem = storeProtosData.Find( p => p.ID == storeId && p.HotSell == 1 );

                if ( storeItem != null )
                    sellingList.Add( storeItem );
                else
                    DebugUtils.LogError( DebugUtils.Type.UI, "Store Item is null , the store id is " + storeId );
            }
            return sellingList;
        }

        public int GetSellingItemCount()
        {
            return GetSellingItemList().Count;
        }

        public int GetSellingItemPrice( int sellingIndex )
        {
            return GetSellingItemList()[sellingIndex].CurrencyValue1;
        }

        public int GetSellingItemIcon( int sellingIndex )
        {
            return GetSellingItemList()[sellingIndex].Icon;
        }

        public string GetSellingItemName( int sellingIndex )
        {
            return GetSellingItemList()[sellingIndex].Name;
        }

        #endregion

        #region Debris Item

        private List<ExchangeProto.Exchange> GetExchangeList()
        {
            List<ExchangeProto.Exchange> exchangeList = new List<ExchangeProto.Exchange>();

            exchangeList.AddRange( exchangeProtosData.FindAll( p => p.ExchangeType == currentDebrisType ) );

            return exchangeList;
        }

        private int GetExchangeItemCount()
        {
            return GetExchangeList().Count;
        }

        private int GetExchangeItemIcon( int debrisIndex )
        {
            return GetExchangeList()[debrisIndex].Icon;
        }

        private string GetExchangeItemName( int debrisIndex )
        {
            if ( currentDebrisType == 3 )
            {
                int level = itemBaseProtosData.Find( p => p.ID == GetExchangeItemId( debrisIndex ) ).Level;
                return "<size=30><color=#0095FF>" + GetExchangeList()[debrisIndex].Name + "</color>" + "<color=#7CFC00>LV:" + level + "</color></size>";
            }
            return GetExchangeList()[debrisIndex].Name;
        }

        public int GetExchangeId( int debrisIndex )
        {
            return GetExchangeList()[debrisIndex].ID;
        }

        private int GetExchangeItemId( int debrisIndex )
        {
            return GetExchangeList()[debrisIndex].ItemId;
        }

        private int GetExchangeItemPrice( int debrisIndex )
        {
            return GetExchangeList()[debrisIndex].DebrisQuantity;
        }

        private int GetExchangeItemDebrisItemId( int debrisIndex )
        {
            return GetExchangeList()[debrisIndex].DebrisType;
        }

        public int GetExchangeItemPrice()
        {
            if ( GetExchangeList().Count == 0 )
                return 0;
            int itemid = GetExchangeList()[0].DebrisType;
            PlayerBagInfo debrisBag = DataManager.GetInstance().GetPlayerBag( BagType.DebrisBag );
            if ( debrisBag.itemList.Find( p => p.itemId == itemid ) == null )
                return 0;
            return debrisBag.itemList.Find( p => p.itemId == itemid ).count;
        }

        public List<StoreItemData> GetStoreItemData_D()
        {
            List<StoreItemData> dataList = new List<UI.StoreController.StoreItemData>();

            for ( int i = 0; i < GetItemCount(); i++ )
            {
                int currentItemType = view.currentItemType;
                StoreItemData data = new StoreItemData( i, GetItemIcon( i ), GetItemCostLeft( i ), GetItemCostRight( i ), currentItemType, GetItemName( i ), GetStoreItemBtType( currentItemType, i ), GetItemCostCurrencyType( currentItemType, i ), ItemHaveLimit( i ), GetItemBoughtGoods( i ) );

                dataList.Add( data );
            }

            return dataList;
        }

        #endregion

        #endregion

        #region Send

        public void SendStoreC2S( StoreType type )
        {
            currentStoreType = type;
            if ( type == StoreType.Conversion )
                return;

            StoreC2S message = new StoreC2S();

            message.storeType = type;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.StoreMessage, data );
        }

        #endregion

        #region ReponseHandling

        private void HandleStoreS2CFeedback( byte[] data )
        {
            StoreS2C feedback = ProtobufUtils.Deserialize<StoreS2C>( data );

            if ( feedback.result )
            {
                storeInfo = feedback.storeInfo;

                if ( currentStoreType == StoreType.Rune )
                    view.RefreshRuneItem();
                else if ( currentStoreType == StoreType.Recommend )
                {
                    view.RefreshSellingItem();
                    view.RefreshStoreItem();
                }
                else
                    view.RefreshStoreItem();
            }
        }

        #endregion
    }
}
