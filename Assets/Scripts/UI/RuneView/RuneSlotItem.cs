using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

using Resource;

namespace UI
{
    public enum RuneSlotState
    {
        LOCK=0,
        CAN_UNLOCK=1,
        CAN_BUY=2,
        UNLOCK=3,
        INLAID=4
    }

    public enum RuneSlotType
    {
        URANUS=1,
        NEPTUNE=2,
        PLUTO=3,
    }

    public enum SlotCanBuyType
    {
        LevelOpen=1,
        MoneyOpen=2,
    }


	public class RuneSlotItem : MonoBehaviour
	{
		public int slotId;
		//slot state 0 - lock, 1 - canUnlock, 2- canbuy, 3- unLock, 4- inlaid;
		public RuneSlotState state;
		public int itemId;
		public int runeIcon;
		public int pageId;
        public int openLevel;
        public SlotCanBuyType canBuyType;
        public  Action<int,int,int> onClickEvent;
		// 1-Uranus 2-Neptune 3-Pluto 
		public RuneSlotType slotType;

		public Toggle slotItemToggle;
		private Image slotImage;
        private Image borderImage;
        private Image lockImage;
        private Image lockbg;
        private Image lockMoney;

        private Text canOpenLevelText;

        private Transform unlockSolt;
        private Transform lockSolt;

        void Awake()
        {
            onClickEvent = null;
            slotItemToggle = transform.GetComponent<Toggle>();
            unlockSolt = transform.Find("Unlock");
            lockSolt = transform.Find("Lock");
            lockMoney = transform.Find("Lock/ImageMoney").GetComponent<Image>();
            lockbg = transform.Find("Lock/Imagebg").GetComponent<Image>();
            lockImage = transform.Find("Lock/Image").GetComponent<Image>();
            slotImage = transform.Find("Unlock/RuneSlotImage").GetComponent<Image>();
            borderImage = transform.Find("Unlock/RuneConfigureItem").GetComponent<Image>();
            canOpenLevelText = lockSolt.Find("Text").GetComponent<Text>();
            slotItemToggle.AddListener(OnClickToggle);
        }


		public void OnClickToggle( bool isOn )
		{
            if (isOn&& onClickEvent != null)
            {
                onClickEvent(pageId, slotId, itemId);
            }
        }

        //slot state 0 - lock, 1 - canUnlock, 2- canbuy, 3- unLock, 4- inlaid;
        public void InitItem()
		{
            switch ( state )
			{
				case RuneSlotState.LOCK: //Lock
					{
                        unlockSolt.gameObject.SetActive(false);
                        lockSolt.gameObject.SetActive(true);
                        lockImage.gameObject.SetActive(true);
                        lockbg.gameObject.SetActive(false);
                        lockMoney.gameObject.SetActive(false);
                        canOpenLevelText.gameObject.SetActive(false);
                        slotItemToggle.interactable = false;                        
						break;
					}

				case RuneSlotState.CAN_UNLOCK: 
					{
                        unlockSolt.gameObject.SetActive(false);
                        lockSolt.gameObject.SetActive(true);
                        lockImage.gameObject.SetActive(false);
                        lockbg.gameObject.SetActive(true);
                        canOpenLevelText.text = "可开启";
                        canOpenLevelText.gameObject.SetActive(true);
                        lockMoney.gameObject.SetActive(false);
                        slotItemToggle.interactable = true;
						break;
					}

				case RuneSlotState.CAN_BUY: //CanBuy
					{
                        unlockSolt.gameObject.SetActive(false);
                        lockSolt.gameObject.SetActive(true);
                        lockImage.gameObject.SetActive(false);

                        if (canBuyType == SlotCanBuyType.LevelOpen)
                        {
                            lockMoney.gameObject.SetActive(false);
                            lockbg.gameObject.SetActive(true);
                            canOpenLevelText.gameObject.SetActive(true);
                            canOpenLevelText.text = string.Format("{0}级\n可开启", openLevel);
                        }
                        else
                        {
                           lockbg.gameObject.SetActive(false);
                           canOpenLevelText.gameObject.SetActive(false);
                            lockMoney.gameObject.SetActive(true);
                        }
                        slotItemToggle.interactable = true;
                        break;
					}
				case RuneSlotState.UNLOCK: //UnLock
					{
						slotItemToggle.interactable = true;
                        unlockSolt.gameObject.SetActive(true);
                        lockSolt.gameObject.SetActive(false);
                        slotImage.gameObject.SetActive( false);
                        break;
					}

				case RuneSlotState.INLAID:  //inlaid
                    {
                        slotItemToggle.interactable = true;
                        unlockSolt.gameObject.SetActive(true);
                        lockSolt.gameObject.SetActive(false);
                        slotItemToggle.interactable = true;
                        slotImage.gameObject.SetActive(true);
                        GameResourceLoadManager.GetInstance().LoadAtlasSprite(                         
                            runeIcon, 
                            delegate ( string name, AtlasSprite atlasSprite, System.Object param ){
                               slotImage.SetSprite( atlasSprite );
                             }, 
                            true 
                        );
                        break;
					}
				default:
					break;
			}
		}

		public void  ShowSlotImage(string path,Image image)
		{
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( path, 
                delegate ( string name, AtlasSprite atlasSprite, System.Object param )
                {
                slotImage.SetSprite( atlasSprite );
                },
                true );
        }

	}
}
