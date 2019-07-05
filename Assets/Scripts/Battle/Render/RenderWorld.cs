/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: RenderWorld.cs
// description: 
// 
// created time：09/27/2016
//
//----------------------------------------------------------------*/

using UnityEngine;
using System;
using System.Collections.Generic;

using Data;
using Utils;
using Constants;
using Resource;
using Map;

namespace Render
{
    public class RenderWorld
    {
        public Dictionary<long, TownRender> townRenders;
		public Dictionary<long, InstituteRender> instituteRenders;
        public Dictionary<long, TowerRender> towerRenders;
        public Dictionary<long, SoldierRender> soldierRenders;
        public Dictionary<long, ProjectileRender> projectileRenders;
        public Dictionary<long, CrystalRender> crystalRenders;
        public Dictionary<long, CrystalCarRender> crystalCarRenders;
        public Dictionary<long, DemolisherRender> demolisherRenders;
        public Dictionary<long, NpcRender> npcRenders;
        public Dictionary<long, PowerUpRender> powerUpRenders;
        public Dictionary<long, SkillRender> skillRenders;
        public Dictionary<long, SummonedUnitRender> summonRenders;
		public Dictionary<long, TrapRender> trapRenders;
        public Dictionary<long, IdolRender> idolRenders;
        public Dictionary<long, IdolGuardRender> idolGuardRenders;

        Dictionary<MatchSide, List<TowerRender>> sideTowers;
        Dictionary<MatchSide, List<TownRender>> sideTowns;

        private Transform soldierRenderRoot;
        private BattlePoolGroop soldierRenderPoolGroup;

        private Transform projectileRenderRoot;
        private BattlePoolGroop projectileRenderPoolGroup;

        private Transform powerUpRenderRoot;
        private BattlePool powerUpRenderPool;

        private Transform towerRenderRoot;
		private BattlePoolGroop towerRenderPoolGroup;

        private Transform instituteRenderRoot;
		private BattlePoolGroop instituteRenderPoolGroup;

        private Transform demolisherRenderRoot;
        private BattlePool demolisherRenderPool;

        private Transform townRenderRoot;

        private Transform crystalRenderRoot;
        private BattlePool crystalRenderPool;

        private Transform crystalCarRenderRoot;
        private BattlePool crystalCarRenderPool;

        private Transform npcRenderRoot;
        private BattlePool npcRenderPool;

        private Transform idolRenderRoot;
        private BattlePool idolRenderPool;

        private Transform idolGuradRenderRoot;
        private BattlePool idolGuradRenderPool;

        private Transform skillRenderRoot;
        private BattlePool skillRenderPool;

        private Transform summonRenderRoot;
        private BattlePool summonRenderPool;

        private Transform trapRenderRoot;
        private BattlePool trapRenderPool;

        [HideInInspector]
        public Transform renderRoot;

        private PathRender pathRender;

        private BattleEffectManager effectManager;

        private CullingGroupManager cullingGroup;

        private MapDataPVE mapData;
		private MapData1V1 mapData1V1;
        private MapData2V2 mapData2V2;

        public RenderWorld()
        {
            sideTowns = new Dictionary<MatchSide, List<TownRender>>();
            sideTowns.Add( MatchSide.Red, new List<TownRender>() );
            sideTowns.Add( MatchSide.Blue, new List<TownRender>() );
			sideTowers = new Dictionary<MatchSide, List<TowerRender>>();

            townRenders = new Dictionary<long, TownRender>();

            towerRenders = new Dictionary<long, TowerRender>();
			towerRenderPoolGroup = new BattlePoolGroop();

            instituteRenders = new Dictionary<long, InstituteRender>();
			instituteRenderPoolGroup = new BattlePoolGroop();

            soldierRenders = new Dictionary<long, SoldierRender>();
            soldierRenderPoolGroup = new BattlePoolGroop();

            projectileRenders = new Dictionary<long, ProjectileRender>();
            projectileRenderPoolGroup = new BattlePoolGroop();

            crystalRenders = new Dictionary<long, CrystalRender>();
            crystalRenderPool = new BattlePool();

            crystalCarRenders = new Dictionary<long, CrystalCarRender>();
            crystalCarRenderPool = new BattlePool();

            demolisherRenders = new Dictionary<long, DemolisherRender>();
            demolisherRenderPool = new BattlePool();

            npcRenders = new Dictionary<long, NpcRender>();
            npcRenderPool = new BattlePool();

            powerUpRenders = new Dictionary<long, PowerUpRender>();
            powerUpRenderPool = new BattlePool( 4 );

            skillRenders = new Dictionary<long, SkillRender>();
            skillRenderPool = new BattlePool();

            summonRenders = new Dictionary<long, SummonedUnitRender>();
            summonRenderPool = new BattlePool();

            trapRenders = new Dictionary<long , TrapRender>();
            trapRenderPool = new BattlePool();

            idolRenders = new Dictionary<long, IdolRender>();
            idolRenderPool = new BattlePool();

            idolGuardRenders = new Dictionary<long, IdolGuardRender>();
            idolGuradRenderPool = new BattlePool();

            renderRoot = new GameObject("RenderRoot").transform;
            soldierRenderRoot = new GameObject( "RenderSoldier" ).transform;
            soldierRenderRoot.SetParent( renderRoot );
            projectileRenderRoot = new GameObject( "RenderProjectile" ).transform;
            projectileRenderRoot.SetParent( renderRoot );
            towerRenderRoot = new GameObject( "RenderTower" ).transform;
            towerRenderRoot.SetParent( renderRoot );
            townRenderRoot = new GameObject( "RenderTown" ).transform;
            townRenderRoot.SetParent( renderRoot );
            crystalRenderRoot = new GameObject( "RenderCrystal" ).transform;
            crystalRenderRoot.SetParent( renderRoot );
            crystalCarRenderRoot = new GameObject( "RenderCrystalCar" ).transform;
            crystalCarRenderRoot.SetParent( renderRoot );
            powerUpRenderRoot = new GameObject( "RenderPowerUp" ).transform;
            powerUpRenderRoot.SetParent( renderRoot );
            instituteRenderRoot = new GameObject( "RenderInstitute" ).transform;
            instituteRenderRoot.SetParent( renderRoot );
            demolisherRenderRoot = new GameObject( "RenderDemolisher" ).transform;
            demolisherRenderRoot.SetParent( renderRoot );
            npcRenderRoot = new GameObject( "RenderNPC" ).transform;
            npcRenderRoot.SetParent( renderRoot );
            skillRenderRoot = new GameObject( "RenderSkill" ).transform;
            skillRenderRoot.SetParent( renderRoot );
            summonRenderRoot = new GameObject( "RenderSummon" ).transform;
            summonRenderRoot.SetParent( renderRoot );
            trapRenderRoot = new GameObject( "trapRenderRoot" ).transform;
            trapRenderRoot.SetParent( renderRoot );
            idolRenderRoot = npcRenderRoot;
            idolGuradRenderRoot = npcRenderRoot;

            pathRender = new GameObject( "PathRender" ).gameObject.AddComponent<PathRender>();
            effectManager = new GameObject("BattleEffects").AddComponent<BattleEffectManager>();

            cullingGroup = new CullingGroupManager();
        }

        public void Release()
        {
            soldierRenderPoolGroup.Release();
            skillRenderPool.Release();
            crystalRenderPool.Release();
            npcRenderPool.Release();
            towerRenderPoolGroup.Release();
            instituteRenderPoolGroup.Release();
            summonRenderPool.Release();
            projectileRenderPoolGroup.Release();
            idolGuradRenderPool.Release();
            idolRenderPool.Release();

            cullingGroup.Release();
        }

        public void RenderStart( MapData2V2 mapData )
        {
            this.mapData2V2 = mapData;
            pathRender.RenderStart();
        }

		public void RenderStart( MapData1V1 mapData )
		{
			this.mapData1V1 = mapData;
			pathRender.RenderStart();
		}

        public void RenderStart( MapDataPVE mapData )
        {
            this.mapData = mapData;
            pathRender.RenderStart();
        }

