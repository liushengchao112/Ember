using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

using Resource;
using Utils;
using Data;

namespace UI
{
	public class RuneMainView : ViewBase
	{
		#region Path

		//only be used on this script
		private const string RUNE_PACK_ITEM_PATH = "RunePackItem";
		private const string RUNE_SLOT_ITEM_PATH = "RuneSlotItem";
		private const string RUNE_AVAILABLE_ITEM_PATH = "RuneAvailableItem";
		private const string RUNE_PAPG_ITEM_PATH = "RunePageItem";
		private const string RUNE_CONTRAST_ITEM_PATH = "RuneContrastItem";

		#endregion

		private ToggleGroup runePageGroup;
		private RuneMainViewController controller;
		private Button runePageListPanelBackButton, runePageRenamePanelExitButon, runePageAddButton;
		private Button  runeTagButton, runeBuyButton, runeSellButton, runeUninstallButton, runeReplaceButton, renameButton ;
        private ToggleGroup runeItemToggerGroup;
        private Transform runeTop, runeMain, runeRight;
		private Transform runePackGroup, runeLevelGroup, runeTagGroup, runeSlotGroup, runeAvailableGroup, contrastRuneGroup;   
        private Transform runeAvailablePanel,  runePageInformationPanel;
        private Transform twSlotPanel, hwSlotPanel, mwSlotPanel;
        private Transform runePackPanle;
        private Transform runeConfigPanle;
        private Transform runePackItemNull;      
		private RuneInformationPanel runeInformationPanel;
		private SlotRuneInformationPanel slotRuneInformationPanel;     

        private Text runePageAttributeText, runePageTitleText;
        private Text runeSlotPageTotalLevel;

		private Toggle[] runeBottomToggles, runeLevelToggles, runeTagToggles;
        private Toggle runePack;
        private Toggle runeConfiger;
        private Toggle runedefault;
        private Text runeTagTitleLabel;     
        public AnimationCurve runeAnimationCurve;       
		private List<RunePackItem> runePackItemList = new List<RunePackItem> ();
		private List<RuneSlotItem> runeSlotItemList = new List<RuneSlotItem> ();
		private List<RunePageItem> runePageItemList = new List<RunePageItem> ();
		private List<RuneAvailableItem> runeAvailableList = new List<RuneAvailableItem> ();
		
		private int[] runeLevels = { 1, 2, 3, 4, 5, 6 };
		private int[] runeTags = { 1, 2, 3, 4, 5 };

		private int currentRuneLevel = 1;
		private int currentRuneTagId = 1;
		private int currentRuneId = -1;
		private string runePageAttribute = "";
		private int currentPageCount = 0;
		private int MaxPageCount = 5;
		private string currentRunePageName;

		public int currentRunePageId = 0;
		public int currentSlotId = 1;
        private int slotPageTotalLevel = 0;
        
        private AttributePenalType openAttributePenalType;
        private enum AttributePenalType
        {
            Equipment=1,
            Contrast=2,
        }

        public override void OnEnter()
        {
            base.OnEnter();
            runeTagGroupTitle = "物理";
            runeTagTitleLabel.text = runeTagGroupTitle;
            currentRuneTagId = runeTags[0];
            currentRuneLevel = runeLevels[0];
            runeInformationPanel.gameObject.SetActive(false);
            if (runeMain.Find("RunePackPanel").gameObject.activeSelf)
            {
                LoadRunePackItem();
            }
            else if (runeMain.Find("RuneConfigurePanel").gameObject.activeSelf)
            {
                LoadRunePageItem();
            }
            runeTagGroup.localScale = new Vector3(1, 0, 1);   
        }

