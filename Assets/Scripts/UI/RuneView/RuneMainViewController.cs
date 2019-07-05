using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Data;
using Network;

namespace UI
{
	public class RuneMainViewController : ControllerBase
	{
		private RuneMainView _view;

		private List<RuneProto.Rune> runes;
		private List<RunePageInfo> runePages;
		private List<LocalizationProto.Localization> localizationProtoDatas;
        private List<SlotProto.Slot> listSolt;
		private List<StoreGoodInfo> goods;
		public PlayerBagInfo runeBag;
       
		private int currentRuneId;
		private int currentRunePageId;
		private string currentRunePageName;
		private int currentCount;
		private bool isUnlock = false;

		public RuneMainViewController( RuneMainView v )
		{
			viewBase = v;
			_view = v;
		}

		public void InitData()
		{        
			runeBag = DataManager.GetInstance ().GetPlayerBag (BagType.RuneBag );
            runes = DataManager.GetInstance ().runeProtoData;
			localizationProtoDatas = DataManager.GetInstance ().localizationProtoData;
            listSolt = DataManager.GetInstance().slotProtoData;
            runePages = DataManager.GetInstance ().GetRunePageList ();
			SendBoughtNumber ();
		}

		public override void OnResume()
		{
			InitData ();			
			NetworkManager.RegisterServerMessageHandler ( MsgCode.EquipRuneMessage , HandleEquipRuneFeedback );
			NetworkManager.RegisterServerMessageHandler ( MsgCode.UnEquipRuneMessage , HandleUnEquipRuneFeedback );
			NetworkManager.RegisterServerMessageHandler ( MsgCode.SellItemMessage , HandleSellRuneFeedback );
			NetworkManager.RegisterServerMessageHandler ( MsgCode.StoreBuyMessage , HandleRuneBuyFeedback );
			NetworkManager.RegisterServerMessageHandler ( MsgCode.UnlockRuneMessage , HandleUnlockRuneSlotFeedback );
			NetworkManager.RegisterServerMessageHandler ( MsgCode.BuyRunePageMessage , HandleBuyRunePageFeedback );
			NetworkManager.RegisterServerMessageHandler ( MsgCode.RefreshRunePageMessage , HandleRefreshRunePageFeedback );
			NetworkManager.RegisterServerMessageHandler ( MsgCode.RenameRunePageNameMessage , HandleRunePageRenameFeedback );
			NetworkManager.RegisterServerMessageHandler ( MsgCode.StoreMessage , HandleBoughtNumberFeedback );
		}

		public override void OnPause()
		{
			NetworkManager.RemoveServerMessageHandler ( MsgCode.EquipRuneMessage , HandleEquipRuneFeedback );
			NetworkManager.RemoveServerMessageHandler ( MsgCode.UnEquipRuneMessage , HandleUnEquipRuneFeedback );
			NetworkManager.RemoveServerMessageHandler ( MsgCode.SellItemMessage , HandleSellRuneFeedback );
			NetworkManager.RemoveServerMessageHandler ( MsgCode.StoreBuyMessage , HandleRuneBuyFeedback );
			NetworkManager.RemoveServerMessageHandler ( MsgCode.UnlockRuneMessage , HandleUnlockRuneSlotFeedback );
			NetworkManager.RemoveServerMessageHandler ( MsgCode.BuyRunePageMessage , HandleBuyRunePageFeedback );
			NetworkManager.RemoveServerMessageHandler ( MsgCode.RefreshRunePageMessage , HandleRefreshRunePageFeedback );
			NetworkManager.RemoveServerMessageHandler ( MsgCode.RenameRunePageNameMessage , HandleRunePageRenameFeedback );
			NetworkManager.RemoveServerMessageHandler ( MsgCode.StoreMessage , HandleBoughtNumberFeedback );
		}

		#region Send

