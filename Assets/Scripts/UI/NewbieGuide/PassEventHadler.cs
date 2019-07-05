using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

using Utils;
using Constants;

namespace UI
{
    public class PassEventHadler : EventTrigger
    {
        public enum EventType
        {
            None = 0,
            Click = 1,
            DragStart = 2,
            Drag = 3,
            DragEnd = 4,
        }

        private int raycastDistance = 40;
        public Action OnButtonClick;
        public Action OnBeginDragEvent;
        public Action<Vector3> OnButtonDrag;

        public bool isCanDrag = false;
        public bool isTap3D = false;

        public override void OnPointerDown( PointerEventData eventData )
        {
            PassEvent( eventData, ExecuteEvents.pointerDownHandler );
        }

        public override void OnPointerUp( PointerEventData eventData )
        {
            PassEvent( eventData , ExecuteEvents.pointerUpHandler );
        }

        public override void OnBeginDrag( PointerEventData eventData )
        {            
            PassEvent( eventData, ExecuteEvents.beginDragHandler,EventType.DragStart );
            if ( OnBeginDragEvent != null )
                OnBeginDragEvent();
        }

        public override void OnDrag( PointerEventData eventData )
        {
            if ( !isCanDrag )
            {
                return;
            }
            PassEvent( eventData, ExecuteEvents.dragHandler, EventType.Drag );
            if ( OnButtonDrag!= null )
            {
                OnButtonDrag( eventData.position );
            }
        }

        public override void OnEndDrag( PointerEventData eventData )
        {
            PassEvent( eventData , ExecuteEvents.endDragHandler,EventType.DragEnd );
        }

        public override void OnPointerClick( PointerEventData eventData )
        {
            if ( OnButtonClick != null )
            {
                OnButtonClick();
            }
            PassEvent( eventData , ExecuteEvents.pointerClickHandler ,EventType.Click);
        }


        //把事件透下去
        public void PassEvent<T>( PointerEventData data , ExecuteEvents.EventFunction<T> function , EventType type = EventType.None ) where T : IEventSystemHandler
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll( data , results );
            GameObject current = data.pointerCurrentRaycast.gameObject;
            for ( int i = 0; i < results.Count; i++ )
            {
                if ( current != results[i].gameObject )
                {
                    ExecuteEvents.Execute( results[i].gameObject , data , function );
                    //break;
                    //RaycastAll后ugui会自己排序，如果你只想响应透下去的最近的一个响应，这里ExecuteEvents.Execute后直接break就行。
                }
            }

            if ( Camera.main && isTap3D )
            {
                switch ( type )
                {
                    case EventType.None:
                    break;
                    case EventType.Click:
                    OnSimpleTap( data.position );
                    break;
                    case EventType.DragStart:
                    MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera , GestureType.DragWithSingleFinger , GestureState.Started , data.position );
                    break;
                    case EventType.Drag:
                    MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera , GestureType.DragWithSingleFinger , GestureState.Updated , data.position );
                    break;
                    case EventType.DragEnd:
                    MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera , GestureType.DragWithSingleFinger , GestureState.Ended , data.position );
                    break;
                    default:
                    break;
                }
            }

        }

        private void OnSimpleTap( Vector3 pos )
        {

            Ray ray = Camera.main.ScreenPointToRay( pos );
            RaycastHit hit;

            if ( Physics.Raycast( ray , out hit , raycastDistance , ( 1 << LayerMask.NameToLayer( LayerName.LAYER_UNIT ) ) ) )
            {
                MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera , GestureType.SingleTapUnit , GestureState.None , hit );
            }
            else if ( Physics.Raycast( ray , out hit , raycastDistance , ( 1 << LayerMask.NameToLayer( LayerName.LAYER_INSTITUTE_BASE ) ) ) )
            {
                MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera , GestureType.SingleTapInstituteBase , GestureState.None , hit );
            }
            else if ( Physics.Raycast( ray , out hit , raycastDistance , ( 1 << LayerMask.NameToLayer( LayerName.LAYER_TOWER_BASE ) ) ) )
            {
                MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera , GestureType.SingleTapTowerBase , GestureState.None , hit );
            }
            else
            {
                if ( Physics.Raycast( ray , out hit , raycastDistance , ( 1 << LayerMask.NameToLayer( LayerName.LAYER_FLYINGWALKABLE ) ) ) )
                {
                    MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera , GestureType.SingleTapFlyingWalkable , GestureState.None , hit );
                }

                if ( Physics.Raycast( ray , out hit , raycastDistance , ( 1 << LayerMask.NameToLayer( LayerName.LAYER_GROUNDWALKABLE ) ) ) )
                {
                    MessageDispatcher.PostMessage( MessageType.GestureOnNavigateCamera , GestureType.SingleTapGroundWalkable , GestureState.None , hit );
                }
            }
        }


    }
}