        public override void OnInit()
		{
			base.OnInit ();

			controller = new RuneMainViewController ( this );
			_controller = controller;
            
             runeTop = transform.Find ( "ScaleManager/RuneTop" );
			runeMain = transform.Find ( "ScaleManager/RuneMain" );
			runeRight = transform.Find ( "ScaleManager/RuneRight" );		

            runePack = runeTop.Find("RunePackToggle").GetComponent<Toggle>();
            runeConfiger = runeTop.Find("RuneConfigureToggle").GetComponent<Toggle>();
            runePackPanle = runeMain.Find("RunePackPanel");
            runeConfigPanle = runeMain.Find("RuneConfigurePanel");
           
            runeAvailablePanel = runeRight.Find ( "RuneAvailablePanel" ).transform;
			runePageInformationPanel = runeRight.Find ( "RunePageInformation" ).transform;

			runeLevelGroup = runeMain.Find ("RunePackPanel/PackBottom/RuneLevelGroup");
			runeTagGroup = runeMain.Find ("RunePackPanel/RuneTagGroup");
            runeTagButton = runeMain.Find("RunePackPanel/RuneTagAll").GetComponent<Button>();
            runePackGroup = runeMain.Find ("RunePackPanel/PackMiddle/ItemGroup");
            runePackItemNull = runeMain.Find("RunePackPanel/PackMiddle/NoItemImage");
            runeTagTitleLabel = runeMain.Find("RunePackPanel/RuneTagAll/RuneTagItemText").GetComponent<Text>();
            runeSlotGroup = runeMain.Find ("RuneConfigurePanel/RuneConfigerPool");
			runePageGroup = runeMain.Find ( "RuneConfigurePanel/PackBottom/RunePageGroup").GetComponent<ToggleGroup> ();
            twSlotPanel = runeSlotGroup.Find("TW");
            hwSlotPanel = runeSlotGroup.Find("HW");
            mwSlotPanel = runeSlotGroup.Find("MW");
            runeAvailableGroup = runeAvailablePanel.Find ( "RuneAvailableList/RuneAvailableGroup" );
            runeInformationPanel = runeRight.Find ( "RuneInformationPanel" ).GetComponent<RuneInformationPanel> ();
			slotRuneInformationPanel = runeRight.Find ( "SlotRuneInformationPanel" ).GetComponent<SlotRuneInformationPanel> ();       
            runePageAddButton = runeMain.Find ("RuneConfigurePanel/PackBottom/RunePageGroup/RunePageAddButton").GetComponent<Button> ();
			runeBuyButton = runeRight.Find ("RuneInformationPanel/RuneBuyButton").GetComponent<Button> ();
			runeSellButton = runeRight.Find ("RuneInformationPanel/RuneSellButton").GetComponent<Button> ();
            runeUninstallButton = runeRight.Find ( "SlotRuneInformationPanel/RuneUninstallButton" ).GetComponent<Button> ();
            runeReplaceButton = runeRight.Find ( "SlotRuneInformationPanel/RuneReplaceButton" ).GetComponent<Button> ();
			renameButton = runeRight.Find ( "RunePageInformation/RenameButton" ).GetComponent<Button> ();           
            runedefault = runeConfigPanle.Find("RuneConfigerPool/Rune").GetComponent<Toggle>();
            runePageAttributeText = runePageInformationPanel.Find ("AttributeImage/AttributeText ").GetComponent<Text> ();
			runePageTitleText = runePageInformationPanel.Find ("Title/TitleText").GetComponent<Text> ();
            runeSlotPageTotalLevel = runePageInformationPanel.Find("Title/Image/Text").GetComponent<Text>();
            runeItemToggerGroup = this.runePackGroup.GetComponent<ToggleGroup>();
            InitOnClickListener();
        }

        public void InitOnClickListener()
		{
            runeMain.Find("RunePackPanel").gameObject.SetActive(true);
            runeMain.Find("RuneConfigurePanel").gameObject.SetActive(false);
            runeConfiger.AddListener(OnClickRuneConfigurePenalToggle);
            runePack.AddListener(OnClickRunePackPanelToggle);

            runeLevelToggles = new Toggle[runeLevelGroup.childCount];
			runeTagToggles = new Toggle[runeTagGroup.childCount];

			for ( int i = 0; i < runeLevelToggles.Length; i++ )
			{
				runeLevelToggles[ i ] = runeLevelGroup.GetChild ( i ).GetComponent<Toggle> ();
				runeLevelToggles[ i ].AddListener ( OnClickSelectLevelToggle );
			}

            for ( int i = 0; i < runeTagToggles.Length; i++ )
			{
				runeTagToggles[ i ] = runeTagGroup.GetChild ( i ).GetComponent<Toggle> ();
				runeTagToggles[ i ].AddListener ( OnClickSelectTagToggle );
			}

            runeTagButton.AddListener(OnClickTagTypeButton);
            runeBuyButton.AddListener(OnClickRuneBuyButton);
            runeSellButton.AddListener(OnClickRuneSellButton);                        
            runePageAddButton.AddListener ( OnClickShowRunePageBuyPanelButton );
            runeUninstallButton.AddListener(OnClickUninstallRuneButton);
            runeReplaceButton.AddListener(OnClickShowRuneAvailablePanelButton);
            renameButton.AddListener(OnClickShowRunePageRenamePanelButton);           
        }

