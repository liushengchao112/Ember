using UnityEngine;
using System.Collections.Generic;

using Utils;
using Data;
using UnityEngine.UI;

namespace Map
{
	//This map data use for PvE. by Dwayne
    public class MapDataPVE : MonoBehaviour
    {
        private GameObject blueBaseArea;
        private GameObject redBaseArea;

		public Transform blueBaseTransform;
		public Transform redBaseTransform;

        public GameObject blueDeployAreas;
        public GameObject redDeployAreas;

        public Transform topCrystal;
        public Transform bottomCrystal;
        public Transform powerUp;

		public Vector3 mapSize = new Vector3( 65f, 0, 45f );//This is art 3D map size

		#region Camera set value

		private Vector2 cameraHeightRange = new Vector2( -35f, -4.5f );
		private Vector2 cameraWidthRange = new Vector2( -20f, 16.5f );

		private Vector3 blueBaseCameraPos = new Vector3( -19, 16.5f, -12 );
		private Vector3 redBaseCameraPos = new Vector3( 15.5f, 16.5f, -13.5f );

		private float highCameraViewValue = 45f;
		private float middleCameraViewValue = 40f;
		private float lowCameraViewValue = 35f;

		#endregion

		#region FormationPoints

		private List<Vector3> redFormationPointList = new List<Vector3>();
		private List<Vector3> blueFormationPointList = new List<Vector3>();

		#endregion

		public void InitializeMapDataPvE()
        {
            GameObject deployMapRoot = GameObject.Find( "rr_launch_v2_deployment/root" );

            blueBaseArea = deployMapRoot.transform.Find( "Blue_Zones" ).Find( "Blue_Base" ).gameObject;
            redBaseArea = deployMapRoot.transform.Find( "Red_Zones" ).Find( "Red_Base" ).gameObject;
            
            blueDeployAreas = blueBaseArea;
            redDeployAreas = redBaseArea;

            Transform crystalRoot = transform.Find( "Crystal" );
            topCrystal = crystalRoot.Find( "TopCrystalBase" );
            bottomCrystal = crystalRoot.Find( "BottomCrystalBase" );

			SetFormationPoint();

			//Drag deployment logic locked.Dwayne 2017.9
			/*MessageDispatcher.AddObserver( ShowDeployAreas, Constants.MessageType.ShowDeployAreas );
            MessageDispatcher.AddObserver( CloseDeployAreas, Constants.MessageType.CloseDeployAreas );*/
			
			//MessageDispatcher.AddObserver( ChangeDeployState, Constants.MessageType.TowerDestroyed ); Now designer not want change deploy range when tower destoryed
        }
			
        public void ShowDeployAreas( object inputMark )
        {
			ForceMark mark = ( ForceMark )inputMark;
			if( mark == ForceMark.TopBlueForce || mark == ForceMark.BottomBlueForce )
            {
                blueDeployAreas.SetActive( true );
            }
			else if( mark == ForceMark.TopRedForce || mark == ForceMark.BottomRedForce )
            {
                redDeployAreas.SetActive( true );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Map, "The operation's owner has error forceMark" + mark );
            }
        }

