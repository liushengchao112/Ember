using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

using Resource;
using System;
using Data;
using Utils;
using System.Diagnostics;

namespace UI
{
	public enum UnitDetailsType
	{
		Health,
		Attack,
		Armor,
		Speed,
		CriticalChance ,
		AttackDistance,
		MagicAttack,
		MagicDefense,
		AttackPerSecond,
		CritDamage,
	}

	public class UnitDetailsSort
	{
		private Text text;
		private Transform progress;

		public Text Text
		{
			get { return text; }
			set { text = value; }
		}

		public Transform Progress
		{
			get { return progress; }
			set { progress = value; }
		}

		public void SetActive(bool showText)
		{
			if (showText)
			{
				progress.gameObject.SetActive( false );
				text.gameObject.SetActive( true );
			}
			else
			{
				text.gameObject.SetActive( false );
				progress.gameObject.SetActive( true );
			}
		}
	}

	public class UnitInfoView : ViewBase, IBeginDragHandler, IDragHandler
    {		
		Dictionary<UnitDetailsType, UnitDetailsSort> infoDict;

		public int unitId;

		private UnitInfoViewController controller;

		private Button detailsChartButton, backButton, forwardButton, blackOffButton, attackDescriptionButton, skillDescriptionButton, backgroundStoryButton, buyButton;

		private Transform unitParent, unitInfoBottom, infoMainUI, unitInfoMiddle, unitBuy, numberTextBg;

		private Text unitNameText, numberText, diamondCostText, goldCostText;

        private GameObject unitModelGameObject;
        private GameObject goUnitModelGameObject = null;

        private UnitBuyPanel unitBuyPanel;
		private UnitAttackInfoPanel unitAttackInfoPanel;
		private BackgroundStoryPanel backgroundStoryPanel;
		private ShakeCamera shakeCameraScript;
        private ShakeCamera shakeCamera
        {
            get
            {
                if ( shakeCameraScript == null )
                {
                    shakeCameraScript = goUnitParent.transform.Find( "Smap00/Main Camera" ).GetComponent<ShakeCamera>();
                }
                return shakeCameraScript;
            }
            set
            {
                shakeCameraScript = value;
            }
        }
        private object blockObj = new object();

        #region unitTypeImage

		private Dictionary<ProfessionType,Image> unitTypeImageDic;

		#endregion

        private bool isShowInfoText = true;    // Show info text mode or progress mode

		private int currentIndex;
		private List<int> currentUnitIdList;
		public Dictionary<int, int> currentUnits;
        private GameObject goUnitParent;
        private Vector2 lastPos;
        private UnitsProto.Unit curUnitData;
        private GameResourceLoadManager loadManager;
        private ShowSummonData curShowSummonData;
        
        public override void OnEnter()
        {
            base.OnEnter();

            this.currentUnitIdList = new List<int>( currentUnits.Keys );
            SetInfoActive( isShowInfoText );

            for ( int i = 0; i < currentUnitIdList.Count; i++ )
            {
                if ( currentUnitIdList[i] == unitId )
                {
                    currentIndex = i;
                    break;
                }
            }

            unitBuyPanel.gameObject.SetActive( false );
            backgroundStoryPanel.gameObject.SetActive( false );
            unitAttackInfoPanel.gameObject.SetActive( false );

            if ( loadManager == null )
            {
                loadManager = GameResourceLoadManager.GetInstance();
            }

            GameObject go = DataManager.GetInstance().GetMainMenuCacheObj<GameObject>( "UnitModelParent" );
            goUnitParent = GameObject.Instantiate( go );
            unitParent = goUnitParent.transform.Find( "node" ).transform;
            controller.PostShowMainBackground( false );
            RefreshView();
        }

