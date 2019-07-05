using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Data;

namespace UI
{
    public class RuneBuySlotScript : MonoBehaviour
    {
        private const string MONEY_NOT_ENOUGH = "{0}不足，请充值";
        private const string TIP_TITLE = "提示";
        private const string JEWEL = "钻石";

        private const string TITLE = "购买";
        private const string SHOW_MESSAGE = "是否要购买新的槽位？";

        private Text txt_title;
        private Text txt_tip;
        private Text txt_cost;
        private Button btn_exit;
        private Button btn_buy;

        public Action exitEvent;
        public Action<int, int, int> buyEvent;

        private SlotInfo slotinfo;

        void Awake()
        {
            this.txt_title = this.transform.Find("TitleText").GetComponent<Text>();
            this.txt_tip = this.transform.Find("ContentText").GetComponent<Text>();
            this.txt_cost = this.transform.Find("ConfirmReplaceButton/CostText").GetComponent<Text>();
            this.btn_buy = this.transform.Find("ConfirmReplaceButton").GetComponent<Button>();
            this.btn_exit = this.transform.Find("ExitButton").GetComponent<Button>();

            this.txt_title.text = TITLE;
            this.txt_tip.text = SHOW_MESSAGE;

            btn_exit.onClick.AddListener(() =>
            {
                exitEvent();
            });

            btn_buy.onClick.AddListener(() =>
            {
                BuySlot(this.slotinfo);
            });
        }

        public void Init(SlotEntity entity)
        {            
            this.slotinfo = entity.slot;
            this.buyEvent = entity.buyEvent;
            this.txt_cost.text = entity.slot.buycost.ToString();
        }

        private void BuySlot(SlotInfo sinfo)
        {
            if (sinfo.buycost <= DataManager.GetInstance().GetPlayerDiamond())
            {
                buyEvent(sinfo.pageid, sinfo.slotid, sinfo.type);
            }
            else
            {
                Utils.MessageDispatcher.PostMessage(Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, string.Format(MONEY_NOT_ENOUGH, JEWEL), TIP_TITLE);
                exitEvent();
            }
        }
    }

    public class SlotEntity
    {
        public SlotInfo slot;
        public Action<int, int, int> buyEvent;

    }
}
