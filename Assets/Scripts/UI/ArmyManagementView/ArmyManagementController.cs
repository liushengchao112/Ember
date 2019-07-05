using System.Collections.Generic;

using Data;
using Network;
using Utils;

namespace UI
{
    public class ArmyManagementController : ControllerBase
    {
        private ArmyManagementView view;

        public List<UnitsProto.Unit> unitsProto;
        public List<InstituteSkillProto.InstituteSkill> instituteSkillsProto;
        public List<LocalizationProto.Localization> localizationsProto;
        public List<AttributeEffectProto.AttributeEffect> attributeEffectsProto;

        private int currentUnlockSkillDeckIndex = 0;
        public int currentSelectSkillDeckIndex = 0;

        private DataManager dataManager;

        public ArmyManagementController( ArmyManagementView v )
        {
            viewBase = v;
            view = v;

            dataManager = DataManager.GetInstance();

            unitsProto = dataManager.unitsProtoData;
            instituteSkillsProto = dataManager.instituteSkillProtoData;
            localizationsProto = dataManager.localizationProtoData;
            attributeEffectsProto = dataManager.attributeEffectProtoData;
        }

        public override void OnResume()
        {
            base.OnResume();
            NetworkManager.RegisterServerMessageHandler( MsgCode.JoinArmyMessage, HandleJoinBattleListFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.BuySciencePageMessage, HandleBuySkillArrributeFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.ChangeScienceSkillMessage, HandleChangeScienceSkillFeedback );

            MessageDispatcher.AddObserver( RefreshArmyManagement, Constants.MessageType.RefreshPlayerUnitsData );
            MessageDispatcher.AddObserver( RefreshArmyManagement, Constants.MessageType.RefreshPlayerBattleListData );
        }

        public override void OnPause()
        {
            base.OnPause();
            NetworkManager.RemoveServerMessageHandler( MsgCode.JoinArmyMessage, HandleJoinBattleListFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.BuySciencePageMessage, HandleBuySkillArrributeFeedback );
            NetworkManager.RemoveServerMessageHandler( MsgCode.ChangeScienceSkillMessage, HandleChangeScienceSkillFeedback );

            MessageDispatcher.RemoveObserver( RefreshArmyManagement, Constants.MessageType.RefreshPlayerUnitsData );
            MessageDispatcher.RemoveObserver( RefreshArmyManagement, Constants.MessageType.RefreshPlayerBattleListData );
        }

        #region Data UI

        //Army UI Data
        public int GetUnitsCount()
        {
            return dataManager.GetPlayerUnits().soldiers.Count;
        }

        public List<SoldierInfo> GetUnitsList()
        {
            return dataManager.GetPlayerUnits().soldiers;
        }

        public int GetUnitCount( int battleListId, int index )
        {
            SoldierInfo info = GetUnitsList()[index];

            int count = info.count;

            List<SoldierInfo> battleList = GetDeckUnitList( battleListId );

            for ( int i = 0; i < battleList.Count; i++ )
            {
                if ( info.metaId == battleList[i].metaId )
                    count--;
            }

            return count;
        }

        public int GetArmyIcon( int id )
        {
            if ( unitsProto.Find( p => p.ID == id ) == null )
                DebugUtils.LogError( DebugUtils.Type.UI, "UnitItem is null , the unit id is " + id );
            return unitsProto.Find( p => p.ID == id ).Icon_box;
        }

        //Army Deck UI Data
        public List<SoldierInfo> GetDeckUnitList( int id )
        {
            List<SoldierInfo> battleListSoldier = new List<SoldierInfo>();

            List<int> soldierIds = dataManager.GetBattleArmyList().Find( p => p.listId == id ).unitIds;

            foreach ( int item in soldierIds )
            {
                SoldierInfo info = dataManager.GetPlayerUnits().soldiers.Find( p => p.metaId == item );

                battleListSoldier.Add( info );
            }

            if ( battleListSoldier.Count < 9 )
                DebugUtils.Log( DebugUtils.Type.UI, "BattleList Insufficient 9" );

            return battleListSoldier;
        }

        public int GetDeckUnitIcon( int listId, int index )
        {
            int id = GetDeckUnitList( listId )[index].metaId;

            return unitsProto.Find( p => p.ID == id ).Icon_box;
        }

        //Institute UI Data
        private List<InstituteSkillProto.InstituteSkill> GetInstituteList()
        {
            List<InstituteSkillProto.InstituteSkill> skillList = new List<InstituteSkillProto.InstituteSkill>();

            skillList.AddRange( instituteSkillsProto.FindAll( p => p.RequiredLevel == 0 ) );

            for ( int i = 0; i < skillList.Count; i++ )
            {
                for ( int j = i; j < skillList.Count; j++ )
                {
                    InstituteSkillProto.InstituteSkill skill;

                    bool isEquip = ( dataManager.GetPlayerSetedPackageInstituteSkills( currentSelectSkillDeckIndex ).Find( p => p == skillList[i].ID ) != 0 );

                    if ( !isEquip )
                    {
                        skill = skillList[i];
                        skillList[i] = skillList[j];
                        skillList[j] = skill;
                    }
                }
            }
            return skillList;
        }

