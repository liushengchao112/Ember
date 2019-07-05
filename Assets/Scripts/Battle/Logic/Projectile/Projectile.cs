using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Utils;
using Data;
using Constants;

using ProjectileProto = Data.ProjectileProto.Projectile;

namespace Logic
{
    public enum ProjectileType
    {
        None,
        NormalProjectile,
        MagicExplodeSkill,
        TrapProjectile,
    }

    public enum ProjectileState
    {
        None,
        Flying,
        Hit,
        TimeOut,
        DistanceOut,
    }

    public class Projectile : LogicUnit
    {
        //private const float SPEED_FACTOR = 3f;
        private static int LIFE_TIME = 10000;
        protected long maxDistance = -1;

        protected AttributeEffectGenerator GenerateAttributeEffect;
        protected TrapGenerator GenerateTrap;

        protected WithinCircleAreaPredicate WithinCircleAreaPredicate;
        protected WithinFrontRectAreaPredicate WithinFrontRectAreaPredicate;

        // might be null
        protected FindOpponentSoldiersMethod FindOpponentSoldiers;
        protected FindNeutralUnitsMethod FindNeutralUnits;
        protected FindOpponentCrystalMethod FindOpponentCrystal;
        protected FindOpponentCrystalCarsMethod FindOpponentCrystalCars;
        protected FindOpponentDemolishersMethod FindOpponentDemolishers;
        protected FindOpponentBuildingsMethod FindOpponentBuildings;

        public ProjectileState state;
        public FixVector3 speed;
        public int speedFactor;
        public int hitEffectId;

        public LogicUnit owner;
        public LogicUnit target;
        public ProjectileType projectileType;

        private int lifeTimer;
        private bool changeDataWhenTargetDeath = false;
        protected AttackPropertyType hurtType;

        protected FixVector3 targetPosition;
        protected FixVector3 startPosition;
        protected FixVector3 destination;

        protected FixVector3 heightOffset = new FixVector3( 0, 1.5f, 0 );

        protected long targetId;

        protected bool isCritAttack;

        /// <summary>
        /// square of the real distance
        /// </summary>
        protected long flyingDistance;

        /// <summary>
        /// square of the real distance
        /// </summary>
        protected long targetDistance;

        // Projectile buff only support soldier to soldier for now!
        protected List<int> attributEffects = new List<int>();

        public void Initialize( LogicUnit owner, ProjectileProto proto, long id, FixVector3 position, LogicUnit target )
        {
            this.id = id;
            this.type = LogicUnitType.Projectile;
            this.owner = owner;
            this.mark = owner.mark;
            this.position = position;
            this.target = target;
            targetId = target.id;

            metaId = proto.ID;
            modelId = proto.Projectile_ResouceId;
            hitEffectId = proto.HitEffect_ResourceId;

            speedFactor = ConvertUtils.ToLogicInt( proto.SpeedFactor );

            damage = owner.damage;

            projectileType = (ProjectileType)proto.ProjectileType;
            startPosition = position;
            targetPosition = target.position;
            speed = ( targetPosition - position ).normalized * speedFactor;
            transform.position = position.vector3;
            state = ProjectileState.Flying;
            targetDistance = 0;
            destination = target.position;

            // Temp data
            hurtType = AttackPropertyType.PhysicalAttack;

            DebugUtils.Log( DebugUtils.Type.AI_Projectile, "the projectile " + id + "'s target is " + target.id + ", speed = " + position );
        }

        public void SetHurtInfo( int hurtValue, bool isCrit )
        {
            damage = hurtValue;
            isCritAttack = isCrit;
        }

        public override void LogicUpdate( int deltaTime )
        {
            if ( state == ProjectileState.Flying )
            {
                if ( target != null && target.Alive() )
                {
                    targetPosition = target.position;
                }
                else
                {
                    // If target has been death
                    // Projectile will fly to the target's body position
                    if ( !changeDataWhenTargetDeath )
                    {
                        DebugUtils.Log( DebugUtils.Type.AI_Projectile, "the projectile " + id + "'s target already death, now set Maxdistance = " + ( targetDistance + flyingDistance ) );

                        SetMaxDistance( targetDistance + flyingDistance );

                        changeDataWhenTargetDeath = !changeDataWhenTargetDeath;
                    }
                }

                // modify coordinates for each frame
                position += speed * deltaTime * GameConstants.LOGIC_FIXPOINT_PRECISION_FACTOR * GameConstants.LOGIC_FIXPOINT_PRECISION_FACTOR;
                transform.position = position.vector3 ;

                // recode total mileage of flight
                flyingDistance = FixVector3.SqrDistance( position, startPosition );

                RenderMessage rm = new RenderMessage();
                rm.type = RenderMessage.Type.SyncPosition;
                rm.ownerId = id;
                rm.arguments.Add( "type", (int)type );
                rm.position = ( position + heightOffset ).vector3;
                rm.direction = speed.vector3;
                PostRenderMessage( rm );

                //To be decide projectile should it be over
                targetDistance = FixVector3.SqrDistance( targetPosition, position );
                FlyingOperation( deltaTime );
            }
            else if ( state == ProjectileState.Hit )
            {
                DebugUtils.Log( DebugUtils.Type.AI_Projectile, "the projectile " + id + " hit the target " + target.id + " targetDistance " + targetDistance );
                Hit();
            }
        }

