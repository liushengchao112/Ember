using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class UnitBehaviorListener
    {
        public delegate bool UnitDeathListener();
        public delegate bool UnitKillEnemyListener();
        public delegate bool UnitChaseStateListener( bool enterState );
        public delegate void UnitIdleStateListener( bool enterState );
        public delegate bool UnitAliveStateListener( bool alive );
        public delegate bool UnitAttackOrderListener();
        public delegate bool UnitHealthChangeListener( int currentHp, int maxHp );
        public delegate bool UnitChasingListener( long targetDistance );
        public delegate bool UnitMoveDistanceListener( long deltaDistance );
        public delegate bool UnitHurtListener();
        public delegate bool UnitFightListener( LogicUnit target ); // return bool will interrupt the fight operation.
        public delegate bool UnitSkillStateListener();
        public delegate void UnitAfterFight();

        private List<UnitFightListener> PostFightOperation;
        private List<UnitHurtListener> PostHurt;
        private List<UnitChaseStateListener> PostChaseStateChange;
        private List<UnitAttackOrderListener> PostAttackOrder;
        private List<UnitIdleStateListener> PostIdleStateChange;
        private List<UnitAliveStateListener> PostAliveStateChange;
        private List<UnitHealthChangeListener> PostHealthChange;
        private List<UnitMoveDistanceListener> PostMoveDistanceChange;
        private List<UnitChasingListener> PostChasingRemainDistance;
        private List<UnitKillEnemyListener> PostKillEnemy;
        private List<UnitDeathListener> PostUnitDeathState;
        private UnitSkillStateListener PostUnitSkillState;
        private List<UnitAfterFight> PostAfterUnitFight;

        private Soldier owner;

        public UnitBehaviorListener( Soldier owner )
        {
            this.owner = owner;

            PostFightOperation = new List<UnitFightListener>();
            PostHurt = new List<UnitHurtListener>();
            PostChaseStateChange = new List<UnitChaseStateListener>();
            PostIdleStateChange = new List<UnitIdleStateListener>();
            PostAliveStateChange = new List<UnitAliveStateListener>();
            PostHealthChange = new List<UnitHealthChangeListener>();
            PostMoveDistanceChange = new List<UnitMoveDistanceListener>();
            PostChasingRemainDistance = new List<UnitChasingListener>();
            PostAttackOrder = new List<UnitAttackOrderListener>();
            PostKillEnemy = new List<UnitKillEnemyListener>();
            PostUnitDeathState = new List<UnitDeathListener>();
            PostUnitSkillState = null;
            PostAfterUnitFight = new List<UnitAfterFight>();
        }

        #region Health Changed Event
        public bool PostHealthChangedEvent( int currentHp, int maxHp )
        {
            bool interruptResult = false;

            if ( PostHealthChange != null )
            {
                bool[] results = new bool[PostHealthChange.Count];
                for ( int i = 0; i < PostHealthChange.Count; i++ )
                {
                    results[i] = PostHealthChange[i]( currentHp, maxHp );
                }

                interruptResult = GetInvokeResult( results );
            }

            return interruptResult;
        }

        public void RegisterHealthChangeListener( UnitHealthChangeListener handler )
        {
            PostHealthChange.Add( handler );
        }

        public void RemoveHealthChangeListener( UnitHealthChangeListener handler )
        {
            if ( !PostHealthChange.Remove( handler ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Soldier, "Try to remove a unregister message!" );
            }
        }
        #endregion

        #region Alive State Changed Event
        public bool PostAliveStateChangedEvent( bool alive )
        {
            bool interruptResult = false;

            if ( PostAliveStateChange != null )
            {
                bool[] results = new bool[PostAliveStateChange.Count];
                for ( int i = 0; i < PostAliveStateChange.Count; i++ )
                {
                    results[i] = PostAliveStateChange[i]( alive );
                }

                interruptResult = GetInvokeResult( results );
            }

            return interruptResult;
        }

        public void RegisterAliveStateListener( UnitAliveStateListener handler )
        {
            PostAliveStateChange.Add( handler );
        }

        public void RemoveAliveListener( UnitAliveStateListener handler )
        {
            if ( !PostAliveStateChange.Remove( handler ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Soldier, "Try to remove a unregister message!" );
            }
        }
        #endregion

        #region Fight Event
        public bool PostFightEvent()
        {
            bool interruptResult = false;

            if ( PostFightOperation != null )
            {
                bool[] results = new bool[PostFightOperation.Count];
                for ( int i = 0; i < PostFightOperation.Count; i++ )
                {
                    results[i] = PostFightOperation[i]( owner.target );
                }

                interruptResult = GetInvokeResult( results );
            }

            return interruptResult;
        }

        public void RegisterFightListener( UnitFightListener handler )
        {
            PostFightOperation.Add( handler );
        }

        public void RemoveFightListener( UnitFightListener handler )
        {
            if ( !PostFightOperation.Remove( handler ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Soldier, "Try to remove a unregister message!" );
            }
        }
        #endregion

        #region Hurt Event
        public bool PostHurtEvent()
        {
            bool interruptResult = false;

            if ( PostHurt != null )
            {
                bool[] results = new bool[PostHurt.Count];
                for ( int i = 0; i < PostHurt.Count; i++ )
                {
                    results[i] = PostHurt[i]();
                }

                interruptResult = GetInvokeResult( results );
            }

            return interruptResult;
        }

        public void RegisterHurtListener( UnitHurtListener handler )
        {
            PostHurt.Add( handler );
        }

        public void RemoveHurtListener( UnitHurtListener handler )
        {
            if ( !PostHurt.Remove( handler ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Soldier, "Try to remove a unregister message!" );
            }
        }
        #endregion

        #region Move Distance Changed
        public bool PostMoveDistanceChanged( long deltaDistance )
        {
            bool interruptResult = false;

            if ( PostMoveDistanceChange != null )
            {
                bool[] results = new bool[PostMoveDistanceChange.Count];
                for ( int i = 0; i < PostMoveDistanceChange.Count; i++ )
                {
                    results[i] = PostMoveDistanceChange[i]( deltaDistance );
                }

                interruptResult = GetInvokeResult( results );
            }

            return interruptResult;
        }

        public void RegisterMoveDistanceListener( UnitMoveDistanceListener handler )
        {
            PostMoveDistanceChange.Add( handler );
        }

        public void RemoveMoveDistanceListener( UnitMoveDistanceListener handler )
        {
            if ( !PostMoveDistanceChange.Remove( handler ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Soldier, "Try to remove a unregister message!" );
            }
        }
        #endregion

        #region Chase State Changed
        public bool PostChaseStateChanged( bool enter )
        {
            bool interruptResult = false;

            if ( PostChaseStateChange != null )
            {
                bool[] results = new bool[PostChaseStateChange.Count];
                for ( int i = 0; i < PostChaseStateChange.Count; i++ )
                {
                    results[i] = PostChaseStateChange[i]( enter );
                }

                interruptResult = GetInvokeResult( results );
            }

            return interruptResult;
        }

        public void RegisterChaseListener( UnitChaseStateListener handler )
        {
            PostChaseStateChange.Add( handler );
        }

        public void RemoveChaseListener( UnitChaseStateListener handler )
        {
            if ( !PostChaseStateChange.Remove( handler ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Soldier, "Try to remove a unregister message!" );
            }
        }
        #endregion

        #region Attack Order Event
        public bool PostAttackOrderEvent()
        {
            bool interruptResult = false;

            if ( PostAttackOrder != null )
            {
                bool[] results = new bool[PostAttackOrder.Count];
                for ( int i = 0; i < PostAttackOrder.Count; i++ )
                {
                    results[i] = PostAttackOrder[i]();
                }

                interruptResult = GetInvokeResult( results );
            }

            return interruptResult;
        }

        public void RegisterAttackOrderListener( UnitAttackOrderListener handler )
        {
            PostAttackOrder.Add( handler );
        }

        public void RemoveAttackOrderListener( UnitAttackOrderListener handler )
        {
            if ( !PostAttackOrder.Remove( handler ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Soldier, "Try to remove a unregister message!" );
            }
        }
        #endregion

        #region Kill Enemy Event
        public bool PostKillEnemyEvent()
        {
            bool interruptResult = false;

            if ( PostKillEnemy != null )
            {
                bool[] results = new bool[PostKillEnemy.Count];
                for ( int i = 0; i < PostKillEnemy.Count; i++ )
                {
                    results[i] = PostKillEnemy[i]();
                }

                interruptResult = GetInvokeResult( results );
            }

            return interruptResult;
        }

        public void RegisterKillEnemyListener( UnitKillEnemyListener handler )
        {
            PostKillEnemy.Add( handler );
        }

        public void RemoveKillEnemyListener( UnitKillEnemyListener handler )
        {
            if ( !PostKillEnemy.Remove( handler ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Soldier, "Try to remove a unregister message!" );
            }
        }
        #endregion

        #region Unit Death Event
        public bool PostDeathEvent()
        {
            bool interruptResult = false;

            if ( PostUnitDeathState != null )
            {
                bool[] results = new bool[PostUnitDeathState.Count];
                for ( int i = 0; i < PostUnitDeathState.Count; i++ )
                {
                    results[i] = PostUnitDeathState[i]();
                }

                interruptResult = GetInvokeResult( results );
            }
            return interruptResult;
        }

        public void RegisterDeathStateListener( UnitDeathListener handler )
        {
            PostUnitDeathState.Add( handler);
        }

        public void RemoveDeathStateListener( UnitDeathListener handler )
        {
            if ( !PostUnitDeathState.Remove( handler ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Soldier, "Try to remove a unregister message!" );
            }
        }
        #endregion

        #region Chasing State Remain Distance Event
        public bool PostChasingStateChanged( long distanceSqr )
        {
            bool interruptResult = false;

            if ( PostChasingRemainDistance != null )
            {
                bool[] results = new bool[PostChasingRemainDistance.Count];
                for ( int i = 0; i < PostChasingRemainDistance.Count; i++ )
                {
                    results[i] = PostChasingRemainDistance[i]( distanceSqr );
                }

                interruptResult = GetInvokeResult( results );
            }

            return interruptResult;
        }

        public void RegisterChasingStateListener( UnitChasingListener handler )
        {
            PostChasingRemainDistance.Add( handler );
        }

        public void RemoveChasingStateListener( UnitChasingListener handler )
        {
            if ( !PostChasingRemainDistance.Remove( handler ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Soldier, "Try to remove a unregister message!" );
            }
        }
        #endregion

        #region Idle State Changed Event
        public void PostIdleStateChanged( bool enter )
        {
           // bool interruptResult = false;

            if ( PostChasingRemainDistance != null )
            {
                bool[] results = new bool[PostIdleStateChange.Count];
                for ( int i = 0; i < PostIdleStateChange.Count; i++ )
                {
                   /* results[i] =*/ PostIdleStateChange[i]( enter );
                }

                //interruptResult = GetInvokeResult( results );
            }
        }

        public void RegisterIdleStateListener( UnitIdleStateListener handler )
        {
            PostIdleStateChange.Add( handler );
        }

        public void RemoveIdleStateListener( UnitIdleStateListener handler )
        {
            if ( !PostIdleStateChange.Remove( handler ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Soldier, "Try to remove a unregister message!" );
            }
        }
        #endregion

        #region SkillState Enter Event
        public void PostSkillStateEnter()
        {
            if ( PostUnitSkillState != null )
            {
                PostUnitSkillState();
            }            
        }

        public void RegisterSkillStateListener( UnitSkillStateListener handler)
        {
            PostUnitSkillState += handler;
        }

        public void RemoveSkillStateListener( UnitSkillStateListener handler )
        {
            PostUnitSkillState -= handler;
        }
        #endregion

        #region AfterUnitFight Event
        public void PostUnitFightAfter()
        {
            bool interruptResult = false;

            if ( PostAfterUnitFight != null )
            {
                bool[] results = new bool[PostAfterUnitFight.Count];
                for ( int i = 0; i < PostAfterUnitFight.Count; i++ )
                {
                    PostAfterUnitFight[i]();
                }

                interruptResult = GetInvokeResult( results );
            }
        }

        public void RegisterAfterUnitFight( UnitAfterFight handler )
        {
            PostAfterUnitFight.Add( handler );
        }

        public void RemoveAfterUnitFight( UnitAfterFight handler )
        {
            if ( !PostAfterUnitFight.Remove( handler ) )
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Soldier, "Try to remove a unregister message!" );
            }
        }
        #endregion

        public bool GetInvokeResult( bool[] results )
        {
            if ( results == null )
            {
                return false;
            }

            for ( int i = 0; i < results.Length; i++ )
            {
                if ( results[i] )
                {
                    return true;
                }
            }

            return false;
        }

        public void Release()
        {
            PostFightOperation.Clear();
            PostHurt.Clear();
            PostChaseStateChange.Clear();
            PostIdleStateChange.Clear();
            PostAliveStateChange.Clear();
            PostHealthChange.Clear();
            PostMoveDistanceChange.Clear();
            PostChasingRemainDistance.Clear();
            PostKillEnemy.Clear();
            PostUnitDeathState.Clear();
            PostUnitSkillState= null;
            PostAfterUnitFight.Clear();
            PostAttackOrder.Clear();
        }
    }
}
