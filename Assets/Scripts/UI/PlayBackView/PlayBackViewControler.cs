using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using Utils;
using Data;
using Constants;
using Network;

namespace UI
{
    public class PlayBackViewControler : ControllerBase
    {
        private PlayBackView view;
        private DataManager dataManager;

        private ForceMark mark;
        private BattleType currentBattleType;        

        private List<AttributeEffectProto.AttributeEffect> attributeEffectData;
        private List<UnitsProto.Unit> unitsProtoData;
        private Dictionary<int , float[]> buffAttributes = new Dictionary<int , float[]>();
        private List<Battler> battlers = new List<Battler>();
        private List<Battler> blueBattlers = new List<Battler>();
        private List<Battler> redBattlers = new List<Battler>();

        private long battleDuration = 0;
        private int lastSecond = 0;
        private int emberCount = 0;
        private int killCount = 0, deadCount = 0;
        private int redKill = 0, blueKill = 0;

        public PlayBackViewControler( PlayBackView v )
        {
            viewBase = v;
            view = v;

            dataManager = DataManager.GetInstance();
            attributeEffectData = dataManager.attributeEffectProtoData;
            unitsProtoData = dataManager.unitsProtoData;
            currentBattleType = dataManager.GetBattleType();
            mark = dataManager.GetForceMark();
            battlers = dataManager.GetBattlers();
            battleDuration = dataManager.GetSimBattleDuration();
            SetBattlers();

            MessageDispatcher.AddObserver( AddBuildingBuffMessage , MessageType.AddBuildingBuffMessage );
            MessageDispatcher.AddObserver( AdjustTimer , MessageType.BattleTimeChanged );
            MessageDispatcher.AddObserver( SetEmber , MessageType.CoinChanged );
            MessageDispatcher.AddObserver( Unit_Destroy , MessageType.SoldierDeath );
            MessageDispatcher.AddObserver( KillIdolNotice , MessageType.BattleUIKillIdolNotice );
            MessageDispatcher.AddObserver( NoticeHandle , MessageType.BattleUIKillUnitNotice );
            MessageDispatcher.AddObserver( BattleResultReceived , MessageType.ShowBattleResultView );            
            MessageDispatcher.AddObserver( RemoveBuildingBuffMessage , MessageType.RemoveBuildingBuffMessage );
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            MessageDispatcher.RemoveObserver( AddBuildingBuffMessage , MessageType.AddBuildingBuffMessage );
            MessageDispatcher.RemoveObserver( AdjustTimer , MessageType.BattleTimeChanged );
            MessageDispatcher.RemoveObserver( SetEmber , MessageType.CoinChanged );
            MessageDispatcher.RemoveObserver( Unit_Destroy , MessageType.SoldierDeath );
            MessageDispatcher.RemoveObserver( KillIdolNotice , MessageType.BattleUIKillIdolNotice );
            MessageDispatcher.RemoveObserver( NoticeHandle , MessageType.BattleUIKillUnitNotice );
            MessageDispatcher.RemoveObserver( BattleResultReceived , MessageType.ShowBattleResultView );            
            MessageDispatcher.AddObserver( RemoveBuildingBuffMessage , MessageType.RemoveBuildingBuffMessage );
        }

        public long GetBattleDurationTime()
        {
            return battleDuration;
        }

        private void BattleResultReceived( object res , object info )
        {
            BattleResultData resInfo = (BattleResultData)info;
            NoticeType resType = (NoticeType)res;

            view.OpenBattleResult( resType , resInfo );
        }

        private void SetBattlers()
        {
            blueBattlers.Clear();
            redBattlers.Clear();
            for ( int i = 0; i < battlers.Count; i++ )
            {
                if ( battlers[i].side == MatchSide.Blue )
                {
                    blueBattlers.Add( battlers[i] );
                }
                else if ( battlers[i].side == MatchSide.Red )
                {
                    redBattlers.Add( battlers[i] );
                }
            }
        }

