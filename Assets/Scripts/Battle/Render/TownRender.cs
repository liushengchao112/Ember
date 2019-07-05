/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: TownHallRender.cs
// description: 
// 
// created time：#CreateDate#
//
//----------------------------------------------------------------*/

using UnityEngine;
using System.Collections;

using Utils;
using Data;
using System;

namespace Render
{
	public class TownRender : BuildingRender
    {
		private GameObject crystal;
		private GameObject pveTownDestroyEffect;//Temp code, Old town destory effect, when we have new pve map need change.Dwayne.
        protected override void Awake()
        {
            unitRenderType = UnitRenderType.Town;

            base.Awake();
			this.deathEffctDuration = 5f;//Town destroyed effect longer than other building.So use 5 second.
            if ( animator == null )
            {
                animator = transform.GetChild( 0 ).GetComponent<Animator>();
            }
			BattleType type = DataManager.GetInstance().GetBattleType();

			if( type == BattleType.Tranining || type == BattleType.Survival )
			{
				pveTownDestroyEffect = transform.Find( "Destroy_Effects" ).gameObject;
			}
        }

        public override void SetPosition( Vector3 p, bool immediately = true )
        {
            healthBarOffset_y = 2.0f;
            base.SetPosition( p, immediately );
        }

        // Play the destroyed animation
        public override void Dying()
		{
			if ( animator != null )
			{
				animator.SetBool( "Destroy", true );

				if( pveTownDestroyEffect != null )
				{
					pveTownDestroyEffect.SetActive( true );
				}
			}

			base.Dying();
        }

        // Trigger by Destroy animation event
        public override void Destroy()
        {
            base.Destroy();
        }

        public override void Reset()
        {
			base.Reset();

            id = -1;
            transform.position = new Vector3( float.MaxValue / 2, float.MaxValue / 2, float.MaxValue / 2 );
			crystal.SetActive( true );
        }

		#region SetEffects functions

		public void SettingEffects( ForceMark mark )
		{
			GetEffects( mark );
		}

		private void GetEffects( ForceMark mark)
		{
            BattleType type = DataManager.GetInstance().GetBattleType();
            if ( type != BattleType.Survival && type != BattleType.Tranining)
            {
                if ( mark == ForceMark.TopRedForce || mark == ForceMark.BottomRedForce )
                {
                    buildEffect = transform.Find( string.Format( "redbase_operation_1" ) ).gameObject;
                    finishEffect = transform.Find( string.Format( "redbase_operation_2" ) ).gameObject;
                    deathEffect = transform.Find( string.Format( "redbase_death" ) ).gameObject;
                    crystal = transform.Find( "RedBase_crystal" ).gameObject;
                }
                else if ( mark == ForceMark.TopBlueForce || mark == ForceMark.BottomBlueForce )
                {
                    buildEffect = transform.Find( string.Format( "bluebase_operation_1" ) ).gameObject;
                    finishEffect = transform.Find( string.Format( "bluebase_operation_2" ) ).gameObject;
                    deathEffect = transform.Find( string.Format( "bluebase_death" ) ).gameObject;
                    crystal = transform.Find( "BlueBase_crystal" ).gameObject;
                }
                else
                {
                    DebugUtils.LogError( DebugUtils.Type.Building, string.Format( "Can't find this type side {0}", mark ) );
                }
            }
		}  

		protected override void BuildingBuiltStartEffect()
		{
			
		}

		protected override void BuildingBuiltCompleteEffect()
		{
			this.crystal.gameObject.SetActive( true );
			animator.enabled = true;

            if ( finishEffect != null )
            {
                finishEffect.SetActive( true );
            }

            if ( buildEffect != null )
            {
                buildEffect.SetActive( true );
            }
        }

		protected override void HideBuildingObj()
		{
            if ( crystal != null )
            {
                crystal.SetActive( false );
				finishEffect.SetActive( false );
            }
        }

		#endregion
    }
}