		public void SendEquipRuneItem( int pageId, int itemId, int slotId )
		{
			currentRuneId = itemId;
			currentCount = 1;
			isUnlock = false;
			EquipRuneC2S equipRuneData = new EquipRuneC2S ();
			equipRuneData.pageId = pageId;
			equipRuneData.itemId = itemId;
			equipRuneData.slotId = slotId;
			byte[] stream = ProtobufUtils.Serialize ( equipRuneData );
			NetworkManager.SendRequest ( MsgCode.EquipRuneMessage, stream );
		}

		public void SendUnEquipRuneItem( int pageId, int itemId, int slotId )
		{
			currentRuneId = itemId;
			currentCount = 1;
			isUnlock = false;
			UnEquipRuneC2S equipRuneData = new UnEquipRuneC2S ();
			equipRuneData.pageId = pageId;
            equipRuneData.slotId = slotId;
			byte[] stream = ProtobufUtils.Serialize ( equipRuneData );
			NetworkManager.SendRequest ( MsgCode.UnEquipRuneMessage, stream );

		}

        private int sellcount;
		public void SendSellRune( int count, int itemId )
		{
            sellcount = count;
            currentRuneId = itemId;
			SellItemC2S message = new SellItemC2S ();
			message.bagType = BagType.RuneBag;
			message.count = count;
			message.itemId = itemId;
            byte[] data = ProtobufUtils.Serialize ( message );
			NetworkManager.SendRequest ( MsgCode.SellItemMessage, data );
		}

		public void SendRuneBuy( int count, int itemId, CurrencyType type )
		{
			currentRuneId = itemId;
			currentCount = count;
			StorePurchaseC2S message = new StorePurchaseC2S ();

			message.currencyType = type;
			message.count = count;
			message.goodId = GetStoreItemId ( itemId );

			byte[] data = ProtobufUtils.Serialize ( message );
			NetworkManager.SendRequest ( MsgCode.StoreBuyMessage, data );
		}

		public void SendBuyRunePage( int pageId )
		{
			BuyRunePageC2S message = new BuyRunePageC2S ();
			message.pageId = pageId;
			byte[] data = ProtobufUtils.Serialize ( message );
			NetworkManager.SendRequest ( MsgCode.BuyRunePageMessage, data );
		}

		public void SendRunePageRename( int pageId, string runePageName )
		{
			currentRunePageId = pageId;
			currentRunePageName = runePageName;

			RunePageRenameC2S message = new RunePageRenameC2S ();
			message.pageId = pageId;
			message.pageName = runePageName;
			byte[] data = ProtobufUtils.Serialize ( message );
			NetworkManager.SendRequest ( MsgCode.RenameRunePageNameMessage, data );
		}

		public void SendUnlockRuneSlot( int pageId, int slotId, int type )
		{
			UnlockRuneSlotC2S message = new UnlockRuneSlotC2S ();
			message.pageId = pageId;
			message.type = type;
			message.slotId = slotId;
			isUnlock = true;
			byte[] data = ProtobufUtils.Serialize ( message );
			NetworkManager.SendRequest ( MsgCode.UnlockRuneMessage, data );
		}

		public void SendBoughtNumber()
		{
			StoreC2S message = new StoreC2S();
			message.storeType = StoreType.Rune;
			byte[] data = ProtobufUtils.Serialize ( message );
			NetworkManager.SendRequest ( MsgCode.StoreMessage, data );
		}

		#endregion

		#region Reponse Handle
		private void HandleEquipRuneFeedback( byte[] data )
		{
			EquipRuneS2C feedback = ProtobufUtils.Deserialize<EquipRuneS2C> ( data );
            if ( feedback.result )
			{
				ReduceRune ();
                _view.LoadRunePageItem();
                _view.LoadRunePackItem();
				_view.ShowRunePageInformationPanel ();
                if (_view.runeController!=null)
                {
                    _view.runeController.Close();
                }
                
            }
		}

