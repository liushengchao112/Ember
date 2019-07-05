using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using Data;
using Utils;

namespace UI
{
    public class InstituteSkillItem : MonoBehaviour
    {
        private Image itemImage, emberImage1, emberImage2;
		private Text buttonText1, buttonText2, skillNameText, skillLevelText, skillValueText1, skillValueText2, nextLevelSkillValueText1, nextLevelSkillValueText2, costText1, costText2;
        private Button clickButton1, clickButton2;
		private InstituteSkillData holdInstituteSkill;
		public InstituteSkillData nextInstituteSkill;
		private List<InstituteSkillData> slotInstituteSkills = new List<InstituteSkillData>();
		private Color32 lockedColor = new Color32( 60, 60, 60, 255 );
		private Color32 canUseColor = new Color32( 255, 255, 255, 255 );
		private DataManager dataManager;
		private int skillLv = 0;
		private int unLockedLv = 0;
		private int skillMaxLv = 0;
		private string skillLevelStr = "LV: ";
		private string skillValueLevelStr = "Lv";
		private string notHoldSkillStr = "未研发";

		public string skillNameStr, skillValueStr1, skillValueStr2, skillNextLevelValueStr1, skillNextLevelValueStr2;
		public int singleUpgradeCost, repeatedlyUpgradeCost;
        public int icon;
        public bool haveSkill;
		public Action<int> clickUpButton;
		public Action<int, int> clickDownButton;

        private void Awake()
        {
            itemImage = transform.Find( "ItemImage" ).GetComponent<Image>();
            emberImage1 = transform.Find( "EmberImage1" ).GetComponent<Image>();
            emberImage2 = transform.Find( "EmberImage2" ).GetComponent<Image>();
            buttonText1 = transform.Find( "ButtonText1" ).GetComponent<Text>();
            buttonText2 = transform.Find( "ButtonText2" ).GetComponent<Text>();
            skillNameText = transform.Find( "SkillNameText" ).GetComponent<Text>();
			skillLevelText = transform.Find( "SkillLevelText" ).GetComponent<Text>();
            skillValueText1 = transform.Find( "HoldSkillValueText1" ).GetComponent<Text>();
			skillValueText2 = transform.Find( "HoldSkillValueText2" ).GetComponent<Text>();
            nextLevelSkillValueText1 = transform.Find( "NextSkillValueText1" ).GetComponent<Text>();
			nextLevelSkillValueText2 = transform.Find( "NextSkillValueText2" ).GetComponent<Text>();
			nextLevelSkillValueText1.color = Color.gray;
			nextLevelSkillValueText2.color = Color.gray;
            costText1 = transform.Find( "CostText1" ).GetComponent<Text>();
            costText2 = transform.Find( "CostText2" ).GetComponent<Text>();

            clickButton1 = transform.Find( "ClickButton1" ).GetComponent<Button>();
            clickButton2 = transform.Find( "ClickButton2" ).GetComponent<Button>();
        }

        private void Start()
        {
            clickButton1.AddListener( OnClickSingleSkillLevelUpButton );
            clickButton2.AddListener( OnClickRepeatedlyButton );
        }

        #region Button Event

        private void OnClickSingleSkillLevelUpButton()
        {
			clickUpButton( this.nextInstituteSkill.skillID );
        }

        private void OnClickRepeatedlyButton()
        {
			clickDownButton( this.nextInstituteSkill.skillID, GetCanLevelUpLevel() );
        }

        #endregion

		public void InitSlotSkills( InstituteSkillData headSkill )
		{
			if( dataManager == null )
			{
				dataManager = DataManager.GetInstance();
			}

			this.nextInstituteSkill = headSkill;

			DebugUtils.Log( DebugUtils.Type.InstitutesSkill, "InitSlotSkills thisInstituteSkill ID is " + this.nextInstituteSkill.skillID );

			List<InstituteSkillProto.InstituteSkill> protoSkills = dataManager.instituteSkillProtoData;

			slotInstituteSkills.Add( headSkill );

			int nextSkillID = headSkill.nextSkillID;

			while( nextSkillID != 0 )
			{
				for( int i = 0; i < protoSkills.Count; i++ )
				{
					if( protoSkills[i].ID == nextSkillID )
					{
						InstituteSkillData tempSkill = new InstituteSkillData();
						tempSkill.SetSkillData( protoSkills[i].ID, protoSkills[i].Level, protoSkills[i].RequiredLevel, protoSkills[i].IconID, 
							protoSkills[i].Txt_ID, protoSkills[i].Description_Id, protoSkills[i].Cost, protoSkills[i].BuffId, protoSkills[i].NextSkill );
						slotInstituteSkills.Add( tempSkill );
						nextSkillID = protoSkills[i].NextSkill;
						break;
					}
				}
			}

			skillMaxLv = slotInstituteSkills.Count;
		}

