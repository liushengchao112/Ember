using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Utils;
using Constants;

namespace UI
{
    public class RunePageRenameUIScript : MonoBehaviour
    {

        private const string TIP_TITLE = "提示";
        private const string TIP_CONTENT = "灵石页名字不能为空！";

        private const string TITLE = "重命名";
        private const string TIP = "请输入修改的名字";
        private const string RENAME = "确认修改";

        private Text txt_title;
        private Text txt_tip;
        private Text txt_rename;
        private InputField itfd_name;
        private Button btn_replace;
        private Button btn_exit;

        private RenameEntity renameinfo;

        public Action exitEvent;
        public Action<int,string> renameEvent;

        void Awake()
        {
            this.txt_title = this.transform.Find("TitleText").GetComponent<Text>();
            this.txt_tip = this.transform.Find("InputField/Placeholder").GetComponent<Text>();
            this.txt_rename = this.transform.Find("ConfirmRenameButton/ConfirmReplace Text").GetComponent<Text>();
            this.itfd_name = this.transform.Find("InputField").GetComponent<InputField>();      
            this.btn_exit = this.transform.Find("ExitButton").GetComponent<Button>();
            this.btn_replace = this.transform.Find("ConfirmRenameButton").GetComponent<Button>();

            this.txt_title.text = TITLE;
            this.txt_tip.text = TIP;
            this.txt_rename.text = RENAME;

            this.btn_exit.onClick.AddListener(() =>
            {
                exitEvent();
            });

            this.btn_replace.onClick.AddListener(() => 
            {
                Rename(renameinfo);
            });
        }

        public void Init(RenameEntity info)
        {
            renameinfo = info;
            this.itfd_name.text = info.name;
            renameEvent = info.renameEvent;
        }

        private void Rename(RenameEntity info)
        {
            if (string.IsNullOrEmpty(itfd_name.text.Trim()))
            {
                MessageDispatcher.PostMessage(MessageType.OpenAlertWindow, null, AlertType.ConfirmAlone, TIP_CONTENT, TIP_TITLE);
            }
            else
            {
                renameEvent(info.id, itfd_name.text.Trim());
            }           
        }
    }

    public class RenameEntity
    {
        public int id;
        public string name;
        public Action<int, string> renameEvent;
    }
}