        public string GetBattleHeadIconStr( MatchSide side , int index )
        {
            string str = "";
            if ( side == MatchSide.Blue )
            {
                if ( blueBattlers.Count > index )
                {
                    return blueBattlers[index].portrait;
                }
            }
            else if ( side == MatchSide.Red )
            {
                if ( redBattlers.Count > index )
                {
                    return redBattlers[index].portrait;
                }
            }
            return str;
        }

        public List<int> GetUnitHeadIcon( MatchSide side, int index )
        {
            List<int> iconLsit = new List<int>();
            if ( side == MatchSide.Blue )
            {
                if ( blueBattlers.Count > index )
                {
                    for ( int i = 0; i < blueBattlers[index].battleUnits.Count; i++ )
                    {
                        for ( int j = 0; j < blueBattlers[index].battleUnits[i].count; j++ )
                        {
                            int icon = unitsProtoData.Find( p => p.ID == blueBattlers[index].battleUnits[i].metaId ).Icon_box;
                            iconLsit.Add( icon );
                        }
                    }
                }
            }
            else if ( side == MatchSide.Red )
            {
                if ( redBattlers.Count > index )
                {
                    for ( int i = 0; i < redBattlers[index].battleUnits.Count; i++ )
                    {
                        for ( int j = 0; j < redBattlers[index].battleUnits[i].count; j++ )
                        {
                            int icon = unitsProtoData.Find( p => p.ID == redBattlers[index].battleUnits[i].metaId ).Icon;
                            iconLsit.Add( icon );
                        }
                    }
                }
            }
            return iconLsit;
        }

        public ForceMark GetForceMark( MatchSide side , int index )
        {
            if ( side == MatchSide.Blue )
            {
                if ( blueBattlers.Count > index )
                {
                    return blueBattlers[index].forceMark;
                }
            }
            else if ( side == MatchSide.Red )
            {
                if ( redBattlers.Count > index )
                {
                    return redBattlers[index].forceMark;
                }
            }
            return ForceMark.NoneForce;
        }

        public void SetFollowForce( ForceMark mark )
        {
            MessageDispatcher.PostMessage( MessageType.SetCameraFollowForce, mark );
        }

        public void SetSpeed( int speed )
        {
            MessageDispatcher.PostMessage( MessageType.SetPlaybackSpeed, speed );
        }

        public void SetPauseAndPlay( int type )
        {
            MessageDispatcher.PostMessage( MessageType.SetPlaybackPlayingState, type );
        }

        private void AddBuildingBuffMessage( object str )
        {
            SplitBuffMessage( str , false );
        }

        private void RemoveBuildingBuffMessage( object str )
        {
            SplitBuffMessage( str , true );
        }

        private void SplitBuffMessage( object str , bool isRemoveAttribute )
        {
            string[] buffIds = str.ToString().Split( new char[] { '|' } , StringSplitOptions.RemoveEmptyEntries );

            for ( int i = 0; i < buffIds.Length; i++ )
            {
                int buffid = int.Parse( buffIds[i] );
                AttributeEffectProto.AttributeEffect attributeEffect = attributeEffectData.Find( p => p.ID == buffid );
                CalculationBuffAttribute( buffAttributes , attributeEffect.AttributeType , attributeEffect.AffectedType , attributeEffect.CalculateType , attributeEffect.MainValue , attributeEffect.DescriptionId , isRemoveAttribute );
            }

            BuildingBuffDescription();
        }

        private void CalculationBuffAttribute( Dictionary<int , float[]> attributes , int attributeType , int affectedType , int calculateType , float value , int descriptionId , bool isRemoveAttribute )
        {
            if ( affectedType == 2 )
            {
                value = -value;
            }

            if ( isRemoveAttribute )
            {
                value = -value;
            }

            int index = affectedType + 1;

            if ( attributes.ContainsKey( attributeType ) )
            {
                if ( calculateType == 1 )
                {
                    attributes[attributeType][0] += value;
                }
                else if ( calculateType == 2 )
                {
                    attributes[attributeType][1] += value;
                }
            }
            else if ( attributeType != 0 )
            {
                if ( calculateType == 1 )
                {
                    attributes.Add( attributeType , new float[] { value , 0 , 0 , 0 } );
                }
                else if ( calculateType == 2 )
                {
                    attributes.Add( attributeType , new float[] { 0 , value , 0 , 0 } );
                }
            }

            attributes[attributeType][index] = descriptionId;
        }

