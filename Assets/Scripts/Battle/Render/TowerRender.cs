/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: TowerRender.cs
// description: 
// 
// created time：10/18/2016
//
//----------------------------------------------------------------*/
using UnityEngine;
using System.Collections.Generic;

using Data;
using Utils;
using System;

namespace Render
{
	public class TowerRender : BuildingRender
    {
        public Transform tapTowerPopUpPoint;
		//Locked building mode drag deployment code.Dwayne.
		//private GameObject deployBuildingArea;

		private GameObject operationEffect;
		private GameObject bottomOperationEffect;

		private ParticleSystem buildEffectStarlight = new ParticleSystem();
		private ParticleSystem buildEffectGlare = new ParticleSystem();
		private ParticleSystem buildEffectGlow = new ParticleSystem();
		private ParticleSystem buildEffectFlash = new ParticleSystem();

		private float buildEffectStarlightOrignalDuration;
		private float buildEffectGlareOrignalLifeTime;
		private float buildEffectGlowLifeTime;
		private float buildEffectFlashDuration;

		//The original time set by art.
		private int originalBuildTime = 3;
		private float particlePerformanceRatio;

		private string buildEffectPath;

		private GameObject buildingBody;

        protected override void Awake()
        {
            unitRenderType = UnitRenderType.Tower;
			originalBuildTime = 3;

            base.Awake(); 

			//Locked building mode drag deployment code.Dwayne.
			//deployBuildingArea = transform.Find( "DeployBuildingArea" ).gameObject;
            tapTowerPopUpPoint = transform.Find( "TapTowerPopUpPoint" );
			buildEffectPath = gameObject.name.Split('(')[0];
			buildingBody = transform.Find( buildEffectPath ).gameObject;
        }

		public void Recyling()
		{
			Dying();
		}
			
        public override void Reset()
        {
            id = -1;
            transform.position = new Vector3( float.MaxValue / 2, float.MaxValue / 2, float.MaxValue / 2 );
			this.gameObject.SetActive( true );

			base.Reset();

			buildingBody.SetActive( true );
        }

        public override void SetPosition( Vector3 p, bool immediately = true )
        {
            healthBarOffset_y = 3.0f;
            base.SetPosition( p );
        }

        #region SetEffects functions

        public void SettingEffects()
		{
			GetEffects();
			GetBuildEffectParticleSystem();
			CheckBuildTime();
			isGameStart = true;
		}

		private void GetEffects()
		{
			buildEffect = transform.Find( buildEffectPath + "_building" ).gameObject;
			finishEffect = transform.Find( buildEffectPath + "_finish" ).gameObject;
			deathEffect = transform.Find( buildEffectPath + "_death" ).gameObject;
			operationEffect = transform.Find( buildEffectPath + "_operation" ).gameObject;
			bottomOperationEffect = transform.Find( buildEffectPath + "_operation_1" ).gameObject;
		}  

		private void GetBuildEffectParticleSystem()
		{
			string effectPath = buildEffectPath + "_building";
			buildEffectStarlight = transform.Find( effectPath + "/xilie_glow08696_2x2" ).GetComponent<ParticleSystem>();
			buildEffectGlare = transform.Find( effectPath + "/wenli_00490" ).GetComponent<ParticleSystem>();
			buildEffectGlow = transform.Find( effectPath + "/glow_00175_a" ).GetComponent<ParticleSystem>();
			buildEffectFlash = transform.Find( effectPath + "/shanguang_00231" ).GetComponent<ParticleSystem>();

			if( buildEffectStarlightOrignalDuration == 0 )
			{
				buildEffectStarlightOrignalDuration = buildEffectStarlight.main.duration;
			}

			if( buildEffectGlareOrignalLifeTime == 0 )
			{
				buildEffectGlareOrignalLifeTime = buildEffectGlare.main.startLifetimeMultiplier;
			}

			if( buildEffectGlowLifeTime == 0 )
			{
				buildEffectGlowLifeTime = buildEffectGlow.main.startLifetimeMultiplier;
			}

			if( buildEffectFlashDuration == 0 )
			{
				buildEffectFlashDuration = buildEffectFlash.main.duration;
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
			var ma = buildEffectStarlight.main;
			ma.duration *= particlePerformanceRatio;

			var ma1 = buildEffectGlare.main;
			ma1.startLifetimeMultiplier *= particlePerformanceRatio;

			var ma2 = buildEffectGlow.main;
			ma2.startLifetimeMultiplier *= particlePerformanceRatio;

			var ma3 = buildEffectFlash.main;
			ma3.duration *= particlePerformanceRatio;
		}

		protected override void BuildingBuiltStartEffect()
		{
			if( finishEffect.activeInHierarchy == false )
			{
				buildEffect.SetActive( true );
				bottomOperationEffect.SetActive( true );
			}
		}

		protected override void BuildingBuiltCompleteEffect()
		{
			finishEffect.SetActive( true );
			buildEffect.SetActive( false );
			operationEffect.SetActive( true );
		}

		protected override void HideBuildingObj()
		{
			buildingBody.SetActive( false );
			operationEffect.SetActive( false );
			finishEffect.SetActive( false );
			bottomOperationEffect.SetActive( false );
		}

		#endregion 

		//Locked building mode drag deployment code.Dwayne.
		/*
		public void ShowDeployBuildingArea()
		{
			deployBuildingArea.SetActive( true );
		}

		public void CloseDeployBuildingArea()
		{
			deployBuildingArea.SetActive( false );
		}*/
    }
}
