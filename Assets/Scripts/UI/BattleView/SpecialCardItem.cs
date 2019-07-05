using UnityEngine;
using System;
using UnityEngine.UI;

using Resource;
using Utils;
using Constants;

namespace UI
{
    public enum SpecialCardType
    {
        None = -1,
		//Locked building mode drag deployment code.Dwayne.
       /* Institute = 0,
        Tower = 1,
        Tramcar = 2,
        Demolisher = 3,*/
		Tramcar = 0,
		Demolisher = 1,
    }

    public class SpecialCardItem : MonoBehaviour
    {
        public SpecialCardType cardType;
        public BattleViewController controller;
		public bool canDeploy = true;
		private int numLimit = 0;

		private int cost, number = 0;        
		public Image maskImage;
        private Text costText;
        private Button unitItemButton;
        private Image itemImageMask;
        private Text countDownTimerText;

        private float buildUnitTimer;
        private bool isTiming;
        private float showNum;

        private void Awake()
        {            
            maskImage = transform.Find( "MaskUI" ).GetComponent<Image>();
            costText = transform.Find( "EmberCostText" ).GetComponent<Text>();
            unitItemButton = transform.Find( "Background" ).GetComponent<Button>();
            itemImageMask = transform.Find( "MaskImage/ItemImageMask" ).GetComponent<Image>();
            countDownTimerText = transform.Find( "TimerText" ).GetComponent<Text>();

            unitItemButton.onClick.AddListener( OnUnitItemClicked );
        }

        public void OnInit()
        {
            if ( cardType == SpecialCardType.Tramcar )
            {

            }
            else if ( cardType == SpecialCardType.Demolisher )
            {

            }
        }

        private void OnUnitItemClicked()
        {
            isTiming = true;
            return;
            if ( cardType == SpecialCardType.Tramcar )
            {
                MessageDispatcher.PostMessage( Constants.MessageType.DeploySquad , GameConstants.UNIT_TRAMCAR_METAID , -1 );
            }
            else if ( cardType == SpecialCardType.Demolisher )
            {
                MessageDispatcher.PostMessage( Constants.MessageType.DeploySquad , GameConstants.UNIT_DEMOLISHER_METAID , -1 );
            }
        }

        private void Update()
        {
            if ( isTiming )
            {
                BuildTiming();
            }
        }

        private void BuildTiming()
        {
            buildUnitTimer += Time.deltaTime;

            itemImageMask.fillAmount = buildUnitTimer / 2;

            showNum = 2 - buildUnitTimer;

            countDownTimerText.text = showNum.ToString( "0.0" );

            if ( buildUnitTimer >= 2 )
            {
                countDownTimerText.text = "";
                isTiming = false;
                buildUnitTimer = 0;
                itemImageMask.fillAmount = 0;

                BuildCompleteUIEffect();                
            }
        }

        private void BuildCompleteUIEffect()
        {
            //TODO:Designer has not confirm effect.
        }

        public void SetItemValue()
        {
			cost = controller.GetSpecialCost( cardType, out numLimit );
        }
			
        public void SetSpecialItem()
        {
            costText.text = cost.ToString();
        }

		public void AddNumber()
		{
			number++;

			if( number >= numLimit )
			{
				canDeploy = false;

				SetCanNotDeploy();
			}
		}

		public void RemoveNumber()
		{
			number--;

			if( number < numLimit && !canDeploy )
			{
				canDeploy = true;

				SetCanDeploy();
			}
		}

		public void SetCanNotDeploy()
		{
			maskImage.gameObject.SetActive( true );
		}

		public void SetCanDeploy()
        {
			maskImage.gameObject.SetActive( false );
        }

		public int GetCardCost()
		{
			return cost;
		}
    }
}
