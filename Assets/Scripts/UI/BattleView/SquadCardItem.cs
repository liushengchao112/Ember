using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Utils;

using Data;

namespace UI
{
    public class SquadCardItem : MonoBehaviour
    {
		//Drag deployment logic locked.Dwayne 2017.9
		/*public bool CanDeploy { get { return canDeploy && battleView.CanDeploy; } }*/
		private bool isTownDestroyed;
		private bool canDeploy;
        public BattleView battleView;
        public SquadData data;

		private Image itemImage;
		private Image itemImageMask;
		private Button unitItemButton;
		private Text squadNameText;
		private Text emberCost;
		private Text countDownTimerText;

        public bool active;
        public string squadName;
        public int icon;
        public int metaId;
		private int cost;
		public bool isLimited;
		public int buttonIndex;

		#region TimerValues

		private float buildUnitTimer;
		public bool isTiming;
		private float showNum;

		private Color lockedColor = Color.black;
		private Color canUseColor = new Color( 255, 255, 255, 255 );

		#endregion

		void Update()
		{
			if( isTiming && !isTownDestroyed )
			{
				BuildTiming();
			}
		}

        public void SetCanDeploy( bool canDeploy )
        {
			if( !isTiming )
			{
				this.canDeploy = canDeploy;

				if( !canDeploy )
				{
					ButtonLockedStatus();
				}
				else
				{
					if( !isLimited )
					{
						ButtonOpenStatus();
					}
				}
			}
        }

       /* public void SetActive( bool active )
        {
            this.active = active;
            gameObject.SetActive( active );
        }*/

        public void SetValues( SquadData data )
        {
            squadName = data.protoData.Name;
            this.metaId = data.protoData.ID;
            icon = data.protoData.Icon;
            cost = data.protoData.DeploymentCost;
            this.data = data;

			RefreshItemInfo();
        }

		public void OnInit( int index )
        {
			itemImage = transform.Find( "MaskImage/ItemImage" ).GetComponent<Image>();
			itemImageMask = transform.Find( "MaskImage/ItemImageMask" ).GetComponent<Image>();
			unitItemButton = transform.Find( "Background" ).GetComponent<Button>();
			squadNameText = transform.Find( "SquadNameText" ).GetComponent<Text>();
			emberCost = transform.Find( "EmberCostText" ).GetComponent<Text>();
			countDownTimerText = transform.Find( "TimerText" ).GetComponent<Text>();
            
            battleView = transform.root.GetComponentInChildren<BattleView>();
			unitItemButton.onClick.AddListener( OnUnitItemClicked );
			buttonIndex = index;
        }

		public void RefreshItemInfo()
        {
            squadNameText.text = squadName;
			emberCost.text = cost.ToString();
			Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
			{
				itemImage.SetSprite( atlasSprite );
			}, true );
        }

		private void OnUnitItemClicked()
		{
			if( this.active == true )
			{
				ButtonLockedStatus();

				isTiming = true;
				battleView.AddHoldeSquad();
			}
		}

		private void BuildTiming()
		{
			buildUnitTimer += Time.deltaTime;

			itemImageMask.fillAmount = buildUnitTimer / data.protoData.DeployTime;

			showNum = data.protoData.DeployTime - buildUnitTimer;

			countDownTimerText.text = showNum.ToString( "0.0" );

			if( buildUnitTimer >= data.protoData.DeployTime )
			{
				countDownTimerText.text = "";
				isTiming = false;
				buildUnitTimer = 0;
				itemImageMask.fillAmount = 0;

				BuildCompleteUIEffect();
				battleView.SendDeploySquad( metaId, data.index, buttonIndex );
			}
		}

		private void ButtonLockedStatus()
		{
			unitItemButton.enabled = false;
			this.active = false;
			unitItemButton.image.color = lockedColor;			
		}

		private void ButtonOpenStatus()
		{
			if( !isTownDestroyed )
			{
				unitItemButton.enabled = true;
				this.active = true;
				unitItemButton.image.color = canUseColor;				
			}
		}

		private void BuildCompleteUIEffect()
		{
			//TODO:Designer has not confirm effect.
		}

		//When player town destroyed, he can use survivor unit but can't buld new unit.
		public void SwitchTownDestroyedStatus()
		{
			isTownDestroyed = true;
			ButtonLockedStatus();
		}
    }
}
