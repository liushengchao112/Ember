using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Data;
using Utils;
using UI;
using Resource;

namespace Render
{
    public enum UnitRenderType
    {
        None = 0,
        Soldier = 1,
        Town = 2,
        Tower = 3,
        Projectile = 4,
        Crystal = 5,
        CrystalCar = 7,
        PowerUp = 8,
        Skill = 9,
		Institute = 10,
        Demolisher = 11,
        NPC = 12,
        Summon = 13,
        Idol = 14,
        IdolGuard = 15,
        Trap = 16,
    }

    public abstract class UnitRender : MonoBehaviour, IBattlePoolUnit
    {
        public long id;
        public int metaId;
        public ForceMark mark;
        public UnitRenderType unitRenderType;

        public CullingGroup cullingGroup;
        public int boundSpheresIndex;

        protected int hp;
        protected int maxHp;
        protected string currentAnimator = "Idle";
        protected float animSpeed;
        protected Animator animator;
        protected Collider colliders;

        protected HealthBar healthBar;
        protected FloatingHealthHandler floatingHealthHandler;

        // bind points
        protected Dictionary<string, Transform> bindPoints;

        // effects
        protected Dictionary<int, BattleEffectHandler> effectHandlers;
        protected Dictionary<long, BattleEffectHandler> attributeEffectHandlers;

        //Whether it is stealth state ,The default is true
        protected bool isVisible = true;

        // smooth rotate
        private Vector3 targetDirection;
        private float rotateSpeed = 8;
        private bool startRotate;

        // smooth move
        private Vector3 nextPostion;
        private float moveSpeed = 8;
        private float startTime;
        private bool startMove;

        private Vector3 healthBarOffset = Vector3.zero;
        protected float healthBarOffset_y = 0.8f;

        public BattlePool pool { get; set; }

        private bool HaveHealthBar 
        {
            get
            {
                return unitRenderType == UnitRenderType.Demolisher
                || unitRenderType == UnitRenderType.Crystal
                || unitRenderType == UnitRenderType.Soldier
                || unitRenderType == UnitRenderType.Tower
                || unitRenderType == UnitRenderType.Institute
                || unitRenderType == UnitRenderType.CrystalCar
                || unitRenderType == UnitRenderType.Town
                || unitRenderType == UnitRenderType.NPC
                || unitRenderType == UnitRenderType.Idol
                || unitRenderType == UnitRenderType.IdolGuard;

            }
        }
            
#region abstract methods
        public abstract void Reset();
#endregion
       
#region virtual methods
        protected virtual void Awake()
        {
            animSpeed = 1;
            bindPoints = new Dictionary<string, Transform>();
            colliders = GetComponent<BoxCollider>();
            effectHandlers = new Dictionary<int, BattleEffectHandler>();
            attributeEffectHandlers = new Dictionary<long, BattleEffectHandler>();
        }

        protected virtual void OnEnable() { }

        protected virtual void Update()
        {
            float deltaTime = Time.deltaTime;
            if ( deltaTime > 1 )
            {
                deltaTime = 1;
            }

            //The direction of rotation is slow action
            if ( startRotate )
            {
                if ( targetDirection == Vector3.zero )
                {
                    return;
                }

                Quaternion quaDir = Quaternion.LookRotation( targetDirection, Vector3.up );
                transform.rotation = Quaternion.Lerp( transform.rotation, quaDir, deltaTime * rotateSpeed );

                if ( quaDir.Equals( transform.rotation ) )
                {
                    startRotate = false;
                }
            }

            if ( startMove )
            {
                Vector3 deltaPostion = Vector3.Lerp( transform.position, nextPostion, deltaTime * moveSpeed );

                transform.position = deltaPostion;

                if ( nextPostion.Equals( transform.position ) )
                {
                    startMove = false;
                }
            }
        }

        public virtual void SetInitialHp( int value )
        {
            this.hp = maxHp = value;
        }

        public virtual void SetCurrentHp( int value, int maxHp )
        {
            this.hp = value;
            this.maxHp = maxHp;
            SetHealth( value, maxHp );
        }

        public virtual void Hurt( int value, bool isCrit )
        {
            DebugUtils.Assert( maxHp != 0, "the maxHp shouldn't be zero!" );

            hp -= value;
            if ( hp < 0 )
                hp = 0;

            SetHealth( hp, maxHp );
            SetFloatDamage( -value, isCrit );
        }