        public void OnClickSelectLevelToggle( bool on )
		{
			for ( int i = 0; i < runeLevelToggles.Length; i++ )
			{
				if( runeLevelToggles[ i ].isOn )
				{
					int runeLevel = runeLevels[ i ];
					if( currentRuneLevel != runeLevel )
					{
						currentRuneLevel = runeLevel;
						LoadRunePackItem ();
					}
				}
			}
		}

		public void OnClickSelectTagToggle( bool on )
		{
			for ( int i = 0; i < runeTagToggles.Length; i++ )
			{
				if( runeTagToggles[ i ].isOn )
				{
					int runeTagId = runeTags[ i ];

					if( currentRuneTagId != runeTagId )
					{
						currentRuneTagId = runeTagId;
						LoadRunePackItem ();
                        runeTagGroupTitle = runeTagToggles[i].transform.Find("RuneTagItemText").GetComponent<Text>().text;
                        PackTagUIEffect(false, runeTagGroupTitle);
                    }
				}
			}
		}

        public void OnClickTagTypeButton()
        {           
            PackTagUIEffect(!runeTagGroupIsShow, runeTagGroupTitle);
        }

        #region TagButton UI Effect

        /*
         * 1.添加 Tag UI效果
         *   * 默认全部显示
         *   * 点击Tag按钮收起下拉框，显示分类灵石
         *   * 分类检索是
         */
        private float tagGroupTransformTime = 0;
        private bool playTagGroupAnimation = false;
        private float liney = 0;
        private bool runeTagGroupIsShow = false;
        private string runeTagGroupTitle = "物理";

        private void PackTagUIEffect(bool isshow,string title)
        {
            runeTagTitleLabel.text = string.Format("{0}",title);
            liney = 0;
            tagGroupTransformTime = Time.time;
            runeTagGroupIsShow = isshow;
            playTagGroupAnimation = true;
        }

        private void PackTagAnimaition(bool isshow)
        {
            if (isshow)
            {
                liney = 1;
                if (liney >= 1)
                {
                    liney = 1;
                    runeTagGroup.localScale = Vector3.one;
                    playTagGroupAnimation = false;
                    runeTagGroupIsShow = true;
                }
                runeTagGroup.localScale = new Vector3(1, Mathf.Lerp(0,1,liney), 1);
            }
            else
            {
                liney = 1;
                //liney = Time.time - tagGroupTransformTime;
                if (liney >= 1)
                {
                    liney = 1;
                    runeTagGroup.localScale = new Vector3(1, 0, 1);
                    playTagGroupAnimation = false;
                    runeTagGroupIsShow = false;
                }
                runeTagGroup.localScale = new Vector3(1, Mathf.Lerp(1, 0, liney), 1);
            }
        }

        void Update()
        {
            if (playTagGroupAnimation)
            {
                PackTagAnimaition(runeTagGroupIsShow);
            }
        }

        #endregion

        #region ButtonEvent

        /*
         * ?? TODO: 需要重构代码
         * 
         */
        private bool isflag = false;
        public RunePopView runeController;
        private void ShowRunePopViewUI(System.Object boj, RunePopType reunpoptype)
        {          
            if (isflag)
            {
                UIManager.Instance.GetUIByType(UIType.RunePopView, (ViewBase ui, System.Object param) => { (ui as RunePopView).EnterRunePopView(boj, reunpoptype); });
            }
            else
            {
                string path = "Prefabs/UI/RunePopView";            
                GameObject runepopview=Instantiate(Resources.Load(path),transform.parent.parent.Find("Layer_Five"))as GameObject;
                runepopview.transform.localPosition = Vector3.zero;
                runepopview.transform.localRotation = Quaternion.identity;
                runeController= runepopview.AddComponent<RunePopView>();
                runeController.EnterRunePopView(boj, reunpoptype);
            }
        }

		private void OnClickRuneBuyButton()
		{
            BuyEntity buyEntity = new BuyEntity();
            buyEntity.info = new RuneInfo();
            buyEntity.info.level = controller.GetRuneLevel(currentRuneId);
            buyEntity.info.iconid= controller.GetRuneIcon(currentRuneId);
            buyEntity.info.count= controller.GetRuneNumber(currentRuneId);
            buyEntity.info.nane= controller.GetRuneName(currentRuneId);
            buyEntity.info.itemattribute= controller.GetRuneAttribute(currentRuneId);
            buyEntity.info.describer= controller.GetRuneIntrotuce(currentRuneId);
            buyEntity.info.boughtNumber= controller.GetRuneBoughtNumber(currentRuneId);
            buyEntity.info.boughtNumberLimit= controller.GetRuneBoughtNumberLimit(currentRuneId);
            buyEntity.info.buyruneandprice= controller.GetItemCost(currentRuneId);
            buyEntity.buyEvent= (int count,int id, CurrencyType type)=> 
            {
                controller.SendRuneBuy(count, currentRuneId, type);
            };
            ShowRunePopViewUI(buyEntity, RunePopType.RuneBuyUI);
		}       

