using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Utils;
using Network;

namespace UI
{
    public class GainItemController : ControllerBase
    {
        private GainItemView view;

        private List<ItemBaseProto.ItemBase> itemBaseDatas;
        private List<ResourcesProto.Resources> resourcesDatas;
        private List<UnitsProto.Unit> unitDatas;

        public List<ItemInfo> itemDatas = new List<ItemInfo>();
        private List<Currency> currencyDatas = new List<Currency>();
        private List<SoldierInfo> soldierDatas = new List<SoldierInfo>();
        private int exp = 0;

        private int useItemId = -1;
        private int useItemCount = 0;
        private BagType useItemType;

        public GainItemController( GainItemView v )
        {
            viewBase = v;
            view = v;

            itemBaseDatas = DataManager.GetInstance().itemsProtoData;
            resourcesDatas = DataManager.GetInstance().resourcesProtoData;
            unitDatas = DataManager.GetInstance().unitsProtoData;
        }

        public override void OnResume()
        {
            base.OnResume();
            NetworkManager.RegisterServerMessageHandler( MsgCode.UseItemMessage, HandleUseItemFeedback );
            MessageDispatcher.AddObserver( OpenGainItemWindow, Constants.MessageType.OpenGainItemWindow );
        }

        public override void OnPause()
        {
            base.OnPause();
            NetworkManager.RemoveServerMessageHandler( MsgCode.UseItemMessage, HandleUseItemFeedback );
            MessageDispatcher.RemoveObserver( OpenGainItemWindow, Constants.MessageType.OpenGainItemWindow );
        }

        private void OpenGainItemWindow( object exps, object currencies, object items, object soldierInfos )
        {
            int exp = (int)exps;
            List<Currency> currency = currencies as List<Currency>;
            List<ItemInfo> itemList = items as List<ItemInfo>;
            List<SoldierInfo> soldierList = soldierInfos as List<SoldierInfo>;

            view.OpenGainItemView( exp, currency, itemList, soldierList );
        }

        #region Data UI

        public void SetItemDatas( int exp, List<Currency> currencyDatas, List<ItemInfo> itemDatas, List<SoldierInfo> soldierDatas )
        {
            this.currencyDatas = currencyDatas;
            this.itemDatas = itemDatas;
            this.soldierDatas = soldierDatas;
            this.exp = exp;

            view.RefreshItem();
        }

        public int GetItemsCount()
        {
            if ( exp > 0 )
                return ( currencyDatas.Count + itemDatas.Count + soldierDatas.Count ) + 1;
            return currencyDatas.Count + itemDatas.Count + soldierDatas.Count;
        }

        public int GetItemIcon( int index )
        {
            int itemIndex = 0;
            if ( index < currencyDatas.Count )
            {
                CurrencyType currencyType = currencyDatas[index].currencyType;
                if ( currencyType == CurrencyType.EMBER )
                    return 15908;
                else if ( currencyType == CurrencyType.GOLD )
                    return 15909;
                else
                    return 0;
            }

            if ( exp > 0 && index == currencyDatas.Count )
                return 50205;

            if ( itemDatas.Count > 0 )
            {
                itemIndex = exp > 0 ? index - currencyDatas.Count - 1 : index - currencyDatas.Count;
                return itemBaseDatas.Find( p => p.ID == itemDatas[itemIndex].itemId ).Icon;
            }

            int soldierIndex = exp > 0 ? index - currencyDatas.Count - itemDatas.Count - 1 : index - currencyDatas.Count - itemDatas.Count;
            return unitDatas.Find( p => p.ID == soldierDatas[soldierIndex].metaId ).Icon;
        }

        public int GetItemNum( int index )
        {
            if ( index < currencyDatas.Count )
                return currencyDatas[index].currencyValue;
            if ( exp > 0 && index == currencyDatas.Count )
                return exp;

            if ( itemDatas.Count > 0 )
            {
                int itemIndex = exp > 0 ? index - currencyDatas.Count - 1 : index - currencyDatas.Count;
                return itemDatas[itemIndex].count;
            }

            int soldierIndex = exp > 0 ? index - currencyDatas.Count - itemDatas.Count - 1 : index - currencyDatas.Count - itemDatas.Count;
            return soldierDatas[soldierIndex].count;
        }

        public string GetItemName( int index )
        {
            if ( index < currencyDatas.Count )
            {
                CurrencyType currencyType = currencyDatas[index].currencyType;
                if ( currencyType == CurrencyType.EMBER )
                    return "余烬币";
                else if ( currencyType == CurrencyType.GOLD )
                    return "金币";
                else
                    return "";
            }
            if ( exp > 0 && index == currencyDatas.Count )
                return "EXP";

            if ( itemDatas.Count > 0 )
            {
                int itemIndex = exp > 0 ? index - currencyDatas.Count - 1 : index - currencyDatas.Count;
                return itemBaseDatas.Find( p => p.ID == itemDatas[itemIndex].itemId ).Name;
            }

            int soldierIndex = exp > 0 ? index - currencyDatas.Count - itemDatas.Count - 1 : index - currencyDatas.Count - itemDatas.Count;
            return unitDatas.Find( p => p.ID == soldierDatas[soldierIndex].metaId ).Name;
        }

        #endregion

        #region Send

        public void SendUseItemC2S( BagType bagType, int count, int itemid )
        {
            useItemId = itemid;
            useItemCount = count;
            useItemType = bagType;

            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );
            UseItemC2S message = new UseItemC2S();

            message.bagType = BagType.BoxBag;
            message.count = count;
            message.itemId = itemid;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.UseItemMessage, data );
        }

        #endregion

        #region ReponseHandling

        private void HandleUseItemFeedback( byte[] data )
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.Normal );
            UseItemS2C feedback = ProtobufUtils.Deserialize<UseItemS2C>( data );

            if ( feedback.result )
            {
                PlayerBagInfo bagInfo = DataManager.GetInstance().GetPlayerBag( useItemType );
                ItemInfo item = bagInfo.itemList.Find( p => p.itemId == useItemId );
                if ( item.count <= useItemCount )
                    bagInfo.itemList.Remove( item );
                else
                    item.count -= useItemCount;
            }
        }

        #endregion
    }
}
