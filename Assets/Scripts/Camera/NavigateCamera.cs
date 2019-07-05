
/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: NavigateCamera.cs
// description: 
// 
// created time： 09/27/2016
//
//----------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

using Constants;
using HedgehogTeam.EasyTouch;

namespace Utils
{
    public enum CameraInvertType
    {
        None,
        Horizontal,
        Vertical,
        All,
    }

    public enum CameraControlType
    {
        None = 0,
        Observer = 1,
        Player = 2,
    }

    [RequireComponent(typeof(Camera))]
    public class NavigateCamera : MonoBehaviour
    {
        private bool isFollowing = false;
        private CameraControlType controlType = CameraControlType.None;

        // Follow target
        private Transform mTarget;
        private float followingDistanceCompensation = 30.0f;
        private float followingCameraHeightCompensation = 800.0f;
        private float followingCameraHeightDamping = 2.0f;
        private float followingCameraRotationDamping = 3.0f;

        private bool isPinching = false;

        // one finger
        private Vector2 mLastPoint;

        // two fingers
        private float zoomNearLimit = 8.5f;//100;
        private float zoomFarLimit = 30;//4500;

        // don't change these
        //private float distWeight;
        private float zoomDistance;
        private float moveSpeed = 0.018f;

        private Vector2 cameraMoveWidthRange;
        private Vector2 cameraMoveHeightRange;

        private Vector3 tempPosition;
		private bool uiGestureOn = false;

        private bool canSingleFinger = true;
        private bool startSingleFinger = false;

        private Material rectMat;
        private Color rectColor = Color.green;
        private Color rectFrameColor;
        private Vector3 dragScreenRectStartPosition = Vector3.zero;
        private Vector3 dragScreenRectEndPosition = Vector3.zero;
        private bool drawingRectOnScreen = false;
		private bool isFlipHorizontal = false;

        private int raycastDistance = 40;
        private Vector3 orginAngle;

        private MirrorCamera mirror;

		#region Lerp move values

		private bool isNeedLerp;
		private Vector3 lerpTaget;
		private float lerpTime;

		#endregion

        void Awake()
        {
            // TODO: Different side need use different start position
            transform.position = new Vector3( -19, 16.5f, -12 );

            rectFrameColor = new Color( Color.green.r, rectColor.g, rectColor.b, 0.1f );

            // build material used to draw rectangle on screen
            // Shader need to be add in GraphicsSettings.Always Included Shaders to prevent shader lost when building apk
            //rectMat = new Material( Shader.Find( "Lines/Colored_Blended" ) );

            EasyTouch.AddCamera( this.GetComponent<Camera>() );

            //Application.targetFrameRate = 60;
            MessageDispatcher.AddObserver( SetUIGestureState, MessageType.ChangeUIGestureState );
            MessageDispatcher.AddObserver( DragFingerToMoveCamera, MessageType.DragFingerToMoveCamera );
            MessageDispatcher.AddObserver( SetFollowingTarget, MessageType.SetCameraFollowTarget );
        }

        // Use this for initialization
        void Start()
        {
            zoomDistance = transform.position.y;
            tempPosition = transform.position;

            EasyTouch.On_SimpleTap += OnSimpleTap;
            EasyTouch.On_DoubleTap += OnDoubleTap;

            EasyTouch.On_DragStart += OnDragStart;
            EasyTouch.On_Drag += OnDrag;
            EasyTouch.On_DragEnd += OnDragEnd;

            EasyTouch.SetEnableTwist( false );
            //DebugUtils.Log( DebugUtils.Type.Map , "MapWidth:" + mapWidthLimit + "MapHeigh:" + mapHeightLimit );
        }

		void FixedUpdate()
		{
			if( isNeedLerp )
			{
				MoveTargetPositonLerp();
			}
		}

		void SetUIGestureState( object uiGesture )
		{
			uiGestureOn = (bool)uiGesture;
		}

