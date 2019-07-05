using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StoreRuneItem : MonoBehaviour
    {
        public delegate void ClickBuyCallBack( int index );

        public ClickBuyCallBack ClickBuyButtonEvent;
        public int icon, RuneCost, index, currentItemType, remainCount;
        public string attributeStr, nameStr;
        public bool haveLimit;
        public StoreCostCurrecyType currecyType;
        
        private Image itemImage, goldIcon, emberIcon, diamondsIcon;
        private Text nameText, costText, attributeText, remainNumText;
        private Button buyButton;

        private void Awake()
        {
            goldIcon = transform.Find( "GoldIconImage" ).GetComponent<Image>();
            emberIcon = transform.Find( "EmberIconImage" ).GetComponent<Image>();
            diamondsIcon = transform.Find( "DiamondsIconImage" ).GetComponent<Image>();
            costText = transform.Find( "CostText" ).GetComponent<Text>();

            itemImage = transform.Find( "ItemImage" ).GetComponent<Image>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            attributeText = transform.Find( "AttributeText" ).GetComponent<Text>();
            remainNumText = transform.Find( "RemainNumText" ).GetComponent<Text>();

            buyButton = transform.Find( "BuyButton" ).GetComponent<Button>();

            buyButton.AddListener( OnClickBuyButton );
        }

        private void OnClickBuyButton()
        {
            if ( haveLimit )
            {
                remainNumText.text = "剩余:" + remainCount;
                if ( remainCount == 0 )
                {
                    string content = "今天购买数量已达上限，请明日购买";

                    string titleText = "提示";

                    Utils.MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, AlertType.ConfirmAlone, content, titleText );
                    return;
                }
            }

            ClickBuyButtonEvent.Invoke( index );
        }

        public void RefreshRuneItem()
        {
            if ( icon != 0 )
                Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    itemImage.SetSprite( atlasSprite );
                }, true );

            costText.text = RuneCost.ToString();
            attributeText.text = attributeStr;
            nameText.text = nameStr;

            remainNumText.gameObject.SetActive( haveLimit );

            SetItemUI();
        }

        private void SetItemUI()
        {
            goldIcon.gameObject.SetActive( false );
            emberIcon.gameObject.SetActive( false );
            diamondsIcon.gameObject.SetActive( false );
            switch ( currecyType )
            {
                case StoreCostCurrecyType.OnlyEmber:
                    emberIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.OnlyDiamonds:
                    diamondsIcon.gameObject.SetActive( true );
                    break;
                case StoreCostCurrecyType.OnlyGold:
                    goldIcon.gameObject.SetActive( true );
                    break;
            }
        }
    }
}