		private void OnClickRuneSellButton()
		{
            SellEntity sellEntity = new SellEntity();
            sellEntity.info = new RuneInfo();
            sellEntity.info.runeid = currentRuneId;
            sellEntity.info.level = controller.GetRuneLevel(currentRuneId);
            sellEntity.info.iconid = controller.GetRuneIcon(currentRuneId);
            sellEntity.info.count = controller.GetRuneNumber(currentRuneId);
            sellEntity.info.nane = controller.GetRuneName(currentRuneId);
            sellEntity.info.itemattribute = controller.GetRuneAttribute(currentRuneId);
            sellEntity.info.describer = controller.GetRuneIntrotuce(currentRuneId);
            sellEntity.info.sellprice_gold = controller.GetItemProto(currentRuneId).Price;
            sellEntity.sellEvent = (int count, int id) =>
            {
                controller.SendSellRune(count, currentRuneId);
            };
            ShowRunePopViewUI(sellEntity, RunePopType.RuneSellUI);
        }

        private void OnClickShowRunePageRenamePanelButton()
        {            
            RenameEntity entity = new RenameEntity();
            entity.name = currentRunePageName;
            entity.id = currentRunePageId;
            entity.renameEvent=(int pageid,string name)=> 
            {
                controller.SendRunePageRename(pageid, name);
            };
            ShowRunePopViewUI(entity, RunePopType.PageRenameUI);
        }

        private void OnClickShowRunePageBuyPanelButton()
        {
            BuyPageEntity entity = new BuyPageEntity();
            entity.id = controller.GetPageInfo().Count;
            entity.cost = controller.GetRunePageUnLockCost(entity.id+1);
            entity.buyEvent = (int id) => {
                controller.SendBuyRunePage(controller.GetPageInfo().Count);
            };
            ShowRunePopViewUI(entity, RunePopType.RunePageBuyUI);
        }

        private void OnClickUnlockRuneSlotCallback()
		{
			controller.SendUnlockRuneSlot ( currentRunePageId , currentSlotId , controller.GetRuneSlotType ( currentSlotId ) );
		}

        private void OnClickUninstallRuneButton()
		{
			controller.SendUnEquipRuneItem ( currentRunePageId , currentRuneId , currentSlotId );
            RuneRightChildChange(runePageInformationPanel.name);
        }

        // 灵石对比
		private  void OnClickShowRuneAvailablePanelButton()
		{
            openAttributePenalType = AttributePenalType.Contrast;
			ShowRuneAvailablePanel ( currentSlotId );	
		}

        #endregion

        #region TopTagButton --页面切换

        private void OnClickRunePackPanelToggle(bool isOn)
        {
            if (isOn)
            {
                RuneMainChildChange(0);
            }            
        }

        private void OnClickRuneConfigurePenalToggle(bool isOn)
        {
            if (isOn)
            {
                RuneMainChildChange(1);
            }           
        }

        private void RuneMainChildChange(int index)
        {
            switch (index)
            {
                case 0:  //Pack页面
                    //关闭所有Right页面
                    RuneRightChildChange(null);
                    runePackPanle.gameObject.SetActive(true);
                    runeConfigPanle.gameObject.SetActive(false);
                    runeTagGroupTitle = "物理";
                    runeTagTitleLabel.text = runeTagGroupTitle;
                    currentRuneTagId = runeTags[0];
                    currentRuneLevel = runeLevels[0];
                    LoadRunePackItem();
                    
                    break;
                case 1:   // 配置页面
                    RuneRightChildChange(null);
                    runePackPanle.gameObject.SetActive(false);
                    runeConfigPanle.gameObject.SetActive(true);
                    LoadRunePageItem();
                    break;
                default:
                    break;
            }
        }

        private void RuneRightChildChange(string name)
        {
            for (int i = 0; i < runeRight.childCount; i++)
            {
                GameObject go = runeRight.GetChild(i).gameObject;

                if (go.name == name)
                {
                    go.SetActive(true);
                }
                else
                {
                    if (go.activeSelf)
                    {
                        go.SetActive(false);
                    }
                }
            }
        }

        #endregion

        public void LoadRunePackItem()
		{
			GameResourceLoadManager.GetInstance ().LoadResource ( RUNE_PACK_ITEM_PATH , OnLoadRunePackItem , true );
		}