        public void HandleRenderMessage( RenderMessage rm )
        {
            switch( rm.type )
            {
				case RenderMessage.Type.SoldierBornComplete:
				{
					SoldierRender soldierRender = soldierRenders[rm.ownerId];
					soldierRender.gameObject.SetActive( true );
					break;
				}
                case RenderMessage.Type.SoldierWalk:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    soldierRender.Walk( rm.direction );
                    break;
                }
                case RenderMessage.Type.SoldierAttack:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    bool isCrit = Convert.ToBoolean( rm.arguments["isCrit"] );
                    float rate = Convert.ToSingle( rm.arguments["intervalRate"] );
                    soldierRender.Attack( rm.direction, rate , isCrit );
                    break;
                }
                case RenderMessage.Type.SoldierSpawnProjectile:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    int projectileMetaId = Convert.ToInt32( rm.arguments["projectileMetaId"] );
                    float rate = Convert.ToSingle( rm.arguments["intervalRate"] );
                    soldierRender.SpawnProjecile( rm.direction, projectileMetaId, rate );
                    break;
                }
                case RenderMessage.Type.SoldierIdle:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    soldierRender.Idle( rm.direction );
                    break;
                }
                case RenderMessage.Type.SoldierDeath:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    int mark = Convert.ToInt32(rm.arguments["mark"]);
                    soldierRender.Dying( rm.direction );
                    pathRender.UnitDead( rm.ownerId, 1 );
                    cullingGroup.RemoveUnitRender( soldierRender );
                    soldierRenders.Remove( rm.ownerId );
                    MessageDispatcher.PostMessage( MessageType.SoldierDeath, mark, rm.ownerId );
                    break;
                }
                case RenderMessage.Type.SoldierHurt:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    int value = Convert.ToInt32( rm.arguments["value"] );
                    int mark = Convert.ToInt32( rm.arguments["mark"] );
                    soldierRender.Hurt( value, false );
                    MessageDispatcher.PostMessage( MessageType.SoldierHurt, mark, rm.ownerId, value );
                    break;
                }
                case RenderMessage.Type.SoldierHeal:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    int value = Convert.ToInt32( rm.arguments["value"] );
                    int mark = Convert.ToInt32( rm.arguments["mark"] );
                    soldierRender.Heal( value );
                    MessageDispatcher.PostMessage( MessageType.SoldierHeal, soldierRender.mark, rm.ownerId, value );
                    break;
                }
                case RenderMessage.Type.SoldierSelected:
                {                 
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    bool value = Convert.ToBoolean( rm.arguments["value"] );
                    soldierRender.SetSelection( value );
                    MessageDispatcher.PostMessage( MessageType.BattleUIOperationFeedBack, BattleUIOperationType.SelectedUnitResult, rm.ownerId, value );
                    break;
                }
                case RenderMessage.Type.SoldierPickUpPowerUp:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    PowerUpType powerUpType = (PowerUpType)Convert.ToInt32( rm.arguments["powerUpType"] );
                    int activeEffectPath = Convert.ToInt32( rm.arguments["activeEffectPath"] );
                    int persistantEffectPath = Convert.ToInt32( rm.arguments["persistantEffectPath"] );
                    int deactivateEffectPath = Convert.ToInt32( rm.arguments["deactivateEffectPath"] );
                    int powerUpLifeTime = Convert.ToInt32( rm.arguments["LifeTime"] );

                    soldierRender.PickUpPowerUp( powerUpType, activeEffectPath, persistantEffectPath, deactivateEffectPath, powerUpLifeTime );
                    break;
                }
                case RenderMessage.Type.SoldierVisibleChange:
                {
                    if ( soldierRenders.ContainsKey( rm.ownerId ) )
                    {
                        SoldierRender soldierRender = soldierRenders[rm.ownerId];
                        bool v = Convert.ToBoolean( rm.arguments["value"] );
                        ForceMark mark = (ForceMark)( rm.arguments["mark"] );
                        soldierRender.SetVisible( v );

                        MessageDispatcher.PostMessage( MessageType.SoldierVisibleChange, rm.ownerId, mark );
                    }
                    else
                    {
                        DebugUtils.LogError( DebugUtils.Type.AI_Soldier, string.Format( "Can't find soldier render {0} in render world", rm.ownerId ) );
                    }

                    break;
                }
                case RenderMessage.Type.SoldierStuned:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    bool value = Convert.ToBoolean( rm.arguments["value"] );
                    soldierRender.Stuned( value );
                    break;
                }
                case RenderMessage.Type.SoldierReleaseSkill:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    int index = Convert.ToInt32( rm.arguments["index"] );
                    int metaId = Convert.ToInt32( rm.arguments["metaId"] );

                    soldierRender.ReleaseSkill( index, metaId, rm.position );
                    break;
                }
                case RenderMessage.Type.SoldierHitTarget:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    soldierRender.Hit();
                    break;
                }
                case RenderMessage.Type.SoldierDash:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    int state = Convert.ToInt32(rm.arguments["state"]);
                    soldierRender.Dash( state, rm.direction );
                    break;
                }
                case RenderMessage.Type.SoldierUseBattery:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    int state = Convert.ToInt32( rm.arguments["state"] );
                    soldierRender.UseBattery( state );
                    break;
                }
                case RenderMessage.Type.SoldierBatteryFire:
                {
                    SoldierRender soldierRender = soldierRenders[rm.ownerId];
                    int projectileId = Convert.ToInt32( rm.arguments["projectileMetaId"] );
                    soldierRender.BatteryFire( rm.direction, projectileId );
                    break;
                }
                case RenderMessage.Type.SyncPosition:
                {
                    UnitRenderType type = (UnitRenderType)Convert.ToInt32( rm.arguments["type"] );

                    bool rotatingImmediately = false;
                    if ( rm.arguments.ContainsKey( "RotatingImmediately" ) )
                    {
                        rotatingImmediately = Convert.ToBoolean( rm.arguments["RotatingImmediately"] );
                    }

                    float deltaDistance = 0;
                    if ( rm.arguments.ContainsKey( "move" ) )
                    {
                        deltaDistance = Convert.ToSingle( rm.arguments["move"] );
                    }

                    SyncPosition( (UnitRenderType)type, rm.ownerId, rm.position, rotatingImmediately,rm.direction, deltaDistance );
                    break;
                }
                case RenderMessage.Type.SyncHP:
                {
                    UnitRenderType type = (UnitRenderType)Convert.ToInt32( rm.arguments["type"] );
                    ForceMark mark =(ForceMark)( rm.arguments["mark"] );
                    int hp = Convert.ToInt32( rm.arguments["hp"] );
                    int maxHp = Convert.ToInt32( rm.arguments["maxHp"] );
                    SyncHp( type, rm.ownerId, mark, hp, maxHp );
                    break;
                }
                case RenderMessage.Type.SyncUnitPathPoint:
                {
                    int type = Convert.ToInt32( rm.arguments["type"] );
                    int mark = Convert.ToInt32( rm.arguments["mark"] );
                    List<Vector3> path = (List<Vector3>)rm.arguments["path"];
                    pathRender.SyncPath( mark, rm.ownerId, type, path );
                    break;
                }
                case RenderMessage.Type.DrawPathPoint:
                {
                    int type = Convert.ToInt32( rm.arguments["type"] );
                    pathRender.AddUnitPathPoint( rm.ownerId, type, rm.position );
                    break;
                }
                case RenderMessage.Type.ClearPath:
                {
                    int type = Convert.ToInt32( rm.arguments["type"] );
                    pathRender.ClearPath( rm.ownerId, type );
                    break;
                }
                case RenderMessage.Type.CritHurtUnit:
                {
                    UnitRenderType type = (UnitRenderType)rm.arguments["unitType"];
                    int hurtValue = Convert.ToInt32( rm.arguments["value"] );
                    UnitRender unitRender = GetUnitRender(rm.ownerId, type);

                    unitRender.Hurt( hurtValue, true );
                    break;
                }
                //Locked building mode drag deployment code.Dwayne.
                /*case RenderMessage.Type.DeployBuildingAreaOpen:
				{
                    ForceMark mark = ( ForceMark )rm.arguments[ "mark" ];
					long towerID = ( long )rm.arguments[ "TowerID" ];
					List<TowerRender> towersList;

                    MatchSide side = GetSideFromMark( mark );
                    sideTowers.TryGetValue( side, out towersList );

					if( towersList != null && towersList.Count > 0 )
					{
						for( int i = 0; i < towersList.Count; i++ )
						{
							if( towersList[i].id == towerID )
							{
								towersList[ i ].ShowDeployBuildingArea();
							}
						}
					}

					DebugUtils.Log(  DebugUtils.Type.Building, "Show deploy building area! ForceMark is :"+ mark );

					break;
				}
				case RenderMessage.Type.DeployBuildingAreaClose:
				{
                    ForceMark mark = (ForceMark)rm.arguments[ "mark" ];
					long towerID = ( long )rm.arguments[ "TowerID" ];
					List<TowerRender> towersList;

                    MatchSide side = GetSideFromMark( mark );
                    sideTowers.TryGetValue( side, out towersList );

					if( towersList != null && towersList.Count > 0 )
					{
						for( int i = 0; i < towersList.Count; i++ )
						{
							if( towersList[i].id == towerID )
							{
								towersList[ i ].CloseDeployBuildingArea();
							}
						}
					}

					DebugUtils.Log(  DebugUtils.Type.Building, "Close deploy building area! ForceMark is :" + mark );

					break;
				}*/
                case RenderMessage.Type.TapTower:
                    {
                        int cost = Convert.ToInt32( rm.arguments["RecylingMoney"] );
                        long id = rm.ownerId;
                        Transform towerHeaderPos = towerRenders[id].tapTowerPopUpPoint;

                        MessageDispatcher.PostMessage( MessageType.TapTower, cost, id, towerHeaderPos );
                        break;
                    }
                case RenderMessage.Type.TowerHurt:
                {
                    TowerRender towerRender = towerRenders[rm.ownerId];
                    int value = Convert.ToInt32( rm.arguments["value"] );
                    towerRender.Hurt( value, false );
                    //MessageDispatcher.PostMessage( MessageType.TowerHurt, rm.ownerId, value );
					MessageDispatcher.PostMessage( MessageType.CreateMiniMapEffect, UI.MiniMapEffectType.Warning, rm.ownerId, towerRender.mark, rm.position );
                    break;
                }
                case RenderMessage.Type.TowerDestroy:
                {
                    TowerRender towerRender = towerRenders[rm.ownerId];
					towerRenders.Remove( rm.ownerId );

					ForceMark mark = ( ForceMark ) rm.arguments["mark"];
                    towerRender.Dying();

					MatchSide side = GetSideFromMark( mark );

					foreach( KeyValuePair<Data.MatchSide,List<TowerRender>> item in sideTowers )
					{
						if( item.Key == side )
						{
							for( int i = 0; i < item.Value.Count; i++ )
							{
								if( item.Value[i].id == rm.ownerId )
								{
									item.Value.Remove( item.Value[i] );
								}
							}
						}
					}
                    MessageDispatcher.PostMessage( MessageType.TowerDestroyed, mark, rm.ownerId );
                    cullingGroup.RemoveUnitRender( towerRender );
                    break;
                }
				case RenderMessage.Type.InstituteHurt:
				{
					InstituteRender instituteRender = instituteRenders[rm.ownerId];
					int value = Convert.ToInt32( rm.arguments["value"] );
					instituteRender.Hurt( value, false );
					//MessageDispatcher.PostMessage( MessageType.InstituteHurt, rm.ownerId, value );
					MessageDispatcher.PostMessage( MessageType.CreateMiniMapEffect, UI.MiniMapEffectType.Warning, rm.ownerId, instituteRender.mark, rm.position );
					break;
				}
				case RenderMessage.Type.InstituteDestroy:
				{
					InstituteRender instituteRender = instituteRenders[rm.ownerId];
					instituteRender.Dying();

					int mark = Convert.ToInt32( rm.arguments["mark"] );
					MessageDispatcher.PostMessage( MessageType.InstituteDestroyed, mark, rm.ownerId, rm.position );
                    cullingGroup.RemoveUnitRender( instituteRender );
                    break;
				}
				case RenderMessage.Type.TapInstitute:
				{
					MessageDispatcher.PostMessage( MessageType.OpenInstitutePopUp );
					break;
				}
				case RenderMessage.Type.InstituteLevelUpStart:
				{
					InstituteRender instituteRender = instituteRenders[rm.ownerId];
					instituteRender.UpgradeStart();
					
					break;
				}
				case RenderMessage.Type.InstituteLevelUpComplete:
				{
					ForceMark mark = ( ForceMark )rm.arguments["mark"];
					InstituteRender instituteRender = instituteRenders[rm.ownerId];
					int hp =  Convert.ToInt32( rm.arguments["HP"] );
					instituteRender.SetCurrentHp( hp, hp );

					MessageDispatcher.PostMessage( MessageType.BuildingLevelUpOperationFeedBack, mark, BuildingType.Institute );
					break;
				}
				case RenderMessage.Type.InstituteSkillLevelUp:
				{
					ForceMark mark = ( ForceMark )rm.arguments["mark"];
					int applySkillID = Convert.ToInt32( rm.arguments["applySkillID"] );
					int upgradeSkillID = Convert.ToInt32( rm.arguments["upgradeSkillID"] );

					MessageDispatcher.PostMessage( MessageType.InstituteSkillOperationFeedBack, mark, applySkillID, upgradeSkillID );
					break;
				}
				case RenderMessage.Type.RecylingTower:
				{
					TowerRender towerRender = towerRenders[rm.ownerId];
					towerRenders.Remove( rm.ownerId );

					towerRender.Recyling();

					ForceMark mark = ( ForceMark )rm.arguments[ "ForceMark" ];
					MatchSide side = GetSideFromMark( mark );

					foreach( KeyValuePair<Data.MatchSide,List<TowerRender>> item in sideTowers )
					{
						if( item.Key == side )
						{
							for( int i = 0; i < item.Value.Count; i++ )
							{
								if( item.Value[i].id == rm.ownerId )
								{
									item.Value.Remove( item.Value[i] );
								}
							}
						}
					}
					MessageDispatcher.PostMessage( MessageType.TowerDestroyed, mark, rm.ownerId );
                    cullingGroup.RemoveUnitRender( towerRender );
					break;
				}
                case RenderMessage.Type.TownHurt:
                {
                    TownRender townRender = townRenders[rm.ownerId];
                    int value = Convert.ToInt32( rm.arguments["value"] );
                    townRender.Hurt( value, false );
                    MessageDispatcher.PostMessage( MessageType.TownHurt, rm.ownerId, value );
					MessageDispatcher.PostMessage( MessageType.CreateMiniMapEffect, UI.MiniMapEffectType.Warning, rm.ownerId, townRender.mark, rm.position );

                    break;
                }
                case RenderMessage.Type.TownDestroy:
                {
                    TownRender townRender = townRenders[rm.ownerId];
                    ForceMark mark = (ForceMark)rm.arguments["mark"];
                    townRender.Dying();
                    cullingGroup.RemoveUnitRender( townRender );
                    DestroyTown( rm.ownerId, mark );
                    break;
                }
                case RenderMessage.Type.ProjectileHit:
                {
                    ProjectileRender projectileRender = projectileRenders[rm.ownerId];
                    projectileRender.Hit();
                    projectileRenders.Remove( rm.ownerId );
                    cullingGroup.RemoveUnitRender( projectileRender );
                    break;
                }
                case RenderMessage.Type.ProjectileTimeOut:
                {
                    ProjectileRender projectileRender = projectileRenders[rm.ownerId];
                    projectileRender.TimeOut();
                    cullingGroup.RemoveUnitRender( projectileRender );
                    projectileRenders.Remove( rm.ownerId );
                    break;
                }
                case RenderMessage.Type.CrystalMined:
                {
                    CrystalRender crystalRender = crystalRenders[rm.ownerId];
                    int value = Convert.ToInt32( rm.arguments["value"] );
                    crystalRender.Hurt( value, false );
                    break;
                }
                case RenderMessage.Type.CrystalRecover:
                {
                    CrystalRender crystalRender = crystalRenders[rm.ownerId];
                    int value = Convert.ToInt32( rm.arguments["value"] );
                    crystalRender.Heal( value );
                    break;
                }
                case RenderMessage.Type.CrystalDestroy:
                {
                    CrystalRender crystalRender = crystalRenders[rm.ownerId];
                    crystalRender.Destroy();
                    break;
                }
                case RenderMessage.Type.CrystalCarIdle:
                {
                    CrystalCarRender crystalCarRender = crystalCarRenders[rm.ownerId];
                    crystalCarRender.Idle( rm.direction );
                    break;
                }
                case RenderMessage.Type.CrystalCarWalk:
                {
                    CrystalCarRender crystalCarRender = crystalCarRenders[rm.ownerId];
                    crystalCarRender.Walk();
                    break;
                }
                case RenderMessage.Type.CrystalCarMining:
                {
                    CrystalCarRender crystalCarRender = crystalCarRenders[rm.ownerId];
                    crystalCarRender.Attack( rm.direction );
                    break;
                }
                case RenderMessage.Type.CrystalCarHurt:
                {
                    CrystalCarRender crystalCarRender = crystalCarRenders[rm.ownerId];
                    int value = Convert.ToInt32( rm.arguments["value"] );
                    crystalCarRender.Hurt( value, false );
                    break;
                }
                case RenderMessage.Type.CrystalCarDestroy:
                {
                    CrystalCarRender crystalCarRender = crystalCarRenders[rm.ownerId];
					ForceMark mark = ( ForceMark )rm.arguments[ "mark" ];
                    crystalCarRender.Dying( rm.direction );
                    crystalCarRenders.Remove( rm.ownerId );
                    cullingGroup.RemoveUnitRender( crystalCarRender );
                    MessageDispatcher.PostMessage( MessageType.TramCarDestroyed, mark, rm.ownerId, rm.direction );
                    break;
                }
                case RenderMessage.Type.DemolisherIdle:
                {
                    DemolisherRender demolisherRender = demolisherRenders[rm.ownerId];
                    demolisherRender.Idle( rm.direction );
                    break;
                }
                case RenderMessage.Type.DemolisherWalk:
                {
                    DemolisherRender demolisherRender = demolisherRenders[rm.ownerId];
                    demolisherRender.Walk();
                    break;
                }
                case RenderMessage.Type.DemolisherFight:
                {
                    DemolisherRender demolisherRender = demolisherRenders[rm.ownerId];
                    demolisherRender.Attack( rm.direction );
                    break;
                }
                case RenderMessage.Type.DemolisherHurt:
                {
                    DemolisherRender demolisherRender = demolisherRenders[rm.ownerId];
                    int value = Convert.ToInt32( rm.arguments["value"] );
                    demolisherRender.Hurt( value, false );
                    break;
                }
                case RenderMessage.Type.DemolisherDestroy:
                {
                    DemolisherRender demolisherRender = demolisherRenders[rm.ownerId];
					ForceMark mark = ( ForceMark )rm.arguments["mark"];
                    demolisherRender.Dying( rm.direction );
                    demolisherRenders.Remove( rm.ownerId );
                    cullingGroup.RemoveUnitRender( demolisherRender );
                    MessageDispatcher.PostMessage( MessageType.DemolisherDestroyed, mark, rm.ownerId, rm.direction );
                    break;
                }
                case RenderMessage.Type.SpawnNPC:
                {
                    int hp = Convert.ToInt32( rm.arguments["hp"] );
                    ForceMark mark = (ForceMark)rm.arguments["mark"];
                    int modelId = Convert.ToInt32( rm.arguments["modelId"] );

                    SetupNpc( rm.ownerId, modelId, mark, rm.position, rm.direction, hp );
                    MessageDispatcher.PostMessage( MessageType.GenerateNpc, mark, rm.ownerId, rm.position, rm.direction );
                    break;
                }
                case RenderMessage.Type.NpcIdle:
                {
                    NpcRender npcRender = npcRenders[rm.ownerId];
                    npcRender.Idle( rm.direction );
                    break;
                }
                case RenderMessage.Type.NpcWalk:
                {
                    NpcRender npcRender = npcRenders[rm.ownerId];
                    npcRender.Walk( rm.direction );
                    break;
                }
                case RenderMessage.Type.NpcHurt:
                {
                    NpcRender npcRender = npcRenders[rm.ownerId];
                    int value = Convert.ToInt32( rm.arguments["value"] );
                    npcRender.Hurt( value, false );
                    break;
                }
                case RenderMessage.Type.NpcFight:
                {
                    NpcRender npcRender = npcRenders[rm.ownerId];
                    float attackInterval = Convert.ToSingle( rm.arguments["intervalRate"] );
                    npcRender.Attack( rm.direction, attackInterval );
                    break;
                }
                case RenderMessage.Type.NpcFightHit:
                {
                    NpcRender npcRender = npcRenders[rm.ownerId];
                    npcRender.AttackHit();
                    break;
                }
                case RenderMessage.Type.NpcDeath:
                {
                    NpcRender npcRender = npcRenders[rm.ownerId];
                    npcRender.Dying();
                    cullingGroup.RemoveUnitRender( npcRender );
                    break;
                }
                case RenderMessage.Type.NpcReborn:
                {
                    NpcRender npcRender = npcRenders[rm.ownerId];
                    npcRender.Reborn();
                    cullingGroup.AddUnitRender( npcRender );
                    break;
                }
                case RenderMessage.Type.SpawnIdol:
                {
                    int modelId = Convert.ToInt32(rm.arguments["modelId"]);
                    int maxHp = Convert.ToInt32( rm.arguments["maxHp"] );
                    ForceMark mark = (ForceMark)rm.arguments["mark"];

                    SetupIdol( rm.ownerId,modelId, mark, rm.position, rm.direction, maxHp );
                    break;
                }
                case RenderMessage.Type.IdolOut:
                {
                    IdolRender idolRender = idolRenders[rm.ownerId];
                    idolRender.Out();
                    break;
                }
                case RenderMessage.Type.IdolHurt:
                {
                    IdolRender idolRender = idolRenders[rm.ownerId];
                    int value = Convert.ToInt32( rm.arguments["value"] );
                    idolRender.Hurt( value, false );
                    break;
                }
                case RenderMessage.Type.IdolDeath:
                {
                    IdolRender idolRender = idolRenders[rm.ownerId];
                    idolRender.Dying();
                    idolGuardRenders.Remove( rm.ownerId );
                    break;
                }
                case RenderMessage.Type.SpawnIdolGuard:
                {
                    int modelId = Convert.ToInt32( rm.arguments["modelId"] );
                    int maxHp = Convert.ToInt32( rm.arguments["maxHp"] );
                    ForceMark mark = (ForceMark)rm.arguments["mark"];

                    SetupIdolGuard( rm.ownerId, modelId, mark, rm.position, rm.direction, maxHp );
                    break;
                }
                case RenderMessage.Type.IdolGuardDeath:
                {
                    IdolGuardRender idolGuard = idolGuardRenders[rm.ownerId];
                    idolGuard.Dying();
                    idolGuardRenders.Remove( rm.ownerId );
                    break;
                }
                case RenderMessage.Type.IdolGuardHurt:
                {
                    IdolGuardRender idolGuard = idolGuardRenders[rm.ownerId];
                    int value = Convert.ToInt32( rm.arguments["value"] );
                    idolGuard.Hurt( value, false );
                    break;
                }
                case RenderMessage.Type.PowerUpPickedUp:
                {
                    if ( powerUpRenders.ContainsKey( rm.ownerId ) )
                    {
                       PowerUpRender powerUpRender = powerUpRenders[rm.ownerId];
                       powerUpRender.PickedUp();
                       powerUpRenders.Remove( powerUpRender.id );
                       MessageDispatcher.PostMessage( MessageType.PowerUpDestroyed, powerUpRender.id, (int)powerUpRender.PowerUpType, powerUpRender.transform.position );
                    }
                    break;
                }
                case RenderMessage.Type.ShowDeployMouseEffect:
                {
                    effectManager.ShowDeployMouseDown( rm.position );
                    break;
                }
                case RenderMessage.Type.ShowFocusTargetEffect:
                {
                    UnitRenderType type = (UnitRenderType)rm.arguments["targetType"];
                    UnitRender ur = GetUnitRender( rm.ownerId, type );

                    effectManager.ShowFocusTargetEffect( ur.transform );
                    break;
                }
                case RenderMessage.Type.SpawnSkill:
                {
                    long id = rm.ownerId;
                    int metaId = Convert.ToInt32( rm.arguments["metaId"] );
                    int index = Convert.ToInt32( rm.arguments["index"] );
                    ForceMark mark = (ForceMark)rm.arguments["mark"];
                    SetUpSkill( id, metaId, mark, rm.position, rm.direction, index );
                    break;
                }
                case RenderMessage.Type.SkillHit:
                {
                    SkillRender skillRender = skillRenders[rm.ownerId];
                    skillRender.Hit();
                    skillRenders.Remove( rm.ownerId );
                    cullingGroup.RemoveUnitRender( skillRender );
                    break;
                }
                case RenderMessage.Type.NoticeKillIdol:
                {
                    ForceMark mark = (ForceMark)rm.arguments["killerMark"];
                    int iconId = Convert.ToInt32(rm.arguments["killerIconId"]);
                    MessageDispatcher.PostMessage( MessageType.BattleUIKillIdolNotice, mark, iconId );
                    break;
                }
                case RenderMessage.Type.NoticeKillUnit:
                {
                    ForceMark mark = (ForceMark)rm.arguments["killerMark"];
                    int iconId = Convert.ToInt32( rm.arguments["killerIconId"] );
                    ForceMark deadMark = (ForceMark)rm.arguments["deadMark"];
                    int deadIconId = Convert.ToInt32( rm.arguments["deadIconId"] );
                    MessageDispatcher.PostMessage( MessageType.BattleUIKillUnitNotice, mark, iconId, deadMark, deadIconId );
                    break;
                }
                case RenderMessage.Type.InitUIUnitCard:
                {
                    MessageDispatcher.PostMessage( MessageType.InitBattleUISquads, rm.arguments["Cards"], rm.arguments["waitingCard"] );
                    break;
                }
                case RenderMessage.Type.FillUnitUICard:
                {
					MessageDispatcher.PostMessage( MessageType.FillBattleUISquads, rm.arguments["fillCard"], rm.arguments["waitingCard"], rm.arguments["buttonIndex"] );
                    break;
                }
                case RenderMessage.Type.SpawnSummonedUnit:
                {
                    ForceMark mark = (ForceMark)rm.arguments["mark"];
                    int metaId = Convert.ToInt32( rm.arguments["metaId"]);
                    SetUpSummon( UnitRenderType.Summon, rm.ownerId, mark, metaId, rm.position, rm.direction );
                    break;
                }
                case RenderMessage.Type.SummonedUnitIdle:
                {
                    SummonedUnitRender sr = summonRenders[rm.ownerId];
                    sr.Idle();
                    break;
                }
                case RenderMessage.Type.SummonedUnitAttack:
                {
                    SummonedUnitRender sr = summonRenders[rm.ownerId];
                    sr.Attack( rm.direction );
                    break;
                }
                case RenderMessage.Type.SummonedUnitDeath:
                {
                    SummonedUnitRender summounedUnit = summonRenders[rm.ownerId];
                    summounedUnit.Dying();
                    summonRenders.Remove( rm.ownerId );
                    cullingGroup.RemoveUnitRender( summounedUnit );
                    break;
                }
			    case RenderMessage.Type.SpawnTrap:
                {
                    //--TODO
                    ForceMark mark = (ForceMark)rm.arguments["mark"];
                    int metaId = Convert.ToInt32( rm.arguments["metaId"] );
                    int model = Convert.ToInt32( rm.arguments["model"] );
                    SetUpTrap( metaId, UnitRenderType.Trap , rm.ownerId , mark , rm.position , rm.direction );
                    break;
                }
                case RenderMessage.Type.TrapDestroy:
                {
                    TrapRender tr = trapRenders[rm.ownerId];
                    tr.Destroy();
                    trapRenders.Remove( rm.ownerId );
                    cullingGroup.RemoveUnitRender( tr );
                    break;
                }
                case RenderMessage.Type.SimUnitEnterIdleState:
                {
                    MessageDispatcher.PostMessage( MessageType.SimUnitEnterIdleState, rm.ownerId );
                    break;
                }
                case RenderMessage.Type.SetCameraFollow:
                {
                    UnitRenderType renderType = (UnitRenderType)Convert.ToInt32( rm.arguments["targetType"] );
                    UnitRender render = GetUnitRender( rm.ownerId, renderType );
                    if ( render != null )
                    {
                        MessageDispatcher.PostMessage( MessageType.SetCameraFollowTarget, render.transform );
                    }

                    break;
                }
                case RenderMessage.Type.AttachAttributeEffect:
                {
                    UnitRenderType holderType = (UnitRenderType)Convert.ToInt32( rm.arguments["HolderType"] );
                    long holderId = Convert.ToInt64( rm.arguments["HolderId"] );
                    int resId = Convert.ToInt32( rm.arguments["resId"] );
                    string bindPoint = Convert.ToString( rm.arguments["bindPointName"] );

                    UnitRender unitRender = GetUnitRender( holderId, holderType );
                    unitRender.PlayAttributeEffect( rm.ownerId, resId, bindPoint );
                    break;
                }
                case RenderMessage.Type.DetachAttributeEffect:
                {
                    UnitRenderType holderType = (UnitRenderType)Convert.ToInt32( rm.arguments["HolderType"] );
                    long holderId = Convert.ToInt64( rm.arguments["HolderId"] );
                    int resId = Convert.ToInt32( rm.arguments["resId"] );

                    UnitRender unitRender = GetUnitRender( holderId, holderType );
                    unitRender.StopAttributeEffect( rm.ownerId );
                    break;
                }
            }
        }

        public void SetupTown( int modelId, ForceMark mark, long id, int hp, Vector3 p )
        {
            DebugUtils.Log( DebugUtils.Type.Battle, "render world sets up a town!" );

            TownRender townRender = null;

            BattleType type = DataManager.GetInstance().GetBattleType();
            ForceMark ownerMark = DataManager.GetInstance().GetForceMark();

            if ( type == BattleType.Tranining || type == BattleType.Survival )
            {
                townRender = mapData.GetTownBaseObject( mark ).AddComponent<TownRender>();

                if ( type == BattleType.Survival && mark != ownerMark )
                {
                    townRender.gameObject.SetActive( false );
                }
            }
			else if( type == BattleType.BattleP1vsP1 || type == BattleType.Tutorial )
			{
				townRender = mapData1V1.GetTownBaseObject( mark ).AddComponent<TownRender>();
			}
            else
            {
                townRender = mapData2V2.GetTownBaseObject( mark ).AddComponent<TownRender>();
            }

            townRender.id = id;
            townRender.mark = mark;
            townRender.SetInitialHp( hp );
            //townRender.SetHp( hp );
            townRender.SetParent( townRenderRoot );
            townRender.SetPosition( p );
			townRender.buildTime = 5f;//This time use for delay waiting battle start show effect.
			townRender.SettingEffects( mark );
            townRender.gameObject.layer = LayerMask.NameToLayer( "Unit" );
            townRenders.Add( id, townRender );

            MatchSide side = GetSideFromMark( mark );
            sideTowns[side].Add( townRender );
            cullingGroup.AddUnitRender( townRender );
        }

        public void SetupTower( int modelID, ForceMark mark, float buildTime , long towerID, int hp, Vector3 p )
        {
            DebugUtils.Log( DebugUtils.Type.Battle, "render world sets up a Tower!" );

            TowerRender towerRender = null;

			towerRender = ( TowerRender )towerRenderPoolGroup.GetUnit( modelID );
            if ( towerRender != null )
            {
                towerRender.Reset();
                towerRender.id = towerID;
				towerRender.buildTime = buildTime;
                towerRender.mark = mark;
				towerRender.modeID = modelID;
                towerRender.SetPosition( p );
                towerRender.SetInitialHp( hp );
                towerRender.SetParent( towerRenderRoot );
                towerRender.SetRotation( mark );
                towerRender.gameObject.SetActive( true );
            }

            if ( towerRender == null )
			{
                GameObject go = GameResourceLoadManager.GetInstance ().LoadAsset<GameObject> ( modelID );
                towerRender = GameObject.Instantiate( go ).AddComponent<TowerRender>();
				towerRender.gameObject.layer = LayerMask.NameToLayer( "Unit" );
				towerRender.id = towerID;
				towerRender.buildTime = buildTime;
				towerRender.modeID = modelID;
				towerRender.mark = mark;
				towerRender.SetPosition( p );
                towerRender.SetInitialHp( hp );
                towerRender.SetParent( towerRenderRoot );
				towerRender.SetRotation( mark );
				towerRender.SettingEffects();

				MatchSide side = GetSideFromMark( mark );
				if ( sideTowers.ContainsKey( side ) )
				{
					List<TowerRender> towers;
					sideTowers.TryGetValue( side, out towers );

					if( towers != null )
					{
						towers.Add( towerRender );
						sideTowers[ side ] = towers;
					}
					else
					{
						DebugUtils.LogError( DebugUtils.Type.Building, "Can not find sideTowers check logic!" );
					}
				}
				else
				{
					List<TowerRender> tempTower = new List<TowerRender>();
					tempTower.Add( towerRender );

					sideTowers.Add( side, tempTower );
				}

				towerRenderPoolGroup.AddUsedUnit( modelID, towerRender );
			}

			towerRenders.Add( towerID, towerRender );
            cullingGroup.AddUnitRender( towerRender );
        }

		public void SetupInstitute( int modelID, ForceMark mark, float buildTime, long id, int hp, Vector3 p )
		{
			DebugUtils.Log( DebugUtils.Type.Battle, "render world set up a Institute!" );
			InstituteRender instituteRender = null;

			instituteRender = ( InstituteRender )instituteRenderPoolGroup.GetUnit( modelID );
            if ( instituteRender != null )
            {
                instituteRender.Reset();
                instituteRender.mark = mark;
                instituteRender.id = id;
                instituteRender.SetPosition( p );
                instituteRender.SetRotation( mark );
                instituteRender.SetInitialHp( hp );
                instituteRender.SetParent( instituteRenderRoot );
            }
				
            if ( instituteRender == null )
			{
                GameObject go = GameResourceLoadManager.GetInstance ().LoadAsset<GameObject> ( modelID );
                instituteRender = GameObject.Instantiate ( go ).AddComponent<InstituteRender> ();
				instituteRender.gameObject.layer = LayerMask.NameToLayer( "Unit" );
				instituteRender.id = id;
				instituteRender.buildTime = buildTime;
				instituteRender.mark = mark;
				instituteRender.modeID = modelID;
				instituteRender.SetPosition( p );
                instituteRender.SetInitialHp( hp );
				instituteRender.SetRotation( mark );
				instituteRender.SetParent( instituteRenderRoot);
				instituteRender.SettingEffects();
				instituteRender.GameStart();
				instituteRenderPoolGroup.AddUsedUnit( modelID, instituteRender );
			}
				
            instituteRenders.Add( instituteRender.id, instituteRender );
            cullingGroup.AddUnitRender( instituteRender );
        }

        public void GenerateSoldier( int metaId, ForceMark mark, UnitType type, long id, int hp, Vector3 p )
        {
            UnitsProto.Unit unitProto = DataManager.GetInstance().unitsProtoData.Find( u => u.ID == metaId );
            Debug.Assert( unitProto != null, string.Format( "Can't find {0} in unit proto", metaId ) );

            SoldierRender soldierRender = null;
            int modelId = unitProto.Model;

            soldierRender = (SoldierRender)soldierRenderPoolGroup.GetUnit( modelId );
            if ( soldierRender != null )
            {
                soldierRender.Reset();
                soldierRender.Initialize( id, type, mark, unitProto );
                soldierRender.SetInitialHp( hp );
                soldierRender.SetPosition( p );
            }
            else
            {
                //if ( Resource.AssetBundleTest.Instance.runtest )
                {
                    GameObject go = GameResourceLoadManager.GetInstance ().LoadAsset<GameObject> ( modelId );
                    soldierRender = GameObject.Instantiate ( go ).AddComponent<SoldierRender> ();
                }
                //else
                //{
                //    GameObject go = Resource.AssetBundleManager.Instance.GetInstantiatedAsset<GameObject>( "battleui", CommonUtil.SoldierTypeToAssetName( type ) );
                //    soldierRender = go.GetComponent<SoldierRender>();
                //}                

                soldierRender.Initialize( id, type, mark, unitProto );
                soldierRender.SetParent( soldierRenderRoot );
                soldierRender.SetInitialHp( hp );
                soldierRender.SetPosition( p );
                soldierRenderPoolGroup.AddUsedUnit( modelId, soldierRender );
                soldierRender.gameObject.SetActive( false );
            }

			//Drag deployment logic locked.Dwayne 2017.9
            //effectManager.ShowDeployModelEffect( soldierRender.transform );
			effectManager.ShowNewDeployModelEffect( soldierRender.transform.position );
            soldierRenders.Add( soldierRender.id, soldierRender );
            cullingGroup.AddUnitRender( soldierRender );
        }

        public void GenerateProjectile( ForceMark mark, long id, int metaId, Vector3 orginPosition, Vector3 direction, UnitRenderType holderType, long holderId )
        {
            ProjectileProto.Projectile proto = DataManager.GetInstance().projectileProtoData.Find( p => p.ID == metaId );
            Debug.Assert(proto != null, "Can't find projectile proto data, meta id = " + metaId);

            Vector3 position = GetBindPosition( holderType, holderId, proto.projectile_startpoint );
            if ( position.Equals( Vector3.zero ) )
            {
                position = orginPosition;
            }

            int modelId = proto.ID;
            ProjectileRender projectileRender = null;
            projectileRender = (ProjectileRender)projectileRenderPoolGroup.GetUnit( modelId );
            if ( projectileRender != null )
            {
                projectileRender.Reset();
                projectileRender.Initialize( id, position, direction, proto );
            }

            if ( projectileRender == null )
            {
                GameObject go = GameResourceLoadManager.GetInstance ().LoadAsset<GameObject> ( proto.Projectile_ResouceId );
                projectileRender = GameObject.Instantiate ( go ).AddComponent<ProjectileRender> ();
                projectileRender.transform.parent = projectileRenderRoot;
                projectileRender.Initialize( id, position, direction, proto );
                projectileRenderPoolGroup.AddUsedUnit( modelId, projectileRender );
            }

            projectileRenders.Add( projectileRender.id, projectileRender );
            cullingGroup.AddUnitRender( projectileRender );
        }

        public void SetupCrystal( int modelId, long id, bool plus, int hp, Vector3 p, Vector3 d )
        {
            DebugUtils.Log( DebugUtils.Type.Battle, "render world sets up a Crystal!" );

            CrystalRender crystalRender = null;

            crystalRender = (CrystalRender)crystalRenderPool.GetUnit();

            if ( crystalRender == null )
            {
                BattleType type = DataManager.GetInstance().GetBattleType();
                if ( type == BattleType.Survival || type == BattleType.Tranining )
                {
                    crystalRender = mapData.GetCrystalObject( p ).AddComponent<CrystalRender>();
                }
				else if( type == BattleType.BattleP1vsP1 || type == BattleType.Tutorial )
				{
					crystalRender = mapData1V1.GetCrystalObject( p ).AddComponent<CrystalRender>();
				}
                else
                {
                    crystalRender = mapData2V2.GetCrystalObject( p ).AddComponent<CrystalRender>();
                }

                crystalRenderPool.AddUsedUnit( crystalRender );
                crystalRenders.Add( id, crystalRender );
            }
            else
            {
                crystalRender.gameObject.SetActive( true );
                crystalRender.Reset();
            }

            crystalRender.id = id;
            crystalRender.SetParent( crystalRenderRoot );
            crystalRender.SetPosition( p );
            crystalRender.SetInitialHp( hp );
            crystalRender.SetRotation( d );
            cullingGroup.AddUnitRender( crystalRender );
        }

        public void SetupCrystalCar( int modelId, ForceMark mark, long id, int hp, Vector3 p )
        {
            CrystalCarRender crystalCarRender = null;

            crystalCarRender = (CrystalCarRender)crystalCarRenderPool.GetUnit();

            if ( crystalCarRender == null )
            {
                GameObject go = GameResourceLoadManager.GetInstance ().LoadAsset<GameObject> ( modelId );
                crystalCarRender = GameObject.Instantiate ( go ).AddComponent<CrystalCarRender> ();
                crystalCarRenderPool.AddUsedUnit( crystalCarRender );
            }
            else
            {
                crystalCarRender.gameObject.SetActive( true );
                crystalCarRender.Reset();
            }

            crystalCarRender.id = id;
            crystalCarRender.mark = mark;
            crystalCarRender.gameObject.layer = LayerMask.NameToLayer( LayerName.LAYER_UNIT );
            crystalCarRender.SetParent( crystalRenderRoot );
            crystalCarRender.SetPosition( p );
            crystalCarRender.SetInitialHp( hp );
            crystalCarRenders.Add( crystalCarRender.id, crystalCarRender );
            cullingGroup.AddUnitRender( crystalCarRender );
        }

        public void SetupDemolisher( int modelId, ForceMark mark, long id, int hp, Vector3 p )
        {
            DemolisherRender demolisherRender = null;

            demolisherRender = (DemolisherRender)demolisherRenderPool.GetUnit();

            if ( demolisherRender == null )
            {
                GameObject go = GameResourceLoadManager.GetInstance ().LoadAsset<GameObject> ( modelId );
                demolisherRender = GameObject.Instantiate ( go ).AddComponent<DemolisherRender> ();
            }
            else
            {
                demolisherRender.gameObject.SetActive( true );
                demolisherRender.Reset();
            }

            demolisherRender.id = id;
            demolisherRender.mark = mark;
            demolisherRender.gameObject.layer = LayerMask.NameToLayer( LayerName.LAYER_UNIT );
            demolisherRender.SetParent( demolisherRenderRoot );
            demolisherRender.SetPosition( p );
            demolisherRender.SetInitialHp( hp );
            demolisherRenders.Add( demolisherRender.id, demolisherRender );
            demolisherRenderPool.AddUsedUnit( demolisherRender );
            cullingGroup.AddUnitRender( demolisherRender );
        }

        public void SetupNpc( long id, int modelId, ForceMark mark, Vector3 p, Vector3 angles, int hp )
        {
            NpcRender render = null;

            render = (NpcRender)npcRenderPool.GetUnit();

            if ( render == null )
            {
                GameObject g = GameResourceLoadManager.GetInstance().LoadAsset<GameObject>(modelId);

                DebugUtils.Assert( g != null, string.Format( "There isn't NPCRender attached on the {0} model!", modelId ) );

                g = GameObject.Instantiate( g );
                render = g.AddComponent<NpcRender>();

                npcRenderPool.AddUsedUnit( render );
                npcRenders.Add( id, render );
            }
            else
            {
                render.Reset();
            }

            render.id = id;
            render.mark = mark;
            render.modelId = modelId;
            render.gameObject.layer = LayerMask.NameToLayer( LayerName.LAYER_UNIT );
            render.SetParent( npcRenderRoot );
            render.SetPosition( p );
            render.SetAngel( angles );
            render.SetInitialHp( hp );
            render.gameObject.SetActive( true );
            cullingGroup.AddUnitRender( render );
        }

        public void SetupIdol( long id, int modelId, ForceMark mark, Vector3 p, Vector3 angles, int hp )
        {
            IdolRender idolRender = null;

            idolRender = (IdolRender)idolRenderPool.GetUnit();

            if ( idolRender == null )
            {
                GameObject g = GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( modelId );

                DebugUtils.Assert( g != null, string.Format( "There isn't NPCRender attached on the {0} model!", modelId ) );

                g = GameObject.Instantiate( g );
                idolRender = g.AddComponent<IdolRender>();

                idolRenderPool.AddUsedUnit( idolRender );
                idolRenders.Add( id, idolRender );
            }
            else
            {
                idolRender.Reset();
            }

            idolRender.id = id;
            idolRender.mark = mark;
            idolRender.modelId = modelId;
            idolRender.gameObject.layer = LayerMask.NameToLayer( LayerName.LAYER_UNIT );
            idolRender.SetParent( idolRenderRoot );
            idolRender.SetPosition( p );
            idolRender.SetAngel( angles );
            idolRender.SetInitialHp( hp );
            idolRender.gameObject.SetActive( true );
            cullingGroup.AddUnitRender( idolRender );

        }

        public void SetupIdolGuard( long id, int modelId, ForceMark mark, Vector3 p, Vector3 angles, int hp )
        {
            IdolGuardRender idolGuardRender = null;

            idolGuardRender = (IdolGuardRender)idolRenderPool.GetUnit();

            if ( idolGuardRender == null )
            {
                GameObject g = GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( modelId );

                DebugUtils.Assert( g != null, string.Format( "There isn't NPCRender attached on the {0} model!", modelId ) );

                g = GameObject.Instantiate( g );
                idolGuardRender = g.AddComponent<IdolGuardRender>();

                idolGuradRenderPool.AddUsedUnit( idolGuardRender );
                idolGuardRenders.Add( id, idolGuardRender );
            }
            else
            {
                idolGuardRender.Reset();
            }

            idolGuardRender.id = id;
            idolGuardRender.mark = mark;
            idolGuardRender.modelId = modelId;
            idolGuardRender.gameObject.layer = LayerMask.NameToLayer( LayerName.LAYER_UNIT );
            idolGuardRender.SetParent( idolRenderRoot );
            idolGuardRender.SetPosition( p );
            idolGuardRender.SetRotation( angles );
            idolGuardRender.SetInitialHp( hp );
            idolGuardRender.gameObject.SetActive( true );
            cullingGroup.AddUnitRender( idolGuardRender );
        }

        public void SetUpSkill( long id, int metaId, ForceMark mark, Vector3 position, Vector3 rotation, int index )
        {
            UnitSkillsProto.UnitSkill proto = DataManager.GetInstance().unitSkillsProtoData.Find( p => p.ID == metaId );
            Debug.Assert( proto != null, "Can't find skill proto data, meta id = " + metaId );

            SkillRender render = null;

            render = (SkillRender)skillRenderPool.GetUnit();

            if ( render == null )
            {
                int modelId = proto.SkillEffectID;
                GameObject go = GameResourceLoadManager.GetInstance ().LoadAsset<GameObject> ( modelId );
                DebugUtils.Assert( go != null, string.Format( "skill {0}'s model is null resource id = {1}!", id, modelId ) );

                render = GameObject.Instantiate ( go ).AddComponent<SkillRender> ();

                skillRenderPool.AddUsedUnit( render );
            }
            else
            {
                render.Reset();
            }

            render.Initialize( id, mark, metaId, index );
            render.SetParent( skillRenderRoot );
            render.SetPosition( position );
            render.SetRotation( rotation );
            render.gameObject.layer = LayerMask.NameToLayer( LayerName.LAYER_UNIT );
            render.gameObject.SetActive( true );
            cullingGroup.AddUnitRender( render );
            skillRenders.Add( id, render );
        }

        public void SetUpPowerUp( int modelId, PowerUpType powerUpType, ForceMark mark ,long id, Vector3 p )
        {
            PowerUpRender powerUpRender = null;

            powerUpRender = (PowerUpRender)powerUpRenderPool.GetUnit();

            if ( powerUpRender == null )
            {
                GameObject go = GameResourceLoadManager.GetInstance ().LoadAsset<GameObject>( modelId );
                powerUpRender = go.AddComponent<PowerUpRender> ();
                if ( powerUpRender == null )
                {
                    DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( "Null power-up render!!!! model path = {0}", modelId ) );
                }

                powerUpRenderPool.AddUsedUnit( powerUpRender );
            }
            else
            {
                powerUpRender.Reset();
            }

            powerUpRender.id = id;
            powerUpRender.mark = mark;
            powerUpRender.transform.parent = powerUpRenderRoot;
            powerUpRender.SetPosition( p );
            powerUpRenders.Add( powerUpRender.id, powerUpRender );
        }

        public void SetUpSummon( UnitRenderType type, long id, ForceMark mark, int metaId, Vector3 position, Vector3 direction )
        {
            SummonProto.Summon summonProto = DataManager.GetInstance().summonProtoData.Find( p => p.ID == metaId );
            
            SummonedUnitRender summonRender = (SummonedUnitRender)summonRenderPool.GetUnit();
            if ( summonRender == null )
            {
                GameObject g = GameObject.Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( summonProto.Model_id ) );
                summonRender = g.AddComponent<SummonedUnitRender>();
                summonRenderPool.AddUsedUnit( summonRender );
            }
            else
            {
                summonRender.Reset();
            }

            summonRender.id = id;
            summonRender.mark = mark;
            summonRender.SetParent( summonRenderRoot );
            summonRender.SetPosition( position );
            summonRender.SetRotation( direction );
            summonRenders.Add( id, summonRender );
            summonRender.Initialized( metaId );
            cullingGroup.AddUnitRender( summonRender );
        }

        public void SetUpTrap( int metaId , UnitRenderType type , long id , ForceMark mark , Vector3 position , Vector3 rotation )
        {
            TrapRender trapRender = (TrapRender)trapRenderPool.GetUnit();
            TrapProto.Trap proto = DataManager.GetInstance().trapProtoData.Find( p => p.ID == metaId );
            if ( trapRender == null )
            {
                GameObject g = GameObject.Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( proto.TrapResID ) );
                trapRender = g.AddComponent<TrapRender>();
                trapRenderPool.AddUsedUnit( trapRender );
            }
            else
            {
                trapRender.Reset();
            }

            trapRender.id = id;
            trapRender.mark = mark;
            trapRender.SetParent( trapRenderRoot );
            trapRender.SetPosition( position );
            trapRender.SetRotation( rotation );
            trapRenders.Add( id , trapRender );
            trapRender.Initialized( metaId );
            cullingGroup.AddUnitRender( trapRender );
        }

        public void SyncPosition( UnitRenderType type, long ownerId, Vector3 position, bool immediately, Vector3 direction, float deltaDistance )
        {
            if ( type == UnitRenderType.Soldier )
            {
                SoldierRender soldierRender = soldierRenders[ownerId];
                soldierRender.SetPosition( position, false );
                soldierRender.speed = direction;

                if ( direction != Vector3.zero )
                {
                    soldierRender.SetRotation( direction, immediately );
                }

                pathRender.SyncPosition( ownerId, type, position, deltaDistance );
                cullingGroup.SyncPosition( soldierRender , position );
                MessageDispatcher.PostMessage( MessageType.ChangeSoldierPosition, ownerId, position );
            }
            else if ( type == UnitRenderType.Projectile )
            {
                ProjectileRender projectileRender = projectileRenders[ownerId];
                projectileRender.SetPosition( position, false );
                projectileRender.speed = direction;
                if ( direction != Vector3.zero )
                {
                    projectileRender.SetRotation( direction );
                }
            }
            else if ( type == UnitRenderType.CrystalCar )
            {
                CrystalCarRender crystalCarRender = crystalCarRenders[ownerId];
                crystalCarRender.SetPosition( position, false );
                crystalCarRender.speed = direction;
                if ( direction != Vector3.zero )
                {
                    crystalCarRender.SetRotation( direction );
                }

                MessageDispatcher.PostMessage( MessageType.ChangeSoldierPosition, ownerId, position );
            }
            else if ( type == UnitRenderType.Demolisher )
            {
                DemolisherRender demolisherRender = demolisherRenders[ownerId];
                demolisherRender.SetPosition( position, false );
                demolisherRender.speed = direction;
                if ( direction != Vector3.zero )
                {
                    demolisherRender.SetRotation( direction );
                }

                MessageDispatcher.PostMessage( MessageType.ChangeSoldierPosition, ownerId, position );
            }
            else if ( type == UnitRenderType.NPC )
            {
                NpcRender npcRender = npcRenders[ownerId];
                npcRender.SetPosition( position, false );
                npcRender.speed = direction;
                if ( direction != Vector3.zero )
                {
                    npcRender.SetRotation( direction );
                }
            }
        }

        public void SyncHp( UnitRenderType type, long ownerId, ForceMark mark, int hp, int maxHp )
        {
            if ( type == UnitRenderType.Soldier )
            {
                SoldierRender soldierRender = soldierRenders[ownerId];
                soldierRender.SetCurrentHp( hp, maxHp );
                MessageDispatcher.PostMessage( MessageType.SyncUnitHp, mark, ownerId, hp, maxHp );
            }
            else if ( type == UnitRenderType.Town )
            {
                TownRender townRender = townRenders[ownerId];
                townRender.SetCurrentHp( hp, maxHp );
            }
            else if ( type == UnitRenderType.Tower )
            {
                TowerRender towerRender = towerRenders[ownerId];
                towerRender.SetCurrentHp( hp, maxHp );
            }
            else if ( type == UnitRenderType.Institute )
            {
                InstituteRender instituteRender = instituteRenders[ownerId];
                instituteRender.SetCurrentHp( hp, maxHp );
            }
            else if ( type == UnitRenderType.CrystalCar )
            {
                CrystalCarRender crystalCarRender = crystalCarRenders[ownerId];
                crystalCarRender.SetCurrentHp( hp, maxHp );
            }
            else if ( type == UnitRenderType.Demolisher )
            {
                DemolisherRender demolisherRender = demolisherRenders[ownerId];
                demolisherRender.SetCurrentHp( hp, maxHp );
            }
            else if ( type == UnitRenderType.Crystal )
            {
                CrystalRender crystalRender = crystalRenders[ownerId];
                crystalRender.SetCurrentHp( hp, maxHp );
            }
        }

        private void SetCameraFollow( UnitRenderType type, long id )
        {
            Transform target = null;
            switch (type)
            {
                case UnitRenderType.Town:
                {
                    TownRender render = townRenders[id];
                    target = render.transform;
                    break;
                }
                case UnitRenderType.Soldier:
                {
                    SoldierRender render = soldierRenders[id];
                    target = render.transform;
                    break;
                }
            }

            DebugUtils.Assert( target != null, "Set follow camera target failed, because target is null" );
            MessageDispatcher.PostMessage( MessageType.SetCameraFollowTarget, target );
        }

        private UnitRender GetUnitRender( long ownerId, UnitRenderType type )
        {
            UnitRender unitRender = null;
            if ( type == UnitRenderType.Soldier )
            {
                unitRender = soldierRenders[ownerId];
            }
            else if ( type == UnitRenderType.Town )
            {
                unitRender = townRenders[ownerId];
            }
            else if ( type == UnitRenderType.Tower )
            {
                unitRender = towerRenders[ownerId];
            }
            else if ( type == UnitRenderType.Institute )
            {
                unitRender = instituteRenders[ownerId];
            }
            else if ( type == UnitRenderType.CrystalCar )
            {
                unitRender = crystalCarRenders[ownerId];
            }
            else if ( type == UnitRenderType.Demolisher )
            {
                unitRender = demolisherRenders[ownerId];
            }
            else if ( type == UnitRenderType.Crystal )
            {
                unitRender = crystalRenders[ownerId];
            }
            else if ( type == UnitRenderType.Idol )
            {
                unitRender = idolRenders[ownerId];
            }
            else if ( type == UnitRenderType.IdolGuard )
            {
                unitRender = idolGuardRenders[ownerId];
            }
            else if ( type == UnitRenderType.NPC )
            {
                unitRender = npcRenders[ownerId];
            }
            else
            {
                DebugUtils.Assert( false, string.Format( "Can't handle this {0} in GetUnitRender method!", type ) );
            }

            return unitRender;
        }

        public void DestroyTown( long townId, ForceMark mark )
        {
            MatchSide side = GetSideFromMark(mark);
            if ( sideTowns[side].Contains( townRenders[townId] ) )
            {
                sideTowns[side].Remove( townRenders[townId] );
                MessageDispatcher.PostMessage( MessageType.TownDestroy, townId );
            }

            if ( sideTowns[side].Count == 0 )
            {
                NoticeType type = NoticeType.BattleResultDraw;
                if ( side == MatchSide.Red )
                {
                    type = NoticeType.BattleResultBlueWin;
                }
                else if ( side == MatchSide.Blue )
                {
                    type = NoticeType.BattleResultRedWin;
                }
                MessageDispatcher.PostMessage( MessageType.BattleEnd, type );
            }
        }

        public MatchSide GetSideFromMark( ForceMark mark )
        {
            DebugUtils.Assert( mark != ForceMark.NoneForce, "there is no side for " + mark );
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
                DebugUtils.LogError( DebugUtils.Type.Battle, "there is no side for " + mark );
                return MatchSide.NoSide;
            }
        }

        public Vector3 GetBindPosition( UnitRenderType type, long id, string bindpoint )
        {
            Vector3 v = Vector3.zero;
            if ( type == UnitRenderType.Soldier )
            {
                SoldierRender soldier = soldierRenders[id];
                v = soldier.GetBindPointPosition( bindpoint );
            }
            else if ( type == UnitRenderType.Tower )
            {
                TowerRender tower = towerRenders[id];
                v = tower.GetBindPointPosition( bindpoint );
            }
            else if ( type == UnitRenderType.Town )
            {
                TownRender town = townRenders[id];
                v = town.GetBindPointPosition( bindpoint );
            }
            else if ( type == UnitRenderType.NPC )
            {
                NpcRender npc = npcRenders[id];
                v = npc.GetBindPointPosition( bindpoint );
            }
            else if ( type == UnitRenderType.Institute )
            {
                InstituteRender institue = instituteRenders[id];
                v = institue.GetBindPointPosition( bindpoint );
            }

            return v;
        }
    }
}

