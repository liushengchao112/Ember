using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Resource;
using Utils;
using Data;

namespace UI
{
    public class RuneBuyUIScript : MonoBehaviour
    {
        private const string BUY_TO_UPPER_LIMIT = "已经达到购买上限";
        private const string BUY_TO_BOTTOM_LIMIT = "购买数量不能为0";
        private const string MONEY_NOT_ENOUGH = "{0}不足，请充值";
        private const string HAVE_COUNT = "拥有:  x{0}";
        private const string JEWEL = "钻石";
        private const string GOLD = "金币";
        private const string EMBERCOIN = "余烬币";
        private const string TITLE = "提示";
        private const string UI_TITLE = "购买";

        private Image img_icon;
        private Text txt_title;
        private Text txt_name;
        private Text txt_count;
        private Text txt_describer;
        private Text txt_attribute;
        private InputField itfd_sellcount;
        private Text txt_costgold;
        private Text txt_costcrystal;
        private Text txt_costember;

        private Button btn_buygold;
        private Button btn_buycrystal;
        private Button btn_buyEmbreCost;
        private Button btn_cancle;
        private Button btn_exit;
        private Button btn_add;
        private Button btn_reduce;

        public Action exitEvent;
        public Action<int, int, CurrencyType> buyEvent;

        private RuneInfo buyRuneInfo;
        private int buycount = 1;
        private int jewelCost, goldCost, emberCost;

        void Awake()
        {
            this.img_icon = this.transform.Find("FrameImage/RuneIcon").GetComponent<Image>();
            this.txt_title = this.transform.Find("TitleText").GetComponent<Text>();
            this.txt_name = this.transform.Find("FrameImage/RuneNameText").GetComponent<Text>();
            this.txt_count = this.transform.Find("FrameImage/RuneNumberText").GetComponent<Text>();
            this.txt_describer = this.transform.Find("DescribePanle/DescribeText").GetComponent<Text>();
            this.txt_attribute = this.transform.Find("DescribePanle/AttributeText").GetComponent<Text>();
            this.txt_costgold = this.transform.Find("GoldCostButton/GoldCostText").GetComponent<Text>();
            this.txt_costember = this.transform.Find("EmberCostButton/CrystalCostText").GetComponent<Text>();
            this.txt_costcrystal = this.transform.Find("CrystalCostButton/GoldCostText").GetComponent<Text>();

            this.itfd_sellcount = this.transform.Find("InputField").GetComponent<InputField>();

            this.btn_exit = this.transform.Find("Exit").GetComponent<Button>();
            this.btn_buygold = this.transform.Find("GoldCostButton").GetComponent<Button>();
            this.btn_buycrystal = this.transform.Find("CrystalCostButton").GetComponent<Button>();
            this.btn_buyEmbreCost = this.transform.Find("EmberCostButton").GetComponent<Button>();
            this.btn_cancle = this.transform.Find("CancleButton").GetComponent<Button>();
            this.btn_add = this.transform.Find("AddButton").GetComponent<Button>();
            this.btn_reduce = this.transform.Find("ReduceButton").GetComponent<Button>();

            this.txt_title.text = UI_TITLE;
            this.itfd_sellcount.text = buycount.ToString();

            this.btn_exit.onClick.AddListener(() => {
                exitEvent();
            });

            this.btn_add.onClick.AddListener(() => {
                Add();
            });

            this.btn_cancle.onClick.AddListener(() => {
                exitEvent();
            });

            this.btn_reduce.onClick.AddListener(() => {
                Reduce();
            });

            this.btn_buygold.onClick.AddListener(() => {
                Buy(buyRuneInfo.runeid, buycount, CurrencyType.GOLD);
            });

            this.btn_buycrystal.onClick.AddListener(() => {
                Buy(buyRuneInfo.runeid, buycount, CurrencyType.DIAMOND);
            });

            this.btn_buyEmbreCost.onClick.AddListener(() => {
                Buy(buyRuneInfo.runeid, buycount, CurrencyType.EMBER);
            });
        }

        public void Init(BuyEntity item)
        {
            buyRuneInfo = item.info;
            this.itfd_sellcount.text = buycount.ToString();
            this.txt_name.text = buyRuneInfo.nane;
            this.txt_attribute.text = buyRuneInfo.itemattribute;
            this.txt_describer.text = buyRuneInfo.describer;
            this.txt_count.text =string.Format(HAVE_COUNT, buyRuneInfo.count);
            this.buyEvent = item.buyEvent;
            GameResourceLoadManager.GetInstance().LoadAtlasSprite(buyRuneInfo.iconid, delegate (string name, AtlasSprite atlasSprite, System.Object param)
            {
                this.img_icon.SetSprite(atlasSprite);
            }, true);

            RefreshButton();

            this.txt_costgold.text = (buycount * goldCost).ToString();
            this.txt_costcrystal.text = (buycount * jewelCost).ToString();
            this.txt_costember.text = (buycount * emberCost).ToString();
        }

