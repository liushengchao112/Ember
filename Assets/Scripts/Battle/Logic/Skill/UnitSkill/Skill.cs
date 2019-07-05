using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Data;
using Utils;

using SkillProto = Data.UnitSkillsProto.UnitSkill;

namespace Logic
{
    public abstract class Skill : LogicUnit
    {
        public Soldier owner;
        public int index; // be used to play unit skill animation
        public UnitSkillHandler handler;
        public bool skillEnable;
        public bool handleOwnerMove = false;
        public bool playerOwner = false;
        
        protected List<AttributeEffect> buffHandler = new List<AttributeEffect>();

        // proto data
        protected string skillName;
        protected int textId;
        protected int duration;
        protected int distance;
        protected int radius;
        protected int repetition;
        protected List<int> attributeEffects = new List<int>();
        protected int carrierType;
        protected int carrierId;
        protected int effectId;
        private int skillPrepareTime = 0;// Useless for now
        protected string skillEffectBindPoint;
        protected int skillHitRes;
        protected string skillHitBindPoint;
        protected int trapMetaId;

        // Swing data
        protected int skillActionDuration;
        protected int skillActionHitTime;

        // Timer
        private int frontSwingTimer = 0;
        private int durationTimer = 0;

        // Register Generator method
        protected AttributeEffectGenerator GenerateAttributeEffect;
        protected ProjectileGenerator GenerateProjectile;
        protected SummonGenerator GenerateSummon;
		protected TrapGenerator GenerateTrap;

        // Register logic method
        protected FindOwnForceSoldiersMethod FindOwnForceSoldiers;
        protected FindFriendlySoldiersMethod FindFriendlySoldiers;
        protected FindOpponentSoldierMethod FindOpponentSoldier;
        protected FindOpponentSoldiersMethod FindOpponentSoldiers;
        protected FindOpponentBuildingMethod FindOpponentBuilding;
        protected FindOpponentBuildingsMethod FindOpponentBuildings;
        protected FindOpponentCrystalMethod FindOpponentCrystal;
        protected FindOpponentCrystalCarMethod FindOpponentCrystalCar;
        protected FindOpponentCrystalCarsMethod FindOpponentCrystalCars;
        protected FindOpponentDemolisherMethod FindOpponentDemolisher;
        protected FindOpponentDemolishersMethod FindOpponentDemolishers;
        protected FindNeutralUnitsMethod FindNeutralUnits;
        protected FindNeutralUnitMethod FindNeutralUnit;

        protected WithinCircleAreaPredicate WithinCircleAreaPredicate;
        protected WithinFrontRectAreaPredicate WithinFrontRectAreaPredicate; 
        protected WithinSectorAreaPredicate WithinSectorAreaPredicate; 

        public virtual void Initialize( long id, Soldier owner, SkillProto skillProto, int skillIndex )
        {
            InitializeSkillData( skillProto );

            this.owner = owner;
            this.id = id;
            mark = owner.mark;
            type = LogicUnitType.Skill;
            index = skillIndex;
            
            if( skillIndex == 1 )
            {
                skillActionDuration = owner.skill1Duration;
                skillActionHitTime =owner.skill1HitTime ;
            }
            else if ( skillIndex == 2 )
            {
                skillActionDuration = owner.skill2Duration;
                skillActionHitTime = owner.skill2HitTime;
            }

            playerOwner = owner.owner;

            skillEnable = true;

            DebugUtils.Log( DebugUtils.Type.AI_Skill, string.Format( "Skill {0} has been initialize, id = {1}, metaId = {2}", skillName, id, metaId ) );
        }

