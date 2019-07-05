using UnityEngine;
using System.Collections.Generic;
using System;

using Data;
using Utils;

using InstituteSkillProto = Data.InstituteSkillProto.InstituteSkill;
using InstituteProto = Data.InstituteProto.Institute;

namespace Logic
{
	public class Institute : LogicUnit
	{
		private AttributeEffectGenerator GenerateAttributeEffect;
		private DataManager datamanager;

		public BuildingState state;
		private Town town;

		private int buildCost;
		private int destroyReward;
		public int buildTime;
		private int buildingTimer;
		private bool isUpgrading = false;
	
		private int physicalResistance;
		private int magicResistance;
		private int nowLv;
		private int maxLv;

		private int healthRecoverInterval;
		private int healthRecoverTimer;
		private int healthRecover;

		public List<InstituteSkillData> instituteHeadSkills;
		public List<InstituteSkillData> usedInstituteSkills;
		public List<Soldier> effectedSoldier;
		private List<AttributeEffect> playerSkillBuffs = new List<AttributeEffect>();
		private List<AttributeEffect> playerSkillDeBuffs = new List<AttributeEffect>();
		private bool isSkillMaxLv;

		public void Initialize( long id, Town town, FixVector3 pos )
		{
			this.id = id;
			this.town = town;
			this.type = LogicUnitType.institute;
			mark = town.mark;
			transform.position = pos.vector3;
			position = new FixVector3( transform.position );
            state = BuildingState.IDLE;

            //There is player build institute init value
            if ( datamanager == null )
			{
				datamanager = DataManager.GetInstance();
			}

			InstituteProto proto = datamanager.instituteProtoData[0];

            metaId = proto.ID;
			buildCost = proto.DeploymentCost;
			buildTime = ConvertUtils.ToLogicInt( proto.BuildingTime );
			maxHp = proto.Health;
			maxLv = proto.MaxLevel;

			if( mark == ForceMark.TopRedForce || mark == ForceMark.BottomRedForce )
			{
				modelId = proto.RedModelID;
			}
			else
			{
				modelId = proto.BlueModelID;
			}
				
			hp = maxHp;
			//healthRecover = 1;//If want healthRecover value need designer add table string.Dwayne
			physicalResistance = proto.PhysicalResistance;
			magicResistance = proto.MagicResistance;
			nowLv = 1;
			destroyReward = proto.DestoryReward;
			healthRecoverInterval = Constants.GameConstants.HP_RECOVERY_INTERVAL_MILLISECOND;
			datamanager.SetInstituteLV( nowLv );
		}

		//Init institute seted all skill and skills all Level data.
		public void InitInstituteSkillsData()
		{
			DebugUtils.Log( DebugUtils.Type.InstitutesSkill, "InitInstituteSkills" );

			instituteHeadSkills = new List<InstituteSkillData>();
			List<int>playerInstituteSkills = datamanager.GetPlayerSetedPackageInstituteSkills( datamanager.GetBattleConfigInsituteSkillIndex( datamanager.GetBattleType() ));
			List<InstituteSkillProto> protoInstituteSkills = datamanager.instituteSkillProtoData;

			for( int i = 0; i < playerInstituteSkills.Count; i++ )
			{
				for ( int j = 0; j < protoInstituteSkills.Count; j++ )
				{
					if( protoInstituteSkills[j].ID == playerInstituteSkills[i] )
					{
						InstituteSkillData tempSkill = new InstituteSkillData();
						tempSkill.SetSkillData( protoInstituteSkills[j].ID, protoInstituteSkills[j].Level, protoInstituteSkills[j].RequiredLevel, protoInstituteSkills[j].IconID, 
							protoInstituteSkills[j].Txt_ID, protoInstituteSkills[j].Description_Id, protoInstituteSkills[j].Cost, protoInstituteSkills[j].BuffId, protoInstituteSkills[j].NextSkill );
						instituteHeadSkills.Add( tempSkill );
						break;
					}
				}
			}
		}

		#region InstituteLevelUp functon

		//use for institute building Level up.
		public int InstituteLevelUpStart()
		{
			int cost = 0;

			cost = datamanager.instituteProtoData.Find( p => p.Level == nowLv ).LevelUpCost;
			isUpgrading = true ;

			RenderMessage message = new RenderMessage();
			message.ownerId = id;
			message.type = RenderMessage.Type.InstituteLevelUpStart;
			PostRenderMessage( message );
		
			return cost;
		}