		private void HandleUnEquipRuneFeedback( byte[] data )
		{
			UnEquipRuneS2C feedback = ProtobufUtils.Deserialize<UnEquipRuneS2C> ( data );
            if ( feedback.result )
			{                    
                RunePageInfo.RuneSlotInfo slot = runePages.Find(t => t.pageId == _view.currentRunePageId).slots.Find(a => a.id == _view.currentSlotId);              
                slot.state =(int) RuneSlotState.UNLOCK;
                slot.itemId = 0;
                _view.LoadRunePageItem();
                _view.RefreshRuneSlotItem();
                _view.LoadRunePackItem();
				_view.ShowRunePageInformationPanel ();
            }
		}

		private void HandleSellRuneFeedback( byte[] data )
		{
			SellItemS2C feedback = ProtobufUtils.Deserialize<SellItemS2C> ( data );
			if( feedback.result )
			{
                runeBag.itemList.Find(p => p.metaId == currentRuneId).count = (runeBag.itemList.Find(p => p.metaId == currentRuneId).count - sellcount);
                if (runeBag.itemList.Find(p => p.metaId == currentRuneId).count<=0)
                {
                    runeBag.itemList.Remove(runeBag.itemList.Find(t => t.metaId == currentRuneId));
                }				
                _view.LoadRunePackItem ();
                _view.runeController.Close();
            }
		}

		private void HandleRuneBuyFeedback( byte[] data )
		{
			StorePurchaseS2C feedback = ProtobufUtils.Deserialize<StorePurchaseS2C> ( data );
			if( feedback.result )
			{
				_view.LoadRunePackItem ();
                _view.runeController.Close();
                _view.ShowRuneInformation(currentRuneId);
				IncreaseRuneBoughtNumber ();
			}
		}

		private void HandleBuyRunePageFeedback( byte[] data )
		{
			BuyRunePageS2C feedback = ProtobufUtils.Deserialize<BuyRunePageS2C> ( data );
            if ( feedback.result )
			{             
                runePages.Add ( feedback.runePageInfo );
				_view.LoadRunePageItem ();
                _view.runeController.Close();
            }
		}

		private void HandleUnlockRuneSlotFeedback( byte[] data )
		{
			UnlockRuneSlotS2C feedback = ProtobufUtils.Deserialize<UnlockRuneSlotS2C> ( data );
			if( feedback.result )
			{
                _view.InitRuneSlotItem(currentRunePageId);
                _view.runeController.Close();
            }
		}

		private void HandleRefreshRunePageFeedback( byte[] data )
		{
			RefreshRunePageS2C feedback = ProtobufUtils.Deserialize<RefreshRunePageS2C> ( data );

			if( feedback.result )
			{
				List<RunePageInfo.RuneSlotInfo> slotList = feedback.slotInfos;

				if( isUnlock )
				{
					for ( int i = 0; i < runePages.Count; i++ )
					{
						List<RunePageInfo.RuneSlotInfo> mySlotList = runePages.Find ( p => p.pageId == runePages[ i ].pageId ).slots;	
						for ( int j = 0; j < slotList.Count; j++ )
						{
							mySlotList.Find ( p => p.id == slotList[ j ].id ).itemId = slotList[ j ].itemId;
							mySlotList.Find ( p => p.id == slotList[ j ].id ).state = slotList[ j ].state;
						}
					}
				}
				else
				{
					List<RunePageInfo.RuneSlotInfo> mySlotList = runePages.Find ( p => p.pageId == feedback.pageId ).slots;	
					for ( int i = 0; i < slotList.Count; i++ )
					{
						mySlotList.Find ( p => p.id == slotList[ i ].id ).itemId = slotList[ i ].itemId;
						mySlotList.Find ( p => p.id == slotList[ i ].id ).state = slotList[ i ].state;
					}
				}
				_view.RefreshRuneSlotItem ();
			}
		}

