using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Data;
using Network;
using Utils;
using Constants;

namespace UI
{
	public class UnitDetailsDataType
	{
		public float textValue;
		public int progressValue;
	}

	public class UnitInfoViewController : ControllerBase
	{
		UnitInfoView view;

		private Dictionary<UnitDetailsType, List<float>> textToProgressDict;

		private Dictionary<UnitDetailsType, UnitDetailsDataType> infoDataDict;

		private List<StoreGoodInfo> goods;

		private int currentUnitId;
		private int currentCount;

		public DataManager dataManager;

		#region InitData

		public UnitInfoViewController( UnitInfoView view )
		{
			viewBase = view;
			this.view = view;
			this.OnCreate ();
			dataManager = DataManager.GetInstance();
        }

		public override void OnCreate()
		{
			base.OnCreate ();

			textToProgressDict = new Dictionary<UnitDetailsType,  List<float>> () {
				{ UnitDetailsType.Health,  new List<float> () { 250, 500, 750 } },
				{ UnitDetailsType.Attack,  new List<float> () { 13, 26, 39 } },
				{ UnitDetailsType.Armor,  new List<float> () { 15, 30, 45, } },
				{ UnitDetailsType.Speed,  new List<float> () { 0.75f, 1.5f, 2.25f } },
				{ UnitDetailsType.CriticalChance,  new List<float> () { 0.25f, 0.5f, 0.75f } },
				{ UnitDetailsType.AttackDistance,  new List<float> () { 5, 10, 15 } },
				{ UnitDetailsType.MagicAttack,  new List<float> () { 13, 26, 39 } },
				{ UnitDetailsType.MagicDefense,  new List<float> () {15 , 30, 45 } },
				{ UnitDetailsType.AttackPerSecond,  new List<float> () { 1.5f, 1f, 0.5f } },
				{ UnitDetailsType.CritDamage,  new List<float> () { 0.5f, 1, 1.5f } },
			};

			infoDataDict = new Dictionary<UnitDetailsType,  UnitDetailsDataType> () 
			{
				{ UnitDetailsType.Health, new UnitDetailsDataType() },
				{ UnitDetailsType.Attack,  new UnitDetailsDataType() },
				{ UnitDetailsType.Armor,  new UnitDetailsDataType() },
				{ UnitDetailsType.Speed,  new UnitDetailsDataType() },
				{ UnitDetailsType.CriticalChance,  new UnitDetailsDataType() },
				{ UnitDetailsType.AttackDistance,  new UnitDetailsDataType() },
				{ UnitDetailsType.MagicAttack,  new UnitDetailsDataType() },
				{ UnitDetailsType.MagicDefense,  new UnitDetailsDataType() },
				{ UnitDetailsType.AttackPerSecond,  new UnitDetailsDataType() },
				{ UnitDetailsType.CritDamage,  new UnitDetailsDataType() },
			};
		}

		public void CalculateData( UnitsProto.Unit data )
		{
			float health = data.Health;
			infoDataDict[ UnitDetailsType.Health ].textValue = Round ( health );
			infoDataDict[ UnitDetailsType.Health ].progressValue = TextToProgress ( UnitDetailsType.Health , health );

			float attack = data.PhysicalAttack;
			infoDataDict[ UnitDetailsType.Attack ].textValue = Round ( attack );
			infoDataDict[ UnitDetailsType.Attack ].progressValue = TextToProgress ( UnitDetailsType.Attack , attack );

			float armor = data.Armor;
			infoDataDict[ UnitDetailsType.Armor ].textValue = Round ( armor );
			infoDataDict[ UnitDetailsType.Armor ].progressValue = TextToProgress ( UnitDetailsType.Armor , armor );

			float speed = data.MoveSpeed;
			infoDataDict[ UnitDetailsType.Speed ].textValue = Round ( speed );
			infoDataDict[ UnitDetailsType.Speed ].progressValue = TextToProgress ( UnitDetailsType.Speed , speed );

			float criticalChance = data.CriticalChance;
			infoDataDict[ UnitDetailsType.CriticalChance ].textValue = Round ( criticalChance );
			infoDataDict[ UnitDetailsType.CriticalChance ].progressValue = TextToProgress ( UnitDetailsType.CriticalChance , criticalChance );

			float attackDistance = data.AttackRange;
			infoDataDict[ UnitDetailsType.AttackDistance ].textValue = Round ( attackDistance );
			infoDataDict[ UnitDetailsType.AttackDistance ].progressValue = TextToProgress ( UnitDetailsType.AttackDistance , attackDistance );

			float magicAttack = data.MagicAttack;
			infoDataDict[ UnitDetailsType.MagicAttack ].textValue = Round ( magicAttack );
			infoDataDict[ UnitDetailsType.MagicAttack ].progressValue = TextToProgress ( UnitDetailsType.MagicAttack , magicAttack );

			float magicDefense = data.MagicResist;
			infoDataDict[ UnitDetailsType.MagicDefense ].textValue = Round ( magicDefense );
			infoDataDict[ UnitDetailsType.MagicDefense ].progressValue = TextToProgress ( UnitDetailsType.MagicDefense , magicDefense );

			float attackPerSecond = data.AttackInterval;
			infoDataDict[ UnitDetailsType.AttackPerSecond ].textValue = Round ( attackPerSecond );
			infoDataDict[ UnitDetailsType.AttackPerSecond ].progressValue = TextToProgress ( UnitDetailsType.AttackPerSecond , attackPerSecond );

			float critDamage = data.CriticalDamage;
			infoDataDict[ UnitDetailsType.CritDamage ].textValue = Round ( critDamage );
			infoDataDict[ UnitDetailsType.CritDamage ].progressValue = TextToProgress ( UnitDetailsType.CritDamage , critDamage );

			SetInfoValue ();
		}

