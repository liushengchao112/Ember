
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Render;
using Resource;
using Utils;
using Data;

namespace UI
{

    public class HealthbarControl : MonoBehaviour
    {
        #region Instance
        private static HealthbarControl instance;
        public static HealthbarControl Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion

        #region ComponentName

        private RectTransform rootTrans;
        private HealthBar healthBar;
        private FloatingHealth floatingHealth;
        private Canvas canvas;
        private ObjectPool<HealthBar> healthBarPool;
        private ObjectPool<FloatingHealth> floatingHealthPool;
        private Vector3 mirrorVector = new Vector3( 0, 180, 0 );

        public Sprite[] healthBarBgs;
        public Sprite[] healthBars;
        public Sprite[] greenNumbers;
        public Sprite[] redNumbers;
        public Sprite[] purpleNumbers;

        #endregion

        void Awake()
        {
            instance = this;
            rootTrans = GetComponent<RectTransform>();
            canvas = GetComponent<Canvas>();
        }

        void OnDestroy()
        {
            healthBar = null;
            floatingHealth = null;

            if (healthBarPool != null)
            {
                healthBarPool.Clear();
                healthBarPool = null;
            }

            if(floatingHealthPool != null)
            {
                floatingHealthPool.Clear();
                floatingHealthPool = null;
            }
        }

        // Use this for initialization
        void Start()
        {
            healthBar = GameResourceLoadManager.GetInstance().LoadAsset<HealthBar>( "HealthBar" );
            healthBar.transform.localScale = Vector3.one;
            healthBar.transform.localPosition = Vector3.zero;
            healthBar.transform.localRotation = Quaternion.identity;

            floatingHealth = GameResourceLoadManager.GetInstance().LoadAsset<FloatingHealth>( "FloatingHealth" );
            floatingHealth.transform.localScale = Vector3.one;
            floatingHealth.transform.localPosition = Vector3.zero;
            floatingHealth.transform.localRotation = Quaternion.identity;

            healthBarPool = new ObjectPool<HealthBar>( OnCreateHealthBar, OnCleanHealthBar );
            floatingHealthPool = new ObjectPool<FloatingHealth>( OnCreateFloatingHealth, OnCleanFloatingHealth );

        }
        
        public void SetWorldCamera(Camera camera)
        {
            canvas.worldCamera = camera;
        }

        #region HealthBar

        private HealthBar OnCreateHealthBar()
        {
            HealthBar bar = GameObject.Instantiate<HealthBar>( healthBar );
            bar.transform.SetParent( rootTrans, false );
            bar.Init( mirrorVector, healthBarBgs, healthBars );
            return bar;
        }

        private void OnCleanHealthBar( HealthBar bar )
        {
            GameObject.Destroy( bar.gameObject );
            bar = null;
        }

        // Get one in the object pool
        public HealthBar GetHealthBar( ForceMark mark )
        {
            HealthBar bar = healthBarPool.GetObject();
            bar.Refresh( GetPoolByMark( mark ) );
            return bar;
        }

        // When you no longer use this item Call it
        public void RemoveHealthBar( ForceMark mark , HealthBar bar )
        {
            if (bar == null) return;
            bar.SetActive( false );
            healthBarPool.DisposeObject( bar );
        }

        private int GetPoolByMark( ForceMark mark )
        {
            ForceMark myForceMark = DataManager.GetInstance().GetForceMark();
            MatchSide myMatchSide = DataManager.GetInstance().GetMatchSide();
            MatchSide unitMatchSide = GetSideFromMark( mark );

            if ( myMatchSide == unitMatchSide )
            {
                if ( myForceMark == mark )
                {
                    return 0;
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                return 1;
            }
        }

        private MatchSide GetSideFromMark( ForceMark mark )
        {
            if ( mark <= ForceMark.NoneForce )
            {
                return MatchSide.NoSide;
            }
            else if ( mark <= ForceMark.BottomRedForce )
            {
                return MatchSide.Red;
            }
            else if ( mark <= ForceMark.BottomBlueForce )
            {
                return MatchSide.Blue;
            }
            else
            {
                // no side for npc
                return MatchSide.NoSide;
            }
        }

        #endregion

        #region FloatingHealth

        private FloatingHealth OnCreateFloatingHealth()
        {
            FloatingHealth floating = GameObject.Instantiate<FloatingHealth>( floatingHealth );
            floating.transform.SetParent( rootTrans, false );
            floating.Init( canvas, mirrorVector, redNumbers, greenNumbers, purpleNumbers );
            return floating;
        }

        private void OnCleanFloatingHealth( FloatingHealth bar )
        {
            GameObject.Destroy( bar.gameObject );
            bar = null;
        }

        // Get one in the object pool
        public FloatingHealth GetFloatingHealth()
        {
            return floatingHealthPool.GetObject();
        }

        // When you no longer use this item Call it
        public void RemoveFloatingHealth(FloatingHealth bar)
        {
            if (bar == null) return;
            bar.gameObject.SetActive( false );
            floatingHealthPool.DisposeObject( bar );
        }

        #endregion

    }
}
