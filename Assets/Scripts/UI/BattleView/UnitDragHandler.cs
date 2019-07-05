using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

using Utils;

namespace UI
{
	public class UnitDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {        
        private SquadCardItem _squad;

        public static GameObject itemBeingDragged;

        public void SetSquadCardItem(SquadCardItem card)
        {
            _squad = card;
        }

        public void OnBeginDrag( PointerEventData eventData )
        {
            /*if ( !_squad.CanDeploy ) return;

            itemBeingDragged = GameObject.Instantiate( transform.Find( "MaskImage/ItemImage" ).gameObject );

            Image im = itemBeingDragged.GetComponent<Image>();
            Color c = im.color;
            c.a = 0.6f;
            im.color = c;

            itemBeingDragged.transform.SetParent( transform );
            itemBeingDragged.transform.position = new Vector3( eventData.pressPosition.x, eventData.pressPosition.y, 0 );
            itemBeingDragged.transform.localPosition = itemBeingDragged.transform.position = eventData.position;
            itemBeingDragged.transform.localScale = new Vector3( 0.75f, 0.75f, 0.75f );

            _squad.battleView.SetLockSquadItems( true );
            MessageDispatcher.PostMessage( Constants.MessageType.ShowDeployAreas, _squad.battleView.Mark );       
			MessageDispatcher.PostMessage( Constants.MessageType.ChangeUIGestureState, true );*/
        }

        public void OnDrag( PointerEventData eventData )
        {
            /*if ( !_squad.CanDeploy ) return;

            if ( itemBeingDragged != null )
                itemBeingDragged.transform.position = new Vector3( eventData.position.x, eventData.position.y, 0 );*/
        }

        public void OnEndDrag( PointerEventData eventData )
        {
			/*if( _squad.CanDeploy )
			{
				List<RaycastResult> hits = new List<RaycastResult>();
				EventSystem.current.RaycastAll( eventData, hits );
				_squad.battleView.deploymentImage = itemBeingDragged;

                if ( _squad.CanDeploy && hits.Count == 0 )
                {
                    _squad.battleView.SendDeploySquad( _squad.metaId, eventData.position, _squad.data.index );
                }
                else
                {
                    _squad.battleView.DestroyDragImage();
                    Destroy( itemBeingDragged );
                }
			}*/
        }
    }
}
