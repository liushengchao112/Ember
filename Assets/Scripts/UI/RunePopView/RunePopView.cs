using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace UI
{
    public enum RunePopType
    {
       PageRenameUI,
       RuneSellUI,
       RuneBuyUI,
       RuneContrastPanel,
       RunePageBuyUI,
       RuneSlotBuyUI
    }

    public class RunePopView : ViewBase
    {
        private RuneBuyPageUIScript runeBuyPageScript;
        private RuneBuySlotScript runeBuySlotScript;
        private RuneBuyUIScript runeBuyUIScript;
        private RuneContrastUIScript runeContrasUIScript;
        private RunePageRenameUIScript runePageRenameUIScript;
        private RuneSellUIScript runeSellUIScript;

        public override void OnInit()
        {
            base.OnInit();
        }

        public override void OnExit(bool isGoBack)
        {
            base.OnExit(isGoBack);
        }

        void Awake()
        {
            RunePopInit();
        }

        private void RunePopInit()
        {
            this.runeBuyPageScript = this.transform.Find("RunePageBuyPanel").gameObject.AddComponent<RuneBuyPageUIScript>();
            this.runeBuySlotScript = this.transform.Find("RuneSlotBuyPanel").gameObject.AddComponent<RuneBuySlotScript>();
            this.runeBuyUIScript = this.transform.Find("RuneBuyPanel").gameObject.AddComponent<RuneBuyUIScript>();
            this.runeSellUIScript = this.transform.Find("RuneSellPanel").gameObject.AddComponent<RuneSellUIScript>();
            this.runeContrasUIScript = this.transform.Find("RuneContrastPanel").gameObject.AddComponent<RuneContrastUIScript>();
            this.runePageRenameUIScript = this.transform.Find("RunePageRenamePanel").gameObject.AddComponent<RunePageRenameUIScript>();
        }

        public void EnterRunePopView(System.Object obj, RunePopType type)
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            switch (type)
            {
                case RunePopType.PageRenameUI:
                    runePageRenameUIScript.gameObject.SetActive(true);
                    runePageRenameUIScript.Init((RenameEntity)obj);
                    runePageRenameUIScript.exitEvent = Close;
                    break;
                case RunePopType.RuneSellUI:
                    runeSellUIScript.gameObject.SetActive(true);
                    runeSellUIScript.Init((SellEntity)obj);
                    runeSellUIScript.exitEvent = Close;
                    break;
                case RunePopType.RuneBuyUI:
                    
                    runeBuyUIScript.gameObject.SetActive(true);
                    runeBuyUIScript.Init((BuyEntity)obj);
                    runeBuyUIScript.exitEvent = Close;
                    break;
                case RunePopType.RuneContrastPanel:
                    runeContrasUIScript.gameObject.SetActive(true);
                    runeContrasUIScript.Init((RuneContrast)obj);
                    runeContrasUIScript.exitEvent = Close;
                    break;
                case RunePopType.RunePageBuyUI:
                    runeBuyPageScript.gameObject.SetActive(true);
                    runeBuyPageScript.Init((BuyPageEntity)obj);
                    runeBuyPageScript.exitEvent = Close;
                    break;
                case RunePopType.RuneSlotBuyUI:
                    runeBuySlotScript.gameObject.SetActive(true);
                    runeBuySlotScript.Init((SlotEntity)obj);
                    runeBuySlotScript.exitEvent = Close;
                    break;
                default:
                    break;
            }
        }

        public void Close()
        {
            Destroy(this.gameObject);
        }
    }
}