        private void RefreshButton()
        {
            btn_buygold.gameObject.SetActive(false);
            btn_buyEmbreCost.gameObject.SetActive(false);
            btn_buycrystal.gameObject.SetActive(false);

            if (buyRuneInfo.buyruneandprice.Count == 1)
            {
                switch (buyRuneInfo.buyruneandprice[0].Key)
                {
                    case RuneCostType.None:
                        break;
                    case RuneCostType.Jewel:
                        btn_buycrystal.gameObject.SetActive(true);
                        jewelCost = buyRuneInfo.buyruneandprice[0].Value;
                        break;
                    case RuneCostType.Gold:
                        btn_buygold.gameObject.SetActive(true);
                        goldCost = buyRuneInfo.buyruneandprice[0].Value;
                        break;
                    case RuneCostType.EmberCoin:
                        btn_buyEmbreCost.gameObject.SetActive(true);
                        emberCost = buyRuneInfo.buyruneandprice[0].Value;
                        break;
                    default:
                        break;
                }
                btn_cancle.gameObject.SetActive(true);
            }

            if (buyRuneInfo.buyruneandprice.Count == 2)
            {
                btn_cancle.gameObject.SetActive(true);
                switch (buyRuneInfo.buyruneandprice[1].Key)
                {
                    case RuneCostType.None:
                        break;
                    case RuneCostType.Jewel:
                        btn_buyEmbreCost.gameObject.SetActive(true);
                        jewelCost = buyRuneInfo.buyruneandprice[1].Value;
                        btn_buyEmbreCost.transform.localPosition = btn_cancle.transform.localPosition;
                        break;
                    case RuneCostType.Gold:
                        btn_buygold.gameObject.SetActive(true);
                        goldCost = buyRuneInfo.buyruneandprice[1].Value;
                        btn_buygold.transform.localPosition = btn_cancle.transform.localPosition;
                        break;
                    case RuneCostType.EmberCoin:
                        btn_buycrystal.gameObject.SetActive(true);
                        emberCost = buyRuneInfo.buyruneandprice[1].Value;
                        btn_buycrystal.transform.localPosition = btn_cancle.transform.localPosition;
                        break;
                    default:
                        break;
                }

                this.txt_costgold.text = (buycount * goldCost).ToString();
                this.txt_costcrystal.text = (buycount * jewelCost).ToString();
                this.txt_costember.text = (buycount * emberCost).ToString();
            }
        }

        private void Add()
        {
            if (buyRuneInfo.boughtNumberLimit != 0)
            {
                if ((buyRuneInfo.boughtNumber + buycount) < buyRuneInfo.boughtNumberLimit)
                {
                    buycount++;
                }
                else
                {
                    MessageDispatcher.PostMessage(Constants.MessageType.OpenAlertWindow, UI.AlertType.ConfirmAlone, BUY_TO_UPPER_LIMIT, TITLE);
                }
            }
            else
            {
                buycount++;
            }

            itfd_sellcount.text = buycount.ToString();
            this.txt_costgold.text = (buycount * goldCost).ToString();
            this.txt_costcrystal.text = (buycount * jewelCost).ToString();
            this.txt_costember.text = (buycount * emberCost).ToString();
        }

        private void Reduce()
        {
            if (buycount > 1)
            {
                buycount--;
                this.txt_costgold.text = (buycount * goldCost).ToString();
                this.txt_costcrystal.text = (buycount * jewelCost).ToString();
                this.txt_costember.text = (buycount * emberCost).ToString();
            }
            else
            {
                MessageDispatcher.PostMessage(Constants.MessageType.OpenAlertWindow, UI.AlertType.ConfirmAlone, BUY_TO_BOTTOM_LIMIT, TITLE);
            }
            itfd_sellcount.text = buycount.ToString();
            this.txt_costgold.text = (buycount * goldCost).ToString();
            this.txt_costcrystal.text = (buycount * jewelCost).ToString();
            this.txt_costember.text = (buycount * emberCost).ToString();
        }

        private void Buy(int runeid, int count, CurrencyType type)
        {
            switch (type)
            {
                case CurrencyType.DIAMOND:
                    if (DataManager.GetInstance().GetPlayerDiamond() < (count * jewelCost))
                    {
                        MessageDispatcher.PostMessage(Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, string.Format(MONEY_NOT_ENOUGH, JEWEL), TITLE);
                        return;
                    }
                    break;
                case CurrencyType.GOLD:
                    if (DataManager.GetInstance().GetPlayerGold() < (count * goldCost))
                    {
                        MessageDispatcher.PostMessage(Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, string.Format(MONEY_NOT_ENOUGH, GOLD), TITLE);
                        return;
                    }
                    break;
                case CurrencyType.EMBER:
                    if (DataManager.GetInstance().GetPlayerEmber() < (count * emberCost))
                    {
                        MessageDispatcher.PostMessage(Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, string.Format(MONEY_NOT_ENOUGH, EMBERCOIN), TITLE);
                        return;
                    }
                    break;
                default:
                    break;
            }
            buyEvent(count, runeid, type);
        }
    }

    public class BuyEntity
    {
        public RuneInfo info;
        public Action<int, int, CurrencyType> buyEvent;
    }
}