        public override void OnInit()
		{
			base.OnInit ();

			controller = new UnitInfoViewController ( this );
			_controller = controller;

			infoDict = new Dictionary<UnitDetailsType, UnitDetailsSort> () 
			{
				{ UnitDetailsType.Health, new UnitDetailsSort() },
				{ UnitDetailsType.Attack, new UnitDetailsSort() },
				{ UnitDetailsType.Armor, new UnitDetailsSort() },
				{ UnitDetailsType.Speed, new UnitDetailsSort() },
				{ UnitDetailsType.CriticalChance, new UnitDetailsSort() },
				{ UnitDetailsType.AttackDistance, new UnitDetailsSort() },
				{ UnitDetailsType.MagicAttack, new UnitDetailsSort() },
				{ UnitDetailsType.MagicDefense, new UnitDetailsSort() },
				{ UnitDetailsType.AttackPerSecond, new UnitDetailsSort() },
				{ UnitDetailsType.CritDamage, new UnitDetailsSort() },
			};

			Transform infoParent = transform.Find ( "InfoMainUI/InfoBg/InfoChart" ).transform;
			unitInfoBottom = transform.Find ( "UnitInfoBottom" );
			infoMainUI = transform.Find ( "InfoMainUI" );
			unitInfoMiddle = transform.Find ( "UnitInfoMiddle" );
			unitBuy = unitInfoBottom.Find ( "UnitBuy" );

			//TODO:Waiting table，Dwayne;
			InitUnitTypeImage();
 
			infoDict[ UnitDetailsType.Health ].Text = infoParent.Find( "Health/Text" ).GetComponent<Text>();
			infoDict[ UnitDetailsType.Health ].Progress = infoParent.Find ( "Health/Progress" ).transform;

			infoDict[ UnitDetailsType.Attack ].Text = infoParent.Find ( "Attack/Text" ).GetComponent<Text> ();
			infoDict[ UnitDetailsType.Attack ].Progress = infoParent.Find ( "Attack/Progress" ).transform;
			
			infoDict[ UnitDetailsType.Armor ].Text = infoParent.Find ( "Armor/Text" ).GetComponent<Text> ();
			infoDict[ UnitDetailsType.Armor ].Progress = infoParent.Find ( "Armor/Progress" ).transform;
			
			infoDict[ UnitDetailsType.Speed ].Text = infoParent.Find ( "Speed/Text" ).GetComponent<Text> ();
			infoDict[ UnitDetailsType.Speed ].Progress = infoParent.Find ( "Speed/Progress" ).transform;
			
			infoDict[ UnitDetailsType.CriticalChance ].Text = infoParent.Find ( "CriticalChance/Text" ).GetComponent<Text> ();
			infoDict[ UnitDetailsType.CriticalChance ].Progress = infoParent.Find ( "CriticalChance/Progress" ).transform;
			
			infoDict[ UnitDetailsType.AttackDistance ].Text = infoParent.Find ( "AttackDistance/Text" ).GetComponent<Text> ();
			infoDict[ UnitDetailsType.AttackDistance ].Progress = infoParent.Find ( "AttackDistance/Progress" ).transform;
			
			infoDict[ UnitDetailsType.MagicAttack ].Text = infoParent.Find ( "MagicAttack/Text" ).GetComponent<Text> ();
			infoDict[ UnitDetailsType.MagicAttack ].Progress = infoParent.Find ( "MagicAttack/Progress" ).transform;

			infoDict[ UnitDetailsType.MagicDefense ].Text = infoParent.Find ( "MagicDefense/Text" ).GetComponent<Text> ();
			infoDict[ UnitDetailsType.MagicDefense ].Progress = infoParent.Find ( "MagicDefense/Progress" ).transform;
			
			infoDict[ UnitDetailsType.AttackPerSecond ].Text = infoParent.Find ( "AttackPerSecond/Text" ).GetComponent<Text> ();
			infoDict[ UnitDetailsType.AttackPerSecond ].Progress = infoParent.Find ( "AttackPerSecond/Progress" ).transform;
			
			infoDict[ UnitDetailsType.CritDamage ].Text = infoParent.Find ( "CritDamage/Text" ).GetComponent<Text> ();
			infoDict[ UnitDetailsType.CritDamage ].Progress = infoParent.Find ( "CritDamage/Progress" ).transform;

			detailsChartButton =  infoParent.GetComponent<Button> ();

			forwardButton =  infoMainUI.Find ( "ForwardButton" ).GetComponent<Button> ();
			blackOffButton =  infoMainUI.Find ( "BlackOffButton" ).GetComponent<Button> ();
			backButton =  infoMainUI.Find ( "BackButton" ).GetComponent<Button> ();
			attackDescriptionButton =  infoMainUI.Find ( "AttackDescriptionButton" ).GetComponent<Button> ();
			skillDescriptionButton =  infoMainUI.Find ( "SkillDescriptionButton" ).GetComponent<Button> ();
			backgroundStoryButton =  infoMainUI.Find ( "BackgroundStoryButton" ).GetComponent<Button> ();
			buyButton =  unitInfoBottom.Find ( "UnitBuy/BuyButton" ).GetComponent<Button> ();

			unitNameText = transform.Find ( "InfoMainUI/InfoBg/InfoChart/UnitNameText" ).GetComponent<Text> ();
			numberText = unitInfoBottom.Find ( "NumberBg/NumberText" ).GetComponent<Text> ();
			diamondCostText = unitInfoBottom.Find ( "UnitBuy/CrystalCostText" ).GetComponent<Text> ();
			goldCostText = unitInfoBottom.Find ( "UnitBuy/GoldCostText" ).GetComponent<Text> ();

			numberTextBg = unitInfoBottom.Find ( "NumberBg" ).transform;

			unitBuyPanel = unitInfoMiddle.Find ( "UnitBuyPanel" ).GetComponent<UnitBuyPanel> ();
			backgroundStoryPanel = unitInfoMiddle.Find ( "BackgroundStoryPanel" ).GetComponent<BackgroundStoryPanel> ();
			unitAttackInfoPanel = unitInfoMiddle.Find ( "UnitAttackInfoPanel" ).GetComponent<UnitAttackInfoPanel> ();

			backButton.AddListener ( BackButtonEvent );

			//TODO need data table fill data 
			buyButton.AddListener ( BuyButtonEvent );

			detailsChartButton.AddListener ( DetailsChartButtonEvent );
			forwardButton.AddListener ( ForwardButtonButtonEvent );
			blackOffButton.AddListener ( BlackOffButtonEvent );
			attackDescriptionButton.AddListener ( AttackDescriptionButtonEvent );
			skillDescriptionButton.AddListener ( SkillDescriptionButtonEvent );
			backgroundStoryButton.AddListener ( BackgroundStoryButtonButtonEvent );
		}

