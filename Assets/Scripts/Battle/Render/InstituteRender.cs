using UnityEngine;
using System.Collections.Generic;

using Data;
using Utils;
using System;

namespace Render
{
	public class InstituteRender : BuildingRender
	{
		//If designer change the build time, just building animator will be change.
		private List<ParticleSystem> buildEffectFlashParticles = new List<ParticleSystem>();
		private List<ParticleSystem> buildEffectLightParticles = new List<ParticleSystem>();

		private float buildEffectFlashParticlesOriginalDuration;
		private float buildEffectLightParticlesOriginalStartLifeTime;

		//The original time set by art.
		private int originalBuildTime = 3;
		private float particlePerformanceRatio;

		private string buildEffectPath;

		private GameObject buildingBody;
		private GameObject buildingCrystal;

		protected override void Awake()
		{
			unitRenderType = UnitRenderType.Institute;
			originalBuildTime = 3;
			base.isGameStart = false;

			base.Awake();

			buildEffectPath = gameObject.name.Split('(')[0];
			buildingBody = transform.Find( buildEffectPath ).gameObject;
			buildingCrystal = transform.Find( string.Format( buildEffectPath + "_crystal" ) ).gameObject;
		}
			
		public override void Reset()
		{
			id = -1;
			transform.position = new Vector3( float.MaxValue / 2, float.MaxValue / 2, float.MaxValue / 2 );
            this.gameObject.SetActive( true );

			base.Reset();

			buildingBody.SetActive( true );
			buildingCrystal.SetActive( false );
		}

        public override void SetPosition( Vector3 p, bool immediately = true )
        {
            healthBarOffset_y = 1.5f;
            base.SetPosition( p, immediately );
        }

        public void UpgradeStart()
		{
			isBuildComplete = false;
			//If need more performance add there.
		}

		public void UpgradeComplete()
		{
			
		}

		#region SetEffects functions

		public void SettingEffects()
		{
			GetEffects();
			GetBuildEffectParticleSystem();
			CheckBuildTime();
		}

		private void GetEffects()
		{
			buildEffect = transform.Find( string.Format( buildEffectPath + "_building" ) ).gameObject;
			finishEffect = transform.Find( string.Format( buildEffectPath + "_finish" ) ).gameObject;
			deathEffect = transform.Find( string.Format( buildEffectPath + "_death") ).gameObject;
		}

		private void GetBuildEffectParticleSystem()
		{
			string path;
			string path2 = "nengliangqiu_0";

			if( mark == ForceMark.TopRedForce || mark == ForceMark.BottomRedForce )
			{
				path = "Lab_red_building/";
			}
			else
			{
				path = "Lab_blue_building/";
			}

			for( int i = 0; i < 4; i++ )
			{
				Transform tempRoot = transform.Find( path + path2 + ( i + 1 ) );

				buildEffectFlashParticles.Add( tempRoot.GetChild(0).GetComponent<ParticleSystem>() );

				buildEffectLightParticles.Add( tempRoot.GetChild(1).GetComponent<ParticleSystem>() );
				buildEffectLightParticles.Add( tempRoot.GetChild(2).GetComponent<ParticleSystem>() );
				buildEffectLightParticles.Add( tempRoot.GetChild(3).GetComponent<ParticleSystem>() );
			}

			if( buildEffectFlashParticlesOriginalDuration == 0 )
			{
				buildEffectFlashParticlesOriginalDuration = buildEffectFlashParticles[0].main.duration;
			}

			if( buildEffectLightParticlesOriginalStartLifeTime == 0 )
			{
				buildEffectLightParticlesOriginalStartLifeTime = buildEffectLightParticles[0].main.startLifetimeMultiplier;
			}
		}

		private void CheckBuildTime()
		{
			if( buildTime != originalBuildTime )
			{
				particlePerformanceRatio = buildTime / originalBuildTime;
				ModifyParticleValues();
			}
		}

		private void ModifyParticleValues()
		{
			for( int i = 0; i < buildEffectFlashParticles.Count; i++ )
			{
				var ma = buildEffectFlashParticles[i].main;
				ma.duration *= particlePerformanceRatio;
			}

			for( int i = 0; i < buildEffectLightParticles.Count; i++ )
			{
				var ma = buildEffectLightParticles[i].main;
				ma.startLifetimeMultiplier *= particlePerformanceRatio;
			}
		}

		protected override void BuildingBuiltStartEffect()
		{
			if( finishEffect.activeInHierarchy == false )
			{
				buildEffect.SetActive( true );
			}
		}

		protected override void BuildingBuiltCompleteEffect()
		{
			base.BuildingBuiltCompleteEffect();
			buildingCrystal.SetActive( true );
		}

		protected override void HideBuildingObj()
		{
			buildingBody.SetActive( false );
			buildingCrystal.SetActive( false );
			buildEffect.SetActive( false );
			finishEffect.SetActive( false );
		} 
			
		#endregion
    }
}