        void DragFingerToMoveCamera( object beginDraw, object p )
        {
            //drawingRectOnScreen = (bool)beginDraw;
            GestureState state = (GestureState)beginDraw;
            Vector2 hitPosition = (Vector2)p;

            switch ( state )
            {
                case GestureState.Started:
                {
                    OnMoveCameraStarted( hitPosition );
                    break;
                }
                case GestureState.Updated:
                {
                    OnMovingCamera( hitPosition );
                    break;
                }
                case GestureState.Ended:
                {
                    OnMoveCameraEnded( hitPosition );
                    break;
                }
            }
        }

        public void SetCameraControlType( CameraControlType type )
        {
            controlType = type;
        }

        public void SetCameraOriginalPosition( Vector3 target )
        {
            transform.position = target;
        }

		//If in game scene want move camera but not want change y, use this.
		public void SetCameraVector2Position( Vector2 target )
		{
			transform.position = new Vector3( target.x, transform.position.y, target.y );
		}

		/// <summary>
		/// Sets the camera vector2 position slerp.
		/// </summary>
		/// <param name="target">Target V2 position.</param>
		/// <param name="time">Finished move use time, Float type value.</param>
		public void SetCameraVector2PositionLerp( Vector2 target, float time )
		{
			isNeedLerp = true;
			lerpTaget = new Vector3( target.x, transform.position.y, target.y );
			lerpTime = time;
		}

		private void MoveTargetPositonLerp()
		{
			Vector3 Dir = ( transform.position - lerpTaget );

			if( Vector3.Distance( transform.position, lerpTaget ) == 0 )
			{
				isNeedLerp = false;
			}
			else
			{
				transform.position -= ( Dir / 30 ) / lerpTime;//this 30 use we game frame rate
			}
		}

		public void SetCameraFieldOfViewValue( float value )
		{
			gameObject.GetComponent<Camera>().fieldOfView = value;
		}

		public void SetCameraRange( Vector2 heightRange, Vector2 widthRange )
		{
			cameraMoveHeightRange = heightRange;
			cameraMoveWidthRange = widthRange;
		}

		public void SetCameraAngle( Vector3 angle )
		{
            orginAngle = angle;

            this.transform.rotation = Quaternion.Euler( angle );
        }

        public void SetCameraInvertMode( CameraInvertType invertType = CameraInvertType.None )
        {
            switch ( invertType )
            {
                case CameraInvertType.None:
                {
                    break;
                }
                case CameraInvertType.Vertical:
                {
                    mirror = gameObject.AddComponent<MirrorCamera>();
                    // do something...
                    break;
                }
                case CameraInvertType.Horizontal:
                {
                    mirror = gameObject.AddComponent<MirrorCamera>();
					mirror.flipHorizontal = true;
					isFlipHorizontal = true;
                    break;
                }
                case CameraInvertType.All:
                {
                    mirror = gameObject.AddComponent<MirrorCamera>();
                    // do something...
                    break;
                }
            }
        }

