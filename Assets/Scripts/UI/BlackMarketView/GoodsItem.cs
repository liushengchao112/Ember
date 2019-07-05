using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using Resource;
using System;

namespace UI
{
    public class GoodsItem : MonoBehaviour
    {
        private Image itemImage, goldImage, rmbImage, emberImage, markImage;
        private Text quantityText, costText;
        public Toggle onClickToggle;

        public int index;
        public int imageIcon;
        public int quantity;
        public int cost;
        public bool isSellOut;
        public Data.CurrencyType buyType;
        public Action<int, Image, bool> clickGoodToggle;

        private void Awake()
        {
            itemImage = transform.Find( "ItemImage" ).GetComponent<Image>();
            goldImage = transform.Find( "GoldImage" ).GetComponent<Image>();
            rmbImage = transform.Find( "RMBImage" ).GetComponent<Image>();
            markImage = transform.Find( "MarkImage" ).GetComponent<Image>();
            markImage.gameObject.SetActive( false );
            emberImage = transform.Find( "EmberImage" ).GetComponent<Image>();
            quantityText = transform.Find( "QuantityText" ).GetComponent<Text>();
            costText = transform.Find( "CostText" ).GetComponent<Text>();
            onClickToggle = transform.Find( "OnClickToggle" ).GetComponent<Toggle>();

            onClickToggle.AddListener( OnClickToggleEvent );
        }

        private void OnClickToggleEvent( bool isOn )
        {
            onClickToggle.interactable = !isOn;
            if ( !isOn )
            { markImage.gameObject.SetActive( false ); return; }
            clickGoodToggle( index, markImage, isSellOut );
        }

        public void RefreshGoodsItem()
        {
            quantityText.text = "X" + quantity;
            costText.text = cost.ToString();

            GameResourceLoadManager.GetInstance().LoadAtlasSprite( imageIcon, delegate( string name, AtlasSprite atlasSprite, System.Object param )
            {
                itemImage.SetSprite( atlasSprite );
            }, true );

            SetSoldOut( isSellOut );
        }

        public void SetSoldOut( bool isSoldOut )
        {
            itemImage.gameObject.SetActive( !isSoldOut );

            quantityText.gameObject.SetActive( !isSoldOut );
            costText.gameObject.SetActive( !isSoldOut );
            onClickToggle.gameObject.SetActive( !isSoldOut );

            if ( isSoldOut )
            {
                goldImage.gameObject.SetActive( !isSoldOut );
                rmbImage.gameObject.SetActive( !isSoldOut );
                emberImage.gameObject.SetActive( !isSoldOut );
            }
            else
            {
                SetCurrencyIcon();
            }
        }

        private void SetCurrencyIcon()
        {
            switch ( buyType )
            {
                case Data.CurrencyType.EMBER:
                    goldImage.gameObject.SetActive( false );
                    emberImage.gameObject.SetActive( true );
                    rmbImage.gameObject.SetActive( false );
                    break;
                case Data.CurrencyType.GOLD:
                    goldImage.gameObject.SetActive( true );
                    emberImage.gameObject.SetActive( false );
                    rmbImage.gameObject.SetActive( false );
                    break;
                case Data.CurrencyType.DIAMOND:
                    goldImage.gameObject.SetActive( false );
                    emberImage.gameObject.SetActive( false );
                    rmbImage.gameObject.SetActive( true );
                    break;
            }
        }
    }
}