        public virtual void Heal( int healValue )
        {
            //add soldier heal effect
            hp += healValue;
            if ( hp < 0 )
                hp = 0;

            //TODO: healthbar handling
            SetHealth( hp, maxHp );
            SetFloatDamage( healValue, false );
        }

        public virtual void Destroy()
        {
            if (healthBar != null && HealthbarControl.Instance != null)
            {
                healthBar.Dispose();
                HealthbarControl.Instance.RemoveHealthBar( mark , healthBar );
            }

            if ( attributeEffectHandlers != null )
            {
                foreach ( KeyValuePair<long, BattleEffectHandler> v in attributeEffectHandlers )
                {
                    v.Value.Stop();
                }
            }

            healthBar = null;

            if ( pool != null )
            {
                Recycle();
            }
        }

        //TODO : Used for stealth, currently unable to test
        public virtual void SetVisible(bool visible)
        {
            isVisible = visible;

            if ( healthBar != null )
            {
                healthBar.SetActive( visible );
            }
        }

        public virtual void SetPosition( Vector3 p, bool immediately =  true )
        {
            if ( immediately )
            {
                transform.position = p;
            }
            else
            {
                startTime = Time.time;
                nextPostion = p;
                startMove = true;
            }

            //TODO : Used to refresh the position again to prevent timing problems
            if (healthBar != null)
            {
                if (colliders != null)
                {
                    healthBarOffset.Set( 0, colliders.bounds.size.y + healthBarOffset_y, 0 );

                    healthBar.SetTarget( transform, healthBarOffset );
                }
                else
                {
                    DebugUtils.LogError( DebugUtils.Type.UI, string.Format( " Unit render {0} can't find collider, health bar attached failed ", id ) );
                }
            }
        }

        public virtual void SetAngel( Vector3 angle )
        {
            transform.rotation = Quaternion.Euler( angle );
        }

        public virtual void SetRotation( Vector3 direction, bool immediately = false )
        {
            if ( immediately )
            {
                transform.rotation = Quaternion.LookRotation( direction, Vector3.up );
                startRotate = false;
            }
            else
            {
                if ( direction != Vector3.zero && targetDirection.Equals( direction ) )
                    return;

                targetDirection = direction;
                startRotate = true;
            }
        }

        public virtual void OnCullingStateChange( bool isVisibleInCamera )
        {
            // Implement in derived class
            // Nothing can do in unitRender for now.
        }
        #endregion

        private void SetHealth(int hp, int maxHp)
        {
            if ( !isVisible ) return;

            CheckHealthBar();

            if (healthBar != null)
            {
                healthBar.SetActive( hp < maxHp );
                healthBar.SetHealth( hp, maxHp );
            }
        }

        //Blood bar, increase or decrease the floating text hints
        private void SetFloatDamage( int changeValue, bool isCrit )
        {
            if ( !isVisible ) return;

            CheckFloatDamage();

            if(floatingHealthHandler != null)
            {
                if (hp < maxHp && changeValue != 0)
                {
                    bool hasBloodBar = healthBar != null;
                    Transform tf = null;
                    if ( hasBloodBar )
                    {
                        tf = healthBar.rectTrans;
                        healthBarOffset.Set( 0, healthBar.rectTrans.rect.height + 3, 0 );
                    }
                    else
                    {
                        tf = transform;
                    }
                    floatingHealthHandler.Show( changeValue, isCrit, tf, healthBarOffset, hasBloodBar );
                }
            }
        }

        private void CheckHealthBar()
        {
            if (healthBar != null) return;
            if (!gameObject.activeSelf) return;
            if (!HaveHealthBar) return;
            if (DataManager.GetInstance().displayBarChoose != 1) return;

            healthBar = HealthbarControl.Instance.GetHealthBar( mark );
            if (healthBar == null)
            {
                DebugUtils.LogError( DebugUtils.Type.UI, "HealthBar asset is null" );
                return;
            }

            healthBar.SetActive( false );
#if UNITY_EDITOR
            healthBar.name = "HealthBar_" + gameObject.name;
#endif

            if ( colliders != null )
            {
                healthBarOffset.Set( 0, colliders.bounds.size.y + healthBarOffset_y , 0 );

                healthBar.SetTarget( transform, healthBarOffset );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.UI, string.Format(" Unit render {0} can't find collider, health bar attached failed ", id));
            }
        }