		public void RefreshInsituteSkillItem( bool mutipleUpgrade )
        {
			if( nextInstituteSkill != null )
			{
				icon = this.nextInstituteSkill.skillIconID;

				DebugUtils.Log( DebugUtils.Type.InstitutesSkill, "RefreshInstituteSkillItem now instituteSkill ID is :" + this.nextInstituteSkill.skillID );

				if( holdInstituteSkill != null )
				{
					this.skillNameStr = this.holdInstituteSkill.skillNameID.Localize();
				}
				else
				{
					this.skillNameStr = "";
				}

				skillLevelText.text = skillLevelStr + skillLv;

				if( holdInstituteSkill != null )
				{
					if( mutipleUpgrade )
					{
						string[] holdSkillStr = this.holdInstituteSkill.BuffsID.Split( '|' );
						List<string> holdSkillDescriptions = GetBuffDescriptions( holdSkillStr );

						skillValueStr1 = string.Format( holdSkillDescriptions[0] );
						skillValueStr2 = holdSkillDescriptions[1];
					}
					else
					{
						skillValueStr1 = skillNextLevelValueStr1;
						skillValueStr2 = skillNextLevelValueStr2;
					}

					skillValueText1.color = Color.green; 
					skillValueText2.color = Color.green;
				}
				else
				{
					skillValueStr1 = notHoldSkillStr;
					skillValueStr2 = notHoldSkillStr;
					skillValueText1.color = Color.gray;
					skillValueText2.color = Color.gray;
				}

				string[] nextSkillStr = this.nextInstituteSkill.BuffsID.Split( '|' );
				List<string> nextSkillDescriptions = GetBuffDescriptions( nextSkillStr );

				skillNextLevelValueStr1 = nextSkillDescriptions[0];
				skillNextLevelValueStr2 = nextSkillDescriptions[1];
			
				singleUpgradeCost = this.nextInstituteSkill.skillCost;
				repeatedlyUpgradeCost = GetOnClickedMaxLevelCost();

				if( icon != 0 )
				{
                    Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
                    {
                        itemImage.SetSprite( atlasSprite );
                    }, true );
                }

				skillNameText.text = skillNameStr;
				skillValueText1.text = string.Format( "{0} {1} : {2}", skillValueLevelStr, skillLv, skillValueStr1 );
				skillValueText2.text = string.Format( "          {0}", skillValueStr2);
				costText1.text = singleUpgradeCost.ToString();

				if( skillLv == skillMaxLv )
				{
					nextLevelSkillValueText1.gameObject.SetActive( false );
					nextLevelSkillValueText2.gameObject.SetActive( false );
				}
				else
				{
					nextLevelSkillValueText1.text = string.Format( "{0} {1} : {2}", skillValueLevelStr, skillLv + 1, skillNextLevelValueStr1 );
					nextLevelSkillValueText2.text = string.Format( "          {0}", skillNextLevelValueStr2 );
				}

				if( repeatedlyUpgradeCost == 0 )
				{
					costText2.gameObject.SetActive( false );
					clickButton2.gameObject.SetActive( false );
					buttonText2.gameObject.SetActive( false );
					emberImage2.gameObject.SetActive( false );
				}
				else
				{
					costText2.gameObject.SetActive( true );
					clickButton2.gameObject.SetActive( true );
					costText2.text = repeatedlyUpgradeCost.ToString();
				}
			}
        }

		public void CanUpgradeInstituteSkill( int instituteLv, int money )
		{
			if( instituteLv >= unLockedLv && instituteLv > skillLv && money >= singleUpgradeCost )
			{
				if( clickButton1.interactable == false )
				{
					clickButton1.interactable = true;
				}
			}
			else if( instituteLv >= unLockedLv && money < singleUpgradeCost )
			{
				if( clickButton1.interactable == true )
				{
					clickButton1.interactable = false;
					clickButton2.interactable = false;
				}
			}

			if( instituteLv >= unLockedLv && instituteLv > skillLv && money >= repeatedlyUpgradeCost )
			{
				if( clickButton2.interactable == false )
				{
					clickButton2.interactable = true;
				}
			}
		}