		private void HandleRunePageRenameFeedback( byte[] data )
		{
			RunePageRenameS2C feedback = ProtobufUtils.Deserialize<RunePageRenameS2C> ( data );

			if( feedback.result )
			{
				SetRunePageName ( currentRunePageId , currentRunePageName );
				_view.ShowRunePageInformationPanel ();
                _view.runeController.Close();

            }
		}

		private void HandleBoughtNumberFeedback( byte[] data )
		{
			StoreS2C feedback = ProtobufUtils.Deserialize<StoreS2C> ( data );

			if( feedback.result )
			{
				goods =  feedback.storeInfo.storeGoodInfos;
			}
		}

        private void AddRune()
        {
            ItemInfo item = runeBag.itemList.Find(p => p.metaId == currentRuneId);
            item.count += 1;

        }

		private void ReduceRune()
		{
			ItemInfo item = runeBag.itemList.Find ( p => p.metaId == currentRuneId );
			item.count -= 1;

			if( item.count <= 0 )
			{
				runeBag.itemList.Remove ( item );
			}
			_view.LoadRuneAvailableItem ();
			_view.LoadRunePackItem ();

		}

		private void IncreaseRuneBoughtNumber()
		{
			int goodld = GetStoreItemId ( currentRuneId );
			StoreGoodInfo good =  goods.Find ( p => p.id == goodld );
			good.boughtGoods += currentCount;
		}

		private void IncreaseRune()
		{
			ItemInfo runeItem = runeBag.itemList.Find ( p => p.metaId == currentRuneId );
			if( runeItem == null )
			{
				ItemInfo newRuneItem = new ItemInfo ();
				newRuneItem.count++;
				newRuneItem.metaId = currentRuneId;
				newRuneItem.itemId = currentRuneId;
				newRuneItem.itemType = ( int ) BagType.RuneBag;
				runeBag.itemList.Add ( newRuneItem );
			}
			else
			{
				runeItem.count++;
			}
			_view.LoadRunePackItem ();
		}

        #endregion

        #region Data UI

        public SlotProto.Slot GetLevelOpenSolt(int id)
        {
           return listSolt.Find(t => t.ID == id);
        }     
 

		public int GetRuneIcon( int runeId )
		{
			return DataManager.GetInstance ().itemsProtoData.Find ( p => p.ID == runeId ).Icon;
		}

		public RuneProto.Rune GetRuneProto( int runeId )
		{
			return runes.Find ( p => p.ID == runeId );
		}

		public string GetRuneIntrotuce( int runeId )
		{
			int desId = DataManager.GetInstance ().itemsProtoData.Find( p => p.ID == runeId ).DescriptionID;
			return desId.Localize();
		}
			
		public ItemBaseProto.ItemBase GetItemProto( int itemId )
		{
			return DataManager.GetInstance ().itemsProtoData.Find ( p => p.ID == itemId );
		}

		public int GetStoreItemId( int itemId )
		{
			return DataManager.GetInstance ().storeProtoData.Find ( p => p.ItemId == itemId ).ID;
		}

        public int GetRuneBuyCostType(int runeid)
        {
            return DataManager.GetInstance().storeProtoData.Find(p => p.ItemId == runeid).CurrencyType1;
        }

        //Get item Cost
        public List< KeyValuePair<RuneCostType, int>> GetItemCost( int itemId )
		{
            List<KeyValuePair<RuneCostType, int>> itemCostKeyValuePair=new List<KeyValuePair<RuneCostType, int>>() ; 
			StoreProto.Store store = DataManager.GetInstance ().storeProtoData.Find ( p => p.ItemId == itemId );
            if (store.CurrencyValue1>0)
            {
                itemCostKeyValuePair.Add( new KeyValuePair<RuneCostType, int>((RuneCostType)store.CurrencyType1, store.CurrencyValue1));
            }

            if (store.CurrencyValue2 > 0)
            {
                itemCostKeyValuePair.Add(new KeyValuePair<RuneCostType, int>((RuneCostType)store.CurrencyType2, store.CurrencyValue2));
            }

            return itemCostKeyValuePair;

        }