        private void CheckFloatDamage()
        {
            if (floatingHealthHandler != null) return;
            if (!gameObject.activeSelf) return;
            if (!HaveHealthBar) return;
            if (DataManager.GetInstance().damageNumChoose != 1) return;

            GameObject go = GameObject.Find( "goFloatingHealthHandler" );

            if (go == null)
            {
                go = new GameObject( "goFloatingHealthHandler" );
                floatingHealthHandler = go.AddComponent<FloatingHealthHandler>();
                floatingHealthHandler.Init();
            }
            else
            {
                floatingHealthHandler = go.GetComponent<FloatingHealthHandler>();
            }
        }

        public void SetParent( Transform trans )
        {
            transform.SetParent( trans );
        }

        protected void PlayAnimation( string currentAnimationName, float speed = 1 )
        {
            animator.speed = speed;
            animator.ResetTrigger( currentAnimator );
            currentAnimator = currentAnimationName;
            animator.SetTrigger( currentAnimator );
        }

        public Vector3 GetBindPointPosition( string pointName )
        {
            Transform t = GetBindPoint( pointName );

            if ( t != null )
            {
                return t.position;
            }
            return Vector3.zero;
        }

        protected Transform GetBindPoint( string pointName )
        {
            if ( bindPoints.ContainsKey( pointName ) )
            {
                return bindPoints[pointName];
            }
            else
            {
                Transform t = FindDeepChild( transform, pointName );
                if ( t != null )
                {
                    bindPoints.Add( t.name, t );
                }
                else
                {
                    DebugUtils.LogWarning( DebugUtils.Type.AI_Skill, "Soldier Render : " + metaId + " couldn't find bindpoint " + pointName );
                }

                return t;
            }
        }

        public Transform FindDeepChild( Transform parent, string childName )
        {
            Transform y = null;
            y = parent.transform.Find( childName );
            if ( y == null )
            {
                foreach ( Transform child in parent )
                {
                    y = FindDeepChild( child, childName );
                    if ( y != null )
                    {
                        return y;
                    }
                }
            }
            return y;
        }

        public void PlayAttributeEffect( long id, int effectResId, string bindpoint )
        {
            if ( !attributeEffectHandlers.ContainsKey( id ) )
            {
                BattleEffectHandler effect = PlayEffect( effectResId, bindpoint );

                if ( effect != null )
                {
                    attributeEffectHandlers.Add( id, effect );
                }
            }
            else
            {
                attributeEffectHandlers[id].Play();
            }
        }

        public void StopAttributeEffect( long id )
        {
            if ( attributeEffectHandlers.ContainsKey( id ) )
            {
                attributeEffectHandlers[id].Stop();
                attributeEffectHandlers.Remove( id );
            }
        }

        protected BattleEffectHandler PlayEffect( int effectResId, Vector3 postion, float speedRate = 1 )
        {
            BattleEffectHandler effect = LoadEffect( effectResId );

            if ( effect != null )
            {
                effect.transform.SetParent( transform );
                effect.transform.position = postion;
                effect.transform.localRotation = Quaternion.identity;
                effect.Play( speedRate );
            }

            return effect;
        }

        protected BattleEffectHandler PlayEffect( int effectResId, string bindpoint, float speedRate = 1 )
        {
            BattleEffectHandler effect = LoadEffect( effectResId );

            if ( effect != null )
            {
                effect.transform.parent = GetBindPoint( bindpoint );
                effect.transform.localPosition = Vector3.zero;
                effect.transform.localRotation = Quaternion.identity;
                effect.Play( speedRate );
            }

            return effect;
        }

        protected BattleEffectHandler LoadEffect( int effectResId )
        {
            if ( effectResId <= 0 )
            {
                DebugUtils.LogWarning( DebugUtils.Type.AI_Skill, "Soldier Render : " + metaId + " play a effect failed！ effectResId:" + effectResId );
                return null;
            }

            BattleEffectHandler effectHandler;
            if ( effectHandlers.ContainsKey( effectResId ) )
            {
                effectHandler = effectHandlers[effectResId];
            }
            else
            {
                GameObject go = GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( effectResId );
                DebugUtils.Assert( go != null, string.Format( "The Resource you want to instantiate is null, resourceId = {0}", effectResId ) );

                effectHandler = GameObject.Instantiate( go ).AddComponent<BattleEffectHandler>();
                effectHandlers.Add( effectResId, effectHandler );
            }

            return effectHandler;
        }

        public virtual void Recycle()
        {
            //gameObject.SetActive( false );
            pool.Recycle( this );
        }

        public virtual void Clear()
        {
            
        }
    }
}