		public void LoadRunePageItem()
		{
			GameResourceLoadManager.GetInstance ().LoadResource ( RUNE_PAPG_ITEM_PATH , OnLoadRunePageItem , true );
		}

		public void LoadRuneAvailableItem()
		{
			GameResourceLoadManager.GetInstance ().LoadResource ( RUNE_AVAILABLE_ITEM_PATH , OnLoadRuneAvailableItem , true );
		}

        //实例化Rune
		private void OnLoadRunePackItem( string name, Object obj, System.Object param )
		{
			CommonUtil.ClearItemList<RunePackItem> ( runePackItemList );
			List<int> rundIdList = controller.GetRuneIdList ( currentRuneLevel , currentRuneTagId );
            bool isflag = false;
            RunePackItem currentSelectRunePackItem=null;
            for ( int i = 0; i < rundIdList.Count; i++ )
			{
                RunePackItem runePackItem;
                int runeId = rundIdList[ i ];
				if( i >= runePackItemList.Count )
				{
					runePackItem = CommonUtil.CreateItem<RunePackItem> ( obj , runePackGroup.transform );
					runePackItemList.Add ( runePackItem );
				}
				runePackItem = runePackItemList[ i ];              
                runePackItem.gameObject.SetActive ( true );
				runePackItem.number = controller.GetRuneNumber ( runeId );
				runePackItem.icon = controller.GetRuneIcon ( runeId );
				runePackItem.runeId = runeId;
                runePackItem.group = runeItemToggerGroup;
                runePackItem.onClickEvent = OnRunePackItemClick;               
                if (runePackItem.runeId==currentRuneId)
                {
                    runePackItem.GetComponent<Toggle>().isOn = true;
                    isflag = true;
                    currentSelectRunePackItem = runePackItem;
                }             
                runePackItem.RefreshItem();
            }
            if (rundIdList.Count > 0)
            {
                runePackItemNull.gameObject.SetActive(false);
                if (isflag&& currentSelectRunePackItem!=null)
                {
                    OnRunePackItemClick(currentSelectRunePackItem.runeId);
                }
                else
                {
                    runePackItemList[0].GetComponent<Toggle>().isOn = true;
                    OnRunePackItemClick(rundIdList[0]);
                }                
            }
            else
            {
                runePackItemNull.gameObject.SetActive(true);
                RuneRightChildChange("RunePanel");
            }
        }
       
       // 实例化灵石配置页面
        private void OnLoadRunePageItem(string name, Object obj, System.Object param)
        {
            CommonUtil.ClearItemList<RunePageItem>(runePageItemList);
            GameObject objRuneItem = null;
            for (int i = 0; i < runePageGroup.transform.childCount; i++)
            {
                objRuneItem = runePageGroup.transform.GetChild(i).gameObject;
                RunePageItem item = objRuneItem.GetComponent<RunePageItem>();
                if (item)
                {
                    objRuneItem.SetActive(false);
                    runePageItemList.Add(runePageGroup.transform.GetChild(i).GetComponent<RunePageItem>());
                }
            }

            //实例化
            List<RunePageInfo> listRunePageInfo = controller.GetPageInfo();
            currentPageCount = controller.GetPageInfo().Count;
            for (int i = 0; i < listRunePageInfo.Count; i++)
            {
                RunePageItem item;
                runePageItemList[i].gameObject.SetActive(true);
                item = runePageItemList[i];
                item.runePageId = i;
                item.runePageItemToggle.group = runePageGroup;
                item.onClickEvent = (int pageid) =>
                {
                    runedefault.isOn = true;
                    currentRunePageId = pageid;
                    InitRuneSlotItem(currentRunePageId);
                };
            }

            if (listRunePageInfo.Count >= MaxPageCount)
            {
                runePageAddButton.gameObject.SetActive(false);
            }
            else
            {
                runePageAddButton.transform.localPosition = runePageItemList[listRunePageInfo.Count].transform.localPosition;
            }
            runePageItemList[currentRunePageId].runePageItemToggle.isOn = true;
            runePageItemList[currentRunePageId].onClickEvent(currentRunePageId);
        }

