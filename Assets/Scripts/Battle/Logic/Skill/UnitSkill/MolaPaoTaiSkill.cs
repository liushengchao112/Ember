using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Constants;
using Utils;

using UnitSkillData = Data.UnitSkillsProto.UnitSkill;

namespace Logic
{
    public class MolaPaoTaiSkill : Skill
    {
        private enum State
        {
            None,
            InstallBattery,
            UsingBattery,
            PackUpBattery,
        }

        public List<AttributeEffect> buffList;

        private State state;

        private int timer;
        private int battryBulletMetaId;
        private int ownerOrginBulletMetaId;
        private long targetId;
        private LogicUnit target;

        public override void Initialize( long id, Soldier owner, UnitSkillData skillProto, int index )
        {
            base.Initialize( id, owner, skillProto, index );

            ownerOrginBulletMetaId = owner.projectileId;
            battryBulletMetaId = carrierId;

            buffList = new List<AttributeEffect>();
        }

        public override void Fire()
        {
            base.Fire();

            for ( int i = 0; i < attributeEffects.Count; i++ )
            {
                AttributeEffect buff = GenerateAttributeEffect( attributeEffects[i] );
                buff.Attach( owner, owner );
                buffList.Add( buff );
            }

            state = State.InstallBattery;
            timer = 0;

            // replace owner's projectile id to battery bullet
            owner.projectileId = battryBulletMetaId;
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            if ( state == State.InstallBattery )
            {
                if ( timer == 0 )
                {
                    RenderMessage rm = new RenderMessage();
                    rm.ownerId = owner.id;
                    rm.type = RenderMessage.Type.SoldierUseBattery;
                    rm.arguments.Add( "state", (int)state );

                    PostRenderMessage( rm );
                }
                else if ( timer >= GameConstants.UNIT_INSTALLBATTRY_DURATION )
                {
                    state = State.UsingBattery;
                    timer = 0;
                }

                timer += deltaTime;
            }
            else if ( state == State.UsingBattery )
            {
                if ( target != null && target.Alive() && targetId == target.id )
                {
                    // focus and fight with enemy
                    BattryFight( deltaTime );
                }
                else
                {
                    // search enemy...
                    LogicUnit enemy = owner.FindOpponentUnit();
                    if ( enemy != null )
                    {
                        BattryAttack( enemy );
                    }
                }
            }
            else if ( state == State.PackUpBattery )
            {
                PackUpBattery();
                ReleaseEnd();
            }
        }

        public override void Stop()
        {
            PackUpBattery();
            base.Stop();
        }

        public override bool DependOnSkillState()
        {
            return true;
        }

        private void PackUpBattery()
        {
            RenderMessage rm = new RenderMessage();
            rm.ownerId = owner.id;
            rm.type = RenderMessage.Type.SoldierUseBattery;
            rm.arguments.Add( "state", (int)state );

            PostRenderMessage( rm );

            // revert owner projectile
            owner.projectileId = ownerOrginBulletMetaId;

            // remove buff
            for ( int i = 0; i < buffList.Count; i++ )
            {
                buffList[i].Detach();
            }

            buffList.Clear();
            buffList = null;
        }

        public override void Reset()
        {
            base.Reset();

            target = null;
            targetId = 0;
        }

        #region Battry fight

        protected enum FightState
        {
            None = 0,
            StartSwingPoint,
            FrontSwing,
            HitPoint,
            BackSwing,
        }

        private bool inFightInterval = false;
        private int fightIntervalTimer;
        private int fightDurationTimer;
        private int fightInterval;
        private int hitTime;
        private int fightDuration;
        private FightState fightState;
        private FightState lastState;

        private void BattryAttack( LogicUnit unit )
        {
            target = unit;
            targetId = unit.id;
            owner.target = target;
            owner.targetId = targetId;

            inFightInterval = false;
            fightIntervalTimer = 0;
            fightDurationTimer = 0;
            fightInterval = 0;
            hitTime = 0;
            fightDuration = 0;
            fightState = FightState.StartSwingPoint;
            lastState = FightState.None;
        }

        private void BattryFight( int deltaTime )
        {
            if ( inFightInterval )
            {
                // Timing interval
                fightIntervalTimer += deltaTime;

                if ( fightIntervalTimer >= fightInterval )
                {
                    // Enter fight
                    fightState = FightState.StartSwingPoint;
                    fightIntervalTimer = 0;
                    fightInterval = 0;
                    inFightInterval = false;
                }
            }
            else
            {
                fightState = GetCurrentState( fightDurationTimer );
                fightDurationTimer += deltaTime;

                if ( fightState == FightState.StartSwingPoint )
                {
                    DebugUtils.Assert( target.id == targetId, string.Format( "Fight Status : the soldier {0}'s targetId = {1}, but its target's id = {2}!", owner.id, targetId, target.id ) );

                    if ( owner.stateListener.PostFightEvent() )
                    {
                        // Trigger skill, will interrupt fight.
                        return;
                    }
                    else
                    {
                        hitTime = owner.attackHitTime;
                        fightDuration = owner.attackDuration;

                        // Reset direction at attack begin.
                        FixVector3 direction = target.position - owner.position;
                        owner.direction = direction;

                        RenderMessage rm = new RenderMessage();
                        rm.type = RenderMessage.Type.SoldierBatteryFire;
                        rm.ownerId = owner.id;
                        rm.direction = direction.vector3;
                        rm.arguments.Add( "projectileMetaId", owner.projectileId );
                        rm.arguments.Add( "intervalRate", 1 );
                        owner.PostRenderMessage( rm );
                    }
                }
                else if ( fightState == FightState.HitPoint )
                {
                    bool isCrit = Formula.TriggerCritical( owner.GetRandomNumber(), owner.GetCriticalChance() );
                    owner.Fight( isCrit );
                    owner.stateListener.PostUnitFightAfter();
                }
                else if ( fightState == FightState.BackSwing )
                {
                    if ( fightDurationTimer >= fightDuration )
                    {
                        fightDurationTimer = 0;

                        fightInterval = owner.GetAttackInterval() - fightDuration;
                        inFightInterval = true;
                    }
                }
            }

            long distance = FixVector3.SqrDistance( target.position, owner.position );
            long attackDistance = owner.GetAttackArea();

            if ( distance >= attackDistance )
            {
                if ( ( target.position - owner.targetPosition ).magnitude > attackDistance )
                {
                    target = null;
                }
                else
                {
                    // continue fight
                }
            }
        }

        private FightState GetCurrentState( int f )
        {
            FightState result;

            if ( f == 0 )
            {
                result = FightState.StartSwingPoint;
            }
            else if ( f < hitTime )
            {
                result = FightState.FrontSwing;
            }
            else if ( f >= hitTime && lastState == FightState.FrontSwing )
            {
                result = FightState.HitPoint;
            }
            else
            {
                result = FightState.BackSwing;
            }

            lastState = result;
            return result;
        }
        #endregion
    }
}
