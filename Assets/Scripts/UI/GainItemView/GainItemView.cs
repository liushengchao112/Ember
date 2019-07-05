using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Utils;

namespace UI
{
    public class GainItemView : ViewBase
    {
        private ScrollRect dragItemPanel;
        private GridLayoutGroup itemGroup;
        private Text titleText, confirmAloneText, confirmText, openText;
        private Button confirmAloneButton, confirmButton, openButton;

        private GainItemController controller;
        
        private List<GainItem> gain_Items = new List<GainItem>();

        #region Path
        private const string GAIN_ITEM_PATH = "Gain_Item";
        #endregion

        public void OpenGainItemView( int exp, List<Data.Currency> currencyDatas, List<Data.ItemInfo> itemDatas, List<Data.SoldierInfo> soldierDatas,bool isAloneBt=true )
        {
            if ( !openState )
                OnEnter();

            SetButton( isAloneBt );

            controller.SetItemDatas( exp, currencyDatas, itemDatas, soldierDatas );
        }

        public override void OnInit()
        {
            base.OnInit();

            controller = new GainItemController( this );
            _controller = controller;

            dragItemPanel = transform.Find( "DragItemPanel" ).GetComponent<ScrollRect>();
            itemGroup = transform.Find( "DragItemPanel/ItemGroup" ).GetComponent<GridLayoutGroup>();
            titleText = transform.Find( "TitleText" ).GetComponent<Text>();
            confirmAloneText = transform.Find( "ConfirmAloneText" ).GetComponent<Text>();
            confirmText = transform.Find( "ConfirmText" ).GetComponent<Text>();
            openText = transform.Find( "OpenText" ).GetComponent<Text>();

            confirmAloneButton = transform.Find( "ConfirmAloneButton" ).GetComponent<Button>();
            confirmButton = transform.Find( "ConfirmButton" ).GetComponent<Button>();
            openButton = transform.Find( "OpenButton" ).GetComponent<Button>();

            confirmAloneButton.AddListener( OnClickConfirmButton );
            confirmButton.AddListener( OnClickConfirmButton );
            openButton.AddListener( OnClickOpenButton, UIEventGroup.Middle, UIEventGroup.Middle );
        }

        private void OnClickConfirmButton()
        {
            OnExit( false );
        }

        private void OnClickOpenButton()
        {
            List<Data.ItemInfo> itemList = controller.itemDatas;
            if ( itemList.Count > 0 && itemList[0].itemType == (int)Data.PlayerBagItemType.BoxItem )
                controller.SendUseItemC2S( Data.BagType.BoxBag, itemList[0].count, itemList[0].metaId );
        }

        #region Init Item

        private int itemCount;
        public void RefreshItem()
        {
            itemCount = controller.GetItemsCount();
            Resource.GameResourceLoadManager.GetInstance().LoadResource( GAIN_ITEM_PATH, OnLoadGainItem, true );
        }

        private void OnLoadGainItem( string name, UnityEngine.Object obj, System.Object param )
        {
            CommonUtil.ClearItemList<GainItem>( gain_Items );

            DG.Tweening.DOTween.To( () => dragItemPanel.verticalNormalizedPosition, value => dragItemPanel.verticalNormalizedPosition = value, 1, 0.3f );

            for ( int i = 0; i < itemCount; i++ )
            {
                GainItem item;
                if ( gain_Items.Count < itemCount )
                {
                    item = CommonUtil.CreateItem<GainItem>( obj, itemGroup.transform );
                    gain_Items.Add( item );
                }
                item = gain_Items[i];
                item.gameObject.SetActive( true );

                item.count = controller.GetItemNum( i );
                item.nameStr = controller.GetItemName( i );
                item.icon = controller.GetItemIcon( i );

                item.RefreshItem();
            }
        }
        
        #endregion

        private void SetButton( bool isAloneButton )
        {
            confirmAloneText.gameObject.SetActive( isAloneButton );
            confirmAloneButton.gameObject.SetActive( isAloneButton );
            confirmText.gameObject.SetActive( !isAloneButton );
            confirmButton.gameObject.SetActive( !isAloneButton );
            openText.gameObject.SetActive( !isAloneButton );
            openButton.gameObject.SetActive( !isAloneButton );
        }
    }
}