		private List<string> GetBuffDescriptions( string[] str )
		{
			float buffValue;
			string description;
			int descriptionID;
			List<string> descriptions = new List<string>();
			List<AttributeEffectProto.AttributeEffect> attributeEffect = dataManager.attributeEffectProtoData;

			for( int i = 0; i < str.Length; i++ )
			{ 
				descriptionID = attributeEffect.Find( p => p.ID == int.Parse( str[i] ) ).DescriptionId;
				description = descriptionID.Localize();
				buffValue = dataManager.attributeEffectProtoData.Find( p => p.ID == int.Parse( str[i] ) ).MainValue;

				if( attributeEffect.Find( p => p.ID == int.Parse( str[i] )).CalculateType == 2 )
				{
					description = string.Format( description, buffValue * 100 + "%" );
				}
				else
				{
					description = string.Format( description, buffValue );
				}

				descriptions.Add( description );
			}

			return descriptions;
		}

		private int GetOnClickedMaxLevelCost()
		{
			int totalCost = 0;
			int instituteLevel = dataManager.GetInstituteLV();
			
			for( int i = skillLv; i < instituteLevel; i++ )
			{
				totalCost += slotInstituteSkills[i].skillCost;
			}

			return totalCost;
		}

		private int GetCanLevelUpLevel()
		{
			int instituteLevel = dataManager.GetInstituteLV();

			return ( instituteLevel - skillLv ) ;
		}

		public void InstituteLevelUpSucceeded( int instituteLv )
		{
			if( unLockedLv <= instituteLv )
			{
				clickButton1.interactable = true;

				if( !clickButton2.gameObject.activeInHierarchy )
				{
					clickButton2.gameObject.SetActive( true );
					buttonText2.gameObject.SetActive( true );
					emberImage2.gameObject.SetActive( true );
					costText2.gameObject.SetActive( true );
				}

				repeatedlyUpgradeCost = GetOnClickedMaxLevelCost();
				costText2.text = repeatedlyUpgradeCost.ToString();
			}
		}
			
		public void SetInsituteSkillUI( int lockedLv )
        {
			unLockedLv = ( lockedLv + 1 );
		
			if( lockedLv != 0 )
			{
				clickButton1.interactable = false;
			}

			nextLevelSkillValueText1.gameObject.SetActive( true );
			nextLevelSkillValueText2.gameObject.SetActive( true );

			itemImage.color = lockedColor;

			clickButton2.gameObject.SetActive( false );
			buttonText2.gameObject.SetActive( false );
			emberImage2.gameObject.SetActive( false );
			costText2.gameObject.SetActive( false );
        }

		private void RefreshInstituteStatus()
		{
			if( haveSkill )
			{
				if( itemImage.color == lockedColor )
				{
					itemImage.color = canUseColor;
				}

				if( skillLv < dataManager.GetInstituteLV() )
				{	
					if( clickButton1.interactable == false )
					{
						clickButton1.interactable = true;
					}

					if( !clickButton2.gameObject.activeInHierarchy )
					{
						clickButton2.gameObject.SetActive( true );
						buttonText2.gameObject.SetActive( true );
						emberImage2.gameObject.SetActive( true );
						costText2.gameObject.SetActive( true );
					}
				}
				else
				{
					clickButton1.interactable = false;

					clickButton2.gameObject.SetActive( false );
					buttonText2.gameObject.SetActive( false );
					emberImage2.gameObject.SetActive( false );
					costText2.gameObject.SetActive( false );
				}
			}
			else
			{
				clickButton1.gameObject.SetActive( true );
				buttonText1.gameObject.SetActive( true );
				emberImage1.gameObject.SetActive( true );
				costText1.gameObject.SetActive( true );
				clickButton1.interactable = false;
			}
		}