        protected virtual void FlyingOperation( int deltaTime )
        {
            if ( IsHit() )
            {
                state = ProjectileState.Hit;
            }
            else if ( IsTimeOut( deltaTime ) )
            {
                lifeTimer = 0;

                TimeOut();
            }
            else if ( IsDistanceOut() )
            {
                targetDistance = 0;

                DebugUtils.Log( DebugUtils.Type.AI_Projectile, string.Format( "Projectile {0} enter Distance state", id ) );

                DistanceOut();
            }
        }

        protected virtual bool IsHit()
        {
            if ( targetDistance < GameConstants.HIT_DISTANCE && target != null )
            {
                return true;
            }

            return false;
        }

        protected virtual bool IsTimeOut( int deltaTime )
        {
            lifeTimer += deltaTime;

            if ( LIFE_TIME <= lifeTimer )
            {
                return true;
            }

            return false;
        }

        protected bool IsDistanceOut()
        {
            if ( maxDistance <= 0 )
            {
                return false;
            }

            if ( flyingDistance >= maxDistance )
            {
                return true;
            }

            return false;
        }

        public virtual void Hit()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Projectile, string.Format( "Projectile {0} enter hit state", id ) );

            RenderMessage message = new RenderMessage();
            message.type = RenderMessage.Type.ProjectileHit;
            message.ownerId = id;
            PostRenderMessage( message );

            PostDestroy( this );
        }

        public void TimeOut()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Projectile, "the projectile " + id + " enter time out state " );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.ProjectileHit;
            rm.ownerId = id;
            PostRenderMessage( rm );

            PostDestroy( this );
        }

        public void DistanceOut()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Projectile, "the projectile " + id + " enter distance out state " );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.ProjectileHit;
            rm.ownerId = id;
            PostRenderMessage( rm );

            PostDestroy( this );
        }

        // When projectile hit a target, will attach all debuff to target.
        public void FillingAttributEffect( int b )
        {
            attributEffects.Add( b );
        }

        // need to input the square of real value  
        public void SetMaxDistance( long v )
        {
            maxDistance = v;
        }

        public override void AddCoin( int coin )
        {
            owner.AddCoin( coin );
        }

        public override void Hurt( int hurtValue, AttackPropertyType type, bool isCrit, LogicUnit injurer )
        {
            DebugUtils.Assert( false, "Hurt() in Projectile is not implemented!" );
        }

        public override bool Alive()
        {
            DebugUtils.Assert( false, "Alive() in Projectile is not implemented!" );
            return false;
        }

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            owner.OnKillEnemy( emberReward, owner, dead );
        }

        public override void Reset()
        {
            id = -1;
            state = ProjectileState.None;
            speed = FixVector3.zero;
            mark = ForceMark.NoneForce;
            position = FixVector3.zero;
            owner = null;
            target = null;
            targetId= 0;

            lifeTimer = 0;
            changeDataWhenTargetDeath = false;
            maxDistance = -1;
            attributEffects.Clear();

            PostRenderMessage = null;
            PostDestroy = null;
            GetRandomNumber = null;
        }

        public void RegisterFindOpponentSoldiers( FindOpponentSoldiersMethod method )
        {
            FindOpponentSoldiers = method;
        }

        public void RegisterFindOpponentCrystal( FindOpponentCrystalMethod method )
        {
            FindOpponentCrystal = method;
        }

        public void RegisterFindOpponentBuildings( FindOpponentBuildingsMethod method )
        {
            FindOpponentBuildings = method;
        }

        public void RegisterFindOpponentCrystalCars( FindOpponentCrystalCarsMethod method )
        {
            FindOpponentCrystalCars = method;
        }

        public void RegisterFindOpponentDemolishers( FindOpponentDemolishersMethod method )
        {
            FindOpponentDemolishers = method;
        }

        public void RegisterWithinFrontRectangleAreaPredicate( WithinFrontRectAreaPredicate method )
        {
            WithinFrontRectAreaPredicate = method;
        }

        public void RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate method )
        {
            WithinCircleAreaPredicate = method;
        }

        public void RegisterGenerateAttributeEffect( AttributeEffectGenerator method )
        {
            GenerateAttributeEffect = method;
        }

        public void RegisterFindNeutralUnits( FindNeutralUnitsMethod method )
        {
            FindNeutralUnits = method;
        }

        public void ReisterTrapGenerator( TrapGenerator method )
        {
            GenerateTrap = method;
        }
    }
}

