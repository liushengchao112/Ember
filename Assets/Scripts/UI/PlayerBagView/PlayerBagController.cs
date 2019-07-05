using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Utils;
using Network;

namespace UI
{
    public class PlayerBagController : ControllerBase
    {
        private PlayerBagView view;
        private List<ItemBaseProto.ItemBase> itemProtoDatas;
        private List<LocalizationProto.Localization> localizationProtoDatas;
        private List<RuneProto.Rune> runeProtoDatas;

        #region Sell & Use Data

        private BagType currentBagType;
        private int currentCount;
        private long currentItemId;

        #endregion

        public PlayerBagController( PlayerBagView v )
        {
            viewBase = v;
            view = v;

            itemProtoDatas = DataManager.GetInstance().itemsProtoData;
            localizationProtoDatas = DataManager.GetInstance().localizationProtoData;
            runeProtoDatas = DataManager.GetInstance().runeProtoData;
        }

        public override void OnResume()
        {
            base.OnResume();
            NetworkManager.RegisterServerMessageHandler( MsgCode.SellItemMessage, HandleSellItemFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.UseItemMessage, HandleUseItemFeedback );
            MessageDispatcher.AddObserver( OpenGainItemWindow, Constants.MessageType.OpenGainItemWindow );
            MessageDispatcher.AddObserver( RefreshBagView, Constants.MessageType.RefreshBagView );
        }

        public override void OnPause()
        {
            base.OnPause();
            NetworkManager.RemoveServerMessageHandler( MsgCode.SellItemMessage, HandleSellItemFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.UseItemMessage, HandleUseItemFeedback );
            MessageDispatcher.RemoveObserver( OpenGainItemWindow, Constants.MessageType.OpenGainItemWindow );
            MessageDispatcher.RemoveObserver( RefreshBagView , Constants.MessageType.RefreshBagView );
        }

        private void RefreshBagView()
        {
            view.RefreshBagItem();
        }

        private void OpenGainItemWindow( object exps, object currencies, object items, object soldierInfos )
        {
            int exp = (int)exps;
            List<Currency> currency = currencies as List<Currency>;
            List<ItemInfo> item = items as List<ItemInfo>;
            List<SoldierInfo> soldier = soldierInfos as List<SoldierInfo>;

            UI.UIManager.Instance.GetUIByType( UI.UIType.GainItemView, ( UI.ViewBase ui, System.Object obj ) => { ( ui as GainItemView ).OpenGainItemView( exp, currency, item, soldier ); } );
        }

        #region Reponse Handle

        private void HandleSellItemFeedback( byte[] data )
        {
            SellItemS2C feedback = ProtobufUtils.Deserialize<SellItemS2C>( data );

            if ( feedback.result )
            {
                PlayerBagInfo bagInfo = DataManager.GetInstance().GetPlayerBag( currentBagType );
                ItemInfo item = bagInfo.itemList.Find( p => p.itemId == currentItemId );
                if ( item.count <= currentCount )
                    bagInfo.itemList.Remove( item );
                else
                    item.count -= currentCount;

                view.RefreshBagItem();
            }
        }

        private void HandleUseItemFeedback( byte[] data )
        {
            UseItemS2C feedback = ProtobufUtils.Deserialize<UseItemS2C>( data );

            if ( feedback.result )
            {
                PlayerBagInfo bagInfo = DataManager.GetInstance().GetPlayerBag( currentBagType );
                ItemInfo item = bagInfo.itemList.Find( p => p.itemId == currentItemId );
                if ( item.count <= currentCount )
                    bagInfo.itemList.Remove( item );
                else
                    item.count -= currentCount;

                view.RefreshBagItem();
            }
        }

        #endregion

        #region Send 

        public void SendSellItem( BagType type, int count, long itemId )
        {
            currentBagType = type;
            currentCount = count;
            currentItemId = itemId;

            SellItemC2S message = new SellItemC2S();

            message.bagType = type;
            message.count = count;
            message.itemId = itemId;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.SellItemMessage, data );
        }