		public void InstituteLevelUpComplete()
		{
			nowLv++;
			datamanager.SetInstituteLV( nowLv );

			InstituteProto proto = datamanager.instituteProtoData[ nowLv - 1 ];
			maxHp = proto.Health;
			hp = maxHp;
			physicalResistance = proto.PhysicalResistance;
			magicResistance = proto.MagicResistance;

			RenderMessage message = new RenderMessage();
			message.ownerId = id;
			message.type = RenderMessage.Type.InstituteLevelUpComplete;
			message.arguments.Add( "mark", mark );
			message.arguments.Add( "HP", maxHp );
			PostRenderMessage( message );
		}
			
		#endregion

		#region instituteSkill functions( upgrade,reset,buffs attach,buffs detach... )

		//InstituteSkill upgrade use next skill id and upgrade unmber of times.
		public int InstituteSkillLevelUp( int skillID, int upgradeNum )
		{
			int cost = 0;
			int index = 0;

			InstituteSkillData temp = instituteHeadSkills.Find( p => p.skillID == skillID );
			List<int> buffsID;

			if( effectedSoldier == null )
			{
				effectedSoldier = town.GetAliveSoldiers();
				usedInstituteSkills = new List<InstituteSkillData>();
			}

			DebugUtils.Log( DebugUtils.Type.InstitutesSkill, "The instituteSkill will upgrade, skill ID is " + skillID );

			if( temp != null )
			{
				if( playerSkillBuffs.Count > 0 )
				{
					for( int i = 0; i < usedInstituteSkills.Count; i++ )
					{
						if( usedInstituteSkills[i].nextSkillID == temp.skillID )
						{
							buffsID = GetBuffsID( usedInstituteSkills[i].BuffsID.Split( '|' ) );
							RemoveBuff( buffsID[0] );
							RemoveDebuff( buffsID[1] );

							usedInstituteSkills.Remove( usedInstituteSkills[i] );
						}
					}
				}
			
				index = instituteHeadSkills.IndexOf( temp );

				if( upgradeNum == 1 )
				{
					usedInstituteSkills.Add( temp );

					cost = temp.skillCost;
					buffsID = GetBuffsID( instituteHeadSkills[index].BuffsID.Split( '|' ));

					AddBuff( buffsID[0] );
					AddDebuff( buffsID[1] );

					instituteHeadSkills[index] = GetFillInstituteSkillData( temp );

					DebugUtils.Log( DebugUtils.Type.InstitutesSkill, string.Format( "Add buff ID is：{0}, Add debuff ID is {1}.", buffsID[0], buffsID[1] ));
				}
				else if( upgradeNum > 1 )
				{
					InstituteSkillRepeatedlyUpgrade( ref cost, ref temp, upgradeNum );
					buffsID = GetBuffsID( temp.BuffsID.Split( '|' ));

					AddBuff( buffsID[0] );
					AddDebuff( buffsID[1] );

					instituteHeadSkills[index] = temp;

					usedInstituteSkills.Add( temp );

					DebugUtils.Log( DebugUtils.Type.InstitutesSkill, string.Format( "Add buff ID is：{0}, Add debuff ID is {1}.", buffsID[0], buffsID[1] ));
				}
				else 
				{
					DebugUtils.LogError( DebugUtils.Type.InstitutesSkill, string.Format( "Can't upgrade the skill,upgrade number of times is : {0}", upgradeNum ));
				}
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.InstitutesSkill, "Can't find the skill." + skillID );
			}

			RenderMessage message = new RenderMessage();
			message.ownerId = id;
			message.type = RenderMessage.Type.InstituteSkillLevelUp;
			message.arguments.Add( "mark", mark );
			message.arguments.Add( "applySkillID", skillID );

			if( upgradeNum == 1 )
			{
				if( temp.nextSkillID != 0 )
				{
					message.arguments.Add( "upgradeSkillID", instituteHeadSkills[index].skillID );
				}
				else
				{
					message.arguments.Add( "upgradeSkillID", 0 );
				}
			}
			else if( upgradeNum > 1 )
			{
				if( upgradeNum == maxLv || isSkillMaxLv )
				{
					message.arguments.Add( "upgradeSkillID", 0 );
				}
				else
				{
					message.arguments.Add( "upgradeSkillID", instituteHeadSkills[index].skillID );
				}
			}
				
			PostRenderMessage( message );
			isSkillMaxLv = false;

			DebugUtils.Log( DebugUtils.Type.InstitutesSkill, "Institute skill return cost is " + cost + " applySkillID is " + skillID + " upgradeSkillID is " + instituteHeadSkills[index].skillID );

			return cost;
		}
			
		private void InstituteSkillRepeatedlyUpgrade( ref int cost, ref InstituteSkillData temp, int upgradeNum )
		{
			for( int i = 0; i < upgradeNum; i++ )
			{
				cost += temp.skillCost;

				temp = GetFillInstituteSkillData( temp );
			}

			DebugUtils.Log( DebugUtils.Type.InstitutesSkill, "Institute skill RepeatedlyUpgrade " + cost + " upgradeSkillID is " + temp.skillID );
		}
			