        private void OnSimpleTap( Gesture gesture )
        {
            if ( controlType == CameraControlType.Observer)
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay( gesture.position );
            RaycastHit hit;

            if ( Physics.Raycast( ray, out hit, raycastDistance, ( 1 << LayerMask.NameToLayer( LayerName.LAYER_UNIT ))))
            {
                DebugUtils.Log( DebugUtils.Type.Gesture, string.Format( "One finger, Simple tap on screen position {0}, world position {1}", gesture.position, hit.point ) );

                MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera, GestureType.SingleTapUnit, GestureState.None, hit );
            }
			else if( Physics.Raycast( ray, out hit, raycastDistance, ( 1 << LayerMask.NameToLayer( LayerName.LAYER_INSTITUTE_BASE ))))
			{
				MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera, GestureType.SingleTapInstituteBase, GestureState.None, hit );
			}
			else if( Physics.Raycast( ray, out hit, raycastDistance, ( 1 << LayerMask.NameToLayer( LayerName.LAYER_TOWER_BASE )))) 
			{
				MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera, GestureType.SingleTapTowerBase, GestureState.None, hit );
			}
            else
            {
                if ( Physics.Raycast( ray, out hit, raycastDistance, ( 1 << LayerMask.NameToLayer( LayerName.LAYER_FLYINGWALKABLE ) ) ) )
                {
                    MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera, GestureType.SingleTapFlyingWalkable, GestureState.None, hit );
                }

                if ( Physics.Raycast( ray, out hit, raycastDistance, ( 1 << LayerMask.NameToLayer( LayerName.LAYER_GROUNDWALKABLE ) ) ) )
                {
                    MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera, GestureType.SingleTapGroundWalkable, GestureState.None, hit );
                }
            }
        }

        private void OnDoubleTap( Gesture gesture )
        {
            if ( controlType == CameraControlType.Observer )
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay( gesture.position );
            RaycastHit hit;

            if ( Physics.Raycast( ray, out hit ) )
            {
                DebugUtils.Log( DebugUtils.Type.Gesture, string.Format( "One finger, double tap on screen position {0}, world position {1}", gesture.position, hit.point ) );
                MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera, GestureType.DoubleTapMap, GestureState.None, hit );
            }
        }

        private void OnMoveCameraStarted( Vector2 touchPoint )
        {       
            mLastPoint = touchPoint;
        }

        private void OnMovingCamera( Vector2 touchPoint )
        {
            if ( mLastPoint == Vector2.zero )
            {
                //Debug.Log( "touch phase is moved, but last point is zero..." );
            }

            Vector2 deltaV2 = touchPoint - mLastPoint;

			if( isFlipHorizontal )
			{
				deltaV2.x = -deltaV2.x;
			}

            Vector3 deltaV3 = new Vector3( -deltaV2.x, 0, -deltaV2.y );

            Vector3 temp = transform.position;
            temp += deltaV3 * moveSpeed;

            if ( mLastPoint != Vector2.zero && temp.x >= cameraMoveWidthRange.x && temp.x <= cameraMoveWidthRange.y
                                            && ( temp.z >= cameraMoveHeightRange.x && temp.z <= cameraMoveHeightRange.y ) )
            {
                transform.position += deltaV3 * moveSpeed;
            }
            else if ( mLastPoint != Vector2.zero && temp.z >= cameraMoveHeightRange.x && temp.z <= cameraMoveHeightRange.y )
            {
                Vector3 pos = new Vector3( 0, 0, deltaV3.z );
                transform.position += pos * moveSpeed;
            }
            else if ( mLastPoint != Vector2.zero && temp.x >= cameraMoveWidthRange.x && temp.x <= cameraMoveWidthRange.y )
            {
                Vector3 pos = new Vector3( deltaV3.x, 0, 0 );
                transform.position += pos * moveSpeed;
            }

            mLastPoint = touchPoint;
        }

        private void OnMoveCameraEnded( Vector2 touchPoint )
        {
            mLastPoint = Vector2.zero;
        }

        private void OnDragStart( Gesture gesture )
        {
            if ( gesture.touchCount > 1 )   return;

            if ( !canSingleFinger ) return;

            DebugUtils.Log( DebugUtils.Type.Gesture, string.Format( "One finger, DragStart on screen position {0}", gesture.position ) );

            startSingleFinger = true;

            if ( controlType == CameraControlType.Player )
            {
                MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera, GestureType.DragWithSingleFinger, GestureState.Started, gesture.position );
            }
            else if( controlType == CameraControlType.Observer )
            {
                if ( isFollowing )
                {
                    mTarget = null;
                    SetCameraAngle( orginAngle );

                    float x = transform.position.x;
                    x = x < cameraMoveWidthRange.x ? cameraMoveWidthRange.x : transform.position.x;
                    x = x > cameraMoveWidthRange.y ? cameraMoveWidthRange.y : x;

                    float z = transform.position.z;
                    z = z < cameraMoveHeightRange.x ? cameraMoveHeightRange.x : transform.position.z;
                    z = z > cameraMoveHeightRange.y ? cameraMoveHeightRange.y : z;

                    SetCameraOriginalPosition( new Vector3( x, transform.position.y, z ) );
                    isFollowing = false;
                }

                OnMoveCameraStarted( gesture.position );
            }
        }

        private void OnDrag( Gesture gesture )
        {
            if ( gesture.touchCount > 1 )   return;

            if ( !canSingleFinger || !startSingleFinger )
            {
                if ( canSingleFinger == true && startSingleFinger == false )
                {
                    OnDragStart( gesture );
                }

                return;
            }

            DebugUtils.Log( DebugUtils.Type.Gesture, string.Format( "One finger, Dragging on screen position {0}", gesture.position ) );

            if ( controlType == CameraControlType.Player )
            {
                MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera, GestureType.DragWithSingleFinger, GestureState.Updated, gesture.position );
            }
            else if ( controlType == CameraControlType.Observer )
            {
                OnMovingCamera( gesture.position );
            }
        }

        private void OnDragEnd( Gesture gesture )
        {
            if ( !canSingleFinger || !startSingleFinger ) return;

            DebugUtils.Log( DebugUtils.Type.Gesture, string.Format( "One finger, DragEnd on screen position {0}", gesture.position ) );

            startSingleFinger = false;

            if ( controlType == CameraControlType.Player )
            {
                MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera, GestureType.DragWithSingleFinger, GestureState.Ended, gesture.position );
            }
            else if ( controlType == CameraControlType.Observer )
            {
                OnMoveCameraEnded( gesture.position );
            }
        }

        public void SetFollowingTarget( object targetObj )
        {
            if ( controlType == CameraControlType.Player )
            {
                return;
            }

            mTarget = (Transform)targetObj;
            isFollowing = true;
            lastTargetPosition = transform.position;
        }
			
        private Vector3 lastTargetPosition;
        void LateUpdate()
        {
            if ( isFollowing )
            {
                // Early out if we don't have a target
                if ( !mTarget )
                    return;

                // Calculate the current rotation angles
                //float wantedRotationAngle = mTarget.eulerAngles.y;
                //float wantedHeight = mTarget.position.y /*+ followingCameraHeightCompensation*/;

                //float currentRotationAngle = transform.eulerAngles.y;
                //float currentHeight = transform.position.y;

                // Damp the rotation around the y-axis
                //currentRotationAngle = Mathf.LerpAngle( currentRotationAngle, wantedRotationAngle, followingCameraRotationDamping * Time.deltaTime );

                // Damp the height
                //currentHeight = Mathf.Lerp( currentHeight, wantedHeight, followingCameraHeightDamping * Time.deltaTime );

                // Convert the angle into a rotation
                //Quaternion currentRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );

                // Set the position of the camera on the x-z plane to:
                // distance meters behind the target

                transform.position = new Vector3( mTarget.position.x - 5f, 16.5f, mTarget.position.z - 5f );

                //Vector3 deltaPostion = mTarget.position - lastTargetPosition;

                //transform.position += new Vector3( deltaPostion.x, 0, deltaPostion.z );
                //transform.position -= currentRotation * Vector3.forward * followingDistanceCompensation;

                // Set the height of the camera
                //transform.position = new Vector3( transform.position.x, transform.position.y, transform.position.z );

                // Always look at the target
                transform.LookAt( mTarget );

                lastTargetPosition = mTarget.position;
            }
        }

        public void OnDestroy()
        {
            EasyTouch.On_SimpleTap -= OnSimpleTap;
            EasyTouch.On_DoubleTap -= OnDoubleTap;

            EasyTouch.On_DragStart -= OnDragStart;
            EasyTouch.On_Drag -= OnDrag;
            EasyTouch.On_DragEnd -= OnDragEnd;

            MessageDispatcher.RemoveObserver( DragFingerToMoveCamera, MessageType.DragFingerToMoveCamera );
            MessageDispatcher.RemoveObserver( SetUIGestureState, MessageType.ChangeUIGestureState );
            MessageDispatcher.RemoveObserver( SetFollowingTarget, MessageType.SetCameraFollowTarget );

            isFollowing = false;
            mTarget = null; 
        }
    }
}