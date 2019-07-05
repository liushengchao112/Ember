using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using Utils;
using Constants;
using Data;

namespace UI
{
    public class SurrenderTips : MonoBehaviour
    {
        #region ComponentName

        private Image[] markImages;
        private Button yesButton;
        private Button noButton;
        private RectTransform trans;

        private readonly int delaySendTime = 30;
        private readonly int delayHideTime = 5;

        private Dictionary<long, SurrenderTips.Status> data;
        private bool isShow;

        #endregion

        #region enum
        //player status
        public enum Status
        {
            /// <summary>
            /// agreed to surrender
            /// </summary>
            Yes = 0,
            /// <summary>
            /// refuse to surrender
            /// </summary>
            No = 1,
            /// <summary>
            /// wait for the operating
            /// </summary>
            Wait = 3,
        }

        public RectTransform Trans
        {
            get
            {
                if ( trans == null )
                {
                    trans = GetComponent<RectTransform>();
                }
                return trans;
            }
        }

        public bool IsShow
        {
            get
            {
                return isShow;
            }
        }

        #endregion

        void Awake()
        {
            trans = GetComponent<RectTransform>();
            markImages = new Image[2];
            markImages[0] = trans.Find( "BlueImage" ).GetComponent<Image>();
            markImages[1] = trans.Find( "RedImage" ).GetComponent<Image>();
            yesButton = trans.Find( "YesButton" ).GetComponent<Button>();
            noButton = trans.Find( "NoButton" ).GetComponent<Button>();

            yesButton.AddListener( OnYesHandler );
            noButton.AddListener( OnNoHandler );
        }

        void OnDestroy()
        {
            isShow = false;
            data = null;
            trans = null;
            CancelInvoke();
        }

        void Start()
        {
            Invoke( "OnNoHandler", delaySendTime );
        }
        
        /// <summary>
        /// Show surrender tips
        /// </summary>
        /// <param name="obj"></param>
        public void Show( object obj )
        {
            if (obj == null) return;
            isShow = true;

            data = obj as Dictionary<long, Status>;

            if (gameObject.activeSelf)
            {
                RefreshView();
            }
            else
            {
                gameObject.SetActive( true );
                CancelInvoke();
                Invoke( "OnNoHandler", delaySendTime );
                RefreshView();
            }
        }

        private void RefreshView()
        {
            //TODO: Record the number of agreed
            int surrenderNumber = 0;
            //TODO: Record the number of refuse
            int refuseNumber = 0;
            //index
            int i = 0;

            var item = data.GetEnumerator();
            while (item.MoveNext())
            {
                Status status = item.Current.Value;

                if (status == Status.Yes)
                {
                    markImages[i].color = Color.green;
                    surrenderNumber++;
                }
                else if (status == Status.No)
                {
                    markImages[i].color = Color.red;
                    refuseNumber++;
                }
                else if (status == Status.Wait)
                {
                    markImages[i].color = Color.white;
                }

                i++;
            }

            //Set button status
            Status myStatus;
            if (!data.TryGetValue( DataManager.GetInstance().GetPlayerId(), out myStatus ))
            {
                myStatus = Status.Wait;
            }
            yesButton.interactable = noButton.interactable = myStatus == Status.Wait;

            if (surrenderNumber + refuseNumber == data.Count)
            {
                CancelInvoke();
                Invoke( "Hide", delayHideTime );
            }
        }

        private void OnYesHandler()
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

            MessageDispatcher.PostMessage( MessageType.QuitBattleRequest, true );
        }

        private void OnNoHandler()
        {
            UILockManager.SetGroupState( UIEventGroup.Middle, UIEventState.WaitNetwork );

            MessageDispatcher.PostMessage( MessageType.QuitBattleRequest, false );
        }

        private void Hide()
        {
            data = null;
            gameObject.SetActive( false );
            isShow = false;
        }
        
    }
}