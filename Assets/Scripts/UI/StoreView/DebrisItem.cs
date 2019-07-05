using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using Resource;
using DebrisItemData = UI.StoreController.StoreItemData;

namespace UI
{
    public class DebrisItem : ScrollViewItemBase
    {
        public delegate void ClickExChangeCallBack( int index );

        public DebrisItemData info;

        public ClickExChangeCallBack clickExChangeEvent;

        private Button exChangeButton;
        private Image itemImage, debrisSkinIcon, debrisUnitIcon, debrisRuneIcon;
        private Text costText, debrisTypeText, nameText,remainNumText;

        private void Awake()
        {
            exChangeButton = transform.Find( "ExChangeButton" ).GetComponent<Button>();
            itemImage = transform.Find( "ItemImage" ).GetComponent<Image>();
            debrisSkinIcon = transform.Find( "DebrisSkinIconImage" ).GetComponent<Image>();
            debrisUnitIcon = transform.Find( "DebrisUnitIconImage" ).GetComponent<Image>();
            debrisRuneIcon = transform.Find( "DebrisRuneIconImage" ).GetComponent<Image>();
            costText = transform.Find( "CostText" ).GetComponent<Text>();
            debrisTypeText = transform.Find( "DebrisTypeText" ).GetComponent<Text>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            remainNumText = transform.Find( "RemainNumText" ).GetComponent<Text>();

            exChangeButton.AddListener( OnClickExChangeButton );
        }

        public override void UpdateItemData( object dataObj )
        {
            base.UpdateItemData( dataObj );
            if ( dataObj == null )
                return;
            info = (DebrisItemData)dataObj;
            RefreshDebrisItem();
        }

        private void OnClickExChangeButton()
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

            if ( clickExChangeEvent != null )
                clickExChangeEvent.Invoke( info.index );
        }

        private void RefreshDebrisItem()
        {
            if ( info.icon != 0 )
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( info.icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    itemImage.SetSprite( atlasSprite );
                }, true );

            costText.text = "X" + info.costRight;
            nameText.text = info.nameStr;
            remainNumText.gameObject.SetActive( info.haveLimit );

            SetItemUI();
        }

        private void SetItemUI()
        {
            debrisSkinIcon.gameObject.SetActive( false );
            debrisUnitIcon.gameObject.SetActive( false );
            debrisRuneIcon.gameObject.SetActive( false );
            switch ( info.currencyType )
            {
                case StoreCostCurrecyType.OnlySkinDebris:
                    debrisSkinIcon.gameObject.SetActive( true );
                    debrisTypeText.text = "皮肤碎片";
                    break;
                case StoreCostCurrecyType.OnlyUnitDebris:
                    debrisUnitIcon.gameObject.SetActive( true );
                    debrisTypeText.text = "英雄碎片";
                    break;
                case StoreCostCurrecyType.OnlyRuneDebris:
                    debrisRuneIcon.gameObject.SetActive( true );
                    debrisTypeText.text = "灵石碎片";
                    break;
            }
        }
    }
}