using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Utils;

using SkillProto = Data.UnitSkillsProto.UnitSkill;

namespace Logic
{
    public class UnitSkillHandler
    {
        public enum UnitSkillTriggerType
        {
            None = 0,
            OnAttackOrderDistance = 1,
            OnAttackOrder = 2,
            OnPersistentIdleState = 3,
            OnAliveStateChanged = 4,
            OnAttackCountIncrease = 5,
            OnHurt = 6,
            OnHealthLower = 7,
            OnDeath = 8,
            OnKillCount = 9,
            OnMoveDistance = 10,
            OnAttackSameTarget = 11
        }

        private class SkillHandler
        {
            public SkillProto skillProto;
            public Skill skill;
            public long skillId;
            public bool enabled;
            public bool inCD;
            public int skillIndex;
            public UnitSkillTriggerType unitSkillTriggerType;
            public long triggerValue;
            public int cdTimer;
        }

        private Soldier owner;
        private UnitBehaviorListener ownerStateListener;
        private Dictionary<int, SkillHandler> ownerSkillDic = new Dictionary<int, SkillHandler>();
        private Dictionary<UnitSkillTriggerType, List<SkillHandler>> ownerSkills = new Dictionary<UnitSkillTriggerType, List<SkillHandler>>();
        private bool isEnabled = true;

        // Runtime data
        private int attackTimes = 0;
        private int killCount = 0;
        private LogicUnit lastTarget;
        private int restoreTargetTimer = 0;
        private int restoreTargetTimeLimit = 5;

        private bool timingClearTarget = false;
        private bool inChaseState = false;
        private bool inIdleState = false;
        private bool timingResetMoveDistance = false;
        private bool timinIdleState = false;
        private long chaseDistance = 0;
        private long moveDistance = 0;
        private int idleTime = 0;

        private List<int> skillFilter = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};

        public UnitSkillHandler( Soldier s )
        {
            owner = s;
            ownerStateListener = s.stateListener;
        }