        private void BuildingBuffDescription()
        {
            string attribute = "";

            foreach ( int attributeType in buffAttributes.Keys )
            {
                float[] attributes = buffAttributes[attributeType];

                if ( attributes[0] != 0 )
                {
                    string description = attributes[0] > 0 ? ( (int)attributes[2] ).Localize() : ( (int)attributes[3] ).Localize();
                    attribute += string.Format( description , Mathf.Abs( attributes[0] ) ) + "\n";
                }

                if ( attributes[1] != 0 )
                {
                    string description = attributes[1] > 0 ? ( (int)attributes[2] ).Localize() : ( (int)attributes[3] ).Localize();
                    attribute += string.Format( description , Mathf.Abs( attributes[1] * 100 ) ) + "%" + "\n";
                }
            }

            if ( attribute.LastIndexOf( "\n" ) != -1 )
            {
                attribute = attribute.Remove( attribute.LastIndexOf( "\n" ) , 1 );
            }

            view.SetBuffPanelText( attribute );
        }
        
        private void AdjustTimer( object timeObj )
        {
            int curSecond = (int)(float)timeObj;
            if ( lastSecond == curSecond )
                return;

            lastSecond = curSecond;

            view.SetTimerText( lastSecond );            
        }

        private void SetEmber( object amount )
        {
            emberCount = (int)amount;
            view.SetEmberText( emberCount );            
        }

        private void Unit_Destroy( object markObj , object idObj )
        {
            long ownerId = (long)idObj;
            ForceMark mark = (ForceMark)markObj;
            
            if ( IsEnemyDead( this.mark , mark ) )
            {
                EnemyDead();
            }
            else if ( this.mark == mark )
            {
                view.SetDeadCountText( ++deadCount );
            }

            if ( GetSideFromMark( mark ) == MatchSide.Red )
            {
                view.SetBlueSideKillCountText( blueKill++ );
            }
            else if ( GetSideFromMark( mark ) == MatchSide.Blue )
            {
                view.SetRedSideKillCountText( redKill++ );
            }
        }

        private bool IsEnemyDead( ForceMark self , ForceMark other )
        {
            if ( GetSideFromMark( self ) == MatchSide.Red && GetSideFromMark( other ) == MatchSide.Blue )
            {
                return true;
            }
            else if ( GetSideFromMark( self ) == MatchSide.Blue && GetSideFromMark( other ) == MatchSide.Red )
            {
                return true;
            }
            return false;
        }

        private MatchSide GetSideFromMark( ForceMark mark )
        {
            if ( mark <= ForceMark.BottomRedForce )
            {
                return MatchSide.Red;
            }
            else if ( mark <= ForceMark.BottomBlueForce )
            {
                return MatchSide.Blue;
            }
            else
            {
                return MatchSide.NoSide;
            }
        }

        private void EnemyDead()
        {
            view.SetKillCountText( ++killCount );
        }

        private void KillIdolNotice( object markObj , object iconId )
        {
            view.noticeView.ShowKillIdolNotice( (ForceMark)markObj , (int)iconId );
        }

        private void NoticeHandle( object killerMarkObj , object killerIconId , object beKillerMarkObj , object beKillerIconId )
        {
            view.noticeView.ShowKillUnitNotice( (ForceMark)killerMarkObj , (int)killerIconId , (ForceMark)beKillerMarkObj , (int)beKillerIconId );
        }

        public void EnterMainMenu()
        {
            PlayBackManager.GetInstance().QuitPlaybattleBack();
        }       
    }
}
