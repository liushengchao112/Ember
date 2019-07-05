using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Resource;
using Utils;

namespace Render
{
    /// <summary>
    ///  Show special effect interface manager;
    /// </summary>
    public class BattleEffectManager : MonoBehaviour
    {
        private GameObject deployMouseDownEffect;
        private GameObject focusTargetEffect;
        private List<Animator> deployModelEffectPool;
		private List<GameObject> newDeployModelEffectPool;

        private Dictionary<int, List<GameObject>> gameObjectPool;

        private void Awake()
        {
            deployModelEffectPool = new List<Animator>();
			newDeployModelEffectPool = new List<GameObject>();
            gameObjectPool = new Dictionary<int, List<GameObject>>();
        }

        #region General Effect
        public void ShowDeployMouseDown( Vector3 pos )
        {
            if ( deployMouseDownEffect == null )
            {
                GameObject go = GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "GroundPositionLocator" );
                deployMouseDownEffect = GameObject.Instantiate( go );
                deployMouseDownEffect.name = go.name;
                deployMouseDownEffect.transform.parent = transform;
            }

            deployMouseDownEffect.transform.position = pos;
            deployMouseDownEffect.SetActive( false );
            deployMouseDownEffect.SetActive( true );
        }

        public void ShowFocusTargetEffect( Transform target )
        {
            if ( focusTargetEffect == null )
            {
                focusTargetEffect = GameObject.Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "Target_Attack" ) );
                focusTargetEffect.transform.parent = transform;
            }

            focusTargetEffect.transform.SetParent( target );
            focusTargetEffect.transform.localPosition = Vector3.zero + new Vector3( 0, 2, 0 );
            focusTargetEffect.SetActive( false );
            focusTargetEffect.SetActive( true );
        }
		//Drag deployment logic locked.Dwayne 2017.9
		/*
        public void ShowDeployModelEffect( Transform trans )
        {
            Animator a;
            if ( deployModelEffectPool.Count > 0 )
            {
                a = deployModelEffectPool[0];
                deployModelEffectPool.RemoveAt( 0 );
                a.gameObject.SetActive( true );
            }
            else
            {
                GameObject g = GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "deployment_pod_v2" );
                a = GameObject.Instantiate( g ).GetComponent<Animator>();
                a.transform.parent = transform;
            }

            a.transform.position = trans.position;

            StartCoroutine( WaitSeconds( () =>
            {
                a.transform.Find( "Avatar_Character_Drop_Pod/Persistant_Effects" ).gameObject.SetActive( true );
                a.transform.Find( "Avatar_Character_Drop_Pod/Destroy_Effects" ).gameObject.SetActive( true );
                a.SetTrigger( "Destroy" );

                trans.gameObject.SetActive( true );

                StartCoroutine( WaitSeconds( () =>
                {
                    a.gameObject.SetActive( false );
                    deployModelEffectPool.Add( a );
                }, 5 ) );

            }, 1.5f ) );
        }*/

		//This is new deployModelEffect.
		public void ShowNewDeployModelEffect( Vector3 pos )
		{
			GameObject effect;

			if( newDeployModelEffectPool.Count > 0 )
			{
				effect = newDeployModelEffectPool[0];
				newDeployModelEffectPool.RemoveAt( 0 );
				effect.SetActive( true );
			}
			else
			{
				GameObject go = GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( 30401 );
				effect = GameObject.Instantiate( go );
			}

			effect.transform.position = pos;

			//transform.gameObject.SetActive( true );

			StartCoroutine( WaitSeconds( () =>
			{
				effect.gameObject.SetActive( false );
				newDeployModelEffectPool.Add( effect );
			}, 1 ) );

		}

        #endregion

        // Common Utils
        private GameObject GetGameObjectFromPool( int skillModelId )
        {
            GameObject g;

            if ( !gameObjectPool.ContainsKey( skillModelId ) )
            {
                gameObjectPool.Add( skillModelId, new List<GameObject>() );
            }

            g = gameObjectPool[skillModelId].Find( p => p.gameObject.activeInHierarchy == false );

            if ( g == null )
            {
                g = GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( skillModelId );
                gameObjectPool[skillModelId].Add( g );
            }
            return g;
        }

        IEnumerator WaitSeconds( Action onComplete, float s )
        {
            yield return new WaitForSeconds( s );
            onComplete();
        }
    }

}