		private void RefreshView()
		{
			RefreshButton ();

            curUnitData = controller.GetUnitProto ( unitId );

			controller.CalculateData ( curUnitData );

			string str = curUnitData.Name;
			string []str2 = str.Split( '/' );
		    
            if( str2.Length == 2)
            {
                unitNameText.text = string.Format( "<color=#1a8af1>{0}</color>  {1}", str2[0], str2[1] );
            }
            
            ProfessionType type = ( ProfessionType )curUnitData.ProfessionType;

			ShowUnitTypeImage( type );

			if( type == ProfessionType.TramcarType || type ==  ProfessionType.DemolisherType  )
			{
				unitBuy.gameObject.SetActive ( false );
				attackDescriptionButton.gameObject.SetActive ( false );
				skillDescriptionButton.gameObject.SetActive ( false );
			}
			else
			{
				unitBuy.gameObject.SetActive ( true );
				attackDescriptionButton.gameObject.SetActive ( true );
				skillDescriptionButton.gameObject.SetActive ( true );

				int[] prices = controller.GetUnitCost ( unitId );
				diamondCostText.text = prices[ 0 ].ToString ();
				goldCostText.text = prices[ 1 ].ToString ();
			}

            int unitNumber = 0;
            currentUnits.TryGetValue( unitId, out unitNumber );
			numberTextBg.gameObject.SetActive( unitNumber != 0 );
			numberText.text = string.Format( " 已拥有\t<color=#00FF00>X{0}</color>", unitNumber );

            DestroyShowModel();

            if ( curUnitData.show_model_res > 0)
            {
                DataManager.GetInstance().AddMainMenuCacheId( curUnitData.show_model_res );
                DataManager.GetInstance().AddMainMenuCacheId( curUnitData.show_effect_res );
                loadManager.LoadAssetAsync<GameObject, int>( curUnitData.show_model_res, LoadModel, curUnitData.show_model_res );
                loadManager.LoadAssetAsync<GameObject, int>( curUnitData.show_effect_res, LoadModelEffect, curUnitData.show_model_res );
            }
            else
            {
                DebugUtils.Log( DebugUtils.Type.UI, "No character models were found, id:" + curUnitData.show_model_res );
            }
        }

        private void LoadModel( GameObject go, int modelID )
        {
            if( curUnitData != null && modelID == curUnitData.show_model_res )
            {
                goUnitModelGameObject = go;
            }
        }

        private void LoadModelEffect( GameObject effect, int modelID )
        {
            if ( curUnitData != null && modelID == curUnitData.show_model_res )
            {
                UnitShow( effect, modelID );
            }
        }

