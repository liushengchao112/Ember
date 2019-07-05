using UnityEngine;
using System;
using UnityEngine.UI;
using Utils;
using Data;

namespace UI
{
    public class TowerItem : MonoBehaviour
    {
        private Image itemImage;
        private Text nameText, propText1, propText2, buttonText, costText;
        private Button clickButton;

        public string nameString, propString1, propString2;
		public int icon, buildingID, cost, index;
        public Action clickButtonEvent;
		public BattleView view;

        private void Awake()
        {
            itemImage = transform.Find( "ItemImage" ).GetComponent<Image>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            buttonText = transform.Find( "ButtonText" ).GetComponent<Text>();
            propText1 = transform.Find( "PropText1" ).GetComponent<Text>();
            propText2 = transform.Find( "PropText2" ).GetComponent<Text>();
            costText = transform.Find( "CostText" ).GetComponent<Text>();

            clickButton = transform.Find( "ClickButton" ).GetComponent<Button>();
        }

        private void Start()
        {
            clickButton.AddListener( OnClickButton );
        }

		#region Button Event

        private void OnClickButton()
        {
			if( view.playerEmber >= cost && view.GetBuildedTowerNum() < Constants.GameConstants.TOWER_DEPLOYMENT_LIMIT )
			{
				MessageDispatcher.PostMessage( Constants.MessageType.GenerateBuilding, ( long )buildingID, BuildingType.Tower );
				MessageDispatcher.PostMessage( Constants.MessageType.CloseTowerPopUp );
			}
        }

        #endregion

        public void RefreshItem()
        {
            if ( icon != 0 )
            {
                Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
                {
                    itemImage.SetSprite( atlasSprite );
                }, true );
            }

            nameText.text = nameString;
            propText1.text = propString1;
            propText2.text = propString2;
            costText.text = cost.ToString();
        }

    }
}