        public void CloseDeployAreas( object inputMark )
        {
			ForceMark mark = ( ForceMark )inputMark;
			if ( mark == ForceMark.TopBlueForce || mark == ForceMark.BottomBlueForce )
            {
                blueDeployAreas.SetActive( false );
            }
			else if( mark == ForceMark.TopRedForce || mark == ForceMark.BottomRedForce )
            {
                redDeployAreas.SetActive( false );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Map, "The operation's owner has error forceMark" + mark );
            }
        }

        public GameObject GetTownBaseObject( ForceMark mark )
        {
            if ( mark == ForceMark.TopBlueForce )
            {
                return blueBaseTransform.gameObject;
            }
            else if ( mark == ForceMark.TopRedForce )
            {
                return redBaseTransform.gameObject;
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Map, string.Format( "The mark doesn't has town! please check input forceMark. mark = {0}", mark ) );
                return null;
            }
        }

        public Vector3 GetTownPosition( ForceMark mark )
        {
            if ( mark == ForceMark.TopBlueForce )
            {
				return blueBaseTransform.position;
            }
            else if ( mark == ForceMark.TopRedForce )
            {
                return redBaseTransform.position;
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.Map, string.Format( "the mark doesn't has town! please check input mark. mark = {0}", mark ) );
                return Vector3.zero;
            }
        }

        public GameObject GetCrystalObject( Vector3 p )
        {
            if ( p == topCrystal.position )
            {
                return topCrystal.gameObject;
            }
            else if ( p == bottomCrystal.position )
            {
                return bottomCrystal.gameObject;
            }

            return null;
        }

        public Vector3 GetCameraOriginalPosition( ForceMark mark )
		{
			if ( mark == ForceMark.TopBlueForce || mark == ForceMark.BottomBlueForce )
			{
				return  blueBaseCameraPos;
			}
			else if ( mark == ForceMark.TopRedForce || mark == ForceMark.BottomRedForce )
			{
				return  redBaseCameraPos;
			}
			else 
			{
				DebugUtils.LogError( DebugUtils.Type.Map, string.Format( "Can't find this ForceMark.mark = {0}", mark ) );
				return Vector3.zero;
			}
		}

		public float GetViewField( int inputChoose )
		{
			if( inputChoose == ( int )UI.CameraViewType.High )
			{
				return highCameraViewValue;
			}
			else if( inputChoose == ( int )UI.CameraViewType.Middle )
			{
				return middleCameraViewValue;
			}
			else if( inputChoose == ( int )UI.CameraViewType.Low )
			{
				return lowCameraViewValue;
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Map, "Can't know this CameraViewType :" + inputChoose );
				return -1;
			}
		}

		public Vector2 GetCameraHeightRange()
		{
			return cameraHeightRange;
		}

		public Vector2 GetCameraWidthRange()
		{
			return cameraWidthRange;
		}

        public CameraInvertType GetCameraInvertType( ForceMark mark )
        {
            // Only flip horizontal now
            CameraInvertType type = CameraInvertType.None;

            switch ( mark )
            {
                case ForceMark.TopRedForce:
                {
                    type = CameraInvertType.Horizontal;
                    break;
                }
                case ForceMark.TopBlueForce:
                {
                    type = CameraInvertType.None;
                    break;
                }
            }

            return type;
        }

		#region FormationPoint functions

		private void SetFormationPoint()
		{
			Transform redFormationPointRoot = transform.Find( "Function/Point/RedBase" );

			for ( int i = 0; i < redFormationPointRoot.childCount; i++ )
			{
				Transform child = redFormationPointRoot.GetChild( i );
				redFormationPointList.Add( child.position );
			}

			Transform blueFormationPointRoot = transform.Find( "Function/Point/BlueBase" );

			for ( int i = 0; i < redFormationPointRoot.childCount; i++ )
			{
				Transform child = blueFormationPointRoot.GetChild( i );
				blueFormationPointList.Add( child.position );
			}
		}

		public List<Vector3> GetFormationPoint( ForceMark mark )
		{
			if( mark == ForceMark.TopRedForce )
			{
				return redFormationPointList;
			}
			else
			{
				return blueFormationPointList;
			}
		}

		#endregion
			
        public void OnDestroy()
        {
			//Drag deployment logic locked.Dwayne 2017.9
			/*MessageDispatcher.RemoveObserver( ShowDeployAreas, Constants.MessageType.ShowDeployAreas );
            MessageDispatcher.RemoveObserver( CloseDeployAreas, Constants.MessageType.CloseDeployAreas );*/
			
            //MessageDispatcher.RemoveObserver( ChangeDeployState, Constants.MessageType.TowerDestroyed );
        }
    }
}
