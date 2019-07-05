using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Resource;
using StoreItemData = UI.StoreController.StoreItemData;


namespace UI
{
    public class StoreItem : ScrollViewItemBase
    {
        public enum ButtonType
        {
            None = 0,
            OnlyBuy = 1,
            OnlyExchange = 2,
            BuyAndGive = 3,
        }

        public delegate void ClickBuyCallBack( int index );
        public delegate void ClickGiveCallBack( int index );

        public StoreItemData info;

        public ClickBuyCallBack ClickBuyButtonEvent;
        public ClickGiveCallBack ClickGiveButtonEvent;

        private GameObject leftCostObj, rightCostObj;

        private Image itemImage, leftGoldIcon, rightGoldIcon, rightEmberIcon, rightDiamondsIcon, rightDebrisSkinIcon, rightDebrisUnitIcon, rightDebrisRuneIcon;
        private Text nameText, buyAloneText, leftBuyText, rightGiveText, leftCostText, rightCostText, remainNumText;
        private Button buyAloneBt, leftBuyBt, rightGiveBt;

        private void Awake()
        {
            leftCostObj = transform.Find( "CostGroup/LeftCost" ).gameObject;
            rightCostObj = transform.Find( "CostGroup/RightCost" ).gameObject;

            itemImage = transform.Find( "ItemImage" ).GetComponent<Image>();
            leftGoldIcon = transform.Find( "CostGroup/LeftCost/GoldIconImage" ).GetComponent<Image>();
            rightGoldIcon = transform.Find( "CostGroup/RightCost/GoldIconImage" ).GetComponent<Image>();
            rightEmberIcon = transform.Find( "CostGroup/RightCost/EmberIconImage" ).GetComponent<Image>();
            rightDiamondsIcon = transform.Find( "CostGroup/RightCost/DiamondsIconImage" ).GetComponent<Image>();
            rightDebrisSkinIcon = transform.Find( "CostGroup/RightCost/DebrisSkinIconImage" ).GetComponent<Image>();
            rightDebrisUnitIcon = transform.Find( "CostGroup/RightCost/DebrisUnitIconImage" ).GetComponent<Image>();
            rightDebrisRuneIcon = transform.Find( "CostGroup/RightCost/DebrisRuneIconImage" ).GetComponent<Image>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            buyAloneText = transform.Find( "BuyAloneText" ).GetComponent<Text>();
            leftBuyText = transform.Find( "BuyLeftText" ).GetComponent<Text>();
            rightGiveText = transform.Find( "GiveRightText" ).GetComponent<Text>();
            leftCostText = transform.Find( "CostGroup/LeftCost/CostText" ).GetComponent<Text>();
            rightCostText = transform.Find( "CostGroup/RightCost/CostText" ).GetComponent<Text>();
            remainNumText = transform.Find( "RemainNumText" ).GetComponent<Text>();

            buyAloneBt = transform.Find( "BuyAloneButton" ).GetComponent<Button>();
            leftBuyBt = transform.Find( "BuyLeftButton" ).GetComponent<Button>();
            rightGiveBt = transform.Find( "GiveRightButton" ).GetComponent<Button>();

            buyAloneBt.AddListener( OnClickBuyButton );
            leftBuyBt.AddListener( OnClickBuyButton );
            rightGiveBt.AddListener( OnClickGiveButton );
        }

        public override void UpdateItemData( object dataObj )
        {
            base.UpdateItemData( dataObj );
            if ( dataObj == null )
                return;
            info = (StoreItemData)dataObj;
            RefreshStoreItem();
        }

        #region Button Event

        private void OnClickBuyButton()
        {
            if ( info.haveLimit )
            {
                remainNumText.text = "剩余:" + info.remainCount;
                if ( info.remainCount == 0 )
                {
                    string content = "今天购买数量已达上限，请明日购买";

                    string titleText = "提示";

                    Utils.MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, AlertType.ConfirmAlone, content, titleText );
                    return;
                }
            }

            if ( ClickBuyButtonEvent != null )
                ClickBuyButtonEvent.Invoke( info.index );
        }

        private void OnClickGiveButton()
        {
            ClickGiveButtonEvent.Invoke( info.index );
        }

        #endregion

        public void RefreshStoreItem()
        {
            if ( info.icon != 0 )
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( info.icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    itemImage.SetSprite( atlasSprite );
                }, true );

            nameText.text = info.nameStr;
            leftCostText.text = info.costLeft.ToString();
            rightCostText.text = info.costRight.ToString();

            remainNumText.gameObject.SetActive( info.haveLimit );

            SetItemUI();
        }

        private void SetItemUI()
        {
            #region Bt Type
            switch ( info.btType )
            {
                case ButtonType.OnlyBuy:
                    buyAloneBt.gameObject.SetActive( true );
                    buyAloneText.gameObject.SetActive( true );
                    leftBuyBt.gameObject.SetActive( false );
                    leftBuyText.gameObject.SetActive( false );
                    rightGiveBt.gameObject.SetActive( false );
                    rightGiveText.gameObject.SetActive( false );

                    buyAloneText.text = "购  买";
                    break;
                case ButtonType.OnlyExchange:
                    buyAloneBt.gameObject.SetActive( true );
                    buyAloneText.gameObject.SetActive( true );
                    leftBuyBt.gameObject.SetActive( false );
                    leftBuyText.gameObject.SetActive( false );
                    rightGiveBt.gameObject.SetActive( false );
                    rightGiveText.gameObject.SetActive( false );

                    buyAloneText.text = "兑  换";
                    break;
                case ButtonType.BuyAndGive:
                    buyAloneBt.gameObject.SetActive( false );
                    buyAloneText.gameObject.SetActive( false );
                    leftBuyBt.gameObject.SetActive( true );
                    leftBuyText.gameObject.SetActive( true );
                    rightGiveBt.gameObject.SetActive( true );
                    rightGiveText.gameObject.SetActive( true );
                    break;
            }
            #endregion
            #region Currency Type
            leftCostObj.SetActive( false );
            rightGoldIcon.gameObject.SetActive( false );
            rightEmberIcon.gameObject.SetActive( false );
            rightDiamondsIcon.gameObject.SetActive( false );
            rightDebrisRuneIcon.gameObject.SetActive( false );
            rightDebrisSkinIcon.gameObject.SetActive( false );
            rightDebrisUnitIcon.gameObject.SetActive( false );

            switch ( info.currencyType )
            {
                case StoreCostCurrecyType.OnlyEmber:
                    rightEmberIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.OnlyDiamonds:
                    rightDiamondsIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.OnlyGold:
                    rightGoldIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.OnlyRuneDebris:
                    rightDebrisRuneIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.OnlySkinDebris:
                    rightDebrisSkinIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.OnlyUnitDebris:
                    rightDebrisUnitIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.GoldAndEmber:
                    leftCostObj.SetActive( true );
                    rightEmberIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.GoldAndDiamonds:
                    leftCostObj.SetActive( true );
                    rightDiamondsIcon.gameObject.SetActive( true );
                    break;
            }
            #endregion
        }
    }
}