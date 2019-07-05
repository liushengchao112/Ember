using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Resource;

namespace UI
{
    public class RuneContrastUIScript : MonoBehaviour
    {
        private  string TITLE = "灵石对比";

        private RuneItem orgineRune ;
        private RuneItem newRune ;

        private SlotInfo slotinfo;
        private RuneInfo runeinfo;

        private Text txt_title;
        private Button btn_replace;
        private Button btn_exit;
        

        public Action exitEvent;
        public Action<int, int, int> replaceEvent;

        void Awake()
        {
            txt_title =  this.transform.Find("TitleText").GetComponent<Text>();
            txt_title.text = TITLE;

            orgineRune = new RuneItem(this.transform.Find("backgroundLeft/FrameImage/backgroundLeft").GetComponent<Text>(),
                    this.transform.Find("backgroundLeft/FrameImage/RuneIcon").GetComponent<Image>(),                    
                    this.transform.Find("backgroundLeft/AttributeText").GetComponent<Text>());
            newRune = new RuneItem(this.transform.Find("backgroundRight/FrameImage/RuneNamenAndLevelText").GetComponent<Text>(),
                    this.transform.Find("backgroundRight/FrameImage/RuneIcon").GetComponent<Image>(),
                    this.transform.Find("backgroundRight/AttributeText").GetComponent<Text>());

            btn_replace = this.transform.Find("ConfirmReplaceButton").GetComponent<Button>();
            btn_exit = this.transform.Find("RuneContrastExitButton").GetComponent<Button>();

            btn_replace.onClick.AddListener(() =>
            {
                Replace();
            });

            btn_exit.onClick.AddListener(() => 
            {
                exitEvent();
            });
        }

        public void Init(RuneContrast runecontrast)
        {
            this.runeinfo = runecontrast.newrune;
            this.slotinfo = runecontrast.slot;
            this.replaceEvent = runecontrast.replaceEvent;
            orgineRune.Init(runecontrast.originrune);
            newRune.Init(runecontrast.newrune);
        }

        private void Replace()
        {
            replaceEvent(slotinfo.pageid, runeinfo.runeid, slotinfo.slotid);
        }
    }

    public class RuneContrast
    {
        public RuneInfo originrune;
        public RuneInfo newrune;
        public SlotInfo slot;
        public Action<int, int, int> replaceEvent;
    }

    public class RuneItem
    {
        public Text  name;
        public Image runeIcon;
        public Text attribute;

        public RuneItem(Text itemname,Image icon,Text itemattribute)
        {
            this.name = itemname;
            this.runeIcon = icon;
            this.attribute = itemattribute;
        }
        public void Init(RuneInfo info)
        {
            this.name.text = String.Format("{0}级  {1}", info.level, info.nane);
            GameResourceLoadManager.GetInstance().LoadAtlasSprite(info.iconid, delegate (string name, AtlasSprite atlasSprite, System.Object param)
            {
               this.runeIcon.SetSprite(atlasSprite);
            }, true);
            this.attribute.text = info.itemattribute;
        }
    }

}
