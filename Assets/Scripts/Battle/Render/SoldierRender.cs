/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: SoldierRenderer.cs
// description: 
// 
// created time：09/28/2016
//
//----------------------------------------------------------------*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Utils;
using Constants;
using Data;
using Resource;

using UnitProto = Data.UnitsProto.Unit;
using ProjectileProto = Data.ProjectileProto.Projectile;
using SkillProto = Data.UnitSkillsProto.UnitSkill;
using SummonProto = Data.SummonProto.Summon;

namespace Render
{
    [RequireComponent( typeof( BoxCollider ) )]
    public class SoldierRender : UnitRender
    {
        private static readonly string TRIGGER_IDLE = "Idle";
        private static readonly string TRIGGER_RUN = "Run";
        private static readonly string TRIGGER_CRIT = "Crit";
        private static readonly string TRIGGER_ATTACK = "Attack";
        private static readonly string TRIGGER_DYING = "Dying";
        private static readonly string TRIGGER_SKILL1 = "Skill1";
        private static readonly string TRIGGER_SKILL2 = "Skill2";

        // dash
        private static readonly string TRIGGER_DASHSTART = "DashStart";
        private static readonly string TRIGGER_DASHING = "Dashing";
        private static readonly string TRIGGER_DASHEND = "DashEnd";

        // battry
        private static readonly string TRIGGER_INSTALLBATTRY = "InstallBattry";
        private static readonly string TRIGGER_USINGBATTRY = "UsingBattry";
        private static readonly string TRIGGER_PACKUPBATTEY = "PackupBattry";

        private static readonly string SKILL_BINDPOINT = "SkillPoint";
        private static readonly string FOOT_BINDPOINT = "FootEffectPoint";
        private static readonly string HEAD_BINDPOINT = "HeadEffectPoint";
        private static readonly string FIRE_BINDPOINT = "fireBindPoint";
        private static readonly string DAMAGE_BINDPOINT = "DamagePoint";


        public Vector3 speed;
        public UnitType soldierType;
        public UnitProto proto;

        // resource info
        public int attackEffectResId;
        public string attackEffectBindpoint;
        public int attackHitResId;
        public string attackHitResBindpoint;
        public int critEffectResId;
        public string critEffectResBindpoint;

        // components
        private SkinnedMeshRenderer render;
        private SpriteRenderer selectionCricle;

        // projectiles
        private Dictionary<int, ProjectileProto> unitProjectiles;

        // summon 
        private Dictionary<int, SummonProto> summonProtos;

        // skills
        private Dictionary<int, SkillProto> skills;

        // material 
        private Color32 highlightColor;
        private Color32 normalColor;

        private new Transform transform;

        protected override void Awake()
        {
            unitRenderType = UnitRenderType.Soldier;

            base.Awake();

            transform = base.transform;

            render = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            //hideShader = Shader.Find( "Unlit/UnitModelTransparent" ); // TODO: templete code

            animator = gameObject.GetComponent<Animator>();
            animator.cullingMode = AnimatorCullingMode.CullCompletely;

            bindPoints = new Dictionary<string, Transform>();

            unitProjectiles = new Dictionary<int, ProjectileProto>();
            skills = new Dictionary<int, SkillProto>();
            summonProtos = new Dictionary<int, SummonProto>();

            SetVisible( true );
        }

        void Start()
        {
            currentAnimator = TRIGGER_IDLE;
            MessageDispatcher.AddObserver( BattleEnd, MessageType.BattleEnd );
        }

        void OnDestroy()
        {
            MessageDispatcher.RemoveObserver( BattleEnd, MessageType.BattleEnd );
        }

        public void Initialize( long id, UnitType type, ForceMark mark, UnitProto proto )
        {
            this.id = id;
            this.soldierType = type;
            this.mark = mark;
            this.proto = proto;

            attackEffectResId = proto.attack_res;
            attackEffectBindpoint = proto.attack_res_bindpoint;
            attackHitResId = proto.attack_hit_res;
            attackHitResBindpoint = proto.attack_hit_res_bindpoint;
            critEffectResId = proto.crit_effect_res;
            critEffectResBindpoint = proto.crit_effect_res_bindpoint;

            gameObject.name = string.Format( "SoldierRender{0}", id );
            gameObject.layer = LayerMask.NameToLayer( LayerName.LAYER_UNIT );

            Color32 color = render.material.GetColor( "_RimColor" );
            normalColor = new Color32( 100, 100, 100, 255 );
            highlightColor = new Color32( 0, 97, 255, 255);
            render.material.SetColor( "_RimColor", color );
        }

        public void Idle( Vector3 direction )
        {
            SetRotation( direction );
            PlayAnimation( TRIGGER_IDLE, 1 );
        }

