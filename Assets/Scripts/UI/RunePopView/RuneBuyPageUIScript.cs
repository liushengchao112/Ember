using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Data;

namespace UI
{
    public class RuneBuyPageUIScript : MonoBehaviour
    {
        private const string MONEY_NOT_ENOUGH = "{0}不足，请充值";
        private const string TIP_TITLE = "提示";
        private const string JEWEL = "钻石";

        private const string TITLE="购买";
        private const string SHOW_MESSAGE = "购买后增加一格灵石页，是否要继续购买？";

        private Text txt_cost;
        private Button btn_exit;
        private Button btn_buy;

        private int costrmb;
        private int pageid;

        public Action exitEvent;
        public Action<int> buyEvent;
            
        void Awake()
        {
            this.txt_cost = this.transform.Find("ConfirmReplaceButton/CostText").GetComponent<Text>();
            this.btn_buy = this.transform.Find("ConfirmReplaceButton").GetComponent<Button>();
            this.btn_exit = this.transform.Find("ExitButton").GetComponent<Button>();

            btn_exit.onClick.AddListener(() => 
            {
                exitEvent();
            });

            btn_buy.onClick.AddListener(() => 
            {
                BuyPage(this.pageid);
            });
        }

        public void Init(BuyPageEntity buypage)
        {
            costrmb = buypage.cost;
            this.pageid = buypage.id;
            buyEvent = buypage.buyEvent;
            this.txt_cost.text = buypage.cost.ToString();
        }
        
        private void BuyPage(int id)
        {
            if (costrmb <= DataManager.GetInstance().GetPlayerDiamond())
            {
                buyEvent(id);
            }
            else
            {
                Utils.MessageDispatcher.PostMessage(Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, string.Format(MONEY_NOT_ENOUGH, JEWEL), TIP_TITLE);
                exitEvent();
            }           
        }       
    }
    
    public class BuyPageEntity
    {
        public int id;
        public int cost;
        public Action<int> buyEvent;
    }
}
