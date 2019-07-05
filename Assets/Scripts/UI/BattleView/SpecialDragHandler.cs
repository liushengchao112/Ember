using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

using Utils;
using Constants;
using Data;

namespace UI
{
    public class SpecialDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private const int INSTITUTE_TITLE = 10000;
        private const int INSTITUTE_CONTENT = 10001;
        private const int TOWER_TITLE = 10002;
        private const int TOWER_CONTENT = 10003;
        private const int TRAMCAR_TITLE = 10004;
        private const int TRAMCAR_CONTENT = 10005;
        private const int DEMOLISHER_TITLE = 10007;
        private const int DEMOLISHER_CONTENT = 10008;

        public SpecialCardItem specialCard;
        private static GameObject dragGameObj;
		private ForceMark mark;

        public void SetCardItem( SpecialCardItem item )
        {
            specialCard = item;
            ClickHandler.Get( item.gameObject ).onClickDown = OnClickDown;
            ClickHandler.Get( item.gameObject ).onClickUp = OnClickUp;
			mark = DataManager.GetInstance().GetForceMark();
        }

        #region Click Event

        public Action<string, string, Vector3> OpenPopUp;
        public Action ClosePopUp;

        private float clickTime = 0.5f;
        private float downTime;
        private bool isClick = false;

        public bool isDrag = false;

        private void OnClickDown( GameObject obj )
        {
            downTime = Time.time;
            isClick = true;
        }

        private void OnClickUp( GameObject obj )
        {
            isClick = false;
            ClosePopUp();
        }

        private void Update()
        {
            if ( isClick && !isDrag )
            {
                float timeInterval = Time.time - downTime;

                if ( timeInterval > clickTime )
                {
                    string title = string.Empty;
                    string content = string.Empty;
                    switch ( specialCard.cardType )
                    {
						//Locked building mode drag deployment code.Dwayne.
						/* case SpecialCardType.Institute:
                        {
                            title = INSTITUTE_TITLE.Localize();
                            content = INSTITUTE_CONTENT.Localize();
                            break;
                        }
                        case SpecialCardType.Tower:
                        {
                            title = TOWER_TITLE.Localize();
                            content = TOWER_CONTENT.Localize();
                            break;
                        }*/
                        case SpecialCardType.Tramcar:
                        {
                            title = TRAMCAR_TITLE.Localize();
                            content = TRAMCAR_CONTENT.Localize();
                            break;
                        }
                        case SpecialCardType.Demolisher:
                        {
                            title = DEMOLISHER_TITLE.Localize();
                            content = DEMOLISHER_CONTENT.Localize();
                            break;
                        }
                    }

                    OpenPopUp( title, content, this.transform.position );
                }
            }
        }

        #endregion

        #region Drag Event

        public void OnBeginDrag( PointerEventData eventData )
        {
            isDrag = true;

			if ( !specialCard.canDeploy || specialCard.maskImage.gameObject.activeInHierarchy )
                return;

            ClosePopUp();

            SpecialCardType cardType = specialCard.cardType;

			//Locked building mode drag deployment code.Dwayne.
			/*
            if ( cardType == SpecialCardType.Institute || cardType == SpecialCardType.Tower )
            {
                MessageDispatcher.PostMessage( MessageType.OpenCheckBattleFieldObj, GetBuildingType( cardType ), eventData.pressPosition );
                MessageDispatcher.PostMessage( MessageType.DeployBuildingAreaOpen, mark );
            }*/

            MessageDispatcher.PostMessage( MessageType.ShowDeployAreas, mark );
        }

		//Locked building mode drag deployment code.Dwayne.
		/*
        private BuildingType GetBuildingType( SpecialCardType type )
		{
			if( type == SpecialCardType.Tower )
			{
				return BuildingType.Tower;
			}
			else if( type == SpecialCardType.Institute )
			{
				return BuildingType.Institute;
			}

            return BuildingType.NoneBuilding;
		}*/

        public void OnDrag( PointerEventData eventData )
        {
			if ( !specialCard.canDeploy || specialCard.maskImage.gameObject.activeInHierarchy )
                return;

            SpecialCardType cardType = specialCard.cardType;

			//Locked building mode drag deployment code.Dwayne.
			/*
            if ( cardType == SpecialCardType.Institute || cardType == SpecialCardType.Tower )
			{
				MessageDispatcher.PostMessage( MessageType.CheckBattleFieldObjPositionChange, eventData.position );
			}*/
        }

        public void OnEndDrag( PointerEventData eventData )
        {
            isDrag = false;

            //Post close deploy building area and extra deploy building are
            SpecialCardType cardType = specialCard.cardType;

			//Locked building mode drag deployment code.Dwayne.
			/*
            if ( cardType == SpecialCardType.Tower || cardType == SpecialCardType.Institute )
            {
                if ( EventSystem.current.IsPointerOverGameObject() )
                {
                    cardType = SpecialCardType.None;
                }

                MessageDispatcher.PostMessage( MessageType.DeployBuildingAreaClose, mark );
                MessageDispatcher.PostMessage( MessageType.CloseCheckBattleFieldObj, GetBuildingType( cardType ) );
            }
            else */
				
			if ( cardType == SpecialCardType.Tramcar )
            {
                MessageDispatcher.PostMessage( Constants.MessageType.DeploySquad, GameConstants.UNIT_TRAMCAR_METAID, eventData.position );
            }
            else if ( cardType == SpecialCardType.Demolisher )
            {
                MessageDispatcher.PostMessage( Constants.MessageType.DeploySquad, GameConstants.UNIT_DEMOLISHER_METAID, eventData.position );
            }

            MessageDispatcher.PostMessage( MessageType.CloseDeployAreas, mark );
            cardType = SpecialCardType.None;
        }

        #endregion
    }
}