        public int GetInstituteSkillCount()
        {
            return GetInstituteList().Count;
        }

        public int GetInstituteSkillId( int skillIndex )
        {
            return GetInstituteList()[skillIndex].ID;
        }

        public int GetInstituteSkillIcon( int skillIndex )
        {
            return GetInstituteList()[skillIndex].IconID;
        }

        public int GetInstituteSkillUnlockLevel( int skillIndex )
        {
            return GetInstituteList()[skillIndex].RequiredLevel;
        }

        public string GetInstituteSkillName( int skillIndex )
        {
            int texId = GetInstituteList()[skillIndex].Txt_ID;
            return ExtensionMethodUtils.Localize( texId );
        }

        public bool GetInstituteSkillIsLock( int skillIndex )
        {
            int unlockLevel = GetInstituteSkillUnlockLevel( skillIndex );
            int playerLevel = dataManager.GetPlayerLevel();

            return unlockLevel > playerLevel;
        }

        public bool GetInstituteSkillIsEquip( int skillIndex )
        {
            InstituteSkillProto.InstituteSkill skillItem = GetInstituteList()[skillIndex];

            foreach ( int id in dataManager.GetPlayerSetedPackageInstituteSkills( currentSelectSkillDeckIndex ) )
            {
                if ( skillItem.ID == id )
                {
                    return true;
                }
            }
            return false;
        }

        //Skill Deck UI Data

        public bool IsLockSkillDeck( int skillDeckItemIndex )
        {
            if ( DataManager.GetInstance().GetPlayerSetedPackageInstituteSkills( skillDeckItemIndex ).Count == 0 )
                return true;
            return false;
        }

        public bool isEnoughUnlockSkillDeck2()
        {
            return DataManager.GetInstance().GetPlayerEmber() >= 500;
        }

        public bool isEnoughUnlockSkillDeck3()
        {
            return DataManager.GetInstance().GetPlayerDiamond() >= 300;
        }

        public int GetSkillDeckItemIcon( int skillDeckItemIndex )
        {
            DataManager data = DataManager.GetInstance();
            int id = data.GetPlayerSetedPackageInstituteSkills( currentSelectSkillDeckIndex )[skillDeckItemIndex];
            return instituteSkillsProto.Find( p => p.ID == id ).IconID;
        }

        public string GetSkillDeckItemName( int skillDeckItemIndex )
        {
            DataManager data = DataManager.GetInstance();

            int id = data.GetPlayerSetedPackageInstituteSkills( currentSelectSkillDeckIndex )[skillDeckItemIndex];
            int texId = instituteSkillsProto.Find( p => p.ID == id ).Txt_ID;

            return ExtensionMethodUtils.Localize( texId );
        }

        //Skill Prop Data
        public string GetSkillPropDescription( int skillId )
        {
            int desId = instituteSkillsProto.Find( p => p.ID == skillId ).Description_Id;
            return ExtensionMethodUtils.Localize( desId );
        }

        private int GetSkillBuffId( int index, int skillId )
        {
            string buffStr = instituteSkillsProto.Find( p => p.ID == skillId ).BuffId;

            string[] buffArr = buffStr.Split( '|' );

            return int.Parse( buffArr[index] );
        }

        private float GetSkillBuffValue( int index, int skillId )
        {
            return attributeEffectsProto.Find( p => p.ID == GetSkillBuffId( index, skillId ) ).MainValue;
        }

        public string GetSkillBuffValueStr( int index, int skillId )
        {
            bool isPercent = attributeEffectsProto.Find( p => p.ID == GetSkillBuffId( index, skillId ) ).CalculateType == 2;
            float value = GetSkillBuffValue( index, skillId );


            return isPercent ? value * 100 + "%" : value.ToString();
        }

        private List<AttributeEffectProto.AttributeEffect> GetSkillDeckBuff()
        {
            List<AttributeEffectProto.AttributeEffect> buff = new List<AttributeEffectProto.AttributeEffect>();
            foreach ( int skillId in dataManager.GetPlayerSetedPackageInstituteSkills( currentSelectSkillDeckIndex ) )
            {
                buff.Add( attributeEffectsProto.Find( p => p.ID == GetSkillBuffId( 0, skillId ) ) );
                buff.Add( attributeEffectsProto.Find( p => p.ID == GetSkillBuffId( 1, skillId ) ) );
            }
            return buff;
        }

        private List<float> GetSkillBuffValue( int skillId )
        {
            List<float> buffValue = new List<float>();

            buffValue.Add( attributeEffectsProto.Find( p => p.ID == GetSkillBuffId( 0, skillId ) ).MainValue );
            buffValue.Add( attributeEffectsProto.Find( p => p.ID == GetSkillBuffId( 1, skillId ) ).MainValue );

            return buffValue;
        }