		public int GetRuneNumber( int itemId )
		{
			return runeBag.itemList.Find ( p => p.metaId == itemId ).count;
		}

		public int GetRuneLevel( int runeId )
		{
			return  runes.Find ( p => p.ID == runeId ).Level;
		}

		public int GetRuneTagId( int runeId )
		{
			return runes.Find ( p => p.ID == runeId ).TagID;
		}

		public string GetRuneName( int runeId )
		{
			return DataManager.GetInstance ().itemsProtoData.Find ( p => p.ID == runeId ).Name;
		}

		public int GetRuneType( int runeId )
		{
			return  DataManager.GetInstance ().itemsProtoData.Find ( p => p.ID == runeId ).Subtype;
		}

		public string GetRunePropertyChinese( int propertyId )
		{
			int id = propertyId + 604;
			return id.Localize ();
		}

		public string GetRuneDescribe( int propertyId )
		{
			return localizationProtoDatas.Find ( p => p.ID == propertyId ).Chinese;
		}

		//Rune Data
		public string GetRuneAttribute( int id )
		{
			string prop = "";
			string prop1 = "", prop2 = "", prop3 = "", prop4 = "";

			RuneProto.Rune rune = runes.Find ( p => p.ID == id );

			//Prop1
			int addition1 = rune.Addition1;
			if( addition1 != 0 )
			{
				int propId1 = rune.Property1;
				string propStr1 = GetRunePropertyChinese ( propId1 );

				float value1 = rune.Value1;

				prop1 = addition1 == 1 ? propStr1 + value1 : propStr1 + string.Format ( "{0:P1}" , value1 );
			}
			//Prop2
			int addition2 = rune.Addition2;
			if( addition2 != 0 )
			{
				int propId2 = rune.Property2;
				string propStr2 = GetRunePropertyChinese ( propId2 );

				float value2 = rune.Value2;

				prop2 = addition2 == 1 ? propStr2 + value2 : propStr2 + string.Format ( "{0:P1}" , value2 );
			}
			//Prop3
			int addition3 = rune.Addition3;
			if( addition3 != 0 )
			{
				int propId3 = rune.Property3;
				string propStr3 = GetRunePropertyChinese ( propId3 );

				float value3 = rune.Value3;

				prop3 = addition3 == 1 ? propStr3 + value3 : propStr3 + string.Format ( "{0:P1}" , value3 );
			}
			//Prop4
			int addition4 = rune.Addition4;
			if( addition4 != 0 )
			{
				int propId4 = rune.Property4;
				string propStr4 = GetRunePropertyChinese ( propId4 );

				float value4 = rune.Value4;

				prop4 = addition4 == 1 ? propStr4 + value4 : propStr4 + string.Format ( "{0:P1}" , value4 );
			}

			//Add prop to prop string
			prop = prop1 + "\r\n" + prop2 + "\r\n" + prop3 + "\r\n" + prop4;

			return prop;
		}

		public string GetRunePageAttribute( int runePageId )
		{
			Dictionary<int,float[]> attributes = DataManager.GetInstance().GetRunePageAttribute(runePageId );
			string attribute = "";
			foreach ( int property in attributes.Keys )
			{
				attribute += GetRunePropertyChinese ( property );

				if( attributes[ property ][ 0 ] != 0 )
				{
					attribute += attributes[ property ][ 0 ];
				}

				if( attributes[ property ][ 0 ] != 0 && attributes[ property ][ 1 ] != 0 )
				{
					attribute += " + ";
				}

				if( attributes[ property ][ 1 ] != 0 )
				{
					attribute += string.Format ( "{0:P1}" , attributes[ property ][ 1 ] );
				}

				attribute += "\n";
			}
			return attribute;
		}
			
