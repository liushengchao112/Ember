using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

using Utils;
using Constants;
using BattleAgent;

namespace Logic
{
    public class ZhiMingYiJiSkill : Skill
    {
        public enum State
        {
            Fire,
            Sprint,
            Attack
        }

        private State state;
        private AttributeEffect spriteAttributeEffect;
        private AttributeEffect attackAttributeEffect;
        private int hitTimer = 0;
        private int attackStateDurationTimer = 0;
        private bool isHit = false;

        public override void Initialize( long id, Soldier owner, UnitSkillsProto.UnitSkill skillProto, int index )
        {
            base.Initialize( id, owner, skillProto, index );

            handleOwnerMove = true;
        }

        public override void Fire()
        {
            base.Fire();

            state = State.Fire;

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SoldierWalk;
            rm.ownerId = owner.id;
            rm.direction = owner.direction.vector3;
            PostRenderMessage( rm );

            spriteAttributeEffect = GenerateAttributeEffect( attributeEffects[0] );
            spriteAttributeEffect.Attach( owner, owner );

            owner.FindChasePath( owner.target.position );
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            if ( state == State.Sprint )
            {
                SprintState( deltaTime );
            }
            else if ( state == State.Attack )
            {
                AttackState( deltaTime );
            }
        }

        private void SprintState( int deltaTime )
        {
            LogicUnit target = owner.target;
            FixVector3 position = owner.position;
            PathAgent agent = owner.pathAgent;

            if ( target != null && target.Alive() && target.id == owner.target.id )
            {
                if ( ( target.position - owner.targetPosition ).sqrMagnitude > GameConstants.BEGINCHASE_DISTANCE ) // 5f is a testing distance
                {
                    // After find path, Refresh target position
                    owner.targetPosition = target.position;
                    owner.FindChasePath( owner.target.position );
                    return;
                }

                if ( TargetWithInAttackArea() )
                {
                    state = State.Attack;
                }
                else
                {
                    if ( !owner.CurrentPathAlreadyFinished() )
                    {
                        owner.WaypointHandler();
                        FixVector3 d = owner.speed * deltaTime;
                        agent.Move( d );
                    }
                    else
                    {
                        // wait for chase path
                    }
                }
            }
            else
            {
                // Skill will be shut down when target death.
                Stop();
                owner.Idle();
            }
        }

        private void AttackState( int deltaTime )
        {
            attackStateDurationTimer += deltaTime;
            if ( attackStateDurationTimer <= skillActionDuration )
            {
                if ( hitTimer == 0 )
                {
                    spriteAttributeEffect.Detach();

                    RenderMessage rm = new RenderMessage();
                    rm.ownerId = owner.id;
                    rm.type = RenderMessage.Type.SoldierReleaseSkill;
                    rm.arguments.Add( "index", index );
                    rm.arguments.Add( "metaId", metaId );

                    PostRenderMessage( rm );
                }
                else if ( !isHit && hitTimer > skillActionHitTime )
                {
                    attackAttributeEffect = GenerateAttributeEffect( attributeEffects[1] );
                    attackAttributeEffect.Attach( owner, owner.target );

                    isHit = true;
                }
                else
                {
                    // back swing...
                }

                hitTimer += deltaTime;
            }
            else
            {
                // skill completely finished 
                ReleaseEnd();
            }
        }

        public override bool DependOnSkillState()
        {
            return true;
        }

        public bool TargetWithInAttackArea()
        {
            FixVector3 v = owner.target.position;
            long distance = FixVector3.SqrDistance( owner.target.position, owner.position );
            long attackDistance = owner.GetAttackArea();

            return distance < attackDistance;
        }

        public override void UnitFoundChasePath()
        {
            state = State.Sprint;
        }

        public override void UnitFinishedMove()
        {
            LogicUnit target = owner.target;

            if ( target != null && target.Alive() )
            {
                if ( TargetWithInAttackArea() )
                {
                    state = State.Attack;
                }
                else
                {
                    owner.FindChasePath( target.position );
                }
            }
            else
            {
                ReleaseEnd();
            }
        }

        public override void Reset()
        {
            base.Reset();

            state = State.Fire;
            spriteAttributeEffect = null;
            attackAttributeEffect = null;
            hitTimer = 0;
            attackStateDurationTimer = 0;
            isHit = false;
        }
    }
}