        public void SendUseItem( BagType type, int count, long itemId )
        {
            currentBagType = type;
            currentCount = count;
            currentItemId = itemId;

            UseItemC2S message = new UseItemC2S();

            message.bagType = type;
            message.count = count;
            message.itemId = itemId;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.UseItemMessage, data );
        }

        #endregion

        #region Data UI

        public List<ItemInfo> GetPlayerBagList( BagType type )
        {
            if ( type == BagType.Bag )
                return GetAllPlayerBagList();
            return DataManager.GetInstance().GetPlayerBag( type ).itemList;
        }

        private List<ItemInfo> GetAllPlayerBagList()
        {
            List<ItemInfo> bagInfo = new List<ItemInfo>();

            bagInfo.AddRange( DataManager.GetInstance().GetPlayerBag( BagType.BoxBag ).itemList );
            bagInfo.AddRange( DataManager.GetInstance().GetPlayerBag( BagType.DebrisBag ).itemList );
            bagInfo.AddRange( DataManager.GetInstance().GetPlayerBag( BagType.ComplexBag ).itemList );
            bagInfo.AddRange( DataManager.GetInstance().GetPlayerBag( BagType.RuneBag ).itemList );

            return bagInfo;
        }

        public int GetPlayerBagListCount( BagType type )
        {
            return GetPlayerBagList( type ).Count;
        }

        public long GetPlayerBagItemId( BagType type, int index )
        {
            return GetPlayerBagList( type )[index].itemId;
        }

        public BagType GetPlayerBagItemType( BagType type, int index )
        {
            if ( type == BagType.Bag )
            {
                return (BagType)( itemProtoDatas.Find( p => p.ID == GetPlayerBagItemId( type, index ) ).Bag );
            }
            return type;
        }

        public int GetPlayerBagItemIcon( BagType type, int index )
        {
            int id = GetPlayerBagList( type )[index].metaId;

            if( itemProtoDatas.Find( p => p.ID == id ) == null )
                DebugUtils.LogError( DebugUtils.Type.UI, "PlayerBagItem is null , the item id is " + id );

            return itemProtoDatas.Find( p => p.ID == id ).Icon;
        }

        public int GetPlayerBagItemCount( BagType type, int index )
        {
            return GetPlayerBagList( type )[index].count;
        }

        public bool IsRune( BagType type, int index )
        {
            int id = GetPlayerBagList( type )[index].metaId;

            if ( itemProtoDatas.Find( p => p.ID == id ) == null )
                DebugUtils.LogError( DebugUtils.Type.UI, "PlayerBagItem is null , the item id is " + id );

            return itemProtoDatas.Find( p => p.ID == id ).Type == (int)PlayerBagItemType.RuneItem;
        }

        public bool IsLoudspeaker( BagType type , int index )
        {
            int id = GetPlayerBagList( type )[index].metaId;
            return itemProtoDatas.Find( p => p.ID == id ).GainType == 2;
        }

        public string GetPlayerBagItemName( BagType type, int index )
        {
            int id = GetPlayerBagList( type )[index].metaId;

            if ( itemProtoDatas.Find( p => p.ID == id ) == null )
                DebugUtils.LogError( DebugUtils.Type.UI, "PlayerBagItem is null , the item id is " + id );

            return itemProtoDatas.Find( p => p.ID == id ).Name;
        }

        public string GetPlayerBagItemDescribe( BagType type, int index )
        {
            int id = GetPlayerBagList( type )[index].metaId;

            if ( itemProtoDatas.Find( p => p.ID == id ) == null )
                DebugUtils.LogError( DebugUtils.Type.UI, "PlayerBagItem is null , the item id is " + id );

            int desId = itemProtoDatas.Find( p => p.ID == id ).DescriptionID;

            return desId.Localize();
        }
        
        public int GetPlayerBagItemPrice( BagType type, int index )
        {
            int id = GetPlayerBagList( type )[index].metaId;

            if ( itemProtoDatas.Find( p => p.ID == id ) == null )
                DebugUtils.LogError( DebugUtils.Type.UI, "PlayerBagItem is null , the item id is " + id );

            return itemProtoDatas.Find( p => p.ID == id ).Price;
        }

        public int GetPlayerBagItemIsShell( BagType type, int index )
        {
            int id = GetPlayerBagList( type )[index].metaId;

            if ( itemProtoDatas.Find( p => p.ID == id ).Type == (int)PlayerBagItemType.RuneItem )
                return 0;

            return itemProtoDatas.Find( p => p.ID == id ).IsShell;
        }

        public int GetPlayerBagItemUseType( BagType type, int index )
        {
            int id = GetPlayerBagList( type )[index].metaId;

            if ( itemProtoDatas.Find( p => p.ID == id ) == null )
                DebugUtils.LogError( DebugUtils.Type.UI, "PlayerBagItem is null , the item id is " + id );

            return itemProtoDatas.Find( p => p.ID == id ).UseType;
        }

        //Rune Data
        public string GetItemProp( BagType type, int index )
        {
            string prop = "";
            string prop1 = "", prop2 = "", prop3 = "", prop4 = "";

            int itemType = GetPlayerBagList( type )[index].itemType;
            if ( itemType == (int)PlayerBagItemType.RuneItem )
            {
                int id = GetPlayerBagList( type )[index].metaId;

                //Prop1
                int addition1 = runeProtoDatas.Find( p => p.ID == id ).Addition1;
                if ( addition1 != 0 )
                {
                    int propId1 = runeProtoDatas.Find( p => p.ID == id ).Property1;
                    string propStr1 = propId1.Localize();

                    float value1 = runeProtoDatas.Find( p => p.ID == id ).Value1;

                    prop1 = addition1 == 1 ? propStr1 + value1 : propStr1 + string.Format( "{0:P}", value1 );
                }
                //Prop2
                int addition2 = runeProtoDatas.Find( p => p.ID == id ).Addition2;
                if ( addition2 != 0 )
                {
                    int propId2 = runeProtoDatas.Find( p => p.ID == id ).Property2;
                    string propStr2 = propId2.Localize();

                    float value2 = runeProtoDatas.Find( p => p.ID == id ).Value2;

                    prop2 = addition2 == 1 ? propStr2 + value2 : propStr2 + string.Format( "{0:P}", value2 );
                }
                //Prop3
                int addition3 = runeProtoDatas.Find( p => p.ID == id ).Addition3;
                if ( addition3 != 0 )
                {
                    int propId3 = runeProtoDatas.Find( p => p.ID == id ).Property3;
                    string propStr3 = propId3.Localize();

                    float value3 = runeProtoDatas.Find( p => p.ID == id ).Value3;

                    prop3 = addition3 == 1 ? propStr3 + value3 : propStr3 + string.Format( "{0:P}", value3 );
                }
                //Prop4
                int addition4 = runeProtoDatas.Find( p => p.ID == id ).Addition4;
                if ( addition4 != 0 )
                {
                    int propId4 = runeProtoDatas.Find( p => p.ID == id ).Property4;
                    string propStr4 = propId4.Localize();

                    float value4 = runeProtoDatas.Find( p => p.ID == id ).Value4;

                    prop4 = addition4 == 1 ? propStr4 + value4 : propStr4 + string.Format( "{0:P}", value4 );
                }

                //Add prop to prop string
                prop = prop1 + "\r\n" + prop2 + "\r\n" + prop3 + "\r\n" + prop4;
            }
            return prop;
        }

        #endregion
    }
}
