using UnityEngine;
using System.Collections;
using System;

using Data;
using Utils;
using Constants;

public enum TowerLocateType
{
    None,
    Top,
    Bottom
}

namespace Logic
{
    public class Tower : LogicUnit
    {
        private FindOpponentSoldierMethod FindOpponentSoldier;
        private FindOpponentDemolisherMethod FindOpponentDemolisher;
        private FindOpponentCrystalCarMethod FindOpponentCrystalCar;
        private FindOpponentSoldiersMethod FindOpponentSoldiers;
        private WithinCircleAreaPredicate WithinCircleAreaPredicate;
        private WithinFrontRectAreaPredicate WithinFrontRectAreaPredicate;

        private ProjectileGenerator GenerateProjectile;

        public BuildingState state;

        private Town town;

        public LogicUnit target;
        public long targetId;
		public int towerTypeID;
		//Show effect value
		public int towerBuildTime;
		private int towerBuildingTimer;
		private FixVector3 towerFirePos;

        //Timer...
        private int fightTimer;
        private int healthRecoverInterval;
        private int healthRecoverTimer;

        private int attackInterval = 0;
        private TowerLocateType locateType = TowerLocateType.None;
        private int attackArea;
        private int rangeDmgBase;
        private int rangeDmgVar;

		private int recycleValue;
		private int physicalResistance;
		private int magicResistance;
		private int buildCost;
        private int healthRecover;

        private int projectileId;
        private long realAttakDistance;

		public void Initialize( long id, Town town, FixVector3 pos, int towerID = 0, TowerLocateType locate = TowerLocateType.None )
		{
			this.id = id;
			this.town = town;
			this.type = LogicUnitType.Tower;
            mark = town.mark;
			transform.position = pos.vector3;
			position = (FixVector3)transform.position;
			state = BuildingState.BUILD;
			towerFirePos = new FixVector3( position.x, position.y + 3.3f, position.z );

            InitializedData( towerID );

            this.gameObject.SetActive( true );
		}

        private void InitializedData( int towerMetaId )
        {
            //There is player builded tower init value
            TowerProto.Tower proto = DataManager.GetInstance().towerProtoData.Find( p => p.ID == towerMetaId );

            metaId = towerMetaId;
            iconId = proto.IconID;
            modelId = proto.ModelID;
            modelRadius = ConvertUtils.ToLogicInt( proto.ModelRadius );
            physicalResistance = (int)proto.PhysicalResistance;
            attackArea = ConvertUtils.ToLogicInt( proto.AttackRange );
            attackInterval = ConvertUtils.ToLogicInt( proto.AttackInterval );
            buildCost = proto.DeploymentCost;
            maxHp = proto.Health;
            rangeDmgBase = proto.RangedDmgBase;
            rangeDmgVar = proto.RangedDmgVar;
            damage = ( int )rangeDmgBase;
            magicResistance = (int)proto.MagicResistance;
            recycleValue = (int)proto.RecycleValue;
            towerTypeID = proto.ID;
			killReward = proto.DestoryReward;
            projectileId = proto.ProjectileID;
			towerBuildTime = ConvertUtils.ToLogicInt( proto.BuildingTime );
			attackInterval = ConvertUtils.ToLogicInt( proto.AttackInterval );
            realAttakDistance = attackArea + modelRadius;

            healthRecover = 1; // Temp data,Need designer add table string;

            healthRecoverInterval = GameConstants.HP_RECOVERY_INTERVAL_MILLISECOND;//Use gameConstants.
            hp = maxHp;
        }

        public override void LogicUpdate( int deltaTime )
        {
			if( state == BuildingState.BUILD )
			{
				BuildPosture( deltaTime );
			}
			else if( state == BuildingState.DESTROY )
			{
				//Maybe need add some show effect logic
			}
			else
			{
				GuardPosture( deltaTime );
			}
        }