        private List<AttributeEffectProto.AttributeEffect> GetSkillBuffType()
        {
            List<AttributeEffectProto.AttributeEffect> buffNewList = new List<AttributeEffectProto.AttributeEffect>();

            foreach ( AttributeEffectProto.AttributeEffect item in GetSkillDeckBuff() )
            {
                if ( buffNewList.Find( p => p.AttributeType == item.AttributeType ) == null )
                    buffNewList.Add( item );
            }

            return buffNewList;
        }

        public string GetSkillDeckPropsString()
        {
            string strings = "";
            int index = 0;

            List<AttributeEffectProto.AttributeEffect> buffs = new List<AttributeEffectProto.AttributeEffect>();
            for ( int i = 0; i < GetSkillBuffType().Count; i++ )
            {
                int type = GetSkillBuffType()[i].AttributeType;

                buffs = GetSkillDeckBuff().FindAll( p => p.AttributeType == type );

                if ( string.IsNullOrEmpty( GetSkillBuffPropStr( buffs ) ) )
                    continue;

                if ( index % 2 != 0 )
                    strings += "\t" + GetSkillBuffPropStr( buffs ) + "\n";
                else
                    strings += GetSkillBuffPropStr( buffs ) + "\t";

                index++;
            }
            return strings;
        }

        private string GetSkillBuffPropStr( List<AttributeEffectProto.AttributeEffect> buffs )
        {
            float value = 0;
            string str;
            for ( int i = 0; i < buffs.Count; i++ )
            {
                value += buffs[i].AffectedType == 1 ? buffs[i].MainValue : -buffs[i].MainValue;
            }

            if ( value == 0 )
                return "";

            if ( value > 0 )
                str = ExtensionMethodUtils.Localize( buffs.Find( d => d.AffectedType == 1 ).DescriptionId );
            else
                str = ExtensionMethodUtils.Localize( buffs.Find( d => d.AffectedType == 2 ).DescriptionId );

            if ( buffs[0].CalculateType == 2 )
                return string.Format( str, UnityEngine.Mathf.Abs( value * 100 ) + "%" );

            return string.Format( str, UnityEngine.Mathf.Abs( value ) );
        }

        #endregion

        private void RefreshArmyManagement()
        {
            view.RefreshArmyItem();
        }

        #region Send

        public void SaveJoinBattleList( int listId, int position, ArmyCardItem item )
        {
            List<SoldierInfo> soldierList = DataManager.GetInstance().GetPlayerUnits().soldiers;

            int soldierId = soldierList.Find( p => p.metaId == item.id ).metaId;

            SendJoinBattleListC2S( listId, position, soldierId );
        }

        private void SendJoinBattleListC2S( int battleListId, int postion, int soldierId )
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

            JoinArmyC2S joinListData = new JoinArmyC2S();

            joinListData.armyId = battleListId;
            joinListData.position = postion;
            joinListData.soldierId = soldierId;

            byte[] data = ProtobufUtils.Serialize( joinListData );
            NetworkManager.SendRequest( MsgCode.JoinArmyMessage, data );
        }

        public void SendUnlockSkillDeck( int skillDeckIndex )
        {
            currentUnlockSkillDeckIndex = skillDeckIndex;
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

            BuySciencePageC2S message = new BuySciencePageC2S();

            message.pageId = skillDeckIndex;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.BuySciencePageMessage, data );
        }

        public void SaveJoinSkillDeckList( int skillIndex, int skillId )
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

            ChangeScienceSkillC2S message = new ChangeScienceSkillC2S();

            message.pageId = currentSelectSkillDeckIndex;
            message.index = skillIndex;
            message.skillId = skillId;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.ChangeScienceSkillMessage, data );
        }

        #endregion

        #region Reponse Handle

        private void HandleJoinBattleListFeedback( byte[] data )
        {
            UILockManager.ResetGroupState( UIEventGroup.Middle );
            JoinArmyS2C feedback = ProtobufUtils.Deserialize<JoinArmyS2C>( data );

            if ( feedback.result )
            {

            }
        }

        private void HandleBuySkillArrributeFeedback( byte[] data )
        {
            UILockManager.ResetGroupState( UIEventGroup.Middle );
            BuySciencePageS2C feedback = ProtobufUtils.Deserialize<BuySciencePageS2C>( data );

            if ( feedback.result )
            {
                DataManager manager = DataManager.GetInstance();

                if ( feedback.pageId == 1 )
                {
                    view.SetSecondSkillDeck( true );
                }
                else if ( currentUnlockSkillDeckIndex == 2 )
                {
                    view.SetThirdSkillDeck( true );
                }

                manager.SetPlayerSetedPackageInstituteSkills( feedback.pageId, feedback.scienceSkillIds );
            }
        }

        private void HandleChangeScienceSkillFeedback( byte[] data )
        {
            UILockManager.ResetGroupState( UIEventGroup.Middle );
            ChangeScienceSkillS2C feedback = ProtobufUtils.Deserialize<ChangeScienceSkillS2C>( data );

            if ( feedback.result )
            {
                DataManager.GetInstance().SetPlayerSetedPackageInstituteSkills( feedback.pageId, feedback.scienceSkillIds );

                view.RefreshInstituteItem();
            }
        }

        #endregion

    }
}
