using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace UI
{
	public class SocialView : ViewBase
	{
		#region Componet

		private SocialController socialController;
		private MailView mailScreen;
		private FriendView friendScreen;

		//toggle
		private Toggle mailToggle;
		private Toggle friendToggle;

		#endregion

		#region Initialize function

		public override void OnInit()
		{
			base.OnInit();
			_controller = socialController = new SocialController( this );
			mailScreen = transform.Find( "MailScreen" ).gameObject.AddComponent<MailView>();
            friendScreen = transform.Find("FriendScreen").gameObject.AddComponent<FriendView>();
            mailToggle = transform.Find("ToggleGroup/MailToggle").GetComponent<Toggle>();
            friendToggle = transform.Find("ToggleGroup/FriendToggle").GetComponent<Toggle>();

            mailScreen.Init();
            friendScreen.Init();
            friendScreen.gameObject.SetActive(false);

            friendToggle.onValueChanged.AddListener( FriendScreenStatusChange );
            mailToggle.onValueChanged.AddListener(MailScreenStatusChange);
            friendScreen.RegisterFriendScreenMessageHandler();
        }

		public override void OnEnter()
        {           
            base.OnEnter();
            friendToggle.isOn = true;
            mailScreen.OnEnter();            
        }

        #endregion

        private void MailScreenStatusChange( bool isOn )
		{
            mailToggle.interactable = !isOn;
			if( isOn )
			{
				friendScreen.gameObject.SetActive( false );
				mailScreen.gameObject.SetActive( true );
                mailScreen.EnterMailView();
			}
		}

		private void FriendScreenStatusChange( bool isOn )
		{
            friendToggle.interactable = !isOn;
			if( isOn )
			{
				mailScreen.gameObject.SetActive( false );
				friendScreen.gameObject.SetActive( true );

				friendScreen.MyOnEnable();
            }
		}

        public override void OnExit( bool isGoBack )
        {
            base.OnExit( isGoBack );
            mailScreen.OnExit();
			
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            friendScreen.RemoveFriendScreenMessageHandler();
        }
    }
}