        private void UnitShow( GameObject effect, int modelID )
        {
            if ( goUnitModelGameObject == null ) return;

            unitModelGameObject = GameObject.Instantiate( goUnitModelGameObject );
            unitModelGameObject.transform.SetParent( unitParent, false );
            UnitAnimatorEvent unitAnimatorEvent = unitModelGameObject.AddComponent<UnitAnimatorEvent>();
            unitAnimatorEvent.Init();
            GameObject.Instantiate( effect ).transform.SetParent( unitModelGameObject.transform, false );

            string[] shakeTimes = curUnitData.show_shake_time.Split( '|' );
            if ( shakeTimes.Length > 0 && float.Parse( shakeTimes[0] ) != 0 )
            {
                string[] shakeLevels = curUnitData.show_shake_num.Split( '|' );
                CameraShakeHandler( unitAnimatorEvent, shakeTimes, shakeLevels, modelID );
            }

            LoadSummon( unitAnimatorEvent, curUnitData.SkillID, curUnitData.show_model_res );
            LoadSummon( unitAnimatorEvent, curUnitData.SkillID2, curUnitData.show_model_res );

        }
        
        private void CameraShakeHandler( UnitAnimatorEvent uae, string[] times, string[] levels, int modelID )
        {
            if( times.Length != levels.Length )
            {
                DebugUtils.LogError( DebugUtils.Type.UI, "The unit table data field length is inconsistent, show_shake_time!=show_shake_num" );
                return;
            }
            //times = new string[] { "1.5", "1.8", "2.5" };
            for ( int i = 0; i < times.Length; i++ )
            {
                uae.AddEvent( float.Parse( times[i] ), string.Concat( levels[i], "|", modelID ), ShowCameraShake );
            }
        }

        private void ShowCameraShake( string data )
        {
            DebugUtils.Log( DebugUtils.Type.UI, "Show camera shake" );
            string[] arr = data.Split( '|' );
            if ( curUnitData.show_model_res == int.Parse( arr[1] ) )
            {
                shakeCamera.ShowShake( int.Parse( arr[0] ) );
            }
        }

        private void LoadSummon( UnitAnimatorEvent uae, int skillId, int modelId )
        {
            UnitSkillsProto.UnitSkill skillData = controller.GetUnitSkillData( skillId );
            //3 summon
            if ( skillData.CarrierType == 3 )
            {
                SummonProto.Summon summonData = controller.GetSummonData( skillData.CarrierID );

                if ( summonData.Show_summon_time == 0 ) return;

                curShowSummonData = new ShowSummonData();
                curShowSummonData.modelId = modelId;
                curShowSummonData.canShow = false;

                string[] pos = summonData.Show_summon_pos.Split( '|' );
                if( pos.Length != 3 )
                {
                    DebugUtils.LogError( DebugUtils.Type.UI, "unit show summon postion err! pos:" + summonData.Show_summon_pos );
                }
                else
                {
                    curShowSummonData.pos = new Vector3( float.Parse( pos[0] ), float.Parse( pos[1] ), float.Parse( pos[2] ) );
                }

                DataManager.GetInstance().AddMainMenuCacheId( summonData.Show_model_id );

                loadManager.LoadAssetAsync<GameObject, int>( summonData.Show_model_id, LoadSummonComplete, modelId );
                uae.AddEvent( summonData.Show_summon_time, modelId.ToString(), ShowSummonTime );
            }
        }

        private void LoadSummonComplete( GameObject go, int modeID )
        {
            if ( curShowSummonData == null ) return;
            if ( curUnitData == null || curUnitData.show_model_res != modeID ) return;

            curShowSummonData.model = go;
            ShowSummon();
        }

        private void ShowSummonTime( string data )
        {
            if ( curShowSummonData == null ) return;
            if ( curUnitData == null || curUnitData.show_model_res != int.Parse( data ) ) return;

            curShowSummonData.canShow = true;
            ShowSummon();
        }

        private void ShowSummon()
        {
            if ( curShowSummonData != null 
                && curUnitData != null
                && curShowSummonData.canShow
                && curShowSummonData.model != null 
                && curShowSummonData.modelId == curUnitData.show_model_res
                && unitModelGameObject != null )
            {
                GameObject go = GameObject.Instantiate( curShowSummonData.model );
                go.transform.SetParent( unitModelGameObject.transform, false );
                go.transform.localPosition = curShowSummonData.pos;
            }
        }