        //显示可用的灵石
        private void OnLoadRuneAvailableItem(string name, Object obj, System.Object param)
        {
            CommonUtil.ClearItemList<RuneAvailableItem>(runeAvailableList);

            int slotType = controller.GetRuneSlotType(currentSlotId);
            List<int> runeIdList = controller.GetRuneIdList(slotType);

            for (int i = 0; i < runeIdList.Count; i++)
            {
                RuneAvailableItem runeAvailableItem;
                int runeId = runeIdList[i];

                if (runeIdList.Count > runeAvailableList.Count)
                {
                    runeAvailableItem = CommonUtil.CreateItem<RuneAvailableItem>(obj, runeAvailableGroup);
                    runeAvailableList.Add(runeAvailableItem);
                }
                runeAvailableItem = runeAvailableList[i];
                runeAvailableItem.gameObject.SetActive(true);

                runeAvailableItem.icon = controller.GetRuneIcon(runeId);
                runeAvailableItem.runeId = runeId;
                runeAvailableItem.openType = (int)openAttributePenalType;
                runeAvailableItem.runeName = controller.GetRuneName(runeId);
                runeAvailableItem.attribute = controller.GetRuneAttribute(runeId);
                runeAvailableItem.level = controller.GetRuneLevel(runeId);
                runeAvailableItem.RefreshItem();

                if (runeAvailableItem.openType == (int)AttributePenalType.Equipment)
                {
                    //装备灵石
                    runeAvailableItem.onClickEvent = OnSendEquipRuneItemClickCallBack;
                }
                else if (runeAvailableItem.openType == (int)AttributePenalType.Contrast)
                {
                    //打开灵石对比
                    runeAvailableItem.onClickEvent = OnShowRuneContrastPanelClickCallBack;
                }
            }
        }

        private List<RuneSlotItem> canBuyRuneSoltList;
        public void InitRuneSlotItem(int pageid)
		{
            canBuyRuneSoltList = new List<RuneSlotItem>();
            slotPageTotalLevel = 0;               
            List<RunePageInfo.RuneSlotInfo> rundSlotIdList = controller.GetRuneSlotsList (pageid);
            List<RunePageInfo.RuneSlotInfo> TWSlotList = new List<RunePageInfo.RuneSlotInfo>();
            List<RunePageInfo.RuneSlotInfo> HWSlotList = new List<RunePageInfo.RuneSlotInfo>();
            List<RunePageInfo.RuneSlotInfo> MWSlotList = new List<RunePageInfo.RuneSlotInfo>();

            for (int i = 0; i < rundSlotIdList.Count; i++)
            {
                RuneSlotType runeSoltType = (RuneSlotType)controller.GetRuneSlotType(rundSlotIdList[i].id);
                switch (runeSoltType)
                {
                    case RuneSlotType.URANUS:
                        TWSlotList.Add(rundSlotIdList[i]);                        
                        break;
                    case RuneSlotType.NEPTUNE:
                        HWSlotList.Add(rundSlotIdList[i]);
                        break;
                    case RuneSlotType.PLUTO:
                        MWSlotList.Add(rundSlotIdList[i]);
                        break;
                    default:
                        break;
                }             
            }
            InitSlotByType(TWSlotList, RuneSlotType.URANUS, twSlotPanel);
            InitSlotByType(HWSlotList, RuneSlotType.NEPTUNE, hwSlotPanel);
            InitSlotByType(MWSlotList, RuneSlotType.PLUTO, mwSlotPanel);
            SetCanBuySolt(canBuyRuneSoltList);
            ShowRunePageInformationPanel ();
        }     
        
        private void InitSlotByType(List<RunePageInfo.RuneSlotInfo> rundSlotInfoList,  RuneSlotType type,Transform slottransform)
        {
            for (int i = 0; i < rundSlotInfoList.Count; i++)
            {
                RunePageInfo.RuneSlotInfo runeSlot = rundSlotInfoList[i];
                RuneSlotItem runeSlotItem = slottransform.GetChild(i).GetComponent<RuneSlotItem>();
                runeSlotItem.slotType = type;
                runeSlotItem.state = (RuneSlotState)runeSlot.state;
                runeSlotItem.slotId = runeSlot.id;
                runeSlotItem.itemId = runeSlot.itemId;
                runeSlotItem.pageId = currentRunePageId;
                runeSlotItem.gameObject.GetComponent<Toggle>().isOn = false;
                if (runeSlot.itemId != 0 && runeSlotItem.state == RuneSlotState.INLAID)
                {
                    runeSlotItem.runeIcon = controller.GetRuneIcon(runeSlot.itemId);
                    slotPageTotalLevel += controller.GetRuneLevel(runeSlot.itemId);
                }

                if (runeSlotItem.state == RuneSlotState.CAN_BUY)
                {
                    canBuyRuneSoltList.Add(runeSlotItem);
                }  
                runeSlotItem.InitItem();
                AddSlotOnClick(runeSlotItem);
            }
        }