		public void TheSkillUpgrade( int applySkillID, int upgradeSkillID )
		{
			DebugUtils.Log( DebugUtils.Type.InstitutesSkill, string.Format( "The skill :{0} is upgrade.", nextInstituteSkill.skillID ) );

			FillNextSkillInSlot( applySkillID, upgradeSkillID );
			RefreshInsituteSkillItem( false );

			if( upgradeSkillID != 0 )
			{
				RefreshInstituteStatus();
			}
			else
			{
				TheSkillLevelMaxShowEffect();
			}
		}

		public void TheSkillRepeatedlyUpgrade( int upgradedSkillID )
		{
			FillTargetSkillInSlot( upgradedSkillID );
			RefreshInsituteSkillItem( true );

			if( upgradedSkillID != 0 )
			{
				RefreshInstituteStatus();
			}
			else
			{
				TheSkillLevelMaxShowEffect();
			}
		}

		private void FillNextSkillInSlot( int applySkillID, int upgradeSkillID )
		{
			if( haveSkill == false )
			{
				haveSkill = true;
			}

			skillLv++;

			if( this.holdInstituteSkill != null )
			{
				MessageDispatcher.PostMessage( Constants.MessageType.RemoveBuildingBuffMessage, this.holdInstituteSkill.BuffsID );
			}

			this.holdInstituteSkill = slotInstituteSkills.Find( p => p.skillID == applySkillID );

			//This message just tell ui show instituteSkill value 
			MessageDispatcher.PostMessage( Constants.MessageType.AddBuildingBuffMessage, this.holdInstituteSkill.BuffsID );

			if( upgradeSkillID != 0 )
			{
				for( int i = 0; i < slotInstituteSkills.Count; i++ )
				{
					if( slotInstituteSkills[i].nextSkillID == upgradeSkillID )
					{
						this.nextInstituteSkill = slotInstituteSkills.Find( p => p.skillID == slotInstituteSkills[i].nextSkillID );
						DebugUtils.Log( DebugUtils.Type.InstitutesSkill, "There changed item instituteSkill, the new skill ID is :" + nextInstituteSkill.skillID );
						break;
					}
				}
			}
		}

		public void FillTargetSkillInSlot( int upgradedSkillID )
		{
			if( haveSkill == false )
			{
				haveSkill = true;
			}

			if( this.holdInstituteSkill != null )
			{
				MessageDispatcher.PostMessage( Constants.MessageType.RemoveBuildingBuffMessage, this.holdInstituteSkill.BuffsID );
			}

			if( upgradedSkillID == 0 )
			{
				skillLv = slotInstituteSkills.Count;
				this.holdInstituteSkill = slotInstituteSkills[skillLv - 1];
				this.nextInstituteSkill = this.holdInstituteSkill;
			}
			else
			{
				for( int i = 0; i < slotInstituteSkills.Count; i++ )
				{
					if( slotInstituteSkills[i].nextSkillID == upgradedSkillID )
					{
						this.holdInstituteSkill = slotInstituteSkills[i];

						if( upgradedSkillID != 0 )
						{
							this.nextInstituteSkill = slotInstituteSkills.Find( p => p.skillID == slotInstituteSkills[i].nextSkillID );
						}
						else
						{
							this.nextInstituteSkill = this.holdInstituteSkill;
						}

						skillLv = slotInstituteSkills.IndexOf( nextInstituteSkill );

						break;
					}
				}
			}

			//This message just tell ui show instituteSkill value 
			MessageDispatcher.PostMessage( Constants.MessageType.AddBuildingBuffMessage, this.holdInstituteSkill.BuffsID );
		}

		private void TheSkillLevelMaxShowEffect()
		{

			if( itemImage.color == lockedColor )
			{
				itemImage.color = canUseColor;
			}
				
			costText1.gameObject.SetActive( false );
			clickButton1.gameObject.SetActive( false );
			buttonText1.gameObject.SetActive( false );
			emberImage1.gameObject.SetActive( false );

			costText2.gameObject.SetActive( false );
			clickButton2.gameObject.SetActive( false );
			buttonText2.gameObject.SetActive( false );
			emberImage2.gameObject.SetActive( false );
		}

		//This function will reset instituteSkill default setting.
		public void ResetSkillData( int unlockLv )
		{
			skillLv = 0;
			haveSkill = false;
			holdInstituteSkill = null;
			nextInstituteSkill = slotInstituteSkills[0];
		
			RefreshInsituteSkillItem( false );
			RefreshInstituteStatus();
			SetInsituteSkillUI( unlockLv );
		}
    }
}