        public void Walk( Vector3 direction )
        {
            SetRotation( direction );
            PlayAnimation( TRIGGER_RUN, 1 );

            // Stop all skill now
            foreach ( KeyValuePair<int, BattleEffectHandler> effect in effectHandlers )
            {
                effect.Value.gameObject.SetActive( false );
            }
        }

        public void Attack( Vector3 direction, float speedRate, bool isCrit )
        {
            SetRotation( direction, true );

            if ( isCrit )
            {
                PlayAnimation( TRIGGER_CRIT, speedRate );
                PlayEffect( critEffectResId, critEffectResBindpoint, speedRate );
            }
            else
            {
                PlayAnimation( TRIGGER_ATTACK, speedRate );
                PlayEffect( attackEffectResId, attackEffectBindpoint, speedRate );
            }
        }

        public void SpawnProjecile( Vector3 direction, int projectileMetaId, float speedRate )
        {
            SetRotation( direction, true );

            ProjectileProto proto = null;
            if ( unitProjectiles.ContainsKey( projectileMetaId ) )
            {
                proto = unitProjectiles[projectileMetaId];
            }
            else
            {
                proto = DataManager.GetInstance().projectileProtoData.Find( p => p.ID == projectileMetaId );
            }

            if ( proto != null )
            {
                PlayAnimation( TRIGGER_ATTACK, speedRate );
                PlayEffect( proto.FireEffect_ResourceId, proto.fire_res_bindpoint, speedRate );
            }
            else
            {
                DebugUtils.Assert( false, string.Format( " Can't find projectile meta id {0} when solder spawn a projectile", projectileMetaId ) );
            }
        }
        
        public void Hit()
        {
            PlayEffect( attackHitResId, attackHitResBindpoint );
        }

        public override void Hurt( int value, bool isCrit )
        {
            base.Hurt( value, isCrit );

            // TODO: health bar will be null when unit first get hurt and unit in invisible state, need be fixed
            if ( !isVisible && healthBar != null )
            {
                healthBar.gameObject.SetActive( false );
            }
        }

        public override void Heal( int healValue )
        {
            base.Heal( healValue );
        }

        //Add in Dead animation clip event
        public override void Destroy()
        {
            gameObject.SetActive( false );
            base.Destroy();
        }

        public void ReleaseSkill( int index, int skillMetaId, Vector3 position )
        {
            SkillProto proto = DataManager.GetInstance().unitSkillsProtoData.Find( p => p.ID == skillMetaId );

            // TODO: Wait new data struct
            int carrierType = proto.CarrierType;

            int fireEffectId = 0;
            string bindPoint = string.Empty;

            if ( proto.CarrierID <= 0 )
            {
                fireEffectId = proto.SkillEffectID;
                bindPoint = proto.skill_effect_bindpoint;
                PlayEffect( fireEffectId, bindPoint );
            }
            else
            {
                int carrierId = proto.CarrierID;

                // 1 projectile
                if ( carrierType == 1 )
                {
                    ProjectileProto projectileProto = null;

                    if ( unitProjectiles.ContainsKey( carrierId ) )
                    {
                        projectileProto = unitProjectiles[carrierId];
                    }
                    else
                    {
                        projectileProto = DataManager.GetInstance().projectileProtoData.Find( p => p.ID == carrierId );
                        unitProjectiles.Add( carrierId, projectileProto );
                    }

                    if ( projectileProto != null )
                    {
                        fireEffectId = projectileProto.FireEffect_ResourceId;
                        bindPoint = projectileProto.fire_res_bindpoint;

                        PlayEffect( fireEffectId, bindPoint );
                    }
                }
                else if ( carrierType == 2 )
                {
                    // trap
                }
                else if ( carrierType == 3 )
                {
                    SummonProto summonProto = null;

                    if ( summonProtos.ContainsKey( carrierId ) )
                    {
                        summonProto = summonProtos[carrierId];
                    }
                    else
                    {
                        summonProto = DataManager.GetInstance().summonProtoData.Find( p => p.ID == carrierId );
                        summonProtos.Add( carrierId, summonProto );
                    }

                    if ( summonProto != null )
                    {
                        // summon
                        fireEffectId = summonProto.Born_effect_id;
                        PlayEffect( fireEffectId, position );
                    }

                }
            }

            if ( index == 1 )
            {
                PlayAnimation( TRIGGER_SKILL1, 1 );
            }
            else if ( index == 2 )
            {
                PlayAnimation( TRIGGER_SKILL2, 1 );
            }
            else
            {
                DebugUtils.Assert( false, "Can't handle this skill index in soldier render, index = " + index );
            }
        }

        public void Dying( Vector3 direction )
        {
            if ( healthBar != null )
            {
                healthBar.SetActive( false );
            }

            SetRotation( direction );
            if ( mark == DataManager.GetInstance().GetForceMark() )
            {
                SetSelection( false );
            }

            PlayAnimation( TRIGGER_DYING, 1 );

            foreach ( KeyValuePair<int, BattleEffectHandler> effect in effectHandlers )
            {
                effect.Value.gameObject.SetActive( false );
            }
        }