        public void OnBeginDrag( PointerEventData eventData )
        {
            lastPos = eventData.position;
        }

        public void OnDrag( PointerEventData eventData )
        {
            float angleY = lastPos.x - eventData.position.x;
            if ( Math.Abs( angleY ) > 0.1f )
            {
                Vector3 angle = new Vector3( 0, angleY, 0 ) * 0.5f;

                if ( unitModelGameObject != null )
                {
                    unitModelGameObject.transform.localEulerAngles += angle;
                }

                lastPos = eventData.position;
            }
        }

        public void AddUnitNumber(int number)
		{
			int unitNumber = currentUnits[ unitId ] += number;

			if( unitNumber == 0 )
			{
				numberTextBg.gameObject.SetActive ( false );
			}
			else
			{
				numberTextBg.gameObject.SetActive ( true );
				numberText.text = string.Format( " 已拥有\t<color=#00FF00>X{0}</color>", unitNumber );
			}
		}

		private  void OnRoateModelCallBack(Vector3 angle)
		{
			
		}

		#region unitTypeImage functions

		private void InitUnitTypeImage()
		{
			unitTypeImageDic = new Dictionary<ProfessionType, Image>();
			//TODO:Waiting table support,there will add image in dic.

			Transform unitTypeRoot = infoMainUI.Find( "InfoBg/InfoChart/UnitTypeIcons" );

			List<Transform> images = new List<Transform>();

			for( int i = 0; i < unitTypeRoot.childCount; i++ )
			{
				images.Add( unitTypeRoot.GetChild( i ) );
			}

			unitTypeImageDic.Add( ProfessionType.FighterType, images[0].GetComponent<Image>() );
			unitTypeImageDic.Add( ProfessionType.WizardType, images[1].GetComponent<Image>() );
			unitTypeImageDic.Add( ProfessionType.AssassinType, images[2].GetComponent<Image>() );
			unitTypeImageDic.Add( ProfessionType.AssistantType, images[3].GetComponent<Image>() );
			unitTypeImageDic.Add( ProfessionType.ShooterType, images[4].GetComponent<Image>() );
			unitTypeImageDic.Add( ProfessionType.TramcarType, images[5].GetComponent<Image>() );
			unitTypeImageDic.Add( ProfessionType.DemolisherType, images[5].GetComponent<Image>() );
			//TODO: If have buildings open this and add buildings type in professionType.
			//unitTypeImageDic.Add( ProfessionType.Buildings, images[6].GetComponent<Image>() );
		}

		private void ShowUnitTypeImage( ProfessionType type )
		{
			Image tempImage;
			unitTypeImageDic.TryGetValue( type, out tempImage );

			if( tempImage != null )
			{
				foreach( Image item in unitTypeImageDic.Values )
				{
					item.gameObject.SetActive( false );
				}

				tempImage.gameObject.SetActive( true );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.UI, string.Format( "The unit type icon can't find.Check this hero type {0}", type ));
			}
		}

		#endregion

		#region ButtonEvent

		private void AttackDescriptionButtonEvent()
		{
			unitAttackInfoPanel.gameObject.SetActive ( true );
			Data.UnitsProto.Unit unitProto =  controller.GetUnitProto ( unitId );
			unitAttackInfoPanel.descripe = controller.GetUnitSkillDescribe ( unitProto.SkillID );
			unitAttackInfoPanel.icon = unitProto.Icon;
			unitAttackInfoPanel.RefreshPanel ();
		}

		private void SkillDescriptionButtonEvent()
		{
			unitAttackInfoPanel.gameObject.SetActive ( true );
			Data.UnitsProto.Unit unitProto =  controller.GetUnitProto ( unitId );
			unitAttackInfoPanel.descripe = controller.GetUnitSkillDescribe ( unitProto.SkillID2 );
			unitAttackInfoPanel.icon = unitProto.Icon;
			unitAttackInfoPanel.RefreshPanel ();
		}

		private void BackgroundStoryButtonButtonEvent()
		{
			ShowBackgroundStoryPanel ();
		}

		public void DetailsChartButtonEvent()
		{
			SetInfoActive( !isShowInfoText );
			isShowInfoText = !isShowInfoText;
		}

		private void ForwardButtonButtonEvent()
		{
			unitId = currentUnitIdList[ ++currentIndex ];
			RefreshView ();
		}