		//When tower state is build, tower can't attack enemy but enemy can damage tower.
		private void BuildPosture( int deltaTime )
		{
			towerBuildingTimer += deltaTime;

			if( towerBuildingTimer >= towerBuildTime )
			{
				state = BuildingState.IDLE;
				towerBuildingTimer = 0;
			}
		}

		private void GuardPosture( int deltaTime )
		{
			if( state == BuildingState.IDLE )
			{
				FindOpponent();
			}
			else if( state == BuildingState.ATTACK )
			{
				// is searching need to ignore the cloaking
				if( target.Alive() && target.id == targetId )
				{
                    long distance = FixVector3.SqrDistance( target.position, position );

                    if ( distance > realAttakDistance )
					{
						state = BuildingState.IDLE;
					}
					else
					{
						fightTimer += deltaTime;
						if( fightTimer > attackInterval )
						{
							fightTimer = 0;
							Fire();
						}
					}
				}
				else
				{
					Idle();
				}
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.AI_Tower, "the tower " + id + " is in strange state " + state );
			}

			//HealthAutoRecovery( deltaTime ); Designer don't want deploy type building have auto recovery.Locked this.
		}

        private void Idle()
        {
            state = BuildingState.IDLE;
        }

        private void Attack( LogicUnit t )
        {
            DebugUtils.Log( DebugUtils.Type.AI_Tower, "the tower " + id + " begins to attack soldier " + t.id );

            targetId = t.id;
            target = t;
            fightTimer = 0;
            state = BuildingState.ATTACK;
        }