		private void SetInfoValue()
		{
			view.SetInfoTextValue( UnitDetailsType.Health, infoDataDict[UnitDetailsType.Health].textValue.ToString() );
			view.SetInfoProgressValue( UnitDetailsType.Health, infoDataDict[UnitDetailsType.Health].progressValue );

			view.SetInfoTextValue( UnitDetailsType.Attack, infoDataDict[UnitDetailsType.Attack].textValue.ToString() );
			view.SetInfoProgressValue( UnitDetailsType.Attack, infoDataDict[UnitDetailsType.Attack].progressValue );

			view.SetInfoTextValue( UnitDetailsType.Armor, infoDataDict[UnitDetailsType.Armor].textValue.ToString() );
			view.SetInfoProgressValue( UnitDetailsType.Armor, infoDataDict[UnitDetailsType.Armor].progressValue );

			view.SetInfoTextValue( UnitDetailsType.Speed, infoDataDict[UnitDetailsType.Speed].textValue.ToString() );
			view.SetInfoProgressValue( UnitDetailsType.Speed, infoDataDict[UnitDetailsType.Speed].progressValue );

			view.SetInfoTextValue( UnitDetailsType.CriticalChance, infoDataDict[UnitDetailsType.CriticalChance].textValue.ToString() );
			view.SetInfoProgressValue( UnitDetailsType.CriticalChance, infoDataDict[UnitDetailsType.CriticalChance].progressValue );

			view.SetInfoTextValue( UnitDetailsType.AttackDistance, infoDataDict[UnitDetailsType.AttackDistance].textValue.ToString() );
			view.SetInfoProgressValue( UnitDetailsType.AttackDistance, infoDataDict[UnitDetailsType.AttackDistance].progressValue );

			view.SetInfoTextValue( UnitDetailsType.MagicAttack, infoDataDict[UnitDetailsType.MagicAttack].textValue.ToString() );
			view.SetInfoProgressValue( UnitDetailsType.MagicAttack, infoDataDict[UnitDetailsType.MagicAttack].progressValue );

			view.SetInfoTextValue( UnitDetailsType.MagicDefense, infoDataDict[UnitDetailsType.MagicDefense].textValue.ToString() );
			view.SetInfoProgressValue( UnitDetailsType.MagicDefense, infoDataDict[UnitDetailsType.MagicDefense].progressValue );

			view.SetInfoTextValue( UnitDetailsType.AttackPerSecond, infoDataDict[UnitDetailsType.AttackPerSecond].textValue.ToString() );
			view.SetInfoProgressValue( UnitDetailsType.AttackPerSecond, infoDataDict[UnitDetailsType.AttackPerSecond].progressValue );

			view.SetInfoTextValue( UnitDetailsType.CritDamage, infoDataDict[UnitDetailsType.CritDamage].textValue.ToString() );
			view.SetInfoProgressValue( UnitDetailsType.CritDamage, infoDataDict[UnitDetailsType.CritDamage].progressValue );
		}

		private int TextToProgress( UnitDetailsType type, float textValue )
		{
			int progressValue = 1;

			if( type == UnitDetailsType.AttackPerSecond )
			{
				for ( int i = 0; i < textToProgressDict[type].Count; i++ )
				{
					if( textValue <= textToProgressDict[ type ][ i ] )
					{
						progressValue = i + 2;
					}
					else
					{
						break;
					}
				}
			}
			else
			{
				for ( int i = 0; i < textToProgressDict[type].Count; i++ )
				{
					if( textValue >= textToProgressDict[ type ][ i ] )
					{
						progressValue = i + 2;
					}
					else
					{
						break;
					}
				}
			}
			return progressValue;
		}

		private float Round( float number )
		{
			float temp = number * Mathf.Pow( 10, 2 );
			return Mathf.Round( temp ) / Mathf.Pow( 10, 2 );
		}

		#endregion