        // 设置可购买的槽位
        private void SetCanBuySolt(List<RuneSlotItem> SoltList)
        {
            SlotProto.Slot slottemp = new SlotProto.Slot();
            for (int i = 0; i < SoltList.Count; i++)
            {
                SlotProto.Slot slot = controller.GetLevelOpenSolt(SoltList[i].slotId);
                if (i == 0)
                {
                    slottemp = slot;
                }
                else
                {
                    if (slot.UnLockLevel < slottemp.UnLockLevel)
                    {
                        slottemp = slot;
                    }
                }                    
            }

            for (int i = 0; i < SoltList.Count; i++)
            {
                if (SoltList[i].slotId == slottemp.ID)
                {
                    SoltList[i].canBuyType = SlotCanBuyType.LevelOpen;
                    SoltList[i].openLevel = slottemp.UnLockLevel;
                }
                else
                {
                    SoltList[i].canBuyType = SlotCanBuyType.MoneyOpen;
                }
                SoltList[i].InitItem();
                AddSlotOnClick(SoltList[i]);
            }           

        }

        //刷新槽位信息
        public void RefreshRuneSlotItem()
        {
            List<RunePageInfo.RuneSlotInfo> rundSlotList = controller.GetRuneSlotsList(currentRunePageId);
            RunePageInfo.RuneSlotInfo runeSlotInfo;
            RuneSlotItem runeSlotItem;
            for (int i = 0; i < rundSlotList.Count; i++)
            {
                runeSlotInfo = rundSlotList[i];
                if (i <= 11)
                {
                    runeSlotItem = twSlotPanel.GetChild(i).GetComponent<RuneSlotItem>();
                }
                else if (i <= 23)
                {
                    int index = i - 12;
                    runeSlotItem = hwSlotPanel.GetChild(index).GetComponent<RuneSlotItem>();
                }
                else
                {
                    int index = i - 24;
                    runeSlotItem = mwSlotPanel.GetChild(index).GetComponent<RuneSlotItem>();
                }

                runeSlotItem.pageId = currentRunePageId;
                if (runeSlotItem.slotId == runeSlotInfo.id)
                {
                    runeSlotItem.state = (RuneSlotState)runeSlotInfo.state;
                    runeSlotItem.itemId = runeSlotInfo.itemId;
                    if (runeSlotInfo.itemId != 0 && runeSlotItem.state == RuneSlotState.INLAID)
                    {
                        runeSlotItem.runeIcon = controller.GetRuneIcon(runeSlotInfo.itemId);
                    }
                    runeSlotItem.InitItem();
                    AddSlotOnClick(runeSlotItem);
                }
            }            
            ShowRunePageInformationPanel();
        }

        //灵石槽根据状态添加click事件
        private void AddSlotOnClick(RuneSlotItem runeSlotItem)
        {
            runeSlotItem.onClickEvent = null;
            switch (runeSlotItem.state)
            {
                case RuneSlotState.CAN_UNLOCK:
                    {
                        runeSlotItem.onClickEvent = OnCanUnLockClick;
                        break;
                    }
                case RuneSlotState.CAN_BUY:
                    {
                        runeSlotItem.onClickEvent = OnShowRuneSlotBuyPanelClick;
                        break;
                    }
                case RuneSlotState.UNLOCK:
                    {
                        runeSlotItem.onClickEvent = OnShowRuneAvailablePanelClick;
                        break;
                    }
                case RuneSlotState.INLAID:
                    {
                        runeSlotItem.onClickEvent = OnShowRuneInformationClick;
                        break;
                    }
            }
        }
      
        //点击PackItem 显示信息
        private void OnRunePackItemClick(int runeId)
        {
            currentRuneId = runeId;
            ShowRuneInformation(runeId);
        }

        //购买灵石槽页面      
        private void OnShowRuneSlotBuyPanelClick(int pageId, int slotId, int itemId)
        {
            currentSlotId = slotId;
            SlotEntity entity = new SlotEntity();
            SlotInfo slot = new SlotInfo();
            slot.slotid = slotId;
            slot.pageid = currentRunePageId;
            slot.type =  controller.GetRuneSlotType(currentSlotId);
            slot.buycost = controller.GetRuneSlotUnLockCost(currentSlotId);
            entity.slot = slot;
            entity.buyEvent = (int pageid,int slotid,int type) => 
            {
                controller.SendUnlockRuneSlot(pageid, slotid, type);
            };
            ShowRunePopViewUI(entity, RunePopType.RuneSlotBuyUI);
        }

