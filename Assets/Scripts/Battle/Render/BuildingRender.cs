using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Utils;

namespace Render
{
	//Mainly use for control performance .Dwayne.
	public class BuildingRender : UnitRender
	{
		#region BuildingRender value

		protected BoxCollider buildingCollider;
		public int modeID;
		public float buildTime;
		protected bool isBuildComplete;
		protected float buildingTimer;
	
		protected GameObject buildEffect;
		protected GameObject finishEffect;
		protected GameObject deathEffect;

		protected bool isDeathEffctShow;
		protected float deathEffctDuration = 5f;
		protected float buildingHideDuration = 1.21f;
		protected float deathEffctTimer;

		protected bool isGameStart;//If this not open, All effect can't active.

		#endregion

		protected override void Awake()
		{
			base.Awake();

			if( unitRenderType != UnitRenderType.Institute )
			{
				animator = gameObject.GetComponent<Animator>();
			}

			buildingCollider = gameObject.GetComponent<BoxCollider>();
		}

		protected override void Update()
		{
			if( isGameStart )
			{
				if( !isBuildComplete )
				{
					BuildingBeingBuilt();
				}

				BuildingEffectShow();
			}
		}

		#region EffectControl functions

		protected void BuildingBeingBuilt()
		{
			buildingTimer += Time.deltaTime;

			BuildingBuiltStartEffect();

			if( buildingTimer >= buildTime )
			{
				isBuildComplete = true;
				buildingTimer = 0;
				buildingCollider.enabled = true;
			}
			else
			{
				if( buildingCollider.enabled == true )
				{
					buildingCollider.enabled = false;
				}
			}
		}

		protected virtual void BuildingBuiltStartEffect()
		{
			
		}

		protected virtual void BuildingBuiltCompleteEffect()
		{
			finishEffect.SetActive( true );
		}

		protected void BuildingEffectShow()
		{
			if( finishEffect != null )
			{
				if( isBuildComplete && finishEffect.activeInHierarchy == false )
				{
					BuildingBuiltCompleteEffect();
				}

				if( isDeathEffctShow )
				{
					deathEffctTimer += Time.deltaTime;

					if( deathEffctTimer >= buildingHideDuration && this.gameObject.activeInHierarchy )
					{
						HideBuildingObj();
					}

					if( deathEffctTimer >= deathEffctDuration )
					{
						isDeathEffctShow = false;
						Destroy();
					}
				}
			}
		}

		public virtual void Dying()
		{
			buildingCollider.enabled = false;

			if( deathEffect != null )
			{
				deathEffect.gameObject.SetActive( true );
			}

			if( healthBar != null )
			{
				healthBar.SetActive( false );
			}

			isDeathEffctShow = true;
		}

		public override void Destroy()
		{
			this.gameObject.SetActive( false );

            if ( finishEffect != null )
            {
                finishEffect.gameObject.SetActive( false );
            }

            if ( deathEffect != null )
            {
                deathEffect.gameObject.SetActive( false );
            }

            base.Destroy();
		}

		public override void Reset()
		{
			isBuildComplete = false;
			buildingTimer = 0;
			deathEffctTimer = 0;
			finishEffect.gameObject.SetActive( false );
		}

		protected virtual void HideBuildingObj()
		{

		}

		#endregion

		public void SetRotation( ForceMark mark )
		{
			if( mark == ForceMark.BottomBlueForce || mark == ForceMark.TopBlueForce )
			{
				transform.rotation = Quaternion.Euler( 0f, 90f, 0f );
			}
			else
			{
				transform.rotation = Quaternion.Euler( 0f, -90f, 0f );
			}
		}

		public Vector3 GetPosition()
		{
			return transform.position;
		}

		public void GameStart()
		{
			isGameStart = true;
			DebugUtils.Log( DebugUtils.Type.Map, "The all player device standby, Map show effect start!" );
		}
	}
}