		public override void OnResume()
		{
			SendBoughtNumber ();
			
			NetworkManager.RegisterServerMessageHandler ( MsgCode.StoreMessage , HandleBoughtNumberFeedback );
			NetworkManager.RegisterServerMessageHandler ( MsgCode.StoreBuyMessage , HandleUnitBuyFeedback );
		}

		public override void OnPause()
		{
			NetworkManager.RemoveServerMessageHandler ( MsgCode.StoreMessage , HandleBoughtNumberFeedback );
			NetworkManager.RemoveServerMessageHandler ( MsgCode.StoreBuyMessage , HandleUnitBuyFeedback );
		}

		#region Send

		public void SendUnitBuy( int count, int unitId, CurrencyType type )
		{
			currentUnitId = unitId;
			currentCount = count;
			StorePurchaseC2S message = new StorePurchaseC2S ();

			message.currencyType = type;
			message.count = count;
			message.goodId = GetStoreItemId ( unitId );

			byte[] data = ProtobufUtils.Serialize ( message );
			NetworkManager.SendRequest ( MsgCode.StoreBuyMessage, data );
		}

		public void SendBoughtNumber()
		{
			StoreC2S message = new StoreC2S();
			message.storeType = StoreType.Hero;
			byte[] data = ProtobufUtils.Serialize ( message );
			NetworkManager.SendRequest ( MsgCode.StoreMessage, data );
		}

        public void PostShowMainBackground( bool show )
        {
            MessageDispatcher.PostMessage( Constants.MessageType.ShowMainBackground, show );
        }

		#endregion

		#region Reponse Handle

		private void HandleBoughtNumberFeedback( byte[] data )
		{
			StoreS2C feedback = ProtobufUtils.Deserialize<StoreS2C> ( data );
		
			if( feedback.result )
			{
				goods =  feedback.storeInfo.storeGoodInfos;
			}
		}

		private void HandleUnitBuyFeedback( byte[] data )
		{
			StorePurchaseS2C feedback = ProtobufUtils.Deserialize<StorePurchaseS2C> ( data );

			if( feedback.result )
			{
				view.CloseUnitBuyPanel ();
				IncreaseUnitBoughtNumber ();
				view.AddUnitNumber ( currentCount );
			}
		}

		private void IncreaseUnitBoughtNumber()
		{
			int goodld = GetStoreItemId ( currentUnitId );
			StoreGoodInfo good =  goods.Find ( p => p.id == goodld );
			good.boughtGoods += currentCount;
		}

		#endregion

		#region Data UI

		public UnitsProto.Unit GetUnitProto(int unitId)
		{
			return DataManager.GetInstance ().unitsProtoData.Find ( p => p.ID == unitId );
		}

		public string GetUnitSkillDescribe (int skillId)
		{
            return GetUnitSkillData( skillId ).Txt_ID.Localize();
		}

        public UnitSkillsProto.UnitSkill GetUnitSkillData( int skillId )
        {
            return DataManager.GetInstance().unitSkillsProtoData.Find( p => p.ID == skillId );
        }

        public SummonProto.Summon GetSummonData( int skillId )
        {
            return DataManager.GetInstance().summonProtoData.Find( p => p.ID == skillId );
        }

        //index 0: diaMond	1: gold 2:ember
        public int[] GetUnitCost( int unitId )
		{
			int[] prices = new int[2];

			int goodId = GetStoreItemId ( unitId );

			StoreProto.Store store = DataManager.GetInstance ().storeProtoData.Find ( p => p.ID == goodId );

			int types = store.CurrencyType1;
			int Cost = store.CurrencyValue1;

			if( types == 1 )
			{
				prices[0] = Cost;
			}
			else if( types == 2 )
			{
				int types2 = store.CurrencyType2;
				int Cost2 = store.CurrencyValue2;

				prices[1] = Cost;

				if( types2 == 1 )
				{
					prices[0] = Cost2;
				}
				else
				{
					DebugUtils.LogError( DebugUtils.Type.UI, string.Format( "Cost type {0} can't be here.", ( Data.CurrencyType ) types ) );
				}
				//TODO:If need ember add logic check in there.
			}
			else
			{
				//TODO:If need ember add logic check in there.
				DebugUtils.LogError( DebugUtils.Type.UI, string.Format( "Can't know this cost type {0}", ( Data.CurrencyType ) types ) );
			}

			return prices;
		}

		public int GetStoreItemId( int unitId )
		{
			return DataManager.GetInstance ().storeProtoData.Find ( p => p.ItemId == unitId ).ID;
		}

		public int GetUnitBoughtNumber( int unitId )
		{
			int goodId = GetStoreItemId ( unitId );

			return goods.Find ( p => p.id == goodId ).boughtGoods;
		}

		public int GetUnitBoughtNumberLimit( int unitId )
		{
			int goodId = GetStoreItemId ( unitId );

			return DataManager.GetInstance ().storeProtoData.Find ( p => p.ID == goodId ).QuantityLimit;
		}

		#endregion

	}
}