		private void BlackOffButtonEvent()
		{
			unitId = currentUnitIdList[ --currentIndex ];
			RefreshView ();
		}

		private void BuyButtonEvent()
		{
			ShowUnitBuyPanel ();
		}

		private void RefreshButton()
		{
			blackOffButton.gameObject.SetActive ( true );
			forwardButton.gameObject.SetActive ( true );
			if( currentIndex == 0 )
			{
				blackOffButton.gameObject.SetActive ( false );
			}

			if( currentIndex == currentUnitIdList.Count - 1 )
			{
				forwardButton.gameObject.SetActive ( false );
			}
		}

		private  void BackButtonEvent()
		{
			UIManager.Instance.GetUIByType ( UIType.UnitMainUI , (ui, param ) =>
			{
				if ( ui != null )
				{
					if ( !ui.openState )
					{
                        ui.OnEnter();
					}
				}
			} );
		}

		private void OnUnitBuyClickCallBack( int count, Data.CurrencyType type )
		{
			controller.SendUnitBuy( count , unitId , type );
		}

#endregion

		public void SetInfoActive(bool showText)
		{
			foreach (var item in infoDict)
			{
				item.Value.SetActive( showText );
			}
		}
			
		public void SetInfoTextValue( UnitDetailsType type, string value)
		{
			infoDict[type].Text.text = value;
		}

		public void SetInfoProgressValue( UnitDetailsType type, int showNum )
		{
			showNum++;
			int count = infoDict[type].Progress.childCount;
			for (int i = 1 ; i < count ; i++)
			{
				if (i < showNum)
				{
					infoDict[type].Progress.GetChild( i ).gameObject.SetActive( true );
				}
				else
				{
					infoDict[type].Progress.GetChild( i ).gameObject.SetActive( false );
				}
			}
		}

        public override void OnExit( bool isGoBack )
        {
            base.OnExit( isGoBack );
            
            if ( goUnitParent != null )
            {
                GameObject.Destroy( goUnitParent );
                goUnitParent = null;
            }
            shakeCamera = null;
            DestroyShowModel();
            curUnitData = null;
            curShowSummonData = null;
            goUnitModelGameObject = null;
            controller.PostShowMainBackground( true );
        }

        private void DestroyShowModel()
        {
            if ( unitModelGameObject != null )
            {
                GameObject.Destroy( unitModelGameObject );
                unitModelGameObject = null;
            }
        }

#region show window

        public void ShowUnitBuyPanel()
		{
			unitBuyPanel.gameObject.SetActive ( true );
			Data.UnitsProto.Unit unitProto =  controller.GetUnitProto ( unitId );
			unitBuyPanel.descripe = unitProto.Description_Txt.Localize();
			unitBuyPanel.icon = unitProto.Icon_bust;
			unitBuyPanel.unitName = unitProto.Name;
			unitBuyPanel.playerGold = controller.dataManager.GetPlayerGold();
			unitBuyPanel.playerCrystal = controller.dataManager.GetPlayerEmber();
			unitBuyPanel.playerDiamond = controller.dataManager.GetPlayerDiamond();
			unitBuyPanel.boughtNumber = controller.GetUnitBoughtNumber ( unitId );
			unitBuyPanel.boughtNumberLimit = controller.GetUnitBoughtNumberLimit ( unitId );

			int[] prices = controller.GetUnitCost ( unitId );
			unitBuyPanel.diamondCost = prices[0];
			unitBuyPanel.goldCost = prices[1];
			//unitBuyPanel.crystalCost = prices[2];

			unitBuyPanel.RefreshPanel ();
			unitBuyPanel.onClickEvent = OnUnitBuyClickCallBack;
		}

		public void ShowUnitAttackInfoPanel()
		{

		}

		public void ShowBackgroundStoryPanel()
		{
			backgroundStoryPanel.gameObject.SetActive ( true );
			Data.UnitsProto.Unit unitProto =  controller.GetUnitProto ( unitId );
			backgroundStoryPanel.descripe = unitProto.Description_Txt.Localize();
			backgroundStoryPanel.RefreshPanel ();
		}

#endregion

#region close window

		public void CloseUnitBuyPanel()
		{
			unitBuyPanel.gameObject.SetActive ( false );
		}

        #endregion

        class ShowSummonData
        {
            public int modelId;
            public bool canShow;
            public GameObject model;
            public Vector3 pos;
        }

    }
}