		private InstituteSkillData GetFillInstituteSkillData( InstituteSkillData inputSkillData )
		{
			if( inputSkillData.nextSkillID != 0 )
			{
				Data.InstituteSkillProto.InstituteSkill skill = datamanager.instituteSkillProtoData.Find( p => p.ID == inputSkillData.nextSkillID );

				InstituteSkillData skillData = new InstituteSkillData();

				skillData.skillID = skill.ID;
				skillData.skillLevel = skill.Level;
				skillData.skillNameID = skill.Txt_ID;
				skillData.skillRequiredLevel = skill.RequiredLevel;
				skillData.skillIconID = skill.IconID;
				skillData.skillDescriptionID = skill.Description_Id;
				skillData.skillCost = skill.Cost;
				skillData.nextSkillID = skill.NextSkill;
				skillData.BuffsID = skill.BuffId;

				return skillData;
			}
			else
			{
				isSkillMaxLv = true;
				return inputSkillData;
			}
		}

		public List<int>GetBuffsID( string[] str )
		{
			List<int> buffIDs = new List<int>();

			for( int i = 0; i < str.Length; i++ )
			{
				buffIDs.Add( int.Parse( str[i] ));
			}

			return buffIDs;
		}

		public void AddBuff( int buffId )
		{
			if( effectedSoldier != null && effectedSoldier.Count > 0 )
			{
				for( int i = 0; i < effectedSoldier.Count; i++ )
				{
					AttributeEffect buff = GenerateAttributeEffect( buffId );
					buff.Attach( this, effectedSoldier[i] );

					DebugUtils.Log( DebugUtils.Type.InstitutesSkill, string.Format( "Add buff,metaID {0} buff attach the Solider ID {1} unit", buff.metaId, effectedSoldier[i].id ));
					playerSkillBuffs.Add( buff );
				}
			}
		}

		public void AddDebuff( int debuffId )
		{
			if( effectedSoldier != null && effectedSoldier.Count > 0 )
			{
				for( int i = 0; i < effectedSoldier.Count; i++ )
				{
                    AttributeEffect debuff = GenerateAttributeEffect( debuffId );
					debuff.Attach( this, effectedSoldier[i] );
					DebugUtils.Log( DebugUtils.Type.InstitutesSkill, string.Format( "Add debuff,metaID {0} debuff attach the Solider ID {1} unit", debuff.metaId, effectedSoldier[i].id ));
					playerSkillDeBuffs.Add( debuff );
				}
			}
		}

