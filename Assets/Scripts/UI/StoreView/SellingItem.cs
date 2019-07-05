using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SellingItem : MonoBehaviour
    {
        public delegate void ClickBuyCallBack( int index );

        public ClickBuyCallBack ClickBuyButtonCallBack;
        public int icon, index ,goodPrice;
        public Data.CurrencyType costType;
        public string nameStr;

        private Image iconImage,goldIcon,emberIcon,diamondIcon;
        private Text nameText, buyText, priceText;
        private Button buyButton;

        private void Awake()
        {
            iconImage = transform.Find( "IconImage" ).GetComponent<Image>();
            goldIcon = transform.Find( "GoldIcon" ).GetComponent<Image>();
            emberIcon = transform.Find( "EmberIcon" ).GetComponent<Image>();
            diamondIcon = transform.Find( "DiamondIcon" ).GetComponent<Image>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            buyText = transform.Find( "BuyText" ).GetComponent<Text>();
            priceText = transform.Find( "CountText" ).GetComponent<Text>();
            buyButton = transform.Find( "BuyButton" ).GetComponent<Button>();

            buyButton.AddListener( OnClickBuyButton );
        }

        private void OnClickBuyButton()
        {
            ClickBuyButtonCallBack.Invoke( index );
        }

        public void RefreshItem()
        {
            if ( icon != 0 )
                Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    iconImage.SetSprite( atlasSprite );
                }, true );

            switch ( costType )
            {
                case Data.CurrencyType.DIAMOND:
                    goldIcon.gameObject.SetActive( false );
                    emberIcon.gameObject.SetActive( false );
                    diamondIcon.gameObject.SetActive( true );
                    break;
                case Data.CurrencyType.GOLD:
                    goldIcon.gameObject.SetActive( true );
                    emberIcon.gameObject.SetActive( false );
                    diamondIcon.gameObject.SetActive( false );
                    break;
                case Data.CurrencyType.EMBER:
                    goldIcon.gameObject.SetActive( false );
                    emberIcon.gameObject.SetActive( true );
                    diamondIcon.gameObject.SetActive( false );
                    break;
            }

            nameText.text = nameStr;
            priceText.text = goodPrice.ToString();
        }
    }
}