        public override void SetVisible( bool visible )
        {
            bool useCloaking = false;

            if ( !visible )
            {
                if ( mark == DataManager.GetInstance().GetForceMark() )
                {
                    // friendly unit 
                    useCloaking = true;
                    visible = true;
                }
                else
                {
                    // enemy unit
                    useCloaking = false;
                    visible = false;
                }
            }
            else
            {
                useCloaking = false;
                visible = true;
            }

            // Set current render visible state.
            // Use cloaking will be deemed as visible.
            base.SetVisible( visible );

            if ( useCloaking )
            {
                // friendly unit 
                render.material.SetFloat( "_Alpha", 0.5f );
            }
            else
            {
                if ( visible )
                {
                    render.material.SetFloat( "_Alpha", 1f );
                    render.enabled = true;
                    //healthBar.gameObject.SetActive( true );
                }
                else
                {
                    render.enabled = false;
                    //healthBar.gameObject.SetActive( false );
                }
            }
        }

        public override void OnCullingStateChange( bool isVisibleInCamera )
        {
            base.OnCullingStateChange( isVisibleInCamera );

            if ( isVisibleInCamera && isVisible )
            {
                if ( !render.enabled )
                {
                    render.enabled = true;
                }
            }
            else
            {
                if ( render.enabled && !CullingGroupManager.disableCullMesh )
                {
                    render.enabled = false;
                }
            }
        }

        public void SetSelection( bool selection )
        {
            if ( selection )
            {
                render.material.SetColor( "_RimColor", highlightColor );
                //if ( selectionCricle == null )
                //{
                //    GameObject go = GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "SelectionCircle" );
                //    selectionCricle = GameObject.Instantiate( go ).GetComponent<SpriteRenderer>();
                //    selectionCricle.transform.parent = transform;
                //    selectionCricle.transform.localPosition = Vector3.zero;
                //}
                //else
                //{
                //    selectionCricle.gameObject.SetActive( true );
                //}

                //if ( mark == DataManager.GetInstance().GetForceMark() )
                //{
                //    selectionCricle.color = new Color32( 0, 255, 0, 255 );
                //}
                //else
                //{
                //    selectionCricle.color = new Color32( 255, 0, 0, 255 );
                //}
            }
            else
            {
                //render.material.get( "_RimColor", 0.5f );
                //if ( selectionCricle != null )
                //{
                //    selectionCricle.gameObject.SetActive( false );
                //}

                render.material.SetColor( "_RimColor", normalColor );
            }
        }

        public void Stuned( bool value )
        {
            if ( value )
            {
                PlayAnimation( TRIGGER_IDLE, 1 );
            }
            else
            {

            }
        }

        public void Dash( int state, Vector3 rotation )
        {
            if ( state == 1 )
            {
                SetRotation( rotation );
                PlayAnimation( TRIGGER_DASHSTART );
                PlayEffect( 70505, "SkillPoint" );
            }
            else if ( state == 2 )
            {
                PlayEffect( 70506, "SkillPoint" );
                PlayAnimation( TRIGGER_DASHING );
            }
            else if ( state == 3 )
            {
                PlayEffect( 70507, "SkillPoint" );
                PlayAnimation( TRIGGER_DASHEND );
            }
        }

        public void UseBattery( int state )
        {
            if ( state == 1 )
            {
                PlayAnimation( TRIGGER_INSTALLBATTRY );
            }
            else if ( state == 3 )
            {
                PlayAnimation( TRIGGER_PACKUPBATTEY );
            }
        }

        public void BatteryFire( Vector3 rotation, int projectileId )
        {
            SetRotation( rotation, true );
            PlayAnimation( TRIGGER_USINGBATTRY );

            ProjectileProto projectileProto = null;

            if ( unitProjectiles.ContainsKey( projectileId ) )
            {
                projectileProto = unitProjectiles[projectileId];
            }
            else
            {
                projectileProto = DataManager.GetInstance().projectileProtoData.Find( p => p.ID == projectileId );
                unitProjectiles.Add( projectileId, projectileProto );
            }

            if ( projectileProto != null )
            {
               int fireEffectId = projectileProto.FireEffect_ResourceId;
               string bindPoint = projectileProto.fire_res_bindpoint;

               PlayEffect( fireEffectId, bindPoint );
            }
        }

        public void PickUpPowerUp( PowerUpType type, int activeEffectPath, int persistantEffectPath, int deactivateEffectPath, int lifeTime )
        {

        }

        public override void Reset()
        {
            id = -1;
            transform.rotation = Quaternion.identity;
            transform.position = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
            speed = Vector3.zero;
        }

        public void BattleEnd( object type )
        {
            Idle( speed );
        }
    }
}