		public void RemoveBuff( int buffId )
		{
			DebugUtils.Log( DebugUtils.Type.InstitutesSkill, "Remove buffID is" + buffId );

			List<AttributeEffect> buffs = playerSkillBuffs.FindAll( p => p.metaId == buffId );

			if( buffs != null )
			{
				for( int i = 0; i < buffs.Count; i++ )
				{
					buffs[i].Detach();
				}

				playerSkillBuffs.RemoveAll( p => p.metaId == buffId );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.InstitutesSkill, "Can't find remove Buff." );
			}
		}

		public void RemoveDebuff( int debuffId )
		{
			List<AttributeEffect> debuffs = playerSkillDeBuffs.FindAll( p => p.metaId == debuffId );

			if( debuffs != null )
			{
				for( int i = 0; i < debuffs.Count; i++ )
				{
					debuffs[i].Detach();
				}

				playerSkillDeBuffs.RemoveAll( p => p.metaId == debuffId );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.InstitutesSkill, "Can't find remove Debuff." );
			}
		}

		//When town create new solider use this.
		public void AddSoliderInEffectedSoldier( Soldier solider )
		{
			for( int i = 0; i < usedInstituteSkills.Count; i++ )
			{
				List<int>buffsID;

				buffsID = GetBuffsID( usedInstituteSkills[i].BuffsID.Split( '|' ));

                AttributeEffect buff = GenerateAttributeEffect( buffsID[0] );
				buff.Attach( this, solider );
				buff.RegisterNoticeGiverDestroyMethod( SoldierDieRemoveBuff );
				playerSkillBuffs.Add( buff );

                AttributeEffect debuff = GenerateAttributeEffect( buffsID[1] );
				debuff.Attach( this, solider );
				debuff.RegisterNoticeGiverDestroyMethod( SoldierDieRemoveDebuff );
				playerSkillDeBuffs.Add( debuff );
			}
				
			effectedSoldier.Add( solider );
		}

		private void SoldierDieRemoveBuff( AttributeEffect buff )
		{
			playerSkillBuffs.Remove( buff );
		}

		private void SoldierDieRemoveDebuff( AttributeEffect debuff )
		{
			playerSkillDeBuffs.Remove( debuff );
		}

		//When soldier die town must use this remove solider
		public void RemoveSoliderInEffectedSoldier( Soldier solider )
		{
			effectedSoldier.Remove( solider );
		}

		private void CleanAllSkillData()
		{
			effectedSoldier = null;

			for( int i = 0; i < playerSkillBuffs.Count; i++ )
			{
				playerSkillBuffs[i].Detach();
			}

			for( int i = 0; i < playerSkillDeBuffs.Count; i++ )
			{
				playerSkillDeBuffs[i].Detach();
			}

			instituteHeadSkills = null;
			usedInstituteSkills = null;
			effectedSoldier = null;
			playerSkillBuffs.Clear();
			playerSkillDeBuffs.Clear();
		}

		#endregion

		#region Default functions( logicUpdate, Hurt, Alive... )

		public override void LogicUpdate( int deltaTime )
		{
			//HealthAutoRecovery( deltaTime );Designer don't want deploy type building have auto recovery.Locked this.

			if( isUpgrading )
			{
				buildingTimer += deltaTime;

				if( buildingTimer >= buildTime )
				{
					isUpgrading = false;
					buildingTimer = 0;
					InstituteLevelUpComplete();
				}
			}
		}

        public override void Hurt( int hurtValue, AttackPropertyType hurtType, bool isCrit, LogicUnit injurer )
		{
			if( Alive() )
			{
                int value = GetInstituteActualHurtValue( hurtType, hurtValue );
				hp -= value;
				RenderMessage message = new RenderMessage();
				message.type = RenderMessage.Type.InstituteHurt;
				message.ownerId = this.id;
				message.position = position.vector3;
				message.arguments.Add( "value", value );
				PostRenderMessage( message );

				if ( hp <= 0 )
				{
					injurer.OnKillEnemy( destroyReward, injurer, this );
					Destroy();
				}
			}
		}

		public override void AddCoin( int coin )
		{
			town.AddCoin( coin );
		}

		public override bool Alive()
		{
			return state != BuildingState.DESTROY;
		}

		public void Destroy()
		{
			DebugUtils.Assert( state != BuildingState.DESTROY );

			CleanAllSkillData();

			if( isUpgrading )
			{
				isUpgrading = false;
				buildingTimer = 0;
			}

			state = BuildingState.DESTROY;
			RenderMessage renderMessage = new RenderMessage();
			renderMessage.type = RenderMessage.Type.InstituteDestroy;
			renderMessage.ownerId = id;
			renderMessage.arguments.Add( "mark", mark );

			PostRenderMessage( renderMessage );

			PostDestroy( this );
			datamanager.SetInstituteLV( 0 );
		}

		private int GetInstituteActualHurtValue( AttackPropertyType type, int hurtValue )
		{
            int value = 0;

			if ( type == AttackPropertyType.PhysicalAttack )
			{
				value = Formula.ActuallyPhysicalDamage( hurtValue, physicalResistance );
			}
			//Building can't be magicAttack( design ),When we need open here
			//else if ( type == HurtType.MagicAttack )
			//{
			//	value = (int)( hurtValue / ( magicResistance / 100 + 1 ) );
			//}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Building, string.Format( "Can't handle this hurtType {0} now!", type ) );
			}

			return value;
		}

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            DebugUtils.Assert( false, "OnKillEnemy() in Institute is not implemented!" );
        }

        public override void Reset()
		{
			id = -1;
			nowLv = 1;
			maxHp = 0;
			hp = maxHp;

			PostRenderMessage = null;
			PostDestroy = null;
			GetRandomNumber = null;
			PostBattleMessage = null;
			GenerateAttributeEffect = null;
		}

		//When we need institute hp auto recovery use this.
		private void HealthAutoRecovery( int deltaTime )
		{
			if( Alive() )
			{
				healthRecoverTimer += deltaTime;
				if ( healthRecoverTimer > healthRecoverInterval )
				{
					hp += healthRecover;
					healthRecoverTimer = 0;

					RenderMessage rm = new RenderMessage();
					rm.type = RenderMessage.Type.SyncHP;
					rm.ownerId = id;
					rm.arguments.Add( "type", type );
					rm.arguments.Add( "hp", hp );
                    rm.arguments.Add( "mark", mark );
                    rm.arguments.Add( "maxHp", maxHp );

                    PostRenderMessage( rm );
				}
			}
		}

		#endregion

		public void RegisterGenerateAttributeEffectMethod( AttributeEffectGenerator method )
		{
			GenerateAttributeEffect = method;	
		}

		public int GetBuildCost()
		{
			return buildCost;
		}
    }
}