        protected virtual void InitializeSkillData( SkillProto skillProto )
        {
            skillName = skillProto.Name;
            metaId = skillProto.ID;
            textId = skillProto.Txt_ID;
            iconId = skillProto.IconID;
            duration = ConvertUtils.ToLogicInt( skillProto.Duration );
            distance = ConvertUtils.ToLogicInt( skillProto.Distance );
            radius = ConvertUtils.ToLogicInt( skillProto.SkillRadius );
            repetition = skillProto.Repetition;

            attributeEffects.Clear();

            string ae = skillProto.AttributeEffectIDs;
            if ( !string.IsNullOrEmpty( ae ) )
            {
                string[] ary = ae.Split( '|' );
                for ( int i = 0; i < ary.Length; i++ )
                {
                    int result = -1;
                    if ( int.TryParse( ary[i], out result ) )
                    {
                        attributeEffects.Add( result );
                    }
                    else
                    {
                        DebugUtils.LogError( DebugUtils.Type.AI_Skill, string.Format( "Parse skill proto's AttributeEffectIDs column failed, id = {0},  AttributeEffectIDs = {1}", skillProto.ID, skillProto.AttributeEffectIDs ) );
                    }
                }
                DebugUtils.Assert( attributeEffects.Count != 0, string.Format( "Why skill {0}'s attributeEffect's count equal to 0, metaid = {1}", skillProto.ID, skillProto.AttributeEffectIDs ) );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Skill, string.Format( "skill {0} attributeEffect string is null or empty, metaId = {1} ", skillProto.ID, skillProto.AttributeEffectIDs ) );
            }

            carrierType = skillProto.CarrierType;
            carrierId = skillProto.CarrierID;
            effectId = skillProto.SkillEffectID;

            skillEffectBindPoint = skillProto.skill_effect_bindpoint;
            skillHitRes = skillProto.skill_hit_res;
            skillHitBindPoint = skillProto.skill_hit_bindpoint;
        }