        private void Fire()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Tower, "the tower " + id + " begins to fire to target " + target.id );

            damage = GetDamageValue();

			Projectile projectile = GenerateProjectile( this, projectileId, towerFirePos, target );
            projectile.RegisterRenderMessageHandler( PostRenderMessage );
            projectile.RegisterDestroyHandler( PostDestroy );
            projectile.RegisterRandomMethod( GetRandomNumber );
            projectile.RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate );
            projectile.RegisterWithinFrontRectangleAreaPredicate( WithinFrontRectAreaPredicate );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnProjectile;
            rm.ownerId = projectile.id;
            rm.position = projectile.position.vector3;
            rm.direction = projectile.speed.vector3;
            rm.arguments.Add( "mark", projectile.mark );
            rm.arguments.Add( "metaId", projectile.metaId );
            rm.arguments.Add( "holderType", (int)projectile.owner.type );
            rm.arguments.Add( "holderId", projectile.owner.id );
            PostRenderMessage( rm );
        }

        private void FindOpponent()
        {
			
			LogicUnit t = FindOpponentSoldier( mark, position, realAttakDistance );

            if ( t == null )
            {
				t = FindOpponentCrystalCar( mark, position, realAttakDistance );
            }

            if ( t == null )
            {
				t = FindOpponentDemolisher( mark, position, realAttakDistance );
            }

            if ( t != null )
            {
                long distance = FixVector3.SqrDistance( t.position, position );
                long attackDistance = realAttakDistance + t.modelRadius;

                DebugUtils.LogWarning( DebugUtils.Type.AI_Tower, string.Format( "tower {0} finds the target {1} of type {2}, distance = {3}, attackDistance = {4}.", id, t.id, t.type, distance, attackDistance ) );

                if ( distance < attackDistance )
                {
                    Attack( t );
                }
            }
        }

        private void HealthAutoRecovery( int deltaTime )
        {
            if ( Alive() )
            {
                healthRecoverTimer += deltaTime;
				if ( healthRecoverTimer > healthRecoverInterval && hp < maxHp )
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

        public override void AddCoin( int coin )
        {
            town.AddCoin( coin );
        }

        public override bool Alive()
        {
            return state != BuildingState.DESTROY;
        }

        public override void Hurt( int hurtValue, AttackPropertyType hurtType, bool isCrit, LogicUnit injurer )
        {
            if( Alive() )
            {
                if ( hurtType == AttackPropertyType.PhysicalAttack )
                {
                    int v = GetTowerActualHurtValue( hurtType, hurtValue );
                    hp -= v;

                    RenderMessage message = new RenderMessage();
                    message.type = RenderMessage.Type.TowerHurt;
                    message.ownerId = id;
                    message.position = position.vector3;
					message.arguments.Add( "value", v );
                    PostRenderMessage( message );

                    if ( hp <= 0 )
                    {
                        injurer.OnKillEnemy( killReward, injurer, this );
                        Destroy();
                    }
                }
            }
        }

		private int GetTowerActualHurtValue( AttackPropertyType type, int hurtValue )
		{
            int value = 0;

			if ( type == AttackPropertyType.PhysicalAttack )
			{
				value = Formula.ActuallyPhysicalDamage( hurtValue, physicalResistance );
			}
			//else if ( type == HurtType.MagicAttack )
			//{
			//	value = (int)( hurtValue / ( magicResistance / 100 + 1 ) );
			//}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.AI_Tower, string.Format( "Can't handle this hurtType {0} now!", type ) );
			}

			return value;
		}

        public int GetDamageValue()
        {
            return Formula.GetAttackFloatingValue( rangeDmgBase, GetRandomNumber(), GetRandomNumber( rangeDmgVar ) );
        }

        public void Destroy()
        {
            DebugUtils.Assert( state != BuildingState.DESTROY );

            state = BuildingState.DESTROY;
            RenderMessage renderMessage = new RenderMessage();
            renderMessage.type = RenderMessage.Type.TowerDestroy;
            renderMessage.ownerId = id;
            renderMessage.arguments.Add( "mark", mark );
            renderMessage.arguments.Add( "locate", (int)locateType );

            PostRenderMessage( renderMessage );

            PostDestroy( this );
        }

		public void RecylingTower()
		{
			state = BuildingState.DESTROY;
			PostDestroy( this );
		}

		public int GetBuildCost()
		{
			return buildCost;
		}

		public int GetRecycleValue()
		{
			return recycleValue;
		}

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            town.OnKillEnemy( emberReward, this, dead );
        }

        public override void Reset()
        {
            this.id = -1;
			this.towerTypeID = -1;
			mark = ForceMark.NoneForce;
			transform.position = Vector3.zero;
			position = FixVector3.zero;
			state = BuildingState.BUILD;
			buildCost = 0;
			maxHp = 0;
			attackArea = 0;
			attackInterval = 0;
			rangeDmgVar = 0;
			rangeDmgBase = 0;
			damage = 0;
			physicalResistance = 0;
			magicResistance = 0;
			recycleValue = 0;
			projectileId = 0;

			towerBuildTime = 0;
			towerBuildingTimer = 0;

			PostRenderMessage = null;
			PostDestroy = null;
			GetRandomNumber = null;
			PostBattleMessage = null;

			FindOpponentSoldier = null;
			FindOpponentDemolisher = null;
			FindOpponentCrystalCar = null;
			FindOpponentSoldiers = null;
			WithinCircleAreaPredicate = null;
			WithinFrontRectAreaPredicate = null;
        }

        public void RegisterProjectileGenerator( ProjectileGenerator generator )
        {
            GenerateProjectile = generator;
        }

        public void RegisterFindOpponentSoldier( FindOpponentSoldierMethod method )
        {
            FindOpponentSoldier = method;
        }

        public void RegisterFindOpponentDemolisher( FindOpponentDemolisherMethod method )
        {
            FindOpponentDemolisher = method;
        }

        public void RegisterFindOpponentCrystalCar( FindOpponentCrystalCarMethod method )
        {
            FindOpponentCrystalCar = method;
        }

        public void RegisterFindOpponentSoldiers( FindOpponentSoldiersMethod method )
        {
            FindOpponentSoldiers = method;
        }

        public void RegisterWithinFrontRectAreaPredicate( WithinFrontRectAreaPredicate method )
        {
            WithinFrontRectAreaPredicate = method;
        }

        public void RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate method )
        {
            WithinCircleAreaPredicate = method;
        }
    }
}