        // 灵石装备
        private void OnShowRuneAvailablePanelClick( int pageId, int slotId, int itemId )
        {
            currentSlotId = slotId;
            openAttributePenalType = AttributePenalType.Equipment;            
			ShowRuneAvailablePanel (slotId);
		}

        //解锁灵石槽页面
		private void OnCanUnLockClick( int pageId, int slotId, int itemId )
		{
			currentSlotId = slotId;
			controller.SendUnlockRuneSlot (pageId , slotId , controller.GetRuneSlotType ( slotId ) );
		}

        //显示灵石信息
		private void OnShowRuneInformationClick( int pageId, int slotId, int itemId )
		{
			currentSlotId = slotId;
			ShowSlotRuneInformation ( itemId , slotId );
		}

        //灵石装备
        private void OnSendEquipRuneItemClickCallBack( int runeId )
		{
			currentRuneId = runeId;
			controller.SendEquipRuneItem ( currentRunePageId , runeId , currentSlotId );
			RuneRightChildChange ( runeAvailablePanel.name );
		}

        //灵石对比
		private  void OnShowRuneContrastPanelClickCallBack( int runeId )
		{
            currentRuneId = runeId;
            RuneContrast contrastentity = new RuneContrast();
            RuneInfo newrune = new RuneInfo();
            RuneInfo originrune = new RuneInfo();
            SlotInfo info = new SlotInfo();
            info.slotid = currentSlotId;
           
            originrune.runeid= controller.GetRuneId(currentSlotId, currentRunePageId);
            originrune.iconid= controller.GetRuneIcon(originrune.runeid);
            originrune.level = controller.GetRuneLevel(originrune.runeid);
            originrune.nane= controller.GetRuneName(originrune.runeid);
            originrune.itemattribute= controller.GetRuneAttribute(originrune.runeid);

            newrune.runeid = currentRuneId;
            newrune.iconid= controller.GetRuneIcon(currentRuneId);
            newrune.level= controller.GetRuneLevel(currentRuneId);
            newrune.nane= controller.GetRuneName(currentRuneId);
            newrune.itemattribute= controller.GetRuneAttribute(currentRuneId);

            contrastentity.newrune = newrune;
            contrastentity.originrune = originrune;
            contrastentity.slot = info;
            contrastentity.replaceEvent=(int pageid,int newruneid,int slotid)=>
            {
                controller.SendEquipRuneItem(pageid, newruneid, slotid);
                RuneRightChildChange(runePageInformationPanel.name);
            };
            ShowRunePopViewUI(contrastentity,RunePopType.RuneContrastPanel);
        }
        
        //灵石信息
		public void  ShowRuneInformation( int runeId )
		{
			RuneRightChildChange ( runeInformationPanel.name );
			runeInformationPanel.level = controller.GetRuneLevel ( runeId );
			runeInformationPanel.icon = controller.GetRuneIcon ( runeId );
			runeInformationPanel.runeNumber = controller.GetRuneNumber ( runeId );
			runeInformationPanel.runeName = controller.GetRuneName ( runeId );
			runeInformationPanel.content = controller.GetRuneAttribute ( runeId );
			runeInformationPanel.RefreshPanel ();
		}

        //灵石槽的灵石信息
		public void  ShowSlotRuneInformation( int runeId, int slotId )
		{
			RuneRightChildChange ( slotRuneInformationPanel.name );
			slotRuneInformationPanel.level = controller.GetRuneLevel ( runeId );
			slotRuneInformationPanel.icon = controller.GetRuneIcon ( runeId );
			slotRuneInformationPanel.runeName = controller.GetRuneName ( runeId );
			slotRuneInformationPanel.attribute = controller.GetRuneAttribute ( runeId );
			slotRuneInformationPanel.RefreshPanel ();
		}

        //显示可用的灵石
        public void ShowRuneAvailablePanel(int slotId)
        {
            RuneRightChildChange(runeAvailablePanel.name);
            currentSlotId = slotId;
            LoadRuneAvailableItem();
        }

        //灵石页信息
        public void ShowRunePageInformationPanel()
        {
            runeSlotPageTotalLevel.text = slotPageTotalLevel.ToString();
            runePageAttribute = controller.GetRunePageAttribute(currentRunePageId);
            currentRunePageName = controller.GetRunePageName(currentRunePageId);
            if (currentRunePageName == "0")
            {
                currentRunePageName = string.Format("灵石页{0}", currentRunePageId + 1);
            }
            RuneRightChildChange(runePageInformationPanel.transform.name);
            runePageAttributeText.text = runePageAttribute;
            runePageTitleText.text = currentRunePageName;
        }

    }
}