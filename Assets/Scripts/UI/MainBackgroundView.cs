using UnityEngine;
using System.Collections;

using Utils;
using Constants;

namespace UI
{
    public class MainBackgroundView : ViewBase  {

        private GameObject goBackGround;
        void Awake()
        {
            goBackGround = transform.Find( "Background" ).gameObject;
            MessageDispatcher.AddObserver( OnShowMainBackground, MessageType.ShowMainBackground );
        }

        private void OnShowMainBackground( object obj )
        {
            goBackGround.SetActive( ( bool )obj );
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            MessageDispatcher.RemoveObserver( OnShowMainBackground, MessageType.ShowMainBackground );
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