		public List<int> GetRuneIdList( int level, int tagId )
		{
			List<int> runeIdList = new List<int> ();

			for ( int i = 0; i < runeBag.itemList.Count; i++ )
			{
				int runeId = runeBag.itemList[ i ].metaId;

				if( tagId == -1 )
				{
					if( GetRuneLevel ( runeId ) == level )
					{
						runeIdList.Add ( runeId );
					}
				}
				else
				{
					if( GetRuneLevel ( runeId ) == level && GetRuneTagId ( runeId ) == tagId )
					{
						runeIdList.Add ( runeId );
					}
				}
			}
			return runeIdList;
		}

		public List<int> GetRuneIdList( int runeType )
		{
			List<int[]> rune = new List<int[]> ();
			List<int> runeIdList = new List<int> ();
			List<ItemInfo> itemList =  runeBag.itemList;

			for ( int i = 0; i < runeBag.itemList.Count; i++ )
			{
				int id = itemList[ i ].metaId;
				int level = GetRuneLevel ( id );
				int type = GetRuneType ( id );

				if( runeType == type )
				{
					rune.Add ( new int[]{ id, level } );
				}
			}

			//sort by leve  down
			rune.Sort ( delegate( int[] x, int[] y )
			{
				return y[ 1 ].CompareTo ( x[ 1 ] );
			} );

			for ( int i = 0; i < rune.Count; i++ )
			{
				runeIdList.Add ( rune[ i ][ 0 ] );
			}

			return runeIdList;
		}


        public List<RunePageInfo> GetPageInfo()
        {
            return this.runePages;
        }

        public List<int>  GetRunePageIdList()
		{
			List<int> pageIdList = new List<int> ();

			for ( int i = 0; i < runePages.Count; i++ )
			{
				pageIdList.Add ( runePages[ i ].pageId );
			}
			return  pageIdList;
		}

		public string GetRunePageName(int pageId)
		{
			return  runePages.Find ( p => p.pageId == pageId ).runePageName;
		}

		public void SetRunePageName(int pageId, string name)
		{
			runePages.Find ( p => p.pageId == pageId ).runePageName = name;
		}

		public int GetPlayerLevel()
		{
			return DataManager.GetInstance ().GetPlayerLevel ();
		}

		public int GetPlayerEmber()
		{
			return DataManager.GetInstance ().GetPlayerEmber ();
		}

		public int GetPlayerGold()
		{
			return DataManager.GetInstance ().GetPlayerGold ();
		}

		public int GetPlayerRMB()
		{
			return DataManager.GetInstance ().GetPlayerDiamond ();
		}

		public List<RunePageInfo.RuneSlotInfo> GetRuneSlotsList( int pageId )
		{
			return  runePages.Find ( p => p.pageId == pageId ).slots;
		}

		public int GetRuneId( int slotId, int pageId )
		{
			return GetRuneSlotsList ( pageId ).Find ( p => p.id == slotId ).itemId;
		}

		public int GetRuneSlotType( int slotId )
		{
			return DataManager.GetInstance ().slotProtoData.Find ( p => p.ID == slotId ).SlotType;
		}

		public int GetRuneSlotUnLockLeve( int slotId )
		{
			return DataManager.GetInstance ().slotProtoData.Find ( p => p.ID == slotId ).UnLockLevel;
		}

		public int GetRuneSlotUnLockCost( int slotId )
		{
			return DataManager.GetInstance ().slotProtoData.Find ( p => p.ID == slotId ).UnLockCost;
		}

        /*
         *  这个地方需要修改
         *  
         */
		public int GetRunePageUnLockCost( int pageCount )
		{
			return ( pageCount - 2 ) * 100;
		}

		public int GetRuneBoughtNumber( int itemId )
		{
			int goodId = GetStoreItemId ( itemId );
			return goods.Find ( p => p.id == goodId ).boughtGoods;
		}

		public int GetRuneBoughtNumberLimit( int itemId )
		{
			return DataManager.GetInstance ().storeProtoData.Find ( p => p.ItemId == itemId ).QuantityLimit;
		}

		#endregion
	}
}