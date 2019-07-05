using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Resource;

using Utils;
using Constants;

namespace UI
{
    public class RuneSellUIScript : MonoBehaviour
    {
        private const string SELL_TO_UPPER_LIMIT = "已经达到出售上限";
        private const string SELL_TO_BOTTOM_LIMIT = "出售数量不能为0";
        private const string HAVE_COUNT = "拥有:  x{0}";
        private const string TIP_TITLE = "提示";

        private const string TITLE = "出售";

        private Image img_icon;
        private Text txt_title;
        private Text txt_name;
        private Text txt_count;
        private Text txt_describer;
        private Text txt_attribute;
        private InputField itfd_sellcount;
        private Text txt_getgold;

        private Button btn_sell;
        private Button btn_exit;
        private Button btn_add;
        private Button btn_reduce;

        public Action exitEvent;
        public Action<int, int> sellEvent;

        private RuneInfo sellRuneItem;

        private int sellcount = 1;

        void Awake()
        {
            this.img_icon = this.transform.Find("FrameImage/RuneIcon").GetComponent<Image>();
            this.txt_title = this.transform.Find("TitleText").GetComponent<Text>();
            this.txt_name = this.transform.Find("FrameImage/RuneNameText").GetComponent<Text>();
            this.txt_count = this.transform.Find("FrameImage/RuneNumberText").GetComponent<Text>();
            this.txt_describer = this.transform.Find("DescribePanle/DescribeText").GetComponent<Text>();
            this.txt_attribute = this.transform.Find("DescribePanle/AttributeText").GetComponent<Text>();
            this.txt_getgold = this.transform.Find("SellButton/CrystalCostText").GetComponent<Text>();

            this.itfd_sellcount= this.transform.Find("InputField").GetComponent<InputField>();

            this.btn_exit = this.transform.Find("Exit").GetComponent<Button>();
            this.btn_sell = this.transform.Find("SellButton").GetComponent<Button>();
            this.btn_add = this.transform.Find("AddButton").GetComponent<Button>();
            this.btn_reduce = this.transform.Find("ReduceButton").GetComponent<Button>();

            this.txt_title.text = TITLE;
            this.itfd_sellcount.text = sellcount.ToString();

            this.btn_exit.onClick.AddListener(() => {
                exitEvent();
            });

            this.btn_add.onClick.AddListener(() => {
                Add();
            });

            this.btn_reduce.onClick.AddListener(() =>
            {
                Reduce();
            });

            this.btn_sell.onClick.AddListener(() => {
                Sell();
            }); 
        }

        public void Init(SellEntity sellEntity)
        {
            sellRuneItem = sellEntity.info;
            sellEvent = sellEntity.sellEvent;
            this.itfd_sellcount.text = sellcount.ToString();
            this.txt_name.text = sellRuneItem.nane;
            this.txt_attribute.text = sellRuneItem.itemattribute;
            this.txt_describer.text = sellRuneItem.describer;       
            this.txt_count.text =string.Format(HAVE_COUNT, sellRuneItem.count);
            GameResourceLoadManager.GetInstance().LoadAtlasSprite(sellRuneItem.iconid, delegate (string name, AtlasSprite atlasSprite, System.Object param)
            {
                this.img_icon.SetSprite(atlasSprite);
            }, true);
            this.txt_getgold.text = (sellcount * sellRuneItem.sellprice_gold).ToString();
        }

        private void Add()
        {
            if (sellcount<sellRuneItem.count)
            {
                sellcount++;
            }
            else
            {
                MessageDispatcher.PostMessage(MessageType.OpenAlertWindow,null, AlertType.ConfirmAlone, SELL_TO_UPPER_LIMIT, TITLE);
            }
            this.itfd_sellcount.text = sellcount.ToString();
            this.txt_getgold.text = (sellcount * sellRuneItem.sellprice_gold).ToString();
        }

        private void Reduce()
        {
            if (sellcount > 1)
            {
                sellcount--;                
            }
            else
            {
                MessageDispatcher.PostMessage(MessageType.OpenAlertWindow,null, AlertType.ConfirmAlone, SELL_TO_BOTTOM_LIMIT, TITLE);
            }
            this.itfd_sellcount.text = sellcount.ToString();
            this.txt_getgold.text = (sellcount * sellRuneItem.sellprice_gold).ToString();
        }

        private void Sell()
        {
            sellEvent(sellcount,sellRuneItem.runeid);
        }
    }

    public class SellEntity
    {
        public RuneInfo info;
        public Action<int, int> sellEvent;
    }

    public enum RuneCostType
    {
        None = 0,
        Jewel = 1,
        Gold = 2,
        EmberCoin = 3,
    }
}