        public virtual void Fire()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Skill, string.Format( "Skill {0} has been Fire, id = {1}", skillName, id ) );

            if ( DependOnSkillState() )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Soldier, "soldier " + owner.id + " enters skill state." );

                owner.ChangeState( SoldierState.SKILL, owner.fsmSkill );
            }
        }

        public override void LogicUpdate( int deltaTime )
        {
            
        }

        // Current skill be interrupted
        public virtual void Stop()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Skill, string.Format( "Skill {0} has been Stop!, id = {1}", skillName, id ) );

            if ( skillEnable )
            {
                Destroy();
            }
        }

        // Finished Skill flow completely
        public virtual void ReleaseEnd()
        {
            if ( skillEnable )
            {
                Destroy();

                if ( DependOnSkillState() )
                {
                    owner.Idle();
                }
            }
        }

        public void Destroy()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Skill, string.Format( "Skill {0} has been ReleaseEnd, id = {1}", skillName, id ) );

            //state = SkillState.BackSwing;
            handler.ResetSkillHandler( id );

            PostDestroy( this );
            skillEnable = false;
        }

        // skill release depend on unit skill state
        // return true, unit will enter skill state and when skill end will enter idle state
        // return false, release skill will not change current unit's state,
        public virtual bool DependOnSkillState()
        {
            return false;
        }

        public virtual void UnitFinishedMove()
        {
            // Important: When unit finished current path in skill state, will call this method to notice skill the end of movement.
            if ( handleOwnerMove )
            {
                DebugUtils.Assert( false, " Skill didn't implement UnitFinishedMove method, why this method be called " );
            }
        }

        public virtual void UnitFoundChasePath()
        {
            // Important: When unit finished current path in skill state, will call this method to notice skill the end of movement.
            if ( handleOwnerMove )
            {
                DebugUtils.Assert( false, " Skill didn't implement UnitFinishedMove method, why this method be called " );
            }
        }

        public virtual void SyncMessageHandler( FixVector3 position )
        {
            DebugUtils.Assert( false, string.Format( " Skill didn't implement SyncMessageHandler method, why this method be called, metaid = {0}, id = {1}", metaId, id ) );
        }

        public override void Hurt( int hurtValue, AttackPropertyType hurtType, bool isCrit, LogicUnit injurer )
        {
            DebugUtils.Assert(false, " Skill didn't implement Hurt " );
        }

        public override void AddCoin( int coin )
        {

        }

        public override bool Alive()
        {
            return true;
        }

        public override void Reset()
        {
            buffHandler.Clear();

            frontSwingTimer = 0;
            durationTimer = 0;

            PostDestroy = null;
            PostBattleMessage = null;
            PostRenderMessage = null;

            // Register Generator method
            GenerateAttributeEffect = null;
            GenerateProjectile = null;

            // Register logic method
            FindOwnForceSoldiers = null;
            FindFriendlySoldiers = null;
            FindOpponentSoldier = null;
            FindOpponentSoldiers = null;
            FindOpponentBuilding = null;
            FindOpponentCrystal = null;
            FindOpponentCrystalCar = null;
            FindOpponentCrystalCars = null;
            FindOpponentDemolisher = null;
            FindOpponentDemolishers = null;
            FindNeutralUnits = null;
            FindNeutralUnit = null;

            WithinCircleAreaPredicate = null;
            WithinFrontRectAreaPredicate = null;
            WithinSectorAreaPredicate = null;
        }

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            DebugUtils.Assert( false, "OnKillEnemy() in Skill is not implemented!" );
        }

        #region Regsiter Method
        public void RegisterGenerateAttributeEffectMethod( AttributeEffectGenerator method )
        {
            GenerateAttributeEffect = method;
        }

        public void RegisterProjectileGenerator( ProjectileGenerator generator )
        {
            GenerateProjectile = generator;
        }

        public void RegisterSummonGenerator( SummonGenerator generator )
        {
            GenerateSummon = generator;
        }

        public void RegisterFindOwnForceSoldiers( FindOwnForceSoldiersMethod method )
        {
            FindOwnForceSoldiers = method;
        }

        public void RegisterFindFriendlySoldiers( FindFriendlySoldiersMethod method )
        {
            FindFriendlySoldiers = method;
        }

        public void RegisterFindOpponentSoldier( FindOpponentSoldierMethod method )
        {
            FindOpponentSoldier = method;
        }

        public void RegisterFindOpponentSoldiers( FindOpponentSoldiersMethod method )
        {
            FindOpponentSoldiers = method;
        }

        public void RegisterFindOpponentBuilding( FindOpponentBuildingMethod method )
        {
            FindOpponentBuilding = method;
        }

        public void RegisterFindOpponentBuildings( FindOpponentBuildingsMethod method )
        {
            FindOpponentBuildings = method;
        }

        public void RegisterFindOpponentCrystal( FindOpponentCrystalMethod method )
        {
            FindOpponentCrystal = method;
        }

        public void RegisterFindOpponentCrystalCar( FindOpponentCrystalCarMethod method )
        {
            FindOpponentCrystalCar = method;
        }

        public void RegisterFindOpponentCrystalCars( FindOpponentCrystalCarsMethod method )
        {
            FindOpponentCrystalCars = method;
        }

        public void RegisterFindOpponentDemolisher( FindOpponentDemolisherMethod method )
        {
            FindOpponentDemolisher = method;
        }

        public void RegisterFindOpponentDemolishers( FindOpponentDemolishersMethod method )
        {
            FindOpponentDemolishers = method;
        }

        public void RegisterFindNeutralUnit( FindNeutralUnitMethod method )
        {
            FindNeutralUnit = method;
        }

        public void RegisterFindNeutralUnits( FindNeutralUnitsMethod method )
        {
            FindNeutralUnits = method;
        }

        public void RegisterWithinFrontSectorAreaPredicate( WithinSectorAreaPredicate method )
        {
            WithinSectorAreaPredicate = method;
        }

        public void RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate method )
        {
            WithinCircleAreaPredicate = method;
        }

        public void RegisterWithinFrontRectangleAreaPredicate( WithinFrontRectAreaPredicate method )
        {
            WithinFrontRectAreaPredicate = method;
        }

        public void RegisterTrapGenerator( TrapGenerator method )
        {
            GenerateTrap = method;
        }
        #endregion
    }
}