        public void AddSkill( SkillProto proto, int index )
        {
            if ( ownerSkillDic.ContainsKey( proto.ID ) )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Skill, string.Format( "Unit {0} can't equip the skill {1} twice!", owner.unitType, proto.ID ) );
                return;
            }

            // temp code.//--test skill add Id 7/8/9/10/12/15/16
            if ( skillFilter.Contains( proto.ID ) )
            {
                UnitSkillTriggerType triggerEvent = (UnitSkillTriggerType)proto.TriggerEvent;
                if ( !ownerSkills.ContainsKey( triggerEvent ) )
                {
                    ownerSkills.Add( triggerEvent, new List<SkillHandler>() );
                }

                RegisteOwnerStateListener( triggerEvent );

                SkillHandler data = new SkillHandler();
                data.skillProto = proto;
                data.skillIndex = index;
                data.enabled = false;
                data.skill = null;
                data.unitSkillTriggerType = triggerEvent;
                data.triggerValue = proto.TriggerValue;

                ownerSkillDic.Add( proto.ID, data );
                ownerSkills[triggerEvent].Add( data );

                DebugUtils.Log( DebugUtils.Type.AI_SkillTrigger, string.Format( "force {0} unit {1} equip the {2}. metaId = {3}, trigger = {4} ", owner.mark, owner.id, proto.Name, proto.ID, triggerEvent ) );
            }
        }

        public void SetHandlerEnabled( bool enabled )
        {
            isEnabled = enabled;
        }

        private void RegisteOwnerStateListener( UnitSkillTriggerType type )
        {
            switch ( type )
            {
                case UnitSkillTriggerType.OnAliveStateChanged:
                {
                    ownerStateListener.RegisterAliveStateListener( OwnerAliveStateChange );
                    break;
                }
                case UnitSkillTriggerType.OnAttackCountIncrease:
                {
                    ownerStateListener.RegisterFightListener( OwnerAttackCountIncrease );
                    break;
                }
                case UnitSkillTriggerType.OnAttackOrderDistance:
                case UnitSkillTriggerType.OnAttackOrder:
                {
                    ownerStateListener.RegisterAttackOrderListener( OwnerReciveAttackOrder );
                    break;
                }
                //{
                //    ownerStateListener.RegisterChasingStateListener( OwnerChasing );
                //    break;
                //}
                case UnitSkillTriggerType.OnAttackSameTarget:
                {
                    timingClearTarget = true;
                    ownerStateListener.RegisterFightListener( OwnerAttackSameTarget );
                    break;
                }
                case UnitSkillTriggerType.OnPersistentIdleState:
                {
                    ownerStateListener.RegisterIdleStateListener( OwnerIdleStateChange );
                    break;
                }
                case UnitSkillTriggerType.OnHurt:
                {
                    ownerStateListener.RegisterHurtListener( OwnerHurt );
                    break;
                }
                case UnitSkillTriggerType.OnHealthLower:
                {
                    ownerStateListener.RegisterHealthChangeListener( OwnerHealthChange );
                    break;
                }
                case UnitSkillTriggerType.OnDeath:
                {
                    ownerStateListener.RegisterDeathStateListener( OwnerDeath );
                    break;
                }
                case UnitSkillTriggerType.OnKillCount:
                {
                    ownerStateListener.RegisterKillEnemyListener( OwnerKillEnemy );
                    break;
                }
                case UnitSkillTriggerType.OnMoveDistance:
                {
                    ownerStateListener.RegisterMoveDistanceListener( HandleOwnerMoveDistance );
                    break;
                }
                default:
                {
                    DebugUtils.LogError( DebugUtils.Type.AI_SkillTrigger, string.Format( "Can't handle {0} as UnitSkillTriggerType", type ) );
                    break;
                }
            }

            DebugUtils.LogWarning( DebugUtils.Type.AI_SkillTrigger, string.Format( "Register skill trigger condition {0}", type ) );
        }

        private void RemoveOwnerStateListener( UnitSkillTriggerType type )
        {
            switch ( type )
            {
                case UnitSkillTriggerType.OnAliveStateChanged:
                {
                    ownerStateListener.RemoveAliveListener( OwnerAliveStateChange );
                    break;
                }
                case UnitSkillTriggerType.OnAttackCountIncrease:
                {
                    ownerStateListener.RemoveFightListener( OwnerAttackCountIncrease );
                    break;
                }
                case UnitSkillTriggerType.OnAttackOrderDistance:
                case UnitSkillTriggerType.OnAttackOrder:
                {
                    ownerStateListener.RemoveAttackOrderListener( OwnerReciveAttackOrder );
                    break;
                }
                //{
                //    ownerStateListener.RemoveChasingStateListener( OwnerChasing );
                //    break;
                //}
                case UnitSkillTriggerType.OnAttackSameTarget:
                {
                    timingClearTarget = true;
                    ownerStateListener.RemoveFightListener( OwnerAttackSameTarget );
                    break;
                }
                case UnitSkillTriggerType.OnPersistentIdleState:
                {
                    ownerStateListener.RemoveIdleStateListener( OwnerIdleStateChange );
                    break;
                }
                case UnitSkillTriggerType.OnHurt:
                {
                    ownerStateListener.RemoveHurtListener( OwnerHurt );
                    break;
                }
                case UnitSkillTriggerType.OnHealthLower:
                {
                    //ownerStateListener.RemoveHealthChangeListener( OwnerHealthChange );
                    break;
                }
                case UnitSkillTriggerType.OnDeath:
                {
                    ownerStateListener.RemoveDeathStateListener( OwnerDeath );
                    break;
                }
                case UnitSkillTriggerType.OnKillCount:
                {
                    ownerStateListener.RemoveKillEnemyListener( OwnerKillEnemy );
                    break;
                }
                case UnitSkillTriggerType.OnMoveDistance:
                {
                    ownerStateListener.RemoveMoveDistanceListener( HandleOwnerMoveDistance );
                    break;
                }
                default:
                {
                    DebugUtils.LogError( DebugUtils.Type.AI_SkillTrigger, string.Format( "Can't handle {0} as UnitSkillTriggerType", type ) );
                    break;
                }
            }

            DebugUtils.LogWarning( DebugUtils.Type.AI_SkillTrigger, string.Format( "Register skill trigger condition {0}", type ) );
        }

        #region Trigger Event
        private bool OwnerAliveStateChange( bool alive )
        {
            bool result = false;
            DebugUtils.Assert( ownerSkills.ContainsKey( UnitSkillTriggerType.OnAliveStateChanged ), "UnitSkillHandler didn't register skill trigger OnAliveStateChanged, but still trigger it" );

            List<SkillHandler> skills = ownerSkills[UnitSkillTriggerType.OnAliveStateChanged];
            if ( alive )
            {
                // alive
                bool r = false;
                for ( int i = 0; i < skills.Count; i++ )
                {
                    r = ExecuteSkill( skills[i] );

                    if ( r )
                    {
                        result = r;
                    }
                }
            }
            else
            {
                // death
                for ( int i = 0; i < skills.Count; i++ )
                {
                    TerminateSkill( skills[i] );
                }
            }

            return result;
        }

        private bool OwnerAttackCountIncrease( LogicUnit target )
        {
            DebugUtils.Assert( ownerSkills.ContainsKey( UnitSkillTriggerType.OnAttackCountIncrease ), "UnitSkillHandler didn't register skill trigger OnAttackCount, but still trigger it" );

            bool result = false;

            attackTimes++;

            List<SkillHandler> skills = ownerSkills[UnitSkillTriggerType.OnAttackCountIncrease];
            for ( int i = 0; i < skills.Count; i++ )
            {
                SkillHandler skill = skills[i];

                if ( attackTimes >= skill.triggerValue )
                {
                    result = ExecuteSkill( skill );
                    attackTimes = 0;
                }
            }

            return result;
        }

        private bool OwnerAttackSameTarget( LogicUnit target )
        {
            DebugUtils.Assert( ownerSkills.ContainsKey( UnitSkillTriggerType.OnAttackSameTarget ), "UnitSkillHandler didn't register skill trigger OnAttackSameTarget, but still trigger it" );
            DebugUtils.LogWarning( DebugUtils.Type.AI_SkillTrigger, string.Format( "Current attack: last target {0}, target {1}", lastTarget == null ? "null" : lastTarget.id.ToString(), target == null ? "null" : target.id.ToString() ) );

            bool result = false;

            List<SkillHandler> skills = ownerSkills[UnitSkillTriggerType.OnAttackSameTarget];
            for ( int i = 0; i < skills.Count; i++ )
            {
                SkillHandler skillHandler = skills[i];
                if ( lastTarget != null && lastTarget.Alive() &&
                         target != null && target.Alive() )
                {
                    if ( target.id == lastTarget.id )
                    {
                        result = ExecuteSkill( skillHandler );

                        restoreTargetTimer = 0;
                    }
                    else
                    {
                        lastTarget = null;
                    }
                }
            }

            lastTarget = target;

            return result;
        }

        private void OwnerIdleStateChange( bool enterState )
        {
            if ( enterState )
            {
                idleTime = 0;
                timinIdleState = true;

                DebugUtils.LogWarning( DebugUtils.Type.AI_SkillTrigger, string.Format( " owner {0} enter idle state ", owner.id ) );
                // excute skill code are in the RuntimeDataRecording()
            }
            else
            {
                DebugUtils.Assert( ownerSkills.ContainsKey( UnitSkillTriggerType.OnPersistentIdleState ), "UnitSkillHandler didn't register skill trigger OnPersistentIdleState, but still trigger it" );
                DebugUtils.LogWarning( DebugUtils.Type.AI_SkillTrigger, string.Format( " owner {0} out idle state ", owner.id ) );

                timinIdleState = false;
                idleTime = 0;
            }
        }

        private bool OwnerHurt()
        {
            bool result = false;
            DebugUtils.Assert( ownerSkills.ContainsKey( UnitSkillTriggerType.OnHurt ), "UnitSkillHandler didn't register skill trigger OnHurt, but still trigger it" );

            bool r = false;

            List<SkillHandler> skills = ownerSkills[UnitSkillTriggerType.OnHurt];
            for ( int i = 0; i < skills.Count; i++ )
            {
                r = ExecuteSkill( skills[i] );
                if ( r )
                {
                    result = r;
                }
            }

            return result;
        }

        private bool OwnerReciveAttackOrder()
        {
            bool result = false;
            DebugUtils.Assert( ownerSkills.ContainsKey( UnitSkillTriggerType.OnAttackOrder ) || ownerSkills.ContainsKey( UnitSkillTriggerType.OnAttackOrderDistance ), "UownerSkills.ContainsKey( UnitSkillTriggerType.OnAttackOrder )nitSkillHandler didn't register skill trigger OwnerReciveAttackOrder, but still trigger it" );

            bool r = false;

            List<SkillHandler> skills = new List<SkillHandler>();
            if ( ownerSkills.ContainsKey( UnitSkillTriggerType.OnAttackOrder ) )
            {
                skills.AddRange( ownerSkills[UnitSkillTriggerType.OnAttackOrder] );
            }

            if ( ownerSkills.ContainsKey( UnitSkillTriggerType.OnAttackOrderDistance ) )
            {
                skills.AddRange( ownerSkills[UnitSkillTriggerType.OnAttackOrderDistance] );
            }

            for ( int i = 0; i < skills.Count; i++ )
            {
                r = ExecuteSkill( skills[i] );
                if ( r )
                {
                    result = r;
                }
            }

            return result;
        }

        private bool OwnerChaseStateChange( bool enterState )
        {
            bool result = false;
            DebugUtils.Assert( ownerSkills.ContainsKey( UnitSkillTriggerType.OnAttackOrder ), "UnitSkillHandler didn't register skill trigger OnAttackOrder, but still trigger it" );

            inChaseState = enterState;

            List<SkillHandler> skills = ownerSkills[UnitSkillTriggerType.OnAttackOrder];
            if ( enterState )
            {
                // begin chase
                bool r = false;
                for ( int i = 0; i < skills.Count; i++ )
                {
                    r = ExecuteSkill( skills[i] );
                    if ( r )
                    {
                        result = r;
                    }
                }

                chaseDistance = 0;
            }
            else
            {
                // end chase
                for ( int i = 0; i < skills.Count; i++ )
                {
                    TerminateSkill( skills[i] );
                }

                chaseDistance = 0;
            }

            return result;
        }

        private bool OwnerChasing( long distanceSqr )
        {
            bool result = false;

            DebugUtils.Assert( ownerSkills.ContainsKey( UnitSkillTriggerType.OnAttackOrderDistance ), "UnitSkillHandler didn't register skill trigger OnAttackOrderDistance, but still trigger it" );

            List<SkillHandler> skills = ownerSkills[UnitSkillTriggerType.OnAttackOrderDistance];
            bool r = false;
            for ( int i = 0; i < skills.Count; i++ )
            {
                SkillHandler skillData = skills[i];
                if ( skills[i].triggerValue > distanceSqr )
                {
                    r = ExecuteSkill( skills[i] );
                    if ( r )
                    {
                        result = r;
                    }
                }
            }

            return result;
        }

        private bool HandleOwnerMoveDistance( long distancePerFrame )
        {
            bool result = false;

            bool r = false;
            // record move distance (chase and walk)
            moveDistance += distancePerFrame;

            DebugUtils.Assert( ownerSkills.ContainsKey( UnitSkillTriggerType.OnMoveDistance ), "UnitSkillHandler didn't register skill trigger OnMoveDistance, but still trigger it" );

            List<SkillHandler> skills = ownerSkills[UnitSkillTriggerType.OnMoveDistance];
            for ( int i = 0; i < skills.Count; i++ )
            {
                SkillHandler skill = skills[i];
                if ( moveDistance > skill.triggerValue )
                {
                    r = ExecuteSkill( skill );
                    if ( r )
                    {
                        result = r;
                        moveDistance = 0;
                    }
                }
            }

            return result;
        }

        private bool OwnerHealthChange( int hp, int maxHp )
        {
            bool result = false;

            bool r = false;
            DebugUtils.Assert( ownerSkills.ContainsKey( UnitSkillTriggerType.OnHealthLower ), "UnitSkillHandler didn't register skill trigger OnHealthLower, but still trigger it" );
            DebugUtils.Assert( maxHp > 0, "Max hp can't be 0" );

            List<SkillHandler> skills = ownerSkills[UnitSkillTriggerType.OnHealthLower];
            for ( int i = 0; i < skills.Count; i++ )
            {
                SkillHandler skill = skills[i];

                if ( skill.triggerValue > hp )
                {
                    r = ExecuteSkill( skill );
                }
                else
                {
                    TerminateSkill( skill );
                }

                if (r)
                {
                    result = r;
                }
            }

            return result;
        }

        // TODO: Need to check kill count only add by kill unit
        private bool OwnerKillEnemy()
        {
            bool result = false;

            bool r = false;
            DebugUtils.Assert( ownerSkills.ContainsKey( UnitSkillTriggerType.OnKillCount ), "UnitSkillHandler didn't register skill trigger OnKillCount, but still trigger it" );

            killCount++;

            List<SkillHandler> skills = ownerSkills[UnitSkillTriggerType.OnKillCount];
            for ( int i = 0; i < skills.Count; i++ )
            {
                SkillHandler skill = skills[i];

                if ( skill.triggerValue == 0 )
                {
                    r = ExecuteSkill( skills[i] );
                    if (r)
                    {
                        result = r;
                    }
                }
                else
                {
                    if ( killCount >= skill.triggerValue )
                    {
                        r = ExecuteSkill( skills[i] );
                        if ( r )
                        {
                            result = r;
                        }

                        killCount = 0;
                    }
                }
            }

            return result;
        }

        private bool OwnerDeath()
        {
            bool result = false;
            DebugUtils.Assert( ownerSkills.ContainsKey( UnitSkillTriggerType.OnDeath ), "UnitSkillHandler didn't register skill trigger OnDeath, but still trigger it" );

            List<SkillHandler> skills = ownerSkills[UnitSkillTriggerType.OnDeath];
            for ( int i = 0; i < skills.Count; i++ )
            {
                result = ExecuteSkill( skills[i] );
            }

            return result;
        }

        #endregion

        public void LogicUpdate( int deltaTime )
        {
            if ( timinIdleState )
            {
                idleTime += deltaTime;

                DebugUtils.Assert( ownerSkills.ContainsKey( UnitSkillTriggerType.OnPersistentIdleState ), "UnitSkillHandler didn't register skill trigger OnPersistentIdleState, but still trigger it" );

                List<SkillHandler> skills = ownerSkills[UnitSkillTriggerType.OnPersistentIdleState];
                for ( int i = 0; i < skills.Count; i++ )
                {
                    SkillHandler skill = skills[i];

                    int triggerValue = Mathf.RoundToInt( skill.triggerValue );
                    if ( idleTime > triggerValue )
                    {
                        ExecuteSkill( skill );
                        idleTime = 0;
                    }
                }
            }

            if ( timingResetMoveDistance )
            {
                if ( !owner.InWalkState() && !owner.InChaseState() )
                {
                    moveDistance = 0;
                }
            }

            // receive the attack event will reset restoreTargetTimer
            if ( timingClearTarget )
            {
                if ( restoreTargetTimer > restoreTargetTimeLimit )
                {
                    restoreTargetTimer = 0;
                    lastTarget = null;
                }
                else
                {
                    restoreTargetTimer += deltaTime;
                }
            }

            TimingSkillCD( deltaTime );
        }

        private bool ExecuteSkill( SkillHandler data )
        {
            bool result = false;
            if ( !isEnabled )
            {
                return result;
            }

            if ( data.inCD )
            {
                return result;
            }

            if ( !data.enabled )
            {
                if ( data.skill == null )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_SkillTrigger, string.Format( "{0} unit {1} execute skill {2} by trigger {3} protoId:{4}", owner.mark, owner.id, data.skillProto.Name, data.unitSkillTriggerType, data.skillProto.ID ) );

                    Skill skillUnit = owner.ReleaseSkill( ownerSkillDic[data.skillProto.ID].skillProto, ownerSkillDic[data.skillProto.ID].skillIndex );
                    if ( skillUnit )
                    {
                        data.enabled = true;
                        data.skill = skillUnit;
                        data.skillId = skillUnit.id;
                        result = skillUnit.DependOnSkillState();
                    }
                    else
                    {
                        DebugUtils.LogError( DebugUtils.Type.AI_SkillTrigger, string.Format( "{0} unit {1} execute skill {2} {3} failed! protoId:{4}", owner.mark, owner.id, data.skillProto.Name, data.unitSkillTriggerType, data.skillProto.ID ) );
                    }
                }
                else
                {
                    //DebugUtils.LogError( DebugUtils.Type.AI_SkillTrigger, " Skill unit was not empty, but SkillHandler in unable state,  It's strange! " );
                }
            }

            return result;
        }

        private void TerminateSkill( SkillHandler data )
        {
            if ( data.skill != null && data.enabled )
            {
                DebugUtils.Log( DebugUtils.Type.AI_SkillTrigger, string.Format( "{0} unit {1} terminate skill {2} id:{3} protoId:{4}", owner.mark, owner.id, data.skillProto.Name, data.skill.id, data.skillProto.ID ) );

                data.skill.ReleaseEnd();
            }
        }

        public void TerminateAllSkill()
        {
            foreach ( KeyValuePair<int, SkillHandler> skill in ownerSkillDic )
            {
                if ( skill.Value.enabled && skill.Value.skill != null )
                {
                    skill.Value.skill.ReleaseEnd();
                }
            }

            ownerSkillDic.Clear();
            ownerSkills.Clear();
        }

        public void ResetSkillHandler( long skillId )
        {
            bool result = false;

            foreach ( KeyValuePair<int, SkillHandler> pair in ownerSkillDic )
            {
                SkillHandler handler = pair.Value;
                if ( handler.enabled && handler.skillId == skillId )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Skill, string.Format( "{0} unit {1} 's skill {2} {3} enter the cooling state, CD:{4}, protoId:{5}", owner.mark, owner.id, handler.skillProto.Name, handler.skill.id, handler.skillProto.CD, handler.skillProto.ID ) );

                    handler.inCD = true;
                    handler.enabled = false;
                    handler.skill = null;

                    result = true;
                }
            }

            DebugUtils.Assert( result, string.Format( "Reset skillhandler faild when skill {0} destroy", skillId ) );
        }

        private void TimingSkillCD( int deltaTime )
        {
            foreach ( KeyValuePair<int, SkillHandler> pair in ownerSkillDic )
            {
                SkillHandler skillHandler = pair.Value;
                if ( !skillHandler.enabled && skillHandler.inCD )
                {
                    skillHandler.cdTimer += deltaTime;

                    if ( skillHandler.cdTimer >= ConvertUtils.ToLogicInt( skillHandler.skillProto.CD ) )
                    {
                        skillHandler.cdTimer = 0;
                        skillHandler.inCD = false;
                        DebugUtils.Log( DebugUtils.Type.AI_SkillTrigger, string.Format( "The cd of {0} unit {1} 's skill {2} is over, protoId:{3}", owner.mark, owner.id, skillHandler.skillProto.Name, skillHandler.skillProto.ID ) );
                    }
                }
            }
        }

        public void ExitSKillState()
        {
            // exit skill state need to stop the skill that depend on unit skill state
            foreach ( KeyValuePair<int, SkillHandler> pair in ownerSkillDic )
            {
                SkillHandler skillHandler = pair.Value;
                if ( skillHandler.enabled )
                {
                    if ( skillHandler.skill != null && skillHandler.skill.skillEnable && skillHandler.skill.DependOnSkillState() )
                    {
                        skillHandler.skill.Stop();
                    }
                    else
                    {
                        DebugUtils.LogWarning( DebugUtils.Type.AI_Skill, string.Format( "Why stop a skill {0}", skillHandler.skillId ) );
                    }
                }
            }
        }

        public void WaitChaseHandlerInSkill()
        {
            foreach ( KeyValuePair<int, SkillHandler> pair in ownerSkillDic )
            {
                SkillHandler handler = pair.Value;
                if ( handler.skill != null && handler.enabled )
                {
                    handler.skill.UnitFoundChasePath();
                }
            }
        }

        public void FinishSkillMove()
        {
            foreach ( KeyValuePair<int, SkillHandler> pair in ownerSkillDic )
            {
                SkillHandler handler = pair.Value;
                if ( handler.skill != null && handler.enabled )
                {
                    handler.skill.UnitFinishedMove();
                }
            }
        }

        public void Reset()
        {
            // TODO: Need more detail
            ownerSkillDic.Clear();
            owner = null;

            attackTimes = 0;
            killCount = 0;
            lastTarget = null;
            restoreTargetTimer = 0;
            restoreTargetTimeLimit = 3;

            inChaseState = false;
            inIdleState = false;
            timingResetMoveDistance = false;
            timinIdleState = false;
            chaseDistance = 0;
            moveDistance = 0;
            idleTime = 0;
        }
    }
}

